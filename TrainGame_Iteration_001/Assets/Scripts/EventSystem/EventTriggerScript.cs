using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{

}

public class EventTriggerScript : MonoBehaviour
{
	// Use number codes to determine event to spawn - obsolete
	//public string _eventCode;
	//public GameObject[] _eventList;
	//private Dictionary<string, GameObject> _eventDictionary = new Dictionary<string, GameObject>();

	// Event to spawn on trigger
	public GameObject _linkedEvent;

	//RewardType _rewardType; // Never used ?

	public string _rewardCode;
	public string _rewardAmount;

	//private WorldScript _worldScript;
	//private Dictionary<string, GameObject> _rewardDictionary = new Dictionary<string, GameObject>();


	// Use this for initialization
	void Start()
	{
		//Populate the event Dictionary
		//_eventDictionary.Add("000", _eventList[0]);

		//Populate the reward Dictionary
		//
	}

	// Update is called once per frame
	void Update()
	{
		if (PauseMenu.isPaused) return;
	}

	void OnTriggerEnter(Collider other)
	{
		// Check if this EventTrigger is hit by the locomotive
		if (other.gameObject.GetComponent<LocomotiveScript>())
			//
			//if (_eventDictionary.ContainsKey(_eventCode))
			//SpawnEvent();
			if (_linkedEvent != null)
				SpawnEvent();
	}

	public void SpawnEvent()
	{
		Instantiate(_linkedEvent, transform.position, Quaternion.identity);
		Destroy(this.gameObject);
	}
}
