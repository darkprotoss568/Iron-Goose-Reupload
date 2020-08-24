using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript_RTS : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } }
	public WorldScript WS { get { return _worldScript; } }
	public HUDScript HS { get { return _worldScript.HUDScript; } } // Shortcut to main HUDScript

	//

	private GameObject _selGroup_archetype;
	private Vector2 _selGroup_initialPos = new Vector2(64, -44);
	private Vector2 _selGroup_distApart = new Vector2(64, 0);
	private List<GameObject> _selGroupObjs;
	private List<List<GameObject>> _selGroups_elements = new List<List<GameObject>>();

	private GameObject _selGroup_element_archetype;
	private Vector2 _selGroup_element_initialPos = new Vector2(-27, -35);
	private float _selGroup_element_distApart = 6;

	public int _test_all_element_count = 0;

	public void Start()
	{
		_selGroup_archetype = Resources.Load("HUDSelGroup") as GameObject;
		_selGroup_element_archetype = _selGroup_archetype.transform.Find("HUDSelGroup_Element").gameObject;

		_selGroupObjs = new List<GameObject>();

		/*if (WS.gameType == GameType.RTS)
		{
			for (int i = 0; i < 10; ++i)
			{
				GameObject go = Instantiate(_selGroup_archetype, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
				go.transform.SetParent(HS.HUDCanvas.transform);

				RectTransform rt = go.GetComponent<RectTransform>();
				Vector2 distApart = new Vector2(_selGroup_distApart.x * i, _selGroup_distApart.y * i);
				rt.anchoredPosition = _selGroup_initialPos + distApart;

				GameObject _hotKeyTextObj = go.transform.Find("HotKeyText").gameObject;
				Text t = _hotKeyTextObj.GetComponent<Text>();
				string tempStr = (i + 1).ToString();
				if (tempStr == "10") tempStr = "0";

				t.text = tempStr;

				_selGroupObjs.Add(go);

				_selGroups_elements.Add(new List<GameObject>());
			}
		}*/
	}

    public void Update()
    {
        if (PauseMenu.isPaused) return;
    }

	private void ManageSelectionGroups_HUD()
	{
		for (int i = 0; i < WS.CS.SelGroups.Count; ++i) // Should be 10
		{
			int grpUnitCount = WS.CS.SelGroups[i].Count; // Number of units in the current selection group

			//grpUnitCount = 5 * i; /// TEST

			// Make sure there are the right number of elements
			while (_selGroups_elements[i].Count < grpUnitCount)
			{
				GameObject go = Instantiate(_selGroup_element_archetype, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
				go.transform.SetParent(_selGroupObjs[i].transform);
				if (!go.activeInHierarchy) go.SetActive(true);

				_selGroups_elements[i].Add(go);

				++_test_all_element_count;

				//print("_selGroups_elements[i].Add(go);");
			}
			while (_selGroups_elements[i].Count > grpUnitCount)
			{
				Destroy(_selGroups_elements[i][0]);
				_selGroups_elements[i].RemoveAt(0);

				--_test_all_element_count;
			}

			//

			int row = 1;

			for (int j = 0; j < _selGroups_elements[i].Count; ++j)
			{
				RectTransform rt = _selGroups_elements[i][j].GetComponent<RectTransform>();
				//Image img = _selGroups_elements[i][j].GetComponent<Image>();

				if (j > 0 && j % 10 == 0) ++row;

				//Vector2 offset = new Vector2((_selGroup_element_distApart * j) - (_selGroup_element_distApart * 10 * (row-1)), _selGroup_element_distApart * -row);
				Vector2 offset = new Vector2((_selGroup_element_distApart * j) - (60 * (row-1)), _selGroup_element_distApart * -row);



				Vector2 nextPos = new Vector2(_selGroup_element_initialPos.x + offset.x, _selGroup_element_initialPos.y + offset.y);
				rt.anchoredPosition = nextPos;

				//print("pos: " + i + ": " + j + ": " + nextPos);
			}
		}

		//print("elements: " + _test_all_element_count);
	}
}