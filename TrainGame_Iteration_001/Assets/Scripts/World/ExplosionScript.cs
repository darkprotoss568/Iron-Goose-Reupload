using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } } public WorldScript WS { get { return _worldScript; } }

	//void Start()
	//{
	//}

	//void Update()
	//{
	// if (PauseMenu.isPaused) return;
	//}

	public void Explosion(Vector3 location, GameObject FX, AudioClip sound, int damageAmount, float damageRadius, List<TrainGameObjScript> immuneObjs, string debugStr, float xplVolume)
	{
		//print("Explosion debug:" + debugStr);

		/*
		Debug.DrawLine(Location, Location + new Vector3(0, 2, 0), Color.yellow, 0.5f);
		Debug.DrawLine(Location, Location + new Vector3(0, -2, 0), Color.yellow, 0.5f);
		Debug.DrawLine(Location, Location + new Vector3(2, 0, 0), Color.yellow, 0.5f);
		Debug.DrawLine(Location, Location + new Vector3(-2, 0, 0), Color.yellow, 0.5f);
		Debug.DrawLine(Location, Location + new Vector3(0, 0, 2), Color.yellow, 0.5f);
		Debug.DrawLine(Location, Location + new Vector3(0, 0, -2), Color.yellow, 0.5f);
		*/

		// Damage any objects in range - if we can
		if (damageAmount > 0 && damageRadius > 0.0f)
		{
			//TrainGameObjScript[] allObjs = (TrainGameObjScript[])FindObjectsOfType(typeof(TrainGameObjScript));

			for (int i = 0; i < WorldScript.GetAllTGOsInWorld().Count; ++i) // allObjs
			{
				if (immuneObjs != null && immuneObjs.Contains(WorldScript.GetAllTGOsInWorld()[i])) continue;

				float dist = Vector3.Distance(location, WorldScript.GetAllTGOsInWorld()[i].gameObject.transform.position);

			}
		}

		GameObject xpl = null;
		if (FX == null)
		{
			// Default explosion effect
			xpl = Instantiate(Resources.Load("FX/Explosion001"), location, Quaternion.identity) as GameObject;
		}
		else
		{
			// Overwritten explosion effect
			xpl = Instantiate(FX, location, Quaternion.identity) as GameObject;
		}

		if (xpl != null)
		{
			// Make sure it plays
			var e = xpl.GetComponent<ParticleSystem>().emission;
			e.enabled = true;
		}

		if (sound == null)
		{
			AudioClip ac = Resources.Load("Sounds/Explosion001") as AudioClip; // Explosion001 // Hit001
			if (ac != null)
			{
				//AudioSource.PlayClipAtPoint(ac, location, xplVolume);
				BBBStatics.PlayClipAtPoint_BBB(ac, location, xplVolume, BBBStatics.RandFlt(0.9f, 1.1f));
			}
		}
		else
		{
			//AudioSource.PlayClipAtPoint(sound, location, xplVolume);
			BBBStatics.PlayClipAtPoint_BBB(sound, location, xplVolume, BBBStatics.RandFlt(0.9f, 1.1f));
		}

		if (Vector3.Distance(location, WorldScript.RTSCameraController.PointOnGround) < 300.0f) /// 800.0f
		{
			//XWorldScript.RTSCameraController.AddCameraShake(0.1f); /// 0.5f
		}
	}
}
