using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldState
{
	Day,
	Night
}

public class DayNightCycleScript : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } } public WorldScript WS { get { return _worldScript; } }

	private WorldState _worldState = WorldState.Day; public WorldState WorldState { get { return _worldState; } set { _worldState = value; } }

	public Light DirectionalLight;

	private float TimeDelay001 = 0.0f;

	List<GameObject> LightPosts; // = new List<GameObject>();

	private bool _bDayNightCycleActive = false;

	public float TimeOfDay = 0.5f; // 0 = Night // 0.5 = Midday // 1 = Night again
	public float TimeOfNight = 0.0f;

	private Vector3 SunriseDirLightAngle = new Vector3(330, 330, 0); // Vector3(-80, 0, 0);
	private Vector3 MiddayDirLightAngle = new Vector3(90, 330, 0); // Vector3(40, 0, 0);
	private Vector3 SunsetDirLightAngle = new Vector3(330, 150, 180); // Vector3(160, 0, 0);

	private float DayLengthSeconds = 30.0f;
	private float DayProgressSeconds = 0.0f;

	private float NightLengthSeconds = 10.0f;
	private float NightProgressSeconds = 0.0f;

	private bool bIsOnNightShift = false; // Distinct system from worldState!


	void Start()
	{
		DayProgressSeconds = DayLengthSeconds * 0.5f; // Start at midday
		NightLengthSeconds = DayLengthSeconds;

		DirectionalLight = GameObject.Find("Directional_Light").GetComponent<Light>();
		if (DirectionalLight == null) print("Error: DirectionalLight == null -- WorldScript");

		GameObject[] GOs = GameObject.FindGameObjectsWithTag("LampPostTag");
		LightPosts = new List<GameObject>(GOs);
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		DayNightCycleManager();
	}

	void DayNightCycleManager()
	{
		float nightAmbientIntensity = 0.75f;

		if (!bIsOnNightShift)
		{
			if (_bDayNightCycleActive)
				DayProgressSeconds += Time.deltaTime;

			if (DayProgressSeconds >= DayLengthSeconds)
			{
				DayProgressSeconds = 0.0f; // A brand new day - or night
				bIsOnNightShift = true;
			}

			///TimeOfDay = BBBStatics.Map(DayProgressSeconds, 0.0f, DayLengthSeconds, 0.0f, 1.0f, false);
			///TimeOfDay = Mathf.Clamp01(TimeOfDay);

			TimeOfDay = BBBStatics.Map(WS.MapCompletionPcnt, 0.0f, 1.0f, 0.4f, 0.75f, false); /// 12-8-18

			//

			//! Ambient lighting

			if (TimeOfDay < 0.5f)
				RenderSettings.ambientIntensity = BBBStatics.Map(TimeOfDay, 0.0f, 0.5f, nightAmbientIntensity, 1.0f, false);
			else
				RenderSettings.ambientIntensity = BBBStatics.Map(TimeOfDay, 0.5f, 1.0f, 1.0f, nightAmbientIntensity, false);

			//

			//print("DirectionalLight.transform.rotation.eulerAngles: " + DirectionalLight.transform.rotation.eulerAngles);

			int DayNightCycleType = 3;

			if (DayNightCycleType == 1)
			{
				TimeDelay001 += Time.deltaTime;

				if (TimeDelay001 >= 2.0f)
				{
					ToggleDayNight();
					TimeDelay001 = 0.0f;
				}
			}
			else if (DayNightCycleType == 2)
			{
				float MappedAngle = BBBStatics.Map(TimeOfDay, 0.0f, 1.0f, SunriseDirLightAngle.x, SunsetDirLightAngle.x, false);
				//MappedAngle = Mathf.Clamp01(MappedAngle);

				Vector3 NextDirLightAngle = new Vector3(MappedAngle, 0, 0);
				DirectionalLight.transform.rotation = Quaternion.Euler(NextDirLightAngle);
			}
			else if (DayNightCycleType == 3)
			{
				Quaternion N = DirectionalLight.transform.rotation; // Defaults to our current rot

				//

				if (TimeOfDay < 0.5f)
					N = Quaternion.Lerp(Quaternion.Euler(SunriseDirLightAngle), Quaternion.Euler(MiddayDirLightAngle), BBBStatics.Map(TimeOfDay, 0.0f, 0.5f, 0.0f, 1.0f, false));
				else
					N = Quaternion.Lerp(Quaternion.Euler(MiddayDirLightAngle), Quaternion.Euler(SunsetDirLightAngle), BBBStatics.Map(TimeOfDay, 0.5f, 1.0f, 0.0f, 1.0f, false));

				//

				DirectionalLight.transform.rotation = N;

				float DayNightThreshold = 0.25f;
				if (TimeOfDay < DayNightThreshold || TimeOfDay > 1 - DayNightThreshold)
				{
					SetNightMode();
				}
				else
				{
					SetDayMode();
				}
			}
		}
		else
		{
			if (_bDayNightCycleActive)
				NightProgressSeconds += Time.deltaTime;

			if (NightProgressSeconds >= NightLengthSeconds)
			{
				NightProgressSeconds = 0.0f; // A brand new day - or night
				bIsOnNightShift = false;
			}

			TimeOfNight = BBBStatics.Map(DayProgressSeconds, 0.0f, DayLengthSeconds, 0.0f, 1.0f, false);
			TimeOfNight = Mathf.Clamp01(TimeOfNight);

			//

			//! Ambient lighting (Night)

			if (TimeOfNight < 0.5f) // TimeOfDay
			{
				RenderSettings.ambientIntensity = BBBStatics.Map(TimeOfNight, 0.0f, 0.5f, nightAmbientIntensity, nightAmbientIntensity, false);
			}
			else
			{
				RenderSettings.ambientIntensity = BBBStatics.Map(TimeOfNight, 0.5f, 1.0f, nightAmbientIntensity, nightAmbientIntensity, false);
			}

			//
		}
	}

	void ToggleDayNight()
	{
		print("ToggleDayNight() - " + Time.time);

		if (WorldState == WorldState.Day)
		{
			SetNightMode();
		}
		else if (WorldState == WorldState.Night)
		{
			SetDayMode();
		}
	}

	void SetNightMode()
	{
		if (WorldState == WorldState.Day)
		{
			//DirectionalLight.enabled = false;

			//RenderSettings.ambientIntensity = 0.0f;

			for (int i = 0; i < LightPosts.Count; ++i)
			{
				GameObject TempGO = LightPosts[i].GetComponent<LampPostScript>().LightHolderObjectRef;
				if (TempGO != null)
				{
					TempGO.SetActive(true);
				}
			}

			WorldState = WorldState.Night;
		}
	}

	void SetDayMode()
	{
		if (WorldState == WorldState.Night)
		{
			//DirectionalLight.enabled = true;

			//RenderSettings.ambientIntensity = 1.0f;

			for (int i = 0; i < LightPosts.Count; ++i)
			{
				GameObject TempGO = LightPosts[i].GetComponent<LampPostScript>().LightHolderObjectRef;
				if (TempGO != null)
				{
					TempGO.SetActive(false);
				}
			}

			WorldState = WorldState.Day;
		}
	}
}
