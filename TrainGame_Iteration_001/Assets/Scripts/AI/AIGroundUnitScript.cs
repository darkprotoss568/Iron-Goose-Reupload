//using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum RailFollowDir
{
	Forward,
	Back
}

public enum TrainSideOn
{
	Right,
	Left
}

public class AIGroundUnitScript : AIDynamicObjScript
{
	[Header("AIGroundUnitScript Values")]

	public float _maxMoveSpeed = 20.0f;
	protected float _moveSpeed = 20.0f;

    protected bool _bIsChop = false;

	protected List<Vector3> _currAINavMeshPathPoints = new List<Vector3>(); // Actual path followed -- determined by the AI
	public List<Vector3> CurrAINavMeshPathPoints { get { return _currAINavMeshPathPoints; } set { _currAINavMeshPathPoints = value; } }

	protected Vector3 _destinationTargetPos = new Vector3(0, 0, 0);

	//protected float _distFromLocomotive = 0.0f;

	protected CharacterController _characterController;

	protected bool _bPostStartRun = false;

	protected Animator _animatorComp;

	protected float _animRate_desired = 0.0f;
	protected float _animRate_curr = 0.0f;
	protected float _animRate_rateOfChange = 0.1f;

	protected bool _bIsOnGround;
	protected bool _bIsAboveGround;
	protected bool _bIsBelowGround;
	protected bool _bNoGroundDetected;

	protected Vector3 _groundHitPos;

	protected float _onGroundHeight;

	private int _offsetToFollow = -1;

	private GameObject _nearestCarriage = null;

    protected RailScript _currFollowingRail = null;
    protected int _currFollowingRail_path = 2;
	private int _currFollowingRail_pathPntIdx = 0;
	private RailFollowDir _railFollowDir;
	protected TrainSideOn _trainSideOn;

	private CarriageScript _targetCarriage = null;
	public CarriageScript TargetCarriage { get { return _targetCarriage; } }

	public override void Start()
	{
		base.Start();
		_bRunMovementOnFixedUpdate = true; // For physics based movement of the rigidbody to be smooth

		_bIsOnGround = false;
		_bIsAboveGround = false;
		_bIsBelowGround = false;
		_bNoGroundDetected = false;

		_onGroundHeight = 0.1f;

		_groundHitPos = Vector3.zero;

		// Make sure that there is no character controller attached to the object
		_characterController = GetComponent<CharacterController>();
		if (_characterController != null) Destroy(_characterController); // CharacterController is the cause of all of the issues we've had with ground units [Mike, 1-6-18]

		_animatorComp = GetComponent<Animator>();
		if (_animatorComp != null)
		{
			//_animatorComp.Play("WalkCycle"); // WalkForwards // Jump
		}

		if (_rigidbody != null)
		{
			_rigidbody.GetComponent<Collider>().gameObject.layer = LayerMask.NameToLayer("GroundUnit");
		}

		_railFollowDir = RailFollowDir.Forward;
		_trainSideOn = TrainSideOn.Right;
	}

	public new void FixedUpdate()
	{
		if (PauseMenu.isPaused) return;

		//_animRate_desired = 0.0f; // Will be changed each frame if MoveToDestination() is called via base.Update() below

		base.FixedUpdate();

		if (!_bPostStartRun)
		{
			if (_worldScript.LocomotiveObjectRef != null) PickRandomPathPoint();
			_bPostStartRun = true;
		}

		// As these constantly use a lot of raytracing, they may be expensive - test!
		///CheckRelativeLocToGround();
		///ManageOnGround();

		if (!_worldScript._bUsingRailBasedAIPathing) CreateAIPath();

		if (_animatorComp)
		{
            //_animRate_desired = BBBStatics.Map(_currMoveSpeed, 0.0f, _moveSpeed, 0.0f, 1.0f, true);

			if (_animRate_curr < _animRate_desired) _animRate_curr += _animRate_rateOfChange;
			if (_animRate_curr > _animRate_desired) _animRate_curr -= _animRate_rateOfChange;
			_animRate_curr = Mathf.Clamp01(_animRate_curr);

			_animatorComp.SetFloat("Walk", _animRate_curr);

			//print("_animRate: " + _animRate);
		}

		/////

		//List<Vector3> testGrid = BBBStatics.MakeVectorGrid(transform.position, 16, 10.0f);
		//for (int i = 0; i < testGrid.Count; ++i)
		//{
		//	Debug.DrawLine(testGrid[i], testGrid[i] + new Vector3(0, 20, 0), Color.green, Time.deltaTime);
		//}

		/////

		if (_targetCarriage == null)
		{
			//_targetCarriage = GetRandomCarriage(true);
			_targetCarriage = GetCarriageToAttack(false); // Will only attack locomotive if no other carriages exist
			if (_targetCarriage != null)
			{
				_targetCarriage.EnemiesTargetingThis.Add(this);
			}
		}
	}

