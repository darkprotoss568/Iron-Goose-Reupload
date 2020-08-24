using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailScript : TrainGameObjScript
{
	protected GameObject railFront;                 // The rail in the front
	protected GameObject railBehind;                // The rail at the back

	protected GameObject FrontSocket;               // Front Rail socket
	protected GameObject FrontDirSocket;            // Socket which determines the angle of the track at its end
	protected GameObject BehindSocket;              // Back rail socket

	public bool bIsFirstRail = false;               // Won't have any rail behind it
	public bool bIsLastRail = false;                // Won't have any rail in front of it

	float DebugLineTime = 30.0f;

	bool bPostStartRun = false;                     // For calling methods in only the first frame of Update()

	private float _railLength;                      // The length of the rail

	public bool bIsStationEntryRail = false;
	public bool bIsStationExitRail = false;

	private WS_ProceduralGen _WS_ProceduralGen;

	private RailInEditorScript _railInEditorScript;

	public GameObject RailFront { get { return railFront; } set { railFront = value; } }
	public GameObject RailBehind { get { return railBehind; } set { railBehind = value; } }

	private bool _bRunRailConnection = true;
	public bool BRunRailConnection { get { return _bRunRailConnection; } set { _bRunRailConnection = value; } }

	public bool _bIsWinConditionRail = false;
    public bool _bMakeMusicHectic = false;

	private List<List<Vector3>> _paths = new List<List<Vector3>>();
	public List<List<Vector3>> Paths { get { return _paths; } }
	private List<bool> _filledPaths = new List<bool>();
	public List<bool> FilledPaths { get { return _filledPaths; } }

	private List<List<Vector3>> _paths_close = new List<List<Vector3>>();
	public List<List<Vector3>> Paths_close { get { return _paths_close; } }
	private List<bool> _filledPaths_close = new List<bool>();
	public List<bool> FilledPaths_close { get { return _filledPaths_close; } }

	new void Start()
	{
		base.Start();

		_railInEditorScript = GetComponent<RailInEditorScript>();

		_team = Team.Neutral;

		//

		// Set the socket objects
		//FrontSocket = transform.GetChild(0).gameObject;
		//BehindSocket = transform.GetChild(1).gameObject;
		//if (transform.childCount >= 3) FrontDirSocket = transform.GetChild(2).gameObject;

		if (transform.Find("FrontSocket") != null)
			FrontSocket = transform.Find("FrontSocket").gameObject;

		if (transform.Find("RearSocket") != null)
			BehindSocket = transform.Find("RearSocket").gameObject;

		if (transform.Find("FrontDirSocket") != null)
			FrontDirSocket = transform.Find("FrontDirSocket").gameObject;

		//

		// Get the length of the rail
		if (FrontSocket != null && BehindSocket != null)
		{
			if (FrontDirSocket == null)
			{
				_railLength = Vector3.Distance(FrontSocket.transform.position, BehindSocket.transform.position);
			}
			else
			{
				_railLength = Vector3.Distance(FrontDirSocket.transform.position, BehindSocket.transform.position);
			}
		}

		GameObject pgh = GameObject.Find("WS_ProceduralGen_Holder");
		if (pgh != null)
		{
			_WS_ProceduralGen = pgh.GetComponent<WS_ProceduralGen>();
			_WS_ProceduralGen.AllGameObjs.Add(gameObject);
			_WS_ProceduralGen.AllRails.Add(this);

			_WS_ProceduralGen.SpawnElementsBelowRail(this);
		}

		PathsAlongside();
	}

	new void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();

		// If the rail cannot be run on yet
		if (!bPostStartRun)
		{
			// Connect the current rail to adjacent rails
			if (_bRunRailConnection) ConnectToAdjacentRails();

			bPostStartRun = true;
		}

		// 
		if (RailFront == null || RailBehind == null)
		{
			if (_worldScript.ShowTrainRailDebugging)
			{
				Debug.DrawLine(transform.position, transform.position + new Vector3(BBBStatics.RandFlt(-0.2f, 0.2f), 15, BBBStatics.RandFlt(-0.2f, 0.2f)), Color.red, Time.deltaTime, false);  // Remember to turn on 'Gizmos' in game view!
			}
		}

		//

		Proc_SpawnFrontRailIfNone();

		//

		///
		bool bRunBezierCurveDebugging = false;
		if (bRunBezierCurveDebugging && FrontDirSocket != null)
		{
			Vector3 startPnt = BehindSocket.transform.position;
			Vector3 ctrlPnt = BBBStatics.BetweenAt(FrontDirSocket.transform.position, BehindSocket.transform.position, 0.5f); // Control point must be half way back to get a perfect curve
			Vector3 endPnt = FrontSocket.transform.position;

			float progress = 0.0f;
			while (progress <= 1.0f)
			{
				Vector3 testPos = BBBStatics.GetPointOnBezierCurve(startPnt, ctrlPnt, endPnt, progress);

				Debug.DrawLine(testPos, testPos + new Vector3(0, 100, 0), Color.white, Time.deltaTime);

				progress += 0.1f;
			}
		}
		///

		// End of Update()
	}

	void Proc_SpawnFrontRailIfNone()
	{
		if (_worldScript.LocomotiveObjectRef != null)
		{
			float dist = Vector3.Distance(_worldScript.LocomotiveObjectRef.transform.position, transform.position);
			if (dist < 100.0f)
			{
				if (railFront == null)
				{
					_railInEditorScript.bSpawnTrackInFront = true;
				}
			}
		}
	}

	/// <summary>
	/// Connect the current rail to the front and back rails
	/// </summary>
	void ConnectToAdjacentRails()
	{
		if (FrontSocket != null && BehindSocket != null)
		{
			//Debug.DrawLine(transform.position, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 15, Random.Range(-0.5f, 0.5f)), Color.red, DebugLineTime, false);  // Remember to turn on 'Gizmos' in game view!

			// Get all Rail Objects in the current scene
			object[] allObjs = GameObject.FindObjectsOfType(typeof(GameObject));

			// Loop through allObjs to find the back and front rails
			for (int i = 0; i < allObjs.Length; ++i)
			{
				GameObject g = (GameObject)allObjs[i];
				RailScript rs = g.GetComponent<RailScript>();
				// If the object does not have a RailScript
				if (rs != null)
				{
					//! We have a rail that we can test
					//! Test if it is in range

					if (rs.FrontSocket != null && rs.BehindSocket != null)
					{
						// Connect the rails based on distance
						float Dist1 = Vector3.Distance(rs.FrontSocket.transform.position, BehindSocket.transform.position);
						//print("Dist1: " + Dist1);

						float Dist2 = Vector3.Distance(rs.BehindSocket.transform.position, FrontSocket.transform.position);
						//print("Dist2: " + Dist2);

						if (RailBehind == null && Dist1 < 1.0f)
						{
							RailBehind = rs.gameObject;
						}

						if (RailFront == null && Dist2 < 1.0f)
						{
							RailFront = rs.gameObject;
						}
					}
				}
			}
		}
		else print("Error: Rail sockets missing!");

		// Debug
		if (_worldScript.ShowTrainRailDebugging)
		{
			if (RailFront != null)
			{
				Vector3 Offset = new Vector3(BBBStatics.RandFlt(-0.5f, 0.5f), BBBStatics.RandFlt(0.1f, 0.5f), BBBStatics.RandFlt(-0.5f, 0.5f));
				Debug.DrawLine(transform.position - Offset, RailFront.transform.position + Offset, Color.red, DebugLineTime, false); // Remember to turn on 'Gizmos' in game view!

				Debug.DrawLine(transform.position, transform.position + new Vector3(BBBStatics.RandFlt(-0.5f, 0.5f), 10, BBBStatics.RandFlt(-0.5f, 0.5f)), Color.yellow, DebugLineTime, false);  // Remember to turn on 'Gizmos' in game view!
			}

			if (RailBehind != null)
			{
				Vector3 Offset = new Vector3(BBBStatics.RandFlt(-0.5f, 0.5f), BBBStatics.RandFlt(0.1f, 0.5f), BBBStatics.RandFlt(-0.5f, 0.5f));
				Debug.DrawLine(transform.position + Offset, RailBehind.transform.position - Offset, Color.green, DebugLineTime, false); // Remember to turn on 'Gizmos' in game view!

				Debug.DrawLine(transform.position, transform.position + new Vector3(BBBStatics.RandFlt(-0.5f, 0.5f), 10, BBBStatics.RandFlt(-0.5f, 0.5f)), Color.cyan, DebugLineTime, false);  // Remember to turn on 'Gizmos' in game view!
			}
		}
	}

	public GameObject GetRailFront() { return RailFront; } // Get the rail object ahead of this rail
	public GameObject GetRailBehind() { return RailBehind; } // Get the rail object behind this rail
	public GameObject GetFrontSocket() { return FrontSocket; } // Get the front socket of this rail
	public GameObject GetBehindSocket() { return BehindSocket; } // Get the back socket of this rail
	public GameObject GetFrontDirSocket() { return FrontDirSocket; } // Get the front direction socket of this rail
	public float GetRailLength() { return _railLength; } // Get the length of this rail

	//public void OnGUI() // For Debug Labels
	//{
	//	var restoreColor = GUI.color; GUI.color = Color.red; // red

	//	UnityEditor.Handles.Label(transform.position, "_railLength: " + _railLength.ToString());

	//	//if (BehindSocket != null)
	//	//{
	//	//	GUI.color = Color.cyan;
	//	//	UnityEditor.Handles.Label(transform.position + new Vector3(1, 1, 1), "BehindSocket != null");
	//	//}

	//	GUI.color = restoreColor;
	//}

	public void PathsAlongside()
	{
		List<float> offsets = new List<float>();

		offsets.Add(15); // 20
		offsets.Add(20); // 26
		offsets.Add(25); // 32

		offsets.Add(-15); // -20
		offsets.Add(-20); // -26
		offsets.Add(-25); // -32

        offsets.Add(8); // Path for Chop
        offsets.Add(-8);
        //

        for (int i = 0; i < offsets.Count; ++i)
		{
			_paths.Add(new List<Vector3>());
			_filledPaths.Add(false);

			_paths[i] = GetPathAlongside(offsets[i]);
		}

		//

		List<float> offsets_close = new List<float>();

		offsets_close.Add(5);
		offsets_close.Add(-5);

		for (int i = 0; i < offsets_close.Count; ++i)
		{
			_paths_close.Add(new List<Vector3>());
			_filledPaths_close.Add(false);

			_paths_close[i] = GetPathAlongside(offsets_close[i]);
		}

		//

		//List<Color> colours = new List<Color>();
		//colours.Add(Color.green);
		//colours.Add(Color.red);
		//colours.Add(Color.blue);
		//colours.Add(Color.cyan);
		//colours.Add(Color.yellow);
		//colours.Add(Color.magenta);
        //colours.Add(Color.grey); 
        //colours.Add(Color.black);

		//for (int i = 0; i < _paths.Count; ++i)
		//{
		//	for (int j = 0; j < _paths[i].Count; ++j)
		//	{
		//		Debug.DrawLine(_paths[i][j], _paths[i][j] + new Vector3(0, 10, 0), colours[i], 120.0f);
		//	}
		//}

		//

		//List<Color> coloursB = new List<Color>();
		//coloursB.Add(Color.white);
		//coloursB.Add(Color.black);

		//for (int i = 0; i < _paths_close.Count; ++i)
		//{
		//	for (int j = 0; j < _paths_close[i].Count; ++j)
		//	{
		//		Debug.DrawLine(_paths_close[i][j], _paths_close[i][j] + new Vector3(0, 10, 0), coloursB[i], 120.0f);
		//	}
		//}
	}

	private List<Vector3> GetPathAlongside(float offset)
	{
		List<Vector3> result = new List<Vector3>();

		// In here so these reset each time
		Vector3 startPnt = BehindSocket.transform.position;
		Vector3 endPnt = FrontSocket.transform.position;

		Vector3 dirLoc = endPnt;

		if (FrontDirSocket != null)
		{
			dirLoc = FrontDirSocket.transform.position;
		}

		//

		startPnt = startPnt + (BehindSocket.transform.rotation * new Vector3(offset, 0, 0));
		endPnt = endPnt + (FrontSocket.transform.rotation * new Vector3(offset, 0, 0));

		dirLoc = dirLoc + (Quaternion.Lerp(BehindSocket.transform.rotation, FrontSocket.transform.rotation, 0.5f) * new Vector3(offset, 0, 0));

		Vector3 ctrlPnt = BBBStatics.BetweenAt(dirLoc, startPnt, 0.5f); // Control point must be half way back to get a perfect curve

		float progress = 0.0f;
		while (progress <= 1.0f)
		{
			Vector3 pathPnt = BBBStatics.GetPointOnBezierCurve(startPnt, ctrlPnt, endPnt, progress);

			Vector3 normal;
			pathPnt = BBBStatics.CheckForGroundV_V2(pathPnt, 20.0f, out normal);

			if (pathPnt != Vector3.zero)
			{
				result.Add(pathPnt);
			}

			progress += 0.25f;
		}

		return result;
	}
}

