using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rest : MonoBehaviour {
	public Brain brain;
	public float energyIncrease = 0.1f;
	void Update()
	{
		brain.energy += energyIncrease * Time.deltaTime;
	}
}
