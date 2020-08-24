using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyTowerScript : TurretScriptParent
{
    public void Awake()
    {
        IsDummy = true;
    }

    public override void Start()
	{
		base.Start();				
		//_bCanFire = false; // Blocks all firing elements including bullets, sounds and muzzle fx
	}

	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();
	}

    protected override void Fire_Simplified()
    {
    }
}
