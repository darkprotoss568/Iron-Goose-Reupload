using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarriageScript : TrainGameObjScript
{
	protected GameObject _currentRail;                      // The rail the carriage is on currently

	protected float _regularMoveSpeed = 20.0f; // 0.2f;
	protected float _minMoveSpeed = 0.0f;
	protected float _maxMoveSpeed = 20.0f; // 0.2f;
	protected float _catchUpMoveSpeed = 21.0f; // 0.25f;
	protected float _tooCloseMoveSpeed = 19.0f; // 0.15f;
	protected float _finalSpeed = 0.0f; public float FinalSpeed { get { return _finalSpeed; } }

	protected float _currentMoveSpeed = 0.0f;               // Actual speed that we're moving

	protected float _currentMoveSpeed_UberMult;				  // To give us full control over the actual move speed
	public float CurrentMoveSpeed_UberMult { get { return _currentMoveSpeed_UberMult; } set { _currentMoveSpeed_UberMult = value; } }

	protected float _pathPointAcceptRadius = 0.1f;

	protected float _nextTrackAcceptRadius = 1.0f;

	private float _desiredMoveSpeed = 0.0f;                 // Desired movement speed
	private float _accelDecelRate = 2.0f; // 0.02f;

	public float _carriageDistApart;                        // Desired distance between carriages

	protected GameObject _frontSocket;                      // Front socket of the current rail
	protected GameObject _rearSocket;                       // Rear socket of the current rail

	protected GameObject _carriageInFront; public GameObject CarriageInFront { get { return _carriageInFront; } }
	protected GameObject _carriageBehind;                   // The carriage behind

	protected float _carriageLength;                        // Length of the carriage

	protected bool _bIsLocomotive;                          // True if the carriage is the locomotive

	private bool bPostStartRun = false;

	//protected Vector3 LastTFormPos = Vector3.zero;

	protected AudioSource _engineSoundSource;

	protected float _progressAlongCurrTrack;

	public bool _bRotate180 = false; // Some models require a 180 flip



	
	//private Renderer _rend;

	private List<TrainGameObjScript> _enemiesTargetingThis = new List<TrainGameObjScript>();
	public List<TrainGameObjScript> EnemiesTargetingThis { get { return _enemiesTargetingThis; } }

	new public void Start()
	{
		base.Start();

		_worldScript.AllCarriages.Add(this);

		// Set team
		_team = Team.Friendly;

		_currentRail = GetNearestRail(); // Get our starting rail
		//if (_currentRail != null) Debug.DrawLine(transform.position, _currentRail.transform.position, Color.cyan, 30.0f, false);  // Remember to turn on 'Gizmos' in game view!

		_desiredMoveSpeed = _regularMoveSpeed;

		_carriageDistApart = 0.5f; // 2.0f // Dist maintained between front and rear sockets of adjacent carriages

		//_frontSocket = transform.GetChild(0).gameObject;
		//_rearSocket = transform.GetChild(1).gameObject;
		_frontSocket = transform.Find("FrontSocket").gameObject;
		_rearSocket = transform.Find("RearSocket").gameObject;

		if (_frontSocket != null && _rearSocket != null)
		{
			_carriageLength = Vector3.Distance(_frontSocket.transform.position, _rearSocket.transform.position);
			//print("Found front and rear socket -- CarriageScript @ Start");

			//Debug.DrawLine(_frontSocket.transform.position, _frontSocket.transform.position + new Vector3(0,20,0), Color.red, 20.0f, false);  // Remember to turn on 'Gizmos' in game view!
			//Debug.DrawLine(_rearSocket.transform.position, _rearSocket.transform.position + new Vector3(0,20,0), Color.white, 20.0f, false);  // Remember to turn on 'Gizmos' in game view!
		}
		else
		{
			print("Error: Could not find front and rear socket -- CarriageScript @ Start");
		}

		//_panel = GameObject.Find("StatusPanel").transform;
		//Debug.Log(_panel.position);
		_bIsLocomotive = false;

		_currentMoveSpeed_UberMult = 1.0f;

		_engineSoundSource = GetComponent<AudioSource>();

		
		// End of Start()
	}

	public void FixedUpdate()
	{
		if (PauseMenu.isPaused) return;

		base.Update();

			//if (_worldScript.Get_RandTime001_AvailableThisTurn())
		//	GetAdjacentCarriages(); /// Expensive

		//if (_frontSocket != null && _rearSocket != null)
		//{
		//	Debug.DrawLine(_frontSocket.transform.position, _frontSocket.transform.position + new Vector3(0, 20, 0), Color.red, 20.0f, false);  // Remember to turn on 'Gizmos' in game view!
		//	Debug.DrawLine(_rearSocket.transform.position, _rearSocket.transform.position + new Vector3(0, 20, 0), Color.white, 20.0f, false);  // Remember to turn on 'Gizmos' in game view!
		//}

		if (!bPostStartRun)
		{
			GetAdjacentCarriages();

			// Get our initial progress along our initial rail piece
			float distFromFrontSocket = Vector3.Distance(transform.position, _currentRail.GetComponent<RailScript>().GetFrontSocket().transform.position);
			_progressAlongCurrTrack = BBBStatics.Map(distFromFrontSocket, 0.0f, _currentRail.GetComponent<RailScript>().GetRailLength(), 1.0f, 0.0f, true);

			bPostStartRun = true;
		}

		// End of Update()
	}

	public void moveCarriage()
	{
		//MaintainCarriageSeparationDist();
		MoveAlongTrack_T3();
		RotateAlongTrack();
	}

	/// Get the first PointOnBezierCurve that is closer to the end socket than we are (use an acceptDist!)
	/// Then set that as our direction destination and use T1's move system
	void MoveAlongTrack_T3()
	{
		GameObject dest = _currentRail.GetComponent<RailScript>().GetFrontSocket();

		if (dest == null) return;

		//float railLength = _currentRail.GetComponent<RailScript>().GetRailLength();

		List<Vector3> curvePoints = GetCurrRailBezierPnts(5);

		/////
		//for (int i = 0; i < curvePoints.Count; ++i)
		//{
		//	Debug.DrawLine(curvePoints[i], curvePoints[i] + new Vector3(0, 50, 0), Color.white, Time.deltaTime);
		//}
		/////

		Vector3 destPoint = dest.transform.position; // Default

		float distToDest = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destPoint.x, destPoint.z));
        //print("distToDest: " + distToDest);

        int count = curvePoints.Count;
		for (int i = 0; i < count; i++)
		{
			float curvePntDist = Vector2.Distance(new Vector2(curvePoints[i].x, curvePoints[i].z), new Vector2(destPoint.x, destPoint.z));

			// Point AcceptDist
			curvePntDist += 0.1f;

			if (curvePntDist < distToDest)
			{
				destPoint = curvePoints[i];
				//Debug.DrawLine(curvePoints[i], curvePoints[i] + new Vector3(0, 500, 0), Color.blue, Time.deltaTime);
				break;
			}
		}

		//
		//

		// T1's move system
		Vector3 Dir = destPoint - transform.position;

		///if (_bIsLocomotive) _currentMoveSpeed = Mathf.Clamp(_currentMoveSpeed, _minMoveSpeed, _maxMoveSpeed); // _maxMoveSpeed
		///else _currentMoveSpeed = Mathf.Clamp(_currentMoveSpeed, _minMoveSpeed, _catchUpMoveSpeed); // Only carriages can ever travel faster than _maxMoveSpeed

		_currentMoveSpeed_UberMult = Mathf.Clamp01(_currentMoveSpeed_UberMult);

		_finalSpeed = _currentMoveSpeed * _currentMoveSpeed_UberMult;

		Vector3 Movement = Vector3.ClampMagnitude(new Vector3(Dir.x, 0.0f, Dir.z), Mathf.Abs(_finalSpeed));
		transform.position += Movement * Time.timeScale; // * Time.deltaTime;

		if (distToDest < _nextTrackAcceptRadius)
		{
			SwitchToNextTrack();
		}

		if (_engineSoundSource != null)
		{
			_engineSoundSource.pitch = BBBStatics.Map(_finalSpeed, 0.0f, _maxMoveSpeed, 0.5f, 1.0f, true);
		}
	}

	//

	void RotateAlongTrack()
	{
		// The carriage follows the rotation of the track - instantly snaps to it
		// lerps between the rotations of the rear socket to the front socket

		Quaternion frontSocketRot = _currentRail.GetComponent<RailScript>().GetFrontSocket().transform.rotation;
		Quaternion behindSocketRot = _currentRail.GetComponent<RailScript>().GetBehindSocket().transform.rotation;

		if (_bRotate180)
		{
			// Rotate by 180 -- some carriages require this (not exactly sure why -- 22-4-18)
			Vector3 rot = frontSocketRot.eulerAngles;
			rot = new Vector3(rot.x, rot.y + 180, rot.z);
			frontSocketRot = Quaternion.Euler(rot);

			rot = behindSocketRot.eulerAngles;
			rot = new Vector3(rot.x, rot.y + 180, rot.z);
			behindSocketRot = Quaternion.Euler(rot);
		}

		//

		float distFromFrontSocket = Vector3.Distance(transform.position, _currentRail.GetComponent<RailScript>().GetFrontSocket().transform.position);

		float map = Mathf.Clamp01(BBBStatics.Map(distFromFrontSocket, 0.0f, _currentRail.GetComponent<RailScript>().GetRailLength(), 0.0f, 1.0f, false));

		//

		Quaternion nextRot = Quaternion.Lerp(frontSocketRot, behindSocketRot, map);

		// So the pitch and roll angles of the rails' sockets don't affect the angle of the train (6-4-18)
		Vector3 ea = nextRot.eulerAngles;
		//nextRot = Quaternion.Euler(new Vector3(-90.0f, ea.y, 0.0f));
		nextRot = Quaternion.Euler(new Vector3(0.0f, ea.y, 0.0f));

		transform.rotation = nextRot;
	}

	//

	protected void SwitchToNextTrack()
	{
		GameObject nextRail = _currentRail.GetComponent<RailScript>().GetRailFront();

		if (nextRail != null)
		{
			_currentRail = nextRail;
		}
		else print("Error: No next rail to switch to! -- CarriageScript @ SwitchToNextTrack()");
	}

	protected List<Vector3> GetCurrRailBezierPnts(int pointCount)
	{
		List<Vector3> pnts = new List<Vector3>();

		bool bCurvedTrack = false;

		GameObject start = _currentRail.GetComponent<RailScript>().GetBehindSocket();
		GameObject end = _currentRail.GetComponent<RailScript>().GetFrontSocket();
		GameObject ctrl = end;

		if (_currentRail.GetComponent<RailScript>().GetFrontDirSocket() != null)
		{
			ctrl = _currentRail.GetComponent<RailScript>().GetFrontDirSocket();
			bCurvedTrack = true;
		}

		Vector3 startPnt = start.transform.position;
		Vector3 ctrlPnt = BBBStatics.BetweenAt(start.transform.position, ctrl.transform.position, 0.5f); // Control point must be half way back to get a perfect curve
		Vector3 endPnt = end.transform.position;

		float pointCount_f = pointCount;

		float pcntPerLoop = 1.0f / pointCount_f;

		float progress = 0.0f;

		while (progress < 1.0f)
		{
			if (bCurvedTrack)
				pnts.Add(BBBStatics.GetPointOnBezierCurve(startPnt, ctrlPnt, endPnt, progress));
			else
				pnts.Add(Vector3.Lerp(startPnt, endPnt, progress));

			progress += pcntPerLoop;
		}

		return pnts;
	}

	//

	protected GameObject GetNearestRail()
	{
		GameObject closestRail = null;
		float closestRailDist = float.PositiveInfinity;

		object[] allObjs = FindObjectsOfType(typeof(GameObject));

        int length = allObjs.Length;
		for (int i = 0; i < length; i++) //foreach (object o in allObjs)
		{
			GameObject go = (GameObject)allObjs[i];
			RailScript rs = go.GetComponent<RailScript>();
			if (rs != null) // Is it a rail?
			{
				float Dist = Vector3.Distance(go.transform.position, transform.position);
				if (Dist < closestRailDist)
				{
					closestRail = go;
					closestRailDist = Dist;
				}
			}
		}

		if (closestRail != null) return closestRail;

		return null;
	}

	//

	public void GetAdjacentCarriages()
	{
		_carriageInFront = null;
		_carriageBehind = null;

		List<GameObject> Carriages = new List<GameObject>();

		//GameObject[] allObjs = (GameObject[])FindObjectsOfType(typeof(GameObject));
		List<GameObject> allObjs = _worldScript.GetAllTGOsInWorld_AsGOs();

        int count = allObjs.Count;
		for (int i = 0; i < count; i++)
		{
			CarriageScript cs = allObjs[i].GetComponent<CarriageScript>();
			LocomotiveScript ls = allObjs[i].GetComponent<LocomotiveScript>();
			if (cs != null || ls != null) // Is it a carriage or locomotive?
			{
				if (allObjs[i] != gameObject) // We don't want to connect to ourself
				{
					Carriages.Add(allObjs[i]);
				}
			}
		}

		//

		float maxCheckDist = _carriageLength; // * 0.49f;
                                              //float maxCheckDist = float.PositiveInfinity;
                                              //print("_carriageLength: " + _carriageLength);

        /// Get the closest carriage to both _frontSocket and _rearSocket within maxCheckDist range
       
		for (int i = 0; i < 2; ++i)
		{
			GameObject closestCarriage = null;
			float closestCarriageDist = float.PositiveInfinity;

            int carriagesCount = Carriages.Count;
            for (int j = 0; j < carriagesCount; ++j) //foreach (object o in allObjs)
			{
				Vector3 socketPos = _frontSocket.transform.position;
				if (i == 1) socketPos = _rearSocket.transform.position;

				float Dist = Vector3.Distance(Carriages[j].transform.position, socketPos);
				if (Dist <= maxCheckDist && Dist < closestCarriageDist)
				{
					closestCarriage = Carriages[j];
					closestCarriageDist = Dist;
				}
			}
			if (closestCarriage != null)
			{
				if (i == 0) _carriageInFront = closestCarriage;
				if (i == 1) _carriageBehind = closestCarriage;

				Carriages.Remove(closestCarriage); // We don't want to use the same carriage twice -- just to be certain -- probably not possible
			}
		}

		///
		if (_carriageInFront != null)
			Debug.DrawLine(_frontSocket.transform.position, _carriageInFront.GetComponent<CarriageScript>()._rearSocket.transform.position, Color.blue, 15.0f, false);  // Remember to turn on 'Gizmos' in game view!

		if (_carriageBehind != null)
			//Debug.DrawLine(_rearSocket.transform.position, _carriageBehind.GetComponent<CarriageScript>()._frontSocket.transform.position, Color.cyan, Time.deltaTime, false);  // Remember to turn on 'Gizmos' in game view!
			Debug.DrawLine(_rearSocket.transform.position, _rearSocket.transform.position + new Vector3(0, 10, 0), Color.cyan, 15.0f, false);
		///

		//if (_carriageInFront == null)
		//	print("Error: _carriageInFront == null");
		//if (_carriageBehind == null)
		//	print("Error: _carriageBehind == null");
	}

	float GetCarriageInFrontSpeed()
	{
		if (_carriageInFront == null)
		{
			print("ERROR: _carriageInFront == null -- GetCarriageInFrontSpeed() -- CarriageScript");
			return 0.0f;
		}

		//if (_carriageInFront.GetComponent<CarriageScript>() != null) return _carriageInFront.GetComponent<CarriageScript>().CurrentMoveSpeed;
		return _carriageInFront.GetComponent<CarriageScript>().CurrentMoveSpeed;

	}

	public void MaintainCarriageSeparationDist_T2() // Called externally from WS
	{
		if (_bIsLocomotive) return;
		if (_carriageInFront == null) return;

		_currentMoveSpeed = _worldScript.getLocomotiveScript.CurrentMoveSpeed; // GetCarriageInFrontSpeed();
		//_currentMoveSpeed = GetCarriageInFrontSpeed(); // GetCarriageInFrontSpeed();

		Vector3 targetLoc = _carriageInFront.GetComponent<CarriageScript>().RearSocket.transform.position;

		// FAULTY
		//Vector3 targetPosRotated = targetLoc + (_carriageInFront.transform.rotation * Vector3.zero);
		//Vector3 ourPosRotated = _frontSocket.transform.position + (_carriageInFront.transform.rotation * Vector3.zero);

		float d = _frontSocket.transform.InverseTransformPoint(targetLoc).z;

		//print(": " + transform.InverseTransformPoint(_carriageInFront.transform.position));

		//if (ourPosRotated.z < targetPosRotated.z)
		if (d < 0)
		{
			//float d = targetPosRotated.z - ourPosRotated.z;
			_currentMoveSpeed += BBBStatics.Map(d, 1, -1, 1.0f, -1.0f, true);
		}
		else
		{
			//float d = ourPosRotated.z - targetPosRotated.z;
			_currentMoveSpeed += BBBStatics.Map(d, 1, -1, -1.0f, 1.0f, true);
		}
	}

	//public void MatchSpeedWithCarriageInFront() // Called externally from WS
	//{
	//	if (_bIsLocomotive) return;
	//	if (_carriageInFront == null) return;

	//	_currentMoveSpeed = GetCarriageInFrontSpeed();
	//}

	//

	//public void OnGUI() // For Debug Labels
	//{
	//	var restoreColor = GUI.color; GUI.color = Color.green; // red

	//	//UnityEditor.Handles.Label(transform.position, "_CurrAIPathPoints.Count: " + _CurrAIPathPoints.Count.ToString());
	//	//UnityEditor.Handles.Label(transform.position, "_currentMoveSpeed: " + _currentMoveSpeed.ToString());
	//	UnityEditor.Handles.Label(transform.position, "_progressAlongCurrTrack: " + _progressAlongCurrTrack.ToString());

	//	GUI.color = restoreColor;
	//}

	public float CurrentMoveSpeed
	{
		get { return _currentMoveSpeed; }
		set { _currentMoveSpeed = value; }
	}

	public float DesiredMoveSpeed
	{
		get { return _desiredMoveSpeed; }
		set { _desiredMoveSpeed = value; }
	}

	public float AccelDecelRate
	{
		get { return _accelDecelRate; }
		set { _accelDecelRate = value; }
	}

	public override void BeginDestroy(bool bRunExplosion, bool bSpawnChunks)
	{
		// Cut out the middle man (as it's about to be destroyed)
		if (_carriageBehind != null && _carriageInFront != null) // We need both to exist or there's nothing to connect!
		{
			_carriageInFront.GetComponent<CarriageScript>()._carriageBehind = _carriageBehind;
			_carriageBehind.GetComponent<CarriageScript>()._carriageInFront = _carriageInFront;
		}

		LocomotiveScript ls = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>();
		ls.CheckCarriageOrder(gameObject);
		_worldScript.GameplayScript.SetInitialRandomOffsets();
		//ls.bPendingCheckCarriageOrder = true;

		if (!bIsLocomotive)
		{
			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._carriagesLost++;
		}

		_worldScript.AllCarriages.Remove(this);

		base.BeginDestroy(true, true);
	}

    protected override void OnDestroy()
    {
        if (!gameObject.GetComponent<LocomotiveScript>())
        {
            _worldScript.GameplayScript.AddResources(_buildCost);
        }
    }

    public bool bIsLocomotive
	{
		get { return _bIsLocomotive; }
	}

	public GameObject RearSocket
	{
		get { return _rearSocket; }
	}

	public GameObject FrontSocket
	{
		get { return _frontSocket; }
	}
	
	public float GetCarriageLength()
	{
		return _carriageLength;
	}
}

