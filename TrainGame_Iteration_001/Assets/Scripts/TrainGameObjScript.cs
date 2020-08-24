using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ConsIconPack
{
    public Sprite mainIcon;
    public Sprite altIcon;
    public Sprite lockedIcon;

    public ConsIconPack(Sprite main, Sprite alt, Sprite locked)
    {
        mainIcon = main;
        altIcon = alt;
        lockedIcon = locked;
    }
}

public enum Team
{
	Friendly,
	Neutral,
	Enemy
}

public enum OrdnanceType
{
	Ballistics = 0,
	AntiArmor = 1,
	Explosive = 2,

	Count = 3
}

/// <summary>
/// Type of Armor. Could be made into a dictonary later on depending on the design
/// </summary>
public enum ArmorType
{
	Light = 0,
	Heavy = 1,

	Count = 2
}

public enum TargetType
{
    Ground,
    Air
}

public enum DamageLevel
{
	None = 0,
	Light = 1,
	Heavy = 2,
	Critical = 3
}

public class TrainGameObjScript : MonoBehaviour
{
	[Header("TrainGameObjScript Values")]

	public int _currentHealth = 100;                    // Current health value
	public int _maxHealth = 100;                        // Maximum health value.  -1 for invincible
	public int _maxArmor = 100;
	public int _currentArmor = 100;
	public ArmorType _armorType = ArmorType.Light;
	protected DamageLevel _damageLevel = DamageLevel.None;
	protected DamageLevel _damageLevel_last = DamageLevel.None;
    [SerializeField]
    protected TargetType _targetType;
	public int _buildCost = 20;                         // Resource cost required to build
	public string _name = "";                     // Name of the object

	//public float _buildTime = 0.0f;                   // Time Required to build // Now determined by the _buildCost
	public Team _team;                                  // The team this object belongs to
	public GameObject _ourCollidingMeshObj = null;      // Colliding mesh object
	protected GameObject _commSocketObj = null;         // Comm Socket Object -- position of the centre of the model
	public List<GameObject> _customChunks;              // Make sure they are each GOs with rigidbodies
    public bool _bShowPositionIndicator = false;
    public bool _bCanBeSelectedByPlayer = false;        // True if selectable by the player
	public bool _bCanBeTargetedByPlayer = true;         // True if targettable by the player
	public bool _bCanBeAITarget = true;                 // True if can be targetted by AI enemies
    [Header("Construction icons")]
    public GameObject _consIcon_GO;
    public Sprite _consIcon = null;                     // Construction icon model sprite
    [SerializeField]
    private ConsIconPack _iconPack;
    public GameObject _explosion = null;                // Explosion Object
	public AudioClip _explosionSound = null;            // Explosion sound

	protected bool _bIsSelected;                        // True if currently selected
	protected WorldScript _worldScript;                 // The object holding the world script

	protected int _randomValue1;

	protected MeshFilter _meshFilter;
	protected MeshRenderer _meshRenderer;
	protected Mesh _mainMesh;

	protected bool _bIsChunk = false;
	protected bool _bIsMainChunk = false;
	protected bool _bIsSmallChunk = false;

	protected bool _bSpawnMainChunk = true;
	public int _defaultChunksCount = 10;

	protected bool _bDamageAmntFrozen = false;

	protected Vector3 _pending_lastTFormPos_posPred;
	protected Vector3 _ourVelocity = Vector3.zero;

	protected float _timeSinceInstantiated;

	protected bool damaged;

	//
	[SerializeField]
	private string _description = string.Empty;
    


	public GameObject _healthBarPrefab;
	private StatusOverlayObj _healthBarObjScript;

	private SelectionRingObjScript _selectionRing;


	protected Vector3 _lastTFormPos_posPred;

	protected List<GameObject> _damageSmokeHardpoints = new List<GameObject>();
	protected List<GameObject> _damageSmokeObjs = new List<GameObject>();

    [Header("In-Tutorial control")]
    [SerializeField]
    private int[] _availableInTutorials = new int[0];
	//

