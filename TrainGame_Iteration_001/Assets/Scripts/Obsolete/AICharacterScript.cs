using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIType_Character
{
	Enemy1,
	Enemy2,
	Enemy3
}

public enum AITask_Character
{
	Idle,
	Patrol,
	LookAtTrain,
	FollowTrain,
	FollowTarget,
	Attack,
	Flee
}

public partial class AICharacterScript : CharacterScript
{
	public AIType_Character _AIType = AIType_Character.Enemy1;
	public AITask_Character _AIState = AITask_Character.Idle;

	private List<Vector3> _CurrAIPathPoints = new List<Vector3>(); // Currently active path
	public List<Vector3> _AIPathPoints_Patrol = new List<Vector3>(); // Path to patrol on a loop

	private List<Vector3> _CurrAINavMeshPathPoints = new List<Vector3>(); // Actual path followed

	private float _AIPathPointAcceptDist = 0.66f;

	public GameObject PatrolPathPointObjHolder;

	private Vector3 DestinationTargetPos = new Vector3(0, 0, 0);

	//private NavMeshAgent NMAgent;

	private float _DistFromTrain = 0.0f;

	new void Start()
	{
		//NMAgent = GetComponent<NavMeshAgent>();

		MoveInput = new Vector2(0.0f, 0.0f); // Only player characters use this

		base.Start(); // Call the super-class function

		//

		if (PatrolPathPointObjHolder != null)
		{
			GetAIPatrolPathPointsFromComps();
		}
		else
		{
			print("Error: PatrolPathPointObjHolder == null -- AICharacterScript");
		}

		// End of Start
	}

	//

	void GetAIPatrolPathPointsFromComps()
	{
		int OurChildCount = PatrolPathPointObjHolder.transform.childCount;
		//print("PatrolPathPointObjHolder.transform.childCount: " + PatrolPathPointObjHolder.transform.childCount);

		if (OurChildCount <= 0) return;

		List<GameObject> Objs = new List<GameObject>();
		for (int i = 0; i < OurChildCount; ++i)
		{
			GameObject GO = PatrolPathPointObjHolder.transform.GetChild(i).gameObject;
			if (GO != null)
			{
				Objs.Add(GO);
			}
		}

		//for (int i = 0; i < Objs.Count; ++i) print("Objs[i].name: " + Objs[i].name);

		if (_AIPathPoints_Patrol.Count > 0) _AIPathPoints_Patrol.Clear();
		for (int i = 0; i < Objs.Count; ++i)
		{
			_AIPathPoints_Patrol.Add(Objs[i].transform.position);
		}
	}

	//

	new void Update()
	{
		Calculations();

		ManageAITasks();
		RunAITasks();

		FollowAIPath();

		base.Update(); // Call the super-class function

		//base.AfterUpdate();
	}

	void Calculations()
	{
		if (_worldScript.LocomotiveObjectRef == null) return;
		_DistFromTrain = Vector3.Distance(_worldScript.LocomotiveObjectRef.transform.position, transform.position);
	}

	//
	//

	//! AI Tasks

	//
	//

	private void ManageAITasks()
	{
		if (_AIPathPoints_Patrol.Count > 0)
		{
			if (_AIState != AITask_Character.Patrol)
			{
				_AIState = AITask_Character.Patrol;
			}
		}

		if (_AIState == AITask_Character.Patrol)
		{
			if (_DistFromTrain < 5.0f)
			{
				_AIState = AITask_Character.LookAtTrain;	
			}
		}
	}

	//

	private void RunAITasks()
	{
		if (_AIState == AITask_Character.Idle) AITask_Idle();
		if (_AIState == AITask_Character.Patrol) AITask_Patrol();
		if (_AIState == AITask_Character.LookAtTrain) AITask_LookAtTrain();
	}

	private void AITask_Idle()
	{ 
		//TODO: Automatically select another task?


	}

