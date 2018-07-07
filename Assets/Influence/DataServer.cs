using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

// the influence arrays that agents connect to
public class DataServer : ScriptableObject
{
    public NativeArray<float> data; // 1D data
}