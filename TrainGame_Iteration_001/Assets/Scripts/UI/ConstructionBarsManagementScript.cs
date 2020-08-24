using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionBarsManagementScript : MonoBehaviour {

    private bool _hiding = true;
    private RectTransform _upperBar;
    private RectTransform _lowerBar;
    [SerializeField]
    private float _transitionTime;
    private float _timeSinceTransitionStarted = 0.0f;
    [SerializeField]
    private HUDScript _hudScript;
	// Use this for initialization
	void Start ()
    {
        _upperBar = transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        _lowerBar = transform.GetChild(1).gameObject.GetComponent<RectTransform>();

	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!PauseMenu.isPaused)
        {
            if (CheckAnimationCompletion())
            {
                if (_hiding)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                _timeSinceTransitionStarted += BBBStatics.GetTimeScaleIndependentDelta();
                float transitionRate = BBBStatics.Map(_timeSinceTransitionStarted, 0.0f, _transitionTime, 0.0f, 1.0f, true);
                if (_upperBar != null)
                {
                    Vector3 targetPos = Vector3.zero;
                    if (_hiding)
                    {
                        targetPos = new Vector3(0.0f, _upperBar.sizeDelta.y, 0.0f);

                    }
                    _upperBar.anchoredPosition = Vector2.Lerp(_upperBar.anchoredPosition, targetPos, transitionRate);

                }
                if (_lowerBar != null)
                {
                    Vector3 targetPos = Vector3.zero;
                    if (_hiding)
                    {
                        targetPos = new Vector3(0.0f, -_lowerBar.sizeDelta.y, 0.0f);
                    }
                    _lowerBar.anchoredPosition = Vector3.Lerp(_lowerBar.anchoredPosition, targetPos, transitionRate);
                }
            }
        }
	}

    private bool CheckAnimationCompletion()
    {
        // TODO: Look into making this more flexible

        if (_upperBar != null)
        {
            Vector2 targetPos = Vector2.zero;
            if (_hiding)
            {
                targetPos = new Vector2(0.0f, _upperBar.sizeDelta.y);
            }
            if (_upperBar.anchoredPosition != targetPos)
            {
                return false;
            }
        }

        if (_lowerBar != null)
        {
            Vector2 targetPos = Vector2.zero;
            if (_hiding)
            {
                targetPos = new Vector2(0.0f, -_lowerBar.sizeDelta.y);
            }
            if (_upperBar.anchoredPosition != targetPos)
            {
                return false;
            }
        }

        return true;
    }
    public void TriggerActivity()
    {

        _timeSinceTransitionStarted = 0.0f;
        _hiding = !_hiding;
        gameObject.SetActive(true);
    }
}
