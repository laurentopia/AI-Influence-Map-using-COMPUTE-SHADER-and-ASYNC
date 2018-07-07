using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class DiffusionSettings : ScriptableObject
{
    //	public float trail; // how much it stays
    [FormerlySerializedAs("diffusion")] public float momentum; // how much it blurs and expands
    [FormerlySerializedAs("attenuation")] public float decay; // how much it brings to zero
    internal Simulation server;
	internal Vector2[] flowVectors;

    // TODO: add HashSet of emitter, the emitter will register to this SO and then the thing that computes the diffusion will look up those HashSets
    internal List<Emitter> emitters = new List<Emitter>();

    public void Register(Emitter em)
    {
        emitters.Add(em);
    }

    public void DeRegister(Emitter em)
    {
        emitters.Remove(em);
    }
}