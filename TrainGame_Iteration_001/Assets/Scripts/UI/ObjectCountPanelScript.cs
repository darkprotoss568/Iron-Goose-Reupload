using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ObjectCountPanelScript : MonoBehaviour
{
	[SerializeField]
	private GameObject _objPrefab;

	//private WorldScript _worldScript;
	private int _count;
	private Text _countText;

	public int _initialCount;                // Temporary

	public int _maxCount;
    public bool _limitToMaxCount;
    public int _hardMaxCount;

    // Use this for initialization
    void Start()
	{
		//_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
		_countText = gameObject.transform.GetComponentInChildren<Text>();
		InitializeCount();
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void IncreaseCount(int amount)
	{
		_count += amount;
        UpdateCounter();
	}

    private void UpdateCounter()
    {
        if (_countText != null)
        {
            _countText.text = _count.ToString();
            if (_limitToMaxCount)
            {
                _countText.text += "/" + _maxCount;
            }
        }
    }

    public void IncreaseMaxCount(int amount)
    {
        _maxCount += amount;
        UpdateCounter();
    }

	public bool CheckAtMaximumCount()
    { 
        if (_hardMaxCount > 0)
        {
            if (_count >= _hardMaxCount)
                return true;
        }

        if (_limitToMaxCount)
            return (_count >= _maxCount);
        else
            return false;
	}

	private void InitializeCount()
	{
        IncreaseCount(_initialCount);
        UpdateCounter();
        
	}

	public GameObject Archetype
	{
		get
		{
			return _objPrefab;
		}
	}
}
