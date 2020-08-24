using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventFormationScript : EventParentObject
{
    public GameObject _squadLeader;             // The formation's leader

    public GameObject[] _soldiers;              // The soldier units within the formation
    /// <summary>
    /// The spawn positions and also the soldier units' positions within the formation. NOTE: ONLY ranged units in this formation.
    /// </summary>
    private Transform[] _soldiersSpawnPos;       
	private GameObject _target;
    // Use this for initialization
	public override void Start ()
    {
        base.Start();

        _soldiersSpawnPos = new Transform[_soldiers.Length];
        // Instantiate the squad leader unit
        if (_squadLeader != null)
        {
            _squadLeader = SpawnUnit(_squadLeader, gameObject.transform);
            
            // Set the current formation script object to the squad leader's child to have the _soldierSpawnPos follow the leader's position
            //gameObject.transform.parent = _squadLeader.transform;
        }
       
        // Instantiate soldiers
        if (_soldiersSpawnPos.Length == transform.childCount)
        {
            // Acquire the spawn positions (each unit's position within the formation) from child transforms
            for (int i = 0; i < _soldiersSpawnPos.Length; i++)
            {
                _soldiersSpawnPos[i] = transform.GetChild(i);
            }
        }
        if (_soldiers.Length == _soldiersSpawnPos.Length)
        {
            for (int i = 0; i < _soldiers.Length; i++)
            {
                if (_soldiers[i] != null)
                {
                    _soldiers[i] = SpawnUnit(_soldiers[i], _soldiersSpawnPos[i]);
                }
            }
        }

        //
        SetInitialDirectives();
    }
	
	// Update is called once per frame
	public override void Update ()
    {
        base.Update();
        
        ManageTransform();

        Debug.DrawLine(transform.position, transform.forward * 0.1f);
    }

    public void OnDestroy()
    {
        // Change the soldiers back to just following the train
        for (int i = 0; i < _soldiers.Length; i++)
        {
            if (_soldiers[i] != null)
            {
                AIDynamicObjScript soldierAIScript = _soldiers[i].GetComponent<AIDynamicObjScript>();
                soldierAIScript.SetAITask(AITask.FollowTrain);
            }
        }
    }

    protected virtual void ManageTransform()
    {
        transform.position = _squadLeader.transform.position;
        if (_target != null)
            transform.rotation = Quaternion.LookRotation(_target.transform.position - _squadLeader.transform.position);
    }

    /// <summary>
    /// Change the initial state of the soldier units and set their follow targets to their positions within the formation
    /// </summary>
    protected override void SetInitialDirectives()
    {
        // Change soldiers' states and set their follow targets to the _soldierSpawnPos transform positions
        for (int i = 0; i < _soldiers.Length; i++)
        {
            AIDynamicObjScript soldierAIScript = _soldiers[i].GetComponent<AIDynamicObjScript>();

            soldierAIScript.SetAITask(AITask.StayInFormation);
            soldierAIScript.SetFollowTarget(_soldiersSpawnPos[i]);
        }
    }

    public virtual void SetTarget(GameObject target)
    {
        _target = target;
        foreach (GameObject soldier in _soldiers)
            soldier.GetComponent<AIDynamicObjScript>().AssignTarget(target);
        _squadLeader.GetComponent<AIDynamicObjScript>().AssignTarget(target);
    }
}
