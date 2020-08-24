using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum GameMethod
{
	One,
	Two
}

public enum GameType
{
	TrainChase,
	RTS
}

public enum MapBoundsEdges
{
	Top,
	Bottom,
	Left,
	Right,
	None
}


public class WorldScript : MonoBehaviour
{
	private HUDScript _HUDScript; public HUDScript HUDScript { get { return _HUDScript; } }
	public HUDScript HS { get { return _HUDScript; } }

	private HUDScript_RTS _HUDScript_RTS; public HUDScript_RTS HUDScript_RTS { get { return _HUDScript_RTS; } }
	public HUDScript_RTS HSR { get { return _HUDScript_RTS; } }
	private BeamEffectScript _BeamEffectScript; public BeamEffectScript BeamEffectScript { get { return _BeamEffectScript; } }
	private ExplosionScript _ExplosionScript; public ExplosionScript ExplosionScript { get { return _ExplosionScript; } }
	private RandomisationScript _RandomisationScript; public RandomisationScript RandomisationScript { get { return _RandomisationScript; } }
	private DayNightCycleScript _DayNightCycleScript; public DayNightCycleScript DayNightCycleScript { get { return _DayNightCycleScript; } }
	private GameplayScript _GameplayScript; public GameplayScript GameplayScript { get { return _GameplayScript; } }
	public GameplayScript GPS { get { return _GameplayScript; } }
	private MusicScript _MusicScript; public MusicScript MusicScript { get { return _MusicScript; } }
	public MusicScript MS { get { return _MusicScript; } }
	private CommandScript _CommandScript; public CommandScript CommandScript { get { return _CommandScript; } }
	public CommandScript CS { get { return _CommandScript; } }
	private StatsRecording _statsRec; public StatsRecording StatsRec { get { return _statsRec; } }

	private ConstructionManagerScript _constructionManager;

	private PlayerControllerScript _playerController;
	private RTSCameraController _RTSCameraController; public RTSCameraController RTSCameraController { get { return _RTSCameraController; } }

	private LocomotiveScript _locomotiveScript;
	public LocomotiveScript getLocomotiveScript { get { return _locomotiveScript; } }

    //

    private List<RailScript> allRails = new List<RailScript>(); public List<RailScript> AllRails { get { return allRails; } }
    private float _railwayWorldHeight; public float RailwayWorldHeight { get { return _railwayWorldHeight; } }

	private List<CarriageScript> _allCarriages = new List<CarriageScript>();
	public List<CarriageScript> AllCarriages { get { return _allCarriages; } }

	private GameDataScript _currentGameData; public GameDataScript CurrentGameData { get { return _currentGameData; } }

    private GameObject _resourcesBarRef; public GameObject ResourceBarRef { get { return _resourcesBarRef; } set { _resourcesBarRef = value; } }

    private GameObject _locomotiveObjectRef; public GameObject LocomotiveObjectRef { get { return _locomotiveObjectRef; } set { _locomotiveObjectRef = value; } }
	private Vector3 _locomotiveInitialPos; public Vector3 LocomotiveInitialPos { get { return _locomotiveInitialPos; } }

	private List<TrainGameObjScript> _allTGOsInWorld = new List<TrainGameObjScript>();
	private List<TrainGameObjScript> _enemies = new List<TrainGameObjScript>();
	public List<TrainGameObjScript> Enemies { get { return _enemies; } }

	// Editable in editor vars

	public List<GameObject> _constructionArchetypes = new List<GameObject>(); public List<GameObject> ConstructionArchetypes { get { return _constructionArchetypes; } }
	private List<GameObject> _constructionArchetypes_BBB = new List<GameObject>(); public List<GameObject> ConstructionArchetypes_BBB { get { return _constructionArchetypes_BBB; } }

	public List<GameObject> _carriagesArchetypes = new List<GameObject>(); public List<GameObject> CarriageArchetypes { get { return _carriagesArchetypes; } }

