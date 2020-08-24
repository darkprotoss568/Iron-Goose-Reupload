using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DescBoxOnConsMenuScript : DescriptionBoxScript {

    private Image _wireframeImage;
    [SerializeField]
    private Sprite _blankImage;
    private GameObject _buildMessage;
	// Use this for initialization
	public override void Start ()
    {
        base.Start();
        
        Clear();
	}
	
	// Update is called once per frame
	protected override void Update () {
		
	}

    protected override void ProcessConsDescriptionText(TrainGameObjScript archetype)
    {
        string header = string.Empty;
        
        // Header 1st line (Build Object name & Hotkey)
        header = " [" + _highlightTextColorTag + _hotKeyTag + "</color>]" + archetype._name ;
        _headerText.text = header;
        _wireframeImage.sprite = archetype._consIcon;
        // Header 2nd line (Build Cost)
        _costText.text = "Cost: " + _highlightTextColorTag + archetype.BuildCost + "</color>";

        //General Description 

        _descriptionText.text = archetype.Description;
    }

    protected override void LoadManagedObjects()
    {
        _headerText = gameObject.transform.Find("HeaderText").GetComponent<Text>();
        _costText = gameObject.transform.Find("CostText").GetComponent<Text>();
        _descriptionText = gameObject.transform.Find("DescriptionText").GetComponent<Text>();
        _wireframeImage = gameObject.transform.Find("Image").GetComponent<Image>();
        _buildMessage = gameObject.transform.Find("BuildMessage").gameObject;
    }

    public override void LoadDescBoxContent(TrainGameObjScript archetype, GameObject followTarget, string hotKeyString, Vector2 staticOffset, DescriptionType descriptionType = DescriptionType.Construction)
    {
        // Set the hotkey tag used for the description box
        SetHotKeyTag(hotKeyString);
        _buildMessage.SetActive(false);

        switch (descriptionType)
        {
            case DescriptionType.Construction:
                ProcessConsDescriptionText(archetype);
                break;
            default:
                break;
        }
        if (archetype._consIcon != null)
            _wireframeImage.sprite = archetype._consIcon;
        _buildMessage.SetActive(false);
    }

    public override void Clear()
    {
        ClearWarning();
        _hotKeyTag = string.Empty;
        _headerText.text = string.Empty;
        _costText.text = string.Empty;
        _descriptionText.text = string.Empty;

        _wireframeImage.sprite = _blankImage;
        _buildMessage.SetActive(true);
    }
    
}