	public virtual void Start()
	{
		SetAllChildrenToMatchTeam(); // Make sure that all children are on our team [Mike, 1-6-18]


		//_team = Team.Neutral;
		_randomValue1 = BBBStatics.RandInt(0, 10);

		_timeSinceInstantiated = 0.0f;

		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();

		if (_worldScript == null) print("Error: TGOScript Couldn't find worldScript");

		//

		if (_commSocketObj == null && transform.Find("CommSocket") != null)
		{
			_commSocketObj = transform.Find("CommSocket").gameObject; // Automatically find the commsocket if it has not been manually set
		}
		else
		{
			//print("Error: _commSocketObj is missing: " + name);

			// Create a commSocket in the centre of the object if there isn't one already -- may not be as accurate as a manually placed one

			Renderer r = GetComponent<Renderer>();
			if (r != null)
			{
				_commSocketObj = new GameObject("CommSocket_Temp");
				_commSocketObj.transform.parent = transform;

				_commSocketObj.transform.position = r.bounds.center;
			}
		}

		//

		GetWorldScript().AddToAllTGOsInWorld(gameObject);

		_bIsSelected = false;

		if (GetComponent<MeshFilter>() != null)
		{
			_meshRenderer = GetComponent<MeshRenderer>();
			_meshFilter = GetComponent<MeshFilter>();

			if (_meshFilter.mesh != null)
			{
				_mainMesh = _meshFilter.mesh;
				//if (_mainMesh == null) print("Error: _mainMesh == null -- TGO");
			}
		}

		//

		_pending_lastTFormPos_posPred = Vector3.zero;

		if (_commSocketObj != null) _lastTFormPos_posPred = _commSocketObj.transform.position;
		else _lastTFormPos_posPred = transform.position;

		// In case our health/armour values are out of bounds
		_currentArmor = Mathf.Clamp(_currentArmor, 0, _maxArmor);
		_currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

		if (_maxHealth > 0 && _healthBarPrefab != null)
		{
			 GameObject healthBar = Instantiate(_healthBarPrefab, GameObject.Find("MainHolder").transform.Find("HUDCanvas").gameObject.transform);
			_healthBarObjScript = healthBar.GetComponent<StatusOverlayObj>();
			_healthBarObjScript.Initialize(this, gameObject);
		}

		if (_bCanBeSelectedByPlayer || _bCanBeTargetedByPlayer)
		{
			_selectionRing = Instantiate((GameObject)Resources.Load("Prefabs/UI/ExperimentalSelectionRing"), gameObject.transform.position, Quaternion.Euler(90, 0, 0), gameObject.transform).GetComponent<SelectionRingObjScript>();

		}

		if (_team == Team.Enemy)
		{
			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._enemiesSpawnedCount++;

			_worldScript.Enemies.Add(this);
		}

		//

		int cc = transform.childCount;
		for (int i = 0; i < cc; ++i)
		{
			if (transform.GetChild(i).gameObject.name.Contains("smoke"))
			{
				_damageSmokeHardpoints.Add(transform.GetChild(i).gameObject);
			}
		}

		if (_damageSmokeHardpoints.Count == 0 && _commSocketObj != null)
		{
			_damageSmokeHardpoints.Add(_commSocketObj);
		}

		//

		// End of Start
		}

	protected virtual void OnDestroy()
	{
	}

	public virtual void Update()
	{
		if (PauseMenu.isPaused) return;

		_timeSinceInstantiated += Time.deltaTime;

		ManageDamageSmoke();


		CheckOurVelocity();

		//if (GetWorldScript() == null)
		//{
		//	Debug.Log("Wat");
		//}
		/////
		//if (GetComponent<TurretScriptParent>() == null)
		//{
		//	Vector3 p = PositionPredictor(10.0f); /// TEST

		//	Debug.DrawLine(transform.position, p, Color.yellow, Time.deltaTime);
		//	Debug.DrawLine(p, p + new Vector3(0, 200, 0), new Color(0, 0, 1, 1), Time.deltaTime);
		//}
		/////
	}