    //not being used
	protected void CheckRelativeLocToGround()
	{
		_bIsOnGround = false;
		_bIsAboveGround = false;
		_bIsBelowGround = false;
		_bNoGroundDetected = false;
		_groundHitPos = Vector3.zero;

		//RaycastHit hit = new RaycastHit();
		//Vector3 start = transform.position;
		//Vector3 end = start + new Vector3(0, -10, 0);
		////Debug.DrawLine(start, end, Color.cyan, 5.0f);
		//bool bHit = Physics.Linecast(start, end, out hit);

		float startHeight = 20.0f;

		Ray r = new Ray { origin = transform.position + new Vector3(0, startHeight, 0), direction = Vector3.down };
		List<RaycastHit> rch = BBBStatics.GetAllRaycastAllHitsInDistOrder(r, startHeight * 2.0f, null); // startHeight

        // If any of the hits are on a 'ground' layer object
        int count = rch.Count;
        for (int i = 0; i < count; ++i)
		{
			if (rch[i].transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
			{
				if (rch[i].distance >= (startHeight - _onGroundHeight) && rch[i].distance <= startHeight) // _onGroundHeight
				{
					_bIsOnGround = true;
					_groundHitPos = rch[i].point;
					return;
				}
				else
				{
					_bIsBelowGround = true;
					return;
				}
			}
		}

		_bNoGroundDetected = true; // We didn't hit any ground
	}

    //not being used
	protected void ManageOnGround()
	{
		if (_bNoGroundDetected)
		{
			_rigidbody.useGravity = false;

			// We haven't detected any ground within the normal range -- we may have fallen too far, be too high or perhaps there is no ground at all
			// Check for ground at ultra-distance

			float startHeight = 1000.0f;

			Ray r = new Ray { origin = transform.position + new Vector3(0, startHeight, 0), direction = Vector3.down };
			List<RaycastHit> rch = BBBStatics.GetAllRaycastAllHitsInDistOrder(r, startHeight * 2.0f, null); // startHeight

            // If any of the hits are on a 'ground' layer object
            int count = rch.Count;
            for (int i = 0; i < count; ++i)
			{
				if (rch[i].transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
				{
					transform.position = rch[i].point;
					return;
				}
			}

			print("Error: Ground unit could not detect any ground");

			return;
		}

		//

		if (_bIsOnGround)
		{
			_rigidbody.useGravity = false;
			_rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z);
		}
		else if (_bIsAboveGround)
		{
			_rigidbody.useGravity = true;
		}
		else if (_bIsBelowGround)
		{
			_rigidbody.useGravity = false;
			transform.position = new Vector3(transform.position.x, _groundHitPos.y, transform.position.z);
		}
	}

	protected override void MoveToDestination(Vector3 destination, bool bLimitSpeedWhenClose)
	{
		Vector3 dir = destination - transform.position;

		Vector3 dirN = dir;
		dirN.Normalize();

		float a1 = transform.rotation.eulerAngles.y; // Our current rotation
		float a2 = BBBStatics.Map(Quaternion.LookRotation(dir).eulerAngles.y, 0.0f, 360.0f, -180.0f, 180.0f, true); // Our desired rotation
		float dirMult = (float)BBBStatics.AngleDiff(a1, a2);
		dirMult = BBBStatics.Map(Mathf.Abs(dirMult), 0.0f, 180.0f, 1.0f, 0.1f, true); // So we move slower when facing the wrong direction

		_rigidbody.MovePosition(transform.position + dirN * dirMult * _moveSpeed * Time.deltaTime); // Uses physics -- make sure to run MoveToDestination() on FixedUpdate if using this!

		//_animRate_curr = 1.0f;
		_animRate_desired = 1.0f;
		//_animRate_desired = BBBStatics.Map(_ourVelocity.magnitude, 0.0f, _moveSpeed, 0.0f, 1.0f, true); // Doesn't seem to be 100% accurate [Mike, 2-6-18]
	}

	protected override void NotMoving()
	{
		_animRate_desired = 0.0f;
	}

	//

	public override void AITask_GoTo()
	{
		// Switch to idle on arrival at the intended destination
		//if (_currAIWaypoints.Count > 0)
		//{
		//	//_pathingDestination = ; // Set in CreateAIPath() below due to ground units using mesh pathing points
		//}
		//else
		//{
		//	//print("GoTo task completed");
		//	_currAITask = AITask.Idle;
		//}

		if (_pathingDestination == Vector3.zero)
		{
			//print("GoTo task completed");
			_currAITask = AITask.Idle;
		}
	}

	//public override void AITask_Follow()
	//{
	//	base.AITask_Follow();
	//}

	public override void AITask_Patrol()
	{
	}

	public override void AITask_StayInFormation()
	{
		if (_followTarget != null)
		{
			MoveToDestination(_followTarget.position, false);
		}
	}

	/// <summary>
	/// Pass AI follow-train points to the main path array if it is empty or every few seconds (random interval)
	/// </summary>
	public override void AITask_FollowTrain()
	{
		if (_worldScript.LocomotiveObjectRef == null)
		{
			CurrAITask = AITask.Idle;
			return;
		}
		//if (_worldScript.RandomisationScript.Get_RandTime001_AvailableThisTurn() || _currAIWaypoints.Count <= 0) //? Get_RandTime003_AvailableThisTurn

		if (!_worldScript._bUsingRailBasedAIPathing)
		{
            PickRandomPathPoint();

			if (_pathingDestination == Vector3.zero) FollowNearestCarriage(); // Backup in case we don't have a random path point - [Mike, 29-7-18]
		}
		else
		{
            if (_targetCarriage != null)
            {
                FollowRailPath();
                MatchSpeedWithTargetCarriage();
            }
		}
	}

	private void FollowRailPath()
	{
		// Get the locomotive's and our own rotated world positions so we can compare them
		//Vector3 targetPosRotated = _targetCarriage.transform.position + (_targetCarriage.transform.rotation * Vector3.zero);
		//Vector3 ourPosRotated = transform.position + (_targetCarriage.transform.rotation * Vector3.zero);

		//if (ourPosRotated.x < targetPosRotated.x) // Are we positioned to the right of the train?
		//{

		//}

		//Vector3 ITP = _targetCarriage.transform.InverseTransformPoint(transform.position);
        float forwardBack = _targetCarriage.transform.InverseTransformPoint(transform.position).z;
        float leftRight = _targetCarriage.transform.InverseTransformPoint(transform.position).x;

        //print(":" + ITP);

        //

        if (forwardBack < 0)
			_railFollowDir = RailFollowDir.Back;
		else
			_railFollowDir = RailFollowDir.Forward;

		if (leftRight < 0)
			_trainSideOn = TrainSideOn.Right;
		else
			_trainSideOn = TrainSideOn.Left;

		if (_pathingDestination == Vector3.zero)
		{
            /// Get the initial rail and path point - should only run at the start
            
            if (_currFollowingRail == null)
            {
                _currFollowingRail = GetNearestRail().GetComponent<RailScript>();
            }

			if (_currFollowingRail != null)
			{
				// Get the path point closest to us

				/// Make sure that it picks a path that doesn't already have another unit on it + is on the correct side of the tracks

				FindFollowRailPath(false);

				int idx = BBBStatics.GetClosestVecIndexFromListToVec(_currFollowingRail.Paths[_currFollowingRail_path], transform.position);

				_currFollowingRail_pathPntIdx = idx;
				_pathingDestination = _currFollowingRail.Paths[_currFollowingRail_path][idx];

				_currFollowingRail.FilledPaths[_currFollowingRail_path] = true;
			}
		}
		else
		{
			float distB = BBBStatics.GetDistance2D(transform.position, _pathingDestination);
			if (distB <= _AIPathPointAcceptDist)
			{
				//_pathingDestination = Vector3.zero;

				if (_railFollowDir == RailFollowDir.Forward)
				{
					++_currFollowingRail_pathPntIdx;

					if (_currFollowingRail_pathPntIdx < (_currFollowingRail.Paths[_currFollowingRail_path].Count - 1))
					{
						_pathingDestination = _currFollowingRail.Paths[_currFollowingRail_path][_currFollowingRail_pathPntIdx];
					}
					else
					{
						if (_currFollowingRail.RailFront != null)
						{
							SwitchToRail(_currFollowingRail.RailFront.GetComponent<RailScript>());
							_currFollowingRail_pathPntIdx = 0;
						}
						else CurrAITask = AITask.Idle; // No rail to switch to
					}
				}
				else if (_railFollowDir == RailFollowDir.Back)
				{
					--_currFollowingRail_pathPntIdx;

					if (_currFollowingRail_pathPntIdx >= 0)
					{
						_pathingDestination = _currFollowingRail.Paths[_currFollowingRail_path][_currFollowingRail_pathPntIdx];
					}
					else
					{
						if (_currFollowingRail.RailBehind != null)
						{
							SwitchToRail(_currFollowingRail.RailBehind.GetComponent<RailScript>());
							_currFollowingRail_pathPntIdx = _currFollowingRail.Paths[_currFollowingRail_path].Count-1;
						}
						else CurrAITask = AITask.Idle; // No rail to switch to
					}
				}
			}
		}
	}

	private void SwitchToRail(RailScript newRail)
	{
		_currFollowingRail.FilledPaths[_currFollowingRail_path] = false;
		_currFollowingRail = newRail;
		if (newRail.FilledPaths[_currFollowingRail_path] == true) FindFollowRailPath(false);
		_currFollowingRail.FilledPaths[_currFollowingRail_path] = true;
	}

	protected virtual void FindFollowRailPath(bool bAlsoSetFilledPaths)
	{
		if (bAlsoSetFilledPaths)
			_currFollowingRail.FilledPaths[_currFollowingRail_path] = false;

        if (_trainSideOn == TrainSideOn.Right) // 3/4/5
        {
            if (_currFollowingRail.FilledPaths[3] == false)
                _currFollowingRail_path = 3;
            else if (_currFollowingRail.FilledPaths[4] == false)
                _currFollowingRail_path = 4;
            else
                _currFollowingRail_path = 5;

        }

        if (_trainSideOn == TrainSideOn.Left) // 0/1/2
        {
            if (_currFollowingRail.FilledPaths[0] == false)
                _currFollowingRail_path = 0;
            else if (_currFollowingRail.FilledPaths[1] == false)
                _currFollowingRail_path = 1;
            else
                _currFollowingRail_path = 2;
        }
        
		if (bAlsoSetFilledPaths)
			_currFollowingRail.FilledPaths[_currFollowingRail_path] = true;
	}

	//void OnGUI() // For Debug Labels
	//{
	//	//base.OnGUI(); // Call the super-class function

	//	var restoreColor = GUI.color; GUI.color = Color.green; // red

	//	UnityEditor.Handles.Label(transform.position, "_currFollowingRail_path: " + _currFollowingRail_path);
	//	UnityEditor.Handles.Label(transform.position + new Vector3(0,3,0), "_trainSideOn: " + _trainSideOn.ToString());

	//	GUI.color = restoreColor;
	//}

	private void MatchSpeedWithTargetCarriage()
	{
		//float dist = Mathf.Abs(transform.InverseTransformPoint(transform.position).z - transform.InverseTransformPoint(_targetCarriage.transform.position).z);
		float dist = Mathf.Abs(_targetCarriage.transform.InverseTransformPoint(transform.position).z);

		//print(":" + _targetCarriage.transform.InverseTransformPoint(transform.position));

		//print("dist: " + dist);
		//Debug.DrawLine(transform.position, _targetCarriage.transform.position, Color.yellow, Time.deltaTime);

		float t = BBBStatics.Map(dist, 0.0f, 1.0f, 1.0f, 0.0f, true);
		t = Mathf.Clamp01(t); // Make sure
		_moveSpeed = Mathf.Lerp(_maxMoveSpeed, _targetCarriage.FinalSpeed, t);
	}

	private void FollowNearestCarriage()
	{
		if (_nearestCarriage == null)
		{
			_pathingDestination = Vector3.zero;

			_nearestCarriage = GetNearestCarriage(true);
		}

		if (_nearestCarriage != null)
		{
			_pathingDestination = Vector3.zero;

			float dist = BBBStatics.GetDistance2D(transform.position, _nearestCarriage.transform.position);

			if (dist > Mathf.Lerp(_AIAttackDist_min, _AIAttackDist_max, 0.5f))
			{
				_pathingDestination = _nearestCarriage.transform.position;
			}
		}
	}

	private GameObject GetNearestCarriage(bool bIncludeLocomotive)
	{
		List<GameObject> carriages = new List<GameObject>();

		List<GameObject> allObjs = _worldScript.GetAllTGOsInWorld_AsGOs();

        int count = allObjs.Count;
		for (int i = 0; i < count; ++i)
		{
			GameObject go = allObjs[i];
			CarriageScript cs = go.GetComponent<CarriageScript>();
			if (cs != null)
			{
				if (bIncludeLocomotive && cs.bIsLocomotive || !cs.bIsLocomotive)
				{
					carriages.Add(go);
				}
			}
		}

		return BBBStatics.GetClosestGOFromListToVec(carriages, transform.position);
	}

	private CarriageScript GetRandomCarriage(bool bIncludeLocomotive)
	{
		List<CarriageScript> carriages = new List<CarriageScript>();

		List<GameObject> allObjs = _worldScript.GetAllTGOsInWorld_AsGOs();

        int count = allObjs.Count;
		for (int i = 0; i < count; ++i)
		{
			GameObject go = allObjs[i];
			CarriageScript cs = go.GetComponent<CarriageScript>();
			if (cs != null)
			{
				if (bIncludeLocomotive && cs.bIsLocomotive || !cs.bIsLocomotive)
				{
					carriages.Add(cs);
				}
			}
		}

		if (carriages.Count == 0) return null;

		if (carriages.Count == 1 && bIncludeLocomotive) // Only the locomotive exists (probably)
		{
			return _worldScript.getLocomotiveScript;
		}

		return carriages[BBBStatics.RandInt(0, carriages.Count)];
	}

	//

	private CarriageScript GetCarriageToAttack(bool bIncludeLocomotive)
	{
		List<CarriageScript> carriages = new List<CarriageScript>();

		List<GameObject> allObjs = _worldScript.GetAllTGOsInWorld_AsGOs();

        int count = allObjs.Count;
        for (int i = 0; i < count; ++i)
		{
			GameObject go = allObjs[i];
			CarriageScript cs = go.GetComponent<CarriageScript>();
			if (cs != null)
			{
				carriages.Add(cs);
			}
		}

		//

		if (carriages.Count == 0) return null;

		// Only the locomotive exists (probably)
		if (carriages.Count == 1)
		{
			return _worldScript.getLocomotiveScript;
		}

		//

		// After this point, we don't want the locomotive in the list anymore
		if (!bIncludeLocomotive) carriages.Remove(_worldScript.getLocomotiveScript);

		//

		// Distribute enemies among carriages so they attack as many carriages as possible simultaneously

		for (int c = 0; c < 10; ++c) // Max number of checks
		{
            int carriagesCount = carriages.Count;
            for (int i = 0; i < carriagesCount; ++i)
			{
				if (carriages[i].EnemiesTargetingThis.Count == c) // c = Current min number of attackers targeting each carriage
				{
					return carriages[i];
				}
			}
		}

		//List<CarriageScript> keepers = new List<CarriageScript>();

		//

		// Last resort, pick a random one -- probably can't get here anymore
		return carriages[BBBStatics.RandInt(0, carriages.Count)];
	}

	public override void AITask_Construct() { }
	public override void AITask_Recycle() { }

	/// <summary>
	/// Choose an offset location on the same side of the locomotive as we currently are
	/// </summary>
	protected void PickRandomPathPoint()
	{
		// TODO: Replace the '' system below with: float d = _worldScript.LocomotiveObjectRef.transform.InverseTransformPoint(transform.position).y;

		// Get the locomotive's and our own rotated world positions so we can compare them
		Vector3 locoPosRotated = _worldScript.LocomotiveObjectRef.transform.position + (_worldScript.LocomotiveObjectRef.transform.rotation * Vector3.zero);
		Vector3 ourPosRotated = transform.position + (_worldScript.LocomotiveObjectRef.transform.rotation * Vector3.zero);
		float dist = BBBStatics.GetDistance2D(transform.position, _pathingDestination);

		if (dist >= _mainTurret.FiringRange || dist <= _AIAttackDist_min || _pathingDestination == Vector3.zero) //? Get_RandTime003_AvailableThisTurn
		{
			//Debug.Log("dist >= _mainTurret._firingRange: " + (dist >= _mainTurret._firingRange).ToString());
			//Debug.Log("dist <= _AIAttackDist_min: " + (dist <= _AIAttackDist_min).ToString());
			//Debug.Log("_pathingDestination: " + (_pathingDestination == Vector3.zero).ToString());
			if (_pathingDestination != Vector3.zero)
			{
				if (ourPosRotated.x < locoPosRotated.x)
				{
					_worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Remove(_offsetToFollow);
				}
				else
				{
					_worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Remove(_offsetToFollow);
				}
			}
			_offsetToFollow = -1;
		}

		//_currAIWaypoints.Clear();

		if (ourPosRotated.x < locoPosRotated.x) // Are we positioned to the right of the train?
		{
			// Use the right-side random offsets from the worldScript
			//_currAIWaypoints.Add(_worldScript.RandOffsetsFromLoco[Random.Range(0, _worldScript.RandOffsetsFromLoco.Count - 1)]);

			//_currAIWaypoints.Add(_worldScript.GameplayScript.RandOffsetsFromLoco_Right[BBBStatics.RandInt(0, _worldScript.GameplayScript.RandOffsetsFromLoco_Right.Count - 2)]);

			if (_offsetToFollow == -1)
			{
				if (_mainTurret.Target != null)
				{
                    int count = _worldScript.GameplayScript.RandOffsetsFromLoco_Right.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 location = _worldScript.GameplayScript.RandOffsetsFromLoco_Right[i];
                        float distBetweenTargetAndLocation = BBBStatics.GetDistance2D(_mainTurret.Target.transform.position, location);
                        if (distBetweenTargetAndLocation <= _mainTurret.FiringRange && distBetweenTargetAndLocation >= _AIAttackDist_min && !(_worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location))))
                        {
                            _offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location);
                            _pathingDestination = location;
                            _worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Add(_offsetToFollow);
                            break;
                        }
                    }
                    if (_offsetToFollow == -1)
                    {
                        //Debug.Log("No Valid Position Found. Taking Empty Location. (Right)");

                        count = _worldScript.GameplayScript.RandOffsetsFromLoco_Right.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Vector3 location = _worldScript.GameplayScript.RandOffsetsFromLoco_Right[i];
                            if (!(_worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location))))
                            {
                                _offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location);
                                _pathingDestination = location;
                                _worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Add(_offsetToFollow);
                                break;
                            }
                        }
                    }

                    //code from before optimisation refactor
                    /*
					foreach (Vector3 location in _worldScript.GameplayScript.RandOffsetsFromLoco_Right)
					{
						float distBetweenTargetAndLocation = BBBStatics.GetDistance2D(_mainTurret.Target.transform.position, location);
						if (distBetweenTargetAndLocation <= _mainTurret.FiringRange && distBetweenTargetAndLocation >= _AIAttackDist_min && !(_worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location))))
						{
							_offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location);
							_pathingDestination = location;
							_worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Add(_offsetToFollow);
							break;
						}
					}
					if (_offsetToFollow == -1)
					{
						//Debug.Log("No Valid Position Found. Taking Empty Location. (Right)");
						foreach (Vector3 location in _worldScript.GameplayScript.RandOffsetsFromLoco_Right)
						{
							if (!(_worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location))))
							{
								_offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Right.IndexOf(location);
								_pathingDestination = location;
								_worldScript.GameplayScript.RandOffsetsFromLoco_Right_Occupied.Add(_offsetToFollow);
								break;
							}
						}
					}*/
                }
				else
				{
					//Debug.Log("You Require Additional Targets");
				}
			}
			else
			{
				_pathingDestination = _worldScript.GameplayScript.RandOffsetsFromLoco_Right[_offsetToFollow];
			}

		}
		else // We're on the left side of the train
		{
			// Use the left-side random offsets from the worldScript
			//_currAIWaypoints.Add(_worldScript.GameplayScript.RandOffsetsFromLoco_Left[BBBStatics.RandInt(0, _worldScript.GameplayScript.RandOffsetsFromLoco_Left.Count - 2)]);
			if (_offsetToFollow == -1)
			{
				if (_mainTurret.Target != null)
				{
                    int count = _worldScript.GameplayScript.RandOffsetsFromLoco_Left.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 location = _worldScript.GameplayScript.RandOffsetsFromLoco_Left[i];
                        float distBetweenTargetAndLocation = BBBStatics.GetDistance2D(_mainTurret.Target.transform.position, location);
                        if (distBetweenTargetAndLocation <= _mainTurret.FiringRange && distBetweenTargetAndLocation >= _AIAttackDist_min && !(_worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location))))
                        {
                            _offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location);
                            _pathingDestination = location;
                            _worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Add(_offsetToFollow);
                            break;
                        }
                    }
                    if (_offsetToFollow == -1)
                    {
                        //Debug.Log("No Valid Position Found. Taking Empty Location. (Left)");

                        count = _worldScript.GameplayScript.RandOffsetsFromLoco_Left.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Vector3 location = _worldScript.GameplayScript.RandOffsetsFromLoco_Left[i];
                            if (!(_worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location))))
                            {
                                _offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location);
                                _pathingDestination = location;
                                _worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Add(_offsetToFollow);
                                break;
                            }
                        }
                    }

                    /*foreach (Vector3 location in _worldScript.GameplayScript.RandOffsetsFromLoco_Left)
					{
						float distBetweenTargetAndLocation = BBBStatics.GetDistance2D(_mainTurret.Target.transform.position, location);
						if (distBetweenTargetAndLocation <= _mainTurret.FiringRange && distBetweenTargetAndLocation >= _AIAttackDist_min && !_worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location)))
						{
							_offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location);
							_pathingDestination = location;
							_worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Add(_offsetToFollow);
							break;
						}
					}
					if (_offsetToFollow == -1)
					{
						//Debug.Log("No Valid Position Found. Taking Empty Location. (Left)");
						foreach (Vector3 location in _worldScript.GameplayScript.RandOffsetsFromLoco_Left)
						{
							if (!(_worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Contains(_worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location))))
							{
								_offsetToFollow = _worldScript.GameplayScript.RandOffsetsFromLoco_Left.IndexOf(location);
								_pathingDestination = location;
								_worldScript.GameplayScript.RandOffsetsFromLoco_Left_Occupied.Add(_offsetToFollow);
								break;
							}
						}
					}*/
                }
				else
				{
					//Debug.Log("You Require Additional Targets");
				}
			}
			else
			{
				_pathingDestination = _worldScript.GameplayScript.RandOffsetsFromLoco_Left[_offsetToFollow];
			}
		}
	}

	//

	/// <summary>
	/// Make an AI path towards our desired destination and set the destination so we can move towards it
	/// Also remove path points as they are reached so we can continue to follow the path
	/// </summary>
	protected void CreateAIPath()
	{
		//MoveInput = new Vector2(0.0f, 0.0f);

		if (_pathingDestination != Vector3.zero) // _currAIWaypoints.Count > 0
		{
			//Debug.DrawLine(transform.position, _currAIWaypoints[0], Color.green, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!

			//
			//

			if (_currAINavMeshPathPoints.Count == 0) // _worldScript.Get_RandTime001_AvailableThisTurn())
			{
				// Path from current pos to _currAIWaypoints[0]

				//NMAgent.SetDestination(new Vector3(0, 0, 0)); // Required if we want to use the NavMeshAgent movement system -- will then move towards this point

				if (_currAINavMeshPathPoints.Count > 0) _currAINavMeshPathPoints.Clear();

				///

				Vector3 SourcePos = _inFrontPoint; // Using InFrontPoint to get a bit of a front-direction bias
				Vector3 TargetPos = _pathingDestination; // _currAIWaypoints[0];

				/// Get the nearest point on the nav-mesh to the source position
				NavMeshHit hit;
				bool bSamplePos = UnityEngine.AI.NavMesh.SamplePosition(SourcePos, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas);
				if (bSamplePos)
				{
					//Debug.DrawLine(transform.position, hit.position, Color.yellow, 0.5f, false); // Remember to turn on 'Gizmos' in game view!
					SourcePos = hit.position;
				}

				/// Get the nearest point on the nav-mesh to the target position
				NavMeshHit hitB;
				bool bSamplePosB = UnityEngine.AI.NavMesh.SamplePosition(TargetPos, out hitB, 5.0f, UnityEngine.AI.NavMesh.AllAreas);
				if (bSamplePosB)
				{
					//Debug.DrawLine(transform.position, hitB.position, Color.magenta, 0.5f, false); // Remember to turn on 'Gizmos' in game view!
					TargetPos = hitB.position;
				}

				_destinationTargetPos = TargetPos;

				NavMeshPath NMP = new NavMeshPath();

				NavMeshQueryFilter Q = new NavMeshQueryFilter
				{
					areaMask = NavMesh.AllAreas
				};

				bool bSuccess = NavMesh.CalculatePath(SourcePos, TargetPos, Q, NMP);

				if (bSuccess)
				{
					_currAINavMeshPathPoints = new List<Vector3>(NMP.corners);
				}
			}

			//
			//

			if (_currAINavMeshPathPoints.Count > 0)
			{
				///// Debugging
				if (_currAINavMeshPathPoints.Count > 1)
				{
					for (int i = 1; i < _currAINavMeshPathPoints.Count; ++i)
					{
						Debug.DrawLine(_currAINavMeshPathPoints[i - 1], _currAINavMeshPathPoints[i], Color.cyan, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!
					}
				}
				/////

				_pathingDestination = _currAINavMeshPathPoints[0]; //! THIS IS WHERE WE SET THE PATH POINT THAT WILL ACTUALLY BE FOLLOWED

				//Debug.DrawLine(transform.position, _currAINavMeshPathPoints[0], Color.red, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!

				//print("MoveInput: " + MoveInput);

				float DistA = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_currAINavMeshPathPoints[0].x, _currAINavMeshPathPoints[0].z));
				if (DistA <= _AIPathPointAcceptDist)
				{
					_currAINavMeshPathPoints.RemoveAt(0);
				}


				Vector3 DistBDest = _destinationTargetPos; // _currAIWaypoints[0]
				float DistB = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(DistBDest.x, DistBDest.z));
				if (DistB <= _AIPathPointAcceptDist)
				{
					//_currAIWaypoints.RemoveAt(0);
					//Debug.Log("AIGroundUnitScript, I was going to set _pathingDestination to zero");
					//_pathingDestination = Vector3.zero;
				}

				//print("_currAIWaypoints.Count: " + _currAIWaypoints.Count);
			}
		}
		else // No waypoints, remove AI path points
		{
			if (_currAINavMeshPathPoints.Count > 0) _currAINavMeshPathPoints.Clear();
			//_pathingDestination = Vector3.zero; // Already the case
		}
	}

	//public override Vector3 PositionPredictor(float SecondsAhead)
	//{
	//	Vector3 CntPos = transform.position;

	//	if (_commSocketObj != null)
	//		CntPos = _commSocketObj.transform.position;

	//	Vector3 prediction = CntPos + (transform.rotation * new Vector3(0, 0, -_maxMoveSpeed * SecondsAhead));

	//	return prediction;
	//}

	protected void OnCollisionStay(Collision collision)
	{
		//_rigidbody.MovePosition(transform.position + new Vector3(0, 1.0f, 0)); // Test [Mike, 2-6-18]
	}

	protected override void ForceRePath()
	{
		base.ForceRePath();

		if (_currAINavMeshPathPoints.Count > 0) _currAINavMeshPathPoints.Clear();
	}

	protected override void OnDestroy()
	{
		if (_targetCarriage != null && _targetCarriage.EnemiesTargetingThis.Contains(this))
			_targetCarriage.EnemiesTargetingThis.Remove(this);

		base.OnDestroy();
	}
}
