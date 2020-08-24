using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialEventType
{
	Input,
	KeyInput,
	Selection,
	Construction,
    SpecificSelection
}

public class EventTutorial : EventParentObject
{

	public string _tutorialMessage;                                         // The message displayed for the tutorial
	public GameObject _messageBox;                                          // Reference to the message box used for this tutorial


	public TutorialEventType _tutorialType;                                 // Type of tutorial. Determine how victory conditions are checked
	public string[] _eventInputStrings;                                     // Types of axis input required from the player
	public KeyCode[] _eventKeyInputs;                                       // Keys the player is required to press
	private int _currentOffset = 0;                                         // Tracks the player's current progress to completing the tutorial
	public GameObject[] _objectConstructionTypes;                           // Types of object the player is required to construct

	public int _desiredOffset = 100;                                        // The threshold the player is required to pass to complete the tutorial
    
    [Header("Object highlight management")]
	public GameObject[] _highlightObjectType;                               // Types of objects that will be highlighted
    [SerializeField]
    private int _tutorialHighlightObjCode;                     
	private List<GameObject> _highlightObjects = new List<GameObject>();                             //
	private List<Color32> _highlightObjectsOriginalColor = new List<Color32>();
	private List<Material> _highlightObjectsOriginalMat = new List<Material>();
	private float _flashTimer = 0.0f;
	public float _flashPeriod = 0.01f;
	public GameObject[] _buttonPrompts;

	public bool _bCanSelectOnlyLocomotiveConsPlatform = false;
	public bool _bCanSelectOnlyNonLocomotiveConsPlatforms = false;

	public bool _bActivateMoveCamera = false;
	public bool _bActivateRotateCamera = false;
	public bool _bActivateSelection = false;
	public bool _bActivateCameraTargetSnapping = false;
	public bool _bActivateCameraZoom = false; // TODO: Set to false and public once zoom has been added to the tutorial [Michael, 26-5-18]

    [SerializeField]
    private bool _bLockSelectionAfterCompletion = false;
    private List<ConsPlatformScript> _consPlatformsToRestore= new List<ConsPlatformScript>();
	private Material _whiteMaterial;
    
    private EventDialogue _attachedDialogueEvent;
	// Use this for initialization
    
	public override void Start()
	{
		base.Start();
		if (_bIsInitialized)
		{
            _worldScript.CurrentEventTutorial = this;
			_whiteMaterial = Resources.Load("Materials/colouredLight_3") as Material;

			// Get the HUDCanvas object
			GameObject HUDCanvas = GameObject.Find("HUDCanvas");
            RectTransform screenRect = HUDCanvas.GetComponent<RectTransform>();
            //Create a message box
            _messageBox = Instantiate(_messageBox, Vector3.zero, Quaternion.identity,HUDCanvas.transform);
			_messageBox.GetComponentInChildren<UnityEngine.UI.Text>().text = "";

			// Get the RectTransform component of the message box
			RectTransform rect = _messageBox.GetComponent<RectTransform>();
			// (Not yet used) Set the scale of the box based on the screen dimensions (might need to change so that these are scaled down instead of up)
			// Set the size of the box based on the screen dimensions
			rect.sizeDelta = new Vector2(screenRect.rect.width, rect.sizeDelta.y);
			// Set the position of the dialogue box (currently not scaled)
			float messageBoxPosX = Screen.width/2; // Always set in the center
			float messageBoxPosY = Screen.height*(1- 0.1f) - rect.sizeDelta.y*screenRect.localScale.y*1.5f;

			//Debug.Log(messageBoxPosY);
			// Position the dialogue box
			rect.position = new Vector2(messageBoxPosX, messageBoxPosY);
            rect.localScale = new Vector3(1, 1, 1);
			_messageBox.SetActive(false);

            AcquireHighlightObj();
			
			// Create highlighted buttons
			if (_buttonPrompts.Length > 0)
			{
				for (int i = 0; i < _buttonPrompts.Length; i++)
				{
					_buttonPrompts[i] = Instantiate(_buttonPrompts[i], Vector3.zero, Quaternion.identity, HUDCanvas.transform);
                    RectTransform buttRect = _buttonPrompts[i].GetComponent<RectTransform>();
                    buttRect.localScale = new Vector3(1, 1, 1);
					// Set position of each button prompts. Assuming that all button prompts are of the same size (Not scaled yet)
					float buttonPosX;
					if (i == 0)
					{
						buttonPosX = Screen.width/2 - (buttRect.sizeDelta.x* screenRect.localScale.x / 2) * (_buttonPrompts.Length - 1);
					}
					else
					{
						buttonPosX = _buttonPrompts[i - 1].GetComponent<RectTransform>().position.x + _buttonPrompts[i - 1].GetComponent<RectTransform>().sizeDelta.x*screenRect.localScale.x;
					}
					float buttonPosY = messageBoxPosY - buttRect.sizeDelta.y*screenRect.localScale.y/ 2 - Screen.height * 0.02f;

					buttRect.position = new Vector2(buttonPosX, buttonPosY);
					// Set original color.
					_highlightObjects.Add(_buttonPrompts[i]);
					_highlightObjectsOriginalColor.Add(_buttonPrompts[i].GetComponent<UnityEngine.UI.Image>().color);
				}
			}
		}
	}

