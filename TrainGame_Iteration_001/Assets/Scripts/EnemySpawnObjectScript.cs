using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class EnemySpawnObjectScript: EventParentObject
{
    [SerializeField]
    private GameObject[] _units;
    private GameObject[] _spawnedUnits;
    private List<Transform> _spawnPos = new List<Transform>();
    public bool _hasSpawnAnimation = false;
    private bool _spawned = false;

	// Use this for initialization
	public void Awake ()
    {
        for (int i = 0; i < transform.childCount; i ++)
        {
            _spawnPos.Add(transform.GetChild(i));
        }
	_spawnedUnits = new GameObject[_units.Length];
    }
	
	// Update is called once per frame
	public override void Update ()
    {
        //base.Update();
	}

    protected override bool CheckVictoryCondition()
    {
        GameObject[] waveCheck = _spawnedUnits.Where<GameObject>(g => g != null).ToArray();
        bool result = (waveCheck.Length == 0);
        return result;
    }

    public List<GameObject> StartSpawning(bool destroySpawnScript)
    {
        int length = _units.Length;
        for (int i = 0; i < length; i++)
        {
            _spawnedUnits[i] = SpawnUnit(_units[i], _spawnPos[i]);

            if (_hasSpawnAnimation)
            {
                _spawnedUnits[i].SetActive(false);
            }
            else
            {
                _spawned = true;
            }
        }

        _bIsActivated = true;
        

        return new List<GameObject>(_spawnedUnits);
    }
}
