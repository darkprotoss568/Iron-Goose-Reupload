using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public struct ConsButtonBorderPack
{
    public Sprite mainIcon;
    public Sprite hoverIcon;
}

public class ConsMenuButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private WorldScript _worldScript;
	public GameObject _archetype;
	private GameObject _descriptionBoxRef;
	private RectTransform _constructionMenu;
	private Text _costText;
	private string _hotKeyTag;
	Vector2 _relativePos;
    private bool _isUnlocked = true;
    private bool _isAvailable = true;

    [Header("Border Graphics")]
    [SerializeField]
    private ConsButtonBorderPack _availableIconPack;                    // The default border sprites pack
    [SerializeField]
    private ConsButtonBorderPack _insufficientResIconPack;              // Border sprites pack for when the player does not have enough resources
    private ConsIconPack _consIconPack;                                 // Construction icons pack for the image in the center

	void Start()
	{
		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
		HUDScript HUD = GameObject.Find("WorldScriptHolder").GetComponent<HUDScript>();
		_descriptionBoxRef = HUD.DescriptionBox;
        UpdateState(_worldScript.GameplayScript.PlayerResources);
    }

	void Update()
	{
		//FollowConstructionMenu();
	}

    /// <summary>
    /// Initialize the data on the construction button
    /// </summary>
    /// <param name="archetype">The archetype to build</param>
    /// <param name="key">The hotkey associated with the button</param>
    /// <param name="image">The image prefab for the icon in the center of the button</param>
    public void InitializeButton(GameObject archetype, KeyCode key, GameObject image)
    {
        _archetype = archetype;
        SetImage(image);
        SetConstructionIconPack();
        SetHotKeyText(key);
    }

    public void FollowConstructionMenu()
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		rt.localPosition = (Vector2)_constructionMenu.localPosition + _relativePos;
	}

    /// <summary>
    /// Modify the description box when the player hovers the mouse over the button
    /// </summary>
    /// <param name="eventData"></param>
	public void OnPointerEnter(PointerEventData eventData)
	{

		DescBoxOnConsMenuScript descriptionBox = _constructionMenu.transform.GetChild(0).gameObject.GetComponent<DescBoxOnConsMenuScript>();
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		if (_archetype != null)
			descriptionBox.LoadDescBoxContent(_archetype.GetComponent<TrainGameObjScript>(), gameObject, _hotKeyTag, new Vector2(rt.sizeDelta.x / 2, 0));
        ConstructionManagerScript consManager = _worldScript.ConstructionManager;
        TurretScriptParent turretComponent = _archetype.GetComponent<TurretScriptParent>();

        // Acivate the range projector if the archetype has a turret script component
        if (turretComponent != null)
        {
            consManager.ActivateRangeProjector(turretComponent.FiringRange, turretComponent.MaxAimAngle);
        }

        //Debug.Log("Created");
        _worldScript.CS.BHoveringMouseOverConsButton = true;
	}

    /// <summary>
    /// Modify the description box when the player moves the mouse off the button
    /// </summary>
    /// <param name="eventData"></param>
	public void OnPointerExit(PointerEventData eventData)
	{
        DescBoxOnConsMenuScript descriptionBox = _constructionMenu.transform.GetChild(0).gameObject.GetComponent<DescBoxOnConsMenuScript>();
        descriptionBox.ClearWarning();
        ConstructionManagerScript consManager = _worldScript.ConstructionManager;
        consManager.DeactivateRangeProjector(false);
		_worldScript.CS.BHoveringMouseOverConsButton = false;
	}

    /// <summary>
    /// Set the hotkey string associated with the button
    /// </summary>
    /// <param name="key"></param>
	public void SetHotKeyText(KeyCode key)
	{
		_hotKeyTag = key.ToString();
	}

	public void SetCostText(int cost)
	{
		_costText = gameObject.transform.Find("CostText").GetComponent<Text>();

		// [Mike, 5-6-18]
		if (cost >= 0)
			_costText.text = cost.ToString();
		else
			_costText.text = "";
	}

    /// <summary>
    /// Create an image prefab for the icon in the center of the button
    /// </summary>
    /// <param name="Image"></param>
    public void SetImage(GameObject Image)
    {
        Instantiate(Image, transform.position, transform.rotation, transform);
    }

    // Initialize the position of the button. TODO: THis is mostly obsolete and can be simplified and combined with InitializeButton.
	public void InitializePosition(RectTransform constructionMenu, Vector2 position)
	{

		_constructionMenu = constructionMenu;
		_relativePos = position;
	}

	public Vector2 RelativePosition
	{
		get
		{
			return _relativePos;
		}
	}

    /// <summary>
    /// Update the sprites of the button depending how much resource the player currently has
    /// </summary>
    /// <param name="playerResources"></param>
    public void UpdateState(int playerResources)
    {
        if (_isUnlocked)
        {
            Sprite currMainBorder = null;
            Sprite currHoverBorder = null;
            Sprite currIcon = null;
            bool stateChanged = false;
            if (playerResources < _archetype.GetComponent<TrainGameObjScript>()._buildCost)
            {
                // Change from available to unavailable if the player does not have enough resource
                if (_isAvailable)
                {
                    currIcon = _consIconPack.altIcon;
                    currMainBorder = _insufficientResIconPack.mainIcon;
                    currHoverBorder = _insufficientResIconPack.hoverIcon;
                    _isAvailable = false;
                    stateChanged = true;
                }
            }
            else
            {
                // Change from unavailable to available if the player has enough resource
                if (!_isAvailable)
                {
                    currIcon = _consIconPack.mainIcon;
                    currMainBorder = _availableIconPack.mainIcon;
                    currHoverBorder = _availableIconPack.hoverIcon;
                    _isAvailable = true;
                    stateChanged = true;
                }
            }

            // stateChanged prevents this from being run almost every frame when the player's resources number is updated
            if (stateChanged)
            {
                gameObject.GetComponent<Image>().sprite = currMainBorder;
                SpriteState sState = gameObject.GetComponent<Button>().spriteState;
                sState.highlightedSprite = currHoverBorder;
                gameObject.GetComponent<Button>().spriteState = sState;
                gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = currIcon;
            }
        }
    }

    /// <summary>
    /// Change the buttons to locked or unlocked.
    /// </summary>
    /// <param name="state"></param>
    public void ChangeLockState(bool state)
    {
        if (_isUnlocked != state)
        {
            _isUnlocked = state;

            if (!state)
            {
                gameObject.GetComponent<Button>().interactable = false;
                SpriteState sState = gameObject.GetComponent<Button>().spriteState;
                Sprite currIcon = _consIconPack.lockedIcon;
                gameObject.GetComponent<Button>().spriteState = sState;
                gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = currIcon;
            }
            else
            {
                gameObject.GetComponent<Button>().interactable = true;
                UpdateState(_worldScript.GameplayScript.PlayerResources);
            }
        }
    }

    private void SetConstructionIconPack()
    {
        _consIconPack = _archetype.GetComponent<TrainGameObjScript>().GetIconPack();
    }
}


