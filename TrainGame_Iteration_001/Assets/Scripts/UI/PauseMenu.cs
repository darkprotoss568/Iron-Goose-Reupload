using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	public static bool isPaused = false;

	public GameObject PauseUI;
	//public GameObject SettingUI;
	public GameObject HUDCanvas;
    float _timeScaleBeforePaused;

	void Start()
	{
		HUDCanvas = GameObject.Find("MainHolder").transform.Find("HUDCanvas").gameObject;
		//if (HUDCanvas == null) print("HUDCanvas == null @ PauseMenu");
		//else print("HUDCanvas != null @ PauseMenu");
	}

	void Update()
	{
		
	}

	public void Resume()
	{
		Time.timeScale = _timeScaleBeforePaused;
        transform.GetComponent<Image>().enabled = false;
        PauseUI.SetActive(false);
		HUDCanvas.SetActive(true);
		isPaused = false;
	}

	public void Pause()
	{
        _timeScaleBeforePaused = Time.timeScale;
        Time.timeScale = 0.0f;
        transform.GetComponent<Image>().enabled = true;        
        PauseUI.SetActive(true);
		HUDCanvas.SetActive(false);
		isPaused = true;
	}

	public void Menu()
	{
		isPaused = false;
		Time.timeScale = 1f;
		SceneManager.LoadScene(0, LoadSceneMode.Single);
	}

	public void QuitGame()
	{
		isPaused = false;
		Debug.Log("Quit Game");
		Application.Quit();
	}

	public void LoadMenu(string loadMenu)
	{
		isPaused = false;
		SceneManager.LoadScene(loadMenu, LoadSceneMode.Single);
	}
}
