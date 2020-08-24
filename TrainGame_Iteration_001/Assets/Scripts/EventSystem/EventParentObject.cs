using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EventParentObject : MonoBehaviour {

    protected WorldScript _worldScript;                     // The current World Script
    protected LocomotiveScript _locomotiveRef;              // The player-controlled Locomotive's script
    protected bool _bIsInitialized;
    protected bool _bIsActivated = false;
    // Use this for initialization
    [SerializeField]
    private bool _stopsTrain;
    [SerializeField]
    private bool _onlyDestroyScript = false;

    [SerializeField]
    private GameObject _chainEvent;                         // The chain event to appear when this event is completed.

    [Header("Timescale control")]
    [SerializeField]
    private float _slowMotionTransitionTime;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private  float _timeScaleWhenActivated = 1.0f;
    [Header("Player Choice Lock")]
    [SerializeField]
    private int[] _constructionLocks = new int[0];
    [Header("Resource gifts")]
    [SerializeField]
    private int _initialResourceGift;                       // The initial resource given to the player when this event is created.
    public virtual void Start ()
    {
        // Get the current WorldScript and LocomotiveScript to access information on the world and the train carriages
        bool bSuccess = false;
        // Exception check in case the event is loaded before the worldScript and/or the locomotive object reference.
        try
        {
            _worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
            _locomotiveRef = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>();
            bSuccess = true;
        }
        catch (NullReferenceException)
        {

        }
        finally
        {
            if (bSuccess)
            {
                _bIsInitialized = true;
            }
        }
        
    }


	// Update is called once per frame
	public virtual void Update ()
    {
        if (!_bIsInitialized)
        {
            Start();
        }

        if (_bIsActivated)
        {
            if (_stopsTrain == true)
            {
                if (!_worldScript.TrainStopped)
                {
                    _worldScript.SetTrainStopped(_stopsTrain);
                }
            }
            
            if (CheckVictoryCondition())
            {
                //TODO: Add reward to player's pool here
                if (_stopsTrain)
                {
                    _worldScript.SetTrainStopped(false);
                }
                //Destroy the event object
                EndEvent();
            }
        }
	}

    /// <summary>
    /// Handle task that must be done at the end of each event.
    /// </summary>
    protected void EndEvent()
    {
        // Unlock the construction menu buttons if there were any locked
        if (_constructionLocks.Length > 0)
        {
            _worldScript.ConstructionManager.UnlockConstructionButtons();
        }
        
        // Reset the timeScale if this event modified the timeScale
        if (_timeScaleWhenActivated < 1.0f)
            _worldScript.GameplayScript.SetTargetTimeScale(1.0f, _slowMotionTransitionTime);
        if (_chainEvent != null)
        {
            // Create a chain event
            // In order for enemy spawn events to act the same even when they are created in runtime, they must always be spawned at the location of the locomotive and default rotation.
            Vector3 spawnPos = _locomotiveRef.transform.transform.TransformPoint(new Vector3(0, 3, 0));
            GameObject newEvent = Instantiate(_chainEvent, spawnPos , _locomotiveRef.transform.rotation);

            //EventParentObject newEventScript = newEvent.GetComponent<EventParentObject>();


            // Force activate the new event.
            //newEventScript.ForceActivate();
        }

        // Check whether only the script is destroyed, or the object containing this script as well. Mostly for spawn events that use existing gameObjects as spawn points
        if (_onlyDestroyScript)
        {
            Destroy(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// Give initial orders for the enemy units
    /// </summary>
    protected virtual void SetInitialDirectives()
    { 
    }

    /// <summary>
    /// Spawn a type of  unit at as specific location.
    /// </summary>
    /// <param name="spawn">Type of enemy</param>
    /// <param name="pos">Position to spawn</param>
    /// <returns></returns>
    protected virtual GameObject SpawnUnit(GameObject spawn, Transform pos)
    {
        GameObject unit = Instantiate(spawn, pos.position, pos.rotation);
        _worldScript.GameplayScript.AddEnemyToWorld(unit);
        return unit;
    }

    /// <summary>
    /// Check the trigger collider to activate the event
    /// </summary>
    /// <param name="other"></param>
    protected void OnTriggerEnter(Collider other)
    {
        if (!_bIsActivated)
        {
            if (other.gameObject.GetComponent<LocomotiveScript>() && _bIsInitialized)
            {
                ForceActivate();
            }
        }
    }

    /// <summary>
    /// Check the trigger collider to activate the event
    /// </summary>
    /// <param name="other"></param>
    protected void OnTriggerStay(Collider other)
    {
        if (!_bIsActivated)
        {
            if (other.gameObject.GetComponent<LocomotiveScript>() && _bIsInitialized)
            {
                ForceActivate();
            }
        }
    }

    /// <summary>
    /// Check the victory condition of the event.
    /// </summary>
    /// <returns></returns>
    protected virtual bool CheckVictoryCondition()
    {
        return false;
    }

    /// <summary>
    /// Activate the event. This is needed to allow an event to activate another event without the trigger collision event.
    /// </summary>
    public void ForceActivate()
    {
        // Prevent this from being run before Start(). We might need to test if handling the initialization in Awake() is better.
        if (!_bIsInitialized)
        {
            Start();
        }
        if (_constructionLocks.Length > 0)
        {
            _worldScript.ConstructionManager.LockConstructionButtons(_constructionLocks);
        }
        if (_timeScaleWhenActivated < 1.0f)
        {
            _worldScript.GameplayScript.SetTargetTimeScale(_timeScaleWhenActivated, _slowMotionTransitionTime);
        }

        _worldScript.GameplayScript.AddResources(_initialResourceGift);
        _bIsActivated = true;
    }

    public bool BIsActivated()
    {
        return _bIsActivated;
    }
}
