using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConsMode
{
	CollectResources,
	Construct,
	Recycle
}

public class AIConsDroneScript : AIFXDroneScript
{
	private int _testPathingDestNum;

	private GameObject _currConsSite = null;
	private GameObject _currRecySite = null;

	private float _constructDistXZ;
	private float _constructDistY;

	private Vector3 _lastConsSitePos;
	private Vector3 _lastSiloPos;
	//private Vector3 _lastRecySitePos;

	//private float _consResGiveTime;
	//private float _consResGiveTime_curr;

	private float _buildTime;
	private float _recyTime;

	private int _framesSinceWasConstructing = 0;
	private int _framesSinceRetrievingResources = 0;

	private ConsMode _currConsMode;

	//

	//private Vector3 _lastLocoPos;

	private float _pickupDistXZ;
	private float _pickupDistY;

	private float _retrieveResTime;
	private float _retrieveResTime_curr;

	public GameObject _constructionMenu;

	public override void Start()
	{
		_currConsMode = ConsMode.Construct;

		_specFX_archetype = Resources.Load("FX/ArcBeam001") as GameObject;
		_specFX_archetype_alt = Resources.Load("FX/ArcBeam002") as GameObject;

		base.Start();
        

		_team = Team.Friendly; // All cons drones must be friendlies (for now at least)

		_maxFlightSpeed = 0.5f; // Now much lower due to MoveToDestination() not setting velocity

		_rotationRate = 4.0f;

		//

		_constructDistXZ = 1.0f;
		_constructDistY = 6.0f; // 1.0f until 13-4-18

		_lastConsSitePos = Vector3.zero;
		//_lastLocoPos = Vector3.zero;
		_lastSiloPos = Vector3.zero;

		//

		_maxHeldResources = 15;
		_heldResources = 15; // 0 until 31-5-18

		//_consResGiveTime = 0.2f; // 0.5f until 17-4-18 @ 00:53
		//_consResGiveTime_curr = 0.0f;

		_buildTime = 2.0f; // 2.0f;
		_recyTime = 2.0f; // 2.0f;

		//

		_pickupDistXZ = 1.0f;
		_pickupDistY = 3.0f; // 1.0f until 14-4-18

		_retrieveResTime = 0.1f; // Per-resource
		_retrieveResTime_curr = 0.0f;

		_currSilo = null;
	}

	public new void FixedUpdate()
	{
		if (PauseMenu.isPaused) return;

		  base.FixedUpdate();

		  if (_framesSinceWasConstructing > 2)
		{
			//if (_consResGiveTime_curr != 0.0f) _consResGiveTime_curr = 0.0f;
		}
		++_framesSinceWasConstructing;

		if (_framesSinceRetrievingResources > 2)
		{
			_retrieveResTime_curr = 0.0f; // Reset
		}
		++_framesSinceRetrievingResources;

		//
		//

		if (_currConsMode != ConsMode.Recycle && _currRecySite != null) // Not in recycle mode but there is still a recy site
		{
			_currRecySite = null;
		}

		if (_currRecySite != null && _currRecySite.GetComponent<RecySite>().Team != _team) // Recy site not on the same team
		{
			_currRecySite = null;
		}

		//
		//

		if (_currConsMode == ConsMode.CollectResources)
		{
			if (_currConsSite != null)
			{
				_currConsSite.GetComponent<ConsSite>().CurrDrone = null;
				_currConsSite = null;
				_lastConsSitePos = Vector3.zero;
				//_lastLocoPos = Vector3.zero;
			}

			//ReturnToRetrieveResources();
			//CheckForRetrieveResources();

			if (_heldResources >= _maxHeldResources || _worldScript.GameplayScript.PlayerResources <= 0)
			{
				_currConsMode = ConsMode.Construct;
			}
		}
		else if (_currConsMode == ConsMode.Construct)
		{
			if (_currSilo != null)
			{
				_currSilo.GetComponent<ResourceModule>().CurrDrone = null;
				_currSilo = null;
				_lastSiloPos = Vector3.zero;
			}

			//if (_worldScript.gameType == GameType.TrainChase) AutoFindConsSite(); // Cons sites are manually set in RTS mode from the CommandScript
			if (_currConsSite != null) FollowConsSite();
			CheckForConstruct();

			if (_heldResources == 0)
			{
				_currConsMode = ConsMode.CollectResources;
			}
		}
		else if (_currConsMode == ConsMode.Recycle)
		{
			if (_currRecySite != null)
			{
				FollowRecySite();
				CheckForRecycle();
			}
			else
			{
				_currConsMode = ConsMode.Construct;
			}
		}

		ManageConstruction();

		// End of Update()
	}

