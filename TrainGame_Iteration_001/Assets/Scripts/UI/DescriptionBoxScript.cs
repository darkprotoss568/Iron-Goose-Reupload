using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DescriptionType
{
	Construction,
    Unit
}

public class DescriptionBoxScript : UIOverlayObjScript
{
    
	protected string _hotKeyTag = "";                         // The hotkey used to construct the turret/archetype
	

    [SerializeField]
    private GameObject _propertiesIconPrefab;               // The prefab for the properties icon

    [Header("Icons")]
    [SerializeField]
    private Sprite[] _armorTypesImg = new Sprite[2];        // Sprites for Armor type. 0 = light, 1 = heavy
    [SerializeField]
    private Sprite[] _ordnanceTypesImg = new Sprite[3];     // Sprites for ordnance types. 0 = ballistics, 1 = Anti-Armor, 2 = explosives
    [SerializeField] 
    private Sprite _splashDamageImg;                        // Sprite for the splash damage icon

    [Header("TextIcons")]
    public Color _highlightTextColor;                       // The color of the highlighted text parts in the description box
    protected string _highlightTextColorTag;                  // A premade html color tag for highlighted text parts
    [Header("Bonus Damage strings")]
    [SerializeField]
    private string[] _bonusDamageStrings = new string[3];   // Currently obsolete. Used for explaining what types of ammunition 

    private GameObject _statsContainer;                     // The child object that contains all the stats indicator
    private GameObject _splashRadiusStatsObj;               // The child object that contains the spash radius stats indicator
    private GameObject _iconsContainerObj;                  // The child object that contains the icons
    protected Text _headerText;                               // The Text object that contains the header of the description box
    protected Text _descriptionText;                          // The Text object that contains the body of the description
    protected Text _costText;                                 // The Text object that indicates the build cost
    private Text _HPText;                                   // The Text object that indicates the HP of the construction/hovered on obj
    private Text _damageText;                               // The Text object that indicates the damage
    private Text _firingRangeText;                          // The Text object that indicates the firing range
    private Text _firingRateText;                           // The Text object that indicates the turret cooldown time
    private Text _splashRadiusText;                         // The Text object that indicates the splash radius size

    protected bool _hasWarning;                               // True if the description already has a warning added
    // Use this for initialization
    public override void Start()
	{

        base.Start();
        // Get references to child objects that need to be managed on this description box
        LoadManagedObjects();
        // Setup the highlight Text color html tag
        _highlightTextColorTag = "<color=#" + ColorUtility.ToHtmlStringRGBA(_highlightTextColor)+ ">";
        // Deactivate the description box by default
        if (_positionType == PositioningType.FollowAnObject)
            Deactivate();
	}

    /// <summary>
    /// Get the references to all the child objects that need to be managed on this description box
    /// </summary>
    protected virtual void LoadManagedObjects()
    {
        Transform headerContainer = gameObject.transform.Find("HeaderContainer");
        _descriptionText = gameObject.transform.Find("DescriptionText").GetComponent<Text>();
        _headerText = headerContainer.Find("Header").Find("HeaderText").GetComponent<Text>();
        _costText = headerContainer.Find("Header").Find("CostText").GetComponent<Text>();

        Transform statsContainer = gameObject.transform.Find("StatsContainer");
        _HPText = statsContainer.Find("HitPoints").GetComponentInChildren<Text>();
        _damageText = statsContainer.Find("Damage").GetComponentInChildren<Text>();
        _firingRangeText = statsContainer.Find("Range").GetComponentInChildren<Text>();
        _firingRateText = statsContainer.Find("FiringRate").GetComponentInChildren<Text>();
        _splashRadiusText = statsContainer.Find("SplashRadius").GetComponentInChildren<Text>();

        _iconsContainerObj = headerContainer.Find("IconsContainer").gameObject;
        _splashRadiusStatsObj = statsContainer.Find("SplashRadius").gameObject;
    }

	// Update is called once per frame
    protected override void Update()
	{
        base.Update();

		if (PauseMenu.isPaused) return;

        // Make the desription box always on top of all other UI elements
		transform.SetAsLastSibling();
		
        // Deactivate the description box if the reference to the follow object is null
		if (_followObject == null && _positionType == PositioningType.FollowAnObject)
		{
            Deactivate();
		}
	}

	
    /// <summary>
    /// Load the content of the description box
    /// </summary>
    /// <param name="archetype">The TrainGameObject the box will display information on</param>
    /// <param name="descriptionType">Type of description. Either for hover on or construction</param>
    /// <param name="followTarget">The GameObject the description box will follow</param>
    /// <param name="hotKeyString">The hotkey used to construct the archetype</param>
    /// <param name="staticOffset">The static offset between the description box and the followed object</param>
	public virtual void LoadDescBoxContent(TrainGameObjScript archetype, GameObject followTarget, string hotKeyString, Vector2 staticOffset, DescriptionType descriptionType)
	{
        // Set the hotkey tag used for the description box
        SetHotKeyTag(hotKeyString);

        switch (descriptionType)
		{
			case DescriptionType.Construction:
				ProcessConsDescriptionText(archetype);
                break;
			default:
				break;
        }

        ProcessDescriptionIcons(archetype, descriptionType);
        
        SetFollowObject(followTarget, true, staticOffset);

        Activate();
    }

