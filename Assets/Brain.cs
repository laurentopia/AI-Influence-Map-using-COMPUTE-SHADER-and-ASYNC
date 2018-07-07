using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2018/2/5 utility implementation
// 2018/3/8 reading that implementation
public class Brain : MonoBehaviour {
	//state machine
	public enum STATE { REST, CHASE, FEED, WANDER, FLEE }
	public STATE state, newState;
	
	// stats
	public float energy = 1; // half = hungry, 0=rest
	public float health = 1; // 0=dead
	
	//behaviors
	public List<MonoBehaviour> rest, chase, feed, wander, flee;

	void Update()
	{
		if (energy > 0.5f)
			newState = STATE.WANDER;
		if (energy < 0.5f)
			newState = STATE.CHASE;
		if (energy == 0)
			newState = STATE.REST;

		if (health < 0.5f)
			newState = STATE.FLEE;

		if (newState != state)
		{
			switch (newState)
			{
				case STATE.CHASE:
					rest.ForEach(m => m.enabled = false);
					feed.ForEach(m => m.enabled = false);
					wander.ForEach(m => m.enabled = false);
					flee.ForEach(m => m.enabled = false);
					chase.ForEach(m => m.enabled = true);
					break;
				case STATE.FEED:
					rest.ForEach(m => m.enabled = false);
					chase.ForEach(m => m.enabled = false);
					wander.ForEach(m => m.enabled = false);
					flee.ForEach(m => m.enabled = false);
					feed.ForEach(m => m.enabled = true);
					break;
				case STATE.REST:
					feed.ForEach(m => m.enabled = false);
					wander.ForEach(m => m.enabled = false);
					chase.ForEach(m => m.enabled = false);
					flee.ForEach(m => m.enabled = false);
					rest.ForEach(m => m.enabled = true);
					break;
				case STATE.WANDER:
					rest.ForEach(m => m.enabled = false);
					feed.ForEach(m => m.enabled = false);
					rest.ForEach(m => m.enabled = false);
					flee.ForEach(m => m.enabled = false);
					wander.ForEach(m => m.enabled = true);
					break;
				case STATE.FLEE:
					rest.ForEach(m => m.enabled = false);
					feed.ForEach(m => m.enabled = false);
					rest.ForEach(m => m.enabled = false);
					wander.ForEach(m => m.enabled = false);
					flee.ForEach(m => m.enabled = true);
					break;
			}
			state = newState;
		}
	}
}
