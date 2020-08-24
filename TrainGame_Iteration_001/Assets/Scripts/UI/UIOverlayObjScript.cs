using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PositioningType
{
    FollowAnObject,
    ScreenCenter,
    Static
}

public abstract class UIOverlayObjScript : MonoBehaviour
{
    protected GameObject _followObject;                     // The GameObject this UIOverlayObj must follow
    [SerializeField]
    protected bool _bFollowObjectIsOnUI;
    [SerializeField]
    protected bool _bUsesParentTransform;

    [SerializeField]
    private Vector2 _staticOffset;
    private Vector2 _originalStaticOffset;
    [SerializeField]
    protected bool _bStaysInHiearchy;
    private Vector3 _staticPosition;

    [SerializeField]
    protected PositioningType _positionType;

    public virtual void Start()
    {
        _originalStaticOffset = new Vector2(_staticOffset.x, _staticOffset.y);
        _staticPosition = new Vector3(_staticOffset.x, _staticOffset.y, 0);
        if (_positionType == PositioningType.Static)
            SetStaticPos();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!PauseMenu.isPaused)
        {
            ManagePosition();
        }
    }

    /// <summary>
    /// Set the parameters to follow an object
    /// </summary>
    /// <param name="followObject">The object this UIOverlayObj would follow</param>
    /// <param name="bObjectIsOnUI">True if the object is on UI Space and not world space</param>
    /// <param name="staticOffset">The offset from the followed gameObject's position on-screen</param>
    public void SetFollowObject(GameObject followObject, bool bObjectIsOnUI, Vector2 staticOffset)
    {
        _bFollowObjectIsOnUI = bObjectIsOnUI;
        _followObject = followObject;
        _staticOffset = staticOffset + _staticOffset;
    }

    /// <summary>
    /// Manage the position of the UIOverlayObj
    /// </summary>
    protected void ManagePosition()
    {
        if (PauseMenu.isPaused) return;

        gameObject.transform.parent.SetAsLastSibling();

        switch (_positionType)
        {
            case PositioningType.FollowAnObject:
                if (_followObject != null)
                {
                    // Only follow the followed object if a reference to it exists
                    if (_bFollowObjectIsOnUI)
                    {
                        FollowUIObject();
                    }
                    else
                    {
                        if (!IsFollowObjectOffScreen())
                            FollowGameObject();
                        else
                            Deactivate();
                    }
                }
                else
                {
                    Deactivate();
                }
                break;
            case PositioningType.ScreenCenter:
                CenterPosition();
                break;
            case PositioningType.Static:
                break;
        }
        
    }

    protected void CenterPosition()
    {
        RectTransform screenRect = GameObject.Find("MainHolder").transform.Find("WorldScriptHolder").GetComponent<WorldScript>().HUDScript.HUDCanvas.GetComponent<RectTransform>();
        RectTransform parentBoxRect = gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
        // Get the RectTransform of the object
        RectTransform boxRect = gameObject.GetComponent<RectTransform>();

        float posX = Screen.width * 0.5f;
        float posY = Screen.height * (0.5f - 0.03f);
        Vector3 newPos = new Vector3(posX, posY, 0);

        // Get offset x and y coordinates of the UIOverlayObj from the followed object's position
        if (_bUsesParentTransform)
        {
            if (parentBoxRect.position != newPos)
                parentBoxRect.position = newPos;
        }
        else
        {
            if (boxRect.position != newPos)
                boxRect.position = newPos;
        }
    }

    protected void SetStaticPos()
    {
        RectTransform screenRect = GameObject.Find("MainHolder").transform.Find("WorldScriptHolder").GetComponent<WorldScript>().HUDScript.HUDCanvas.GetComponent<RectTransform>();
        RectTransform parentBoxRect = gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
        // Get the RectTransform of the object
        RectTransform boxRect = gameObject.GetComponent<RectTransform>();
        if (_bUsesParentTransform)
        {
            if (parentBoxRect.localPosition != _staticPosition)
                parentBoxRect.localPosition = _staticPosition; 
        }
        else
        {
            if (boxRect.localPosition != _staticPosition)
                boxRect.localPosition = _staticPosition; ;
        }
    }

    /// <summary>
    /// Force the UIOverlayObj to follow an UI space Object
    /// </summary>
    private void FollowUIObject()
    {
        // Get the RectTransform of the HUDCanvas to get the current width and height scale factors
        RectTransform rect;
        if (_bUsesParentTransform)
            rect = gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
        else
            rect = gameObject.GetComponent<RectTransform>();
        // Get the RectTransform of the followed UI object
        RectTransform followTargetRect = _followObject.GetComponent<RectTransform>();

        float posX;
        float posY;

        // Get offset x and y coordinates of the UIOverlayObj from the followed object's position
        posX = followTargetRect.position.x + _staticOffset.x;
        posY = followTargetRect.position.y + _staticOffset.y;

        // Get the minimum and maximum x and y positions for the UIOverlayObjectScript
        float minX;
        float maxX;
        float minY;
        float maxY;

        GetOnScreenLimits(rect, out minX, out maxX, out minY, out maxY);

        // Check if the box is on the top or the bottom half of the screen to "flow" it upward or downward to avoid overlapping with other UI elements.
        if (posX > maxX)
        {
            if (posY < Screen.height / 2)
            {
                posY += rect.sizeDelta.y;
            }
            else
            {
                posY -= followTargetRect.sizeDelta.y / 2;
            }
        }

        // Clamp the position of the UIOverlayObj
        posX = Mathf.Clamp(posX, minX, maxX);
        posY = Mathf.Clamp(posY, minY, maxY);

        // Finalize the movement of the UIOverlayObj
        rect.position = new Vector2(posX, posY);

    }

    /// <summary>
    /// Force the UIOVerlayObj to follow a world space object
    /// </summary>
    private void FollowGameObject()
    {
        
        // Get the RectTransform of the parent object
        // Get the RectTransform of the object
        RectTransform rect;
        if (_bUsesParentTransform)
            rect = gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
        else
            rect = gameObject.GetComponent<RectTransform>();

        float posX = GetFollowedObjectPositionOnScreen().x + _staticOffset.x;
        float posY = GetFollowedObjectPositionOnScreen().y + _staticOffset.y;

        // Get the minimum and maximum x and y positions for the UIOverlayObjectScript
        float minX;
        float maxX;
        float minY;
        float maxY;

        GetOnScreenLimits(rect, out minX, out maxX, out minY, out maxY);
        // Clamp the position of the UIOverlayObj.
        Vector2 finalPos = new Vector2(Mathf.Clamp(posX, minX, maxX),
                                       Mathf.Clamp(posY, minY, maxY));

        // Finalize the movement of the UIOverlayObj
        rect.position = finalPos;
    }

    /// <summary>
    /// Check whether the followed object is off-screen. Return false if the object is within the view, true if otherwise.
    /// </summary>
    /// <returns></returns>
    private bool IsFollowObjectOffScreen()
    {
        // Get the coordinates of the followedObj relative the the screen view.
        Vector2 followedObjPos = GetFollowedObjectPositionOnScreen();
        float posX = followedObjPos.x;
        float posY = followedObjPos.y;

        // Check if the X and Y value are both within 0 and screen width and screen height respectively
        if (posY > 0 && posY < Screen.height && posX < Screen.width && posX > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Get the minimum/ maximum X value. Limitation: Only works when the anchor of the RectTransform is at the bottom left corner
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="minX"></param>
    /// <param name="maxX"></param>
    /// <param name="minY"></param>
    /// <param name="maxY"></param>
    protected void GetOnScreenLimits(RectTransform rect, out float minX, out float maxX, out float minY, out float maxY)
    {
        RectTransform screenRect = GameObject.Find("MainHolder").transform.Find("WorldScriptHolder").GetComponent<WorldScript>().HUDScript.HUDCanvas.GetComponent<RectTransform>();
        
        // TODO: Add Padding
        minX = 0 + rect.sizeDelta.x * rect.pivot.x * screenRect.localScale.x;
        maxX = Screen.width - rect.sizeDelta.x * (1- rect.pivot.x) * screenRect.localScale.x;
        minY = 0 + rect.sizeDelta.y * rect.pivot.y * screenRect.localScale.y;
        maxY = Screen.height - rect.sizeDelta.y * (1 - rect.pivot.y) * screenRect.localScale.y;
    }
    /// <summary>
    /// Get the on-screen coordinates of the followed object relative to the main camera view
    /// </summary>
    /// <returns></returns>
    private Vector2 GetFollowedObjectPositionOnScreen()
    {
        Camera viewCamera = Camera.main;
        Vector3 screenPos;
        // Check if the followed object has a TrainGameObjScript component attached. In which case, the position returned would be the CommSocketObj's position.
        // Otherwise, the position returned would be the followed object's actual position.
        if (_followObject.GetComponent<TrainGameObjScript>() != null)
        {
            screenPos = viewCamera.WorldToScreenPoint(_followObject.GetComponent<TrainGameObjScript>().CommSocketObj.transform.position);
        }
        else
            screenPos = viewCamera.WorldToScreenPoint(_followObject.transform.position);
        //Vector2 result = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));
        return screenPos;
    }


    /// <summary>
    /// Deactive the UIOverlayObj using different methods depending on the _bStaysInHiearchy and the _bUsersParentTransform fields.
    /// If _bUsesParenTransform is true, the target object would be the parent object, if false, the target object would be gameObject containing this script.
    /// If _bStaysInHiearchy is true, deactiation is performed by setting the target object to inactive, if true, deactivation is performed by deleting the target object. 
    /// </summary>
    public virtual void Deactivate()
    {
        // Check whether the UIOverlayObj is meant to stay in the hiearchy when deactivated or must be destroyed.
        if (_bStaysInHiearchy)
        {
            _staticOffset = new Vector2(_originalStaticOffset.x, _originalStaticOffset.y);
            if (_bUsesParentTransform)
                gameObject.transform.parent.gameObject.SetActive(false);
            else
                gameObject.SetActive(false);
        }
        else
        {
            if (_bUsesParentTransform)
                Destroy(gameObject.transform.parent.gameObject);
            else
                Destroy(gameObject);

        }
    }

    /// <summary>
    /// If the UIOverlayObj is only set to inactive in the hiearchy instead of destroyed, move the UIOverlayObj to the correct position in relation to the followed object's.
    /// </summary>
    public virtual void Activate()
    {
        if (!_bFollowObjectIsOnUI && IsFollowObjectOffScreen())
        {
            // If the followed object is in world space and currently offscreen, only change the position of the UIOverlayObj
            ManagePosition();
        }
        else
        {
            // Otherwise, change the position of the UIOverlayObj and set either the parent gameObject or the gameObject to active in the hiearchy
            ManagePosition();
            if (_bUsesParentTransform)
                gameObject.transform.parent.gameObject.SetActive(true);
            else
                gameObject.SetActive(true);
        }
    }
}
