using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusOverlayObj : UIOverlayObjScript
{
    
    public float _maxHealthBarLength;
    private TrainGameObjScript _connectedTGO;
    private RectTransform _outlineRect;
    private RectTransform _barRect;
    //ivate Image _barColor;
    
    private float _timeSinceOpened;
    public float _maxOpenTime;
    
	// Use this for initialization
	public override void Start ()
    {
        base.Start();
        _timeSinceOpened = 0;
        _outlineRect = transform.Find("BackgroundBar").gameObject.GetComponent<RectTransform>();
        _barRect = transform.Find("MainBar").gameObject.GetComponent<RectTransform>();
        if (_maxHealthBarLength == 0)
        {
            _maxHealthBarLength = _barRect.sizeDelta.x;
        } else
        {
            _outlineRect.sizeDelta = new Vector2(_maxHealthBarLength, _outlineRect.sizeDelta.y);
            _barRect.sizeDelta = _outlineRect.sizeDelta;
        }
        Deactivate();
    }
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

        if (_timeSinceOpened >= _maxOpenTime)
        {
            Deactivate();
        }
        else
        {
            _timeSinceOpened += Time.deltaTime;
            base.Update();
        }
	}

    public void UpdateState()
    {
        //_barColor = transform.Find("MainBar").gameObject.GetComponent<Image>();   
        float currHP = _connectedTGO._currentHealth;
        float maxHP = _connectedTGO._maxHealth;
        double remainingPercentage = currHP / maxHP;
        _outlineRect.sizeDelta = new Vector2((float)(_maxHealthBarLength * remainingPercentage), _outlineRect.sizeDelta.y);
        _barRect.sizeDelta = _outlineRect.sizeDelta;
        Activate();
        /*
        if (remainingPercentage < 0.3f)
        {
            _barColor.color = new Color(1, 0, 0);
        }
        else if (remainingPercentage < 0.65f)
        {
            _barColor.color = new Color(1, 1, 0);
        }
        else
        {
            _barColor.color = new Color(0, 1, 0);
        }
        */
    }

    public void Initialize(TrainGameObjScript connectedTGO, GameObject followObject)
    {
        _connectedTGO = connectedTGO;
        SetFollowObject(followObject, false, Vector2.zero);
    }

    public override void Activate()
    {
        _timeSinceOpened = 0;
        base.Activate();
    }
    public override void Deactivate()
    {
        if (_connectedTGO == null)
            Destroy(gameObject);
        else
            base.Deactivate();
    }
}