	// Update is called once per frame
	public override void Update()
	{
		base.Update();
		if (_bIsActivated)
		{
            _messageBox.SetActive(true);
			_messageBox.GetComponentInChildren<UnityEngine.UI.Text>().text = _tutorialMessage;

			FlashHighlightedObjects();

			if (_bActivateMoveCamera)
			{
				_worldScript.RTSCameraController.BCanMoveCamera = true;
				_bActivateMoveCamera = false;
			}

			if (_bActivateRotateCamera)
			{
				_worldScript.RTSCameraController.BCameraRotationActive = true;
				_bActivateRotateCamera = false;
			}

			if (_bActivateSelection)
			{
				_worldScript.GameplayScript.BSelectionActive = true;
				_bActivateSelection = false;
			}

			if (_bActivateCameraTargetSnapping)
			{
				_worldScript.RTSCameraController.BCameraTargetSnappingActive = true;
				_bActivateCameraTargetSnapping = false;
			}

			if (_bActivateCameraZoom)
			{
				_worldScript.RTSCameraController.BZoomActive = true;
				_bActivateCameraZoom = false;
			}
		}


	}

    private void AcquireHighlightObj()
    {
        // Get Objects to be highlighted
        List<GameObject> allObjects = _worldScript.GetAllTGOsInWorld_AsGOs();

        for (int i = 0; i < _highlightObjectType.Length; i++)
        {
            for (int j = 0; j < allObjects.Count; j++)
            {
                try
                {
                    if (allObjects[j].name.Length >= _highlightObjectType[i].name.Length)
                    {
                        if (_highlightObjectType[i].name == allObjects[j].name.Substring(0, _highlightObjectType[i].name.Length))
                        {
                            ConsPlatformScript consPlatformScript = allObjects[j].GetComponent<ConsPlatformScript>();
                            if (consPlatformScript == null)
                            {
                                Renderer renderer = allObjects[j].GetComponent<Renderer>();
                                _highlightObjects.Add(allObjects[j]);
                                _highlightObjectsOriginalColor.Add(renderer.material.color);
                                _highlightObjectsOriginalMat.Add(renderer.material);
                            }
                            else
                            {
                                if (consPlatformScript.CheckAvailableToTutorial(_tutorialHighlightObjCode))
                                {
                                    Renderer renderer = allObjects[j].GetComponent<Renderer>();
                                    _highlightObjects.Add(allObjects[j]);
                                    _highlightObjectsOriginalColor.Add(renderer.material.color);
                                    _highlightObjectsOriginalMat.Add(renderer.material);
                                }
                                else
                                {
                                    consPlatformScript.SetSelectableByPlayer(false);
                                    _consPlatformsToRestore.Add(consPlatformScript);
                                }

                            }
                        }
                    }
                }
                catch (NullReferenceException exception)
                {
                    Debug.Log(exception.StackTrace);
                }
            }
        }
        
    }
	public void OnDestroy()
	{
		RestoreHighlightObjectColors();
		//Destroy the message box and tell the dialogue event that the tutorial has been completed.
		Destroy(_messageBox);
		for (int i = 0; i < _buttonPrompts.Length; i++)
		{
			Destroy(_buttonPrompts[i]);
		}
        RestoreSelectableObjs();
        if (_bLockSelectionAfterCompletion)
        {
            _worldScript.GameplayScript.BSelectionActive = false;
        }
		if (_attachedDialogueEvent) // [Michael, 26-5-18] -- To fix null ref exception [Shin, 22-9-18] Change because FindObjectOfType is a bit too expensive
		{
            _attachedDialogueEvent.CompleteTutorial();
		}
	}

