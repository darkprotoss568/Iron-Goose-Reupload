using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } }
	public WorldScript WS { get { return _worldScript; } }

	private GameObject _HUDObjOverlay_archetype;
	public bool _bIsConsMenuOpen;

	private GameObject _HUDCanvasObj = null;
	private GameObject _HUDConsMenuObj = null;
	private GameObject _HUDConsMenuObj_BGImage = null;
	private GameObject _HUDCompletionBarMenuObj = null;
	//private GameObject _HUDShopMenuObj = null;

	//private GameObject _HUDDronePanelObj = null;
	private GameObject _HUDMapCompletionBar_train = null;
	private Image _HUDMapCompletionBar_trainImage = null;

	private GameObject _HUDMapCompletionBar = null;
	private GameObject _HUDMapCompletionBarBG = null;

	private Vector2 _HUDHealthBar_OrigSize;
	//private Vector2 _HUDHealthBarBG_OrigSize;
	private Vector2 _HUDMapCompletionBar_OrigSize;
	private Vector2 _HUDMapCompletionBarBG_OrigSize;

	private Vector2 _HUDMapCompletionBar_train_OrigPos;
    private Vector2 _HUDMapCompletionBar_train_EndPos;

    private List<GameObject> _HUDObjOverlayIcons = new List<GameObject>();

	private Sprite _mouseHoverOverObjTex1;
	private Sprite _mouseHoverOverObjTex2;
	private Sprite _mouseHoverOverObjTex3;
	private Sprite _selectedObjSprite;

	private Sprite _defaultIcon; public Sprite DefaultIcon { get { return _defaultIcon; } }

	private Sprite _recyIcon; public Sprite RecyIcon { get { return _recyIcon; } }

	private List<GameObject> _HUDPointerIcons = new List<GameObject>();

	//private bool _bIsShopMenuOpen = false;

	//

	private GameObject _HUDLine1_archetype = null;
	private GameObject _HUDLine1_curr = null;
	private RectTransform _HUDLine1_rt = null;
	private Image _HUDLine1_img = null;

	//

	private GameObject _HUDPoint1_archetype = null;

	private GameObject _HUDPointA_curr = null;
	private RectTransform _HUDPointA_rt = null;
	private Image _HUDPointA_img = null;

	private GameObject _HUDPointB_curr = null;
	private RectTransform _HUDPointB_rt = null;
	private Image _HUDPointB_img = null;

	private List<string> _guiTextStrs = new List<string>();
	private List<Vector3> _guiTextLocs = new List<Vector3>();
	private List<Color> _guiTextColours = new List<Color>();

	private bool _bPending_Debug_ClearGUIText_WM = false;

	private Texture2D _cursor_main;
	private Texture2D _cursor_select;
	private Texture2D _cursor_target;
	//private Texture2D _cursor_cog;
	private Texture2D _cursor_goto; public Texture2D Cursor_goto { get { return _cursor_goto; } }
	private Texture2D _cursor_follow; public Texture2D Cursor_follow { get { return _cursor_follow; } }
	private Texture2D _cursor_attack; public Texture2D Cursor_attack { get { return _cursor_attack; } }
	private Texture2D _cursor_build; public Texture2D Cursor_build { get { return _cursor_build; } }
	private Texture2D _cursor_impossible; public Texture2D Cursor_impossible { get { return _cursor_impossible; } }
	private Texture2D _cursor_recy; public Texture2D Cursor_recy { get { return _cursor_recy; } }

	private int _framesSinceConsMenuWasOpen = 0;

	private List<GameObject> _constructionMenuImages = new List<GameObject>();

	List<TrainGameObjScript> _tgosOnScreen = new List<TrainGameObjScript>();

	private GameObject _mouseOverObj;
	private GameObject _mouseOverObj_alwaysOn;
	private GameObject _xtSelObj;

	private Vector3 _mouseWorldPos; public Vector3 MouseWorldPos { get { return _mouseWorldPos; } }
	private RaycastHit _mouseWorldHit; public RaycastHit MouseWorldHit { get { return _mouseWorldHit; } }

	private Vector2 _xtSelObj_2DPos;


	[SerializeField]
	private GameObject _descriptionBox;
    [SerializeField]
    private ConstructionBarsManagementScript _constructionBarsForAesthetics;

	private GameObject _HUDAreaSelectionBox = null;
	private Vector2 _HUDAreaSelectionBox_topLeft = Vector2.zero;
	private Vector2 _HUDAreaSelectionBox_btmRight = Vector2.zero;
	public Vector2 HUDAreaSelectionBox_topLeft { get { return _HUDAreaSelectionBox_topLeft; } set { _HUDAreaSelectionBox_topLeft = value; } }
	public Vector2 HUDAreaSelectionBox_btmRight { get { return _HUDAreaSelectionBox_btmRight; } set { _HUDAreaSelectionBox_btmRight = value; } }

	private GameObject _turretAngleIndicator_obj = null;
	private GameObject _turretAngleIndicator_obj_archetype = null;

	private GameObject _turretAngleIndicator_currTurr = null;

	private GameObject _HUDDescriptionBox = null;
	private GameObject _HUDDescriptionBox_text = null;
	private Text _HUDDescriptionBox_text_t = null;

	private bool _bPendingCloseConsMenu = false;
	public bool BPendingCloseConsMenu { get { return _bPendingCloseConsMenu; } set { _bPendingCloseConsMenu = value; } }

	private GameObject _HUDImagePF;
	private GameObject _HUDPointerIcon;
    
	void Start()
	{
		_bIsConsMenuOpen = false;
		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
		//_HUDCanvasObj = GameObject.Find("HUDCanvas"); //? GameObject.Find cannot find inactive objects!!
		if (GameObject.Find("MainHolder") != null)
		{
			_HUDCanvasObj = GameObject.Find("MainHolder").transform.Find("HUDCanvas").gameObject; // This can, but it must have a parent
			if (_HUDCanvasObj != null)
			{
				if (!_HUDCanvasObj.activeInHierarchy) _HUDCanvasObj.SetActive(true);

				//FadeParent = HUDCanvasObj.transform.Find("FadeImage").gameObject;
				//FadeImage = GameObject.Find("FadeImage").GetComponent<Image>();
			}

			//

			_HUDDescriptionBox = _HUDCanvasObj.transform.Find("HUDDescriptionBox").gameObject;
			if (_HUDDescriptionBox != null)
			{
				_HUDDescriptionBox_text = _HUDDescriptionBox.transform.Find("Text").gameObject;
				if (_HUDDescriptionBox_text != null)
				{
					_HUDDescriptionBox_text_t = _HUDDescriptionBox_text.GetComponent<Text>();
				}
			}

			//

			_HUDConsMenuObj = _HUDCanvasObj.transform.Find("ConsMenu").gameObject;
			if (_HUDConsMenuObj != null)
			{
				//if (!_HUDConsMenuObj.activeInHierarchy) _HUDConsMenuObj.SetActive(true);

				_HUDConsMenuObj_BGImage = _HUDConsMenuObj.transform.Find("Image").gameObject;
			}

			//

			_HUDCompletionBarMenuObj = _HUDCanvasObj.transform.Find("CompletionBarMenuObj").gameObject;
			if (_HUDCompletionBarMenuObj != null)
			{
				_HUDMapCompletionBarBG = _HUDCompletionBarMenuObj.transform.Find("MapCompletionBarBG").gameObject;
				if (_HUDMapCompletionBarBG != null)
				{
					_HUDMapCompletionBar = _HUDMapCompletionBarBG.transform.Find("MapCompletionBar").gameObject;
					if (_HUDMapCompletionBar != null)
					{
						RectTransform rt = _HUDMapCompletionBar.GetComponent<RectTransform>();
						RectTransform rtb = _HUDMapCompletionBarBG.GetComponent<RectTransform>();
						_HUDMapCompletionBar_OrigSize = rt.sizeDelta;
                        _HUDMapCompletionBarBG_OrigSize = rtb.sizeDelta;
					}
				}

				_HUDMapCompletionBar_train = _HUDCompletionBarMenuObj.transform.Find("ProgressMarker").gameObject;
                _HUDMapCompletionBar_trainImage = _HUDMapCompletionBar_train.GetComponent<Image>();
				if (_HUDMapCompletionBar_train != null)
				{
					_HUDMapCompletionBar_train_OrigPos = _HUDMapCompletionBar_train.GetComponent<RectTransform>().anchoredPosition;
                    _HUDMapCompletionBar_train_EndPos = _HUDMapCompletionBar_train_OrigPos;
                    _HUDMapCompletionBar_train_EndPos.x += _HUDMapCompletionBarBG_OrigSize.x;
                    _HUDMapCompletionBar_train_EndPos.x -= (_HUDMapCompletionBar_trainImage.rectTransform.sizeDelta.x)/2;
                }
            }

			//
			//_HUDShopMenuObj = _HUDCanvasObj.transform.Find("ShopMenu").gameObject;
			//if (_HUDShopMenuObj != null)
			//{

			//}

			//_HUDDronePanelObj = _HUDCanvasObj.transform.Find("DronePanel").gameObject;

			// Get the default hud health bar size values
			_HUDObjOverlay_archetype = Resources.Load("HUDObjOverlay") as GameObject;
			if (_HUDObjOverlay_archetype != null)
			{
				GameObject HUDHealthBar = _HUDObjOverlay_archetype.transform.Find("HealthBar").gameObject;
				//GameObject HUDHealthBarBG = _HUDObjOverlay_archetype.transform.Find("HealthBarBG").gameObject;
				//GameObject HUDResBar = _HUDObjOverlay_archetype.transform.Find("ResourceBar").gameObject;

				RectTransform rth = HUDHealthBar.GetComponent<RectTransform>();
				//RectTransform rthBG = HUDHealthBarBG.GetComponent<RectTransform>();
				//RectTransform rtr = HUDResBar.GetComponent<RectTransform>();

				_HUDHealthBar_OrigSize = rth.sizeDelta;
				//_HUDHealthBarBG_OrigSize = rthBG.sizeDelta;
				//print("_HUDHealthBar_OrigSize: " + _HUDHealthBar_OrigSize);
				//_HUDResourceBar_OrigSize = rtr.sizeDelta;
			}

			_HUDLine1_archetype = Resources.Load("HUDLine1") as GameObject;

			_HUDPoint1_archetype = Resources.Load("HUDPoint1") as GameObject;

			//
            
		}

		//

		_mouseHoverOverObjTex1 = Resources.Load<Sprite>("mouseHoverOver001");
		if (_mouseHoverOverObjTex1 == null) print("ERROR: _mouseHoverOverObjTex1 == null");

		_mouseHoverOverObjTex2 = Resources.Load<Sprite>("mouseHoverOver002");
		if (_mouseHoverOverObjTex2 == null) print("ERROR: _mouseHoverOverObjTex2 == null");

		_mouseHoverOverObjTex3 = Resources.Load<Sprite>("mouseHoverOver003");
		if (_mouseHoverOverObjTex3 == null) print("ERROR: _mouseHoverOverObjTex3 == null");

		_selectedObjSprite = Resources.Load<Sprite>("selectedObjSpriteB"); // selectedObjSprite
		if (_selectedObjSprite == null) print("ERROR: _selectedObjSprite == null");

		_defaultIcon = Resources.Load<Sprite>("defaultIcon");
		if (_defaultIcon == null) print("ERROR: _defaultIcon == null");

		_recyIcon = Resources.Load<Sprite>("recyIcon");
		if (_recyIcon == null) print("ERROR: _recyIcon == null");


		// Cursors

		//_cursor_cog = Resources.Load("Textures/cog1") as Texture2D;
		//Cursor.SetCursor(_cursor_main, Vector2.zero, CursorMode.Auto);
		_cursor_main = Resources.Load("Textures/cursor1") as Texture2D;
		_cursor_select = Resources.Load("Textures/cursor2") as Texture2D;
		_cursor_target = Resources.Load("Textures/cursor3") as Texture2D;

		_cursor_goto = Resources.Load("Textures/cursor_goto1") as Texture2D;
		_cursor_follow = Resources.Load("Textures/cursor_follow1") as Texture2D;
		_cursor_attack = Resources.Load("Textures/cursor_attack1") as Texture2D;
		_cursor_build = Resources.Load("Textures/cursor_build1") as Texture2D;

		_cursor_impossible = Resources.Load("Textures/cursor_impossible1") as Texture2D;

		_cursor_recy = Resources.Load("Textures/cursor_recycle1") as Texture2D;

		//

		_mouseOverObj = null;
		_xtSelObj = null;

		_xtSelObj_2DPos = Vector2.zero;


		if (_descriptionBox != null) _descriptionBox = Instantiate(_descriptionBox, Vector3.zero, Quaternion.identity, _HUDCanvasObj.transform);

		_turretAngleIndicator_obj_archetype = Resources.Load("turretAngleIndicator1") as GameObject;
		if (_turretAngleIndicator_obj_archetype == null) print("Error: turretAngleIndicator1 prefab can't be found");

		_HUDImagePF = Resources.Load("HUDImagePF") as GameObject;
		_HUDPointerIcon = Resources.Load("HUDPointerIcon") as GameObject;

		// End of Start()
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		_tgosOnScreen = WorldScript.GetObjsOnScreen(WorldScript.GetAllTGOsInWorld(), 50.0f, false);
		//_tgosOnScreen = WorldScript.GetAllTGOsInWorld(); // Test [Mike, 2-6-18]

		//print("_tgosOnScreen count: " + _tgosOnScreen.Count);

		++_framesSinceConsMenuWasOpen;

		GetMouseWorldPos();
		ManageMouseOver();
        
		ManageHUD();

		ManagePendings();

		//if (_bIsShopMenuOpen)
		//	ManageShopmenu();

		// End of Update()
	}

	private void FixedUpdate()
	{
		CheckForConsMenuClose(); /// Also checking here -- trying to always capture the mouse clicks -- 10-8-18
	}

	void ManagePendings()
	{
		if (_bPendingCloseConsMenu)
		{
			_bPendingCloseConsMenu = false;
		}

		if (_bPending_Debug_ClearGUIText_WM)
		{
			Debug_ClearGUIText_WM();
			_bPending_Debug_ClearGUIText_WM = false;
		}
	}

	public bool bIsConsMenuOpen
	{
		get { return _bIsConsMenuOpen; }
		set { _bIsConsMenuOpen = value; }
	}

	private void CheckForConsMenuClose()
	{
		if (_bIsConsMenuOpen && (Input.GetMouseButton(1) || Input.GetMouseButtonDown(1)))
		{
			_bPendingCloseConsMenu = true;
		}
	}

	private void RemoveConsMenuImages()
	{
		for (int i = 0; i < _constructionMenuImages.Count; ++i)
		{
			Destroy(_constructionMenuImages[i]);
		}
	}

	private void ManageHUD()
	{

		//

		DrawDirectionPointerIcons();
		DrawExtendedSelLine();
        

		//DrawTurretAngle3DHUDObj();

		ManageHUDDescriptionBox();

		ManageHUDMapCompletionBar();
	}

	private void ManageHUDDescriptionBox()
	{
		bool bSetEmptyText = true;
		bool bContinue = true;

		if (_worldScript.HUDScript._bIsConsMenuOpen)
		{
			_HUDDescriptionBox_text_t.text = "Right-click to close the construction menu";
			bContinue = false;
			bSetEmptyText = false;
		}

		if (bContinue && _xtSelObj != null)
		{
			TrainGameObjScript tgoXT = _xtSelObj.GetComponent<TrainGameObjScript>();
			if (tgoXT != null && tgoXT.Description != "")
			{
				_HUDDescriptionBox_text_t.text = tgoXT.Description;
				bSetEmptyText = false;
			}
			bContinue = false;
		}

		if (bContinue && _mouseOverObj_alwaysOn != null)
		{
			TrainGameObjScript tgo = BBBStatics.TGO(_mouseOverObj_alwaysOn);
			if (tgo != null)
			{
				_HUDDescriptionBox_text_t.text = tgo.Description;
				bSetEmptyText = false;
			}
		}

		if (bSetEmptyText && _HUDDescriptionBox_text_t.text != "")
		{
			_HUDDescriptionBox_text_t.text = "";
		}
	}
    
	private void ManageHUDMapCompletionBar()
	{
		if (WS.LocomotiveObjectRef == null) return;

		Vector3 endPos = WS.WinConditionRail.transform.position;
		float distToCover = Vector3.Distance(WS.LocomotiveInitialPos, endPos);
		float distCovered = Vector3.Distance(WS.LocomotiveObjectRef.transform.position, endPos);

		float pcnt = Mathf.Clamp01(distCovered / distToCover);
        pcnt = 1-pcnt;
		
		_worldScript.MapCompletionPcnt = pcnt;

        // inner bar
		RectTransform rt = _HUDMapCompletionBar.GetComponent<RectTransform>();

		int szX = Mathf.RoundToInt(_HUDMapCompletionBar_OrigSize.x * pcnt);
		if (szX % 2 != 0) szX += 1; szX = Mathf.Clamp(szX, 0, Mathf.RoundToInt(_HUDMapCompletionBar_OrigSize.x)); // Round to nearest power of 2

		rt.sizeDelta = new Vector2(szX, Mathf.RoundToInt(_HUDMapCompletionBar_OrigSize.y));

		rt.anchoredPosition = new Vector2(Mathf.RoundToInt((_HUDMapCompletionBar_OrigSize.x - szX) / 2) * -1, rt.anchoredPosition.y);

		//_HUDMapCompletionBar_train;

		RectTransform rt_img = _HUDMapCompletionBar_train.GetComponent<RectTransform>();

        float p = Mathf.Lerp(_HUDMapCompletionBar_train_OrigPos.x, _HUDMapCompletionBar_train_EndPos.x, pcnt);
        
        rt_img.anchoredPosition = new Vector2(p, _HUDMapCompletionBar_train_OrigPos.y);
	}
    
	private void DrawTurretAngle3DHUDObj()
	{

		if (_xtSelObj != null)
		{
			if (_turretAngleIndicator_currTurr != null && _xtSelObj != _turretAngleIndicator_currTurr) // _xtSelObj is a different object
			{
				_turretAngleIndicator_currTurr = null;
			}

			TurretScriptParent tsp = _xtSelObj.GetComponent<TurretScriptParent>();
			if (tsp != null && tsp.CommSocketObj != null)
			{
				if (_turretAngleIndicator_obj == null)
				{
					_turretAngleIndicator_obj = Instantiate(_turretAngleIndicator_obj_archetype, tsp.CommSocketObj.transform.position, tsp.transform.rotation, tsp.transform) as GameObject;
					_turretAngleIndicator_currTurr = tsp.gameObject;
				}
			}
		}
		else
		{
			_turretAngleIndicator_currTurr = null;
		}


		if (_turretAngleIndicator_currTurr == null && _turretAngleIndicator_obj != null)
		{
			Destroy(_turretAngleIndicator_obj);
		}
	}

	private void DrawHUDObjIcons()
	{
		//List<TrainGameObjScript> allObjs = new List<TrainGameObjScript>((TrainGameObjScript[])FindObjectsOfType(typeof(TrainGameObjScript)));

		//_tgosOnScreen.Clear();
		//_tgosOnScreen = WorldScript.GetObjsOnScreen(WorldScript.GetAllTGOsInWorld(), 50.0f, false); // allObjs

		////? Hacky and probably costly fix - I honestly can't figure out why the hell the HUD overlay icon issue (not appearing until another one has been on-screen) is occuring [Mike, 2-6-18]
		////? For now, just destroy all of them every update so they can be reinstantiated every update -- this, for some reason, fixes the issue, although I really don't like it
		//while (_HUDObjOverlayIcons.Count > 0)
		//{
		//	GameObject go = _HUDObjOverlayIcons[0];
		//	_HUDObjOverlayIcons.RemoveAt(0);
		//	Destroy(go);
		//}

		//

		List<TrainGameObjScript> TGOsOnScreen_Keepers = new List<TrainGameObjScript>();

        int tgosOnScreenCount = _tgosOnScreen.Count;
		for (int i = 0; i < tgosOnScreenCount; ++i) // _tgosOnScreen
		{
			if (_tgosOnScreen[i]._team == Team.Neutral) { continue; } // Don't display stats overlay for neutral objects
			if (_tgosOnScreen[i].GetComponent<ChunkScript>() != null) { continue; } // Don't display stats for chunks

			TGOsOnScreen_Keepers.Add(_tgosOnScreen[i]);
		}
		//_tgosOnScreen = TGOsOnScreen_Keepers; // Copy back

		//

		while (_HUDObjOverlayIcons.Count < TGOsOnScreen_Keepers.Count)
		{
			GameObject go = Instantiate(_HUDObjOverlay_archetype, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			go.transform.SetParent(_HUDCanvasObj.transform);

			if (!go.activeInHierarchy) go.SetActive(true); /// Not sure if this has any effect

			_HUDObjOverlayIcons.Add(go);
		}

		// TODO: Figure out why this causes the HUD overlay icon issue (not appearing until another one has been on-screen) - [Mike, 2-6-18]
		while (_HUDObjOverlayIcons.Count > TGOsOnScreen_Keepers.Count)
		{
			GameObject go = _HUDObjOverlayIcons[0];
			_HUDObjOverlayIcons.RemoveAt(0);
			//go.SetActive(false);
			Destroy(go);
		}

		//print("_HUDObjOverlayIcons.Count: " + _HUDObjOverlayIcons.Count);
		//print("TGOsOnScreen_Keepers.Count: " + TGOsOnScreen_Keepers.Count);

		//

		///

		///
		if (_HUDObjOverlayIcons.Count != TGOsOnScreen_Keepers.Count)
		{
			print("Mismatch detected -- WorldScript -- DrawHUDObjIcons()");
		}
        ///

        //print("_HUDObjOverlayIcons.Count: " + _HUDObjOverlayIcons.Count);

        /////
        //int activeCount = 0;
        //for (int i = 0; i < _HUDObjOverlayIcons.Count; ++i)
        //{
        //	if (_HUDObjOverlayIcons[i].activeInHierarchy) ++activeCount;
        //}
        //print("activeCount: " + activeCount);
        /////

        /////

        //

        int hudObjOverlayIconsCount = _HUDObjOverlayIcons.Count;
        for (int i = 0; i < hudObjOverlayIconsCount; ++i) // TGOsOnScreen_Keepers
		{
			//if (!_HUDObjOverlayIcons[i].activeInHierarchy) _HUDObjOverlayIcons[i].SetActive(true);

			Image iconImg = _HUDObjOverlayIcons[i].GetComponent<Image>();
			iconImg.color = new Color(1, 1, 1, 0.0f); // Invisible
			Cursor.SetCursor(_cursor_main, Vector2.zero, CursorMode.Auto); // Default

			iconImg.sprite = _mouseHoverOverObjTex2; // _mouseHoverOverObjTex1 // _mouseHoverOverObjTex2

			RectTransform recthooi = _HUDObjOverlayIcons[i].GetComponent<RectTransform>();
			TrainGameObjScript tgo = TGOsOnScreen_Keepers[i];

			//if (_worldScript.GameplayScript.PlayerSelectedObjects.Contains(TGOsOnScreen_Keepers[i].gameObject))
			//{
			//	iconImg.sprite = _mouseHoverOverObjTex1;
			//}

			//if (_xtSelObj != null && tgo.gameObject == _xtSelObj)
			//{
			//	iconImg.color = new Color(1, 1, 1, 1);
			//}

			//int healthPcnt = Mathf.RoundToInt(tgo._currentHealth / tgo._maxHealth);
			float healthPcnt = 1.0f;

			// Health
			float ch = tgo._currentHealth; // Convert to float
			float mh = tgo._maxHealth;
			if (mh > 0.0f) healthPcnt = ch / mh;

			//

			GameObject HUDHealthBar = _HUDObjOverlayIcons[i].transform.Find("HealthBar").gameObject;
			GameObject HUDHealthBarBG = _HUDObjOverlayIcons[i].transform.Find("HealthBarBG").gameObject;

			if (HUDHealthBar != null && HUDHealthBarBG != null) // && HUDArmourBar != null && HUDArmourBarBG != null) // && HUDResourceBar != null && HUDResourceBarBG != null)
			{
				//if (!HUDHealthBar.activeInHierarchy) { HUDHealthBar.SetActive(true); }
				//if (!HUDHealthBarBG.activeInHierarchy) { HUDHealthBarBG.SetActive(true); }

				Image img = HUDHealthBar.GetComponent<Image>();
				Image imgBG = HUDHealthBarBG.GetComponent<Image>();

				RectTransform rth = HUDHealthBar.GetComponent<RectTransform>();

				int szX = Mathf.RoundToInt(_HUDHealthBar_OrigSize.x * healthPcnt);
				rth.sizeDelta = new Vector2(szX, Mathf.RoundToInt(_HUDHealthBar_OrigSize.y));

				imgBG.color = new Color(0, 0, 0, 0.5f);

				//img_Ar.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
				//imgBG_Ar.color = new Color(0, 0, 0, 0.5f);

				// Health bar colour
				if (healthPcnt > 0.66f) img.color = new Color(0, 1, 0, 0.5f);
				else if (healthPcnt > 0.33f) img.color = new Color(1, 1, 0, 0.5f);
				else img.color = new Color(1, 0, 0, 0.5f);

				imgBG.color = new Color(0, 0, 0, 1.0f);

				//rth.sizeDelta = new Vector2(szX, _HUDHealthBar_OrigSize.y);
				//rthBG.sizeDelta = new Vector2(_HUDHealthBarBG_OrigSize.x, Mathf.RoundToInt(_HUDHealthBarBG_OrigSize.y));

				if (_xtSelObj != null && tgo.gameObject == _xtSelObj)
				{
					if (tgo._team == Team.Friendly)
					{
						iconImg.color = new Color(0, 1, 0, 0.5f);
						Cursor.SetCursor(_cursor_select, Vector2.zero, CursorMode.Auto);
					}
					else if (tgo._team == Team.Neutral)
					{
						iconImg.color = new Color(1, 1, 1, 0.5f);
					}
					else if (tgo._team == Team.Enemy)
					{
						iconImg.sprite = _mouseHoverOverObjTex3;
						Cursor.SetCursor(_cursor_target, Vector2.zero, CursorMode.Auto);
						//print("3: " + Time.time);

						float animTime = 0.25f;

						if (WS.CommandScript.TimeSinceGaveTargetingOrder > animTime)
						{
							iconImg.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
						}
						else
						{
							float map = BBBStatics.Map(WS.CommandScript.TimeSinceGaveTargetingOrder, 0.0f, animTime, 1.0f, 0.0f, true);
							iconImg.color = Color.Lerp(new Color(1.0f, 0.0f, 0.0f, 0.5f), new Color(1.0f, 1.0f, 0.0f, 0.75f), map);
							//iconImg.color = new Color(1.0f, 0.5f, 0.0f, 0.5f);
						}
					}
				}

				//

				if (tgo.CommSocketObj != null)
				{
					Vector3 screenPos = Camera.main.WorldToScreenPoint(tgo.CommSocketObj.transform.position);
					recthooi.anchoredPosition = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));
				}
				else
				{
					Vector3 screenPos = Camera.main.WorldToScreenPoint(tgo.transform.position);
					recthooi.anchoredPosition = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));
				}
			}
		}
	}

	/// <summary>
	/// Draw directional indicator icons at the edges of the screen when the train is off-screen
	/// </summary>
	void DrawDirectionPointerIcons()
	{
		if (WorldScript.LocomotiveObjectRef == null)
		{
			if (_HUDPointerIcons.Count > 0) _HUDPointerIcons.Clear();
			return;
		}

		//

		List<TrainGameObjScript> objsToPointAt = new List<TrainGameObjScript>();
        //List<Color> objsToPointAt_colours = new List<Color>();
        //objsToPointAt.Add(WorldScript.LocomotiveObjectRef.GetComponent<TrainGameObjScript>());

        //List<GameObject> carriages = WorldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetCarriagesInOrderAsGameObjects();

        int carriageCount = _worldScript.AllCarriages.Count;
        for (int i = 0; i < carriageCount; ++i)
		{
			if (_worldScript.AllCarriages[i] == null) continue; // If it is null, that would already be an issue as GetCarriagesInOrderAsGameObjects() would be returning nulls [Mike, 28-7-18]

			objsToPointAt.Add(_worldScript.AllCarriages[i].GetComponent<TrainGameObjScript>());
			//objsToPointAt_colours.Add(Color.green);
		}

        int enemyCount = _worldScript.Enemies.Count;
		for (int i = 0; i < enemyCount; ++i)
		{
			if (_worldScript.Enemies[i] == null) continue;

			//if (!_worldScript.Enemies[i]._bCanBeTargetedByPlayer) continue;

            if (!_worldScript.Enemies[i]._bShowPositionIndicator) continue;

			objsToPointAt.Add(_worldScript.Enemies[i]);
			//objsToPointAt_colours.Add(Color.red);
		}

		//

		float iconSz = 25.0f; // Default
		if (_HUDPointerIcons.Count > 0) iconSz = _HUDPointerIcons[0].GetComponent<RectTransform>().sizeDelta.x / 2;

		//

		objsToPointAt = WorldScript.GetObjsOnScreen(objsToPointAt, iconSz, true);

		//

		while (_HUDPointerIcons.Count < objsToPointAt.Count)
		{
			GameObject go = Instantiate(_HUDPointerIcon, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			go.transform.SetParent(_HUDCanvasObj.transform);

			_HUDPointerIcons.Add(go);
		}

		while (_HUDPointerIcons.Count > objsToPointAt.Count)
		{
			GameObject go = _HUDPointerIcons[0];
			_HUDPointerIcons.RemoveAt(0);
			Destroy(go);
		}

		//

		Vector2 screenCentre = new Vector2(0, 0);

        int objsCount = objsToPointAt.Count;
		for (int i = 0; i < objsCount; ++i)
		{
			//if (!_HUDPointerIcons[i].activeInHierarchy) _HUDPointerIcons[i].SetActive(true);

			Image iconImg = _HUDPointerIcons[i].GetComponent<Image>();
			//iconImg.color = new Color(1, 1, 1, 1.0f);
			//iconImg.color = objsToPointAt_colours[i];

			if (objsToPointAt[i]._team == Team.Friendly) iconImg.color = Color.green;
			if (objsToPointAt[i]._team == Team.Enemy) iconImg.color = Color.red;

			RectTransform rt = _HUDPointerIcons[i].GetComponent<RectTransform>();

			//

			Vector3 worldPos = objsToPointAt[i].transform.position;
			if (objsToPointAt[i].CommSocketObj != null) worldPos = objsToPointAt[i].CommSocketObj.transform.position;

			Vector3 screenPos = BBBStatics.WorldToScreenPointProjected(Camera.main, worldPos);
			Vector3 screenPosClamped = BBBStatics.ScreenPointEdgeClamp(screenPos, iconSz);

			Vector2 ap = new Vector2(screenPosClamped.x - (Screen.width / 2), screenPosClamped.y - (Screen.height / 2));
			Vector2 ap_unclamped = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));

			rt.anchoredPosition = ap;

			float angle = (Mathf.Atan2((ap_unclamped.y - screenCentre.y), (ap_unclamped.x - screenCentre.x)) * Mathf.Rad2Deg) - 90.0f;

			rt.rotation = Quaternion.Euler(new Vector3(rt.rotation.eulerAngles.x, rt.rotation.eulerAngles.y, angle));
		}
	}

	/// <summary>
	/// Draw a line between the cursor and the current extended selection object [15-4-18]
	/// </summary>
	void DrawExtendedSelLine()
	{
		if (_xtSelObj != null)
		{
			TrainGameObjScript tgo_xtSelObj = BBBStatics.TGO(_xtSelObj);

			if (_HUDLine1_curr == null)
			{
				// Create the line and the 2 end points + get relevant components

				_HUDLine1_curr = Instantiate(_HUDLine1_archetype, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
				_HUDLine1_curr.transform.SetParent(_HUDCanvasObj.transform);

				_HUDLine1_rt = _HUDLine1_curr.GetComponent<RectTransform>();
				_HUDLine1_img = _HUDLine1_curr.GetComponent<Image>();

				//

				_HUDPointA_curr = Instantiate(_HUDPoint1_archetype, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
				_HUDPointA_curr.transform.SetParent(_HUDCanvasObj.transform);

				_HUDPointA_rt = _HUDPointA_curr.GetComponent<RectTransform>();
				_HUDPointA_img = _HUDPointA_curr.GetComponent<Image>();
				//
				_HUDPointB_curr = Instantiate(_HUDPoint1_archetype, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
				_HUDPointB_curr.transform.SetParent(_HUDCanvasObj.transform);

				_HUDPointB_rt = _HUDPointB_curr.GetComponent<RectTransform>();
				_HUDPointB_img = _HUDPointB_curr.GetComponent<Image>();
			}

			if (_HUDLine1_curr != null)
			{
				//Vector2 screenCentre = new Vector2(Screen.width / 2, Screen.height / 2);
				//Vector2 screenCentreB = new Vector2(0, 0);

				Vector2 mousePos2D = Input.mousePosition;

				//

				_HUDPointA_rt.position = mousePos2D;
				_HUDPointB_rt.position = _xtSelObj_2DPos;

				//

				float dist = Vector2.Distance(mousePos2D, _xtSelObj_2DPos);

				float szX = dist;
				//int szX = Mathf.RoundToInt(dist);
				//if (szX % 2 != 0) szX += 1; // Round to nearest power of 2

				Vector2 betweenPos = BBBStatics.BetweenAt(mousePos2D, _xtSelObj_2DPos, 0.5f);
				//Vector2 betweenPos = BBBStatics.BetweenAt(mousePos2D, screenCentreB, 0.5f);

				_HUDLine1_rt.position = betweenPos;
				_HUDLine1_rt.sizeDelta = new Vector2(szX, _HUDLine1_rt.sizeDelta.y);

				float angle = (Mathf.Atan2((_xtSelObj_2DPos.y - mousePos2D.y), (_xtSelObj_2DPos.x - mousePos2D.x)) * Mathf.Rad2Deg);
				_HUDLine1_rt.rotation = Quaternion.Euler(new Vector3(_HUDLine1_rt.rotation.eulerAngles.x, _HUDLine1_rt.rotation.eulerAngles.y, angle));

				//

				if (tgo_xtSelObj._team == Team.Enemy)
				{
					_HUDLine1_img.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
					_HUDPointA_img.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
					_HUDPointB_img.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);

					Cursor.SetCursor(_cursor_target, Vector2.zero, CursorMode.Auto); // Default
				}
				else if (tgo_xtSelObj._team == Team.Neutral) // Shouldn't be possible yet, but as this may change...
				{
					_HUDLine1_img.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					_HUDPointA_img.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					_HUDPointB_img.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

					Cursor.SetCursor(_cursor_main, Vector2.zero, CursorMode.Auto); // Default
				}
				else if (tgo_xtSelObj._team == Team.Friendly)
				{
					_HUDLine1_img.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
					_HUDPointA_img.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
					_HUDPointB_img.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);

					Cursor.SetCursor(_cursor_select, Vector2.zero, CursorMode.Auto); // Default
				}
			}
		}
		else // There is no target - purge the line and end points & their component refs
		{
			if (_HUDLine1_curr != null)
			{
				Destroy(_HUDLine1_curr);
				_HUDLine1_rt = null;
				_HUDLine1_img = null;

				Destroy(_HUDPointA_curr);
				_HUDPointA_rt = null;
				_HUDPointA_img = null;

				Destroy(_HUDPointB_curr);
				_HUDPointB_rt = null;
				_HUDPointB_img = null;
			}
		}
	}

	void OnGUI() // For Debug Labels
	{
		var restoreColor = GUI.color;
		GUI.color = Color.green; // red

        //for (int i = 0; i < _tgosOnScreen.Count; ++i)
        //{
        //	if (_tgosOnScreen[i] != null && _tgosOnScreen[i].GetComponent<TrainGameObjScript>()._commSocketObj != null)
        //	{
        //		Vector3 screenPos = Camera.main.WorldToScreenPoint(_tgosOnScreen[i].GetComponent<TrainGameObjScript>()._commSocketObj.transform.position);
        //		Vector2 sp2D = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));
        //		float sp2Dxabs = Mathf.Abs(sp2D.x);
        //		UnityEditor.Handles.Label(_tgosOnScreen[i].GetComponent<TrainGameObjScript>()._commSocketObj.transform.position, "sp2Dxabs: " + sp2Dxabs.ToString());
        //	}
        //}

        //


        int guiStrsCount = _guiTextStrs.Count;
		for (int i = 0; i < guiStrsCount; ++i)
		{
			GUI.color = _guiTextColours[i];

#if UNITY_EDITOR
			// if (!Application.isEditor) 
			if (!Application.isEditor) UnityEditor.Handles.Label(_guiTextLocs[i], _guiTextStrs[i]);
#endif
		}

		_bPending_Debug_ClearGUIText_WM = true; // Data must be cleared after this frame or nothing will be shown

		GUI.color = restoreColor;
	}

	public void Debug_ClearGUIText_WM()
	{
		_guiTextStrs.Clear();
		_guiTextLocs.Clear();
		_guiTextColours.Clear();
	}

	public void Debug_AddOnGUIText_WS(string str, Vector3 vec, Color colour) // So we can add GUI text from anywhere in the code, not just under OnGUI()s
	{
		_guiTextStrs.Add(str);
		_guiTextLocs.Add(vec);
		_guiTextColours.Add(colour);
	}

	//public bool bIsShopMenuOpen
	//{
	//	get { return _bIsShopMenuOpen; }
	//}

	public GameObject MouseOverObj { get { return _mouseOverObj; } set { _mouseOverObj = value; } }

	private void GetMouseWorldPos()
	{
		_mouseWorldPos = new Vector3(0, 0, 0);

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			//print("hit obj name: " + hit.collider.transform.name);

			_mouseWorldPos = hit.point;
		}

		_mouseWorldHit = hit;
	}

	private void ManageMouseOver()
	{
		/////
		/////
		///// TEST
		//Vector3 mp = Input.mousePosition;
		//mp.z = mp.y;
		//mp.y = 0;
		//Vector3 screenMP = Camera.main.WorldToScreenPoint(mp);
		//MapBoundsEdges testBounds = MapBoundsEdges.None;
		//bool b = CheckIfVecWithinMapBounds(screenMP, out testBounds, 0.0f);
		//if (b)
		//	Debug.DrawLine(screenMP, screenMP + new Vector3(0, 600, 0), Color.green, Time.deltaTime);
		//else
		//	Debug.DrawLine(screenMP, screenMP + new Vector3(0, 600, 0), Color.red, Time.deltaTime);
		/////
		/////
		/////

		//

		bool bOnlyUseXTRS = false;

		_mouseOverObj = null;
		_xtSelObj = null;

		_xtSelObj_2DPos = Vector2.zero;

		if (!WorldScript.GameplayScript.bCanPlayerSelectObjs) return;

		//if (WorldScript.GameplayScript.PlayerSelectedObjects.Count > 0) return; // We've already got something selected
		if (WS.CS.BHoveringMouseOverConsButton) return;

		if (WorldScript.GPS.BAreaSelectionActive) return;

		if (!bOnlyUseXTRS)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				//print("hit obj name: " + hit.collider.transform.name);

				_mouseOverObj = hit.collider.transform.gameObject;

				bool b_xrs = false;
				if (_mouseOverObj != null)
				{
					TrainGameObjScript tgo = BBBStatics.TGO(_mouseOverObj);
					if (tgo == null || (!tgo._bCanBeSelectedByPlayer && !tgo._bCanBeTargetedByPlayer))
					{
						// We're hovering our mouse over a non-TGO or a TGO that cannot be targeted/selected
						b_xrs = true;
					}
					else // else we're already hovering over something we can use -- no need for extended range
					{
						_xtSelObj = _mouseOverObj;
						if (tgo.CommSocketObj != null) _xtSelObj_2DPos = Camera.main.WorldToScreenPoint(tgo.CommSocketObj.transform.position);
						else _xtSelObj_2DPos = Camera.main.WorldToScreenPoint(_mouseOverObj.transform.position);
					}
				}
				else
				{
					// We're not hovering our mouse over anything
					b_xrs = true;
				}

				if (b_xrs)
				{
					ExtendedRangeSelection();
				}
			}
		}
		else
		{
			ExtendedRangeSelection();
		}

		Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit2;
		if (Physics.Raycast(ray2, out hit2))
		{
			_mouseOverObj_alwaysOn = hit2.collider.transform.gameObject;
		}
	}

	/// <summary>
	/// So we are able to select/target objects without actually hovering the cursor directly over them [15-4-18]
	/// </summary>
	private void ExtendedRangeSelection()
	{
		float xrsMaxRange = 50.0f; // 150.0f until 31-5-18

		Vector2 mp = Input.mousePosition;

		float closestDist = float.PositiveInfinity;
		GameObject closestObj = null;
		Vector2 closestObjScreenPos = Vector2.zero;

		bool plSel = (WS.GPS.PlayerSelectedObjects.Count > 0);

        int _tgosCount = _tgosOnScreen.Count;
		for (int i = 0; i < _tgosCount; ++i)
		{
			// Can only xtr select enemies if we already have friendlies selected
			if ((_tgosOnScreen[i]._team == Team.Enemy && plSel) || _tgosOnScreen[i]._team == Team.Friendly) // Must be a friendly or an enemy tgo
			{
				ConsPlatformScript cps = _tgosOnScreen[i].GetComponent<ConsPlatformScript>();

				// Not a cons platform or a cons platform that has no fixture + all cps are selectable (SelectableConsPlatforms.Count == 0) or it's in the list of selectable ones
				if (cps == null || (cps != null && cps.Fixture == null &&
					(WorldScript.GameplayScript.SelectableConsPlatforms.Count == 0 || WorldScript.GameplayScript.SelectableConsPlatforms.Contains(cps))))
				{
                    // TODO: This is a rough quick fix. find a different method later.
                    //Check if the object contains a turret component. even if not selectable, if the object has a turret component, the mouse hover function still works.
                    TurretScriptParent turretScript = null;
                    try
                    {
                        turretScript = _tgosOnScreen[i].GetComponent<TurretScriptParent>();
                    }
                    catch
                    {
                    }
                    if ((_tgosOnScreen[i]._bCanBeSelectedByPlayer && _tgosOnScreen[i]._team == Team.Friendly)
						|| (_tgosOnScreen[i]._bCanBeTargetedByPlayer && _tgosOnScreen[i]._team == Team.Enemy)
                        || (!_tgosOnScreen[i]._bCanBeTargetedByPlayer && turretScript != null))
					{
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(_tgosOnScreen[i].transform.position);
                        //Vector2 screenPos2D = BBBStatics.VXZ(screenPos); // Bad!

                        float dist = Vector2.Distance(mp, screenPos); // screenPos2D
                        if (dist < xrsMaxRange && dist < closestDist)
                        {
                            closestObj = _tgosOnScreen[i].gameObject;
                            closestDist = dist;
                            closestObjScreenPos = screenPos;
                        }
					}
				}
			}
		}

		_mouseOverObj = closestObj;
		_xtSelObj = closestObj;
		_xtSelObj_2DPos = closestObjScreenPos;
	}

	private void DrawAreaSelectionBox()
	{
		if (WS.GPS.BAreaSelectionActive)
		{
			if (!_HUDAreaSelectionBox.activeInHierarchy) _HUDAreaSelectionBox.SetActive(true);
			RectTransform rt = _HUDAreaSelectionBox.GetComponent<RectTransform>();

			Vector2 mp = (Vector2)Input.mousePosition - new Vector2(Screen.width / 2, Screen.height / 2);
			Vector2 ip = WS.GPS.AreaSelectionInitialPos - new Vector2(Screen.width / 2, Screen.height / 2);

			if (mp.x > ip.x) // Mouse to the [right] of initial click pos
			{
				if (mp.y < ip.y) // Mouse to the [bottom-right] of initial click pos
				{
					rt.anchoredPosition = ip;
					//print("[bottom-right]");
				}
				else // Mouse to the [top-right] of initial click pos
				{
					// Initial's x, cursor's y
					rt.anchoredPosition = new Vector2(ip.x, mp.y);
					//print("[top-right]");
				}
			}
			else // Mouse to the [left] of initial click pos
			{
				if (mp.y < ip.y) // Mouse to the [bottom-left] of initial click pos
				{
					// Cursor's x, initial's y
					rt.anchoredPosition = new Vector2(mp.x, ip.y);
					//print("[bottom-left]");
				}
				else // Mouse to the [top-left] of initial click pos
				{
					rt.anchoredPosition = mp;
					//print("[top-left]");
				}
			}

			rt.sizeDelta = new Vector2(Mathf.Abs(ip.x - mp.x), Mathf.Abs(ip.y - mp.y));

			// Save the values so we can use them to determine the screen selection area in GameplayScript
			_HUDAreaSelectionBox_topLeft = rt.anchoredPosition + new Vector2(Screen.width / 2, Screen.height / 2);
			_HUDAreaSelectionBox_btmRight = rt.anchoredPosition + new Vector2(Screen.width / 2, Screen.height / 2) + new Vector2(rt.sizeDelta.x, -rt.sizeDelta.y);
		}
		else
		{
			if (_HUDAreaSelectionBox.activeInHierarchy) _HUDAreaSelectionBox.SetActive(false);

			_HUDAreaSelectionBox_topLeft = Vector2.zero;
			_HUDAreaSelectionBox_btmRight = Vector2.zero;
		}
	}

	public GameObject DescriptionBox { get { return _descriptionBox; } }
	public GameObject HUDCanvas { get { return _HUDCanvasObj; } }

	public GameObject XtSelObj { get { return _xtSelObj; } set { _xtSelObj = value; } }

    public Vector2 GetScreenScale()
    {
        return HUDCanvas.GetComponent<RectTransform>().localScale;
    }
    public void TriggerConstructionAestheticsBars()
    {
        if (_constructionBarsForAesthetics != null)
        {
            _constructionBarsForAesthetics.TriggerActivity();
        }
    }

}
