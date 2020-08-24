using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; /// Deactivate for final build
#endif

[System.Serializable] // Make it editable in the inspector
public struct ProcPrefab
{
	public List<GameObject> _prefabs;
	public List<float> _fixedRotationOffset;

	public ProcPrefab(List<GameObject> prefabs, List<float> fixedRotationOffset)
	{
		_prefabs = prefabs;
		_fixedRotationOffset = fixedRotationOffset;
	}
}

#if UNITY_EDITOR
[ExecuteInEditMode] /// Deactivate for final build
#endif

public class Proc1 : MonoBehaviour
{
	private WS_ProceduralGen _WS_ProceduralGen;

	private bool _bOnlySpawnObjsNearTracks = false;
	public bool BOnlySpawnObjsNearTracks { get { return _bOnlySpawnObjsNearTracks; } set { _bOnlySpawnObjsNearTracks = value; } }

	private Vector3 _top = new Vector3();
	private Vector3 _right = new Vector3();
	private Vector3 _bottom = new Vector3();
	private Vector3 _left = new Vector3();

	private Vector3 _topLeft = new Vector3();
	private Vector3 _topRight = new Vector3();
	private Vector3 _bottomRight = new Vector3();
	private Vector3 _bottomLeft = new Vector3();

	[Header("Main Function")]
	public bool _bActive = false; // Public to allow manual activation
	private bool _bActiveInfinite = false; public bool BActiveInfinite { get { return _bActiveInfinite; } set { _bActiveInfinite = value; } }

	//public int _activationTimesCount;
	//private int _activationTimesCount_actual;

	[Header("Other Functions")]
	public bool _bIsBaseProcObj = false;

	public bool _bResetAllProc1DirDones = false;
	public bool _bDestroyAllNonBaseProcObjs = false;
	public bool _bSelectAllProc1BaseObjs = false;
	public bool _bSelectAllUnderHouseProcObjs = false;
	public bool _bClearAllOccupiedGridSpaces = false;

	[Header("Options")]
	public Vector2 _objSize = new Vector2(10, 10); // Should always be multiples of 5
	public bool _bDestroyAfterSpawnChildren = false;
	public bool _bCanBeAutomaticallyActive = true;

