using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    IDirection[] directions;
    [SerializeField] float speed = 1;
    [SerializeField] NavMeshAgent agent;

    float refreshFudge, timeNextRefresh;
	public Brain brain;

    void OnEnable()
    {
        NavMesh.pathfindingIterationsPerFrame += 10;
        directions = GetComponents<IDirection>();
        refreshFudge = Random.value * refreshPeriod * .5f;
        agent.speed = speed;
    }

    [SerializeField] float refreshPeriod = .1f;

	public float energyConsumption = 0.1f;

    void Update()
    {
        if (Time.time > timeNextRefresh)
        {
            timeNextRefresh = Time.time + refreshFudge + refreshPeriod;
            var dir = Vector3.zero;
            foreach (var d in directions)
                dir += d.value;
            dir.Normalize();
            agent.SetDestination(transform.position + speed * dir.normalized);
			brain.energy -= energyConsumption * Time.deltaTime;
            //		transform.position += speed * dir.normalized * Time.deltaTime;
        }
    }
}