	private GameObject _defaultChunkArchetype = null; public GameObject DefaultChunkArchetype { get { return _defaultChunkArchetype; } }
	public List<Mesh> _defaultChunkMeshes = new List<Mesh>();
	public List<Material> _defaultChunkMaterials = new List<Material>();
	private List<GameObject> _allChunks = new List<GameObject>(); public List<GameObject> AllChunks { get { return _allChunks; } }
	private int _maxChunksInWorld = 100;

    private List<GameObject> _allResourceDrones = new List<GameObject>(); public List<GameObject> AllResourceDrones {  get { return _allResourceDrones; } }
    private List<GameObject> _allResourceSilos = new List<GameObject>(); public List<GameObject> AllResourceSilos {  get { return _allResourceSilos; } }

	[SerializeField]
	private List<GameObject> _allEnemyArchetypes = new List<GameObject>();
	public List<GameObject> AllEnemyArchetypes { get { return _allEnemyArchetypes; } }

	public bool bEnemySpawningActive = true;
	public bool _bUsingRailBasedAIPathing = true;
	public bool _bShowCarriageOffsetDebugging = false;

	// Map vars

	private Vector3 _v3_MapTopLeft; public Vector3 V3_MapTopLeft { get { return _v3_MapTopLeft; } }
	private Vector3 _v3_MapBottomRight; public Vector3 V3_MapBottomRight { get { return _v3_MapBottomRight; } }
	private Vector3 _v3_MapTopRight; public Vector3 V3_MapTopRight { get { return _v3_MapTopRight; } }
	private Vector3 _v3_MapBottomLeft; public Vector3 V3_MapBottomLeft { get { return _v3_MapBottomLeft; } }

	private Vector3 _v3_MapCentre; public Vector3 V3_MapCentre { get { return _v3_MapCentre; } }

	//

	private GameObject _locomotiveConsPlatform; public GameObject LocomotiveConsPlatform { get { return _locomotiveConsPlatform; } set { _locomotiveConsPlatform = value; } }

	// Resources

	private AudioSource _2DMainAudioSource; public AudioSource AS_2DMainAudioSource { get { return _2DMainAudioSource; } }
	private AudioClip ws_beep1; public AudioClip WS_beep1 { get { return ws_beep1; } }
	private AudioClip ws_beep2; public AudioClip WS_beep2 { get { return ws_beep2; } }
	private AudioClip ws_beep3; public AudioClip WS_beep3 { get { return ws_beep3; } }
	private AudioClip ws_beep4; public AudioClip WS_beep4 { get { return ws_beep4; } }
	private AudioClip ws_beep5; public AudioClip WS_beep5 { get { return ws_beep5; } }
	private AudioClip ws_beep6; public AudioClip WS_beep6 { get { return ws_beep6; } }
	private AudioClip ws_beep7; public AudioClip WS_beep7 { get { return ws_beep7; } }
	private AudioClip ws_beep8; public AudioClip WS_beep8 { get { return ws_beep8; } }
	private AudioClip ws_beep9; public AudioClip WS_beep9 { get { return ws_beep9; } }
	private AudioClip ws_beep10; public AudioClip WS_beep10 { get { return ws_beep10; } }

	// Debugging

	//private GameObject _currentConstructionPlatform;

	private bool bShowTrainRailDebugging = false;

	//

	private GameObject _winConditionRail; public GameObject WinConditionRail { get { return _winConditionRail; } }

	private float _mapCompletionPcnt = 0.0f;
	public float MapCompletionPcnt { get { return _mapCompletionPcnt; } set { _mapCompletionPcnt = value; } }

	//

	private GameObject _damageSmokeLvl1 = null; public GameObject DamageSmokeLvl1 { get { return _damageSmokeLvl1; } }
	private GameObject _damageSmokeLvl2 = null; public GameObject DamageSmokeLvl2 { get { return _damageSmokeLvl2; } }
	private GameObject _damageSmokeLvl3 = null; public GameObject DamageSmokeLvl3 { get { return _damageSmokeLvl3; } }

    private bool _trainIsStopped;

