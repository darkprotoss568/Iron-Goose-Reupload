using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBasedTurretScript : TurretScriptParent {

    [Header("Projectile")]
    private int _shotsFired = 0;
    [SerializeField]
    private int _shotsPerVolley = 1;    
    [SerializeField]
    private float _volleyDelay;
    private float _currVolleyTimer;

    [SerializeField]
    private GameObject _projectilePrefab;
    // Use this for initialization
    public override void Start ()
    { 
        base.Start();

        _currShotTimer = _shotDelay;
    }
	
	// Update is called once per frame
	public override void Update () {
        base.Update();

        _currShotTimer += Time.deltaTime;
        if (_volleyDelay > 0)
        {
            if (_shotsFired >= _shotsPerVolley)
            {
                _currVolleyTimer += Time.deltaTime;
                if (_currVolleyTimer >= _volleyDelay)
                {
                    _currVolleyTimer = 0;
                    _shotsFired = 0;
                }
            }
        }
        else
        {
            _shotsFired = 0;
        }

        if (_target == null)
        {
            ForceReload();
        }
	}

    protected override void Fire_Simplified()
    {
       // Debug.Log("Fire the god damn cannons");
        if (_currShotTimer >= _shotDelay && _shotsFired < _shotsPerVolley && _projectilePrefab != null)
        {

            LaunchBullet();
            _currShotTimer = 0;
            _shotsFired += 1;

            if (_muzzleFlashFX != null)
            {
                GameObject muzzleFlash = Instantiate(_muzzleFlashFX, _hardpointObjs[_currHardpoint].transform.position, Quaternion.Euler(new Vector3(0, _horizontalRotatorObj.transform.eulerAngles.y + 90, _verticalRotatorObj.transform.eulerAngles.x))) as GameObject;
                muzzleFlash.transform.parent = _hardpointObjs[_currHardpoint].transform;
                muzzleFlash.GetComponent<MuzzleFlashScript>().ShrinkTime = _muzzleFlashTime;
            }
            ++_currHardpoint;
            if (_currHardpoint >= _hardpointObjs.Count)
            {
                _currHardpoint = 0;
            }
        }
    }

    private void ForceReload()
    {
        if (_shotsFired < _shotsPerVolley)
        {
            _currVolleyTimer = _volleyDelay * 0.8f;
        }

        _shotsFired = _shotsPerVolley;
    }

    private void LaunchBullet()
    {
        // Vector3 forwardBulletDir = ;
        Vector3 forwardBulletDir = (_target.CommSocketObj.transform.position - _hardpointObjs[CurrHardpoint].transform.position).normalized;

        if (_target != null)
        {
            Vector3 targetPoint = GetImpactPoint(forwardBulletDir);
            Quaternion bulletRotation = Quaternion.Euler(new Vector3(0, _horizontalRotatorObj.transform.eulerAngles.y + 90, _verticalRotatorObj.transform.eulerAngles.x));
            GameObject projectile = Instantiate(_projectilePrefab, _hardpointObjs[CurrHardpoint].transform.position, bulletRotation);
            projectile.GetComponent<ProjectileScript>().InitializeProjectileStats(forwardBulletDir, targetPoint, _target);
        }
    }

    public float FiringRate
    {
        get
        {
            if (ShotsPerVolley > 1)
                return _volleyDelay;
            else
                return _shotDelay;
        }
    }

    public int ShotsPerVolley
    {
        get
        {
            return _shotsPerVolley;
        }

    }

    public int BaseDamage
    {
        get
        {
            return _projectilePrefab.GetComponent<ProjectileScript>().BaseDamage;
        }
    }

    public OrdnanceType DamageType
    {
        get
        {
            return _projectilePrefab.GetComponent<ProjectileScript>().DamageType;
        }
    }

    public bool BDealsSplashDamage
    {
        get
        {
            return _projectilePrefab.GetComponent<ProjectileScript>().BDealsSplashDamage;
        }
    }

    public float SplashRadius
    {
        get
        {
            return _projectilePrefab.GetComponent<ProjectileScript>().SplashRadius;
        }
    }

    public int BonusDamage
    {
        get
        {
            return _projectilePrefab.GetComponent<ProjectileScript>().BonusDamage;
        }
    }
}