	//protected override void ManageRotation()
	//{
	//	//? Offline for now (restored 2-4-18 @ 16:09)
	//	// TODO: Make it so AIDynamicObjScript's ManageRotation() doesn't cause this drone class to go haywire
	//	//base.ManageRotation();
	//}

	public override void AITask_GoTo()
	{
		if (_currConsSite != null) _currConsSite = null;
		if (_currRecySite != null) _currRecySite = null;

		base.AITask_GoTo();
	}

	public override void AITask_Follow()
	{
		if (_currConsSite != null) _currConsSite = null;
		if (_currRecySite != null) _currRecySite = null;

		base.AITask_Follow();
	}

	public override void AITask_Attack()
	{
		// Deactivate base method as we're not an attack unit
	}

	public override void AITask_Construct()
	{
		if (_currConsSite == null)
		{
			_currAITask = AITask.Idle;
		}
	}

	public override void AITask_Recycle()
	{
		if (_currRecySite == null)
		{
			_currAITask = AITask.Idle;
		}
	}

	public void SetConsSite(GameObject csite)
	{
		if (csite == null) return;

		_currConsSite = csite;

		csite.GetComponent<ConsSite>().CurrDrone = gameObject;
	}

	/// <summary>
	/// Looks for a cons-site to work on
	/// If it finds one, send the drone to hover over it
	/// If not, just resume following the train
	/// This is not relevant if the current cons site has already been set remotely [Mike, 4-6-18]
	/// </summary>
	private void AutoFindConsSite()
	{
		if (_currConsSite == null)
		{
			_lastConsSitePos = Vector3.zero;
			//_lastLocoPos = Vector3.zero;
			_lastSiloPos = Vector3.zero;

			FlightAltitude_Curr = FlightAltitude_Regular;

			if (_worldScript.RandomisationScript.Get_RandTime005_AvailableThisTurn()) // Every 0.1-0.3 seconds
			{
				ConsSite[] allCSites = (ConsSite[])FindObjectsOfType(typeof(ConsSite));
				if (allCSites.Length > 0)
				{
					List<GameObject> csgos = new List<GameObject>();
					for (int i = 0; i < allCSites.Length; ++i)
					{
						if (allCSites[i].CurrDrone == null)
						{
							csgos.Add(allCSites[i].gameObject);
						}
					}

					if (csgos.Count > 0)
					{
						GameObject csite = BBBStatics.GetClosestGOFromListToVec(csgos, transform.position);
						SetConsSite(csite);

						//_currConsSite.GetComponent<ConsSite>().CurrDrone = gameObject;
					}
				}

				//

				if (_currConsSite == null) // Couldn't find a cons site - just follow the train
				{
						if (_worldScript.RandomisationScript.Get_RandTime003_AvailableThisTurn())
						{
							_testPathingDestNum = BBBStatics.RandInt(0, _worldScript.GameplayScript.RandOffsetsFromLoco.Count - 2);
						}
						_pathingDestination = _worldScript.GameplayScript.RandOffsetsFromLoco[_testPathingDestNum];
				}
			}
		}
	}

	/// <summary>
	/// Go to the cons site
	/// </summary>
	public void FollowConsSite()
	{
		//Debug.DrawLine(transform.position, _currConsSite.transform.position, Color.green, Time.deltaTime);

		_pathingDestination = _currConsSite.transform.position;

		TurretScriptParent tsp = _currConsSite.GetComponent<ConsSite>().Archetype.GetComponent<TurretScriptParent>();
		if (tsp != null) _worldAltitudeOverride = _currConsSite.transform.position.y + tsp.TurretHeight + 6.0f;
		else _worldAltitudeOverride = _currConsSite.transform.position.y + 6.0f;

		//

		bool bMatchSpeed = true;
		if (bMatchSpeed && _lastConsSitePos != Vector3.zero)
		{
			// Directly match speed with the train (actually the cons site) - just like the camera

			Vector3 diff = _currConsSite.transform.position - _lastConsSitePos;

			//transform.Translate(diff); // Takes rotation into account
			transform.position += diff;
		}

		//

		_lastConsSitePos = _currConsSite.transform.position;
	}