    //
    private EventTutorial _currentEventTutorial;
    private EventDialogue _currentDialogueEvent = null;
    public void OverrideDialogueEvent(EventDialogue dialogueEvent)
    {
        if (_currentDialogueEvent != dialogueEvent)
        {
            if (_currentDialogueEvent != null)
            {
                _currentDialogueEvent.CloseDialogueEvent();
            }
            _currentDialogueEvent = dialogueEvent;
        }
    }

	void Awake()
	{
		_HUDScript = gameObject.GetComponent<HUDScript>();
		_HUDScript.WorldScript = this;

		//_HUDScript_RTS = gameObject.AddComponent<HUDScript_RTS>();
		//_HUDScript_RTS.WorldScript = this;

		_BeamEffectScript = gameObject.AddComponent<BeamEffectScript>();
		_BeamEffectScript.WorldScript = this;

		_ExplosionScript = gameObject.AddComponent<ExplosionScript>();
		_ExplosionScript.WorldScript = this;

		_RandomisationScript = gameObject.AddComponent<RandomisationScript>();
		_RandomisationScript.WorldScript = this;

		_DayNightCycleScript = gameObject.AddComponent<DayNightCycleScript>();
		_DayNightCycleScript.WorldScript = this;
        
        _GameplayScript = gameObject.AddComponent<GameplayScript>();
		_GameplayScript.WorldScript = this;

		_constructionManager = gameObject.GetComponent<ConstructionManagerScript>();

		_playerController = gameObject.GetComponent<PlayerControllerScript>();

		_MusicScript = gameObject.AddComponent<MusicScript>();
		_MusicScript.WorldScript = this;

		_CommandScript = gameObject.AddComponent<CommandScript>();
		_CommandScript.WorldScript = this;

		_statsRec = gameObject.AddComponent<StatsRecording>();
		_statsRec.WorldScript = this;
        _RTSCameraController = Camera.main.GetComponent<RTSCameraController>();
    }

