using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManagerScript : MonoBehaviour
{
    public GameObject[] _turretArchetypes;
    public GameObject[] _droneArchetypes;
	private WorldScript _worldScript;
	private GameObject _currentConstructor; // Was _currentConstructionPlatform -- now could also be a cons drone [Mike, 4-6-18]
	private int _framesSinceLastOpened;
	private bool _bIsConsMenuOpen;
	private ConstructionMenuObjScript _consMenu;
    public ObjectCountPanelScript[] _droneCounters = new ObjectCountPanelScript[2];
    private int[] _buttonsToLock = new int[0];
    public GameObject _rangeProjectorPrefab;
    private ConeRangeProjectorScript _currRangeProjector;

	// Use this for initialization
	void Start()
	{
		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();

		_framesSinceLastOpened = 0;
		_bIsConsMenuOpen = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (PauseMenu.isPaused) return;

		++_framesSinceLastOpened;
	}

	public void OpenConstructionMenu(ConsPlatformScript cps)
	{
		if (!_bIsConsMenuOpen && _worldScript.ConstructionManager._framesSinceLastOpened > 2)
		{

			//if (!_HUDConsMenuObj.activeInHierarchy) _HUDConsMenuObj.SetActive(true);

			_currentConstructor = cps.gameObject;
			GameObject newConsMenu = Instantiate(cps._constructionMenu, Vector2.zero, Quaternion.identity, _worldScript.HUDScript.HUDCanvas.transform);
			SetConstructionActive(true);
			_consMenu = newConsMenu.GetComponent<ConstructionMenuObjScript>();
            _consMenu.InitializeConsMenu(_buttonsToLock);
            _currRangeProjector = cps.CreateRangeProjector();

        }
	}

	public void OpenConstructionMenu_BBB(AIConsDroneScript consDrone)
	{
		if (!_bIsConsMenuOpen && _worldScript.ConstructionManager._framesSinceLastOpened > 2)
		{

			//if (!_HUDConsMenuObj.activeInHierarchy) _HUDConsMenuObj.SetActive(true);

			_currentConstructor = consDrone.gameObject;
			GameObject newConsMenu = Instantiate(consDrone._constructionMenu, Vector2.zero, Quaternion.identity, _worldScript.HUDScript.HUDCanvas.transform);
			SetConstructionActive(true);
			_consMenu = newConsMenu.GetComponent<ConstructionMenuObjScript>();

            
			//PopulateConsMenu(cps);
		}
	}

    

    public void ActivateRangeProjector(float turretFiringRange, float maxAimAngle)
    {
        /*Projector projectorComponent = _currRangeProjector.GetComponent<Projector>();

        projectorComponent.orthographicSize = turretFiringRange;*/
        _currRangeProjector.gameObject.SetActive(true);

        _currRangeProjector.AdjustCone(turretFiringRange, maxAimAngle);
        
    }

    public void DeactivateRangeProjector(bool destroyProjector)
    {
        if (_currRangeProjector != null)
        {
            if (destroyProjector)
            {
                Destroy(_currRangeProjector.gameObject);
            }
            else
            {
                _currRangeProjector.gameObject.SetActive(false);
            }
        }
    }
    
	public int FramesSinceLastOpened
	{
		get
		{
			return _framesSinceLastOpened;
		}
	}

	public void SetConstructionActive(bool value)
	{
		_bIsConsMenuOpen = value;
        
			_worldScript.GameplayScript.bCanPlayerSelectObjs = !value;
			_worldScript.GameplayScript.bCanPlayerTargetObjs = !value;

		if (value == false)
		{
			if (_worldScript.CS.BuildObjArchetype == null && !_worldScript.CS.BInRecycleMode) // Otherwise we want to keep the selection as it should be a cons drone [Mike, 4-6-18]
			{
				_worldScript.GameplayScript.DeselectAll();
			}

			RemoveConsPlatformSelection();
		}
	}

	public void SetConsPlatformSelection(GameObject consPlatform)
	{
		_currentConstructor = consPlatform;
	}

	public void RemoveConsPlatformSelection()
	{
		_currentConstructor = null;
	}

	public GameObject CurrentSelectedConstructor
	{
		get
		{
			return _currentConstructor;
		}
	}

	public bool bIsConsMenuOpen
	{
		get
		{
			return _bIsConsMenuOpen;
		}
	}

    /// <summary>
    /// Return the currently opened construction menu
    /// </summary>
    /// <returns></returns>
	public ConstructionMenuObjScript GetOpenedConstructionMenu()
	{
		return _consMenu;
	}

    public void LockConstructionButtons(int[] buttonsToLock)
    {
        if (_consMenu != null)
        {
            _consMenu.LockButtons(buttonsToLock);
        }
        _buttonsToLock = buttonsToLock;
        
    }
    public void UnlockConstructionButtons()
    {
        if (_consMenu != null)
        {
            _consMenu.UnlockAllButtons();
        }

        if (_buttonsToLock.Length > 0)
        {
            _buttonsToLock = new int[0];
        }
    }

    public void UpdateConsButtonState()
    {
        if (_consMenu != null)
        {
            _consMenu.UpdateAllButtonsStates();
        }
    } 
}
