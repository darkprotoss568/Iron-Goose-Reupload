using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerVolumeScript : MonoBehaviour
{
	protected WorldScript _worldScript;

	public virtual void Start()
	{
		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
	}

	//void Update()
	//{
	//}

	public virtual void OnTriggerEnter(Collider other)
	{
		Destroy(gameObject);
	}

	//private void OnTriggerExit(Collider other)
	//{
	//}

	//private void OnTriggerStay(Collider other)
	//{
	//}
}
