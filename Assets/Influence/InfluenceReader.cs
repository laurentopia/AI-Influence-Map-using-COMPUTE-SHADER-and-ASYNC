using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceReader : MonoBehaviour
{
    public CustomRenderTexture texture;

    private void Update()
    {
        RenderTexture.active = texture;
    }
}