	private void AITask_LookAtTrain()
	{
		if (_worldScript.LocomotiveObjectRef == null) return;

		//RotationOverride = ;

		if (_CurrAIPathPoints.Count > 0) _CurrAIPathPoints.Clear();
		if (_CurrAINavMeshPathPoints.Count > 0) _CurrAINavMeshPathPoints.Clear();

		float Angle = Vector3.Angle(
			Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized,
			Vector3.ProjectOnPlane(transform.position - _worldScript.LocomotiveObjectRef.transform.position, Vector3.up).normalized);

		Vector3 crossA = Vector3.Cross(
			Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized,
			Vector3.ProjectOnPlane(transform.position - _worldScript.LocomotiveObjectRef.transform.position, Vector3.up).normalized);

		if (crossA.y < 0) Angle *= -1;

		RotationOverride = new Vector3(0, Angle, 0);

		if (_DistFromTrain > 5.0f)
		{
			_AIState = AITask_Character.Idle;
		}
	}

	private void AITask_Patrol()
	{
		// Pass AI patrol path points to the main path array if it is empty -- so patrols can loop

		if (_AIPathPoints_Patrol.Count > 0)
		{
			if (_CurrAIPathPoints.Count <= 0)
			{
				//?_CurrAIPathPoints = _AIPathPoints_Patrol; // Makes _CurrAIPathPoints a reference to _AIPathPoints_Patrol! Not what we want!

				// Copy the path point vectors from _AIPathPoints_Patrol to _CurrAIPathPoints
				_CurrAIPathPoints.Clear(); for (int i = 0; i < _AIPathPoints_Patrol.Count; ++i) _CurrAIPathPoints.Add(_AIPathPoints_Patrol[i]);
			}
		}
		else
		{
			_AIState = AITask_Character.Idle; // If for some reason we are in patrol mode without any patrol path points
		}
	}

	//
	//

	//! End of AI Tasks

	//
	//

