using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public Transform levels;

	private void Start()
	{
        //LevelReferences();
    }

	public void NewGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
	}

	public void LoadGame(string loadGame)
	{
		SceneManager.LoadScene(loadGame, LoadSceneMode.Single);
	}
	/*
	public void ContinueGame()
	{

	}
	*/
	//private void LevelReferences()
	//{
	//    if (levels == null) Debug.Log("Nothing is assigned yet");
	//    int i = 0;
	//    foreach (Transform level in levels)
	//    {
	//        int currentIndex = i;

	//        Button b = level.GetComponent<Button>();
	//        b.onClick.AddListener(() => SelectLevel(currentIndex));
	//        i++;
	//    }
	//}

	//public void SelectLevel(int currentIndex)
	//{
	//    SceneManager.LoadScene(currentIndex + 1, LoadSceneMode.Single);
	//}

	public void QuitGame()
	{
		Debug.Log("Quit Game");
		Application.Quit();
	}
}