	public void LateUpdate()
	{
		if (PauseMenu.isPaused) return;

		//if (_commSocketObj != null) _lastTFormPos_posPred = _commSocketObj.transform.position;
		//else _lastTFormPos_posPred = transform.position;

		if (_pending_lastTFormPos_posPred != Vector3.zero)
		{
			_lastTFormPos_posPred = _pending_lastTFormPos_posPred;
			_pending_lastTFormPos_posPred = Vector3.zero;
		}
	}



	public void Damage_HealthOnly(int amount, OrdnanceType damageType)
	{
		_currentHealth -= amount;
		_currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
	}

	public virtual void Damage_Additive(OrdnanceType ordnanceType, int amount, int bonus, bool bIsDirectTarget)
	{
		if (!_bDamageAmntFrozen && _maxHealth > 0)
		{
			// Deplete Armor (while armor feature has not been removed yet)
			if (_currentArmor > 0)
				_currentArmor = 0;

			int resultAmount = amount;
			// Check if the ordnance type has any bonus against this TGO's armor type
			if (OrdnanceBonusCheck(ordnanceType, bIsDirectTarget))
				resultAmount += bonus;
			// Check Armor type. Reduce damage if the armor type is Heavy
			if (_armorType == ArmorType.Heavy)
				resultAmount -= 1;
			_currentHealth = _currentHealth - resultAmount;
			_currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
			if (_healthBarObjScript != null)
				_healthBarObjScript.UpdateState();
			CheckHealth();

			DamageStats(resultAmount);
		}
	}

	private void DamageStats(int damage)
	{
		if (_worldScript == null) return;
		if (_worldScript.StatsRec == null) return;

		_worldScript.StatsRec.Initiate(); // Make sure it has been initiated

		if (_team == Team.Enemy) _worldScript.StatsRec.Rec.CurrPt._overallDamageInflicted += damage;
		if (_team == Team.Friendly) _worldScript.StatsRec.Rec.CurrPt._overallDamageTaken += damage;
	}

	/// <summary>
	/// Check if current health is below 0 to begin the destruction sequence.
	/// </summary>
	private void CheckHealth()
	{
		if (_maxHealth < 0) return; // Invincible

		_damageLevel_last = _damageLevel;

		float h = GetHealth0to1();

		if (h >= 1.0f) _damageLevel = DamageLevel.None;
		else if (h >= 0.5f) _damageLevel = DamageLevel.Light;
		else if (h >= 0.25f) _damageLevel = DamageLevel.Heavy;
		else _damageLevel = DamageLevel.Critical;

		//print("_damageLevel: " + _damageLevel);

		//

		if (_currentHealth <= 0)
		{
			BeginDestroy(true, true);
		}
	}

    public bool IsTargettableByTurretType(bool bTurretShootsGround, bool bTurretShootsAir)
    {
        if (_bCanBeAITarget)
        {
            if (_targetType == TargetType.Ground)
                return bTurretShootsGround;
            else if (_targetType == TargetType.Air)
                return bTurretShootsAir;
            else
                return false;
        } else
        {
            return false;
        }
    }