	/// <summary>
	/// Check if we're in range of our target cons-site so we can begin to work on it
	/// </summary>
	private void CheckForConstruct()
	{
		if (_currConsSite != null)
		{
			// 2D distance (2-4-18 @ 18:08)
			Vector2 v1 = new Vector2(transform.position.x, transform.position.z);
			Vector2 v2 = new Vector2(_currConsSite.transform.position.x, _currConsSite.transform.position.z);

			float distXZ = Vector2.Distance(v1, v2);
			float distY = float.PositiveInfinity;

			TurretScriptParent tsp = _currConsSite.GetComponent<ConsSite>().Archetype.GetComponent<TurretScriptParent>();
			if (tsp != null) distY = Mathf.Abs(transform.position.y - (_currConsSite.transform.position.y + tsp.TurretHeight + 6.0f));
			else distY = Mathf.Abs(transform.position.y - (_currConsSite.transform.position.y + 6.0f));

			//print("distXZ: " + distXZ);
			//print("distY: " + distY);

			if (distXZ <= _constructDistXZ && distY <= _constructDistY)
			{
				Construct();
			}
		}
	}

	private void Construct()
	{
		_framesSinceWasConstructing = 0;

		ConsSite cs = _currConsSite.GetComponent<ConsSite>();

		//if (_worldScript.Get_RandTime004_AvailableThisTurn())
		//{
		//	cs.PercentComplete += 15;
		//}

		//_consResGiveTime_curr += Time.deltaTime;

		//if (_consResGiveTime_curr >= _consResGiveTime)
		//{
		//	//cs.AddResourcesToCSite(1);
		//	_consResGiveTime_curr = 0.0f;

		//	cs.PercentComplete += 10;

		//	//_heldResources -= 1;
		//}

		//

		cs.PercentComplete += (Time.deltaTime / _buildTime) * 100.0f;

		//Debug.DrawLine(transform.position, _currConsSite.transform.position, Color.cyan, Time.deltaTime);

		List<Vector3> tempFXList = new List<Vector3>();
		for (int i = 0; i < _desiredSpecFXCount; ++i)
		{
			_specFX_vertIdx[i] += 3;

			Vector3 vertPos = BBBStatics.GetVert(_specFX_vertIdx[i], cs.LocalSpaceVertices, cs.transform);
			tempFXList.Add(vertPos);
		}

		if (_specFX_archetype_curr != _specFX_archetype_orig)
		{
			// Switch to the original FX
			InitialiseFXGO(_specFX_archetype_orig);
			_specFX_archetype_curr = _specFX_archetype_orig;
		}

		InitialiseFXAudio(_fxAudioClip1);

		RunFX(tempFXList);
	}

	//

	public void SetRecySite(GameObject rsite)
	{
		//if (rsite == null) return;

		_currRecySite = rsite;

		rsite.GetComponent<ConsSite>().CurrDrone = gameObject;
	}

	public void FollowRecySite()
	{
		Debug.DrawLine(transform.position, _currRecySite.transform.position, Color.red, Time.deltaTime);

		_pathingDestination = _currRecySite.transform.position;

		_worldAltitudeOverride = _currRecySite.transform.position.y + 6.0f;

		//_lastRecySitePos = _currRecySite.transform.position;
	}

	private void CheckForRecycle()
	{
		if (_currRecySite != null)
		{
			Vector2 v1 = new Vector2(transform.position.x, transform.position.z);
			Vector2 v2 = new Vector2(_currRecySite.transform.position.x, _currRecySite.transform.position.z);
			float distXZ = Vector2.Distance(v1, v2);
			//float distY = float.PositiveInfinity;

			if (distXZ <= _constructDistXZ) // && distY <= _constructDistY)
			{
				if (!_currRecySite.activeInHierarchy) _currRecySite.SetActive(true);

				Recycle();
			}
			else
			{
				if (_currRecySite.activeInHierarchy) _currRecySite.SetActive(false);
			}
		}
	}

	private void Recycle()
	{
		RecySite rs = _currRecySite.GetComponent<RecySite>();

		rs.PercentComplete += (Time.deltaTime / _recyTime) * 100.0f;

		if (rs.LocalSpaceVertices.Count > 0) // Will be 0 until post update is run on the recy site -- this avoids an out of bounds exception freeze-crash during the first frame
		{
			List<Vector3> tempFXList = new List<Vector3>();
			for (int i = 0; i < _desiredSpecFXCount; ++i)
			{
				_specFX_vertIdx[i] += 3;

				Vector3 vertPos = BBBStatics.GetVert(_specFX_vertIdx[i], rs.LocalSpaceVertices, rs.transform);
				tempFXList.Add(vertPos);
			}

			if (_specFX_archetype_curr != _specFX_archetype_alt)
			{
				// Switch to the alternative FX (red beams)
				InitialiseFXGO(_specFX_archetype_alt);
				_specFX_archetype_curr = _specFX_archetype_alt;
			}

			InitialiseFXAudio(_fxAudioClip2);

			RunFX(tempFXList);
		}
	}

