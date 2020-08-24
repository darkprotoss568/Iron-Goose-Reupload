using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashScript : MonoBehaviour
{
	private float _shrinkTime = 0.1f;
	private float _shrinkTime_curr = 0.0f;

	void Start()
	{
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		_shrinkTime_curr += Time.deltaTime;

		float pcnt = BBBStatics.Map(_shrinkTime_curr, 0.0f, _shrinkTime, 1.0f, 0.0f, true);

		transform.localScale = new Vector3(pcnt, pcnt, pcnt);

		if (pcnt == 0.0f)
		{
			Destroy(gameObject);
		}
	}

	public float ShrinkTime // In case we want to adjust this externally
	{
		get { return _shrinkTime; }
		set { _shrinkTime = value; }
	}
}