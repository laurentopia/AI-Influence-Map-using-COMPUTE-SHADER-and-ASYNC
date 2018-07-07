using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emitter : MonoBehaviour
{
    public DiffusionSettings settings;
    public float value = 1;

    private void OnEnable()
    {
        settings.Register(this);
    }

    private void OnDisable()
    {
        settings.DeRegister(this);
    }
}