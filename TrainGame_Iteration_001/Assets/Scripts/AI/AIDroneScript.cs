using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIDroneScript : AIDynamicObjScript
{
	protected float _maxFlightSpeed = 15.0f;
	protected float _flightAltitude_regular;
	protected float _flightAltitude_curr;

	protected float _currAltitude;

	protected float _elevationSpeed;

	protected float _worldAltitudeOverride;

	protected bool _bUseVelocity;

	//

	protected List<Collider> _collidersInAvoidanceRange = new List<Collider>();

	protected float _maxAvoidSpeed;
	protected float _minAvoidSpeed;

	protected SphereCollider _ourAvoidanceTriggerCollider;

	protected bool _bCutMovementWhileColliding = false;

	protected bool _bObstacleAvoidanceActive = false;

	//

	/// For cons and scav drones
	protected int _maxHeldResources = 30;
	protected int _heldResources = 0;

	protected GameObject _currSilo;

	//

	protected float _timeSinceDownRayCastHit = 0.0f;

	protected Vector3 _belowHitPos = Vector3.zero; public Vector3 BelowHitPos { get { return _belowHitPos; } }

	public override void Start()
	{
		base.Start();

		_flightAltitude_regular = 14.0f;
		_flightAltitude_curr = _flightAltitude_regular;

		_worldAltitudeOverride = float.PositiveInfinity;

		_elevationSpeed = 0.1f;

		_bUseVelocity = false; // false from 2-4-18 @ 15:58

		_minAvoidSpeed = 0.1f;
		_maxAvoidSpeed = 15.0f;

		SphereCollider sc = GetComponent<SphereCollider>();
		if (sc.isTrigger)
		{
			_ourAvoidanceTriggerCollider = sc;
		}
	}

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_team == Team.Friendly)
        {
            ObjectCountPanelScript[] droneCounters = _worldScript.ConstructionManager._droneCounters;

            for (int i = 0; i < droneCounters.Length; i++)
            {
                if (droneCounters[i] != null)
                {
                    if (_name == droneCounters[i].Archetype.GetComponent<TrainGameObjScript>()._name)
                    {
                        droneCounters[i].IncreaseCount(-1);
                    }
                }
            }
            _worldScript.AllResourceDrones.Remove(gameObject);
        }
    }
    public new void FixedUpdate()
	{
		if (PauseMenu.isPaused) return;

        base.FixedUpdate();

        //DampenRotation();

        if (_bObstacleAvoidanceActive) AvoidObstacles();

		CheckAltitude();
		MaintainAltitude();

		_rigidbody.drag = 2.0f; // 2.0f
		_rigidbody.angularDrag = 1.0f; // TEST - 1-4-18 @ 19:49

		//

		if (_currSilo != null)
		{
			Debug.DrawLine(_currSilo.transform.position, transform.position, Color.cyan, Time.deltaTime);
		}
	}

	//protected void DampenRotation()
	//{
	//	_rigidbody.angularVelocity = new Vector3(_rigidbody.angularVelocity.x, _rigidbody.angularVelocity.y * 0.9f, _rigidbody.angularVelocity.z);
	//}

	protected void CheckAltitude()
	{
		_belowHitPos = Vector3.zero;

		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down * 500, out hit))
		{
			_currAltitude = Vector3.Distance(transform.position, hit.point);
			_timeSinceDownRayCastHit = 0.0f;

			_belowHitPos = hit.point;
		}
		else
		{
			//? We're not over the terrain -- something has gone wrong

			//_worldAltitudeOverride = float.PositiveInfinity;
			//BeginDestroy(false, false);
			transform.position += new Vector3(0, 1, 0); /// Move us upwards -- maybe we'll get back above ground?
			_timeSinceDownRayCastHit += Time.deltaTime;

			if (_timeSinceDownRayCastHit > 5.0f)
			{
				BeginDestroy(false, false);
			}
		}
	}

	/// <summary>
	/// Keep the drone at a set altitude
	/// </summary>
	protected void MaintainAltitude()
	{
		float currAltitudeToUse = _currAltitude;
		float desiredAltitudeToUse = _flightAltitude_curr;

		if (_worldAltitudeOverride != float.PositiveInfinity)
		{
			currAltitudeToUse = transform.position.y;
			desiredAltitudeToUse = _worldAltitudeOverride;

			_worldAltitudeOverride = float.PositiveInfinity; // Reset
		}

		//

		//_rigidbody.AddForce(new Vector3(0, -Physics.gravity.y, 0), ForceMode.Acceleration);
		if (_bUseVelocity)
		{
			if (currAltitudeToUse < desiredAltitudeToUse)
			{
				_rigidbody.velocity = new Vector3(_rigidbody.velocity.x, BBBStatics.Map(currAltitudeToUse, 0.0f, desiredAltitudeToUse, -Physics.gravity.y, 0.0f, true), _rigidbody.velocity.z);
			}
		}
		else
		{
			float tempElSpeed = _elevationSpeed * Time.deltaTime * 60.0f;
			transform.position += new Vector3(0.0f, BBBStatics.Map(currAltitudeToUse, desiredAltitudeToUse - 2.0f, desiredAltitudeToUse + 2.0f, tempElSpeed, -tempElSpeed, true), 0.0f);
		}
	}

	/// <summary>
	/// [Deprecated desc.] Directly set our velocity so we move towards our intended vector: destination
	/// Now we set the position directly -- far more reliable although will no doubt mess with physics
	// TODO: Make it so drones use the same rigidbody movement system as the ground units
	/// </summary>
	protected override void MoveToDestination(Vector3 destination, bool bLimitSpeedWhenClose)
	{
		//print("Drone MoveToDestination()");

		float speedUberMult = 1.0f;

		if (_bObstacleAvoidanceActive && _collidersInAvoidanceRange.Count > 0)
		{
			if (_bCutMovementWhileColliding) // Don't interfere with obstacle avoidance (7-4-18)
			{
				return;
			}
			else
			{
				speedUberMult = 0.25f; // Slow down to reduce interference with obstacle avoidance
			}
		}

		//

		Vector3 dir = destination - transform.position;
		dir.Normalize();

		float distToDest = Vector3.Distance(destination, transform.position);

		if (_bUseVelocity)
		{
			_rigidbody.velocity = Vector3.ClampMagnitude(new Vector3(_rigidbody.velocity.x + dir.x, _rigidbody.velocity.y, _rigidbody.velocity.z + dir.z), _maxFlightSpeed);
		}
		else
		{
			float speed = _maxFlightSpeed;

			//if (bLimitSpeedWhenClose) speed = BBBStatics.Map(distToDest, 20.0f, 0.0f, _maxFlightSpeed, 0.0f, true);
			if (bLimitSpeedWhenClose) speed = BBBStatics.Map(distToDest, 5.0f, 0.0f, _maxFlightSpeed, 0.0f, true);

			speed *= speedUberMult;

			//transform.Translate(Vector3.ClampMagnitude(new Vector3(dir.x, 0.0f, dir.z), _maxFlightSpeed));
			transform.position += Vector3.ClampMagnitude(new Vector3(dir.x, 0.0f, dir.z), speed);
		}
	}

	//protected override void NotMoving()
	//{
	//}

	public override void AITask_Idle()
	{
		if (_pathingDestination != Vector3.zero)
		{
			_pathingDestination = Vector3.zero;
		}
	}

	public override void AITask_GoTo()
	{
		if (_pathingDestination == Vector3.zero)
		{
			_currAITask = AITask.Idle;
			return;
		}

		//if (_currAIWaypoints.Count > 0)
		//{
		//	//_pathingDestination = _currAIWaypoints[0];

		//	float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_currAIWaypoints[0].x, _currAIWaypoints[0].z));
		//	if (dist < _AIPathPointAcceptDist)
		//	{
		//		_currAIWaypoints.RemoveAt(0);
		//	}
		//}
		//else
		//{
		//	_currAITask = AITask.Idle;
		//}

		float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_pathingDestination.x, _pathingDestination.z));
		if (dist < _AIPathPointAcceptDist)
		{
			_currAITask = AITask.Idle;
		}
	}

	//public override void AITask_Follow()
	//{
	//	base.AITask_Follow();
	//}


	public override void AITask_Attack()
	{
		base.AITask_Attack();
	}

	public override void AITask_FollowTrain() {}
	public override void AITask_Patrol() {}
	public override void AITask_Construct() {}
	public override void AITask_Recycle() { }

	void OnGUI() // For Debug Labels
	{
		//var restoreColor = GUI.color; GUI.color = Color.green; // red
		//UnityEditor.Handles.Label(transform.position, "currAltitude: " + currAltitude.ToString());
		//UnityEditor.Handles.Label(transform.position, "_collidersInAvoidanceRange.Count: " + _collidersInAvoidanceRange.Count.ToString());
		//GUI.color = restoreColor;
	}

	public float FlightAltitude_Curr
	{
		get { return _flightAltitude_curr; }
		set { _flightAltitude_curr = value; }
	}

	public float FlightAltitude_Regular
	{
		get { return _flightAltitude_regular; }
	}

	/////

	private void OnTriggerEnter(Collider other)
	{
		if (!_collidersInAvoidanceRange.Contains(other))
		{
			if (other == transform.GetComponent<Collider>()) return; // We don't want to collide with ourself

			Collider[] colliders = transform.GetComponentsInChildren<Collider>(); // Or any of our child objects

            int length = colliders.Length;
			for (int i = 0; i < length; ++i)
			{
				if (other == colliders[i]) return; // It's one of our children
			}

			// Only these types of colliders can be used with the 'ClosestPoint' check
			if (!(other.GetType().Equals(typeof(SphereCollider)) || other.GetType().Equals(typeof(BoxCollider))
			|| other.GetType().Equals(typeof(CapsuleCollider)) || other.GetType().Equals(typeof(MeshCollider))))
			{
				return;
			}

			_collidersInAvoidanceRange.Add(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (_collidersInAvoidanceRange.Contains(other))
		{
			_collidersInAvoidanceRange.Remove(other);
		}
	}

	//private void OnTriggerStay(Collider other)
	//{
	//	// Avoid obstacles
	//	Vector3 dir = other.ClosestPoint(transform.position) - transform.position;
	//	dir.Normalize();

	//	transform.position += Vector3.ClampMagnitude(new Vector3(dir.x, dir.y, dir.z), _maxFlightSpeed);
	//}

	/// <summary>
	/// Move away from any obstacles within our trigger sphere collider's radius
	/// </summary>
	protected void AvoidObstacles()
	{
		// First remove any nulled colliders in the array
		List<Collider> collidersToKeep = new List<Collider>();

        int count = _collidersInAvoidanceRange.Count;
		for (int i = 0; i < count; ++i)
		{
			if (_collidersInAvoidanceRange[i] != null)
			{
				collidersToKeep.Add(_collidersInAvoidanceRange[i]);
			}
		}
		_collidersInAvoidanceRange = collidersToKeep;

        //

        // Avoid the obstacles
        int collidersToKeepCount = collidersToKeep.Count;
        for (int i = 0; i < collidersToKeepCount; ++i)
		{
			Vector3 cp = _collidersInAvoidanceRange[i].ClosestPoint(transform.position);

			Vector3 dir = transform.position - cp;
			dir.Normalize();

			float distToColliderPnt = Vector3.Distance(cp, transform.position);
			float speed = BBBStatics.Map(distToColliderPnt, _ourAvoidanceTriggerCollider.radius, 0.0f, _minAvoidSpeed, _maxAvoidSpeed, true);

			transform.position += Vector3.ClampMagnitude(new Vector3(dir.x, dir.y, dir.z), speed);
		}
	}

	public int MaxHeldResources
	{
		get { return _maxHeldResources; }
	}

	public int HeldResources
	{
		get { return _heldResources; }
	}

	protected GameObject GetNearestUnclaimedResourceSilo()
	{
		GameObject result = null;

		
		if (_worldScript.AllResourceSilos.Count > 0)
		{
			List<GameObject> availableResSilos = new List<GameObject>();

            int count = _worldScript.AllResourceSilos.Count;
            for (int i = 0; i < count; i++)
            {
                if (_worldScript.AllResourceSilos[i].GetComponent<ResourceModule>().CurrDrone == null)
                {
                    availableResSilos.Add(_worldScript.AllResourceSilos[i]);
                }
            }
                        
            //ResourceModule[] allResSilos = (ResourceModule[])FindObjectsOfType(typeof(ResourceModule));
            //for (int i = 0; i < allResSilos.Length; ++i)
            //{
            //	if (allResSilos[i].CurrDrone == null)
            //	{
            //		availableResSilos.Add(allResSilos[i].gameObject);
            //	}
            //}

            if (availableResSilos.Count > 0)
			{
				result = BBBStatics.GetClosestGOFromListToVec(availableResSilos, transform.position);
				//result.GetComponent<ResourceModule>().CurrDrone = gameObject;
			}
		}

		return result;
	}

	public override void AITask_StayInFormation()
	{
		if (_followTarget != null)
		{
			MoveToDestination(_followTarget.position, false);
		}
	}
}
