using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public partial class LocomotiveScript : CarriageScript
{
	public List<CarriageScript> CarriagesInOrder;
	public List<GameObject> CarriagesInOrderGameObjects;

	private bool bPostStartRun_Loco = false;

	private float DesiredSpeedAdjustmentRate;

	//private GameObject _resourceDropOffPoint;

	private bool _bPendingCheckCarriageOrder = false;

	private Vector3 _stationStopMidPoint;
	private Vector3 _entireTrainMidPoint;

	private GameObject _currStationStopEntryRail;
	private GameObject _currStationStopExitRail;

    private int prevCarriageCount = -1;
    private float prevCarriageLength = -1;

	private float _timeSinceStopped;

	//private float _stationStopTime = 25.0f;

	//private bool _bTrackBlockedAhead = false; // Can we start moving again after we stop at a station or an ambush?

	private bool _bWinConditionTriggered = false;
	private bool _bFailConditionTriggered = false;

    public List<EventDialogue> Result;

    new void Start()
	{
		//_maxHealth = 8000;
		//_currentHealth = 8000;

		//

		AccelDecelRate = 0.001f; // 0.05f;

		DesiredSpeedAdjustmentRate = 0.01f; // 0.05f;

		base.Start(); // Call the super-class function

		//DesiredMoveSpeed = _currentMoveSpeed; /// When the player wants control of the speed, this is the var we adjust
		DesiredMoveSpeed = 0.17f;

		_bIsLocomotive = true;

		CarriagesInOrder = new List<CarriageScript>();

		_bDamageAmntFrozen = true;

		//_resourceDropOffPoint = transform.Find("ResourceDropOffPoint").gameObject;
		//_resourceDropOffPoint = transform.Find("resourceModule_001").Find("ResourceDropOffPoint").gameObject;
		//if (_resourceDropOffPoint == null) print("Error: Couldn't find ResourceDropOffPoint -- LocomotiveScript @ Start()");

		_stationStopMidPoint = Vector3.zero;
		_entireTrainMidPoint = Vector3.zero;

		_currStationStopEntryRail = null;
		_currStationStopExitRail = null;

		_timeSinceStopped = 0.0f;

        //Result = FindObjectOfType<EventDialogue>;
        
		//Destroy(gameObject, 7.0f);
	}

	new void FixedUpdate()
	{
		if (PauseMenu.isPaused) return;

		// Call the superclass function
		base.FixedUpdate();

		CheckForGameWinCondition();
        CheckForMusicChange();

		//ManageSpeed();

		//if (_worldScript.Get_RandTime001_AvailableThisTurn())
		//	CheckCarriageOrder(); /// Expensive

		if (!bPostStartRun_Loco)
		{
			CheckCarriageOrder(null);
			bPostStartRun_Loco = true;
		}

		//ManageCarriages();

		if (CarriagesInOrder.Count > 0) _bDamageAmntFrozen = true; /// Locomotive is indestructable if it has carriages
		else _bDamageAmntFrozen = false;

		//print("CarriagesInOrder.Count: " + CarriagesInOrder.Count);

		CheckForEntireTrainMidPoint();
		CheckForStationStop();
		StopAtStation();

		//

		if (bPendingCheckCarriageOrder)
		{
			CheckCarriageOrder(null);

			bPendingCheckCarriageOrder = false;
		}

        if (_worldScript.TrainStopped)
        {
            _currentMoveSpeed = 0.0f;
        }
        else
        {
            _currentMoveSpeed = 0.17f;
        }
        /// Profiler said this was using a lot of resources
        //if (GameObject.FindObjectOfType<EventDialogue>() != null || FindObjectOfType<EventTutorial>() != null)
        //{
        //	_currentMoveSpeed = 0.0f;
        //}
    }

	void CheckForEntireTrainMidPoint()
	{
		if (CarriagesInOrder.Count > 0)
		{
			// The mid point between the front socket of the locomotive and the rear socket of the last carriage
			float p = 0.5f;
			_entireTrainMidPoint = _frontSocket.transform.position * p + (1 - p) * CarriagesInOrder[CarriagesInOrder.Count - 1].GetComponent<CarriageScript>().RearSocket.transform.position;
		}
		else
		{
			// Only the locomotive
			//_entireTrainMidPoint = _commSocketObj.transform.position;
			float p = 0.5f;
			_entireTrainMidPoint = _frontSocket.transform.position * p + (1 - p) * _rearSocket.transform.position;
		}

		//Debug.DrawLine(_entireTrainMidPoint, _entireTrainMidPoint + new Vector3(0, 300, 0), Color.magenta, Time.deltaTime);
	}

	void CheckForGameWinCondition()
	{
		if (_currentRail.GetComponent<RailScript>()._bIsWinConditionRail)
		{
			if (!_bFailConditionTriggered)
			{
				_bWinConditionTriggered = true;

				_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
				_worldScript.StatsRec.Rec.CurrPt._bWonGame = true;

				SceneManager.LoadScene("winMenu_alpha", LoadSceneMode.Single);
			}
		}
	}

    void CheckForMusicChange()
    {
        if (_currentRail.GetComponent<RailScript>()._bMakeMusicHectic)
        {
            _worldScript.GameplayScript._BIntenseMusic = true;
        }
    }

	void CheckForStationStop()
	{
		if (_currStationStopEntryRail == null) // We haven't begun to stop yet
		{
			if (_currentRail.GetComponent<RailScript>().bIsStationEntryRail)
			{
				_currStationStopEntryRail = _currentRail;

				GameObject currItrRail = _currentRail;
				for (int i = 0; i < 100; ++i) // Check up to the next 100 rail segments in front of us for an exit rail
				{
					currItrRail = currItrRail.GetComponent<RailScript>().GetRailFront(); // Move to the next rail in front

					if (currItrRail.GetComponent<RailScript>().bIsStationExitRail)
					{
						_currStationStopExitRail = currItrRail;

						float p = 0.5f;
						_stationStopMidPoint = _currStationStopEntryRail.GetComponent<RailScript>().GetBehindSocket().transform.position * p + (1 - p) *
							_currStationStopExitRail.GetComponent<RailScript>().GetFrontSocket().transform.position;

						//Debug.DrawLine(_stationStopMidPoint, _stationStopMidPoint + new Vector3(0, 300, 0), Color.white, 30.0f);

						return; // We've found our exit point
					}
				}

				_currStationStopEntryRail = null; // We didn't find an exit point -- don't keep the entry rail
			}
		}
	}

	void StopAtStation()
	{
		if (_currStationStopEntryRail != null && _currStationStopExitRail != null)
		{
			float distToStop = Vector3.Distance(_entireTrainMidPoint, _stationStopMidPoint);

			float distFromEntryToStop = Vector3.Distance(_currStationStopEntryRail.transform.position, _stationStopMidPoint);

			//

			float speedMultJustBeforeStop = 0.2f;
			float distBeforeStop = 2.0f;

			if (distToStop > 2.0f)
				_currentMoveSpeed_UberMult = BBBStatics.Map(distToStop, distFromEntryToStop, distBeforeStop, 1.0f, speedMultJustBeforeStop, true);
			else
				_currentMoveSpeed_UberMult = BBBStatics.Map(distToStop, distBeforeStop, 0.0f, speedMultJustBeforeStop, 0.0f, true);

			//

			//print("_currentMoveSpeed_UberMult loco: " + _currentMoveSpeed_UberMult);

			if (_currentMoveSpeed_UberMult < 0.05f)
			{
				// We've reached the station - stop the train

				_currentMoveSpeed = 0.0f;
				_currentMoveSpeed_UberMult = 0.0f; /// May not be necessary here
				_timeSinceStopped += Time.deltaTime;

				//if (!_worldScript.HUDScript.bIsShopMenuOpen)
				//{
				//	_worldScript.HUDScript.OpenShopMenu();
				//	if (!_worldScript.GameplayScript.bIsTrainStoppedAtStation)
				//	{
				//		_worldScript.GameplayScript.InitialTrainStopAction();
				//		_worldScript.GameplayScript.bIsTrainStoppedAtStation = true;
				//	}
				//	else
				//	{
				//		_worldScript.HUDScript.OpenShopMenu();
				//	}
				//}

				//if (!_bTrackBlockedAhead && _timeSinceStopped > _stationStopTime)
				//{
				//	// Start moving again and pull out of the station

				//	_currentMoveSpeed_UberMult = 1.0f;

				//	_currStationStopEntryRail = null;
				//	_currStationStopExitRail = null;

				//	_timeSinceStopped = 0.0f; // Reset for the next stop

				//	if (_worldScript.HUDScript.bIsShopMenuOpen)
				//	{
				//		_worldScript.HUDScript.CloseShopMenu();
				//		_worldScript.GameplayScript.bIsTrainStoppedAtStation = false;
				//	}
				//}
			}
		}
	}

	public void ManageSpeed()
	{
		// Manual speed adjustment
		if (Input.GetAxisRaw("SpeedChange") < 0.0f)
		{
			DesiredMoveSpeed -= DesiredSpeedAdjustmentRate;
			DesiredMoveSpeed = Mathf.Clamp(DesiredMoveSpeed, _minMoveSpeed, _maxMoveSpeed);
		}
		else if (Input.GetAxisRaw("SpeedChange") > 0.0f)
		{
			DesiredMoveSpeed += DesiredSpeedAdjustmentRate;
			DesiredMoveSpeed = Mathf.Clamp(DesiredMoveSpeed, _minMoveSpeed, _maxMoveSpeed);
		}

		// Interpolate the current movespeed toward the desired movespeed
		if (_currentMoveSpeed < DesiredMoveSpeed)
		{
			_currentMoveSpeed += AccelDecelRate;
		}
		else if (_currentMoveSpeed > DesiredMoveSpeed)
		{
			_currentMoveSpeed -= AccelDecelRate;
		}
	}

    //void ManageCarriages()
    //{
    //	//MaintainCarriageSeparationDist_LocomotiveVersion();
    //}

    public float GetFullTrainLength()
    {
        int count = CarriagesInOrder.Count;
        if (count == prevCarriageCount)
        {
            return prevCarriageLength;
        }
        else
        {
            prevCarriageCount = count;
            float result = GetCarriageLength();          

            for (int i = 0; i < count; i++)
            {
                result += CarriagesInOrder[i].GetCarriageLength();
            }
            prevCarriageLength = result;

            return result;
        }
    }

    //void MaintainCarriageSeparationDist_LocomotiveVersion()
    //{
    //	for (int i = 0; i < CarriagesInOrder.Count; ++i)
    //	{
    //		// Get the distance between this carriage and the one in front of it
    //		float Dist = 0.0f;

    //		if (i == 0)
    //		{
    //			Dist = Vector3.Distance(CarriagesInOrder[0].transform.position, transform.position);
    //		}
    //		else
    //		{
    //			Dist = Vector3.Distance(CarriagesInOrder[i].transform.position, CarriagesInOrder[i - 1].transform.position);
    //		}

    //		// Match speed with the locomotive
    //		if (Dist > _carriageDistApart)
    //		{
    //			// Too far

    //			float mappedSpeed = BBBStatics.Map(Dist, _carriageDistApart, _carriageDistApart + 1, _currentMoveSpeed, _maxMoveSpeed, false);

    //			CarriagesInOrder[i].GetComponent<CarriageScript>().CurrentMoveSpeed = mappedSpeed;
    //		}
    //		else if (Dist < _carriageDistApart)
    //		{
    //			// Too close

    //			bool bUseMappedSpeed = false;

    //			if (bUseMappedSpeed)
    //			{
    //				float mappedSpeed = BBBStatics.Map(Dist, _carriageDistApart, _carriageDistApart - 1, _currentMoveSpeed, _minMoveSpeed, false);

    //				CarriagesInOrder[i].GetComponent<CarriageScript>().CurrentMoveSpeed = mappedSpeed;
    //			}
    //			else
    //			{
    //				// Instantly stop dead
    //				CarriagesInOrder[i].GetComponent<CarriageScript>().CurrentMoveSpeed = 0.0f;
    //			}
    //		}
    //		else
    //		{
    //			// Just right
    //			CarriagesInOrder[i].GetComponent<CarriageScript>().CurrentMoveSpeed = _currentMoveSpeed; // Match speed with locomotive
    //		}
    //	}
    //}

    /// <summary>
    /// Find nearby carriages
    /// Then check the current order of the carriages
    /// </summary>
    public void CheckCarriageOrder(GameObject excludeGO)
	{
		//print("CheckCarriageOrder() @ " + Time.time);

		// Get all of the carriages and add them to CarriagesInOrder in order of distance away from the locomotive
		// Maybe run every second rather than every tick? -- Or perhaps every time a carriage is created or destroyed? (The latter was implemented on 2-4-18 @ 17:06)

		if (CarriagesInOrder.Count > 0) CarriagesInOrder.Clear();

		//GameObject[] allObjs = (GameObject[])FindObjectsOfType(typeof(GameObject));
		List<GameObject> allObjs = _worldScript.GetAllTGOsInWorld_AsGOs();

		List<CarriageScript> CarriagesNotYetOrdered = new List<CarriageScript>();

        int count = allObjs.Count;
        for (int i = 0; i < count; ++i)
		{
			GameObject go = allObjs[i];
			CarriageScript cs = go.GetComponent<CarriageScript>();
			if (cs != null && !cs.bIsLocomotive) // Is it a carriage?
			{
				if (go != excludeGO)
				{
					CarriagesNotYetOrdered.Add(cs);
				}
			}
		}

		//

		while (CarriagesNotYetOrdered.Count > 0)
		{
			//print("Itr --------");

			CarriageScript closestCarriage = null;
			float closestCarriageDist = float.PositiveInfinity;

            int unorderedCarriagesCount = CarriagesNotYetOrdered.Count;
            for (int i = 0; i < unorderedCarriagesCount; ++i)
			{
				float Dist = Vector3.Distance(CarriagesNotYetOrdered[i].transform.position, transform.position);
				if (Dist < closestCarriageDist)
				{
					closestCarriage = CarriagesNotYetOrdered[i];
					closestCarriageDist = Dist;
				}
			}

			CarriagesInOrder.Add(closestCarriage);
			CarriagesInOrderGameObjects.Add(closestCarriage.gameObject);
			CarriagesNotYetOrdered.Remove(closestCarriage);
		}

		//_worldScript.GameplayScript.SetInitialRandomOffsets();
	}

	//new void OnGUI() // For Debug Labels
	//{
	//	base.OnGUI(); // Call the super-class function

	//	//

	//	//var restoreColor = GUI.color; GUI.color = Color.green; // red

	//	//if (CarriagesInOrder.Count > 0)
	//	//{
	//	//	for (int i = 0; i < CarriagesInOrder.Count; ++i)
	//	//	{
	//	//		UnityEditor.Handles.Label(CarriagesInOrder[i].transform.position, "Carriage Number: " + i.ToString());
	//	//	}
	//	//}

	//	//GUI.color = Color.cyan;

	//	//UnityEditor.Handles.Label(transform.position, "CarriagesInOrder.Count: " + CarriagesInOrder.Count.ToString());

	//	//GUI.color = restoreColor;
	//}

	/// <summary>
	/// Get the list of carriages on the train in the correct sequence
	/// </summary>
	/// <returns></returns>
	public List<CarriageScript> GetCarriagesInOrder()
	{
		return CarriagesInOrder;
	}

	public List<GameObject> GetCarriagesInOrderAsGameObjects()
	{
		return CarriagesInOrderGameObjects;
	}

	//public GameObject ResourceDropOffPoint
	//{
	//	get { return _resourceDropOffPoint; }
	//}

	public bool bPendingCheckCarriageOrder
	{
		get { return _bPendingCheckCarriageOrder; }
		set { _bPendingCheckCarriageOrder = value; }
	}

	public float TimeSinceStopped
	{
		get { return _timeSinceStopped; }
	}

	//private void OnDestroy()
	//{

	//}

	public override void BeginDestroy(bool bRunExplosion, bool bSpawnChunks)
	{
		if (!_bWinConditionTriggered)
		{
			_bFailConditionTriggered = true;

			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._bLostGame = true;
			_worldScript.StatsRec.Rec.CurrPt._locomotiveDestroyedLoc = transform.position;

			SceneManager.LoadScene("failMenu_alpha", LoadSceneMode.Single);
		}

		base.BeginDestroy(bRunExplosion, bSpawnChunks);
	}

    public Vector3 TrainMidPoint
    {
        get
        {
            return _entireTrainMidPoint;
        }
    }
}