	/// <summary>
	/// Start the destruction sequence
	/// Create chunk objects
	/// Create explosion effects
	/// Play explosion sound effect
	/// </summary>
	public virtual void BeginDestroy(bool bRunExplosion, bool bSpawnChunks)
	{
		if (gameObject == null)
		{
			print("Error: gameObject is already null -- TGOScript -- BeginDestroy");
			return;
		}

		if (GetWorldScript() == null)
		{
			print("Error: worldScript is null -- TGOScript -- BeginDestroy");
			return;
		}

		if (gameObject.GetComponent<AIDroneScript>() != null && _selectionRing != null)
		{
			Destroy(_selectionRing.gameObject);
		}

		GetWorldScript().RemoveFromAllTGOsInWorld(gameObject);
		//RemoveAllChildrenFromAllTGOsInWorld(); // Must be called before explosion is run
		RemoveAllChildrenFromAllTGOsInWorld_T2(); // Must be called before explosion is run

		if (bRunExplosion)
		{
			Vector3 p = transform.position;
			if (_commSocketObj != null) p = _commSocketObj.transform.position;

			if (_bIsSmallChunk)
			{
				_worldScript.ExplosionScript.Explosion(p, Resources.Load("FX/Explosion002") as GameObject, null, 5, 1.0f, null, name, 0.2f);
			}
			else
			{
				_worldScript.ExplosionScript.Explosion(p, null, null, 10, 2.0f, null, name, 0.7f);
			}
		}

		if (bSpawnChunks) SpawnChunks();


		if (_bIsChunk)
		{
			GetWorldScript().AllChunks.Remove(gameObject);
		}

		// Stinger beeps/sounds for the destruction of certain types of object
		if (GetComponent<AIScavDroneScript>() != null) GetWorldScript().AS_2DMainAudioSource.PlayOneShot(GetWorldScript().WS_beep1);
		if (GetComponent<AIConsDroneScript>() != null) GetWorldScript().AS_2DMainAudioSource.PlayOneShot(GetWorldScript().WS_beep2);
		if (GetComponent<TurretScriptParent>() != null) GetWorldScript().AS_2DMainAudioSource.PlayOneShot(GetWorldScript().WS_beep3);
		if (GetComponent<CarriageScript>() != null && !GetComponent<CarriageScript>().bIsLocomotive) GetWorldScript().AS_2DMainAudioSource.PlayOneShot(GetWorldScript().WS_beep10);
		if (GetComponent<CarriageScript>() != null && GetComponent<CarriageScript>().bIsLocomotive) GetWorldScript().AS_2DMainAudioSource.PlayOneShot(GetWorldScript().WS_beep10);
 
		if (_team == Team.Enemy)
		{
			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._enemiesDestroyedCount++;

			_worldScript.Enemies.Remove(this);
		}

		//

		Destroy(gameObject);
	}


	/// <summary>
	/// Recursively remove all children from the worldScript's main TGO list
	/// WARNING: This is not effective if a member of the hierarchy-tree does not have a TGO
	/// </summary>
	public void RemoveAllChildrenFromAllTGOsInWorld()
	{
		for (int i = 0; i < transform.childCount; ++i)
		{
			Transform ct = transform.GetChild(i);
			TrainGameObjScript tgo = ct.gameObject.GetComponent<TrainGameObjScript>();

			if (tgo != null)
			{
				GetWorldScript().RemoveFromAllTGOsInWorld(tgo);

				tgo.RemoveAllChildrenFromAllTGOsInWorld(); // Recursion
			}
		}
	}

	/// <summary>
	/// Get all child objects and iterate over them to remove any which are TGOs from the worldScript's AllTGOsInWorld list
	/// This version should have no trouble with non TGO members of the hierarchy-tree
	/// </summary>
	public void RemoveAllChildrenFromAllTGOsInWorld_T2()
	{
		List<GameObject> allChildren = BBBStatics.GetAllChildObjs(gameObject);

		for (int i = 0; i < allChildren.Count; ++i)
		{
			TrainGameObjScript tgo = allChildren[i].GetComponent<TrainGameObjScript>();
			if (tgo != null)
			{
				GetWorldScript().RemoveFromAllTGOsInWorld(tgo);
			}
		}
	}

