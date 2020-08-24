using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;

//public enum AIType
//{
//	TrainFollowing,
//	RTS
//}

public enum AISkillLevel
{
	One,
	Two,
	Three
}

public enum AITask
{
	Null,
	Idle,
	GoTo,
	Follow,
	Attack,
	Patrol,
	HoldPosition,
	LookAtTrain,
	FollowTrain,
	FollowTarget,
	Flee,
	StayInFormation,
	Construct,
	Recycle
}

public abstract class AIDynamicObjScript : TrainGameObjScript
{
	//public AIType aiType = AIType.TrainFollowing; // Default for now, until RTS mode is fully implemented
	public AISkillLevel aiSkillLevel;

	protected Rigidbody _rigidbody;                                         // AI Object's Rigid body to control movement

	protected Vector3 _pathingDestination = Vector3.zero;                   // Current destination on the path
	public Vector3 PathingDestination { get { return _pathingDestination; } set { _pathingDestination = value; } }

	protected AITask _currAITask; public AITask CurrAITask { get { return _currAITask; } set { _currAITask = value; } }

	protected Vector3 _inFrontPoint = Vector3.zero;                         //

	protected Vector3 _rotationOverride = new Vector3(0, 0, 0);             //

	protected float _rotationRate;                                          // The rate at which the AI object can rotate

	protected Vector3 _lastTFormPos = new Vector3(0, 0, 0);                 //

	//protected TurretScriptParent _mountedTurret;
	protected List<TurretScriptParent> _mountedTurrets;

	protected GameObject _currentForcedTarget;
	protected Transform _followTarget;

	protected GameObject _followObj; public GameObject FollowObj { get { return _followObj; } set { _followObj = value; } }
	protected GameObject _attackObj; public GameObject AttackObj { get { return _attackObj; } set { _attackObj = value; } }

	protected float _currMoveSpeed = 0.0f;

	protected bool _bRunMovementOnFixedUpdate = true;

	//protected List<Vector3> _currAIWaypoints = new List<Vector3>(); // Currently active path
	//public List<Vector3> CurrAIWaypoints { get { return _currAIWaypoints; } set { _currAIWaypoints = value; } }

	protected float _AIPathPointAcceptDist = 1.5f; // 0.66f until 25-4-18
	protected float _AIFollowDist = 12.0f;
	protected float _AIAttackDist = 30.0f; // Static unless _bAIAttackDistShiftingOnline is true (where it is randomly set between _AIAttackDist_min and _AIAttackDist_max)
	protected float _AIAttackDist_min = 10.0f;
	protected float _AIAttackDist_max = 30.0f;
	protected float _AIBackOffDist = 15.0f;
	protected float _AIBackOffDist_min = 15.0f;
	protected float _AIBackOffDist_max = 20.0f;

	// All of these are mutually exclusive - the first one set to true (in order presented here) will be used
	protected bool _bAIFlankingOnline = false;
	protected bool _bAIAttackDistShiftingOnline = false;
	protected bool _bAIAttackPositionShiftingOnline = true;
	//

	protected float _bAIAttackDistShiftTime_delay = 3.0f;
	protected float _bAIAttackDistShiftTime_delay_min = 3.0f;
	protected float _bAIAttackDistShiftTime_delay_max = 5.0f;
	protected float _bAIAttackDistShiftTime_curr = 0.0f;

	protected float _bAIAttackPosShiftTime_delay = 3.0f;
	protected float _bAIAttackPosShiftTime_delay_min = 2.0f;
	protected float _bAIAttackPosShiftTime_delay_max = 4.0f;
	protected float _bAIAttackPosShiftTime_curr = 0.0f;

	protected TurretScriptParent _mainTurret;
	protected string _myType;

