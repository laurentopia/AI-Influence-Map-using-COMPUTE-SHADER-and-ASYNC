using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2018/2/5 TODO: inherit from Behavior and have Brain call an update function that evaluate the new direction value

public interface IDirection
{
    Vector3 value { get; }
}

public class DirectionFlow : MonoBehaviour, IDirection
{
    public DiffusionSettings settings;

	void OnEnable() { }

    public Vector3 value
    {
        get
        {
            var index = settings.server.World2Index(transform.position);
            if (index == Mathf.Infinity) // probably an error, this guy shouldn't be outside the grid
                return Vector3.zero;
            var vf = settings.flowVectors[(int) index];
            return enabled ? scale * new Vector3(vf.x, 0, vf.y)
				: Vector3.zero;
		}
    }

    [SerializeField] float scale = 1;
}