	private void FollowAIPath()
	{
		//MoveInput = new Vector2(0.0f, 0.0f);

		if (_CurrAIPathPoints.Count > 0)
		{
			//Debug.DrawLine(transform.position, _CurrAIPathPoints[0], Color.green, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!

			//
			//

			if (_CurrAINavMeshPathPoints.Count == 0) // _worldScript.Get_RandTime001_AvailableThisTurn())
			{
				// Path from current pos to _CurrAIPathPoints[0]

				//NMAgent.SetDestination(new Vector3(0, 0, 0)); // Required if we want to use the NavMeshAgent movement system -- will then move towards this point

				if (_CurrAINavMeshPathPoints.Count > 0) _CurrAINavMeshPathPoints.Clear();

				///

				Vector3 SourcePos = InFrontPoint; // InFrontPoint // transform.position // Using InFrontPoint to get a bit of a front-direction bias
				Vector3 TargetPos = _CurrAIPathPoints[0];

				/// Get the nearest point on the nav-mesh to the source position
				NavMeshHit hit;
				bool bSamplePos = NavMesh.SamplePosition(SourcePos, out hit, 5.0f, NavMesh.AllAreas);
				if (bSamplePos)
				{
					Debug.DrawLine(transform.position, hit.position, Color.yellow, 0.5f, false); // Remember to turn on 'Gizmos' in game view!
					SourcePos = hit.position;
				}

				/// Get the nearest point on the nav-mesh to the target position
				NavMeshHit hitB;
				bool bSamplePosB = NavMesh.SamplePosition(TargetPos, out hitB, 5.0f, NavMesh.AllAreas);
				if (bSamplePosB)
				{
					Debug.DrawLine(transform.position, hitB.position, Color.magenta, 0.5f, false); // Remember to turn on 'Gizmos' in game view!
					TargetPos = hitB.position;
				}

				DestinationTargetPos = TargetPos;

				NavMeshPath NMP = new NavMeshPath();

				NavMeshQueryFilter Q = new NavMeshQueryFilter();
				Q.areaMask = NavMesh.AllAreas;
				
				//bool bSuccess = NMAgent.CalculatePath(_CurrAIPathPoints[0], NMP);
				bool bSuccess = NavMesh.CalculatePath(SourcePos, TargetPos, Q, NMP);

				if (bSuccess)
				{
					_CurrAINavMeshPathPoints = new List<Vector3>(NMP.corners);
					//_CurrAINavMeshPathPoints.Reverse();

					//x_CurrAINavMeshPathPoints.RemoveAt(0); // Remove the first point (at our feet) as we don't need it

					//print("_CurrAINavMeshPathPoints.Count: " + _CurrAINavMeshPathPoints.Count);

					//print("Pathing success: " + Time.time);
				}
				//else
				//{
				//	print("Pathing failed: " + Time.time);
				//}
			}

			//
			//

			if (_CurrAINavMeshPathPoints.Count > 0)
			{
				///
				if (_CurrAINavMeshPathPoints.Count > 1)
				{
					for (int i = 1; i < _CurrAINavMeshPathPoints.Count; ++i)
					{
						Debug.DrawLine(_CurrAINavMeshPathPoints[i-1], _CurrAINavMeshPathPoints[i], Color.cyan, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!
					}
				}
				///

				//FollowDestination(_CurrAIPathPoints[0]);
				FollowDestination(_CurrAINavMeshPathPoints[0]);

				Debug.DrawLine(transform.position, _CurrAINavMeshPathPoints[0], Color.red, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!

				//print("MoveInput: " + MoveInput);

				float DistA = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_CurrAINavMeshPathPoints[0].x, _CurrAINavMeshPathPoints[0].z));
				if (DistA <= _AIPathPointAcceptDist)
				{
					_CurrAINavMeshPathPoints.RemoveAt(0);
				}


				Vector3 DistBDest = DestinationTargetPos; // _CurrAIPathPoints[0]
				float DistB = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(DistBDest.x, DistBDest.z));
				if (DistB <= _AIPathPointAcceptDist)
				{
					_CurrAIPathPoints.RemoveAt(0);
				}

				//print("_CurrAIPathPoints.Count: " + _CurrAIPathPoints.Count);
			}
		}
	}

	private Vector3 FollowDestination(Vector3 Dest)
	{
		//Dest = new Vector3(0, 0, 0); /// TEST

		Vector3 Dir = Dest - transform.position;

		//MoveInput = new Vector2(Dir.x, Dir.y); // Not used by the AI - only the player
		MovementOverride = Vector3.ClampMagnitude(new Vector3(Dir.x * MoveSpeedMult, 0.0f, Dir.z * MoveSpeedMult), MoveSpeedMult);

		//print("MovementOverride: " + MovementOverride);

		//NMAgent.Move(MovementOverride);

		return Dest; // Pass back again
	}

	//

	new void LateUpdate()
	{
		base.LateUpdate(); // Call the super-class function


	}

	new void OnGUI() // For Debug Labels
	{
		base.OnGUI(); // Call the super-class function

		var restoreColor = GUI.color; GUI.color = Color.green; // red

		//UnityEditor.Handles.Label(transform.position, "_CurrAIPathPoints.Count: " + _CurrAIPathPoints.Count.ToString());
		//UnityEditor.Handles.Label(transform.position, "_AIState: " + _AIState.ToString());


		GUI.color = restoreColor;
	}

	//

	/// For AI visibility checks (not yet implemented) -- 5-3-18
	bool CheckIfRaycastAllHitsObjFirst(Ray r, List<GameObject> IgnoredObjs, Collider CheckIfHitObj)
	{
		if (CheckIfHitObj == null) return false;

		// Make sure that the player is the first thing hit -- remember, RaycastAll returns hits in random order - need to sort them by distance first

		RaycastHit[] hits = Physics.RaycastAll(r, 100.0f);

		List<RaycastHit> hitsNotInOrder = new List<RaycastHit>(hits);
		List<RaycastHit> hitsInOrder = new List<RaycastHit>();

		//! Recursively get the closest hit until we run out of hits
		while (hitsNotInOrder.Count > 0)
		{
			// Get the closest to the start point, add it to hitsInOrder and remove it from hitsNotInOrder

			RaycastHit CurrClosestHit = new RaycastHit();
			float CurrClosestHitDist = float.MaxValue;

			for (int j = 0; j < hitsNotInOrder.Count; ++j)
			{
				float HitDist = hitsNotInOrder[j].distance;
				if (HitDist < CurrClosestHitDist)
				{
					CurrClosestHitDist = HitDist;
					CurrClosestHit = hitsNotInOrder[j];
				}
			}

			//print("CurrClosestHit:" + CurrClosestHit.collider.gameObject.name);

			hitsNotInOrder.Remove(CurrClosestHit);

			if (!IgnoredObjs.Contains(CurrClosestHit.collider.gameObject))
			{
				hitsInOrder.Add(CurrClosestHit);
			}
		}

		if (hitsInOrder.Count > 0 && hitsInOrder[0].collider == CheckIfHitObj) // Is the given object the first thing hit?
		{
			return true;
		}

		return false;
	}
}