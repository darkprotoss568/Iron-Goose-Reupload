using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization;

public class StatsRecording : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } }
	public WorldScript WS { get { return _worldScript; } }

	private StatsRecord _rec = null;
	public StatsRecord Rec { get { return _rec; } }

	private bool _bInitiated = false;

	void Start()
	{
		Initiate();
	}

	public void Initiate()
	{
		if (_bInitiated) return;
		_bInitiated = true;

		// Read the instance of StatsRecord from file

		//_rec = LoadStatsRecordFromFile(); // If it can't, this will just create a new one
		_rec = new StatsRecord();

		_rec.BeginNewPlayThrough();
	}

	void Update()
	{
		if (_rec != null)
		{
			_rec.Update();
		}
	}

	private void OnDestroy()
	{
		_rec.CurrPt.End();
		_rec.PrintAllStatsToFile();
		//SaveStatsRecordToFile();
	}

	void SaveStatsRecordToFile()
	{
		if (_rec == null) return;

		try
		{
			using (Stream stream = File.Open("statsRecord.bin", FileMode.Create))
			{
				BinaryFormatter bin = new BinaryFormatter();
				bin.Serialize(stream, _rec);
			}
		}
		catch (IOException)
		{
		}
	}

	StatsRecord LoadStatsRecordFromFile()
	{
		StatsRecord sr = new StatsRecord();

		try
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream("statsRecord.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
			sr = (StatsRecord)formatter.Deserialize(stream);
		}
		catch (IOException)
		{
		}

		return sr;
	}
}

[System.Serializable()]
public class StatsRecord
{
	private List<PlayThrough> _playThroughs = new List<PlayThrough>();
	public List<PlayThrough> PlayThroughs { get { return _playThroughs; } set { _playThroughs = value; } }

	private PlayThrough _currPt;
	public PlayThrough CurrPt { get { return _currPt; } set { _currPt = value; } }

	public StatsRecord()
	{

	}

	public void Update()
	{
		if (_currPt != null)
		{
			_currPt.Update();
		}
	}

	public void BeginNewPlayThrough()
	{
		_currPt = new PlayThrough(this);
		_playThroughs.Add(_currPt);
	}

	public void PrintAllStatsToFile()
	{
		if (_playThroughs.Count == 0) return;

		List<string> _toPrint = new List<string>();

		//

		float accumulatedPlayTime = 0.0f;
		float accumulatedAvgFramerate = 0.0f;

		int timesWonGame = 0;
		int timesLostGame = 0;

		//

		for (int i = 0; i < _playThroughs.Count; ++i)
		{
			_toPrint.Add("-------------------------------------------------");
			_toPrint.Add("-------------------------------------------------");
			_toPrint.Add("-------------------------------------------------");

			_toPrint.Add("Play ID: " + _playThroughs[i]._id);
			_toPrint.Add("Play time: " + _playThroughs[i]._playTime);

			_toPrint.Add("PC user name: " + _playThroughs[i]._pcUserName);
			_toPrint.Add("Average framerate: " + _playThroughs[i]._averageFrameRate);

			_toPrint.Add("Won game: " + _playThroughs[i]._bWonGame);
			_toPrint.Add("Lost game: " + _playThroughs[i]._bLostGame);
			_toPrint.Add("Quit early: " + _playThroughs[i]._bQuitEarly);

			if (_playThroughs[i]._bWonGame) timesWonGame++;
			if (_playThroughs[i]._bLostGame) timesLostGame++;

			if (_playThroughs[i]._locomotiveDestroyedLoc != Vector3.zero)
				_toPrint.Add("Locomotive destroyed location: " + _playThroughs[i]._locomotiveDestroyedLoc);

			_toPrint.Add("Resources spent: " + _playThroughs[i]._resourcesSpent);
			_toPrint.Add("Resources gathered: " + _playThroughs[i]._resourcesGathered);

			_toPrint.Add("Overall damage taken: " + _playThroughs[i]._overallDamageTaken);
			_toPrint.Add("Overall damage inflicted: " + _playThroughs[i]._overallDamageInflicted);

			_toPrint.Add("Enemies spawned count: " + _playThroughs[i]._enemiesSpawnedCount);
			_toPrint.Add("Enemies destroyed count: " + _playThroughs[i]._enemiesDestroyedCount);

			_toPrint.Add("Turrets built count: " + _playThroughs[i]._turretsBuiltCount);
			_toPrint.Add("Turrets lost count: " + _playThroughs[i]._turretsLostCount);

			_toPrint.Add("Carriages lost count: " + _playThroughs[i]._carriagesLost);

			_toPrint.Add("----------------------");

			//

			/// Calculate overall averages

			accumulatedPlayTime += _playThroughs[i]._playTime;
			accumulatedAvgFramerate += _playThroughs[i]._averageFrameRate;
		}

		//

		_toPrint.Add("----------------------");

		_toPrint.Add("Overall playthrough count: " + _playThroughs.Count);

		_toPrint.Add("Overall average play time: " + (accumulatedPlayTime / _playThroughs.Count));
		_toPrint.Add("Overall average framerate: " + (accumulatedAvgFramerate / _playThroughs.Count));

		float percentFinishedWon = (float)_playThroughs.Count / (float)timesWonGame;
		float percentFinishedLost = (float)_playThroughs.Count / (float)timesLostGame;

		_toPrint.Add("Overall Percent Finished & Won: " + percentFinishedWon);
		_toPrint.Add("Overall Percent Finished & Lost: " + percentFinishedLost);

		//

		// TODO: Per-user overall percentages ?

		//

		_toPrint.Add("-------------------------------------------------");
		_toPrint.Add("-------------------------------------------------");
		_toPrint.Add("-------------------------------------------------");

		//

		//System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", lines);
		//System.IO.File.WriteAllLines("statsRecord_output.txt", lines);

		try
		{
			string[] readInLines = System.IO.File.ReadAllLines("statsRecord_output.txt");
			_toPrint.AddRange(readInLines);
		}
		catch (IOException) {}

		string[] lines = _toPrint.ToArray();

		try
		{
			System.IO.File.WriteAllLines("statsRecord_output.txt", lines);
		}
		catch (IOException) { }
	}
}

[System.Serializable()]
public class PlayThrough
{
	//StatsRecord _parent;

	public int _id;
	public float _playTime = 0.0f;

	public string _pcUserName;
	public float _averageFrameRate;

	public bool _bWonGame;
	public bool _bLostGame;
	public bool _bQuitEarly;

	public Vector3 _locomotiveDestroyedLoc;
	public float _timeToWinGame;

	public int _resourcesSpent;
	public int _resourcesGathered;

	public int _overallDamageTaken;
	public int _overallDamageInflicted;

	public int _enemiesSpawnedCount;
	public int _enemiesDestroyedCount;

	public int _turretsBuiltCount;
	public int _turretsLostCount;

	public int _carriagesLost;

	//

	private int _averageFrameRate_frameCount = 0;

	public PlayThrough(StatsRecord parent)
	{
		//_parent = parent;

		_id = parent.PlayThroughs.Count;

		_pcUserName = System.Environment.UserName;


	}

	public void Update()
	{
		_playTime += Time.deltaTime;
		++_averageFrameRate_frameCount;
	}

	public void End()
	{
		_averageFrameRate = 1 / (_playTime / (float)_averageFrameRate_frameCount);

		if (_bWonGame) _timeToWinGame = _playTime;

		if (!_bWonGame && !_bLostGame) _bQuitEarly = true;
	}
}