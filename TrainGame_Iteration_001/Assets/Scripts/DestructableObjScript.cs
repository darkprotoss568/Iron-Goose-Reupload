using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// NOTE: Destructable objects also have a TrainGameObjScript attached in addition to this one

public class DestructableObjScript : MonoBehaviour
{
	public bool _bExplosion = true;

	private List<AudioClip> xplSnds = new List<AudioClip>();

	void Start()
	{
		// We only want to use custom chunks on a destructable object
		BBBStatics.TGO(gameObject).bSpawnMainChunk = false;
		BBBStatics.TGO(gameObject)._defaultChunksCount = 0;

		xplSnds.Add(Resources.Load("Sounds/crateXpl001") as AudioClip);
		xplSnds.Add(Resources.Load("Sounds/crateXpl002") as AudioClip);
	}

	//void Update()
	//{
	//}

	void OnCollisionEnter(Collision col)
	{
		// We hit something

		// Destroy us on impact
		if (col.gameObject.GetComponent<Rigidbody>() != null)
		{
			gameObject.GetComponent<TrainGameObjScript>().BeginDestroy(_bExplosion, true); // _bExplosion

			//AudioSource.PlayClipAtPoint(BBBStatics.RandomlyPickAudioClip(xplSnds), transform.position, 1.0f);
			BBBStatics.PlayClipAtPoint_BBB(BBBStatics.RandomlyPickAudioClip(xplSnds), transform.position, 1.0f, 1.0f);
		}
	}
}
