using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionRandom : MonoBehaviour, IDirection
{
	void OnEnable() { }

	public Vector3 value
    {
        get
        {
			var vf = Random.onUnitSphere;
			return scale * new Vector3(vf.x, 0, vf.y);
        }
    }

    [SerializeField] float scale = 1;
}