using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomisationScript : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } }

	// Fixed 1 second delay
	private float RandTime001_Length = 1.0f;
	private float RandTime001_Curr = 0.0f;
	private bool RandTime001_AvailableThisTurn = false;

	// Variable 1-3 second delay
	private float RandTime002_Length = 1.0f;
	private float RandTime002_Curr = 0.0f;
	private bool RandTime002_AvailableThisTurn = false;

	// Variable 5-10 second delay
	private float RandTime003_Length = 5.0f;
	private float RandTime003_Curr = 0.0f;
	private bool RandTime003_AvailableThisTurn = false;

	// Fixed 0.5 second delay
	private float RandTime004_Length = 0.5f;
	private float RandTime004_Curr = 0.0f;
	private bool RandTime004_AvailableThisTurn = false;

	// Variable 0.1-0.3 second delay
	private float RandTime005_Length = 0.15f;
	private float RandTime005_Curr = 0.0f;
	private bool RandTime005_AvailableThisTurn = false;

	// Variable 5-10 second delay (2)
	private float RandTime006_Length = 5.0f;
	private float RandTime006_Curr = 0.0f;
	private bool RandTime006_AvailableThisTurn = false;

	// Variable 3-6 second delay
	private float RandTime007_Length = 4.5f;
	private float RandTime007_Curr = 0.0f;
	private bool RandTime007_AvailableThisTurn = false;

	void Start()
	{

	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		RandomisationManager();
	}

	private void RandomisationManager()
	{
		RandTime001_AvailableThisTurn = false;
		RandTime001_Curr += Time.deltaTime;
		if (RandTime001_Curr >= RandTime001_Length)
		{
			RandTime001_Curr = 0.0f;
			RandTime001_AvailableThisTurn = true;
		}

		RandTime002_AvailableThisTurn = false;
		RandTime002_Curr += Time.deltaTime;
		if (RandTime002_Curr >= RandTime002_Length)
		{
			RandTime002_Length = BBBStatics.RandFlt(1.0f, 3.0f);
			RandTime002_Curr = 0.0f;
			RandTime002_AvailableThisTurn = true;
		}

		RandTime003_AvailableThisTurn = false;
		RandTime003_Curr += Time.deltaTime;
		if (RandTime003_Curr >= RandTime003_Length)
		{
			RandTime003_Length = BBBStatics.RandFlt(5.0f, 10.0f);
			RandTime003_Curr = 0.0f;
			RandTime003_AvailableThisTurn = true;
		}

		RandTime004_AvailableThisTurn = false;
		RandTime004_Curr += Time.deltaTime;
		if (RandTime004_Curr >= RandTime004_Length)
		{
			RandTime004_Curr = 0.0f;
			RandTime004_AvailableThisTurn = true;
		}

		RandTime005_AvailableThisTurn = false;
		RandTime005_Curr += Time.deltaTime;
		if (RandTime005_Curr >= RandTime005_Length)
		{
			RandTime005_Length = BBBStatics.RandFlt(0.1f, 0.3f);
			RandTime005_Curr = 0.0f;
			RandTime005_AvailableThisTurn = true;
		}

		RandTime006_AvailableThisTurn = false;
		RandTime006_Curr += Time.deltaTime;
		if (RandTime006_Curr >= RandTime006_Length)
		{
			RandTime006_Length = BBBStatics.RandFlt(5.0f, 10.0f);
			RandTime006_Curr = 0.0f;
			RandTime006_AvailableThisTurn = true;
		}

		RandTime007_AvailableThisTurn = false;
		RandTime007_Curr += Time.deltaTime;
		if (RandTime007_Curr >= RandTime007_Length)
		{
			RandTime007_Length = BBBStatics.RandFlt(3.0f, 6.0f);
			RandTime007_Curr = 0.0f;
			RandTime007_AvailableThisTurn = true;
		}
	}

	public bool Get_RandTime001_AvailableThisTurn() { return RandTime001_AvailableThisTurn; }
	public bool Get_RandTime002_AvailableThisTurn() { return RandTime002_AvailableThisTurn; }
	public bool Get_RandTime003_AvailableThisTurn() { return RandTime003_AvailableThisTurn; }
	public bool Get_RandTime004_AvailableThisTurn() { return RandTime004_AvailableThisTurn; }
	public bool Get_RandTime005_AvailableThisTurn() { return RandTime005_AvailableThisTurn; }
	public bool Get_RandTime006_AvailableThisTurn() { return RandTime006_AvailableThisTurn; } 
	public bool Get_RandTime007_AvailableThisTurn() { return RandTime007_AvailableThisTurn; } // Variable 3-6 second delay
}
