using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    MeshRenderer mesh;

    private void OnEnable()
    {
        mesh = GetComponent<MeshRenderer>();
        Simulation.RegisterObstacle(this);
    }

    private void OnDisable()
    {
        Simulation.DeRegisterObstacle(this);
    }

    public Bounds GetBounds()
    {
        return mesh.bounds;
    }
}