using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; /// Deactivate for final build
#endif

//

[System.Serializable] // Make it editable in the inspector
public class ProcPrefab_Env
{
#if UNITY_EDITOR
	public GameObject _prefab;
	public Vector2 _minMaxScales;
	public int _maxCount = -1;
	private int _count = 0; public int Count { get { return _count; } set { _count = value; } }

	//public ProcPrefab_Env()
	//{
	//	_prefab = null;
	//	_minMaxScales = new Vector2(0.0f, 0.0f);
	//	_pcnt = 0.0f;
	//}
#endif
}

//

#if UNITY_EDITOR
[ExecuteInEditMode] /// Deactivate for final build
#endif
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class EnvProcGenScript : MonoBehaviour
{
#if UNITY_EDITOR
	public static List<EnvProcGenScript> _allEnvProcGenScripts = new List<EnvProcGenScript>();

	static EnvProcGenScript()
	{
		EditorApplication.update += UpdateStatic;
		_allEnvProcGenScripts = new List<EnvProcGenScript>();
	}

	static float _classDelta = 0.0f;

	static RTSCameraController _inGameCam;

	//public EnvProcGenScript()
	//{
	//	_allEnvProcGenScripts.Add(this);
	//}

	static void UpdateStatic()
	{
		float t = (float)EditorApplication.timeSinceStartup;
		for (int i = 0; i < _allEnvProcGenScripts.Count; ++i)
		{
			_allEnvProcGenScripts[i].UpdateManualCall();
		}
		_classDelta = (float)EditorApplication.timeSinceStartup - t;
	}

	//
	//
	//
	//

	[Header("Create")]
	public bool _bRun = false;
	public bool _bSpawnEdgeFence = false;

	[Header("Remove")]
	public bool _bRemoveAllObjs = false;
	public bool _bRemoveAllFenceObjs = false;

	[Header("Options")]
	public int _preCullObjCount = 1000;
	public bool _bRunOnVertMove = false;
	public bool _bShowPointDebugs = false;
	public bool _bDisplayEdgeDebugLines = false;
	public float _maxTreeAngle = 0.1f; // 0 - 1

	//[Header("Functions")]
	//public bool _bClearDebugLines = false;

	[Header("Spawn prefab archetypes")]
	//public List<GameObject> _spawnPrefabs = new List<GameObject>();
	//public List<Vector2> _spawnPrefabs_minMaxScales = new List<Vector2>();
	//public List<float> _spawnPrefabs_pcnt = new List<float>();

	public List<ProcPrefab_Env> _ProcPrefab_Envs = new List<ProcPrefab_Env>();
	public GameObject _edgeFenceArchetype;

	//

	private GameObject _spawnedObjHolder;
	private GameObject _spawnedFenceHolder;
	//private List<GameObject> _spawnedObjs = new List<GameObject>();

	//

	//private bool _bAnyChildVisible = false;

	private bool _bCulled = false;

	private void OnBecameInvisible()
	{

	}

	void Awake()
	{
		//if (!_allEnvProcGenScripts.Contains(this)) _allEnvProcGenScripts.Add(this);

		_bRunOnVertMove = false;
	}

	void Start()
	{
		_bRun = false;
		_bRemoveAllObjs = false;
		_bRunOnVertMove = false;

		// Hide everything in-game

		//gameObject.GetComponent<Renderer>().enabled = !Application.isEditor || Application.isPlaying;
		///gameObject.GetComponent<Renderer>().enabled = false;

		//int cc = transform.childCount;
		//for (int i = 0; i < cc; ++i)
		//{
		//	if (!transform.GetChild(i).name.Contains("vert")) continue;
		//	//transform.GetChild(i).gameObject.SetActive(false);
		//}

		////gameObject.SetActive(false);
		//gameObject.GetComponent<Renderer>().enabled = false;

		if (_inGameCam == null && Camera.main != null) _inGameCam = Camera.main.GetComponent<RTSCameraController>();
	}

	//void OnEnable()
	//{
	//	gameObject.GetComponent<Renderer>().enabled = !Application.isEditor || Application.isPlaying;
	//}
	//void OnDisable()
	//{
	//	gameObject.GetComponent<Renderer>().enabled = true;
	//}

	private void OnDestroy()
	{
		///gameObject.GetComponent<Renderer>().enabled = true;
		if (_allEnvProcGenScripts.Contains(this)) _allEnvProcGenScripts.Remove(this);
	}

	private void OnEnable()
	{
		if (!_allEnvProcGenScripts.Contains(this)) _allEnvProcGenScripts.Add(this);

		//if (_inGameCam == null && Camera.main != null) _inGameCam = Camera.main.GetComponent<RTSCameraController>();
	}

	void Update()
	{

	}

	void UpdateManualCall()
	{
		///
		//CullAtDist();
		///

		HideAll(true); /// Make sure that everything is visible

		if (!Application.isEditor || Application.isPlaying) return;

		if (_bRun)
		{
			_bRun = false;
			Run();
		}

		if (_bSpawnEdgeFence)
		{
			_bSpawnEdgeFence = false;
			SpawnEdgeFence();
		}

		//if (_bClearDebugLines)
		//{
		//	_bClearDebugLines = false;
		//}

		if (_bRemoveAllObjs)
		{
			_bRemoveAllObjs = false;
			RemoveAllObjs();
		}

		if (_bRemoveAllFenceObjs)
		{
			_bRemoveAllFenceObjs = false;
			RemoveAllFenceObjs();
		}

		if (_bDisplayEdgeDebugLines)
		{
			List<Object> sel = new List<Object>(Selection.objects);

			if (sel.Count > 0)
			{
				int cc = transform.childCount;

				bool bContainsAChild = false;
				for (int i = 0; i < cc; ++i)
				{
					if (sel.Contains(transform.GetChild(i).gameObject))
					{
						bContainsAChild = true;
						break;
					}
				}

				if (bContainsAChild || sel.Contains(gameObject))
				{
					//Vector3 last_v = Vector3.zero;
					//Vector3 v = Vector3.zero;

					List<Vector3> Vs = new List<Vector3>();

					for (int i = 0; i < cc; ++i)
					{
						if (transform.GetChild(i).name.Contains("vert"))
						{
							Vs.Add(transform.GetChild(i).gameObject.transform.position);
						}
					}

					for (int i = 0; i < Vs.Count; ++i)
					{
						if (i > 0)
							Debug.DrawLine(Vs[i], Vs[i - 1], Color.red, _classDelta); // _classDelta // Time.deltaTime
						else
							Debug.DrawLine(Vs[0], Vs[Vs.Count - 1], Color.red, _classDelta); // _classDelta // Time.deltaTime
					}

					//for (int i = 0; i < cc; ++i)
					//{
					//	if (transform.GetChild(i).name.Contains("vert"))
					//	{
					//		last_v = v;
					//		v = transform.GetChild(i).gameObject.transform.position;
					//		if (i >= 1) Debug.DrawLine(v, last_v, Color.red, Time.deltaTime);
					//		else Debug.DrawLine(transform.GetChild(0).gameObject.transform.position, transform.GetChild(cc - 2).gameObject.transform.position, Color.red, Time.deltaTime);
					//	}
					//}
				}
			}
		}
	}

	public void Run()
	{
		_maxTreeAngle = Mathf.Clamp01(_maxTreeAngle);

		_spawnedObjHolder = transform.Find("Holder").gameObject;

		//

		List<Vector3> vertsOrig = new List<Vector3>();
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> verts2D = new List<Vector2>();

		int cc = transform.childCount;
		for (int i = 0; i < cc; ++i)
		{
			if (!transform.GetChild(i).name.Contains("vert")) continue;

			Vector3 v = transform.GetChild(i).gameObject.transform.position;
			vertsOrig.Add(v);
			//Vector3 normal;
			//v = BBBStatics.CheckForGroundV_V2(v, 20.0f, out normal);
			verts.Add(v);
			verts2D.Add(new Vector2(v.x, v.z));

			///// Debug
			//if (i >= 1) Debug.DrawLine(vertsOrig[i - 1], vertsOrig[i], Color.red, Time.deltaTime);
			/////
		}

		Vector3 centre = BBBStatics.AveragePos(verts);

		Vector3 farthestVert = BBBStatics.GetFarthestVec(centre, verts);
		float distToFV = Vector3.Distance(centre, farthestVert);

		List<Vector3> points = GetRandOffsetPositions(centre, _preCullObjCount, distToFV); // distToFV // 20.0f

		List<Vector3> keepers = new List<Vector3>();
		for (int i = 0; i < points.Count; ++i)
		{
			if (BBBStatics.IsPointInPolygon_T3(new Vector2(points[i].x, points[i].z), verts2D))
			{
				keepers.Add(points[i]);
			}
		}
		points = keepers;

		//

		List<Vector3> normals = new List<Vector3>();

		for (int i = 0; i < points.Count; ++i)
		{
			Vector3 normal;
			points[i] = BBBStatics.CheckForGroundV_V2(points[i], 40.0f, out normal);
			normals.Add(normal);
		}

		//

		if (_bShowPointDebugs)
		{
			for (int i = 0; i < points.Count; ++i)
			{
				Debug.DrawLine(points[i], points[i] + new Vector3(0, 20, 0), Color.green, Time.deltaTime); // Time.deltaTime // 10.0f
			}
		}

		//

		SpawnObjs(points, normals);

		if (_bRunOnVertMove) SpawnEdgeFence(); // Test
	}

	private List<Vector3> GetRandOffsetPositions(Vector3 centre, int count, float maxDist)
	{
		List<Vector3> positions = new List<Vector3>();

		for (int i = 0; i < count; ++i)
		{
			positions.Add(centre + new Vector3(BBBStatics.RandFlt(-maxDist, maxDist), 0, BBBStatics.RandFlt(-maxDist, maxDist)));
		}

		return positions;
	}

	private void SpawnObjs(List<Vector3> locs, List<Vector3> normals)
	{
		RemoveAllObjs(); // If there are any

		//

		bool bAnyProcHasNoMaxCount = false;
		for (int i = 0; i < _ProcPrefab_Envs.Count; ++i)
		{
			if (_ProcPrefab_Envs[i]._maxCount < 0)
			{
				bAnyProcHasNoMaxCount = true;
				//break;
			}

			_ProcPrefab_Envs[i].Count = 0;
		}

		//

		for (int i = 0; i < locs.Count; ++i)
		{
			if (_ProcPrefab_Envs.Count > 0)
			{
				int randIdx = BBBStatics.RandInt(0, _ProcPrefab_Envs.Count);

				if (_ProcPrefab_Envs[randIdx]._maxCount < 0 || _ProcPrefab_Envs[randIdx].Count < _ProcPrefab_Envs[randIdx]._maxCount)
				{
					// Limit tree angle (only if it's a tree - must be named 'tree')

					if (_ProcPrefab_Envs[randIdx]._prefab.name.Contains("tree"))
					{
						normals[i] = new Vector3(
							 Mathf.Clamp(normals[i].x, -_maxTreeAngle, _maxTreeAngle),
							 Mathf.Clamp(normals[i].y, -(1 - _maxTreeAngle), (1 - _maxTreeAngle)),
							 Mathf.Clamp(normals[i].z, -_maxTreeAngle, _maxTreeAngle));
					}

					//

					GameObject go = Instantiate(_ProcPrefab_Envs[randIdx]._prefab, locs[i], Quaternion.FromToRotation(Vector3.up, normals[i]), _spawnedObjHolder.transform);

					//Quaternion rot = Quaternion.Euler(0, BBBStatics.RandFlt(-180.0f, 180.0f), 0);
					go.transform.Rotate(transform.up, BBBStatics.RandFlt(-180.0f, 180.0f));

					float min = _ProcPrefab_Envs[randIdx]._minMaxScales.x;
					float max = _ProcPrefab_Envs[randIdx]._minMaxScales.y;
					float s = BBBStatics.RandFlt(min, max);
					go.transform.localScale = new Vector3(s, s, s);

					//_spawnedObjs.Add(go);

					_ProcPrefab_Envs[randIdx].Count += 1;
				}
				else
				{
					if (bAnyProcHasNoMaxCount)
						--i; // Repeat -- careful with this! -- If all of the procs have a limited number of spawns, this will cause an infinite loop
				}
			}
		}
	}

	private void RemoveAllObjs()
	{
		//		for (int i = 0; i < _spawnedObjs.Count; ++i)
		//		{
		//#if UNITY_EDITOR
		//			DestroyImmediate(_spawnedObjs[i]);
		//#else
		//			Destroy(_spawnedObjs[i]);
		//#endif
		//		}

		//_spawnedObjs.Clear();

		if (_spawnedObjHolder == null) _spawnedObjHolder = transform.Find("Holder").gameObject;

		if (_spawnedObjHolder != null)
		{
			while (_spawnedObjHolder.transform.childCount > 0)
			{
				DestroyImmediate(_spawnedObjHolder.transform.GetChild(0).gameObject);
			}
		}
	}

	private void RemoveAllFenceObjs()
	{
		if (_spawnedFenceHolder == null) _spawnedFenceHolder = transform.Find("FenceHolder").gameObject;

		if (_spawnedFenceHolder != null)
		{
			while (_spawnedFenceHolder.transform.childCount > 0)
			{
				DestroyImmediate(_spawnedFenceHolder.transform.GetChild(0).gameObject);
			}
		}
	}

	private void SpawnEdgeFence()
	{
		RemoveAllFenceObjs();

		if (_edgeFenceArchetype == null) return;
		_spawnedFenceHolder = transform.Find("FenceHolder").gameObject;
		if (_spawnedFenceHolder == null)
		{
			print("_spawnedFenceHolder == null");
			return;
		}

		//

		List<Vector3> verts = new List<Vector3>();
		//List<Vector2> verts2D = new List<Vector2>();

		int cc = transform.childCount;
		for (int i = 0; i < cc; ++i)
		{
			if (!transform.GetChild(i).name.Contains("vert")) continue;

			Vector3 v = transform.GetChild(i).gameObject.transform.position;
			Vector3 normal;
			v = BBBStatics.CheckForGroundV_V2(v, 50.0f, out normal);
			verts.Add(v);
			//verts2D.Add(new Vector2(v.x, v.z));

			///// Debug
			//if (i >= 1) Debug.DrawLine(vertsOrig[i - 1], vertsOrig[i], Color.red, Time.deltaTime);
			/////

		}

		float fenceLength = 5.2f;

		for (int i = 0; i < verts.Count; ++i)
		{
			Vector3 v1 = verts[i];
			Vector3 v2 = verts[0];
			if ((i + 1) < verts.Count) v2 = verts[i + 1];

			float distBetween = Vector3.Distance(v1, v2);
			float distBetween_curr = distBetween;

			///
			int type = 2;
			///

			if (type == 1)
			{
				//int fenceCountBtwnVerts = BBBStatics.TimesNumInNum(fenceLength, distBetween);

				Quaternion rot = Quaternion.LookRotation(v2 - v1);

				while (distBetween_curr > 0)
				{
					Vector3 spawnPos = Vector3.Lerp(v1, v2, BBBStatics.Map(distBetween_curr, 0.0f, distBetween, 0.0f, 1.0f, true));

					//Quaternion.FromToRotation(Vector3.up, normals[i])
					Instantiate(_edgeFenceArchetype, spawnPos, rot, _spawnedFenceHolder.transform);
					distBetween_curr -= fenceLength;
				}
			}

			if (type == 2)
			{
				GameObject lastFenceSpawned = null;
				while (distBetween_curr > 0)
				{
					float dist2D = fenceLength;

					Vector3 spawnPos = v1;
					Quaternion rot = Quaternion.LookRotation(v2 - v1);

					if (lastFenceSpawned != null) // 1st one
					{
						spawnPos = lastFenceSpawned.transform.Find("end").transform.position;

						Vector3 normal;
						Vector3 hitPnt = BBBStatics.CheckForGroundV_V2(spawnPos, 50.0f, out normal);

						spawnPos = hitPnt;

						dist2D = Vector2.Distance(new Vector2(lastFenceSpawned.transform.position.x, lastFenceSpawned.transform.position.z), new Vector2(hitPnt.x, hitPnt.z));

						rot = Quaternion.LookRotation(hitPnt - lastFenceSpawned.transform.position);
					}

					lastFenceSpawned = Instantiate(_edgeFenceArchetype, spawnPos, rot, _spawnedFenceHolder.transform);

					distBetween_curr -= dist2D; // dist2D // fenceLength
				}
			}
		}
	}

	//

	private void CullAtDist()
	{
		//#if UNITY_EDITOR
		//if (Camera.current == null) return;
		//if (UnityEditor.SceneView.currentDrawingSceneView == null) return;
		//if (UnityEditor.SceneView.currentDrawingSceneView.camera == null) return;

		//float dist = Vector3.Distance(Camera.current.transform.position, transform.position);

		Vector3 camLoc = Vector3.zero;

		if (Application.isEditor) // || !Application.isPlaying)
		{
			if (UnityEditor.SceneView.GetAllSceneCameras().Length == 0) return;
			else camLoc = UnityEditor.SceneView.GetAllSceneCameras()[0].transform.position;
		}

		if (camLoc == Vector3.zero && Application.isPlaying && _inGameCam != null)
		{
			camLoc = _inGameCam.transform.position;
		}

		if (camLoc == Vector3.zero) return;

		//

		float dist = Vector3.Distance(camLoc, transform.position);

		//print("dist: " + dist);

		//if (dist > 110)
		//{
		//	print("HideAll(true)");
		//	HideAll(true);
		//}

		if (!_bCulled)
		{
			if (dist > 510)
			{
				HideAll(false);
				_bCulled = true;
			}
		}
		else
		{
			if (dist < 500)
			{
				HideAll(true);
				_bCulled = false;
			}
		}
		//#endif
	}

	private void HideAll(bool bHide)
	{
		if (_spawnedObjHolder == null) _spawnedObjHolder = transform.Find("Holder").gameObject;
		if (_spawnedFenceHolder == null) _spawnedFenceHolder = transform.Find("FenceHolder").gameObject;

		int cc = _spawnedObjHolder.transform.childCount;
		for (int i = 0; i < cc; ++i)
		{
			Renderer rend = _spawnedObjHolder.transform.GetChild(i).GetComponent<Renderer>();
			if (rend != null)
			{
				rend.enabled = bHide;
			}
		}

		cc = _spawnedFenceHolder.transform.childCount;
		for (int i = 0; i < cc; ++i)
		{
			Renderer rend = _spawnedFenceHolder.transform.GetChild(i).GetComponent<Renderer>();
			if (rend != null)
			{
				rend.enabled = bHide;
			}
		}
	}

	//void OnGUI() // For Debug Labels
	//{
	//	//base.OnGUI(); // Call the super-class function

	//	var restoreColor = GUI.color; GUI.color = Color.green; // red

	//	UnityEditor.Handles.Label(transform.position, "_currFollowingRail_path: " + _currFollowingRail_path);
	//	UnityEditor.Handles.Label(transform.position + new Vector3(0, 3, 0), "_trainSideOn: " + _trainSideOn.ToString());

	//	GUI.color = restoreColor;
	//}
#endif
}
