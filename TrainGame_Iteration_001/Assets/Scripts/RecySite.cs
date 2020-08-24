using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecySite : ConsSite
{
	private GameObject _recyObj; public GameObject RecyObj { get { return _recyObj; } set { _recyObj = value; } }

	public override void Start()
	{
		base.Start();

		_consSiteMaterial = Resources.Load("Materials/consSiteMat2") as Material;

		//_recyObj.SetActive(false);

		_resourcesNeeded = 0;

		_goToCopy = _recyObj;
	}

	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		//

		if (_recyObj != null)
		{
			if (gameObject.activeInHierarchy)
			{
				if (_recyObj.activeInHierarchy) _recyObj.SetActive(false);
			}
			else
			{
				if (!_recyObj.activeInHierarchy) _recyObj.SetActive(true);
			}

			// Better option than actually attaching the transform as _recyObj's child -- that causes weird issues [Mike, 6-6-18]
			transform.position = _recyObj.transform.position;
			transform.rotation = _recyObj.transform.rotation;
		}

		//

		base.Update();
	}

	protected override void ManageMaterial()
	{
		if (_meshObjects.Count > 0)
		{
			Color c = _meshObjects[0].GetComponent<MeshRenderer>().material.color;
			float alpha = BBBStatics.Map(_percentComplete, 0.0f, 100.0f, 2.0f, 0.0f, true);
			Color next = new Color(c.r, c.g, c.b, alpha);

			for (int i = 0; i < _meshObjects.Count; ++i)
			{
				_meshObjects[i].GetComponent<MeshRenderer>().material.SetColor("_Color", next);
			}
		}
	}

	protected override void CheckForComplete()
	{
		if (_currDrone != null && _percentComplete >= 100.0f)
		{
			_currDrone.GetComponent<AIConsDroneScript>().CurrRecySite = null;

			_recyObj.SetActive(true); // Necessary ?
			_recyObj.GetComponent<TrainGameObjScript>().BeginDestroy(false, false);

			//

			TrainGameObjScript tgo = _recyObj.GetComponent<TrainGameObjScript>();
			if (tgo != null)
			{
				float healthPcnt = (float)tgo._currentHealth / (float)tgo._maxHealth;
				int refund = Mathf.RoundToInt((float)tgo.BuildCost * (float)healthPcnt);
				//refund = Mathf.Clamp(refund, Mathf.RoundToInt(tgo.BuildCost / 4), tgo.BuildCost); // Minimum of 1/4 refund

				_worldScript.GameplayScript.AddResources(refund);
				_recyObj.GetComponent<TrainGameObjScript>().BeginDestroy(false, false);

				_worldScript.AS_2DMainAudioSource.PlayOneShot(_worldScript.WS_beep4, 0.75f);
			}

			//

			//Destroy(gameObject);
			BeginDestroy();
		}
	}

	public override void BeginDestroy()
	{
		if (_recyObj != null) _recyObj.SetActive(true);

		base.BeginDestroy();
	}
}
