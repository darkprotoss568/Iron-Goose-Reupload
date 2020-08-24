using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum UpgradeLevel
{
	Level1,
	Level2,
	Level3
}

public abstract class TurretScriptParent : Module
{

	[Header("Turret Behavior Parameters")]
	[Header("General Behavior")]
    [SerializeField]
    protected float _detectionRange = 250;              // Detection Radius
    [SerializeField]
	protected float _firingRange;                       // Firing Range
    [SerializeField]
    protected bool _bCanShootGround;                    // True if the turret can shoot at ground targets
    [SerializeField]
    protected bool _bCanShootAir;                       // True if the turret can shoot at aerial target
	protected GameObject _currTarget;                   // Current Target (As GameObject)
    protected TrainGameObjScript _target;               // Target the turret is currently locked on
    protected TrainGameObjScript _forcedTarget;         // For when we want to override the target - such as when the player is controlling the turret's target
    
    protected string _attachedTo;
    //protected List<GameObject> attachments;
    protected bool _isDummyTurret = false;
    protected float _turretHeight = 6.5f;               // Height of the turret -- for cons drones  
    [SerializeField]
	protected GameObject _rangeProjector;
    
    [Header("Shot Timing Management")]
	[SerializeField]
	protected float _shotDelay;                         // Minimum time between each shot
	protected float _currShotTimer;    

    [Header("Turret Movement Management")]
	protected GameObject _verticalRotatorObj;           // Part that rotates vertically
	protected GameObject _horizontalRotatorObj;         // Part that rotates horizontally
	[SerializeField]
	protected float _rotSpeed;// Rotation speed of the turret
	[SerializeField]
	protected float _maxPitchAngle = 90.0f;// maximum pitch angle of the turret barrel
	[SerializeField]
	protected float _minPitchAngle = -90.0f;
	public float _maxRotAngle = 360.0f; // Max rotation away from forward-facing // Set to 360 to have full circular rotation // AI will only target enemies within this angle

	[Header("Recoil Movement Management")]
	protected List<GameObject> _recoilObjs = new List<GameObject>();        // Part that gets pushed back due to recoil on shot
	protected List<Vector3> _originalRecoilPositions = new List<Vector3>(); // Initial position of the _recoilObj before recoiling
	[SerializeField]
	protected float _recoilDistance = 0.45f;                                           // The maximum distance the _recoilObj is pushed back
	protected bool _recoilDone = true;                                      // True if the _recoilObj has already recoiled within the cooldown time
	protected bool __recoilObjPushedBack = false;                           // True if the recoildObj has been pushed back during recoil
	protected List<GameObject> _hardpointObjs = new List<GameObject>();     // The turret muzzle points
	protected int _currHardpoint = 0;
     // How quickly a homing bullet turns to follow its target //TOMOVE (actually, we may not need this)	

	[Header("Sound Effects")]
	[SerializeField]
	protected AudioClip _fireSound;                                                 // Sound clip for the firing SFX	

	[Header("Muzzle Flash FX Control")]
	public GameObject _muzzleFlashFX;
	public float _muzzleFlashTime;

	[Header("Mike")]
	protected bool _bCanFire;       // What does this do?
    private float _timeUntilAutoRetarget = 0.0f;
    public int _targetingType = 1; // 1 == Nearest // 2 == Randomise    


    protected Vector3 _nextImpactPoint;
	public override void Start()
	{
		base.Start();

		_horizontalRotatorObj = transform.Find("turret_y").gameObject;
		_verticalRotatorObj = _horizontalRotatorObj.transform.Find("turret_x").gameObject;

		for (int i = 0; i < 20; ++i) // Max number of recoil objects
		{
			string name = "recoil" + i.ToString();
			if (i == 0) name = "recoil";
			Transform t = _verticalRotatorObj.transform.Find(name);
			if (t != null && t.gameObject != null)
			{
				_recoilObjs.Add(t.gameObject);

				_originalRecoilPositions.Add(t.localPosition);

				Transform hp = (t.transform.Find("hardpoint"));
				if (hp != null) _hardpointObjs.Add(hp.gameObject);
			}
			//else break;
		}
        
          CreateRangeProjector();		 

		//_bCanFire = true; 

		AutoChooseTarget();		

		if (_team == Team.Friendly)
		{
			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._turretsBuiltCount++;
		}

		  
		// End of Start()
	}

