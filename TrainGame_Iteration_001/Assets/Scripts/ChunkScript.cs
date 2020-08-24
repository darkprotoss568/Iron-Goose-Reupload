using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// NOTE: Chunks also have a TrainGameObjScript attached in addition to this one

public class ChunkScript : MonoBehaviour
{
	private int _chunkResourceValue;
	private int _randomChunkValue;
	public int _staticChunkValue = 5;
	private GameObject _currDrone; // The drone assigned to pick us up

	public bool _bCanBeDestroyed = true;
	public bool _bCanExplode = true;
	public bool _bCanBeCulledByWorldScript = true;
	public float _minCullDistFromLoco = 0.0f;

	private float _pickUpTime;
	private float _pickUpCurrTime;

	private float _selfDestructTime = -1.0f;

	void Start()
	{
		_randomChunkValue = BBBStatics.RandInt(1, 10);

		//_chunkResourceValue = Random.Range(1, 11); // 1-10
		_chunkResourceValue = _staticChunkValue;
		GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>().AllChunks.Add(gameObject); // TrainGameObjScript removes it from this list on destroy

		_currDrone = null;

		_pickUpTime = 0.5f;
		_pickUpCurrTime = 0.0f;
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		if (_currDrone == null)
		{
			_pickUpCurrTime = 0.0f;
		}

		if (_selfDestructTime > 0.0f)
		{
			_selfDestructTime -= Time.deltaTime;

			if (_selfDestructTime <= 0.0f)
			{
				GetComponent<TrainGameObjScript>().BeginDestroy(true, false);
			}
		}
	}

	void OnCollisionEnter(Collision col)
	{
		// We hit something

		//if (col.gameObject != null)
		//{
		//	TrainGameObjScript tgo = col.gameObject.GetComponent<TrainGameObjScript>();
		//	if ((tgo != null && !tgo.bIsChunk) || tgo == null) // Not if we hit other chunks
		//	{
		//		GetComponent<TrainGameObjScript>()._currentHealth = 0;
		//	}
		//}

		// UPDATE: Chunks can no longer collide with each other at all (collision layers) - 31-3-18
		// They were hitting each other and exploding at the time of spawning as they all spawn in the same place

		TrainGameObjScript tgo = GetComponent<TrainGameObjScript>();

		if (tgo.GetWorldScript() == null) return;

		if (_bCanBeDestroyed)
		{
			// Main chunks must be destroyed on impact
			if (tgo.bIsMainChunk || _randomChunkValue <= 6) // 60% chance of destruction on impact
			{
				// Destroy us on impact
				tgo.BeginDestroy(_bCanExplode, true);
			}
		}
	}

	public int ChunkResourceValue
	{
		get { return _chunkResourceValue; }
	}

	public GameObject CurrDrone
	{
		get { return _currDrone; }
		set { _currDrone = value; }
	}

	public float PickUpCurrTime
	{
		get { return _pickUpCurrTime; }
		set { _pickUpCurrTime = value; }
	}

	public float PickUpTime
	{
		get { return _pickUpTime; }
		set { _pickUpTime = value; }
	}

	public void SelfDestruct(float time)
	{
		_selfDestructTime = time;
	}
}
