using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerControllerScript : MonoBehaviour
{
    private WorldScript _worldScript;
    public bool _bIsConstructionInputEnabled = true;
    public PauseMenu _pauseMenu;
	// Use this for initialization
	void Start ()
    {
        // Assuming there is only one world script in the scene, get a reference to the world script
        try
        {
            _worldScript = gameObject.GetComponent<WorldScript>();
        }
        catch (NullReferenceException)
        {
            Debug.Log("Scene not finished loading");
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_worldScript == null)
        {
            Start();
        }
        else
        {
            GetDebugInput();
            if (_worldScript.ConstructionManager.bIsConsMenuOpen)
            {
                GetConstructionMenuInput();
            }
            else
            {
                GetCameraMovementInput();
                GetPauseMenuInput();
            }
        }
	}

    public void GetConstructionMenuInput()
    {
        ConstructionManagerScript _consManager = _worldScript.ConstructionManager;
        if (_bIsConstructionInputEnabled)
        {
            ConstructionMenuObjScript consMenu = _consManager.GetOpenedConstructionMenu();
            for (int i = 0; i < consMenu.GetArchetypesList().Count; i++)
            {
                // Check for build hotkey input press
                if (Input.GetKeyDown(consMenu._hotKeysList[i]))
                {
                    consMenu.ClearDescriptionBoxWarning();
                    consMenu.LoadDescriptionBox(i);
                    consMenu.SelectConsOptionByIndex(i);
                    return;
                }
            }

            // Check whether the player clicks the right mouse button or clicks the left mouse button but outside of the menu to close it
            if (Input.GetMouseButtonDown(1) || (Input.GetMouseButtonDown(0) && !consMenu.CheckIfPosWithinConsMenuRadius(Input.mousePosition))
                || Input.GetKeyDown(KeyCode.Escape))
            {
                if (consMenu != null)
                {
                    consMenu.CloseConsMenu();
                    return;
                }
            }

            
        }
    }

    public void GetCameraMovementInput()
    {
        float vAxis = Input.GetAxis("Vertical");
        float hAxis = Input.GetAxis("Horizontal");
        float mouseAxis = Input.mouseScrollDelta.y;
        _worldScript.RTSCameraController.PanCamera_T2(vAxis, hAxis);
        _worldScript.RTSCameraController.ManageZoom(mouseAxis);
    }


    public void GetDebugInput()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Scene currentScene = SceneManager.GetActiveScene();
                    SceneManager.LoadScene(currentScene.buildIndex, LoadSceneMode.Single);
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    SceneManager.LoadScene(0, LoadSceneMode.Single);
                }
            }
        }
    }

    private void GetPauseMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseMenu.isPaused)
            {
                _pauseMenu.Resume();
            }
            else _pauseMenu.Pause();
        }
    }
}
