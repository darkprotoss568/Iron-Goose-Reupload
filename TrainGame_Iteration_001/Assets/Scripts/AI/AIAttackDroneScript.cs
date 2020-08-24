using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttackDroneScript : AIDroneScript
{
	private int _pathingDestNum;

	private bool _bPostStartRun = false;

	public override void Start()
	{
		base.Start();

		_maxFlightSpeed = 0.5f; // Now much lower due to MoveToDestination() not setting velocity

		_rotationRate = 4.0f;
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
		if (_worldScript.RandomisationScript.Get_RandTime003_AvailableThisTurn())
		{
			_pathingDestNum = BBBStatics.RandInt(0, _worldScript.GameplayScript.RandOffsetsFromLoco.Count - 2);
		}
		_pathingDestination = _worldScript.GameplayScript.RandOffsetsFromLoco[_pathingDestNum];
	}

	public override void AITask_Patrol()
	{
	}
}