    /// <summary>
    /// Set up the description icons
    /// </summary>
    /// <param name="archetype"></param>
    /// <param name="descType"></param>
    private void ProcessDescriptionIcons(TrainGameObjScript archetype, DescriptionType descType)
    {
        if (descType == DescriptionType.Construction)
        {
            // Check if the archetype can be casted to a TurretScriptParent object class
            ProjectileBasedTurretScript turret = null;
            try
            {
                turret = archetype as ProjectileBasedTurretScript;
            }
            catch
            {
                Debug.Log("Not a turret");
            }

            // TODO: Consider refactoring these into a single method
            // Set the icon for the armor type          
            int armorIndex = (int)archetype._armorType;
            if (armorIndex < _armorTypesImg.Length)
            {
                GameObject armorTypeIcon = Instantiate(_propertiesIconPrefab, _iconsContainerObj.transform);
                if (_armorTypesImg[armorIndex] != null)
                    armorTypeIcon.GetComponent<Image>().sprite = _armorTypesImg[armorIndex];
            }

            // Set the icons for the turret objects
            if (turret != null)
            {
                //  Set the icon for the ordnance type
                int ordnanceIndex = (int)turret.DamageType;
                if (ordnanceIndex < _ordnanceTypesImg.Length)
                {
                    GameObject ordnanceTypeIcon = Instantiate(_propertiesIconPrefab, _iconsContainerObj.transform);
                    if (_ordnanceTypesImg[ordnanceIndex] != null)
                        ordnanceTypeIcon.GetComponent<Image>().sprite = _ordnanceTypesImg[ordnanceIndex];
                }

                // Set the icon to indicate whether the ordnance type deals splash damage
                if (turret.BDealsSplashDamage)
                {
                    GameObject splashDamageIcon = Instantiate(_propertiesIconPrefab, _iconsContainerObj.transform);
                    if (_splashDamageImg != null)
                        splashDamageIcon.GetComponent<Image>().sprite = _splashDamageImg;
                }
            }
        }
    }

    /// <summary>
    /// Process the text elements for the construction box
    /// </summary>
    /// <param name="archetype"></param>
	protected virtual void ProcessConsDescriptionText(TrainGameObjScript archetype)
	{
		ProjectileBasedTurretScript turret = null;
		try
		{
			turret = archetype as ProjectileBasedTurretScript;
		}
		catch
		{
			Debug.Log("Not a turret");
		}
        string header = String.Empty;
        
        // Header 1st line (Build Object name & Hotkey)
		header = "Build " + archetype._name + " [" +_highlightTextColorTag + _hotKeyTag + "</color>]" + Environment.NewLine;
        _headerText.text = header;
        // Header 2nd line (Build Cost)
        _costText.text = "Cost: " + _highlightTextColorTag + archetype.BuildCost + "</color>";

        _HPText.text = archetype.HitPoints.ToString();



        if (turret != null)
		{
            //Damage
			_damageText.text = turret.BaseDamage.ToString();
            if (turret.ShotsPerVolley > 1)
            {
                _damageText.text += "x" + turret.ShotsPerVolley.ToString();
            }
            if (_bonusDamageStrings[(int)turret.DamageType] != string.Empty)
            {
                _damageText.text += "(" + _highlightTextColorTag + "+" + turret.BonusDamage + "</color>) "; //+ _bonusDamageStrings[(int)turret.DamageType] + ")";
            }
            //Firing Range
            _firingRangeText.text = _highlightTextColorTag + turret.FiringRange + "</color> m";
            //FiringRate
            _firingRateText.text = turret.FiringRate.ToString() + " s";
            if (turret.BDealsSplashDamage)
            {
                _splashRadiusText.text = _highlightTextColorTag + turret.SplashRadius + "</color> m";
                _splashRadiusStatsObj.SetActive(true);
            }
            else
                _splashRadiusStatsObj.SetActive(false);         
		}

        //General Description 

        _descriptionText.text = archetype.Description;
	}

	
    /// <summary>
    /// Set the hotkey tag
    /// </summary>
    /// <param name="key"></param>
	protected void SetHotKeyTag(string key)
	{
		_hotKeyTag = key;
	}

    /// <summary>
    /// Clear the description box's content
    /// </summary>
    public virtual void Clear()
    {
        // Clear the text elements
        ClearWarning();
        _followObject = null;
        _hotKeyTag = string.Empty;
        _headerText.text = string.Empty;
        _descriptionText.text = string.Empty;
        _damageText.text = string.Empty;
        _HPText.text = string.Empty;
        _firingRangeText.text = string.Empty;
        _firingRateText.text = string.Empty;
        _splashRadiusText.text = string.Empty;
        
        // Clear the properties icons
        foreach (Transform child in _iconsContainerObj.transform)
        {
            Destroy(child.gameObject);
        }
    }
     
    public virtual void ClearWarning()
    {
        _hasWarning = false;
    }

    /// <summary>
    ///  Add a warning line to the description box
    /// </summary>
    /// <param name="warning"></param>
    public virtual void AddWarning(string warning)
    {
        if (!_hasWarning)
        {
            // Prepare the color and size of the warning line
            string line = "<color=red><size=30>" + warning + "</size></color>";
            _hasWarning = true;
            _descriptionText.text = line;
        }
        
    }

    /// <summary>
    /// Deactivate the description box
    /// </summary>
    public override void Deactivate()
    {
        /// Clear the box before deactivating
        Clear();

        base.Deactivate();
    }
}