	protected override bool CheckVictoryCondition()
	{
		bool result = false;
		switch (_tutorialType)
		{
			case TutorialEventType.Input:
				CheckInput();
				break;
			case TutorialEventType.KeyInput:
				CheckKeyInput();
				break;
			case TutorialEventType.Selection:
				CheckObjectSelection();
				break;
			case TutorialEventType.Construction:
				break;
            case TutorialEventType.SpecificSelection:
			default:
				break;
		}

		if (_currentOffset >= _desiredOffset)
			result = true;
		return result;
	}

    private void RestoreSelectableObjs()
    {
        for (int i = 0; i < _consPlatformsToRestore.Count; i++)
        {
            _consPlatformsToRestore[i].SetSelectableByPlayer(true);
        }
    }
	private void FlashHighlightedObjects()
	{
		Color32 whiteColor = Color.white;
		Color32 greenColor = new Color(0, 1f, 0.25f, 1.0f);
		_flashTimer += Time.deltaTime;
		if (_flashTimer > _flashPeriod)
		{
			for (int i = 0; i < _highlightObjects.Count; i++)
			{
                Renderer renderer = _highlightObjects[i].GetComponent<Renderer>();

                // Switch between the normal material and the highlight material
                if (renderer != null)
				{
                    if (renderer.material == _highlightObjectsOriginalMat[i])
					{
                        renderer.material = _whiteMaterial;
					}
					else
					{
                        renderer.material = _highlightObjectsOriginalMat[i];
					}
				}
				else if (_highlightObjects[i].GetComponent<UnityEngine.UI.Image>())
				{
					if (_highlightObjects[i].GetComponent<UnityEngine.UI.Image>().color == _highlightObjectsOriginalColor[i])
					{
						_highlightObjects[i].GetComponent<UnityEngine.UI.Image>().color = greenColor;
					}
					else
					{
						_highlightObjects[i].GetComponent<UnityEngine.UI.Image>().color = _highlightObjectsOriginalColor[i];
					}
				}
			}
			_flashTimer = 0f;
		}
	}

	private void RestoreHighlightObjectColors()
	{
		for (int i = 0; i < _highlightObjects.Count; i++)
		{
			if (_highlightObjects[i] != null)
			{
				if (_highlightObjects[i].GetComponent<Renderer>())
				{
					//_highlightObjects[i].GetComponent<Renderer>().material.color = _highlightObjectsOriginalColor[i];
					_highlightObjects[i].GetComponent<Renderer>().material = _highlightObjectsOriginalMat[i];
				}
				else if (_highlightObjects[i].GetComponent<UnityEngine.UI.Image>())
				{
					_highlightObjects[i].GetComponent<UnityEngine.UI.Image>().color = _highlightObjectsOriginalColor[i];
				}
			}
		}
	}
	/// <summary>
	/// Check if the player has used a correct type of Axis Input
	/// </summary>
	/// <returns></returns>
	private bool CheckInput()
	{
		bool result = false;
		for (int i = 0; i < _eventInputStrings.Length; i++)
		{
			if (Input.GetAxis(_eventInputStrings[i]) != 0.0f)
			{
				_currentOffset++;
				result = true;
			}
		}

		return result;
	}

	/// <summary>
	/// Check if the player has pressed a button as specified.
	/// </summary>
	/// <returns></returns>
	private bool CheckKeyInput()
	{
		bool result = false;
		for (int i = 0; i < _eventKeyInputs.Length; i++)
		{
			if (Input.GetKey(_eventKeyInputs[i]))
			{
				_currentOffset++;

				result = true;
			}
		}


		return result;
	}

	/// <summary>
	/// Check if the object constructed is the same as the type the player is required to build
	/// </summary>
	/// <param name="archetype"></param>
	/// <returns></returns>
	public bool CheckObjectConstruction(GameObject archetype)
	{
		bool result = false;
		if (archetype != null)
		{
			for (int i = 0; i < _objectConstructionTypes.Length; i++)
			{
				if (GameObject.ReferenceEquals(archetype, _objectConstructionTypes[i]))
				{
					result = true;
					_currentOffset++;
				}
			}
		}
		return result;
	}

	private bool CheckObjectSelection()
	{
		for (int i = 0; i < _highlightObjects.Count; i++)
		{
			if (_worldScript.GameplayScript.IsObjPlayerSelected(_highlightObjects[i]))
			{
				_currentOffset++;
				return true;
			}
		}

		return false;
	}

    public void SetAttachedDialogueEvent(EventDialogue dialogueScript)
    {
        _attachedDialogueEvent = dialogueScript;
    }

	/*public void SetObjectInConstruction(GameObject archetype)
	{
		 _objectInConstruction = archetype;
	}*/
}
