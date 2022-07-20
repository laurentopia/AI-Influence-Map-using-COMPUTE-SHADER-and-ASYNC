using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

// 2018/01/24 added obstacle vector flow

public class InfluenceCompute : Simulation
{
    [Space] public ComputeShader shader;

    ///*
    RenderTexture[] output = new RenderTexture[2];

    // */
    /*
    Vector4[] output;
    */
    [SerializeField] Material materialViz = null;

    int indexOutput, indexBuffer;

    public override void OnEnable()
    {
        base.OnEnable();
        size = new Vector2Int((int) (boundsSize.x / cellSize), (int) (boundsSize.z / cellSize));

        indexOutput = 0;
        indexBuffer = (indexOutput + 1) % 2;
        for (int i = 0; i < 2; i++)
        {
            output[i] = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear);
            //output.filterMode = FilterMode.Point;
            output[i].enableRandomWrite = true;
            output[i].Create();
        }

        materialViz.SetTexture("_MainTex", output[indexOutput]);
        //*/
        /*
        output = new Vector4[size.x * size.y];
        */

//		obstacleSetting.server = this;
//		obstacleSetting.flowVectors = new Vector2[size.x * size.y];
        foreach (var s in settings)
        {
            s.server = this;
            s.flowVectors = new Vector2[size.x * size.y];
        }

        obstalceValues = new float[size.x * size.y];
        obstacleZero = new float[size.x * size.y];
        emitterValues = new Vector4[size.x * size.y];
        emitterZero = new Vector4[size.x * size.y];

		requestFlow = new List<AsyncGPUReadbackRequest>(settings.Length);

		StartCoroutine(AsyncExtract());
    }

    internal float[] obstalceValues, obstacleZero;
    Vector4[] emitterValues, emitterZero;

	List<AsyncGPUReadbackRequest> requestFlow;

	IEnumerator AsyncExtract()
    {
        while (true)
        {
            UnityEngine.Profiling.Profiler.BeginSample("fill the input arrays");
            //
            emitterZero.CopyTo(emitterValues, 0);
            for (int i = 0; i < settings.Length; i++)
            {
                foreach (var e in settings[i].emitters)
                {
                    var pos = World2Grid(e.transform.position);
                    if (pos.x >= 0 && pos.x < size.x && pos.y >= 0 && pos.y < size.y)
                        emitterValues[pos.x + pos.y * size.x] +=
                            e.value * new Vector4(i == 0 ? 1 : 0, i == 1 ? 1 : 0, i == 2 ? 1 : 0, i == 3 ? 1 : 0);
                }
            }

            //
            UnityEngine.Profiling.Profiler.EndSample();
            ComputeBuffer emittersBuffer = new ComputeBuffer(size.x * size.y, 4 * sizeof(float));
            emittersBuffer.SetData(emitterValues);

            obstacleZero.CopyTo(obstalceValues, 0);
            foreach (var e in obstacles)
            {
                var bounds = World2Grid(e.GetBounds());
                var bX = new Vector2Int(bounds.min.x, bounds.max.x);
                var bY = new Vector2Int(bounds.min.y, bounds.max.y);
                for (int x = bX.x; x < bX.y; x++)
                {
                    for (int y = bY.x; y < bY.y; y++)
                    {
                        obstalceValues[x + y * size.x] = 1;
                    }
                }
            }

            ComputeBuffer obstaclesBuffer = new ComputeBuffer(size.x * size.y, sizeof(float));
            obstaclesBuffer.SetData(obstalceValues);

            shader.SetBuffer(0, "emitters", emittersBuffer);
            shader.SetBuffer(0, "obstacles", obstaclesBuffer);
            shader.SetInt("w", size.x);
            shader.SetInt("h", size.y);

            //outputs

            ComputeBuffer[] vectorFlowBuffer = new ComputeBuffer[settings.Length];
            for (var i = 0; i < settings.Length; i++)
            {
                vectorFlowBuffer[i] = new ComputeBuffer(size.x * size.y, 2 * sizeof(float));
                vectorFlowBuffer[i].SetData(settings[i].flowVectors);
                shader.SetBuffer(0, "direction" + (i + 1).ToString(), vectorFlowBuffer[i]);
            }

            shader.SetTexture(0, "buffer", output[indexBuffer]);
            shader.SetTexture(0, "tex", output[indexOutput]);
            materialViz.SetTexture("_MainTex", output[indexOutput]);

            // we pack those values in a vector to fit the compute buffer
            shader.SetVector("decay",
                new Vector4(settings[0].decay, settings[1].decay, settings[2].decay, settings[3].decay));
            shader.SetVector("momentum",
                new Vector4(settings[0].momentum, settings[1].momentum, settings[2].momentum, settings[3].momentum));

            // dispatch the compute buffer
            uint tx, ty, tz;
            shader.GetKernelThreadGroupSizes(0, out tx, out ty, out tz);
            shader.Dispatch(0, size.x / (int) tx, size.y / (int) ty, 1);

			// extract
			requestFlow.Clear();
            for (int i = 0; i < settings.Length; i++)
            {
                requestFlow.Add(AsyncGPUReadback.Request(vectorFlowBuffer[i]));
				yield return new WaitUntil(() => requestFlow[i].done && requestFlow[i].hasError == false);
				requestFlow[i].GetData<Vector2>().CopyTo(settings[i].flowVectors);
				//vectorFlowBuffer[i].GetData(settings[i].flowVectors);
				yield return null;
			}

			AsyncGPUReadbackRequest request = new AsyncGPUReadbackRequest();
            request = AsyncGPUReadback.Request(output[indexOutput]);
            yield return new WaitUntil(() => request.done);
            //

            yield return null;

            for (int i = 0; i < settings.Length; i++)
            {
                vectorFlowBuffer[i].Dispose();
            }

            emittersBuffer.Dispose();
            obstaclesBuffer.Dispose();
            indexBuffer = (indexBuffer + 1) % 2;
            indexOutput = (indexOutput + 1) % 2;
        }
    }

	//void LateUpdate()
	//{

	//	for (int i = 0; i < settings.Length; i++)
	//	{
	//		if (requestFlow[i].done && requestFlow[i].hasError==false)
	//			requestFlow[i].GetData<Vector2>().CopyTo(settings[i].flowVectors);
	//	}
	//}

	/*
    private void OnGUI()
    {
        int w = Screen.width / 2;
        int h = Screen.height / 2;
        int s = 512;

        GUI.DrawTexture(new Rect(w - s / 2, h - s / 2, s, s), output);
    }
    */

	void OnDestroy()
    {
        output[0].Release();
        output[1].Release();
    }
}