	[Header("Prefabs")]
	public ProcPrefab _top_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());
	public ProcPrefab _bottom_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());
	public ProcPrefab _left_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());
	public ProcPrefab _right_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());

	public ProcPrefab _topLeft_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());
	public ProcPrefab _topRight_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());
	public ProcPrefab _bottomLeft_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());
	public ProcPrefab _bottomRight_prefabs = new ProcPrefab(new List<GameObject>(), new List<float>());

	private List<bool> _dirDone = new List<bool>();

	private bool _bPendingDestroy = false;

	private GameObject _holder = null;

	void OnEnable()
	{
		// https://answers.unity.com/questions/39313/how-do-i-get-a-callback-every-frame-in-edit-mode.html
		//EditorApplication.update += Update;
	}

	void Awake()
	{
		GameObject go = GameObject.Find("WS_ProceduralGen_Holder");
		if (go != null)
		{
			_WS_ProceduralGen = go.GetComponent<WS_ProceduralGen>();
			_holder = go;
		}

		//_underHousePrefab = Resources.Load("BBB_S2/Prefabs/underHousePref1") as GameObject;

		_bOnlySpawnObjsNearTracks = false;
		_bActive = false;
		_bActiveInfinite = false;
		//_activationTimesCount_actual = 0;
		//_activationTimesCount = 0;
		_bDestroyAllNonBaseProcObjs = false;

		ResetDirDone();
	}

	void Start()
	{
		_bDestroyAllNonBaseProcObjs = false; // Make sure

		_WS_ProceduralGen = GameObject.Find("WS_ProceduralGen_Holder").GetComponent<WS_ProceduralGen>(); //? Again necessary?
		_WS_ProceduralGen.AllGameObjs.Add(gameObject);
		if (_bIsBaseProcObj) _WS_ProceduralGen.BaseObjs.Add(gameObject);

		//_WS_ProceduralGen.OccupiedGridSpace_objs.Add(gameObject);
		Vector3 p = transform.position; p.y = 0;
		_WS_ProceduralGen.OccupiedGridSpaces.Add(p);
	}

	void Update()
	{
		if (Application.isPlaying) return;

		//if (PauseMenu.isPaused) return;

		//print(Time.time);

		if (_bPendingDestroy)
		{
			DestroyImmediate(gameObject);
			return;
		}

		if (_bOnlySpawnObjsNearTracks) Debug.DrawLine(transform.position, transform.position + new Vector3(0, 50, 0), Color.green, 2.0f);

		if (_bActive || (_bActiveInfinite && _bCanBeAutomaticallyActive))
		{
			Run();

			_bActive = false;
		}

		//if (_activationTimesCount > 0)
		//{
		//	_activationTimesCount_actual = _activationTimesCount;
		//	_activationTimesCount = 0;
		//}

		//if (_activationTimesCount_actual > 0)
		//{
		//	Run();
		//}

		//

		if (_bDestroyAllNonBaseProcObjs)
		{
			_WS_ProceduralGen.DestroyAllProcObjsExceptBaseObjs();
			_bDestroyAllNonBaseProcObjs = false;
		}

		if (_bResetAllProc1DirDones)
		{
			_WS_ProceduralGen.ResetAllProc1DirDones();
			_bResetAllProc1DirDones = false;
		}

		if (_bSelectAllProc1BaseObjs)
		{
			SelectAllProc1BaseObjs();
			_bSelectAllProc1BaseObjs = false;
		}

		if (_bSelectAllUnderHouseProcObjs)
		{
			SelectAllUnderHouseProcObjs();
			_bSelectAllUnderHouseProcObjs = false;
		}

		if (_bClearAllOccupiedGridSpaces)
		{
			_WS_ProceduralGen.ClearOccupiedGridSpaces();
			_bClearAllOccupiedGridSpaces = false;
		}
	}

	public void ResetDirDone()
	{
		//print("ResetDirDone()");

		_dirDone.Clear(); // Make sure it's empty
		for (int i = 0; i < 8; ++i)
		{
			_dirDone.Add(false);
		}

		if (_top_prefabs._prefabs.Count == 0) _dirDone[0] = true;
		if (_right_prefabs._prefabs.Count == 0) _dirDone[1] = true;
		if (_bottom_prefabs._prefabs.Count == 0) _dirDone[2] = true;
		if (_left_prefabs._prefabs.Count == 0) _dirDone[3] = true;

		if (_topLeft_prefabs._prefabs.Count == 0) _dirDone[4] = true;
		if (_topRight_prefabs._prefabs.Count == 0) _dirDone[5] = true;
		if (_bottomLeft_prefabs._prefabs.Count == 0) _dirDone[6] = true;
		if (_bottomRight_prefabs._prefabs.Count == 0) _dirDone[7] = true;
	}

	private void Run()
	{
		if (_WS_ProceduralGen.TerrainObjs.Count == 0)
		{
			_WS_ProceduralGen.GetAllTerrainObjs();
		}

		if (_dirDone.Count != 8) // Should only run the first time we hit the button
		{
			ResetDirDone();
		}

		// TODO: Get all rails in the world - so we can limit the range of the proc to near rails (check if the spawn pnt is within x range of any rail before spawning)

		//

		float distApartX = _objSize.x;
		float distApartY = _objSize.y;

		Vector3 tfrot_euler = new Vector3(0, transform.rotation.eulerAngles.y, 0);
		Quaternion tfrot = Quaternion.Euler(tfrot_euler);

		_top = transform.position + (tfrot * new Vector3(distApartY, 0, 0));
		_right = transform.position + (tfrot * new Vector3(0, 0, distApartX));
		_bottom = transform.position + (tfrot * new Vector3(-distApartY, 0, 0));
		_left = transform.position + (tfrot * new Vector3(0, 0, -distApartX));

		_topRight = transform.position + (tfrot * new Vector3(distApartY, 0, distApartX));
		_topLeft = transform.position + (tfrot * new Vector3(distApartY, 0, -distApartX));
		_bottomRight = transform.position + (tfrot * new Vector3(-distApartY, 0, distApartX));
		_bottomLeft = transform.position + (tfrot * new Vector3(-distApartY, 0, -distApartX));

		List<Object> newObjs = new List<Object>();

		for (int i = 0; i < 8; ++i)
		{
			if (_dirDone[i]) continue;

			//

			List<Vector3> testPositions = new List<Vector3>();

			ProcPrefab procPrefab = new ProcPrefab();

			if (i == 0) { testPositions.Add(_top); procPrefab = _top_prefabs; }
			if (i == 1) { testPositions.Add(_right); procPrefab = _right_prefabs; }
			if (i == 2) { testPositions.Add(_bottom); procPrefab = _bottom_prefabs; }
			if (i == 3) { testPositions.Add(_left); procPrefab = _left_prefabs; }

			if (i == 4) { testPositions.Add(_topLeft); procPrefab = _topLeft_prefabs; }
			if (i == 5) { testPositions.Add(_topRight); procPrefab = _topRight_prefabs; }
			if (i == 6) { testPositions.Add(_bottomLeft); procPrefab = _bottomLeft_prefabs; }
			if (i == 7) { testPositions.Add(_bottomRight); procPrefab = _bottomRight_prefabs; }

			if (procPrefab._prefabs.Count == 0) continue;

			int prefab_idx = BBBStatics.RandInt(0, procPrefab._prefabs.Count); // Apparently not inclusive (23-7-18)
			GameObject newPrefabArchetype = procPrefab._prefabs[prefab_idx];

			//

			List<float> fl = new List<float> { 0.0f, 90.0f, 180.0f, 270.0f };
			Quaternion randomRot = Quaternion.Euler(0.0f, fl[BBBStatics.RandInt(0, 3)], 0.0f);
			Quaternion rotToUse = randomRot;

			if (prefab_idx < procPrefab._fixedRotationOffset.Count)
			{
				rotToUse = Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y + procPrefab._fixedRotationOffset[prefab_idx], 0.0f);
			}

			//

			if (newPrefabArchetype != null)
			{
				Proc1 pfProc = newPrefabArchetype.GetComponent<Proc1>();
				int xSz = Mathf.RoundToInt(pfProc._objSize.x / 10.0f); // These assume that the size is a multiple of 5
				int ySz = Mathf.RoundToInt(pfProc._objSize.y / 10.0f);

				//Vector3 dir = testPositions[0] - transform.position;
				//Vector3 dirRot_euler = new Vector3(0, dir.y, 0);
				//Quaternion dirRot = Quaternion.Euler(dir);

				if (xSz > 1)
				{
					for (int j = 1; j < xSz; ++j)
					{
						Vector3 newPos = testPositions[0] + (tfrot * new Vector3(10 * j, 0, 0));
						testPositions.Add(newPos);

						//Debug.DrawLine(newPos, newPos + new Vector3(0, 50, 0), Color.green, 2.0f);
					}

					int tpc = testPositions.Count; // Needed as the count increases below and would cause an infinite loop
					for (int j = 0; j < tpc; ++j)
					{
						for (int k = 1; k < ySz; ++k)
						{
							Vector3 newPosB = testPositions[j] + (tfrot * new Vector3(0, 0, -10 * k));
							testPositions.Add(newPosB);

							//Debug.DrawLine(newPosB, newPosB + new Vector3(0, 50, 0), Color.cyan, 2.0f);
						}
					}
				}

				//

				bool bContinueOuter = false;
				for (int j = 0; j < testPositions.Count; ++j)
				{
					bool bHit = CheckForHit(testPositions[j], 10.0f, _WS_ProceduralGen.TerrainObjs); //! Note: Make sure that every prefab has a ground-plane collider
					if (bHit)
					{
						bContinueOuter = true; // There is already something there

						//

						///
						int type = 1;
						///

						if (type == 1) // Block direction if anything is hit in any of the test positions
						{
							_dirDone[i] = true;
						}
						if (type == 2) // Block direction if anything is hit only if the prefab is single-grid space sized
						{
							if (testPositions.Count == 1) _dirDone[i] = true;
						}
						if (type == 3) // Block direction if anything is hit only in the main/first/closest test position
						{
							if (j == 0) _dirDone[i] = true;
						}

						//

						break;
					}
				}
				if (bContinueOuter) continue;

				//

				Vector3 spawnLoc = GetAvgLoc(testPositions);

				//if (_WS_ProceduralGen != null && !_WS_ProceduralGen.IsVecWithinRangeOfAnyRail(spawnLoc, 50.0f, 150.0f)) continue;
				//if (_bOnlySpawnObjsNearTracks && !_WS_ProceduralGen.IsVecWithinRangeOfAnyRail(spawnLoc, 50.0f, 150.0f))
				if (_bOnlySpawnObjsNearTracks && !_WS_ProceduralGen.IsVecWithinRangeOfAnyRail(spawnLoc, 50.0f, 150.0f))
				{
					//xDestroy(gameObject); /// TEST -- destroy the object if it can't spawn anything -- 25-7-18
					continue;
				}

				//

				GameObject go = Instantiate(newPrefabArchetype, spawnLoc, rotToUse, _holder.transform);
				//go.name = "procObj" + BBBStatics.RandInt(0, 99999);
				go.name = "procObj_" + _WS_ProceduralGen.AllGameObjs.Count + "_" + BBBStatics.RandInt(0, 99999);
				newObjs.Add(go);

				_dirDone[i] = true;

				//

				Proc1 goProc = go.GetComponent<Proc1>();
				if (goProc != null) // && _activationTimesCount_actual > 0)
				{
					// System didn't work very well - lots of issues (23-7-18)
					//--_activationTimesCount_actual;
					//goProc._bActive = true;
					//goProc._activationTimesCount_actual = _activationTimesCount_actual; // Pass it down the line
					//_activationTimesCount_actual = 0; // We've passed it, no need to retain it

					if (_bActiveInfinite)
					{
						if (goProc._bCanBeAutomaticallyActive) goProc._bActiveInfinite = true;
						//goProc._bActive = true;
						if (_bOnlySpawnObjsNearTracks) goProc._bOnlySpawnObjsNearTracks = true;
					}
				}

				//

				for (int j = 0; j < testPositions.Count; ++j)
				{
					if (!_WS_ProceduralGen.OccupiedGridSpaces.Contains(testPositions[j]))
					{
						_WS_ProceduralGen.OccupiedGridSpaces.Add(testPositions[j]);

						//

						if (testPositions.Count > 1)
						{
							if (_WS_ProceduralGen._underHousePrefab == null) continue;
							GameObject goB = Instantiate(_WS_ProceduralGen._underHousePrefab, testPositions[j], rotToUse, _holder.transform);
							//goB.name = "procObj_underHouse" + BBBStatics.RandInt(0, 99999);
							goB.name = "procObj_underHouse" + _WS_ProceduralGen.AllGameObjs.Count + "_" + BBBStatics.RandInt(0, 99999);

							///newObjs.Add(goB); // If we want the under-house procs to be auto selected

							//Debug.DrawLine(testPositions[j], testPositions[j] + new Vector3(0, 100, 0), Color.white, 1.0f);
						}
					}
				}
			}
		}

