using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxEnemyCountTrigger : TriggerVolumeScript
{
	public int _newMaxEnemyCount = 5;

	public override void Start()
	{
		base.Start();
	}

	//void Update()
	//{
	//}

	public override void OnTriggerEnter(Collider other)
	{
		if (other.gameObject != null && other.gameObject == _worldScript.LocomotiveObjectRef)
		{
			_worldScript.GameplayScript.MaxEnemiesInWorld = _newMaxEnemyCount;

			base.OnTriggerEnter(other); // Destroys the trigger volume
		}
	}

	//private void OnTriggerExit(Collider other)
	//{
	//}

	//private void OnTriggerStay(Collider other)
	//{
	//}
}