	public override void Start()
	{
		base.Start();

		// Get the turret mounted on this unit
		//_mountedTurret = gameObject.GetComponentInChildren<TurretScriptParent>();
		_myType = GetType().Name;
		_mountedTurrets = new List<TurretScriptParent>(gameObject.GetComponentsInChildren<TurretScriptParent>());
		if (_mountedTurrets.Count > 0)
		{
            int turretCount = _mountedTurrets.Count;
            for (int i = 0; i < turretCount; i++)
            {
                _mountedTurrets[i].AttachedTo = _myType;
                if (!(_mountedTurrets[i].IsDummy))
                {
                    _mainTurret = _mountedTurrets[i];
                    break;
                }
            }

			if (_mainTurret == null)
			{
				_mainTurret = _mountedTurrets[0];
			}

            for (int i = 0; i < turretCount; i++)
            {
                _mountedTurrets[i].ConnectedParentObj = gameObject;
            }
		}

		// Get the rigidbody component and assign to _rigidbody
		_rigidbody = GetComponent<Rigidbody>();

		if (_rigidbody != null)
		{
			_rigidbody.velocity = Vector3.zero;
		}

		// Set default state to following the train
		//if (_worldScript.gameType == GameType.TrainChase && _currAITask != AITask.StayInFormation) // aiType == AIType.TrainFollowing
		//{
		//	_currAITask = AITask.FollowTrain;
		//}

		if (_currAITask != AITask.FollowTrain)
		{
			_currAITask = AITask.FollowTrain;
		}

		_rotationRate = 8.0f;

		int randSkill = BBBStatics.RandInt(1, 3);
		if (randSkill == 1) aiSkillLevel = AISkillLevel.One;
		if (randSkill == 2) aiSkillLevel = AISkillLevel.Two;
		if (randSkill == 3) aiSkillLevel = AISkillLevel.Three;
	}

	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();

		GetInFrontPoint();

		//ChooseAITask();
		//RunAITasks(); // Until 8-8-18

		if (!_bRunMovementOnFixedUpdate)
		{
			RunAITasks();
			RunMovement();
		}

		ManageRotation();

