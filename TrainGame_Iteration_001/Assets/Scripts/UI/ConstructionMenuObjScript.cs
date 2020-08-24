using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ConstructionMenuObjScript : UIOverlayObjScript
{
    private GameObject _constructionPlatform;
    private WorldScript _worldScript;
    public KeyCode[] _hotKeysList;
    //public GameObject _constructionMenuPrefab;
    public GameObject _constructionButtonPrefab;
    private List<GameObject> _archetypes = new List<GameObject>();
    private List<GameObject> _constructionButtons = new List<GameObject>();

    public AudioClip ConsMenuOpenSound;
    public AudioClip ConsMenuCloseSound;
    public AudioClip ConsMenuConfirmSound;
    public AudioClip ConsMenuErrorSound;

    public float _distanceFromCenter;

    public GameObject _rangeProjector;

    [SerializeField]
    private float _menuTransitionTime;
    [Range(0.0f, 1.0f)]
    public float _slowMoRate;
    private float _previousTimescale = 1.0f;
    private bool _isInitialized = false;
    private bool _objectBuilt = false;

    private DescBoxOnConsMenuScript _descriptionBox;

    // Use this for initialization
    public override void Start()
    {
        //base.Start();
        _previousTimescale = Time.timeScale;
        if (_worldScript == null)
        {
            _worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
        }
        _descriptionBox = transform.GetChild(0).GetComponent<DescBoxOnConsMenuScript>();
        _worldScript.GameplayScript.SetTargetTimeScale(_slowMoRate, _menuTransitionTime);
    }

    public void InitializeConsMenu(int[] buttonsToLock)
    {
        _worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
        ConstructionManagerScript consManager = _worldScript.ConstructionManager;
        _constructionPlatform = consManager.CurrentSelectedConstructor;

        //List <GameObject> allowedConsArchetypes = new List<GameObject>();
        // Set the initial position of this construction menu
        RectTransform consMenuPos = gameObject.GetComponent<RectTransform>();
        //consMenuPos.localPosition = Vector2.zero;
        SetFollowObject(_constructionPlatform, false, Vector2.zero);
        ManagePosition();
        consMenuPos.localScale = new Vector2(1.0f, 1.0f);

        /*if (_constructionPlatform.GetComponent<ConsPlatformScript>() != null)
			_archetypes = _constructionPlatform.GetComponent<ConsPlatformScript>().GetConstructionArchetypesInList();
		else
			_archetypes = _worldScript.ConstructionArchetypes; // [Mike, 4-6-18] [Shin, 03-07-18]*/

        AddArchetypesToConsMenu();
        //_distanceFromCenter = gameObject.GetComponent<RectTransform>().sizeDelta.x/2 - _constructionButtonPrefab.GetComponent<RectTransform>().sizeDelta.x/2 - 20f;
        //
        CreateConsButtons();
        _worldScript.AS_2DMainAudioSource.PlayOneShot(ConsMenuOpenSound);
        _worldScript.ConstructionManager.SetConstructionActive(true);
        _worldScript.HUDScript.TriggerConstructionAestheticsBars();
        
        LockButtons(buttonsToLock);
        _isInitialized = true;
    }
    // Update is called once per frame
    protected override void Update()
    {
        if (_isInitialized)
            base.Update();

        //TODO: Monitor state of the player's resources to change how icons are rendered

        // Destroy the menu when the linked construction platform is destroyed

    }

    private void CreateConsButtons()
    {
        int count = _archetypes.Count;

        if (count == 0)
        {
            CloseConsMenu(); // Nothing left to build
        }
        else
        {
            // Create a construction button for each archetype

            Vector2 initialPos = new Vector2(0, _distanceFromCenter);
            float angleDifference = 360.0f / count;
            for (int i = 0; i < count; i++)
            {
                GameObject newConstructionButton = Instantiate(_constructionButtonPrefab, Vector3.zero, Quaternion.identity, gameObject.transform);
                //_constructionMenuImages.Add(go);
                //go.transform.SetParent(_HUDConsMenuObj.transform);

                RectTransform rect = newConstructionButton.GetComponent<RectTransform>();

                // Set the position
                
                TrainGameObjScript tgo = _archetypes[i].GetComponent<TrainGameObjScript>();


                ConsMenuButtonScript cmbs = newConstructionButton.GetComponent<ConsMenuButtonScript>();
                //cmbs._archetype = _archetypes[i];
                
                /*if (tgo)
                {
                    cmbs.SetImage(tgo._consIcon_GO);
                }*/

                Button btn = newConstructionButton.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectConsOption(cmbs._archetype));

                //cmbs.SetCostText(_archetypes[i].GetComponent<TrainGameObjScript>().BuildCost);
                //cmbs.SetHotKeyText();
                cmbs.InitializeButton(_archetypes[i], _hotKeysList[i], tgo._consIcon_GO);
                if (i == 0)
                {
                    rect.localPosition = initialPos;
                    cmbs.InitializePosition(gameObject.GetComponent<RectTransform>(), initialPos);
                }
                else
                {
                    RectTransform previousButton = _constructionButtons[i - 1].GetComponent<RectTransform>();
                    Vector2 relativePos = BBBStatics.RotateByAngle(previousButton.localPosition, angleDifference);
                    //Debug.Log(relativePos);
                    rect.localPosition = relativePos;
                    cmbs.InitializePosition(gameObject.GetComponent<RectTransform>(), relativePos);
                }

                _constructionButtons.Add(newConstructionButton);
            }

            /*for (int i = 0; i < _constructionButtons.Count; i++)
            {
                _constructionButtons[i].transform.SetParent(_worldScript.HUDScript.HUDCanvas.transform);
            }*/
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _constructionButtons.Count; i++)
        {
            Destroy(_constructionButtons[i]);
        }
        _worldScript.ConstructionManager.SetConstructionActive(false);
    }

    public bool CheckIfPosWithinConsMenuRadius(Vector2 point)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        float radius = rect.sizeDelta.x / 2 * _worldScript.HUDScript.HUDCanvas.GetComponent<RectTransform>().localScale.x;
        if (Vector2.Distance(rect.position, point) <= radius)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void AddArchetypesToConsMenu()
    {
        ConstructionManagerScript consManager = _worldScript.ConstructionManager;

        if (_constructionPlatform.GetComponent<ConsPlatformScript>()._bOnlyAllowBuildFixtures)
        {
            _archetypes.AddRange(new List<GameObject>(consManager._turretArchetypes));
        }

        if (_constructionPlatform.GetComponent<ConsPlatformScript>()._bOnlyAllowBuildMobile)
        {
            _archetypes.AddRange(new List<GameObject>(consManager._droneArchetypes));
        }
    }

	public void SelectConsOption(GameObject archetype)
	{
		EventTutorial tutorialObject = FindObjectOfType<EventTutorial>();
        if (archetype != null && _constructionPlatform != null)
        {
   
            ConsPlatformScript cps = _constructionPlatform.GetComponent<ConsPlatformScript>();
            if (cps != null)
            {
                bool buildSuccessful = false;
                string warning;
                // Not a drone = free // A drone = check if can afford
                if (archetype.GetComponent<Module>().CanBeBuilt(_worldScript.GameplayScript.PlayerResources, out warning))
                {
                    // Check if a tutorial object is present
                    if (tutorialObject != null)
                    {
                        // Check if the archetype is within the allowed types
                        if (tutorialObject.CheckObjectConstruction(archetype))
                        {
                            buildSuccessful = true;
                        }
                        else
                        {
                            warning = "Cannot build this right now";
                        }
                    }
                    else
                    {
                        buildSuccessful = true;
                    }
                }
               
                if (buildSuccessful)
                {
                    _worldScript.AS_2DMainAudioSource.PlayOneShot(ConsMenuConfirmSound);
                    cps.BeginBuildObject(archetype);
                    _objectBuilt = true;
                    CloseConsMenu();
                }
                else
                {
                    _descriptionBox.AddWarning(warning);
                    _worldScript.AS_2DMainAudioSource.PlayOneShot(ConsMenuErrorSound);
                }
            }
        }
	}

	public void CloseConsMenu()
	{
        ConstructionManagerScript consManager = _worldScript.ConstructionManager;
        consManager.DeactivateRangeProjector(true);
        // Reset the target time scale
        if (_objectBuilt)
        {
            _worldScript.GameplayScript.SetTargetTimeScale(1.0f, _menuTransitionTime);
        }
        else
        {
            _worldScript.GameplayScript.SetTargetTimeScale(_previousTimescale, _menuTransitionTime);
        }
        _worldScript.HUDScript.TriggerConstructionAestheticsBars();
        Destroy(gameObject);
	}

	public void SelectConsOptionByIndex(int i)
	{
		SelectConsOption(_archetypes[i]);
	}

	public List<GameObject> GetArchetypesList()
	{
		return _archetypes;
	}

	private void ActivateRecycleMode_InCommandScript()
	{
		_worldScript.CS.BInRecycleMode = true;
		CloseConsMenu();
	}

    /// <summary>
    /// Lock buttons 
    /// </summary>
    /// <param name="buttonsToLock">Array containing IDs of construction buttons to lock. Note: IDs start from 0 and go clockwise</param>
    public void LockButtons(int[] buttonsToLock)
    {
        for (int i = 0; i < buttonsToLock.Length; i++) 
        {
            for (int j = 0; j < _constructionButtons.Count; j++)
            {
                if (buttonsToLock[i] == j)
                {
                    _constructionButtons[j].GetComponent<ConsMenuButtonScript>().ChangeLockState(false);
                }
            }
        }
    }


    /// <summary>
    ///  Unlock all the buttons on the construction menu
    /// </summary>
    public void UnlockAllButtons()
    {
        for (int j = 0; j < _constructionButtons.Count; j++)
        {
            _constructionButtons[j].GetComponent<ConsMenuButtonScript>().ChangeLockState(true);
        }
    }

    /// <summary>
    /// Update the states of all the buttons when resource count is changed
    /// </summary>
    public void UpdateAllButtonsStates()
    {
        int playerResources = _worldScript.GameplayScript.PlayerResources;
        for (int j = 0; j < _constructionButtons.Count; j++)
        {
            _constructionButtons[j].GetComponent<ConsMenuButtonScript>().UpdateState(playerResources);
        }
    }

    public void LoadDescriptionBox(int consIndex)
    {
        ConsMenuButtonScript button = _constructionButtons[consIndex].GetComponent<ConsMenuButtonScript>();
        _descriptionBox.LoadDescBoxContent(_archetypes[consIndex].GetComponent<TrainGameObjScript>(), gameObject, _hotKeysList[consIndex].ToString(), Vector2.zero);
    }

    public void ClearDescriptionBoxWarning()
    {
        _descriptionBox.ClearWarning();
    }
}
