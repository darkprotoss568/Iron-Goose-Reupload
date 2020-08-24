using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class EventDialogue : EventParentObject
{
	public string _dialogueClipsDirectory = "VOs/Tutorial/";
	public TextAsset _voiceLinesFile;                                                               // .txt file containing data on the voice lines in the dialogue         
	public TextAsset _portraitsFile;                                                                // .txt file containing data on portraits used in the dialogue
	public TextAsset _voiceClipsFile;                                                               // .txt file containing data on the voice clips used in the dialogue

	private List<string> _voiceLines = new List<string>();                                          // List containing the voice lines used in the dialogue
	//private List<Sprite> _portraits = new List<Sprite>();                                           // List containing portrait sprites used in the dialogue
	private List<AudioClip> _voiceClips = new List<AudioClip>();                                    // List containing voice clips used in the dialogue

	//private Dictionary<string, Sprite> _portraitDictionary = new Dictionary<string, Sprite>();      // Dictionary storing information on portraits. Currently Unused

	public int[] _tutorialAddressArray;                                                             // Array containing info on which line a tutorial will pop up
	public GameObject[] _tutorialArray;                                                             // Array of tutorial events that can pop up throughout the dialogue

    [SerializeField]
    private int[] _eventTriggersAddresses;
    [SerializeField]
    private EventParentObject[] _eventsToTrigger = new EventParentObject[0];

	public GameObject _dialogueBox;                                                                 // Reference to the dialogue box

	private GameObject _tutorialSkipButton;
	private GameObject _dialogueSkipButton;
	private GameObject _lineSkipButton;

	private int _lineID = -1;                                                                       // Currently displayed line of the dialogue
	private AudioSource _voicePlayer;                                                               // Reference to the audio source the voice clips will be played from

	private UnityEngine.UI.Text _dialogueText;                                                      // Reference to the text component of the dialogue box         

	private GameObject _tutorialEvent = null;                                                              // Current Tutorial event
	private bool _bTutorialCompleted;                                                               // Indicated whether the previously created Tutorial Event has been completed

	private bool _bIsInitialised_BBB = false;

    private bool _dialogueCompleted = false;
    [SerializeField]
    private bool _forceUnlockSelectionOnCompletion = false;
	// Use this for initialization
	// Use this for initialization
	public override void Start()
	{
		base.Start();

		if (_bIsInitialized)
		{
			// Get the HUDCanvas Object
			GameObject HUDCanvas = GameObject.Find("HUDCanvas");
			//Create the dialogue box
			_dialogueBox = Instantiate(_dialogueBox, Vector3.zero, Quaternion.identity);
			_dialogueBox.transform.SetParent(HUDCanvas.transform);

			// Get the rect transform of the dialogue box object
			RectTransform rect = _dialogueBox.GetComponent<RectTransform>();

			// Set the position of the dialogue box based on the scale
			float dialogueBoxPosX = 0 + Screen.width * 0.02f;
			float dialogueBoxPosY = 0 + Screen.height * 0.48f;
			// Position the dialogue box
			rect.position = new Vector2(dialogueBoxPosX, dialogueBoxPosY);
			rect.localScale = new Vector3(1, 1, 1);
			// Add a onClick Listener to the dialogue box to progress the dialogue when clicked
			_dialogueBox.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ProgressDialogue_Check());
			//Get the dialogue text
			_dialogueText = _dialogueBox.transform.Find("DialogueText").GetComponent<UnityEngine.UI.Text>();
			// Get the audio source to player the voice from
			_voicePlayer = _worldScript.AS_2DMainAudioSource;

			_tutorialSkipButton = _dialogueBox.transform.Find("SkipTutorial").gameObject;
			_tutorialSkipButton.GetComponent<Button>().onClick.AddListener(() => SkipTutorial());
            _tutorialSkipButton.SetActive(false);
			_dialogueSkipButton = _dialogueBox.transform.Find("SkipDialogue").gameObject;
			_dialogueSkipButton.GetComponent<Button>().onClick.AddListener(() => SkipDialogue());
            _dialogueSkipButton.SetActive(false);
			_lineSkipButton = _dialogueBox.transform.Find("Continue").gameObject;
			_lineSkipButton.GetComponent<Button>().onClick.AddListener(() => ProgressDialogue_Check());
			//Disable the dialogue box until the player triggers the event
			_dialogueBox.SetActive(false);

			//Get line data from specified file
			InitializeFromFile();
		}
	}

	// Update is called once per frame
	public override void Update()
	{
		if (!PauseMenu.isPaused)
		{
			base.Update();
		}
		else
		{
			if (_voicePlayer.isPlaying)
			{
				_voicePlayer.Pause();
			}
		}
	}

    private void LateUpdate()
    {
        if (_bIsActivated)
        {
            _worldScript.OverrideDialogueEvent(this);
            if (!_voicePlayer.isPlaying)
                _voicePlayer.UnPause();
            // Set the dialogue box to active
            _dialogueBox.SetActive(true);

            _worldScript.GameplayScript.bIsTrainStoppedAtStation = true;
            //Automatically progress the dialogue if _lineID < 0 or a tutorial has been completed
            if (_lineID < 0 || _bTutorialCompleted)
            {
                ProgressDialogue_Check();
                _bTutorialCompleted = false;
            }

            if (!_voicePlayer.isPlaying)
            {
                ProgressDialogue_Check();
            }

            //_tutorialSkipButton.SetActive(_tutorialEvent != null);
            _lineSkipButton.SetActive(_tutorialEvent == null);
        }

        if (!_bIsInitialised_BBB)
        {
            _worldScript.GameplayScript.SetAllElementsInactive();
            _bIsInitialised_BBB = true;
        }
    }

    private void ProgressDialogue_Check() // [Michael, 26-5-18]
	{
		if (_voicePlayer.isPlaying)
		{
			_voicePlayer.Stop();
		}

		ProgressDialogue();
	}

	/// <summary>
	/// Progress the dialogue lines
	/// </summary>
	public void ProgressDialogue()
	{
		if (_tutorialEvent == null)
		{
			_worldScript.GameplayScript.BTutorialRunning = true;

			//Increment Line
			_lineID++;

            
			// Check if _lineID is within the _voiceLines.Count range
			if (_lineID < _voiceLines.Count)
			{
                _dialogueText.text = _voiceLines[_lineID];
                AudioClip clip = null;
                try
                {
                    clip = _voiceClips[_lineID];
                }
                catch
                {

                }
                
                // can be used to adjust tutorial voice volume
                _voicePlayer.PlayOneShot(_voiceClips[_lineID], 1.1f);



				// NOTE: Could improve for optimization here
				if (_tutorialEvent == null)
				{
					for (int i = 0; i < _tutorialAddressArray.Length; i++)
					{
						if (_lineID == _tutorialAddressArray[i])
						{
                            Vector3 spawnPos = _locomotiveRef.transform.transform.TransformPoint(new Vector3(0, 3, 0));
                            // Create the tutorial event
                            _tutorialEvent = Instantiate(_tutorialArray[i], spawnPos, gameObject.transform.rotation);
                            _tutorialEvent.GetComponent<EventTutorial>().SetAttachedDialogueEvent(this);
                        }
					}
				}
                for (int i = 0; i < _eventTriggersAddresses.Length; i++)
                {
                    if (_lineID == _eventTriggersAddresses[i])
                    {
                        // Create the tutorial event
                        try
                        {
                            _eventsToTrigger[i].ForceActivate();
                            _eventsToTrigger[i].gameObject.transform.SetParent(null);
                        } catch
                        {
                        }
                    }
                }
            }
			else
			{
                // If not, close the dialogue event
                _dialogueCompleted = true;
			}
		}
	}

	/// <summary>
	/// Complete the currentTutorial
	/// </summary>
	public void CompleteTutorial()
	{
		_bTutorialCompleted = true;
		//_worldScript.GameplayScript.bIsTrainStoppedAtStation = false;
		_worldScript.GameplayScript.SelectableConsPlatforms.Clear();
	}

	public void CloseDialogueEvent()
	{
        SkipTutorial();
		_worldScript.GameplayScript.BTutorialRunning = false;

		_worldScript.GameplayScript.SetAllElementsActive(_forceUnlockSelectionOnCompletion);

		_voicePlayer.Stop();
        if (_dialogueBox != null)
        {
            Destroy(_dialogueBox);
        }
		Destroy(this);
	}

    protected override bool CheckVictoryCondition()
    {
        if (_dialogueCompleted)
        {
            CloseDialogueEvent();
        }

        return _dialogueCompleted;
    }
    /// <summary>
    /// Acquire data for the voice lines text, voice clips and portrait data from the specified TextAsset files
    /// </summary>
    public void InitializeFromFile()
	{
		// TODO: Load Portrait Data
		//

		//Acquire Voice Lines Data from the file
		List<string> temporaryList = new List<string>(_voiceLinesFile.text.Split('\n'));
		// Process the voice lines
		for (int i = 0; i < temporaryList.Count; i++)
		{
			string[] line = temporaryList[i].Split(new string[] { "::" }, System.StringSplitOptions.None);
			if (line.Length > 1)
			{
				_voiceLines.Add(line[1]);
			}
		}

		//Acquire voice clip files names from the files
		temporaryList = new List<string>(_voiceClipsFile.text.Split(new string[] { "\r\n" }, System.StringSplitOptions.None));
		//Process the files names
		for (int i = 0; i < temporaryList.Count; i++)
		{
			// Get the sound file directory
			string directory = _dialogueClipsDirectory + temporaryList[i];
			AudioClip clip = Resources.Load<AudioClip>(directory);

			// Add the voice clip to the list
			_voiceClips.Add(clip);
		}

		// TODO: Add Portraits
	}

	public void SkipTutorial()
	{
		if (_tutorialEvent != null)
		{
			Destroy(_tutorialEvent);
			_bTutorialCompleted = true;
		}
	}

	public void SkipDialogue()
	{
		SkipTutorial();
        _dialogueCompleted = true;
	}
}