	void Start()
	{
        _trainIsStopped = false;
		_currentGameData = gameObject.AddComponent(typeof(GameDataScript)) as GameDataScript;

		//_RTSCameraController = Camera.main.GetComponent<RTSCameraController>();

		_2DMainAudioSource = GameObject.Find("MainHolder").transform.Find("2DMainAudioSource1").GetComponent<AudioSource>();
		if (_2DMainAudioSource == null) print("Error: _2DMainAudioSource == null -- WorldScript @ Start()");

		Camera[] AllCams = new Camera[Camera.allCamerasCount];
		Camera.GetAllCameras(AllCams);

		//

		LocomotiveObjectRef = GameObject.Find("LocomotiveB");
		if (LocomotiveObjectRef == null) print("There is no locomotive in the world");
		else _locomotiveInitialPos = LocomotiveObjectRef.transform.position;

		try
		{
			_locomotiveScript = LocomotiveObjectRef.GetComponent<LocomotiveScript>();
		}
		catch (NullReferenceException e)
		{
			throw (e);
		}

        //


         ResourceBarRef = GameObject.Find("ResourcesMenuObj");
        if (ResourceBarRef == null) print("There is no Resource bar in the world");

        //

        ConsPlatformScript[] allCPs = GetAllConsPlatforms();
		for (int i = 0; i < allCPs.Length; ++i)
		{
			if (allCPs[i]._bOnlyAllowBuildMobile) // There should only ever be one cons platform with this set to true in the world
			{
				LocomotiveConsPlatform = allCPs[i].gameObject;
				break;
			}
		}
		if (LocomotiveObjectRef != null && LocomotiveConsPlatform == null) print("Error: _locomotiveConsPlatform == null -- WorldScript @ Start()");

		// Resources

		_defaultChunkArchetype = Resources.Load("DefaultChunk") as GameObject;

		_defaultChunkMeshes.Add((Resources.Load("Mesh/DefaultChunk001") as GameObject).GetComponent<MeshFilter>().sharedMesh);
		_defaultChunkMeshes.Add((Resources.Load("Mesh/DefaultChunk002") as GameObject).GetComponent<MeshFilter>().sharedMesh);
		_defaultChunkMeshes.Add((Resources.Load("Mesh/DefaultChunk003") as GameObject).GetComponent<MeshFilter>().sharedMesh);
		_defaultChunkMeshes.Add((Resources.Load("Mesh/DefaultChunk004") as GameObject).GetComponent<MeshFilter>().sharedMesh);
		_defaultChunkMeshes.Add((Resources.Load("Mesh/DefaultChunk005") as GameObject).GetComponent<MeshFilter>().sharedMesh);
		_defaultChunkMaterials.Add((Resources.Load("Materials/chunkMat1") as Material));

		ws_beep1 = Resources.Load("Sounds/beep1") as AudioClip;
		ws_beep2 = Resources.Load("Sounds/beep2") as AudioClip;
		ws_beep3 = Resources.Load("Sounds/beep3") as AudioClip;
		ws_beep4 = Resources.Load("Sounds/beep4") as AudioClip;
		ws_beep5 = Resources.Load("Sounds/beep5") as AudioClip;
		ws_beep6 = Resources.Load("Sounds/beep6") as AudioClip;
		ws_beep7 = Resources.Load("Sounds/beep7") as AudioClip;
		ws_beep8 = Resources.Load("Sounds/beep8") as AudioClip;
		ws_beep9 = Resources.Load("Sounds/beep9") as AudioClip;
		ws_beep10 = Resources.Load("Sounds/beep10") as AudioClip;

		// Map vectors

		_v3_MapTopLeft = GameObject.Find("MainHolder").transform.Find("MapTopLeft").transform.position;
		_v3_MapTopLeft.y = 0;
		_v3_MapBottomRight = GameObject.Find("MainHolder").transform.Find("MapBottomRight").transform.position;
		_v3_MapBottomRight.y = 0;

		// x = top or bottom // z = left or right
		_v3_MapTopRight = new Vector3(_v3_MapTopLeft.x, 0.0f, _v3_MapBottomRight.z);
		_v3_MapBottomLeft = new Vector3(_v3_MapBottomRight.x, 0.0f, _v3_MapTopLeft.z);

		_v3_MapCentre = BBBStatics.BetweenAt(_v3_MapTopLeft, _v3_MapBottomRight, 0.5f);

		//

		allRails = new List<RailScript>((RailScript[])FindObjectsOfType(typeof(RailScript)));
		if (allRails.Count > 0)
		{
			_railwayWorldHeight = allRails[0].transform.position.y;
		}

		for (int i = 0; i < allRails.Count; ++i)
		{
			if (allRails[i]._bIsWinConditionRail)
			{
				_winConditionRail = allRails[i].gameObject;
				//print("Found _winConditionRail");
				break;
			}
		}
        
		_damageSmokeLvl1 = Resources.Load("BBB_S2/Prefabs/SmokeFX1") as GameObject;
		_damageSmokeLvl2 = Resources.Load("BBB_S2/Prefabs/SmokeFX2") as GameObject;
		_damageSmokeLvl3 = Resources.Load("BBB_S2/Prefabs/SmokeFX2") as GameObject;

        

		// End of Start()
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;


		ManageChunks();
        AssignDroneToChunk();        
    }

	private void FixedUpdate()
	{
		FrameSyncing();
	}

	private void FrameSyncing()
	{
		_locomotiveScript.ManageSpeed();
		//_locomotiveScript.MaintainCarriageSeparationDist();

		if (_locomotiveScript.GetCarriagesInOrder().Count > 0)
		{
			List<CarriageScript> carriagesInOrder = _locomotiveScript.GetCarriagesInOrder();


            int count = carriagesInOrder.Count;
            for (int i = 0; i < count; i++)
            {
                CarriageScript c = carriagesInOrder[i];
                if (c.CarriageInFront == null) continue;

                c.CurrentMoveSpeed_UberMult = _locomotiveScript.CurrentMoveSpeed_UberMult;

                //c.MaintainCarriageSeparationDist();
                //c.MatchSpeedWithCarriageInFront();
                c.MaintainCarriageSeparationDist_T2();

                c.moveCarriage();
            }

			//foreach (CarriageScript c in carriagesInOrder) // AllCarriages
			//{
			//	if (c.CarriageInFront == null) continue;

			//	c.moveCarriage();
			//}
		}

		_locomotiveScript.moveCarriage();

		//

		_RTSCameraController.UpdateActual();
	}