	// Update is called once per frame
	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();
        
        if (_target != null)
        {
            if (!CheckTargetStillInRange() || !IsObjWithinMaxTargetingAngle(_target.gameObject))
            {
                AutoChooseTarget();
            }
            else
            {
                if (CheckCanHitTarget())
                {
                    Fire_Simplified();
                }
            }
        }
        else
        {
            AutoChooseTarget();
        }

		if (_currShotTimer < _shotDelay)
		{
			Recoil_BBB();
		}

		ManageProjectorState();
	}

    public virtual void FixedUpdate()
    {
        if (_target != null)
        {
            AimAtTarget_BBB_T2(_target.CommSocketObj.transform.position);
        }
    }

	protected virtual void AimAtTarget_BBB_T2(Vector3 target) // [Mike, 5-6-18]
	{
		// New Y axis rotation method -- avoid the 'slowness' when aim is close but not quite there of the other AimAtTarget_BBB method
		float desired_angle_y = BBBStatics.GetAngleOnPlaneToFrom(target, transform.position);
		float curr_angle_y = _horizontalRotatorObj.transform.rotation.eulerAngles.y;
		float diff_angle_y = BBBStatics.AngleDiff(curr_angle_y, desired_angle_y);

		float addition = -(_rotSpeed * Time.deltaTime); // _rotSpeed * Time.deltaTime // -2.0f
		if (diff_angle_y > 0.0f) addition *= -1;

		float slowDownAngle = 2.0f;
		if (Mathf.Abs(diff_angle_y) < slowDownAngle) addition *= BBBStatics.Map(Mathf.Abs(diff_angle_y), slowDownAngle, 0.0f, 1.0f, 0.0f, true);

		Vector3 y = _horizontalRotatorObj.transform.rotation.eulerAngles;
        
		// TODO: Limit the rotation of the y axis so it can't go beyond the targeting angle limit

		_horizontalRotatorObj.transform.rotation = Quaternion.Euler(new Vector3(y.x, y.y + addition, y.z));

		Vector3 toXDirection = (_verticalRotatorObj.transform.position - target);

		Quaternion rotateXTo = Quaternion.LookRotation(toXDirection);
		Vector3 rotateXTo_e = rotateXTo.eulerAngles;

		_maxPitchAngle = Mathf.Clamp(_maxPitchAngle, 0.0f, 90.0f);
		_minPitchAngle = Mathf.Clamp(_minPitchAngle, -90.0f, 0.0f);

		if (rotateXTo_e.x > 90 && rotateXTo_e.x < (360 - -_minPitchAngle)) rotateXTo_e.x = (360 - -_minPitchAngle);
		if (rotateXTo_e.x > 0 && rotateXTo_e.x < 90 && rotateXTo_e.x > _maxPitchAngle) rotateXTo_e.x = _maxPitchAngle;

		rotateXTo = Quaternion.Euler(rotateXTo_e);

		Vector3 XDirFinal = Quaternion.Lerp(_verticalRotatorObj.transform.rotation, rotateXTo, _rotSpeed * Time.deltaTime).eulerAngles;

		XDirFinal = new Vector3(XDirFinal.x, _horizontalRotatorObj.transform.eulerAngles.y, 0.0f);

		_verticalRotatorObj.transform.eulerAngles = XDirFinal;
	}

	/// <summary>
	/// Pick the closest target to attack
	/// Or randomly pick a target within detection range // Move over to AI component
	/// </summary>
	protected virtual void AutoChooseTarget()
	{
		_target = null;

		//int _targetingType = 1;// Random.Range(1,3); // 1 or 2
		if (AttachedTo != null)
		{
			if (_targetingType == 1) _target = BBBStatics.ChooseClosestTarget(transform.position, _detectionRange, _worldScript, _team, this, AttachedTo);
		}
		else
		{
			if (_targetingType == 1) _target = BBBStatics.ChooseClosestTarget(transform.position, _detectionRange, _worldScript, _team, this);
		}
	}



    /// <summary>
    /// Check if the target is still within the detection range of the turret then automatically switch to a new target if not
    /// (Could be changed to make the code more uniform by moving AutoChooseTarget or the method name can be changed to make it more readable)
    /// </summary>
    protected bool CheckTargetStillInRange()
    {
        bool result = false;
        if (BBBStatics.CheckGameObjectColliderWithinRange(transform.position, _firingRange, _target.gameObject))
        {
            result = true;
        }

        return result;
        
    }

	protected void CheckTargetWithinMaxRotAngle()
	{
		if (_target != null)
		{
			if (!IsObjWithinMaxTargetingAngle(_target.gameObject))
			{
				AutoChooseTarget();
			}
		}
	}

	public bool IsObjWithinMaxTargetingAngle(GameObject go)
	{

		if (_maxRotAngle >= 360) return true;

		if (go != null)
		{
			float angToTarget = BBBStatics.GetAngleOnPlaneToFrom(go.transform.position, transform.position);
			angToTarget = BBBStatics.WrapAroundT3(angToTarget - 180, -180, 180);
			angToTarget = BBBStatics.Map(angToTarget, -180, 180, 0, 360, true);

			float ourAng = transform.rotation.eulerAngles.y;

			float diff = BBBStatics.AngleDiff(angToTarget, ourAng);

			if (Mathf.Abs(diff) <= (_maxRotAngle / 2))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Check if the turret is lining up with the target
	/// </summary>
	/// <param name="rtm"></param>
	/// <returns></returns>
	protected virtual bool CheckCanHitTarget()
	{
        if (!BBBStatics.CheckGameObjectColliderWithinRange(_verticalRotatorObj.transform.position, _firingRange, _target.gameObject))
            return false;
        //Vector3 dir = _hardpointObjs[_currHardpoint].transform.TransformDirection(Vector3.forward);

        Vector3 dir = -_verticalRotatorObj.transform.TransformDirection(Vector3.forward);
        bool result = false;
        RaycastHit[] hits;

        Vector3 start = _verticalRotatorObj.transform.position;
        //Vector3 end = start + (dir.normalized);
        Ray ray = new Ray(start, dir.normalized);
        hits = Physics.RaycastAll(ray, _firingRange * 3);
        int hitsLength = hits.Length;
        if (hitsLength > 0)
        {
            for (int i = 0; i < hitsLength; i++)
            {
                if (hits[i].collider.gameObject.GetComponentInChildren<TrainGameObjScript>() == _target)
                {
                    result = true;
                    break;
                }
            }
        }

        return result;
    }

    

	protected abstract void Fire_Simplified();


	/// <summary>
	/// Solution 1 for animation recoil effect
	/// </summary>
	protected void Recoil()
	{
		// Acquire the current localPosition of the _recoilObj
		Vector3 _recoilObjPos = _recoilObjs[_currHardpoint].transform.localPosition;
		// Acquire the position the _recoilObj is pushed back to
		// Can be made a public class attribute to improve processing speed at the cost of memory
		Vector3 recoilPos = new Vector3(0, 0, _originalRecoilPositions[_currHardpoint].z - _recoilDistance);

		// Check if the recoil has been done at least once
		if (_recoilDone == false)
		{
			// Check if the _recoilObj is being pushed back or pushed forward
			if (!__recoilObjPushedBack)
			{
				_recoilObjPos = Vector3.Slerp(_recoilObjPos, recoilPos, 0.9f);
				if (_recoilObjPos.z <= recoilPos.z)
					__recoilObjPushedBack = true;
			}
			else
			{
				_recoilObjPos = Vector3.Slerp(_recoilObjPos, _originalRecoilPositions[_currHardpoint], 0.7f);
				if (_recoilObjPos.z >= _originalRecoilPositions[_currHardpoint].z)
				{
					_recoilObjPos = _originalRecoilPositions[_currHardpoint];
					_recoilDone = true;
					__recoilObjPushedBack = false;
				}
			}
		}

		_recoilObjs[_currHardpoint].transform.localPosition = _recoilObjPos;
	}

	/// <summary>
	///  Solution 2 for animation recoil effect
	/// </summary>
	protected void Recoil_BBB()
	{
		// Acquire the position the _recoilObj is pushed back to
		// Can be made a public class attribute to improve processing speed at the cost of memory
		Vector3 recoilBackPos = new Vector3(_originalRecoilPositions[_currHardpoint].x, _originalRecoilPositions[_currHardpoint].y, _originalRecoilPositions[_currHardpoint].z + _recoilDistance);

		// Change the local position of the _recoilObject in relation to _timeSinceLastFired and _currShotDelay
		float v = BBBStatics.Map(_currShotTimer, 0.0f, _shotDelay, 0.0f, 1.0f, true);

		_recoilObjs[_currHardpoint].transform.localPosition = Vector3.Lerp(recoilBackPos, _originalRecoilPositions[_currHardpoint], v);
	}

	public float TurretHeight
	{
		get { return _turretHeight; }
	}

	public TrainGameObjScript ForcedTarget
	{
		get { return _forcedTarget; }
		set { _forcedTarget = value; }
	}

	public List<GameObject> HardpointObjs
	{
		get { return _hardpointObjs; }
	}

	public int CurrHardpoint
	{
		get { return _currHardpoint; }
	}

	public TrainGameObjScript Target
	{
		get { return _target; }
		set { _target = value; }
	}

	public bool IsDummy
	{
		get { return _isDummyTurret; }
		set { _isDummyTurret = value; }
	}

	public string AttachedTo
	{
		get { return _attachedTo; }
		set { _attachedTo = value; }
	}

	//public void OnGUI() // For Debug Labels
	//{
	//	var restoreColor = GUI.color; GUI.color = Color.green; // red

	//	if (_testHitObj != null)
	//	{
	//		UnityEditor.Handles.Label(transform.position, "_testHitObj.name: " + _testHitObj.name);
	//	}

	//	//if (BehindSocket != null)
	//	//{
	//	//	GUI.color = Color.cyan;
	//	//	UnityEditor.Handles.Label(transform.position + new Vector3(1, 1, 1), "BehindSocket != null");
	//	//}

	//	GUI.color = restoreColor;
	//}



	protected Vector3 GetImpactPoint(Vector3 dir)
	{
		// TODO: Use Layer Mask instead
		RaycastHit[] hits;
		Ray ray = new Ray(_hardpointObjs[CurrHardpoint].transform.position, dir);

		hits = Physics.RaycastAll(ray, _firingRange * 3);

		if (hits.Length != 0)
		{
			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].collider.gameObject.GetComponent<TrainGameObjScript>() == _target)
					return hits[i].point;
			}
		}

		return new Vector3();
	}
	 
	 public void CreateRangeProjector()
	 {
		  if (_rangeProjector != null)
		  {
				_rangeProjector = Instantiate(_rangeProjector, transform.position, transform.rotation, transform);
				_rangeProjector.transform.localPosition = new Vector3(0, 1, 0);
                _rangeProjector.GetComponent<ConeRangeProjectorScript>().AdjustCone(_firingRange, _maxRotAngle);
				_rangeProjector.SetActive(false);
		  }
	 }

	 public void ManageProjectorState()
	 {
		  if (_rangeProjector != null)
		  {
				if (GameObject.ReferenceEquals(_worldScript.HUDScript.MouseOverObj, gameObject))
				{
					 if (_rangeProjector.activeInHierarchy == false)
						  _rangeProjector.SetActive(true);
				}
				else
				{ 
					 if (_rangeProjector.activeInHierarchy == true)
						  _rangeProjector.SetActive(false);
				}
		  }
	 }

	 protected LayerMask GetShotLayerMask()
	{
		return new LayerMask();
	}

	protected void StoreImpactPoint()
	{

	}


	public float FiringRange
	{
		get
		{
			return _firingRange;
		}
	}

    public bool TurretCanShootGround
    {
        get
        {
            return _bCanShootGround;
        }
    }

    public bool TurretCanShootAir
    {
        get
        {
            return _bCanShootAir;
        }
    }

    public float MaxAimAngle
    {
        get
        {
            return _maxRotAngle;
        }
    }
    public override void BeginDestroy(bool bRunExplosion, bool bSpawnChunks)
	{
		if (_team == Team.Friendly)
		{
			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._turretsLostCount++;
		}

		base.BeginDestroy(bRunExplosion, bSpawnChunks);
	}
}