	/// <summary>
	/// Spawn 'destroyed' models which either crash and explode or become resources for scavenger drones
	/// </summary>
	private void SpawnChunks()
	{
		List<GameObject> allChunks = new List<GameObject>();

		if (!_bIsSmallChunk) // We're a full object or a main chunk // Small chunks can't have chunks or we'd get into an infinite loop
		{
			if (_bSpawnMainChunk && !_bIsChunk && _mainMesh != null) // Chunks can't have main chunks
			{
				

				GameObject mainChunk = Instantiate(_worldScript.DefaultChunkArchetype, transform.position, transform.rotation) as GameObject;

				TrainGameObjScript tgo = mainChunk.GetComponent<TrainGameObjScript>();

				tgo._bIsChunk = true;
				tgo._bIsMainChunk = true;

				tgo._defaultChunksCount = 5;
				mainChunk.GetComponent<MeshFilter>().mesh = _mainMesh;
				mainChunk.GetComponent<MeshRenderer>().materials = _meshRenderer.materials;
				mainChunk.transform.localScale = transform.localScale;

				mainChunk.GetComponent<ChunkScript>().SelfDestruct(7.0f);

				allChunks.Add(mainChunk);
			}

			for (int i = 0; i < _defaultChunksCount; ++i)
			{
				Mesh meshToUse = null;

				if (_worldScript._defaultChunkMeshes.Count > 0)
				{
					meshToUse = _worldScript._defaultChunkMeshes[Random.Range(0, _worldScript._defaultChunkMeshes.Count - 1)];
				}

				GameObject defaultChunk = Instantiate(_worldScript.DefaultChunkArchetype, transform.position,
					Quaternion.Euler(new Vector3(BBBStatics.RandFlt(0, 360), BBBStatics.RandFlt(0, 360), BBBStatics.RandFlt(0, 360)))) as GameObject;

				defaultChunk.GetComponent<TrainGameObjScript>()._bIsChunk = true;
				defaultChunk.GetComponent<TrainGameObjScript>()._bIsSmallChunk = true;

				defaultChunk.transform.localScale = new Vector3(BBBStatics.RandFlt(0.25f, 1.0f), BBBStatics.RandFlt(0.25f, 1.0f), BBBStatics.RandFlt(0.25f, 1.0f));

				if (meshToUse != null)
				{
					MeshFilter mf = defaultChunk.GetComponent<MeshFilter>();
					mf.mesh = meshToUse;

					MeshRenderer mr = defaultChunk.GetComponent<MeshRenderer>();

					if (_worldScript._defaultChunkMaterials.Count > 0)
					{
						mr.material = _worldScript._defaultChunkMaterials[BBBStatics.RandInt(0, _worldScript._defaultChunkMaterials.Count - 2)];
					}
				}

				allChunks.Add(defaultChunk);
			}

			for (int i = 0; i < _customChunks.Count; ++i)
			{
				if (_customChunks[i] != null)
				{
					GameObject customChunk = Instantiate(_customChunks[i], transform.position, transform.rotation) as GameObject;

					if (!customChunk.activeInHierarchy) customChunk.SetActive(true);

					customChunk.GetComponent<TrainGameObjScript>()._bIsChunk = true;
					customChunk.GetComponent<TrainGameObjScript>()._bIsSmallChunk = true;

					allChunks.Add(customChunk);
				}
			}

			for (int i = 0; i < allChunks.Count; ++i)
			{
				// Idea: Make it so chunk models will fly away from the centre of the destroyed object in the direction of their comm-socket from the comm-socket of the destroyed object ?

				Rigidbody rb = allChunks[i].GetComponent<Rigidbody>();

				float chunkVelForce = 10.0f;

				rb.velocity = new Vector3(
					BBBStatics.RandFlt(-chunkVelForce, chunkVelForce), // rb.velocity.x + Random.Range(-chunkVelForce, chunkVelForce),
					BBBStatics.RandFlt(-chunkVelForce, chunkVelForce), // rb.velocity.y + Random.Range(-chunkVelForce, chunkVelForce),
					BBBStatics.RandFlt(-chunkVelForce, chunkVelForce)); // rb.velocity.z + Random.Range(-chunkVelForce, chunkVelForce));

				rb.angularVelocity = new Vector3(
					BBBStatics.RandFlt(-chunkVelForce, chunkVelForce), // rb.angularVelocity.x + Random.Range(-chunkVelForce, chunkVelForce),
					BBBStatics.RandFlt(-chunkVelForce, chunkVelForce), // rb.angularVelocity.y + Random.Range(-chunkVelForce, chunkVelForce),
					BBBStatics.RandFlt(-chunkVelForce, chunkVelForce)); // rb.angularVelocity.z + Random.Range(-chunkVelForce, chunkVelForce));
			}
		}
	}

	/// <summary>
	/// Set this game object to "selected by player" state
	/// </summary>
	public void Select()
	{
		if (!_bCanBeSelectedByPlayer)
		{
			print("Error: Tried to select an object that can't be selected by the player");
		}

		if (_bCanBeSelectedByPlayer && !_worldScript.GameplayScript.IsObjPlayerSelected(gameObject))
		{
			_bIsSelected = true;
			_worldScript.GameplayScript.AddPlayerSelectedObj(gameObject);
		}
	}