	//
	//
	//
	//

	/// <summary>
	/// Offline as of 31-5-18
	/// </summary>
	private void ReturnToRetrieveResources()
	{
		if (_currSilo == null) _currSilo = GetNearestUnclaimedResourceSilo();

		if (_currSilo != null)
		{
			if (_currSilo.GetComponent<ResourceModule>().CurrDrone != gameObject) _currSilo.GetComponent<ResourceModule>().CurrDrone = gameObject;

			//_pathingDestination = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().ResourceDropOffPoint.transform.position;
			_pathingDestination = _currSilo.transform.position;

			_worldAltitudeOverride = _pathingDestination.y + 6.0f;

			//

			// Directly match speed with the train (actually _currSilo)
			if (_lastSiloPos != Vector3.zero)
			{
				transform.position += (_currSilo.transform.position - _lastSiloPos);
			}
			_lastSiloPos = _currSilo.transform.position;
		}
		else
		{
			// Couldn't find a silo - just follow the train
			if (_worldScript.RandomisationScript.Get_RandTime003_AvailableThisTurn())
			{
				_testPathingDestNum = BBBStatics.RandInt(0, _worldScript.GameplayScript.RandOffsetsFromLoco.Count - 2);
			}
			_pathingDestination = _worldScript.GameplayScript.RandOffsetsFromLoco[_testPathingDestNum];
		}
	}

	/// <summary>
	/// Offline as of 31-5-18
	/// </summary>
	private void CheckForRetrieveResources()
	{
		if (_currSilo != null)
		{
			//Vector3 retrievePos = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().ResourceDropOffPoint.transform.position;
			Vector3 retrievePos = _currSilo.transform.position;

			// 2D distance
			Vector2 v1 = new Vector2(transform.position.x, transform.position.z);
			Vector2 v2 = new Vector2(retrievePos.x, retrievePos.z);

			float distXZ = Vector2.Distance(v1, v2);
			float distY = Mathf.Abs(transform.position.y - (retrievePos.y + 6.0f));

			/////
			//print("distXZ: " + distXZ);
			//print("_pickupDistXZ: " + _pickupDistXZ);

			//print("distY: " + distY);
			//print("_pickupDistY: " + _pickupDistY);
			/////

			if (distXZ <= _pickupDistXZ && distY <= _pickupDistY)
			{
				RetrieveResources();
			}
		}
	}

	/// <summary>
	/// Offline as of 31-5-18
	/// </summary>
	private void RetrieveResources()
	{
		_framesSinceRetrievingResources = 0;

		if (_retrieveResTime_curr < _retrieveResTime)
		{
			_retrieveResTime_curr += Time.deltaTime;

			//

			//GameObject rdp = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().ResourceDropOffPoint;
			GameObject rdp = _currSilo.transform.Find("ResourceDropOffPoint").gameObject;

			MeshFilter mf = rdp.GetComponent<MeshFilter>();
			if (mf != null)
			{
				List<Vector3> tempFXList = new List<Vector3>();
				for (int i = 0; i < _desiredSpecFXCount; ++i)
				{
					_specFX_vertIdx[i] += 3;

					Vector3 vertPos = BBBStatics.GetVert(_specFX_vertIdx[i], new List<Vector3>(mf.mesh.vertices), rdp.transform);
					tempFXList.Add(vertPos);
				}

				if (_specFX_archetype_curr != _specFX_archetype_alt)
				{
					// Switch to the alternative FX
					InitialiseFXGO(_specFX_archetype_alt);
					_specFX_archetype_curr = _specFX_archetype_alt;
				}

				InitialiseFXAudio(_fxAudioClip2);

				RunFX(tempFXList);
			}
		}
		else
		{
			//_worldScript.AddResources(_heldResources);
			//_heldResources = 0;

			_worldScript.GameplayScript.SubtractResources(1);
			_heldResources += 1;

			_retrieveResTime_curr = 0.0f;
		}
	}

	public GameObject CurrConsSite
	{
		get { return _currConsSite; }
		set { _currConsSite = value; }
	}

	public GameObject CurrRecySite
	{
		get { return _currRecySite; }
		set { _currRecySite = value; }
	}

	public ConsMode CurrConsMode { get { return _currConsMode; } set { _currConsMode = value; } }

	private void ManageConstruction()
	{
		//if (_bIsSelected && !_worldScript.ConstructionManager.bIsConsMenuOpen)
		//{
		//	_worldScript.ConstructionManager.OpenConstructionMenu_BBB(this);
		//}
	}
}
