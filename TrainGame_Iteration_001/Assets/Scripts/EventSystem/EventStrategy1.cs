using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventStrategy1 : EventParentObject
{
    // Array of spawn points to be used
    [SerializeField]
    private EnemySpawnObjectScript[] _spawnPoints;
    private List<GameObject> _enemies = new List<GameObject>();

    //Timer for spawn delay
    public float[] _spawnDelayTimer;
	private float _currentTimer;
	//Indicates whether the second wave has been spawned to stop spawning
	//private bool _bSecondWaveSpawned = false; // Never used ?
	private int _wavesSpawned = 0;
	public override void Start()
	{
		base.Start();
	}

	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();
        // Increment the timer
        if (_bIsActivated && _bIsInitialized)
        {
            _currentTimer += Time.deltaTime;

            // 
            if (_wavesSpawned < _spawnPoints.Length)
            {
                if (_currentTimer >= _spawnDelayTimer[_wavesSpawned])
                {
                    EnemySpawnObjectScript spawnPoint = _spawnPoints[_wavesSpawned];

                    if (spawnPoint != null)
                    {
                        _enemies.AddRange(spawnPoint.StartSpawning(false));
                    }
                    _wavesSpawned++;
                    // Reset the timer
                    _currentTimer = 0;
                }
            }
        }
}



	protected override bool CheckVictoryCondition()
	{
        bool result = false;
        if (_wavesSpawned >= _spawnPoints.Length)
        {
            GameObject[] waveCheck = _enemies.Where<GameObject>(g => g != null).ToArray();
            result = waveCheck.Length == 0;
        }
        // Check if all the enemy units spawned by this event has been destroyed
        return result;
	}


}
