using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesMenuScript : MonoBehaviour
{
    private WorldScript _worldScript;    
    private GameObject _HUDResourcesMenuObj = null;
    private GameObject _HUDResourcesText = null;    
    private GameObject _HUDResourcesBarBG = null;    
    private GameObject _HUDResourcesBar = null;
    private GameObject _HUDResourcesOverBar = null;
    private float _HUDResourcesBarBG_time = 0.0f;
    private float _ResourceBarMax = 100.0f;
    private bool _BarIncrement;
    private float _startPosition;
    [SerializeField]
    private float _IncrementSpeed = 1.0f;
    
    void Start ()
    {
        _worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
        _HUDResourcesMenuObj = gameObject;
        if (_HUDResourcesMenuObj != null)
        {
            _HUDResourcesText = _HUDResourcesMenuObj.transform.Find("ResourcesText").gameObject;
            _HUDResourcesBarBG = _HUDResourcesMenuObj.transform.Find("ResourcesBarBG").gameObject;
            if (_HUDResourcesBarBG != null)
            {
                _HUDResourcesBar = _HUDResourcesBarBG.transform.Find("ResourcesBar").gameObject;
                _HUDResourcesOverBar = _HUDResourcesBarBG.transform.Find("ResourcesOverBar").gameObject;
            }
        }
        _HUDResourcesBar.GetComponent<Image>().fillAmount = 0;
        _HUDResourcesOverBar.GetComponent<Image>().fillAmount = 0;
        _BarIncrement = true;
    }
		
	void Update ()
    {
        if(_BarIncrement) ManageHUDResourceBar();
        LowResourceAlert();
    }

    /// <summary>
    /// Manage the transition of the resource progress bar using MoveTowards().
    /// </summary>
    private void ManageHUDResourceBar()
    {
        if (_HUDResourcesText == null) return;
        _HUDResourcesText.GetComponent<Text>().text = _worldScript.GameplayScript.PlayerResources.ToString();               

        float barResourceCount = 1.0f;
        float overBarResourceCount = 0f;
        
        if (_worldScript.GameplayScript.PlayerResources <= _ResourceBarMax && _HUDResourcesOverBar.GetComponent<Image>().fillAmount > 0) // Empty the OverlayBar before decreasing the UnderlayBar
        {            
            overBarResourceCount = 0f;
            _HUDResourcesOverBar.GetComponent<Image>().fillAmount = Mathf.MoveTowards(_HUDResourcesOverBar.GetComponent<Image>().fillAmount, overBarResourceCount / _ResourceBarMax, Time.deltaTime * _IncrementSpeed);
        }
        else if (_worldScript.GameplayScript.PlayerResources <= _ResourceBarMax && _HUDResourcesOverBar.GetComponent<Image>().fillAmount == 0) // UnderlayBar Transition
        {
            barResourceCount = _worldScript.GameplayScript.PlayerResources;            
            _HUDResourcesBar.GetComponent<Image>().fillAmount = Mathf.MoveTowards(_HUDResourcesBar.GetComponent<Image>().fillAmount, barResourceCount / _ResourceBarMax, Time.deltaTime * _IncrementSpeed);
            if (_HUDResourcesBar.GetComponent<Image>().fillAmount == _worldScript.GameplayScript.PlayerResources / _ResourceBarMax) { _BarIncrement = false; }
        }        
        else if (_worldScript.GameplayScript.PlayerResources > _ResourceBarMax && _HUDResourcesBar.GetComponent<Image>().fillAmount < 1.0) // Fill the UnderlayBar before increasing the OverlayBar
        {
            barResourceCount = _worldScript.GameplayScript.PlayerResources;            
            _HUDResourcesBar.GetComponent<Image>().fillAmount = Mathf.MoveTowards(_HUDResourcesBar.GetComponent<Image>().fillAmount, barResourceCount / _ResourceBarMax, Time.deltaTime * _IncrementSpeed);            
        }
        else if (_worldScript.GameplayScript.PlayerResources > _ResourceBarMax && _HUDResourcesBar.GetComponent<Image>().fillAmount == 1.0) // OverlayBar Transition
        {            
            overBarResourceCount = _worldScript.GameplayScript.PlayerResources - _ResourceBarMax;
            _HUDResourcesOverBar.GetComponent<Image>().fillAmount = Mathf.MoveTowards(_HUDResourcesOverBar.GetComponent<Image>().fillAmount, overBarResourceCount / _ResourceBarMax, Time.deltaTime * _IncrementSpeed);
            if (_HUDResourcesOverBar.GetComponent<Image>().fillAmount == _worldScript.GameplayScript.PlayerResources / _ResourceBarMax) { _BarIncrement = false; }          
        }          
    }

    /// <summary>
    /// Resource bar outline flash red when the player resource is low
    /// </summary>
    private void LowResourceAlert()
    {
        if (_worldScript.GameplayScript.PlayerResources <= 15)
        {
            _HUDResourcesBarBG_time += Time.deltaTime;

            if (_HUDResourcesBarBG_time >= 0.33f)
            {
                if (_HUDResourcesBarBG.GetComponent<Image>().color == Color.red) _HUDResourcesBarBG.GetComponent<Image>().color = Color.white;
                else if (_HUDResourcesBarBG.GetComponent<Image>().color == Color.white) _HUDResourcesBarBG.GetComponent<Image>().color = Color.red;
                else _HUDResourcesBarBG.GetComponent<Image>().color = Color.white;

                _HUDResourcesBarBG_time = 0.0f;
            }
        }
        else
        {
            _HUDResourcesBarBG.GetComponent<Image>().color = Color.white;
        }
    }

    public bool BarIncrement
    {
        set { _BarIncrement = value; }
    }
}
