using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapScript : MonoBehaviour
{
	private WorldScript _worldScript;
	private GameObject _locomotive;
	private GameObject _miniMapHolder;
	private GameObject _miniMap;
	private GameObject _trainIndicator;
	private GameObject _trainIcon;
	private GameObject _enemyIcon;
	public GameObject _RTSCam;
	private GameObject _wayPoint;
	public float _radarRange = 250f;
	List<GameObject> NearbyEnemy;

	void Start()
	{
		NearbyEnemy = new List<GameObject>();
		_trainIcon = Resources.Load("MiniMap_TrainIndicator") as GameObject;
		_enemyIcon = Resources.Load("MiniMap_EnemyIndicator") as GameObject;
		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
		_miniMapHolder = GameObject.Find("MainHolder").transform.Find("HUDCanvas").transform.Find("MiniMapHolder").gameObject;
		_miniMap = _miniMapHolder.transform.Find("MiniMap").gameObject;
		_RTSCam = GameObject.Find("MainHolder").transform.Find("RTS_Camera").gameObject;
		_wayPoint = Instantiate(Resources.Load("MiniMap_WayPoint") as GameObject);
		_wayPoint.transform.parent = GameObject.Find("MainHolder").transform;
	}


	void Update()
	{
		if (PauseMenu.isPaused) return;

		if (_worldScript.LocomotiveObjectRef) Radar();
	}

	void Radar()
	{
		if (_trainIndicator == null)
		{
			_locomotive = _worldScript.LocomotiveObjectRef;
			_trainIndicator = Instantiate(_trainIcon, new Vector3(_locomotive.transform.position.x, _locomotive.transform.position.y + 190, _locomotive.transform.position.z), Quaternion.identity);
		}
		_trainIndicator.transform.position = new Vector3(_locomotive.transform.position.x, _locomotive.transform.position.y + 190, _locomotive.transform.position.z);

		NearbyEnemy.Clear();
		foreach (AIDynamicObjScript _ai in FindObjectsOfType<AIDynamicObjScript>())
		{
			if (!_ai.GetComponent<AIConsDroneScript>() && !_ai.GetComponent<AIScavDroneScript>())
			{
				if (Vector3.Distance(_ai.transform.position, _locomotive.transform.position) < _radarRange) NearbyEnemy.Add(_ai.gameObject);
				else if (_ai.transform.Find("MiniMap_Indicator(Clone)")) Destroy(_ai.gameObject.transform.Find("MiniMap_EnemyIndicator(Clone)").gameObject);
			}
		}

		foreach (GameObject _nearbyEnemy in NearbyEnemy)
		{
			if (!_nearbyEnemy.transform.Find("MiniMap_Indicator(Clone)"))
			{
				GameObject enemyIcon = Instantiate(_enemyIcon, new Vector3(_nearbyEnemy.transform.position.x, _nearbyEnemy.transform.position.y + 200, _nearbyEnemy.transform.position.z), Quaternion.identity);
				enemyIcon.transform.parent = _nearbyEnemy.transform;
			}
		}
	}

	public void MoveCamera()
	{
		Rect minimapRect = _miniMap.GetComponent<RectTransform>().rect;
		Vector3 mousePos = Input.mousePosition;
		mousePos.x = mousePos.x + minimapRect.width - Screen.width;
		mousePos.y = mousePos.y + minimapRect.height - Screen.height;
		Vector3 movePos;
		movePos.x = (mousePos.y * (_worldScript.V3_MapTopRight.x - _worldScript.V3_MapBottomRight.x) / minimapRect.height) + _worldScript.V3_MapBottomLeft.x;
		movePos.z = (mousePos.x * (_worldScript.V3_MapTopRight.z - _worldScript.V3_MapTopLeft.z) / minimapRect.width) + _worldScript.V3_MapBottomLeft.z;
		_wayPoint.transform.position = new Vector3(movePos.x, _RTSCam.transform.position.y, movePos.z);
		_RTSCam.GetComponent<RTSCameraController>().TriggerMoveToMiniMap();
	}
}
