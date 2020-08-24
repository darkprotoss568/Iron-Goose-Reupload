using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; /// Deactivate for final build
#endif

#if UNITY_EDITOR
[ExecuteInEditMode] /// Deactivate for final build
#endif
public class RailInEditorScript : MonoBehaviour
{
	public bool bSpawnTrackInFront;
	private bool _bRunRailConnection = true;

	public string _nameOfNewObject = "newRail";

	private GameObject FrontSocket;
	//private GameObject BehindSocket;

	public GameObject RailPrefabToSpawn;

	public List<GameObject> RandRailPrefabsToSpawn = new List<GameObject>();

	private GameObject RailHolder;

	private GameObject LastNewRail = null;

	private RailScript rs;

	void Start()
	{
		rs = GetComponent<RailScript>();

		bSpawnTrackInFront = false;

		FrontSocket = transform.GetChild(0).gameObject;
		//BehindSocket = transform.GetChild(1).gameObject;

		RailHolder = GameObject.Find("RailHolder");
	}
	
	void Update()
	{
		if (PauseMenu.isPaused) return;

		if (bSpawnTrackInFront)
		{
			if (RandRailPrefabsToSpawn.Count > 0)
			{
				RailPrefabToSpawn = RandRailPrefabsToSpawn[BBBStatics.RandInt(0, RandRailPrefabsToSpawn.Count)];
			}

			if (RailPrefabToSpawn != null)
			{
				GameObject tempRailPrefabToSpawn = RailPrefabToSpawn;
				List<GameObject> tempRandRailPrefabsToSpawn = RandRailPrefabsToSpawn;

				LastNewRail = Instantiate(RailPrefabToSpawn, FrontSocket.transform.position, FrontSocket.transform.rotation);
				LastNewRail.name = _nameOfNewObject + BBBStatics.RandInt(0, 99999);

				if (RailHolder != null)
				{
					LastNewRail.transform.SetParent(RailHolder.transform);
				}

				RailInEditorScript ries = LastNewRail.GetComponent<RailInEditorScript>();
				if (ries != null)
				{
					ries.RailPrefabToSpawn = tempRailPrefabToSpawn;
					ries.RandRailPrefabsToSpawn = tempRandRailPrefabsToSpawn;
				}

				if (rs != null)
				{
					rs.RailFront = LastNewRail;
					RailScript nrs = LastNewRail.GetComponent<RailScript>();
					if (nrs)
					{
						nrs.RailBehind = rs.gameObject;
						nrs.BRunRailConnection = _bRunRailConnection;
					}
				}

#if UNITY_EDITOR
				Selection.objects = new Object[] { LastNewRail }; /// Deactivate for final build
#endif
			}
			else print("Error: RailPrefabToSpawn is null -- RailInEditorScript");
		}

		bSpawnTrackInFront = false;
		_bRunRailConnection = true;
	}
}
