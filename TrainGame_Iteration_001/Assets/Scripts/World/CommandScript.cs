using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandScript : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } } public WorldScript WS { get { return _worldScript; } }

	private float _timeSinceGaveOrder = 0.0f; public float TimeSinceGaveOrder { get { return _timeSinceGaveOrder; } set { _timeSinceGaveOrder = value; } }
	private int _framesSinceGaveOrder = 0;

	private float _timeSinceGaveTargetingOrder = 0.0f; public float TimeSinceGaveTargetingOrder { get { return _timeSinceGaveTargetingOrder; } set { _timeSinceGaveTargetingOrder = value; } }
	private int _framesSinceEnemyTargeted = 0;

	private GameObject _buildObjArchetype = null;
	public GameObject BuildObjArchetype { get { return _buildObjArchetype; } set { _buildObjArchetype = value; } }

	private bool _bInRecycleMode = false;
	public bool BInRecycleMode { get { return _bInRecycleMode; } set { _bInRecycleMode = value; } }

	private bool _bHoveringMouseOverConsButton = false;
	public bool BHoveringMouseOverConsButton { get { return _bHoveringMouseOverConsButton; } set { _bHoveringMouseOverConsButton = value; } }

	private int _selectionGroupsCount = 10; public int SelectionGroupsCount { get { return _selectionGroupsCount; } }
	private List<List<GameObject>> _selGroups = new List<List<GameObject>>(); public List<List<GameObject>> SelGroups { get { return _selGroups; } }

	private int _currPressedNumKey = -1;
	private bool _bCtrlBtnPressed = false;

	void Awake()
	{
	}

	void Start()
	{
		for (int i = 0; i < _selectionGroupsCount; ++i)
		{
			_selGroups.Add(new List<GameObject>());
		}
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		//

		CheckCurrPressedNumKey();

		

		if (!WS.ConstructionManager.bIsConsMenuOpen) { _bHoveringMouseOverConsButton = false; }

			PlayerControlOfAllFriendlyTurrets();

		_timeSinceGaveOrder += Time.deltaTime;
		++_framesSinceGaveOrder;

		_timeSinceGaveTargetingOrder += Time.deltaTime;
		++_framesSinceEnemyTargeted;
	}

	private void CheckCurrPressedNumKey()
	{
		if (Input.GetKey(KeyCode.Alpha1)) _currPressedNumKey = 1;
		else if (Input.GetKey(KeyCode.Alpha2)) _currPressedNumKey = 2;
		else if (Input.GetKey(KeyCode.Alpha3)) _currPressedNumKey = 3;
		else if (Input.GetKey(KeyCode.Alpha4)) _currPressedNumKey = 4;
		else if (Input.GetKey(KeyCode.Alpha5)) _currPressedNumKey = 5;
		else if (Input.GetKey(KeyCode.Alpha6)) _currPressedNumKey = 6;
		else if (Input.GetKey(KeyCode.Alpha7)) _currPressedNumKey = 7;
		else if (Input.GetKey(KeyCode.Alpha8)) _currPressedNumKey = 8;
		else if (Input.GetKey(KeyCode.Alpha9)) _currPressedNumKey = 9;
		else if (Input.GetKey(KeyCode.Alpha0)) _currPressedNumKey = 0;
		else _currPressedNumKey = -1;

		//print("_currPressedNumKey: " + _currPressedNumKey);

		if (Input.GetKeyDown(KeyCode.LeftShift)) _bCtrlBtnPressed = true;
		if (Input.GetKeyUp(KeyCode.LeftShift)) _bCtrlBtnPressed = false;
	}

	private void ControlAIUnits()
	{
		if (WS.GPS.PlayerSelectedObjects.Count > 0 && !_bHoveringMouseOverConsButton) // && !WS.ConstructionManager.bIsConsMenuOpen)
		{
			WS.GPS.BSelectionActive = false; // Make sure that selection is blocked while anything is selected

			if (!_bInRecycleMode)
			{
				if (WS.HS.MouseWorldHit.collider != null && _buildObjArchetype == null) // Non-construction mode
				{
					// Pointing at the ground
					if (WS.HS.MouseWorldHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && WS.HS.XtSelObj == null)
					{
						// GoTo

						if (!IsTypeSelected(typeof(TurretScriptParent))) // Turrets can't move
						{
							Cursor.SetCursor(WS.HS.Cursor_goto, new Vector2(16.0f, 16.0f), CursorMode.Auto);

							if (Input.GetMouseButtonDown(0))
							{
								GiveGoToCommand();
								WS.GPS.DeselectAll();
								WS.GPS.BPendingSetSelectionActive = true;
							}
						}
						else
						{
							Cursor.SetCursor(WS.HS.Cursor_impossible, new Vector2(16.0f, 16.0f), CursorMode.Auto);
						}
					}
					else // Not pointing at the ground
					{
						//GameObject go = WS.HS.MouseWorldHit.collider.gameObject;
						GameObject go = WS.HS.XtSelObj;
						if (go != null && !WS.GPS.PlayerSelectedObjects.Contains(go)) // We don't want to attack or follow our own selected units
						{
							TrainGameObjScript tgo = go.GetComponent<TrainGameObjScript>();
							if (tgo != null)
							{
								if (tgo._team == Team.Friendly || tgo._team == Team.Neutral)
								{
									// Follow

									if (!IsTypeSelected(typeof(TurretScriptParent))) // Turrets can't follow
									{
										Cursor.SetCursor(WS.HS.Cursor_follow, new Vector2(16.0f, 16.0f), CursorMode.Auto);

										if (Input.GetMouseButtonDown(0))
										{
											GiveFollowCommand(go);
											WS.GPS.DeselectAll();
											//WS.GPS.BPendingSetSelectionActive = true;
										}
									}
									else
									{
										Cursor.SetCursor(WS.HS.Cursor_impossible, new Vector2(16.0f, 16.0f), CursorMode.Auto);
									}
								}
								else if (tgo._team == Team.Enemy)
								{
									// Attack

									if (!IsTypeSelected(typeof(AIConsDroneScript)) && !IsTypeSelected(typeof(AIScavDroneScript)))
									{
										Cursor.SetCursor(WS.HS.Cursor_attack, new Vector2(16.0f, 16.0f), CursorMode.Auto);

										if (Input.GetMouseButtonDown(0))
										{
											GiveAttackCommand(go);
											WS.GPS.DeselectAll();
											//WS.GPS.BPendingSetSelectionActive = true;
										}
									}
									else
									{
										Cursor.SetCursor(WS.HS.Cursor_impossible, new Vector2(16.0f, 16.0f), CursorMode.Auto);
									}
								}
							}
						}
					}
				}
				else if (WS.HS.MouseWorldHit.collider != null && _buildObjArchetype != null) // Construction mode (can only get here via cons drones in RTS mode)
				{
					int cost = _buildObjArchetype.GetComponent<TrainGameObjScript>().BuildCost;
					bool bCanAfford = cost < _worldScript.GameplayScript.PlayerResources;

					// Pointing at the ground
					if (bCanAfford && WS.HS.MouseWorldHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
					{
						Cursor.SetCursor(WS.HS.Cursor_build, new Vector2(16.0f, 16.0f), CursorMode.Auto);

						if (Input.GetMouseButtonDown(0))
						{
							GiveConstructCommand(); // consSite.GetComponent<ConsSite>()
							WS.GPS.DeselectAll();
						}
					}
					else // Not pointing at the ground or can't afford to build it
					{
						Cursor.SetCursor(WS.HS.Cursor_impossible, new Vector2(16.0f, 16.0f), CursorMode.Auto);
					}
				}
			}
			else
			{
				//print("In recycle mode: " + Time.time);

				if (WS.HS.XtSelObj != null && BBBStatics.TGO(WS.HS.XtSelObj)._team == Team.Friendly)
				{
					Cursor.SetCursor(WS.HS.Cursor_recy, new Vector2(16.0f, 16.0f), CursorMode.Auto);

					if (Input.GetMouseButtonDown(0))
					{
						GiveRecycleCommand(WS.HS.XtSelObj);
						WS.GPS.DeselectAll();
					}
				}
				else
				{
					Cursor.SetCursor(WS.HS.Cursor_impossible, new Vector2(16.0f, 16.0f), CursorMode.Auto);
				}
			}
		}
	}

	private void GiveGoToCommand()
	{
		// We need a grid point count that is a whole number sqrt
		int gridPointCount = 1; // Minimum number of grid points
		for (int i = 0; i < 120; ++i) // Maximum number of grid points
		{
			if (gridPointCount >= WS.GPS.PlayerSelectedObjects.Count && Mathf.Sqrt(gridPointCount) % 1 == 0) { break; }
			++gridPointCount;
		}

		List<Vector3> positionGrid = BBBStatics.MakeVectorGrid_OnGround(WS.HS.MouseWorldPos, gridPointCount, 10.0f);

		//print("gridPointCount: " + gridPointCount);
		//for (int i = 0; i < positionGrid.Count; ++i)
		//{
		//	Debug.DrawLine(positionGrid[i], positionGrid[i] + new Vector3(0, 10, 0), Color.cyan, 5.0f);
		//}

		//print("positionGrid.Count: " + positionGrid.Count);

		for (int i = 0; i < WS.GPS.PlayerSelectedObjects.Count; ++i)
		{
			AIDynamicObjScript ds = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIDynamicObjScript>();
			if (ds != null)
			{
				ds.CurrAITask = AITask.GoTo;
				//ds.CurrAIWaypoints.Clear();
				//ds.PathingDestination = Vector3.zero; // No point

				//if (WS.GPS.PlayerSelectedObjects.Count == 1) ds.CurrAIWaypoints.Add(WS.HS.MouseWorldPos);
				if (WS.GPS.PlayerSelectedObjects.Count == 1) ds.PathingDestination = WS.HS.MouseWorldPos;
				else
				{
					//ds.CurrAIWaypoints.Add(positionGrid[i]);
					ds.PathingDestination = positionGrid[i];
				}

				WorldScript.AS_2DMainAudioSource.PlayOneShot(WorldScript.WS_beep4, 0.75f);
			}

			AIGroundUnitScript gu = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIGroundUnitScript>();
			if (gu != null)
			{
				gu.CurrAINavMeshPathPoints.Clear();
			}
		}
	}

	private void GiveFollowCommand(GameObject followThis)
	{
		for (int i = 0; i < WS.GPS.PlayerSelectedObjects.Count; ++i)
		{
			AIDynamicObjScript ds = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIDynamicObjScript>();
			if (ds != null)
			{
				ds.CurrAITask = AITask.Follow;
				//ds.CurrAIWaypoints.Clear();

				ds.FollowObj = followThis;

				ds.PathingDestination = Vector3.zero;

				WorldScript.AS_2DMainAudioSource.PlayOneShot(WorldScript.WS_beep4, 0.75f);
			}

			AIGroundUnitScript gu = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIGroundUnitScript>();
			if (gu != null)
			{
				gu.CurrAINavMeshPathPoints.Clear();
			}
		}
	}

	private void GiveAttackCommand(GameObject attackThis)
	{
		for (int i = 0; i < WS.GPS.PlayerSelectedObjects.Count; ++i)
		{
			AIDynamicObjScript ds = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIDynamicObjScript>();
			if (ds != null)
			{
				ds.CurrAITask = AITask.Attack;
				ds.AttackObj = attackThis;
				WorldScript.AS_2DMainAudioSource.PlayOneShot(WorldScript.WS_beep4, 0.75f);
			}

			AIGroundUnitScript gu = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIGroundUnitScript>();
			if (gu != null)
			{
				gu.CurrAINavMeshPathPoints.Clear();
			}
		}
	}

	private void GiveConstructCommand() // ConsSite csite
	{
		_worldScript.GameplayScript.SubtractResources(_buildObjArchetype.GetComponent<TrainGameObjScript>().BuildCost);

		// Create a cons site for the object so the selected cons drones can build it
		GameObject consSite = Instantiate(Resources.Load("DefaultConsSite"), WS.HS.MouseWorldHit.point, transform.rotation) as GameObject;
		consSite.GetComponent<ConsSite>().Archetype = _buildObjArchetype;
		consSite.GetComponent<ConsSite>().Team = Team.Friendly;

		for (int i = 0; i < WS.GPS.PlayerSelectedObjects.Count; ++i)
		{
			AIConsDroneScript cds = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIConsDroneScript>();
			if (cds)
			{
				cds.CurrAITask = AITask.Construct;

				//cds.CurrConsSite = consSite;
				cds.SetConsSite(consSite);

				WorldScript.AS_2DMainAudioSource.PlayOneShot(WorldScript.WS_beep4, 0.75f);

				break; // Make sure that only one cons drone can be assigned to this cons site (not sure if necessary here) [Mike, 4-6-18]
			}
		}

		_buildObjArchetype = null;
	}

	private void GiveRecycleCommand(GameObject recycleThis) // [Mike, 5-6-18]
	{
		GameObject recySite = Instantiate(Resources.Load("DefaultRecySite"), recycleThis.transform.position, recycleThis.transform.rotation) as GameObject;
		recySite.GetComponent<RecySite>().Archetype = recycleThis; // Needed in a RecySite so we can copy its mesh elements
		recySite.GetComponent<RecySite>().RecyObj = recycleThis;
		recySite.GetComponent<RecySite>().Team = Team.Friendly;
		//recySite.transform.parent = recycleThis.transform; /// Bad idea
		recySite.SetActive(false);

		for (int i = 0; i < WS.GPS.PlayerSelectedObjects.Count; ++i)
		{
			AIConsDroneScript cds = WS.GPS.PlayerSelectedObjects[i].GetComponent<AIConsDroneScript>();
			if (cds)
			{
				//cds.SetRecySite(consSite);
				cds.SetRecySite(recySite);

				cds.CurrAITask = AITask.Recycle;

				cds.CurrConsMode = ConsMode.Recycle;

				WorldScript.AS_2DMainAudioSource.PlayOneShot(WorldScript.WS_beep4, 0.75f);

				break; // Make sure that only one cons drone can be assigned to this cons site (not sure if necessary here) [Mike, 5-6-18]
			}
		}

		_bInRecycleMode = false;
	}

	// Non-RTS only
	private void PlayerControlOfAllFriendlyTurrets()
	{
		if (WS.GPS.bCanPlayerTargetObjs && WorldScript.HUDScript.MouseOverObj != null && _framesSinceEnemyTargeted > 2)
		{
			TrainGameObjScript tgo = BBBStatics.TGO(WorldScript.HUDScript.MouseOverObj);
			if (tgo != null && tgo._team == Team.Enemy)
			{
				//if (Input.GetMouseButton(0))
				if (Input.GetMouseButtonDown(0))
				{
					TurretScriptParent[] allTurrs = (TurretScriptParent[])FindObjectsOfType(typeof(TurretScriptParent));

					for (int i = 0; i < allTurrs.Length; ++i)
					{
						if (allTurrs[i]._team == Team.Friendly)
						{
							allTurrs[i].Target = tgo;

							_timeSinceGaveTargetingOrder = 0.0f;

							//AudioSource.PlayClipAtPoint(ws_beep4, Camera.main.transform.position, 1.0f);
							WorldScript.AS_2DMainAudioSource.PlayOneShot(WorldScript.WS_beep4, 0.75f);

							_framesSinceEnemyTargeted = 0;
						}
					}
				}
			}
		}
	}

	private bool IsTypeSelected(System.Type type)
	{
		for (int i = 0; i < WS.GPS.PlayerSelectedObjects.Count; ++i)
		{
			if (WS.GPS.PlayerSelectedObjects[i].GetType() == type || WS.GPS.PlayerSelectedObjects[i].GetType().IsSubclassOf(type))
			{
				return true;
			}

			List<MonoBehaviour> comps = new List<MonoBehaviour>(WS.GPS.PlayerSelectedObjects[i].GetComponents<MonoBehaviour>());
			for (int j = 0; j < comps.Count; ++j)
			{
				//print(comps[j].GetType().FullName);

				if (comps[j].GetType() == type || comps[j].GetType().IsSubclassOf(type))
				{
					return true;
				}
			}
		}

		return false;
	}

	private void AddToSelectionGroups()
	{
		/// Add the currently selected units to a group (via ctrl + 1/2/3/4 etc)
		/// A unit can only be in one group at a time

		if (_currPressedNumKey >= 0 && _bCtrlBtnPressed)
		{
			int num = _currPressedNumKey - 1;
			if (_currPressedNumKey == 0) num = 9;

			for (int i = 0; i < WS.GPS.PlayerSelectedObjects.Count; ++i)
			{
				RemoveUnitFromAllSelGroups(WS.GPS.PlayerSelectedObjects[i]);

				_selGroups[num].Add(WS.GPS.PlayerSelectedObjects[i]);
			}
		}
	}

	private void RemoveUnitFromAllSelGroups(GameObject unit)
	{
		for (int i = 0; i < _selGroups.Count; ++i)
		{
			if (_selGroups[i].Contains(unit))
			{
				_selGroups[i].Remove(unit);
			}
		}
	}

	private void CheckSelGroupsForNulls()
	{
		for (int i = 0; i < _selGroups.Count; ++i)
		{
			List<GameObject> keepers = new List<GameObject>();
			for (int j = 0; j < _selGroups[i].Count; ++j)
			{
				if (_selGroups[i][j] != null)
				{
					keepers.Add(_selGroups[i][j]);
				}
			}
			_selGroups[i] = keepers;
		}
	}

	private void SelectSelectionGroup()
	{
		if (_currPressedNumKey >= 0)
		{
			int num = _currPressedNumKey - 1;
			if (_currPressedNumKey == 0) num = 9;

			WS.GPS.DeselectAll();

			for (int i = 0; i < _selGroups[num].Count; ++i)
			{
				WS.GPS.AddPlayerSelectedObj(_selGroups[num][i]);
			}
		}
	}
}