	//
	/// <summary>
	/// Automatically enerate archetypes list for the construction platform (obsolete)
	/// Reactivated 30-5-18 [Michael]
	/// </summary>
	/// <param name="cps"></param>
	public void GenerateArcheList(ConsPlatformScript cps)
	{
		List<GameObject> constructionArchetypesToUse = _constructionArchetypes;

		for (int i = 0; i < constructionArchetypesToUse.Count; ++i)
		{
			if (constructionArchetypesToUse[i] == null) continue;
			if (cps._bOnlyAllowBuildFixtures && constructionArchetypesToUse[i].GetComponent<AIDynamicObjScript>() != null) continue;
			if (cps._bOnlyAllowBuildMobile && constructionArchetypesToUse[i].GetComponent<AIDynamicObjScript>() == null) continue;

			cps._consArchetypes.Add(constructionArchetypesToUse[i]);
		}
	}

	public List<TrainGameObjScript> GetObjsOnScreen(List<TrainGameObjScript> inList, float edgeLeeway, bool bOffScreenInstead)
	{
		List<TrainGameObjScript> resultOnScreen = new List<TrainGameObjScript>();
		List<TrainGameObjScript> resultOffScreen = new List<TrainGameObjScript>();

		for (int i = 0; i < inList.Count; ++i)
		{
			if (inList[i].CommSocketObj != null && !inList[i].bIsChunk)
			{
				Vector3 screenPos = Camera.main.WorldToScreenPoint(inList[i].CommSocketObj.transform.position);
				Vector2 sp2D = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));

				if (Mathf.Abs(sp2D.x) < ((Screen.width / 2) - edgeLeeway) && Mathf.Abs(sp2D.y) < ((Screen.height / 2) - edgeLeeway))
				{
					resultOnScreen.Add(inList[i]);
				}
				else if (bOffScreenInstead)
				{
					resultOffScreen.Add(inList[i]);
				}
			}
		}

		if (bOffScreenInstead)
		{
			return resultOffScreen;
		}

		return resultOnScreen;
	}

	public List<TrainGameObjScript> GetObjsInScreenArea(List<TrainGameObjScript> inList, Vector2 areaTopLeft, Vector2 areaBtmRight, bool bOutOfAreaInstead)
	{
		List<TrainGameObjScript> resultInArea = new List<TrainGameObjScript>();
		List<TrainGameObjScript> resultOutOfArea = new List<TrainGameObjScript>();

		for (int i = 0; i < inList.Count; ++i)
		{
			if (inList[i].CommSocketObj != null && !inList[i].bIsChunk)
			{
				Vector3 screenPos = Camera.main.WorldToScreenPoint(inList[i].CommSocketObj.transform.position);
				//Vector2 sp2D = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));
				Vector2 sp2D = new Vector2(screenPos.x, screenPos.y);

				if (sp2D.x > areaTopLeft.x && sp2D.x < areaBtmRight.x && sp2D.y < areaTopLeft.y && sp2D.y > areaBtmRight.y)
				{
					resultInArea.Add(inList[i]);
				}
				else if (bOutOfAreaInstead)
				{
					resultOutOfArea.Add(inList[i]);
				}
			}
		}

		if (bOutOfAreaInstead)
		{
			return resultOutOfArea;
		}

		return resultInArea;
	}

	//

	public void AddToAllTGOsInWorld(TrainGameObjScript tgo)
	{
		if (!_allTGOsInWorld.Contains(tgo))
		{
			_allTGOsInWorld.Add(tgo);
		}
	}
	public void AddToAllTGOsInWorld(GameObject go) // Overloaded
	{
		TrainGameObjScript tgo = BBBStatics.TGO(go);
		if (tgo != null && !_allTGOsInWorld.Contains(tgo))
		{
			_allTGOsInWorld.Add(tgo);
		}
	}

	public void RemoveFromAllTGOsInWorld(TrainGameObjScript tgo)
	{
		if (tgo != null && _allTGOsInWorld.Contains(tgo))
		{
			_allTGOsInWorld.Remove(tgo);
		}
	}
	public void RemoveFromAllTGOsInWorld(GameObject go) // Overloaded
	{
		if (go == null) return;

		TrainGameObjScript tgo = BBBStatics.TGO(go);
		if (tgo != null && _allTGOsInWorld.Contains(tgo))
		{
			_allTGOsInWorld.Remove(tgo);
		}
	}

	public List<TrainGameObjScript> GetAllTGOsInWorld() { return _allTGOsInWorld; }

	public List<GameObject> GetAllTGOsInWorld_AsGOs()
	{
		List<GameObject> tempList = new List<GameObject>();
		for (int i = 0; i < _allTGOsInWorld.Count; ++i)
		{
			tempList.Add(_allTGOsInWorld[i].gameObject);
		}

		return tempList;
	}

	//

	/// Destroy as many chunks as we need to in order to stay below the _maxChunksInWorld count
	/// It doesn't matter if they're on-screen as they will actually explode
	/// Destroy them in order of farthest away from the locomotive
	/// Only check/destroy them if they're more than 5 seconds old -- half of new chunks may automatically crash and be destroyed before then
	void ManageChunks()
	{
		if (_allChunks.Count > _maxChunksInWorld)
		{
			float minChunkAge = 5.0f;

			List<GameObject> keeperChunks = new List<GameObject>();
			for (int i = 0; i < _allChunks.Count; ++i)
			{
				if (BBBStatics.TGO(_allChunks[i]).TimeSinceInstantiated > minChunkAge) // Old enough
				{
					if (_allChunks[i].GetComponent<ChunkScript>()._bCanBeCulledByWorldScript) // Can be culled at all
					{
						float distFromLoco = 0.0f;
						if (LocomotiveObjectRef != null) distFromLoco = Vector3.Distance(BBBStatics.Pos(_allChunks[i]), BBBStatics.Pos(LocomotiveObjectRef));

						if (distFromLoco > _allChunks[i].GetComponent<ChunkScript>()._minCullDistFromLoco) // Far enough away from the locomotive
						{
							keeperChunks.Add(_allChunks[i]);
						}
					}
				}
			}

			GameObject farthestChunk = BBBStatics.GetFarthestGOFromListToVec(keeperChunks, BBBStatics.Pos(LocomotiveObjectRef));
			if (farthestChunk != null)
			{
				TrainGameObjScript tgo = BBBStatics.TGO(farthestChunk);
				if (tgo != null)
				{
					bool bExplode = tgo.GetComponent<ChunkScript>()._bCanExplode;

					tgo.BeginDestroy(bExplode, false);
				}
			}
		}
	}

    void AssignDroneToChunk()
    {        
        if (_allChunks.Count > 0)
        {            
            float searchRange = _allResourceDrones[0].GetComponent<AIScavDroneScript>()._maxResourceSearchRange;

            int dronesCount = _allResourceDrones.Count;
            for (int i = 0; i < dronesCount; i++)
            {
                GameObject resourceDrone = _allResourceDrones[i];
                GameObject nearestChunk = BBBStatics.GetClosestGOFromListToVec(_allChunks, resourceDrone.transform.position);
                if (nearestChunk != null)
                {
                    if (BBBStatics.GetDistance2D(nearestChunk.transform.position, resourceDrone.transform.position) < searchRange)
                    {
                        resourceDrone.GetComponent<AIScavDroneScript>().ChunkNearby = true;
                    }
                    else
                    {
                        resourceDrone.GetComponent<AIScavDroneScript>().ChunkNearby = false;
                    }
                }
                else
                {
                    resourceDrone.GetComponent<AIScavDroneScript>().ChunkNearby = false;
                }
            }         
        }
    }

	public bool CheckIfVecWithinMapBounds(Vector3 vec, out MapBoundsEdges outBounds1, out MapBoundsEdges outBounds2, float padding)
	{
		bool bReturn = true;

		vec.y = 0;

		outBounds1 = MapBoundsEdges.None;
		outBounds2 = MapBoundsEdges.None;

		// Beyond the top
		if (vec.x > (_v3_MapTopLeft.x - padding))
		{
			if (outBounds1 == MapBoundsEdges.None) outBounds1 = MapBoundsEdges.Top;
			else outBounds2 = MapBoundsEdges.Top;
			bReturn = false;
		}

		if (vec.x < (_v3_MapBottomLeft.x + padding)) // Beyond the bottom
		{
			if (outBounds1 == MapBoundsEdges.None) outBounds1 = MapBoundsEdges.Bottom;
			else outBounds2 = MapBoundsEdges.Bottom;
			bReturn = false;
		}

		if (vec.z > (_v3_MapTopLeft.z - padding)) // Beyond the left
		{
			if (outBounds1 == MapBoundsEdges.None) outBounds1 = MapBoundsEdges.Left;
			else outBounds2 = MapBoundsEdges.Left;
			bReturn = false;
		}

		if (vec.z < (_v3_MapTopRight.z + padding)) // Beyond the right
		{
			if (outBounds1 == MapBoundsEdges.None) outBounds1 = MapBoundsEdges.Right;
			else outBounds2 = MapBoundsEdges.Right;
			bReturn = false;
		}

		return bReturn;
	}

	public ConsPlatformScript[] GetAllConsPlatforms()
	{
		return (ConsPlatformScript[])FindObjectsOfType(typeof(ConsPlatformScript));
	}

	//

	// Debugging

	public bool ShowTrainRailDebugging
	{
		get { return bShowTrainRailDebugging; }
		set { bShowTrainRailDebugging = value; }
	}

	private List<Vector3> DrawGizmoAtPos = new List<Vector3>();

	void OnDrawGizmos()
	{
		for (int i = 0; i < DrawGizmoAtPos.Count; ++i)
		{
			Gizmos.DrawIcon(DrawGizmoAtPos[i], "Light Gizmo.tiff", false);
		}
		if (DrawGizmoAtPos.Count > 0) DrawGizmoAtPos.Clear();
	}

	public void AddGizmo(Vector3 v)
	{
		DrawGizmoAtPos.Add(v);
	}

	public ConstructionManagerScript ConstructionManager
	{
		get
		{
			return _constructionManager;
		}
	}

	public List<TurretScriptParent> GetAllFriendlyTurretsOnTrain()
	{
		List<TurretScriptParent> result = new List<TurretScriptParent>();

		for (int i = 0; i < _allTGOsInWorld.Count; ++i)
		{
			if (_allTGOsInWorld[i] == null) continue;

			TurretScriptParent tsp = _allTGOsInWorld[i].GetComponent<TurretScriptParent>();
			if (tsp != null && tsp._team == Team.Friendly && tsp.ConnectedParentObj != null)
			{
				result.Add(tsp);
			}
		}

		return result;
	}

	public void CloseConsMenu_WS()
	{
		if (_HUDScript != null) _HUDScript.BPendingCloseConsMenu = true; // _HUDScript.CloseConsMenu();
		else print("_HUDScript == null @ WS - CloseConsMenu()");
	}

    public bool TrainStopped
    {
        get { return _trainIsStopped; }
    }

    public void SetTrainStopped(bool value)
    {
        _trainIsStopped = value;
    }

    /// <summary>
    ///  The currently running tutorial event
    /// </summary>
    public EventTutorial CurrentEventTutorial
    {
        get
        {
            return _currentEventTutorial;
        }
        set
        {
            _currentEventTutorial = value;
        }
    }
}