#if UNITY_EDITOR
		List<Object> newSel = new List<Object>(Selection.objects);

		newSel.AddRange(newObjs);

		List<Object> keepers = new List<Object>();
		for (int i = 0; i < newSel.Count; ++i)
		{
			GameObject go = newSel[i] as GameObject;
			if (go != null)
			{
				Proc1 pr1 = go.GetComponent<Proc1>();
				if (pr1 != null && pr1._dirDone.Count == 8)
				{
					// Does it have an empty space in any up/down direction?

					if (
							(!pr1._dirDone[0])
						|| (!pr1._dirDone[1])
						|| (!pr1._dirDone[2])
						|| (!pr1._dirDone[3])
						)
					{
						keepers.Add(newSel[i]);
					}
				}
			}
		}
		newSel = keepers;

		//

		//newSel.AddRange(newObjs);

		//

		Selection.objects = newSel.ToArray(); /// Deactivate for final build
#endif

		//

		if (_bDestroyAfterSpawnChildren)
		{
			print("_bDestroyAfterSpawnChildren @ Run");
			_bPendingDestroy = true;
		}
	}

	//

	public static bool CheckForHit(Vector3 at, float startHeight, List<GameObject> ignoreList)
	{
		Ray r = new Ray { origin = at + new Vector3(0, startHeight, 0), direction = Vector3.down };
		List<RaycastHit> rch = BBBStatics.GetAllRaycastAllHitsInDistOrder(r, startHeight * 2.0f, ignoreList);
		if (rch.Count > 0) return true;
		return false;
	}

	public Vector3 GetAvgLoc(List<Vector3> vecs)
	{
		Vector3 result = new Vector3();
		for (int i = 0; i < vecs.Count; ++i)
		{
			result += vecs[i];
		}
		return result / vecs.Count;
	}

	public void SelectAllProc1BaseObjs()
	{
#if UNITY_EDITOR
		List<Object> newSel = new List<Object>(); // (Selection.objects)

		foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			//if (go == gameObject) continue;

			Proc1 pr1 = go.GetComponent<Proc1>();
			if (pr1 != null)
			{
				if (pr1._bIsBaseProcObj)
				{
					newSel.Add(go);
				}
			}
		}

		//

		Selection.objects = newSel.ToArray(); /// Deactivate for final build
#endif
	}

	public void SelectAllUnderHouseProcObjs()
	{
#if UNITY_EDITOR
		List<Object> newSel = new List<Object>(); // (Selection.objects)

		foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			//if (go == gameObject) continue;

			Proc1 pr1 = go.GetComponent<Proc1>();
			if (pr1 != null)
			{
				if (pr1.gameObject.name.Contains("procObj_underHouse"))
				{
					newSel.Add(go);
				}
			}
		}

		//

		print("procObj_underHouse count: " + newSel.Count);

		Selection.objects = newSel.ToArray(); /// Deactivate for final build
#endif
	}

	public void OnDestroy()
	{
		if (_WS_ProceduralGen == null) return;

		Vector3 p = transform.position; p.y = 0;
		_WS_ProceduralGen.OccupiedGridSpaces.Remove(p);

		_WS_ProceduralGen.AllGameObjs.Remove(gameObject);
		if (_WS_ProceduralGen.BaseObjs.Contains(gameObject)) _WS_ProceduralGen.BaseObjs.Remove(gameObject);
	}
}
