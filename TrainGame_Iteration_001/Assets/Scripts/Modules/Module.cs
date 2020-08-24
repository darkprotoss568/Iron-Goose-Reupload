using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Module : TrainGameObjScript
{
    [SerializeField]
    protected float _refundRate = 50;
    protected GameObject _connectedParentObj;           // The object this turret is attached to

    public override void Start()
	{
		base.Start();        
	}

	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();
	}

    /// <summary>
    /// Check if the module can be built. Returns true if the player has resources, otherwise outputs a warning message
    /// </summary>
    /// <param name="resources">Amount of available resources to spend</param>
    /// <param name="warning">warning message to return </param>
    /// <returns></returns>
    public virtual bool CanBeBuilt(int resources, out string warning)
    {
        bool result = true;
        warning = string.Empty;
        if (resources < _buildCost)
        {
            result = false;
            warning = "Not enough resources";
        }

        return result;
    }

    public override void BeginDestroy(bool bRunExplosion, bool bSpawnChunks)
	{
		base.BeginDestroy(bRunExplosion, bSpawnChunks);
	}

    protected override void OnDestroy()
    {
        if (_team == Team.Friendly)
        {
            _worldScript.GameplayScript.AddResources((int)(_buildCost * _refundRate / 100));
        }
        if (_connectedParentObj != null)
        {
            ConsPlatformScript consPlatform = _connectedParentObj.GetComponent<ConsPlatformScript>();
            if (consPlatform != null)
            {
                consPlatform.SetSelectableByPlayer(true);
            }   
        }
    }

    public GameObject ConnectedParentObj { get { return _connectedParentObj; } set { _connectedParentObj = value; } }
}
