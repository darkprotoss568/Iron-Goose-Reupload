using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class EnemySpawnTutorialEvent : EventParentObject
{
    RTSCameraController _cameraScript;
    [SerializeField]
    private EnemySpawnObjectScript _enemySpawn;
    private bool _spawned = false;
    GameObject[] _enemies;
	// Use this for initialization
	public override void Start ()
    {
        base.Start();
        
	}
	
	// Update is called once per frame
	public override void Update ()
    {
        if (_bIsActivated)
        {
            if (!_spawned)
            {
                // For now, only spawn ONE enemy per tutorial
                _enemies = _enemySpawn.StartSpawning(true).ToArray();
		//This shuold be looked into more, but for right now it's too glichy to have in the playtest.
                //_worldScript.RTSCameraController.SetViewTarget(_enemies[0]);
                _spawned = true;
            }
        }
        base.Update();
	}

    protected override bool CheckVictoryCondition()
    {
        GameObject[] waveCheck = _enemies.Where<GameObject>(g => g != null).ToArray();
        bool result = (waveCheck.Length == 0);
        return result;
    }
}
