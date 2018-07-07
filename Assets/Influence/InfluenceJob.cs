using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

//...2018/01/30 it's a job and it doesn't work, massive GC


public class InfluenceJob : Simulation
{
    [Space] public int batchSize = 100;

    [SerializeField] Material materialViz = null;

    Texture2D output;

    float[] obstacleValues, obstacleZero;

    public override void OnEnable()
    {
        base.OnEnable();
        size = new Vector2Int((int) (boundsSize.x / cellSize), (int) (boundsSize.z / cellSize));

        output = new Texture2D(size.x, size.y, TextureFormat.RGBAFloat, false, true);
        materialViz.SetTexture("_MainTex", output);

        foreach (var s in settings)
        {
            s.server = this;
			s.flowVectors = new Vector2[size.x * size.y];
		}

		obstacleValues = new float[size.x * size.y];
        obstacleZero = new float[size.x * size.y];
        colors = new Color[size.x * size.y];
    }

    Color[] colors;

    struct ComputeInfluenceJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> influencesBuffer, obstacleGrid, emitterGrid;
        [ReadOnly] public float decay, momentum;
        [ReadOnly] public int width, height;

        [WriteOnly] public NativeArray<float> influencesOut;

        public void Execute(int i)
        {
            //champandar
            float minInf = 0, maxInf = 0;
            float inf = emitterGrid[i];
            maxInf = Mathf.Max(inf, maxInf);
            minInf = Mathf.Min(inf, minInf);

            Vector3[] retVal = null;
            GetNeighbors(ref retVal, i);
            for (int k = 0; k < 8; k++)
            {
                Vector3 n = retVal[k];
                if (n.z != 0)
                {
                    inf = influencesBuffer[(int) n.x + width * (int) n.y] * Mathf.Exp(-decay * n.z);
                    maxInf = Mathf.Max(inf, maxInf);
                    minInf = Mathf.Min(inf, minInf);
                }
            }

            if (Mathf.Abs(minInf) > maxInf)
                influencesOut[i] = Mathf.Lerp(influencesBuffer[i], minInf, momentum) * (1 - obstacleGrid[i]);
            else
                influencesOut[i] = Mathf.Lerp(influencesBuffer[i], maxInf, momentum) * (1 - obstacleGrid[i]);
        }

        // x,y and z= 0 if out of bounds
        void GetNeighbors(ref Vector3[] array, int index)
        {
            int x = index % width;
            int y = index / width;
            Vector3 v = Vector3.one;
            array = new Vector3[8];
            if (x > 0)
            {
                array[0].Set(x - 1, y, 1);
            }

            if (x < width - 1)
            {
                array[1].Set(x + 1, y, 1);
            }

            if (y > 0)
            {
                array[2].Set(x, y - 1, 1);
            }

            if (y < height - 1)
            {
                array[3].Set(x, y + 1, 1);
            }

            // diagonals

            if (x > 0 && y > 0)
            {
                array[4].Set(x - 1, y - 1, 1.4142f);
            }

            if (x < width - 1 && y < height - 1)
            {
                array[5].Set(x + 1, y + 1, 1.4142f);
            }

            if (x > 0 && y < height - 1)
            {
                array[6].Set(x - 1, y + 1, 1.4142f);
            }

            if (x < width - 1 && y > 0)
            {
                array[7].Set(x + 1, y - 1, 1.4142f);
            }
        }
    }

    struct ComputeFlowVectors : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> influences;
        [ReadOnly] public int width, height;

        [WriteOnly] public NativeArray<Vector2> flowVectorsOut;

        //vector fields
        public void Execute(int i)
        {
            Vector2 flowV = Vector2.zero;
            int x = i % width;
            int y = i / height;
            Vector3[] retVal = null;
            GetNeighbors(ref retVal, i);
            for (int k = 0; k < 8; k++)
            {
                Vector3 n = retVal[k];
                if (n.z != 0)
                    flowV += influences[(int) n.x + width * (int) n.y] * new Vector2(n.x - x, n.y - y);
            }

            flowVectorsOut[i] = flowV;
        }

        // x,y and z= 0 if out of bounds
        void GetNeighbors(ref Vector3[] array, int index)
        {
            int x = index % width;
            int y = index / width;
            array = new Vector3[8];
            if (x > 0)
            {
                array[0] = new Vector3(x - 1, y, 1);
            }

            if (x < width - 1)
            {
                array[1] = new Vector3(x + 1, y, 1);
            }

            if (y > 0)
            {
                array[2] = new Vector3(x, y - 1, 1);
            }

            if (y < height - 1)
            {
                array[3] = new Vector3(x, y + 1, 1);
            }

            // diagonals

            if (x > 0 && y > 0)
            {
                array[4] = new Vector3(x - 1, y - 1, 1.4142f);
            }

            if (x < width - 1 && y < height - 1)
            {
                array[5] = new Vector3(x + 1, y + 1, 1.4142f);
            }

            if (x > 0 && y < height - 1)
            {
                array[6] = new Vector3(x - 1, y + 1, 1.4142f);
            }

            if (x < width - 1 && y > 0)
            {
                array[7] = new Vector3(x + 1, y - 1, 1.4142f);
            }
        }
    }

    Color[] viz;

    int currentIndex;
    public NativeArray<float> influences, influencesBuffer;
    public NativeArray<Vector2> flowVectors;
    JobHandle handleInfluence, handleFlowVector;
    NativeArray<float> emitterGrid;

    void Update()
    {
        if (handleFlowVector.IsCompleted)
        {
            //setup the emittergrid
            emitterGrid = new NativeArray<float>(obstacleZero, Allocator.Persistent);
            foreach (var e in settings[currentIndex].emitters)
            {
                var pos = World2Grid(e.transform.position);
                if (pos.x >= 0 && pos.x < size.x && pos.y >= 0 && pos.y < size.y)
                    emitterGrid[pos.x + pos.y * size.x] += e.value;
            }

            // launching current influence job
            influences = new NativeArray<float>(size.x * size.y, Allocator.Persistent);
            influencesBuffer = new NativeArray<float>(size.x * size.y, Allocator.Persistent);
            ComputeInfluenceJob jobInfluences = new ComputeInfluenceJob()
            {
                decay = settings[currentIndex].decay,
                height = size.y,
                influencesOut = influences,
                influencesBuffer = influencesBuffer,
                momentum = settings[currentIndex].momentum,
                obstacleGrid = new NativeArray<float>(obstacleValues, Allocator.TempJob),
                emitterGrid = emitterGrid,
                width = size.x
            };
            handleInfluence = jobInfluences.Schedule(size.x * size.y, batchSize);

            // launching computation of vector fields
            flowVectors = new NativeArray<Vector2>(size.x * size.y, Allocator.Persistent);
            ComputeFlowVectors jobFlow = new ComputeFlowVectors()
            {
                flowVectorsOut = flowVectors,
                height = size.y,
                influences = influences,
                width = size.x
            };
            handleFlowVector = jobFlow.Schedule(size.x * size.y, batchSize, handleInfluence);
        }
    }

    void LateUpdate()
    {
        if (handleFlowVector.IsCompleted)
        {
            handleFlowVector.Complete();
            flowVectors.CopyTo(settings[currentIndex].flowVectors);
            output.SetPixels(colors);
            output.Apply();
            //
            flowVectors.Dispose();
            influencesBuffer.Dispose();
            influences.Dispose();
            emitterGrid.Dispose();
            //
            currentIndex++;
            currentIndex %= settings.Length;
        }
    }
}