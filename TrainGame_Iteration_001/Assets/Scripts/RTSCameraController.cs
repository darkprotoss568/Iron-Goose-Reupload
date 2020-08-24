using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
	public float panSpeed = 80.0f; // 60 until 16-4-18
	public float panBorder = 20.0f;
	public GameObject player;

	//private Vector3 _initialPos;
	//private Quaternion _initialRot;

	//private bool camRotate;

	private WorldScript _worldScript;

	private float _maxZoomOutDist = 60.0f; // 50.0f until 18-4-18

	private Vector3 _zoomOffsetFromTarget;
	private Vector3 _zoomOffsetFromTarget_low;
	private Vector3 _zoomOffsetFromTarget_high;

	private Vector3 _posOffsetFromTarget; // Local offset from _followTarget
	//private Vector3 _posOffsetFromTarget_orig;

	private Vector3 _desiredOffset;

	private Vector3 _extra_posOffsetFromTarget;
	private Vector3 _extra_posOffsetFromTarget_orig;

	private float _zoomPercent;
	private float _desiredZoomPercent;
	private int _zoomLevels;

	private float _zoomSpeed;

	private GameObject _followTarget; // Object that we maintain an offset position from
	private GameObject _lastFollowTarget;
	private GameObject _viewTarget; // Object that we automatically centre our camera over

	private bool _bPostStartRun = false;

	public bool _bMouseControlActive = true;

	private float _snapZoomSpeed = 0.2f; // This determines how quickly we move towards our intended target location // 0 (never) -> 1 (instant)

	private Camera _cam;
	private Vector3 _pointOnGround;
	private Vector3 _pointOnGroundBelow;

	private float _camAltitude = 20.0f; // How high we are above the train level

	private float _trainAutoFollowDist = 50.0f;


	private float _distToGnd;

	private bool _bCameraToMiniMapPoint = false;

	private Vector3 _camShakeStoredRot;
	private float _camShakeIntensity_max;
	private float _camShakeIntensity_curr;

    //
    private float _maxDistanceFromTrainCenter;              // The maximum distance the camera can stay from the center point of the train.

	private bool _bCanMoveCamera; public bool BCanMoveCamera { get { return _bCanMoveCamera; } set { _bCanMoveCamera = value; } }
	private bool _bCameraRotationActive; public bool BCameraRotationActive { get { return _bCameraRotationActive; } set { _bCameraRotationActive = value; } }
	private bool _bCameraTargetSnappingActive; public bool BCameraTargetSnappingActive { get { return _bCameraTargetSnappingActive; } set { _bCameraTargetSnappingActive = value; } }
	private bool _bZoomActive; public bool BZoomActive { get { return _bZoomActive; } set { _bZoomActive = value; } }

    [SerializeField]
    [Range(0.0f, 1f)]
    private float _maxDistanceInTrainLength = 0.0f;
    //private bool _readjusting = false;

	void Start()
	{
		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();

		_cam = GetComponent<Camera>();

		_bCanMoveCamera = true;
		_bCameraRotationActive = true;
		_bCameraTargetSnappingActive = true;
		_bZoomActive = true;

		_distToGnd = 0.0f;

		//_initialPos = transform.position;
		//_initialRot = transform.rotation; // Reactivate if needed

		_zoomOffsetFromTarget = new Vector3(0.0f, 0.0f, 0.0f);
		_zoomOffsetFromTarget_low = new Vector3(0.0f, 0.0f, 0.0f);

		Quaternion tfr = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(-30, -180, 0));
		//Quaternion tfr = transform.rotation;
		_zoomOffsetFromTarget_high = _zoomOffsetFromTarget_low + (tfr * new Vector3(0.0f, _maxZoomOutDist, 0.0f)); // Example of a rotated vector in Unity

		_zoomPercent = 0.7f; // 1.0f
		_desiredZoomPercent = _zoomPercent;
		_zoomLevels = 5; // 4 until 2-6-18
		//ManageZoom(); // Set initial pos -- [Mike, 30-7-18]

		_posOffsetFromTarget = new Vector3(0.0f, _camAltitude, 0.0f);
		//_posOffsetFromTarget = new Vector3(0.0f, _worldScript.LocomotiveObjectRef.transform.position.y, 0.0f);
		//_posOffsetFromTarget_orig = _posOffsetFromTarget;

		//_extra_posOffsetFromTarget_orig = new Vector3(-7.5f, 0.0f, -7.5f);
		_extra_posOffsetFromTarget_orig = new Vector3(0.0f, -7.5f, 0.0f); // y here is not up/down
		_extra_posOffsetFromTarget = _extra_posOffsetFromTarget_orig;

		//_viewTarget = _worldScript.LocomotiveObjectRef;
		_viewTarget = null;

		_desiredOffset = Vector3.zero;

		_pointOnGround = Vector3.zero;
		_pointOnGroundBelow = Vector3.zero;

		_zoomSpeed = 0.05f;

		_camShakeStoredRot = transform.rotation.eulerAngles;
		_camShakeIntensity_max = 0.25f; // 0.5f
		_camShakeIntensity_curr = 0.0f;

        //_maxDistanceFromTrainCenter = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetFullTrainLength();
}

	void Update()
	{
		if (PauseMenu.isPaused) return;
	}

	public void UpdateActual() // Now called from WS to ensure desired call order
	{
		if (PauseMenu.isPaused) return;

		if (!_bPostStartRun)
		{
			//_viewTarget = _worldScript.LocomotiveObjectRef; // Only if we want to snap our view to the locomotive at the start of the game (if _followTarget is something else)
			_followTarget = _worldScript.LocomotiveObjectRef; // We will start the game looking at whatever is set here
			FollowOurTarget(); // Initial move to over our first _followTarget

			_bPostStartRun = true;
		}

		//if (_worldScript.Get_RandTime001_AvailableThisTurn())
		//{
		//	AddCameraShake(0.5f); // Testing camera shake
		//}

		//CameraShakePartA();

		//print(_worldScript.LocomotiveObjectRef.transform.position - transform.position);

		ManageVectorRotations();
		GetRayCastPointOnGround(); // Must be called before ChooseFollowTarget()
		GetRayCastPointOnGroundBelow();
		ChooseFollowTarget(); // Must be called before PanCamera() as we might want to override our result in that method
		//PanCamera(); // Must be called before CheckForChangedFollowTarget() as our follow target may be changed from within it
		//if (_bCanMoveCamera) PanCamera_T2(); // Must be called before CheckForChangedFollowTarget() as our follow target may be changed from within it
		if (_bCameraRotationActive) RotateCamera();
		CheckForChangedFollowTarget();
		//if (_bZoomActive) ManageZoom();
		ManageCamViewObj();
		if (_bCameraTargetSnappingActive) MoveToViewTarget();
        //ClampPosWithinMap(); // Offline from 25-7-18
        _maxDistanceFromTrainCenter = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetFullTrainLength() * _maxDistanceInTrainLength;
        
       
		//if (_bCameraToMiniMapPoint) MoveToMiniMap();
        //
        
        _lastFollowTarget = _followTarget;
		// End of update*/
	}

    public void LateUpdate()
    {
        ReAdjustCameraPos();
        FollowOurTarget();
    }

    public void TriggerMoveToMiniMap()
	{
		_bCameraToMiniMapPoint = true;
	}

	void MoveToMiniMap()
	{
		//int speed = 200;
		//Vector3 _direction = new Vector3(0, 0, 0);
		//Vector3 _position = new Vector3(0, 0, 0);
		//_position = GameObject.Find("MiniMap_WayPoint").transform.position;
		//_position.y = transform.position.y;
		//_direction = (_position - transform.position).normalized;
		//float _distance = Vector3.Distance(_position, transform.position);
		//_posOffsetFromTarget += _direction * panSpeed * Time.deltaTime;
		//if (_distance < 20) _bCameraToMiniMapPoint = false;
		_viewTarget = GameObject.Find("MiniMap_WayPoint(Clone)").gameObject;
		_bCameraToMiniMapPoint = false;
	}

	/// <summary>
	/// Make it so our position is clamped inside the map
	/// </summary>
	void ClampPosWithinMap()
	{
		GameObject loco = _worldScript.LocomotiveObjectRef;
		if (loco == null) return;

		float padding = 50.0f; // How close to the edge of the map can we be? /// 150.0f

		// x = top or bottom // z = left or right

		Vector3 ftPosXZ = BBBStatics.VXZ(_followTarget.transform.position);
		ftPosXZ.y = loco.transform.position.y;

		Vector3 testPos = _posOffsetFromTarget + ftPosXZ;

		if (testPos.z > (_worldScript.V3_MapTopLeft.z - padding))
		{
			//print("Out of map bounds - left");
			_posOffsetFromTarget.z = _worldScript.V3_MapTopLeft.z - ftPosXZ.z - padding;

		}

		if (testPos.z < (_worldScript.V3_MapTopRight.z + padding))
		{
			//print("Out of map bounds - right");
			_posOffsetFromTarget.z = _worldScript.V3_MapTopRight.z - ftPosXZ.z + padding;

		}

		if (testPos.x < (_worldScript.V3_MapBottomLeft.x + padding))
		{
			//print("Out of map bounds - bottom");
			_posOffsetFromTarget.x = _worldScript.V3_MapBottomLeft.x - ftPosXZ.x + padding;
		}

		if (testPos.x > (_worldScript.V3_MapTopLeft.x - padding))
		{
			//print("Out of map bounds - top");
			_posOffsetFromTarget.x = _worldScript.V3_MapTopLeft.x - ftPosXZ.x - padding;
		}
	}

	//private void FixedUpdate() // called multiple times per frame if the framerate is slow enough
	//{
	//	//ManageCamViewObj();
	//	if (_bCameraRotationActive) RotateCamera(); // Jittery under here - [16-4-18]
	//}

	//

	/// <summary>
	/// Keep the camera's offset consistent with its rotation [16-4-18]
	/// </summary>
	void ManageVectorRotations()
	{
		_extra_posOffsetFromTarget = transform.rotation * _extra_posOffsetFromTarget_orig; // Example of a rotated vector in Unity

		Quaternion tfr = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(-30, -180, 0));
		_zoomOffsetFromTarget_high = _zoomOffsetFromTarget_low + (tfr * new Vector3(0.0f, _maxZoomOutDist, 0.0f)); // Example of a rotated vector in Unity
	}

	void GetRayCastPointOnGround()
	{
		Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			_pointOnGround = hit.point;
			//Debug.DrawLine(hit.point, hit.point + new Vector3(0, 10, 0));
		}
	}

	void GetRayCastPointOnGroundBelow() // [Michael, 5-5-18]
	{
		_pointOnGroundBelow = Vector3.zero;

		Ray ray = new Ray { origin = transform.position, direction = Vector3.down };

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 1000.0f))
		{
			_pointOnGroundBelow = hit.point;
			//Debug.DrawLine(hit.point, hit.point + new Vector3(0, 10, 0));
		}

		_distToGnd = Vector3.Distance(_pointOnGroundBelow, transform.position);
		//print("distToGnd: " + _distToGnd);
	}

	public void PanCamera_T2(float vAxis, float hAxis)
	{
		// Mouse (at edge of screen)
		if (Input.mousePosition.x >= Screen.width - panBorder && _bMouseControlActive) hAxis = 1.0f; // Right
		if (Input.mousePosition.y >= Screen.height - panBorder && _bMouseControlActive) vAxis = 1.0f; // Up
		if (Input.mousePosition.y <= panBorder && _bMouseControlActive) vAxis = -1.0f; // Down
		if (Input.mousePosition.x <= panBorder && _bMouseControlActive) hAxis = -1.0f; // Left

		//

		// Not snapping to anything if we've moved the camera manually at all
		if (Input.GetAxisRaw("Vertical") != 0.0f || Input.GetAxisRaw("Horizontal") != 0.0f || vAxis != 0.0f || hAxis != 0.0f)
		{
			_viewTarget = null;
		}

		//

		if (vAxis != 0.0f || hAxis != 0.0f)
		{
			//float tempPanSpeed = panSpeed * Mathf.Clamp(1 - _zoomPercent, 0.4f, 1.0f);
			float tempPanSpeed = panSpeed * Mathf.Clamp(1 - _zoomPercent, 0.6f, 1.0f);

            ////? x = top or bottom // z = left or right

            //float vAxis_raw = Input.GetAxisRaw("Vertical");
            //float hAxis_raw = Input.GetAxisRaw("Horizontal");
            Vector3 tempOffset = _posOffsetFromTarget;
			if (vAxis != 0.0f)
			{
				Vector3 tff = transform.forward;
				tff = new Vector3(tff.x, 0.0f, tff.z);
				tempOffset += (tff * vAxis * BBBStatics.GetTimeScaleIndependentDelta() * tempPanSpeed * 1.75f); // 1.75f equalises the speed with the horizontal for some reason
			}

			if (hAxis != 0.0f)
			{
				Vector3 tfr = transform.right;
				tfr = new Vector3(tfr.x, 0.0f, tfr.z);
				tempOffset += (tfr * hAxis * Time.deltaTime * tempPanSpeed);
			}

            if (!BBBStatics.CheckPositionTooFarFromTrain(GetProjectedPosition(tempOffset), _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>(), true, _maxDistanceFromTrainCenter))
                _posOffsetFromTarget = tempOffset;

        }
	}

	void RotateCamera()
	{
		if (Input.GetAxis("RotateCam") != 0.0f)
		{
			transform.RotateAround(_pointOnGround, Vector3.up, Input.GetAxis("RotateCam") * Time.deltaTime * 100.0f); // _followTarget.transform.position
		}
	}

	public void ManageZoom(float mouseAxis)
	{
		if (mouseAxis != 0.0f)
		{
			// [Michael, 5-5-18]
			if (mouseAxis > 0.0f && _distToGnd > 15.0f) // Zooming down -- must be high enough
			{
				Zoom(mouseAxis);
			}
			else if (mouseAxis < 0.0f) // Zooming up
			{
				Zoom(mouseAxis);
			}
		}

		///

		// [Michael, 5-5-18]
		if (_distToGnd < 5.0f || _pointOnGroundBelow == Vector3.zero) // Close to the ground or through it
		{
			Zoom(-1.0f); // Zoom out a level
		}

		///

		if (Mathf.Abs(_zoomPercent - _desiredZoomPercent) > (_zoomSpeed * 1.01f)) // Make sure they're different enough
		{
			if (_zoomPercent > _desiredZoomPercent) _zoomPercent -= _zoomSpeed;
			else if (_zoomPercent < _desiredZoomPercent) _zoomPercent += _zoomSpeed;
		}

		_zoomOffsetFromTarget = Vector3.Lerp(_zoomOffsetFromTarget_high, _zoomOffsetFromTarget_low, _zoomPercent);
	}

	void Zoom(float input) // [Michael, 5-5-18]
	{
		float zoomLevelsFloat = _zoomLevels - 1;
		_desiredZoomPercent += input * (1.0f / zoomLevelsFloat);
		_desiredZoomPercent = Mathf.Clamp01(_desiredZoomPercent);
	}

	void ManageCamViewObj()
	{
		if (Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.Alpha5) 
			|| Input.GetKey(KeyCode.Alpha6) || Input.GetKey(KeyCode.Alpha7) || Input.GetKey(KeyCode.Alpha8) || Input.GetKey(KeyCode.Alpha9) || Input.GetKey(KeyCode.Alpha0))
		{
			if (_worldScript.LocomotiveObjectRef != null)
			{
				LocomotiveScript ls = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>();
				List<GameObject> carriages = ls.GetCarriagesInOrderAsGameObjects();

				if (Input.GetKey(KeyCode.Alpha1) && _worldScript.LocomotiveObjectRef != null)
					_viewTarget = _worldScript.LocomotiveObjectRef;
				else if (Input.GetKey(KeyCode.Alpha2) && carriages.Count >= 1) SetViewTarget(carriages[0]);
				else if (Input.GetKey(KeyCode.Alpha3) && carriages.Count >= 2) _viewTarget = carriages[1];
				else if (Input.GetKey(KeyCode.Alpha4) && carriages.Count >= 3) _viewTarget = carriages[2];
				else if (Input.GetKey(KeyCode.Alpha5) && carriages.Count >= 4) _viewTarget = carriages[3];
				else if (Input.GetKey(KeyCode.Alpha6) && carriages.Count >= 5) _viewTarget = carriages[4];
				else if (Input.GetKey(KeyCode.Alpha7) && carriages.Count >= 6) _viewTarget = carriages[5];
				else if (Input.GetKey(KeyCode.Alpha8) && carriages.Count >= 7) _viewTarget = carriages[6];
				else if (Input.GetKey(KeyCode.Alpha9) && carriages.Count >= 8) _viewTarget = carriages[7];
				else if (Input.GetKey(KeyCode.Alpha0) && carriages.Count >= 9) _viewTarget = carriages[8];
			}
		}
	}

    public void SetViewTarget(GameObject target )
    {
        _viewTarget = target;
    }
	/// <summary>
	/// Maintain an offset position from whichever carriage is closest to us (or whatever else is set as our _followTarget)
	/// </summary>
	void FollowOurTarget()
	{
		_desiredOffset = _posOffsetFromTarget + _zoomOffsetFromTarget + _extra_posOffsetFromTarget;
        Vector3 desiredPos = GetProjectedPosition(_posOffsetFromTarget);
        transform.position = desiredPos;
	}

    private Vector3 GetProjectedPosition(Vector3 offsetFromTarget)
    {
        // Need to refactor this
        Vector3 projectedOffset = offsetFromTarget + _zoomOffsetFromTarget + _extra_posOffsetFromTarget;

        Vector3 projectedPos = projectedOffset;

        // Have us follow our target
        if (_followTarget != null)
        {
            Vector3 ftPosXZ = BBBStatics.VXZ(_followTarget.transform.position);
            if (_worldScript.LocomotiveObjectRef != null)
                ftPosXZ.y = 20.0f;
            else
                ftPosXZ.y = 0.0f;

            projectedPos+= ftPosXZ;
        }

        return projectedPos;
    }

	void ChooseFollowTarget()
	{
        // If we're in range of the train, we will want to use the nearest carriage to us
        // If not, we'll just want to follow the WorldScriptHolder
        if (_worldScript.LocomotiveObjectRef == null) { return; } // We won't follow any carriages if there is no locomotive
        _followTarget = _worldScript.LocomotiveObjectRef; // Default -- we won't move if we're following this
        
		List<GameObject> list = new List<GameObject> { _worldScript.LocomotiveObjectRef };

		List<GameObject> cio = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().GetCarriagesInOrderAsGameObjects();

		for (int i = 0; i < cio.Count; ++i)
		{
			if (cio[i] != null)
			{
				list.Add(cio[i]);
			}
		}

		//if (cio.Count > 0)
		//	list.AddRange(cio);

		if (list.Count > 0)
		{
			GameObject go = BBBStatics.GetClosestGOFromListToVec(list, _pointOnGround, _trainAutoFollowDist);
			if (go != null)
			{
				_followTarget = go;
			}
		}
	}

	void CheckForChangedFollowTarget()
	{
		if (_lastFollowTarget != null && _followTarget != null && _lastFollowTarget != _followTarget)
		{
			// Move _posOffsetFromTarget to compensate for the switch of follow target

			Vector3 offset = _lastFollowTarget.transform.position - _followTarget.transform.position;
			offset = new Vector3(offset.x, 0.0f, offset.z);

			_posOffsetFromTarget += offset;

			//

			_lastFollowTarget = _followTarget;
		}
	}

	void MoveToViewTarget()
	{
		// While this is running, move _posOffsetFromTarget slightly towards _viewTarget (offset _viewTarget's pos from _followTarget.transform.position) at _snapZoomSpeed
		// Keep tracking the _viewTarget until it is nulled (such as when we manually move the camera)
		if (_viewTarget != null && _followTarget != null && _desiredOffset != Vector3.zero)
		{
			Vector3 destPos = _viewTarget.transform.position - _followTarget.transform.position;

			float pointPercentage = (1.0f - _snapZoomSpeed); // This determines how quickly we move towards our intended target location
			Vector3 p = _posOffsetFromTarget * pointPercentage + (1 - pointPercentage) * destPos; // * Time.deltaTime

			_posOffsetFromTarget = new Vector3(p.x, _posOffsetFromTarget.y, p.z);
		}
	}

	public void AddCameraShake(float power)
	{
		_camShakeIntensity_curr += power;
	}

	private void CameraShakePartA() // Call at beginning of update
	{
		transform.rotation = Quaternion.Euler(_camShakeStoredRot); // Restore original rotation
	}

	private void CameraShakePartB() // Call at end of update
	{
		_camShakeStoredRot = transform.rotation.eulerAngles;

		_camShakeIntensity_curr -= Time.deltaTime;
		_camShakeIntensity_curr = Mathf.Clamp(_camShakeIntensity_curr, 0.0f, _camShakeIntensity_max);

		double power = _camShakeIntensity_curr;

		Vector3 shakeRot = transform.rotation.eulerAngles;
		shakeRot.x += (float)BBBStatics.RandDbl(-power, power);
		shakeRot.y += (float)BBBStatics.RandDbl(-power, power);
		shakeRot.z += (float)BBBStatics.RandDbl(-power, power);

		transform.rotation = Quaternion.Euler(shakeRot);
	}

	public Vector3 PointOnGround
	{
		get { return _pointOnGround; }
	}

    /// <summary>
    /// Used to pull the camera to the center of the train when a carriage is destroyed.
    /// </summary>
    public void ReAdjustCameraPos()
    {
        if (BBBStatics.CheckPositionTooFarFromTrain(GetProjectedPosition(_posOffsetFromTarget), _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>(), true, _maxDistanceFromTrainCenter))
        {
            _posOffsetFromTarget = Vector3.Lerp(_posOffsetFromTarget, new Vector3(0, _posOffsetFromTarget.y, 0), 0.2f);
        }
    }
}