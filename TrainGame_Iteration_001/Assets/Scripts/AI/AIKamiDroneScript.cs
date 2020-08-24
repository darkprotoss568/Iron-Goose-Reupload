using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIKamiDroneScript : AIDroneScript
{
	private int _pathingDestNum;

	private GameObject _ramTarget;

	private bool _bRammingTarget;

	public Material _switchedMat; // Material we switch to when attacking
	private Material _origMat;

	private float RandTime001_Length;
	private float RandTime001_Curr = 0.0f;
	private bool RandTime001_AvailableThisTurn = false;

	private float RandTime002_Length;
	private float RandTime002_Curr = 0.0f;
	private bool RandTime002_AvailableThisTurn = false;

	private bool _bPostStartRun = false;

	private AudioClip beep1;

	public override void Start()
	{
		base.Start();

		_maxFlightSpeed = 0.5f; // Now much lower due to MoveToDestination() not setting velocity

		_elevationSpeed = 0.2f; // Faster than that of normal drones

		_rotationRate = 4.0f;

		_ramTarget = null;
		_bRammingTarget = false;

		_origMat = null;

		//

		RandTime001_Length = BBBStatics.RandFlt(5.0f, 10.0f);
		RandTime002_Length = BBBStatics.RandFlt(5.0f, 10.0f);

		//

		beep1 = Resources.Load("Sounds/beep8") as AudioClip;
	}

	public new void FixedUpdate()
	{
		if (PauseMenu.isPaused) return;

		base.FixedUpdate();

		if (!_bPostStartRun)
		{
			_pathingDestNum = BBBStatics.RandInt(0, _worldScript.GameplayScript.RandOffsetsFromLoco.Count - 2);

			_bPostStartRun = true;
		}

		RandTime001_AvailableThisTurn = false;
		RandTime001_Curr += Time.deltaTime;
		if (RandTime001_Curr >= RandTime001_Length)
		{
			RandTime001_Length = BBBStatics.RandFlt(3.0f, 6.0f);
			RandTime001_Curr = 0.0f;
			RandTime001_AvailableThisTurn = true;
		}

		RandTime002_AvailableThisTurn = false;
		RandTime002_Curr += Time.deltaTime;
		if (RandTime002_Curr >= RandTime002_Length)
		{
			RandTime002_Length = BBBStatics.RandFlt(4.0f, 8.0f);
			RandTime002_Curr = 0.0f;
			RandTime002_AvailableThisTurn = true;
		}

		//

		ChooseRamTarget();
		RamTarget();
	}

	protected override void NotMoving()
	{
	}

	public override void AITask_Idle()
	{
	}

	public override void AITask_GoTo()
	{
	}

	public override void AITask_Follow()
	{
	}

	public override void AITask_Attack()
	{
	}

	public override void AITask_FollowTrain()
	{
		if (RandTime001_AvailableThisTurn) // Get_RandTime006_AvailableThisTurn
		{
			_pathingDestNum = Random.Range(0, _worldScript.GameplayScript.RandOffsetsFromLoco.Count - 1);
		}
		_pathingDestination = _worldScript.GameplayScript.RandOffsetsFromLoco[_pathingDestNum];
	}

	public override void AITask_Patrol()
	{
	}

	/// Pick which target we're going to slam into and explode all over
	public void ChooseRamTarget()
	{
		if (!_bRammingTarget && RandTime002_AvailableThisTurn)
		{
			//TrainGameObjScript tgo = BBBStatics.ChooseClosestTarget(transform.position, float.PositiveInfinity, _worldScript, _team, null);
			//TrainGameObjScript tgo = BBBStatics.ChooseRandomTarget(transform.position, float.PositiveInfinity, _worldScript, _team);

			GameObject nc = GetNearestCarriage();

			if (nc != null)
			{
				_ramTarget = nc;
				_bRammingTarget = true;

				_currAITask = AITask.Null; // Turn off the class-automatic MoveToDestination() call
				//_pathingDestination = Vector3.zero;

				//print("ChooseRamTarget()");

				if (_switchedMat != null)
				{
					MeshRenderer mr = GetComponent<MeshRenderer>();
					if (mr != null)
					{
						_origMat = mr.material;

						mr.material = _switchedMat;
					}
				}

				//AudioSource.PlayClipAtPoint(beep1, BBBStatics.Pos(gameObject), 1.0f);
				BBBStatics.PlayClipAtPoint_BBB(beep1, BBBStatics.Pos(gameObject), 1.0f, 1.0f);
			}
		}
	}

	private GameObject GetNearestCarriage()
	{
		List<GameObject> carriages = GetAllCarriages();

		if (carriages.Count == 1) return carriages[0]; // Locomotive is alone

		List<GameObject> keepers = new List<GameObject>();
		for (int i = 0; i < carriages.Count; ++i)
		{
			CarriageScript cs = carriages[i].GetComponent<CarriageScript>();
			if (!cs.bIsLocomotive) keepers.Add(carriages[i]);
		}
		carriages = keepers;

		return BBBStatics.GetClosestGOFromListToVec(carriages, transform.position);
	}

	private List<GameObject> GetAllCarriages()
	{
		List<GameObject> carriages = new List<GameObject>();

		List<GameObject> allObjs = _worldScript.GetAllTGOsInWorld_AsGOs();
		for (int i = 0; i < allObjs.Count; ++i)
		{
			CarriageScript cs = allObjs[i].GetComponent<CarriageScript>();
			if (cs != null)
			{
				carriages.Add(allObjs[i]);
			}
		}

		return carriages;
	}

	/// Get direction to target and move towards it
	public void RamTarget()
	{
		if (_bRammingTarget)
		{
			//if (BBBStatics.TGO(_ramTarget) != null) _ramTarget = null; // Probably a bandaid fix

			if (_ramTarget == null) // Have we lost our target?
			{
				_bRammingTarget = false;
				_currAITask = AITask.FollowTrain;

				MeshRenderer mr = GetComponent<MeshRenderer>();
				if (mr != null && _origMat != null)
				{
					mr.material = _origMat;
				}

				return;
			}

			//

			if (BBBStatics.TGO(_ramTarget).CommSocketObj == null)
			{
				print("Error: _ramTarget has no commSocket: " + _ramTarget.name);
			}

			Vector3 targetPos = BBBStatics.Pos(_ramTarget);

			MoveToDestination(targetPos, false);
			_worldAltitudeOverride = targetPos.y;

			Debug.DrawLine(transform.position, targetPos, Color.red, Time.deltaTime);
		}
	}

	void OnCollisionEnter(Collision col)
	{
		if (_bRammingTarget)
		{
			// We don't want to damage ourself just yet
			List<TrainGameObjScript> immuneTGOs = new List<TrainGameObjScript>() { GetComponent<TrainGameObjScript>() };

			_worldScript.ExplosionScript.Explosion(_commSocketObj.transform.position, Resources.Load("FX/Explosion001") as GameObject, null, 200, 8.0f, immuneTGOs, name, 1.0f);
			GetComponent<TrainGameObjScript>().BeginDestroy(false, true);
		}
	}
}
