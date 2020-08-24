using UnityEngine;

public class BeamTurretScript : TurretScriptParent
{
    [Header("Beam Management")]
    [SerializeField]
    private GameObject _beam;
    private bool _beamActive = false;
    private LineRenderer _currentBeamRenderer;
    [SerializeField]
    private float _delayBetweenBeams;
    private float _beamDelayTimer;
    
    [Header("Beam Damage Management")]
    [SerializeField]
    private int _damage;                                // Damage applied every time shot delay ends 
    [SerializeField]
    private int _bonusDamage;
    [SerializeField]
    private bool _continuous;                           // Whether the turret fires in bursts or continuously
    [SerializeField]
    private float _maxBeamDuration;                     // The maximum amount of time the beam can stay active
    private float _beamDuration;                        // The amount of time has passed since the beam was activated

    [SerializeField]
    private AudioClip _beamImpactSFX;
    [SerializeField]
    private GameObject _beamImpactParticle;
    private float _timeSinceDamageLastApplied;
    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        _muzzleFlashTime = _shotDelay;
        _beamDelayTimer = _delayBetweenBeams;
	}
	
	// Update is called once per frame
	public override void Update ()
    {
        if (_beamActive)
        {
            _beamDuration += Time.deltaTime;
            _currShotTimer += Time.deltaTime;

            if (_target == null || !CheckTargetStillInRange() || !IsObjWithinMaxTargetingAngle(_target.gameObject))
            {
                DeactivateBeam();
            }
        }
        else
        {
            _beamDelayTimer += Time.deltaTime;
        }

        base.Update();
	}

    protected override void Fire_Simplified()
    {
        if (_beamActive)
        {
            MaintainBeam();
        }
        else
        {
            if (_beamDelayTimer >= _delayBetweenBeams)
            {
                ActivateBeam();
                MaintainBeam();
            }
        }

    }
    private void ActivateBeam()
    {
        // TODO: Possibly make multiple-muzzle laser turrets with more complex firing mechanisms?
        _beamActive = true;
        _beamDelayTimer = 0;
        // Create a new beam 
        Transform muzzleTransform = _hardpointObjs[_currHardpoint].transform;
        _currentBeamRenderer = Instantiate(_beam, muzzleTransform.position, muzzleTransform.transform.rotation, _hardpointObjs[_currHardpoint].transform).GetComponent<LineRenderer>();
        _currShotTimer = _shotDelay;
    }

    private void DeactivateBeam()
    {
        _beamActive = false;
        
        _beamDuration = 0;
        _currShotTimer = 0;

        // TODO: Set the beam to kill itself here before setting _currentBeam to null to disconnect it from the turret.
        _currentBeamRenderer.gameObject.GetComponent<BeamScript>().KillBeam();
        _currentBeamRenderer = null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_currentBeamRenderer != null)
        {
            Destroy(_currentBeamRenderer.gameObject);
        }
    }

    private void MaintainBeam()
    {
        if (_target != null)
        {
            Vector3 impactPoint = GetImpactPoint(_verticalRotatorObj.transform.TransformDirection(-Vector3.forward));
            Vector3 localImpactPoint = _hardpointObjs[_currHardpoint].transform.InverseTransformPoint(impactPoint);
            _currentBeamRenderer.SetPosition(2, localImpactPoint);

            if (_muzzleFlashFX != null)
            {
                GameObject muzzleFlash = Instantiate(_muzzleFlashFX, _hardpointObjs[_currHardpoint].transform.position, Quaternion.Euler(new Vector3(0, _horizontalRotatorObj.transform.eulerAngles.y + 90, _verticalRotatorObj.transform.eulerAngles.x))) as GameObject;
                muzzleFlash.transform.parent = _hardpointObjs[_currHardpoint].transform;
                muzzleFlash.GetComponent<MuzzleFlashScript>().ShrinkTime = _muzzleFlashTime;
            }

            //Debug.Log(_currShotTimer);
            if (_currShotTimer >= _shotDelay)
            {
                _target.Damage_Additive(OrdnanceType.Ballistics, _damage, _bonusDamage, true);
                _currShotTimer = 0f;
                GameObject newSFX = Instantiate(_beamImpactParticle, impactPoint, Quaternion.identity);
                AudioSource audioSource = newSFX.AddComponent<AudioSource>();
                audioSource.PlayOneShot(_beamImpactSFX, 0.25f);

            }
            
            if (!_continuous && _beamDuration >= _maxBeamDuration)
            {
                DeactivateBeam();
            }
        }
        else
        {
            DeactivateBeam();
        }
    }
}