	/// <summary>
	/// Deselect this game object
	/// </summary>
	public void Deselect()
	{
		//if (_worldScript.GameplayScript.IsObjPlayerSelected(gameObject))
		//{
		//	_bIsSelected = false;
		//	_worldScript.GameplayScript.RemovePlayerSelectedObj(gameObject);
		//}

		_bIsSelected = false;
	}

	/// <summary>
	/// Get bool value indicating whether this game object is selected
	/// </summary>
	public bool bIsSelected
	{
		get { return _bIsSelected; }
		//set { _bIsSelected = value; }
	}

	public bool bIsChunk
	{
		get { return _bIsChunk; }
	}

	public bool bIsSmallChunk
	{
		get { return _bIsSmallChunk; }
	}

	public bool bIsMainChunk
	{
		get { return _bIsMainChunk; }
	}

	public void SetMatchParentTeam()
	{
		if (transform.parent.gameObject != null)
		{
			GameObject parent = transform.parent.gameObject;

			TrainGameObjScript tgoParent = parent.GetComponent<TrainGameObjScript>();
			if (tgoParent != null)
			{
				if (_team != tgoParent._team)
				{
					_team = tgoParent._team;
				}
			}
		}
	}

	/// <summary>
	/// Set all of the children and childrens' children of this object's _team(s) to match our _team
	/// </summary>
	public void SetAllChildrenToMatchTeam()
	{
		List<Transform> allChildTFs = GetAllChildTForms();

		for (int i = 0; i < allChildTFs.Count; ++i)
		{
			TrainGameObjScript tgo = allChildTFs[i].gameObject.GetComponent<TrainGameObjScript>();
			if (tgo != null)
			{
				if (tgo._team != _team)
				{
					tgo._team = _team;
				}
			}
		}
	}

	public List<Transform> GetAllChildTForms() // [Mike, 1-6-18] -- Might be possible to replace with BBBStatics version
	{
		bool bFoundNewTForms = true;

		List<Transform> AllChildTransforms = new List<Transform> { gameObject.transform };

		while (bFoundNewTForms) // Get all child objects' transforms
		{
			bFoundNewTForms = false;
			for (int i = 0; i < AllChildTransforms.Count; ++i)
			{
				Transform t = AllChildTransforms[i];
                
                for (int j = 0; j < t.childCount; j++)
                {
                    if (!AllChildTransforms.Contains(t.GetChild(j)))
                    {
                        AllChildTransforms.Add(t.GetChild(j));
                        bFoundNewTForms = true;
                    }
                }
			}
		}

		return AllChildTransforms;
	}

	public WorldScript GetWorldScript()
	{
		return _worldScript;
	}

	public void CheckOurVelocity()
	{
		Vector3 cntPos = transform.position;
		if (_commSocketObj != null) cntPos = _commSocketObj.transform.position;

		_ourVelocity = cntPos - _lastTFormPos_posPred;
	}

	public Vector3 PositionPredictor(float SecondsAhead)
	{
		Vector3 cntPos = transform.position; if (_commSocketObj != null) cntPos = _commSocketObj.transform.position;

		//_ourVelocity = cntPos - _lastTFormPos_posPred;
		CheckOurVelocity();

		//! WARNING: THIS WAS BEING CALLED MULTIPLE TIMES EACH FRAME BY MANY FRIENDLIES -- THEREFORE _lastTFormPos_posPred WOULD ONLY NOT = cntPos ONCE PER FRAME!
		//_lastTFormPos_posPred = cntPos; // Store for next time // DON'T USE THIS!
		//! ////////

		// Only store it in the next frame so _lastTFormPos_posPred will remain the same until then -- we need the difference to be constant for the duration of the frame as 
		// PositionPredictor is called multiple times each frame by different attackers
		_pending_lastTFormPos_posPred = cntPos;

		return cntPos + (_ourVelocity * SecondsAhead);
	}