		ManageFormationRotation();
	}

	public void FixedUpdate()
	{
		if (_bRunMovementOnFixedUpdate)
		{
			RunAITasks();
			RunMovement();
		}
	}

	private void RunMovement()
	{
		if (_pathingDestination != Vector3.zero && _currAITask != AITask.Null) // && _currAITask != AITask.Idle && _currAITask != AITask.StayInFormation)
		{
			MoveToDestination(_pathingDestination, true);
		}
		else
		{
			NotMoving();
		}
	}

	protected abstract void MoveToDestination(Vector3 destination, bool bLimitSpeedWhenClose);
	protected abstract void NotMoving();

	//

	//private void ChooseAITask()
	//{
	//	if (_AIPathPoints_FollowTrain.Count > 0)
	//	{
	//		if (_currAITask != AITask.FollowTrain)
	//		{
	//			_currAITask = AITask.FollowTrain;
	//		}
	//	}

	//	if (_AIPathPoints_Patrol.Count > 0)
	//	{
	//		if (_currAITask != AITask.Patrol)
	//		{
	//			_currAITask = AITask.Patrol;
	//		}
	//	}

	//	if (_currAITask == AITask.Patrol)
	//	{
	//		if (_distFromLocomotive < 5.0f)
	//		{
	//			_currAITask = AITask.LookAtTrain;
	//		}
	//	}
	//}

	//

	protected void RunAITasks()
	{
		if (_currAITask == AITask.Idle) AITask_Idle();
		if (_currAITask == AITask.GoTo) AITask_GoTo();
		if (_currAITask == AITask.Follow) AITask_Follow();
		if (_currAITask == AITask.Attack) AITask_Attack();
		if (_currAITask == AITask.FollowTrain) AITask_FollowTrain();
		if (_currAITask == AITask.StayInFormation) AITask_StayInFormation();
		if (_currAITask == AITask.Patrol) AITask_Patrol();
		if (_currAITask == AITask.Construct) AITask_Construct();
		if (_currAITask == AITask.Recycle) AITask_Recycle();
		//if (_currAITask == AITask.LookAtTrain) AITask_LookAtTrain();
	}

	public abstract void AITask_GoTo();
	public abstract void AITask_Patrol();
	public abstract void AITask_FollowTrain();
	public abstract void AITask_StayInFormation();
	public abstract void AITask_Construct();
	public abstract void AITask_Recycle();

	public virtual void AITask_Idle()
	{
		//Debug.Log("I'm idling");
		if (_pathingDestination != Vector3.zero)
		{
			//Debug.Log("I'm setting pathing destination to zero");
			_pathingDestination = Vector3.zero;
		}

        int count = _mountedTurrets.Count;

        for (int i = 0; i < count; ++i)
		{
			if (_mountedTurrets[i].Target != null)
			{
				_currAITask = AITask.Attack;
				_attackObj = _mountedTurrets[i].Target.gameObject;
			}
		}
	}

	public virtual void AITask_Follow()
	{
		if (_followObj != null)
		{
			Vector3 followObjPos = _followObj.transform.position;

			AIDroneScript ds = _followObj.GetComponent<AIDroneScript>(); // If it's a drone, get the position of the ground below the drone so we can path to it
			if (ds != null) followObjPos = ds.BelowHitPos; // Note: ds.BelowHitPos may not always be on the ground -- depends what's under the drone

			//Debug.DrawLine(transform.position, followObjPos, Color.yellow, Time.deltaTime);

			Debug.Log("Trying to change the pathing destination via followObjPoss");
			//_pathingDestination = followObjPos;

			float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(followObjPos.x, followObjPos.z));
			if (dist < _AIFollowDist)
			{
				//_pathingDestination = Vector3.zero;
				Debug.Log("AIDynamicObjectScript, I was going to set _pathingDestination to zero");
			}
		}
		else
		{
			_currAITask = AITask.Idle;
		}
	}

	public virtual void AITask_Attack()
	{
		// TODO Possible: Add check for whether any child turret has LOS

		if (_attackObj != null && _attackObj.GetComponent<TrainGameObjScript>() != null)
		{
			if (_attackObj.GetComponent<TrainGameObjScript>()._team == _team || _attackObj.GetComponent<TrainGameObjScript>()._team == Team.Neutral) // Friendly or neutral
			{
				_attackObj = null;
			}
		}

		if (_attackObj != null)
		{
			Vector3 attackObjPos = _attackObj.transform.position;

			// If it's a drone, get the position of the ground below the drone so we can path to it
			AIDroneScript ds = _attackObj.GetComponent<AIDroneScript>();
			if (ds != null) attackObjPos = ds.BelowHitPos; // Note: ds.BelowHitPos may not always be on the ground -- depends what's under the drone

			//

			//_pathingDestination = Vector3.zero; // Note: This ruins flanking if put up here

			float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_attackObj.transform.position.x, _attackObj.transform.position.z));
			if (dist > _AIAttackDist)
			{
				// Too far away - move towards
				Debug.Log("Trying to change to a new attack position");
				//_pathingDestination = attackObjPos;
			}
			else if (dist < _AIBackOffDist)
			{
				// Too close - move away

				float angle = BBBStatics.GetAngleOnPlaneToFrom(attackObjPos, transform.position);
				Quaternion retreatDir = Quaternion.Euler(new Vector3(0, angle, 0));
				Vector3 retreatPos = BBBStatics.GetRandomOffsetOnGroundInDirection(attackObjPos, retreatDir, 25.0f, 15.0f);
				Debug.DrawLine(transform.position, retreatPos, Color.cyan, Time.deltaTime);
				Debug.Log("Trying to retreat for some dumb reason");
				//_pathingDestination = retreatPos;

				ForceRePath(); // Only child classes which implement this (such as AIGroundUnitScript) will do this
			}
			else if (_bAIFlankingOnline) // In range - get a random flanking position every few seconds and move towards it
			{
				//_currAIWaypoints.Clear();

				if (aiSkillLevel == AISkillLevel.One || aiSkillLevel == AISkillLevel.Two || aiSkillLevel == AISkillLevel.Three)
				{
					//if (_worldScript.RandomisationScript.Get_RandTime007_AvailableThisTurn() || _currAIWaypoints.Count == 0) // Get_RandTime007_AvailableThisTurn -- Between 3-6 seconds
					if (_worldScript.RandomisationScript.Get_RandTime007_AvailableThisTurn()) // Get_RandTime007_AvailableThisTurn -- Between 3-6 seconds
					{
						//if (BBBStatics.RandInt(1, 1) == 1 || aiSkillLevel == AISkillLevel.Three)
						//{
						//Vector3 offset = BBBStatics.GetRandomOffsetOnGround(attackObjPos, 15.0f, 10.0f);

						//float angle = BBBStatics.GetAngleOnPlaneToFrom(attackObjPos, transform.position);

						//float offsetAngle = 90.0f;
						//if (BBBStatics.RandInt(1, 2) == 1) offsetAngle = -90.0f;

						//Quaternion flankingDir = Quaternion.Euler(new Vector3(0, angle + offsetAngle + 180.0f, 0));

						//Vector3 flankingPos = BBBStatics.GetRandomOffsetOnGroundInDirection(transform.position, flankingDir, 25.0f, 15.0f);
						//Debug.DrawLine(flankingPos, flankingPos + new Vector3(0, 20, 0), Color.red, 2.0f);

						//if (gu != null)
						//	_currAIWaypoints = new List<Vector3>() { flankingPos }; // Ground units use waypoints as points to mesh-path towards -- a legacy system
						//else
						Debug.Log("I was doing the flanking thing");
						//_pathingDestination = flankingPos;
						//}
					}
				}
			}
			else if (_bAIAttackDistShiftingOnline) // In range - randomly set our attack distance - this way we might move towards and further away from the enemy we're attacking
			{
				_bAIAttackDistShiftTime_curr += Time.deltaTime;

				if (_bAIAttackDistShiftTime_curr >= _bAIAttackDistShiftTime_delay)
				{
					_AIAttackDist = BBBStatics.RandFlt(_AIAttackDist_min, _AIAttackDist_max);
					_AIBackOffDist = BBBStatics.RandFlt(_AIAttackDist_min, _AIAttackDist_max);

					_bAIAttackDistShiftTime_delay = BBBStatics.RandFlt(_bAIAttackDistShiftTime_delay_min, _bAIAttackDistShiftTime_delay_max);
					_bAIAttackDistShiftTime_curr = 0.0f;
				}
			}
			else if (_bAIAttackPositionShiftingOnline) // In range - randomly choose a direction and move our pathing destination to a random (but close) distance in that direction
			{
				_bAIAttackPosShiftTime_curr += Time.deltaTime;

				if (_bAIAttackPosShiftTime_curr >= _bAIAttackPosShiftTime_delay)
				{
					//Quaternion relocatingDir = Quaternion.Euler(new Vector3(0, BBBStatics.RandFlt(0.0f, 360.0f), 0));
					//Vector3 relocatingPos = BBBStatics.GetRandomOffsetOnGroundInDirection(transform.position, relocatingDir, 25.0f, 10.0f);

					Debug.Log("Trying to do the relocating thing");
					//_pathingDestination = relocatingPos;

					_bAIAttackPosShiftTime_delay = BBBStatics.RandFlt(_bAIAttackPosShiftTime_delay_min, _bAIAttackPosShiftTime_delay_max);
					_bAIAttackPosShiftTime_curr = 0.0f;
				}
			}
			else
			{
				Debug.Log("AIDynamicObjectScript, I was going to set _pathingDestination to zero");
				//_pathingDestination = Vector3.zero; // Stand still by default if within range and other behaviours above are offline
			}
		}
		else
		{
			_currAITask = AITask.Idle;
		}
	}

	//

	///// <summary>
	///// Predict the position of the craft in [float SecondsAhead] second's time based on current velocity and direction
	///// </summary>
	///// <param name="SecondsAhead"></param>
	///// <param name="Type"></param>
	///// <returns></returns>
	//public virtual Vector3 PositionPredictor(float SecondsAhead, int Type)
	//{
	//	Vector3 cntPos = transform.position; if (_commSocketObj != null) cntPos = _commSocketObj.transform.position;

	//	if (Type == 1) // Velocity based
	//	{
	//		Vector3 Vel = new Vector3(0, 0, 0);
	//		if (_rigidbody != null)
	//		{
	//			Vel = _rigidbody.velocity;
	//		}

	//		return cntPos + (Vel * SecondsAhead);

	//		//DrawDebugSphere(GetWorld(), Pos, 50.0f, 32, FColor::Cyan, false, -1.0f, '\000');
	//	}

	//	if (Type == 2) // Position shift based
	//	{
	//		//Vector3.Distance(_lastTFormPos, transform.position);

	//		Vector3 vel = _lastTFormPos - transform.position; //? MAY BE WRONG WAY AROUND

	//		return cntPos + (vel * SecondsAhead);
	//	}

	//	return cntPos;
	//}

	/// <summary>
	/// Get a vector3 point a little way on the Z plane in front of the object
	/// </summary>
	protected void GetInFrontPoint()
	{
		_inFrontPoint = transform.position + (transform.rotation * new Vector3(0, 0, -2)); // Example of a rotated vector in Unity
		//Debug.DrawLine(_inFrontPoint, _inFrontPoint + new Vector3(0, 2, 0), Color.white, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!
	}

	/// <summary>
	/// Rotate the object towards the direction that it is moving
	/// </summary>
	protected virtual void ManageRotation()
	{
		float minMoveSpeedBeforeDoThis = 0.1f; // 0.1f;

		// _lastTFormPos == transform.position // We're not moving // Now must be moving faster than
		_currMoveSpeed = Vector3.Distance(_lastTFormPos, transform.position); // We now save this value for use with animations [25-4-18]
		if (_currMoveSpeed < minMoveSpeedBeforeDoThis)
		{
			if (_rotationOverride == new Vector3(0, 0, 0)) // Not using rotation override
			{
				//Debug.DrawLine(transform.position, transform.position + new Vector3(0, 20, 0), Color.red, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!
				return;
			}
		}

		//Debug.DrawLine(transform.position, transform.position + new Vector3(0, 20, 0), Color.green, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!

		Vector3 dir = _lastTFormPos - transform.position;

		float MovingAngle = Vector3.Angle(
			 Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized,
			 Vector3.ProjectOnPlane(dir, Vector3.up).normalized);

		Vector3 crossA = Vector3.Cross(
			 Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized,
			 Vector3.ProjectOnPlane(dir, Vector3.up).normalized);

		if (crossA.y < 0) MovingAngle *= -1;

		Vector3 CER = transform.rotation.eulerAngles; // Current Euler Rotation

		float angleToUse = MovingAngle;

		if (_rotationOverride != new Vector3(0, 0, 0))
		{
			angleToUse = _rotationOverride.y;
			_rotationOverride = new Vector3(0, 0, 0);
		}

		//

		float diffAngle = BBBStatics.GetSignedAngle(transform.rotation, Quaternion.Euler(0, angleToUse, 0), Vector3.up);

		float Mult = BBBStatics.Map(Mathf.Abs(diffAngle), 45.0f, 0.0f, 1.0f, 0.0f, false);
		Mult = Mathf.Clamp01(Mult);

		float rotRate = _rotationRate * Mult;

		if (diffAngle < 0)
			transform.rotation = Quaternion.Euler(0, CER.y - rotRate, 0);
		else if (diffAngle > 0)
			transform.rotation = Quaternion.Euler(0, CER.y + rotRate, 0);

		_lastTFormPos = transform.position;
	}

	/// <summary>
	/// Manually assign a target to the turret and the current enemy AI for following
	/// </summary>
	/// <param name="target"></param>
	public void AssignTarget(GameObject target)
	{
		_currentForcedTarget = target;

		if (_mountedTurrets[0] != null)
		{
			if (target.GetComponent<TrainGameObjScript>())
			{
				_mountedTurrets[0].ForcedTarget = target.GetComponent<TrainGameObjScript>();
			}
		}
	}

	public void SetAITask(AITask task)
	{
		_currAITask = task;
	}

	public void SetFollowTarget(Transform target)
	{
		_followTarget = target;
	}

	public void ManageFormationRotation()
	{
		if (gameObject.GetComponentInChildren<EventFormationScript>())
		{
			if (_currentForcedTarget != null)
				GetComponentInChildren<EventFormationScript>().gameObject.transform.rotation = Quaternion.LookRotation(_currentForcedTarget.transform.position - transform.position);
		}
	}

	protected virtual void ForceRePath()
	{

	}

    protected GameObject GetNearestRail()
    {
        GameObject closestRail = null;
        float closestRailDist = float.PositiveInfinity;

        int count = _worldScript.AllRails.Count;
        for (int i = 0; i < count; ++i) //foreach (object o in allObjs)
        {
            GameObject go = _worldScript.AllRails[i].gameObject;

            float Dist = Vector3.Distance(go.transform.position, transform.position);
            if (Dist < closestRailDist)
            {
                closestRail = go;
                closestRailDist = Dist;
            }
        }
        return closestRail;
    }

}
