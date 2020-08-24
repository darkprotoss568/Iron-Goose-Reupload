using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WS_ProceduralGen : MonoBehaviour
{
	//private WorldScript _worldScript;
	private RTSCameraController _rtsCam;

	private List<RailScript> _allRails = new List<RailScript>();
	public List<RailScript> AllRails { get { return _allRails; } set { _allRails = value; } }
	//List<Proc1> _allProcObjs;
	private List<GameObject> _allGameObjs = new List<GameObject>();
	public List<GameObject> AllGameObjs { get { return _allGameObjs; } set { _allGameObjs = value; } }

	private GameObject _loco = null;

	public List<GameObject> _spawnPrefabs = new List<GameObject>();
	public GameObject _spawnBelowRailPrefab;

	private List<GameObject> _baseObjs = new List<GameObject>();
	public List<GameObject> BaseObjs { get { return _baseObjs; } set { _baseObjs = value; } }

	public bool _bIsProcSpawningOnline = true;
	public bool _bIsDeletionOnline = true;
	public bool _bIsRailSpawningOnline = true;

	//private Vector2 _lastTestPos = Vector2.zero;
	//private Vector2 _lastTestPos_prev = Vector2.zero;

	//private List<GameObject> _occupiedGridSpace_objs = new List<GameObject>();
	//public List<GameObject> OccupiedGridSpace_objs { get { return _occupiedGridSpace_objs; } set { _occupiedGridSpace_objs = value; } }

	private List<Vector3> _occupiedGridSpaces = new List<Vector3>();
	public List<Vector3> OccupiedGridSpaces { get { return _occupiedGridSpaces; } set { _occupiedGridSpaces = value; } }

	public GameObject _underHousePrefab = null;

	private List<GameObject> _terrainObjs = new List<GameObject>();
	public List<GameObject> TerrainObjs { get { return _terrainObjs; } set { _terrainObjs = value; } }

	private void Awake()
	{
		_underHousePrefab = Resources.Load("BBB_S2/Prefabs/underHousePref1") as GameObject;

		GetAllTerrainObjs();
	}

	void Start()
	{
		//_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
		//_loco = _worldScript.LocomotiveObjectRef;
		_loco = GameObject.Find("Locomotive");
		_rtsCam = Camera.main.GetComponent<RTSCameraController>();

		_allRails = new List<RailScript>((RailScript[])FindObjectsOfType(typeof(RailScript))); // Get the initial/original rails in the world
		//_allProcObjs = new List<Proc1>((Proc1[])FindObjectsOfType(typeof(Proc1)));
		_allGameObjs.AddRange(new List<GameObject>((GameObject[])FindObjectsOfType(typeof(GameObject)))); // Get the initial/original GOs in the world

		///_allGameObjs.AddRange(_worldScript.GetAllTGOsInWorld_AsGOs()); // ?

		List<GameObject> keepers = new List<GameObject>();
		for (int i = 0; i < _allGameObjs.Count; ++i)
		{
			MeshRenderer mr = _allGameObjs[i].GetComponent<MeshRenderer>();
			if (mr != null) { keepers.Add(mr.gameObject); }
		}
		_allGameObjs = keepers;
	}

	void Update()
	{
		//if (!_bIsOnline) return;

		//if (_loco == null && _worldScript.LocomotiveObjectRef != null)
		//{
		//	_loco = _worldScript.LocomotiveObjectRef;
		//}

		//if (_worldScript != null)
		//{
		//	_worldScript.MusicScript.BIsPlayingMusic = false; //? TEMPORARY
		//}

		//for (int i = 0; i < _occupiedGridSpaces.Count; ++i)
		//{
		//	//_occupiedGridSpaces
		//	Debug.DrawLine(_occupiedGridSpaces[i], _occupiedGridSpaces[i] + new Vector3(0, 20, 0), Color.red, Time.deltaTime);
		//}

		//

		if (_bIsProcSpawningOnline) SpawnElements();

		if (_bIsDeletionOnline) ManageElements();
	}

	void SpawnElements()
	{
		if (_baseObjs.Count > 20) return;

		//! Spawn a grid of road-centre pieces (with _bIsBaseProcObj == true) and set them to active
		//! Make sure that they are aligned and far enough apart -- ensure alignment by setting the position of the newbie to match either the x or y of the last spawned one

		//! Newly spawned items must be within x dist of a rail segment

		/// First, get a random location offset from the locomotive
		/// - Must be in the middle of a 5x5 (or 10x10) grid space
		/// - Run a line trace down to it to test if the space is already occupied

		//float minDist = 50.0f;
		//float maxDist = 300.0f;

		int gridSize = 10;

		Vector2 locoPosFixedOffset = new Vector2(5, 5); // To match the grid of pre-placed objects -- might want to get this dynamically

		Vector2 locoPosRounded = new Vector2(_loco.transform.position.x, _loco.transform.position.z);
		locoPosRounded.x = BBBStatics.RoundUpToNearestMultiple(locoPosRounded.x, gridSize);
		locoPosRounded.y = BBBStatics.RoundUpToNearestMultiple(locoPosRounded.y, gridSize);
		locoPosRounded += locoPosFixedOffset;

		//

		//List<Vector3> railPositions = new List<Vector3>();
		//for (int i = 0; i < _allRails.Count; ++i)
		//{
		//	railPositions.Add(_allRails[i].transform.position);
		//}

		//Debug.DrawLine(BBBStatics.V3(locoPosRounded), BBBStatics.V3(locoPosRounded) + new Vector3(0, 20, 0), Color.red, Time.deltaTime);

		for (int i = 0; i < 10; ++i) // How many times we test per frame
		{
			float randX = gridSize * BBBStatics.RandInt(10, 30);
			float randY = gridSize * BBBStatics.RandInt(10, 30);

			if (BBBStatics.RandInt(0, 2) == 1) randX *= -1;
			if (BBBStatics.RandInt(0, 2) == 1) randY *= -1;

			Vector3 testPos = new Vector3(locoPosRounded.x + randX, 0.0f, locoPosRounded.y + randY);

			//
			//int r = BBBStatics.RandInt(0, 2);
			//if (r == 1)
			//{
			//	if (_lastTestPos != Vector2.zero) testPos.x = _lastTestPos.x;
			//	if (_lastTestPos_prev != Vector2.zero) testPos.y = _lastTestPos_prev.y;
			//}
			//else
			//{
			//	if (_lastTestPos_prev != Vector2.zero) testPos.x = _lastTestPos_prev.x;
			//	if (_lastTestPos != Vector2.zero) testPos.y = _lastTestPos.y;
			//}
			//

			// TODO: Check if testPos is within x range of any rail -- or just make it so rails spawn grid pieces underneath themselves
			if (!IsVecWithinRangeOfAnyRail(testPos, 50.0f, 150.0f))
			{
				continue;
			}

			if (IsGridSpaceClear(testPos))
			{
				//Debug.DrawLine(testPos, testPos + new Vector3(0, 50, 0), Color.red, Time.deltaTime);

				if (_spawnPrefabs.Count > 0)
				{
					GameObject prefabToSpawn = _spawnPrefabs[BBBStatics.RandInt(0, _spawnPrefabs.Count)];

					GameObject go = Instantiate(prefabToSpawn, testPos, Quaternion.identity);
					Proc1 proc1 = go.GetComponent<Proc1>();
					if (proc1 != null)
					{
						proc1._bActive = true;
						proc1.BActiveInfinite = true;
						proc1.BOnlySpawnObjsNearTracks = true;
					}

					//_lastTestPos_prev = _lastTestPos;
					//_lastTestPos = testPos;
				}

				break;
			}
			else
			{
				//Debug.DrawLine(testPos, testPos + new Vector3(0, 50, 0), Color.yellow, Time.deltaTime);
			}
		}
	}

	void ManageElements()
	{
		/// Iterate over _allGameObjs, check if they are in front of or behind the locomotive (within 180 degrees)
		/// Then remove any that are beyond a certain distance behind the locomotive

		float locoAng = BBBStatics.WrapAround(_loco.transform.forward.y + 270.0f, 360.0f); // Behind the loco

		List<GameObject> toDestroy = new List<GameObject>();
		for (int i = 0; i < _allGameObjs.Count; ++i)
		{
			if (_allGameObjs[i] == null) continue;
			if (_allGameObjs[i] == _loco) continue;
			if (_allGameObjs[i] == _rtsCam.gameObject) continue;

			Vector3 start = _loco.transform.position;
			Vector3 end = _allGameObjs[i].transform.position;

			Vector3 dir = end - start;
			if (dir == Vector3.zero) continue;
			Quaternion dir_q = Quaternion.LookRotation(dir);

			float diff = BBBStatics.AngleDiff(dir_q.eulerAngles.y, locoAng);

			float dist = Vector3.Distance(start, end);

			if (dist > 200.0f)
			{
				if (Mathf.Abs(diff) > 90)
				{
					//Debug.DrawLine(start, end, Color.red, Time.deltaTime);
					toDestroy.Add(_allGameObjs[i]);
				}
			}
		}

		for (int i = 0; i < toDestroy.Count; ++i)
		{
			_allGameObjs.Remove(toDestroy[i]);
			if (_baseObjs.Contains(toDestroy[i])) _baseObjs.Remove(toDestroy[i]);

			RailScript rs = toDestroy[i].GetComponent<RailScript>();
			if (rs != null)
			{
				_allRails.Remove(rs);
			}

			Destroy(toDestroy[i]);
		}
	}

	public bool IsVecWithinRangeOfAnyRail(Vector3 vec, float minRange, float maxRange)
	{
		for (int i = 0; i < AllRails.Count; ++i)
		{
			float dist = Vector3.Distance(AllRails[i].transform.position, vec);

			if (dist >= minRange && dist <= maxRange)
			{
				return true;
			}
		}

		return false;
	}

	public bool IsGridSpaceClear(Vector3 gridPnt)
	{
		if (_occupiedGridSpaces.Contains(gridPnt))
		{
			return false;
		}

		bool bHit = Proc1.CheckForHit(gridPnt, 1.0f, _terrainObjs);
		if (bHit)
		{
			return false;
		}

		return true;
	}

	public void ClearOccupiedGridSpaces()
	{
		_occupiedGridSpaces.Clear();
	}

	public void SpawnElementsBelowRail(RailScript rail)
	{
		if (!_bIsRailSpawningOnline) return;
		if (_spawnBelowRailPrefab == null) return;

		Vector3 railStart = rail.GetFrontSocket().transform.position;
		Vector3 railEnd = rail.GetBehindSocket().transform.position;

		int gridSize = 10;
		Vector3 fixedGridOffset = new Vector3(-5, 0, -5); // To match the grid of pre-placed objects -- might want to get this dynamically

		Vector3 centre = BBBStatics.BetweenAt(railStart, railEnd, 0.5f);
		Vector3 centreRounded = new Vector3(centre.x, 0.0f, centre.z);
		centreRounded.x = BBBStatics.RoundUpToNearestMultiple(centreRounded.x, gridSize);
		centreRounded.z = BBBStatics.RoundUpToNearestMultiple(centreRounded.z, gridSize);
		centreRounded += fixedGridOffset;

		if (IsGridSpaceClear(centreRounded))
		{
			GameObject go = Instantiate(_spawnBelowRailPrefab, centreRounded, Quaternion.identity);
			_allGameObjs.Add(go);
		}

		//

		MeshRenderer mr = rail.gameObject.GetComponent<MeshRenderer>();
		if (mr)
		{
			Vector3 bnds_c = mr.bounds.center;
			Vector3 bnds_e = mr.bounds.extents;
			//bnds_e = Quaternion.Euler(new Vector3(0, rail.transform.rotation.eulerAngles.y + 90, 0)) * bnds_e;

			//float railAngle = rail.transform.rotation.eulerAngles.y;

			//Vector3 lns = bnds_c;
			//lns.y = 1;
			//Debug.DrawLine(lns, new Vector3(lns.x + bnds_e.x, lns.y, lns.z + bnds_e.z), Color.red, 30.0f);
			//Debug.DrawLine(lns, new Vector3(lns.x + -bnds_e.x, lns.y, lns.z + bnds_e.z), Color.green, 30.0f);
			//Debug.DrawLine(lns, new Vector3(lns.x + bnds_e.x, lns.y, lns.z + -bnds_e.z), Color.yellow, 30.0f);
			//Debug.DrawLine(lns, new Vector3(lns.x + -bnds_e.x, lns.y, lns.z + -bnds_e.z), Color.cyan, 30.0f);

			//Vector3 topLeft = new Vector3(bnds_c.x + bnds_e.x, 0, bnds_c.z + bnds_e.z);
			//Vector3 topRight = new Vector3(bnds_c.x + bnds_e.x, 0, bnds_c.z + -bnds_e.z);
			//Vector3 bottomLeft = new Vector3(bnds_c.x + -bnds_e.x, 0, bnds_c.z + bnds_e.z);
			//Vector3 bottomRight = new Vector3(bnds_c.x + -bnds_e.x, 0, bnds_c.z + -bnds_e.z);

			Vector3 top = new Vector3(bnds_c.x + bnds_e.x, 0, bnds_c.z);
			Vector3 right = new Vector3(bnds_c.x, 0, bnds_c.z + -bnds_e.z);
			Vector3 bottom = new Vector3(bnds_c.x + -bnds_e.x, 0, bnds_c.z);
			Vector3 left = new Vector3(bnds_c.x, 0, bnds_c.z + bnds_e.z);

			// Extra leeway for more ground pieces around it
			///top += new Vector3(gridSize * 2, 0, 0);
			///right += new Vector3(0, 0, -gridSize * 2);
			///bottom += new Vector3(-gridSize * 2, 0, 0);
			///left += new Vector3(0, 0, gridSize * 2);

			//top = RotatePointAroundPivot(top, bnds_c, new Vector3(0, railAngle, 0));
			//right = RotatePointAroundPivot(right, bnds_c, new Vector3(0, railAngle, 0));
			//bottom = RotatePointAroundPivot(bottom, bnds_c, new Vector3(0, railAngle, 0));
			//left = RotatePointAroundPivot(left, bnds_c, new Vector3(0, railAngle, 0));

			//Debug.DrawLine(top, top + new Vector3(0, 20, 0), Color.red, 30.0f);
			//Debug.DrawLine(right, right + new Vector3(0, 20, 0), Color.green, 30.0f);
			//Debug.DrawLine(bottom, bottom + new Vector3(0, 20, 0), Color.yellow, 30.0f);
			//Debug.DrawLine(left, left + new Vector3(0, 20, 0), Color.cyan, 30.0f);

			//

			List<Vector3> directions = new List<Vector3>
			{
				new Vector3(1, 0, 0),
				new Vector3(-1, 0, 0),
				new Vector3(0, 0, -1),
				new Vector3(0, 0, 1),
				new Vector3(1, 0, 1),
				new Vector3(-1, 0, 1),
				new Vector3(1, 0, -1),
				new Vector3(-1, 0, -1),
			};

			for (int i = 0; i < directions.Count; ++i)
			{
				for (int j = 1; j < 10; ++j)
				{
					Vector3 testPos = centreRounded + new Vector3(directions[i].x * (gridSize * j), 0, directions[i].z * (gridSize * j));

					if (testPos.x <= top.x && testPos.x >= bottom.x && testPos.z <= left.z && testPos.z >= right.z)
					{
						if (IsGridSpaceClear(testPos))
						{
							GameObject go = Instantiate(_spawnBelowRailPrefab, testPos, Quaternion.identity);
							_allGameObjs.Add(go);
						}
					}
					else
					{
						break;
					}
				}
			}
		}
	}

	public void GetAllTerrainObjs()
	{
        //print("GetAllTerrainObjs() 1");

        object[] allObjs = FindObjectsOfType(typeof(GameObject));

        int length = allObjs.Length;
        for (int i = 0; i < length; i++)
        {
            GameObject go = (GameObject)allObjs[i];
            Terrain terrain = go.GetComponent<Terrain>();
            if (terrain != null)
            {
                //print("GetAllTerrainObjs() 2");

                _terrainObjs.Add(go);
            }
        }
	}

	public void DestroyAllProcObjsExceptBaseObjs()
	{
        //bool bDestroyUsAfter = false;

        object[] allObjs = FindObjectsOfType(typeof(GameObject));

        int length = allObjs.Length;
        for (int i = 0; i < length; i++)
        {
            GameObject go = (GameObject)allObjs[i];
            if (go.name.Contains("procObj"))
            {
                //if (go == gameObject)
                //{
                //	bDestroyUsAfter = true; // We are also a 'procObj' so we need to self destruct once we're done
                //}
                //else
                //{
                //	DestroyImmediate(go);
                //}

                DestroyImmediate(go);
            }
        }

		// Reset dirDones on all remaining Proc1 script objects
		ResetAllProc1DirDones();

		//if (bDestroyUsAfter)
		//{
		//	DestroyImmediate(gameObject);
		//}

		OccupiedGridSpaces.Clear();
	}

	public void ResetAllProc1DirDones()
	{
        object[] allObjs = FindObjectsOfType(typeof(GameObject));

        int length = allObjs.Length;
        for (int i = 0; i < length; i++)
        {
            GameObject go = (GameObject)allObjs[i];
            Proc1 pr1 = go.GetComponent<Proc1>();
            if (pr1 != null)
            {
                pr1.ResetDirDone();
            }
        }
	}
}