	public bool bDamageAmntFrozen
	{
		get { return _bDamageAmntFrozen; }
		set { _bDamageAmntFrozen = value; }
	}

	public float TimeSinceInstantiated
	{
		get { return _timeSinceInstantiated; }
	}

	public bool bSpawnMainChunk
	{
		get { return _bSpawnMainChunk; }
		set { _bSpawnMainChunk = value; }
	}

	public GameObject CommSocketObj
	{
		get { return _commSocketObj; }
	}

	public int BuildCost
	{
		get { return _buildCost; }
		set { _buildCost = value; }
	}

	public string GetParticleEffects(OrdnanceType ordnanceType)
	{
		string result = "VFX/";
		switch (ordnanceType)
		{
			case OrdnanceType.Ballistics:
				//result += ;
				break;
			case OrdnanceType.Explosive:
				break;
			case OrdnanceType.AntiArmor:
				break;
			default:
				break;
		}
		return result;
	}

	public bool OrdnanceBonusCheck(OrdnanceType ordnanceType, bool bIsDirectTarget)
	{
		bool result = false;

		switch (ordnanceType)
		{
			case OrdnanceType.Ballistics:
				result = false;
				break;
			case OrdnanceType.Explosive:
				if (bIsDirectTarget)
					result = true;
				else
					result = false;
				break;
			case OrdnanceType.AntiArmor:
				if (_armorType == ArmorType.Heavy)
					result = true;
				break;
			default:
				result = false;
				break;
		}

		return result;
	}

	public string Description
	{
		get
		{
			return _description;
		}
	}

	public int HitPoints
	{
		get
		{
			return _maxHealth;
		}
	}

	public float GetOverallHealth0to1()
	{
		float result = 0.0f;

		result += (float)_currentArmor / (float)_maxArmor;
		result += (float)_currentHealth / (float)_maxHealth;

		result /= 2;

		return result;
	}

	public float GetHealth0to1()
	{
		return (float)_currentHealth / (float)_maxHealth;
	}

	private void ManageDamageSmoke()
	{
		if (_damageSmokeHardpoints.Count == 0) return;

		if (_damageLevel != _damageLevel_last)
		{
			//while (_damageSmokeObjs.Count > 0)
			//{
			//	Destroy(_damageSmokeObjs[0]);
			//}

			for (int i = 0; i < _damageSmokeObjs.Count; ++i)
			{
				Destroy(_damageSmokeObjs[i]);
			}
			_damageSmokeObjs.Clear();

			_damageLevel_last = _damageLevel; // Make sure
		}

		if (_damageSmokeObjs.Count < _damageSmokeHardpoints.Count)
		{
			for (int i = 0; i < _damageSmokeHardpoints.Count; ++i)
			{
				//if (_damageLevel == DamageLevel.Light || _damageLevel == DamageLevel.None)
				//{
				//	_damageSmokeObjs.Add(Instantiate(_worldScript.DamageSmokeLvl1, _damageSmokeHardpoints[i].transform.position, Quaternion.identity, _damageSmokeHardpoints[i].transform));
				//}

				if (_damageLevel == DamageLevel.Heavy)
				{
					_damageSmokeObjs.Add(Instantiate(_worldScript.DamageSmokeLvl2, _damageSmokeHardpoints[i].transform.position, Quaternion.identity, _damageSmokeHardpoints[i].transform));
				}

				if (_damageLevel == DamageLevel.Critical)
				{
					_damageSmokeObjs.Add(Instantiate(_worldScript.DamageSmokeLvl3, _damageSmokeHardpoints[i].transform.position, Quaternion.identity, _damageSmokeHardpoints[i].transform));
				}
			}
		}
	}

    public ConsIconPack GetIconPack()
    {
        return _iconPack;
    }

    public void SetSelectableByPlayer(bool state)
    {
        _bCanBeSelectedByPlayer = state;
    }

    public bool CheckAvailableToTutorial(int tutorialValue)
    {
        bool result = false;
        for (int i = 0; i < _availableInTutorials.Length; i++)
        {
            if (_availableInTutorials[i] == tutorialValue)
            {
                result = true;
                break;
            }
        }

        return result;
    }
}
