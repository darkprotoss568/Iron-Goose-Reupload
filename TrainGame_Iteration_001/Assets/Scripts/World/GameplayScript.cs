using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayScript : MonoBehaviour
{
    private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } }
    public WorldScript WS { get { return _worldScript; } }

    private bool _bIsTrainStoppedAtStation = false;
    private bool _bIsTrainStoppedAtStation_last = false;

    private List<GameObject> _playerSelectedObjects; public List<GameObject> PlayerSelectedObjects { get { return _playerSelectedObjects; } }

    private bool _bCanPlayerSelectObjs;
    private bool _bCanPlayerTargetObjs;

    private int _playerResources;
    private int _playerMaxResources;

    private List<GameObject> _currEnemyArchetypes = new List<GameObject>();

    private List<GameObject> _enemiesInWorld = new List<GameObject>();
    public List<GameObject> EnemiesInWorld { get { return _enemiesInWorld; } }

    private int _maxEnemiesInWorld = 0;

    private List<Vector3> _randOffsetsFromLoco; public List<Vector3> RandOffsetsFromLoco { get { return _randOffsetsFromLoco; } }
    private List<Vector3> _randOffsetsFromLoco_localOffsets;

    private List<Vector3> _randOffsetsFromLoco_right;
    private List<Vector3> _randOffsetsFromLoco_left;
    private List<int> _randOffsetsFromLoco_right_occupied = new List<int>();
    private List<int> _randOffsetsFromLoco_left_occupied = new List<int>();
    private List<Vector3> _randOffsetsFromLoco_local_right;
    private List<Vector3> _randOffsetsFromLoco_local_left;

    public List<Vector3> RandOffsetsFromLoco_Right { get { return _randOffsetsFromLoco_right; } }
    public List<Vector3> RandOffsetsFromLoco_Left { get { return _randOffsetsFromLoco_left; } }
    public List<int> RandOffsetsFromLoco_Right_Occupied { get { return _randOffsetsFromLoco_right_occupied; } }
    public List<int> RandOffsetsFromLoco_Left_Occupied { get { return _randOffsetsFromLoco_left_occupied; } }

    private List<ConsPlatformScript> _selectableConsPlatforms = new List<ConsPlatformScript>(); public List<ConsPlatformScript> SelectableConsPlatforms { get { return _selectableConsPlatforms; } set { _selectableConsPlatforms = value; } }

    private bool _bSelectionActive; public bool BSelectionActive { get { return _bSelectionActive; } set { _bSelectionActive = value; } }
    private bool _bPendingSetSelectionActive; public bool BPendingSetSelectionActive { get { return _bPendingSetSelectionActive; } set { _bPendingSetSelectionActive = value; } }

    private bool _bTutorialRunning; public bool BTutorialRunning { get { return _bTutorialRunning; } set { _bTutorialRunning = value; } }
    private bool _bIntenseMusic; public bool _BIntenseMusic { get { return _bIntenseMusic; } set { _bIntenseMusic = value; } }

    private float _freebieResourcesDelay = 10.0f;
    private float _freebieResourcesDelay_curr = 0.0f;

    //

    private bool _bAreaSelectionActive = false; public bool BAreaSelectionActive { get { return _bAreaSelectionActive; } } // set { _bAreaSelectionActive = value; } }
    private Vector2 _areaSelectionInitialPos = Vector2.zero; public Vector2 AreaSelectionInitialPos { get { return _areaSelectionInitialPos; } } // set { _areaSelectionInitialPos = value; } }

    //private bool _bPostStartRun = false;

    private bool _bMouseLeftDown = false; public bool BMouseLeftDown { get { return _bMouseLeftDown; } set { _bMouseLeftDown = value; } }
    private bool _bMouseRightDown = false; public bool BMouseRightDown { get { return _bMouseRightDown; } set { _bMouseRightDown = value; } }
    private bool _bMouseMiddleDown = false; public bool BMouseMiddleDown { get { return _bMouseMiddleDown; } set { _bMouseMiddleDown = value; } }

    //
    private bool _bRandomOffsetsCleared = true;

    
    [SerializeField]
    private float _timeScaleTransitionTime;
    private float _currTransitionTime;
    private float _targetTimeScale;

    private void Awake()
    {
        _timeScaleTransitionTime = 0.0f;
        _currTransitionTime = 0.0f;
        _targetTimeScale = 1;
        Time.timeScale = _targetTimeScale;
    }
    void Start()
    {
        //_bSelectionActive = false;
        _bTutorialRunning = false;

        _currEnemyArchetypes = WorldScript.AllEnemyArchetypes;

        _playerResources = 0;
        _playerMaxResources = 100;

        _bCanPlayerSelectObjs = true;
        _bCanPlayerTargetObjs = true;

        _playerSelectedObjects = new List<GameObject>();

        _randOffsetsFromLoco = new List<Vector3>();
        _randOffsetsFromLoco_localOffsets = new List<Vector3>();

        _randOffsetsFromLoco_right = new List<Vector3>();
        _randOffsetsFromLoco_left = new List<Vector3>();
        _randOffsetsFromLoco_local_right = new List<Vector3>();
        _randOffsetsFromLoco_local_left = new List<Vector3>();

        SetInitialRandomOffsets();

        SetAllElementsActive(true);

        
    }

    void Update()
    {
        if (PauseMenu.isPaused) return;

        CheckMouseClicks();

        //CheckSelectedObjectsAreValid();
        if (_bSelectionActive) ManageSelection();

        if (!_bRandomOffsetsCleared)
        {
            ManageRandomOffsets();
        }

        if (WorldScript.bEnemySpawningActive) ManageEnemies();

        CheckForPullingOutOfStation();

        //

        if (_bPendingSetSelectionActive)
        {
            if (!BMouseLeftDown) // Don't reactivate selection until the left mouse button has been released
            {
                _bSelectionActive = true;
                _bPendingSetSelectionActive = false;
            }
        }

        if (Input.GetMouseButtonUp(1)) // [Mike, 31-5-18]
        {
            // Right click will deselect everything
            if (_playerSelectedObjects.Count > 0)
            {
                DeselectAll();
                WorldScript.AS_2DMainAudioSource.PlayOneShot(WorldScript.WS_beep8, 0.75f);
            }

            // Even though DeselectAll() checks this, we want to make absolutely sure that if it is open it will always close with a right click
            if (_worldScript.ConstructionManager.bIsConsMenuOpen)
            {
                _worldScript.ConstructionManager.GetOpenedConstructionMenu().CloseConsMenu();
            }

            if (WS.CS.BuildObjArchetype != null)
            {
                WS.CS.BuildObjArchetype = null;
            }
        }

        //
        if (_timeScaleTransitionTime > 0)
                AdjustTimeScale();
        
        // 
        _bIsTrainStoppedAtStation_last = _bIsTrainStoppedAtStation;
    }

    void CheckForPullingOutOfStation()
    {
        if (!_worldScript.GameplayScript.bIsTrainStoppedAtStation && _worldScript.GameplayScript.bIsTrainStoppedAtStation_last)
        {
            _worldScript.MusicScript.BTrainPullingOutOfStation = true;
        }
    }

    /// <summary>
    /// Set the local positions that will be offset from the locomotive and will follow it via ManageRandomOffsets()
    /// Called only at start
    /// </summary>
    public void SetInitialRandomOffsets()
    {
        if (WorldScript.LocomotiveObjectRef != null)
        {

            ClearRandomOffsets();

            float distFromTrain = 12.0f;
            float mr_x = 28.0f; // Away from the side of he train

            float mr_length;
            if (WorldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetFullTrainLength() > WorldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetCarriageLength())
            {
                //Debug.Log("Train Length" + WorldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetFullTrainLength());
                mr_length = WorldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetFullTrainLength(); // Down the length of the train
            }
            else
            {
                mr_length = 80.0f;
            }
            for (int i = 0; i < 50; ++i) // Anywhere (for drones)
            {
                float mr = 40.0f;
                Vector3 v = new Vector3(BBBStatics.RandFlt(-mr, mr), BBBStatics.RandFlt(-mr, mr), 0.0f);
                _randOffsetsFromLoco_localOffsets.Add(v);
                _randOffsetsFromLoco.Add(Vector3.zero);
            }

            for (int i = 0; i < 25; ++i) // Right of the train (for ground AI units)
            {
                Vector3 v = new Vector3(BBBStatics.RandFlt(distFromTrain, mr_x), BBBStatics.RandFlt(-5.0f, mr_length), 0.0f);
                _randOffsetsFromLoco_local_right.Add(v);
                _randOffsetsFromLoco_right.Add(Vector3.zero);
            }

            for (int i = 0; i < 25; ++i) // Left of the train  (for ground AI units)
            {
                Vector3 v = new Vector3(BBBStatics.RandFlt(-mr_x, -distFromTrain), BBBStatics.RandFlt(-5.0f, mr_length), 0.0f);
                _randOffsetsFromLoco_local_left.Add(v);
                _randOffsetsFromLoco_left.Add(Vector3.zero);
            }

            _bRandomOffsetsCleared = false;
        }
    }

    private void ClearRandomOffsets()
    {
        _bRandomOffsetsCleared = true;

        _randOffsetsFromLoco.Clear();
        _randOffsetsFromLoco_localOffsets.Clear();
        _randOffsetsFromLoco_right.Clear();
        _randOffsetsFromLoco_local_right.Clear();
        _randOffsetsFromLoco_local_left.Clear();
        _randOffsetsFromLoco_left.Clear();
    }
    /// <summary>
    /// Maintain the local positions of random offsets from the locomotive as it moves through the world
    /// </summary>
    void ManageRandomOffsets()
    {
        if (WorldScript.LocomotiveObjectRef == null) return;


        for (int i = 0; i < _randOffsetsFromLoco_localOffsets.Count; ++i)
        {
            _randOffsetsFromLoco[i] = WorldScript.LocomotiveObjectRef.transform.position + (WorldScript.LocomotiveObjectRef.transform.rotation * _randOffsetsFromLoco_localOffsets[i]);

            //if (_randOffsetsFromLoco_LocalOffsets[i].x > 0.0f)
            //	Debug.DrawLine(_randOffsetsFromLoco[i], _randOffsetsFromLoco[i] + new Vector3(0, 500, 0), Color.yellow, Time.deltaTime); /// Right of train
            //else
            //	Debug.DrawLine(_randOffsetsFromLoco[i], _randOffsetsFromLoco[i] + new Vector3(0, 500, 0), Color.cyan, Time.deltaTime); /// Left of train
        }

        if (WorldScript._bShowCarriageOffsetDebugging)
        {
            for (int i = 0; i < _randOffsetsFromLoco_local_right.Count; ++i)
            {
                _randOffsetsFromLoco_right[i] = WorldScript.LocomotiveObjectRef.transform.position + (WorldScript.LocomotiveObjectRef.transform.rotation * _randOffsetsFromLoco_local_right[i]);
                Debug.DrawLine(_randOffsetsFromLoco_right[i], _randOffsetsFromLoco_right[i] + new Vector3(0, 500, 0), Color.yellow, Time.deltaTime);
            }

            for (int i = 0; i < _randOffsetsFromLoco_local_left.Count; ++i)
            {
                _randOffsetsFromLoco_left[i] = WorldScript.LocomotiveObjectRef.transform.position + (WorldScript.LocomotiveObjectRef.transform.rotation * _randOffsetsFromLoco_local_left[i]);
                Debug.DrawLine(_randOffsetsFromLoco_left[i], _randOffsetsFromLoco_left[i] + new Vector3(0, 500, 0), Color.cyan, Time.deltaTime);
            }
        }
    }

    private void ManageSelection()
    {
        if (WS.CS.BuildObjArchetype != null) return;

        if (_bCanPlayerSelectObjs && WorldScript.HUDScript.MouseOverObj != null && _playerSelectedObjects.Count == 0 && !_bAreaSelectionActive)
        {
            // Single unit selection
            TrainGameObjScript tgo = BBBStatics.TGO(WorldScript.HUDScript.MouseOverObj);
            if (tgo != null && tgo._team == Team.Friendly)
            {
                if (Input.GetMouseButtonUp(0)) // Input.GetMouseButtonDown(0)
                {
                    DeselectAll(); // Deselect everything else that may already be selected first [Mike, 31-5-18]
                    tgo.Select();

                    CheckIfConsDroneSelection(tgo.gameObject); // For now, the cons menu on drones only opens when they are single-selected
                }
            }
        }
        else if (Input.GetMouseButtonDown(0) && _playerSelectedObjects.Count == 0)
        {
            if (!_bAreaSelectionActive)
            {
                _areaSelectionInitialPos = Input.mousePosition;
                DeselectAll();
                _bAreaSelectionActive = true;
            }
        }
        //else if (Input.GetMouseButtonUp(0)) // [Mike, 31-5-18] -- Offline from 2-6-18 so we only deselect via the CommandScript or right clicking below
        //{
        //	// Not hovering over anything, or something is already selected
        //	DeselectAll();
        //}

        if (_bAreaSelectionActive)
        {
            ManageAreaSelection();
        }
    }

    private void CheckIfConsDroneSelection(GameObject newSelection)
    {
        AIConsDroneScript cds = newSelection.GetComponent<AIConsDroneScript>();
        if (cds != null)
        {
            if (!_worldScript.ConstructionManager.bIsConsMenuOpen)
            {
                _worldScript.ConstructionManager.OpenConstructionMenu_BBB(cds);
            }
        }
    }

    private void CheckSelectedObjectsAreValid()
    {
        if (_playerSelectedObjects.Count > 0)
        {
            List<GameObject> tempList = new List<GameObject>();
            for (int i = 0; i < _playerSelectedObjects.Count; ++i)
            {
                if (_playerSelectedObjects[i] != null)
                {
                    tempList.Add(_playerSelectedObjects[i]); // Keep it
                }
            }
            _playerSelectedObjects = tempList;
        }
        else
        {
            ResetSelection(); // Make sure that selection is always available when there are not selected objects -- bit of a catch-all
        }
    }

    private List<TrainGameObjScript> _currHoveredOverObjs = new List<TrainGameObjScript>(); /// Class var

    private void ManageAreaSelection()
    {
        //_currHoveredOverObjs = WorldScript.GetObjsInScreenArea(WorldScript.GetAllTGOsInWorld(), _areaSelectionInitialPos, new Vector2(Input.mousePosition.x, Input.mousePosition.y), false);
        _currHoveredOverObjs = WorldScript.GetObjsInScreenArea(WorldScript.GetAllTGOsInWorld(), WS.HS.HUDAreaSelectionBox_topLeft, WS.HS.HUDAreaSelectionBox_btmRight, false);
        //HUDAreaSelectionBox_topLeft

        //print("----------------------");
        //print("_areaSelectionInitialPos: " + _areaSelectionInitialPos.ToString());
        //print("Input.mousePosition: " + Input.mousePosition.ToString());
        //print("-----------");
        //print("WS.HS.HUDAreaSelectionBox_topLeft: " + WS.HS.HUDAreaSelectionBox_topLeft.ToString());
        //print("WS.HS.HUDAreaSelectionBox_btmRight: " + WS.HS.HUDAreaSelectionBox_btmRight.ToString());
        //print("----------------------");


        if (Input.GetMouseButtonUp(0)) // End the selection drag
        {
            for (int i = 0; i < _currHoveredOverObjs.Count; ++i)
            {
                _currHoveredOverObjs[i].Select();
            }

            _areaSelectionInitialPos = Vector2.zero;
            _bAreaSelectionActive = false;
        }
    }

    public void DeselectAll()
    {
        for (int i = 0; i < _playerSelectedObjects.Count; ++i)
        {
            TrainGameObjScript tgo = BBBStatics.TGO(_playerSelectedObjects[i]);
            if (tgo != null)
            {
                tgo.Deselect();
            }
        }

        ResetSelection();
    }

    public void ResetSelection()
    {
        if (_playerSelectedObjects.Count > 0) _playerSelectedObjects.Clear();

        if (!_bSelectionActive) _bPendingSetSelectionActive = true;

        if (WS.CS.BInRecycleMode) WS.CS.BInRecycleMode = false;

        if (WS.ConstructionManager.bIsConsMenuOpen)
        {
            WS.ConstructionManager.GetOpenedConstructionMenu().CloseConsMenu();
        }

        if (WS.CS.BuildObjArchetype != null)
        {
            WS.CS.BuildObjArchetype = null;
        }
    }

    public bool bPlayerHasAnObjectSelected
    {
        get { return _playerSelectedObjects.Count > 0; }
    }

    public void AddPlayerSelectedObj(GameObject o)
    {
        _playerSelectedObjects.Add(o);
    }

    public void RemovePlayerSelectedObj(GameObject o)
    {
        _playerSelectedObjects.Remove(o);
    }

    public bool IsObjPlayerSelected(GameObject o)
    {
        if (_playerSelectedObjects.Count > 0 && o != null && _playerSelectedObjects.Contains(o)) return true;
        return false;
    }

    public bool bCanPlayerSelectObjs
    {
        get { return _bCanPlayerSelectObjs; }
        set { _bCanPlayerSelectObjs = value; }
    }

    public bool bCanPlayerTargetObjs
    {
        get { return _bCanPlayerTargetObjs; }
        set { _bCanPlayerTargetObjs = value; }
    }

    public int PlayerResources
    {
        get { return _playerResources; }
        set { _playerResources = value; }
    }

    public int PlayerMaxResources
    {
        get { return _playerMaxResources; }
        set { _playerMaxResources = value; }
    }

    public bool AddResources(int resources) // Return bSuccess
    {
        if (_playerResources >= _playerMaxResources) return false; // Resources already full

        _playerResources += resources;
        _playerResources = Mathf.Clamp(_playerResources, 0, _playerMaxResources);
        _worldScript.ResourceBarRef.GetComponent<ResourcesMenuScript>().BarIncrement = true;
        _worldScript.ConstructionManager.UpdateConsButtonState();
        return true;
    }

    public bool SubtractResources(int resources) // Return bSuccess
    {
        if (_playerResources <= 0) return false; // Already no resources

        _playerResources -= resources;
        _playerResources = Mathf.Clamp(_playerResources, 0, _playerMaxResources);
        _worldScript.ResourceBarRef.GetComponent<ResourcesMenuScript>().BarIncrement = true;
        _worldScript.ConstructionManager.UpdateConsButtonState();
        return true;
    }

    private void ManageEnemies()
    {
        //print("_enemiesInWorld.Count: " + _enemiesInWorld.Count);

        List<GameObject> newlySpawnedEnemies = new List<GameObject>();

        // Get rid of list refs to any enemies that have been destroyed
        List<GameObject> keepers = new List<GameObject>();
        for (int i = 0; i < _enemiesInWorld.Count; ++i)
        {
            if (_enemiesInWorld[i] != null) keepers.Add(_enemiesInWorld[i]);
        }
        _enemiesInWorld = keepers;

        //

        if (!bIsTrainStoppedAtStation && _enemiesInWorld.Count < _maxEnemiesInWorld)
        {
            if (WorldScript.RandomisationScript.Get_RandTime001_AvailableThisTurn()) // RandTime002_AvailableThisTurn = Every 1 to 3 seconds
            {
                float spawnMaxRange = 150.0f;

                Vector3 spawnPnt = Vector3.zero;

                for (int i = 0; i < 20; ++i)
                {
                    Vector3 testSpawnPnt = WorldScript.LocomotiveObjectRef.transform.position + new Vector3(BBBStatics.RandFlt(-spawnMaxRange, spawnMaxRange), 0.0f, BBBStatics.RandFlt(-spawnMaxRange, spawnMaxRange));

                    //Debug.DrawLine(testSpawnPnt, testSpawnPnt + new Vector3(0, 200, 0));

                    // If the testSpawnPnt is outside of our camera view, set it as spawnPnt
                    if (!BBBStatics.Is3DVecOnScreen(testSpawnPnt))
                    {
                        spawnPnt = testSpawnPnt;
                        break;
                    }
                }

                //

                if (spawnPnt != Vector3.zero)
                {
                    RaycastHit hit = new RaycastHit();
                    bool bHit = Physics.Linecast(spawnPnt + new Vector3(0, 500, 0), spawnPnt + new Vector3(0, -500, 0), out hit);
                    if (bHit && _currEnemyArchetypes.Count > 0)
                    {
                        spawnPnt = hit.point;

                        int rand = BBBStatics.RandInt(0, _currEnemyArchetypes.Count);

                        GameObject newEnemy = Instantiate(_currEnemyArchetypes[rand], spawnPnt, Quaternion.identity) as GameObject;
                        _enemiesInWorld.Add(newEnemy);
                        newlySpawnedEnemies.Add(newEnemy);

                        //print("Spawned an enemy - BBB");
                    }
                }
            }
        }
        else if (_enemiesInWorld.Count > _maxEnemiesInWorld)
        {
            // Not sure that it's possible to get here -- probably a good idea to keep it though just to be absolutely certain

            // Destroy the first enemy found that is off screen
            for (int i = 0; i < _enemiesInWorld.Count; ++i)
            {
                if (!BBBStatics.Is3DVecOnScreen(_enemiesInWorld[i].transform.position))
                {
                    GameObject temp = _enemiesInWorld[i];
                    _enemiesInWorld.Remove(_enemiesInWorld[i]);

                    TrainGameObjScript tgo = BBBStatics.TGO(temp);
                    if (tgo != null) tgo.BeginDestroy(false, false);
                    //else Destroy(temp);

                    break;
                }
            }
        }

        // Remove enemies that are too far away
        for (int i = 0; i < _enemiesInWorld.Count; ++i)
        {
            if (WorldScript.LocomotiveObjectRef == null) continue;

            // We don't want any chance of destroying the enemy that we just created in the same frame -- prevents initialisation so BeginDestroy() causes issues
            if (newlySpawnedEnemies.Contains(_enemiesInWorld[i])) continue;

            float dist = Vector3.Distance(_enemiesInWorld[i].transform.position, WorldScript.LocomotiveObjectRef.transform.position);

            if (dist > 200.0f)
            {
                if (!BBBStatics.Is3DVecOnScreen(_enemiesInWorld[i].transform.position))
                {
                    GameObject temp = _enemiesInWorld[i];
                    //_enemiesInWorld.Remove(_enemiesInWorld[i]);

                    TrainGameObjScript tgo = temp.GetComponent<TrainGameObjScript>();
                    if (tgo != null) tgo.BeginDestroy(false, false);
                    _enemiesInWorld.RemoveAll(item => item == null);
                    //else Destroy(temp);
                }
            }
        }
    }

    public void AddEnemyToWorld(GameObject enemy)
    {
        _enemiesInWorld.Add(enemy);

    }
    public bool bIsTrainStoppedAtStation
    {
        get { return _bIsTrainStoppedAtStation; }
        set { _bIsTrainStoppedAtStation = value; }
    }

    public bool bIsTrainStoppedAtStation_last
    {
        get { return _bIsTrainStoppedAtStation_last; }
        set { _bIsTrainStoppedAtStation_last = value; }
    }

    public void InitialTrainStopAction()
    {
        AddResources(60); // Main Charity
        if (_playerResources == 0) AddResources(20); // Extra Charity

        // TODO: Add system for adding passengers to the passenger carriages
    }

    public int MaxEnemiesInWorld
    {
        get { return _maxEnemiesInWorld; }
        set { _maxEnemiesInWorld = value; }
    }

    /// <summary>
    /// Offline [Mike, 31-5-18]
    /// </summary>
    private void FreebieResources()
    {
        if (_playerResources > 40) return;

        // Basically a cheat -- to avoid the situation where players get stuck without resources

        _freebieResourcesDelay_curr += Time.deltaTime;

        if (_freebieResourcesDelay_curr >= _freebieResourcesDelay)
        {
            AddResources(15);
            _freebieResourcesDelay_curr = 0.0f;
        }
    }

    public void SetAllElementsActive(bool keepSelectionActive)
    {
        SelectableConsPlatforms = new List<ConsPlatformScript>(_worldScript.GetAllConsPlatforms());
        _worldScript.RTSCameraController.BCanMoveCamera = true;
        _worldScript.RTSCameraController.BCameraRotationActive = true;
        _worldScript.RTSCameraController.BCameraTargetSnappingActive = true;
        _worldScript.RTSCameraController.BZoomActive = true;
        _bSelectionActive = keepSelectionActive;

        bIsTrainStoppedAtStation = false;
    }

    public void SetAllElementsInactive()
    {
        SelectableConsPlatforms = new List<ConsPlatformScript>();
        _worldScript.RTSCameraController.BCanMoveCamera = false;
        _worldScript.RTSCameraController.BCameraRotationActive = false;
        _worldScript.RTSCameraController.BCameraTargetSnappingActive = false;
        _worldScript.RTSCameraController.BZoomActive = false;
        _bSelectionActive = false;

        bIsTrainStoppedAtStation = false;
    }

    private void CheckMouseClicks()
    {
        if (Input.GetMouseButtonDown(0)) _bMouseLeftDown = true;
        if (Input.GetMouseButtonUp(0)) _bMouseLeftDown = false;
        if (Input.GetMouseButtonDown(1)) _bMouseRightDown = true;
        if (Input.GetMouseButtonUp(1)) _bMouseRightDown = false;
        if (Input.GetMouseButtonDown(2)) _bMouseMiddleDown = true;
        if (Input.GetMouseButtonUp(2)) _bMouseMiddleDown = false;
    }

    /// <summary>
    /// Change the target time scale to allow the Gameplay script to transition to slow-motion state.
    /// </summary>
    /// <param name="timeScale"></param>
    public void SetTargetTimeScale(float timeScale, float transitionTime)
    {
        _targetTimeScale = timeScale;
        if (transitionTime > 0)
               _timeScaleTransitionTime = transitionTime;
        else
            Time.timeScale = timeScale;
    }

    public void DirectTimeScaleChange(float timeScale)
    {
        _targetTimeScale = timeScale;
        Time.timeScale = timeScale;
    }
    /// <summary>
    /// Adjust the slow-mo rate 
    /// </summary>
    private void AdjustTimeScale()
    {
        if (_targetTimeScale != Time.timeScale)
        {
            _currTransitionTime += BBBStatics.GetTimeScaleIndependentDelta();            
            float resultTimeScale = 1.0f - (1.0f - _targetTimeScale) * BBBStatics.Map(_currTransitionTime, 0.0f, _timeScaleTransitionTime, 0.0f, 1.0f, true);
            Time.timeScale = resultTimeScale;
        }
        else
        {
            if (_currTransitionTime != 0.0f)
            {
                _currTransitionTime = 0.0f;
            }
        }
    }
}
