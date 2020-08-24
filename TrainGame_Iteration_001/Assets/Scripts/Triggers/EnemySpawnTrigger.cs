using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; /// Deactivate for final build
#endif

#if UNITY_EDITOR
[ExecuteInEditMode] /// Deactivate for final build
#endif
public class EnemySpawnTrigger : TriggerVolumeScript
{
	public GameObject _spawnArchetype;
	public GameObject _spawnPosObj;

	public override void Start()
	{
		base.Start();
	}

	void Update()
	{
		/*for (int i = 0; i < Selection.objects.Length; ++i)
		{
			if ((_spawnPosObj != null && Selection.objects[i] == _spawnPosObj) || Selection.objects[i] == gameObject)
			{
				Debug.DrawLine(transform.position, _spawnPosObj.transform.position, Color.cyan, Time.deltaTime);
				break;
			}
		}*/
	}

	public override void OnTriggerEnter(Collider other)
	{
		if (_worldScript == null) return;
		if (_worldScript.LocomotiveObjectRef == null) return;

		if (other.gameObject != null && other.gameObject == _worldScript.LocomotiveObjectRef && _spawnPosObj != null)
		{
			Vector3 normal;
			Vector3 spawnPnt = BBBStatics.CheckForGroundV_V2(_spawnPosObj.transform.position, 20.0f, out normal);

			if (spawnPnt == Vector3.zero) return;
			//if (BBBStatics.Is3DVecOnScreen(spawnPnt)) return; // Block spawning if the point is on-screen

			if (_worldScript.GameplayScript.EnemiesInWorld.Count < _worldScript.GameplayScript.MaxEnemiesInWorld)
			{
				if (_spawnArchetype != null)
				{
					Instantiate(_spawnArchetype, spawnPnt, _spawnPosObj.transform.rotation);
				}
			}

			base.OnTriggerEnter(other); // Destroys the trigger volume
		}
	}
}
