using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceModule : Module
{
    [Header("Resource Silo properties")]
    [SerializeField]
	private int _playerMaxResourcesAddition = 25;

	private GameObject _currDrone;

    private GameObject _linkedDrone;
	public override void Start()
	{
		base.Start();
        _worldScript.AllResourceSilos.Add(gameObject);
		_worldScript.GameplayScript.PlayerMaxResources += _playerMaxResourcesAddition;

        ObjectCountPanelScript droneCounter = _worldScript.ConstructionManager._droneCounters[0].GetComponent<ObjectCountPanelScript>();
        _linkedDrone = Instantiate(droneCounter.Archetype, transform.position, transform.rotation);

        _currDrone = null;
	}
    
	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();
	}

	public override void BeginDestroy(bool bRunExplosion, bool bSpawnChunks)
	{
		base.BeginDestroy(bRunExplosion, bSpawnChunks);
	}

    protected override void OnDestroy()
    {
        _worldScript.GameplayScript.PlayerMaxResources -= _playerMaxResourcesAddition;
        _worldScript.GameplayScript.PlayerResources = Mathf.Clamp(_worldScript.GameplayScript.PlayerResources, 0, _worldScript.GameplayScript.PlayerMaxResources);
        _linkedDrone.GetComponent<AIScavDroneScript>().BeginDestroy(true, false);

        _worldScript.AllResourceSilos.Remove(gameObject);
        base.OnDestroy();
    }
    
    /// <summary>
    /// Check if the module can be built. Returns false if the player has too many drones.
    /// </summary>
    /// <param name="resources">Amount of available resources to spend</param>
    /// <param name="warning">warning message to return </param>
    /// <returns></returns>
    public override bool CanBeBuilt(int resources, out string warning)
    {
        // Get the drone counter panel script
        ObjectCountPanelScript droneCounter = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>().ConstructionManager._droneCounters[0].GetComponent<ObjectCountPanelScript>();
        if (droneCounter.CheckAtMaximumCount())
        {
            // if the count is at the maximum, return false and output warning
            warning = "Too many drones right now";
            return false;
        } else
        {
            bool enoughResources = base.CanBeBuilt(resources, out warning);
            // check whether the player has enough resources to build
            if (enoughResources)
            {
                // If the player has enough resources, increase the drone count to prevent the player from building more resource silo while the current one is being bult, as the building process is not interruptable
                droneCounter.IncreaseCount(1);
            }

            return enoughResources;
        }
    }

    public GameObject CurrDrone
	{
		get { return _currDrone; }
		set { _currDrone = value; }
	}


}
