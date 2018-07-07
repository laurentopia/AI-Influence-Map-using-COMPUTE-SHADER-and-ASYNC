using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Unity.Collections;
using Unity.Jobs;

public class DiffusionGL : MonoBehaviour
{
    public ComputeShader shader;
    Texture2D emiters;
    RenderTexture tex;
    [SerializeField] Vector2Int size;
    ComputeBuffer buffer;


    void OnEnable()
    {
        buffer = new ComputeBuffer(size.x * size.y, 4 * sizeof(float));
        //compute
        tex = new RenderTexture(size.x, size.y, 0);
        tex.enableRandomWrite = true;
        tex.Create();

        emiters = new Texture2D(size.x, size.y);

        Color[] c = new Color[size.x * size.y];
        for (int i = 0; i < size.x * size.y; i++)
        {
            c[i] = Random.value > (1 - probability) ? new Color(5, 5, 5, 1) : new Color(1, 1, 1, 0);
        }

        emiters.SetPixels(c);
        emiters.Apply();
    }

    public float probability = 0.1f;
    public float attenuation = 0.9f, diffusion = 0.1f;
    AsyncGPUReadbackRequest request;

    void Update()
    {
        if (request.done)
        {
            shader.SetTexture(0, "emitters", emiters);
            shader.SetTexture(0, "tex", tex);
            shader.SetFloat("probability", probability);
            //shader.SetInt("width", tex.width);
            shader.SetFloat("atten", attenuation);
            shader.SetFloat("diffusion", diffusion);
            shader.SetInt("someValue", (int) Time.time);
            shader.Dispatch(0, tex.width / 8, tex.height / 8, 1);

            // extract
            request = AsyncGPUReadback.Request(tex);
        }
    }


    private void OnGUI()
    {
        int w = Screen.width / 2;
        int h = Screen.height / 2;
        int s = 512;

        GUI.DrawTexture(new Rect(w - s / 2, h - s / 2, s, s), tex);
    }

    void OnDestroy()
    {
        tex.Release();
        buffer.Dispose();
    }
}