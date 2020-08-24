using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    [Header("General Parameters")]
    [SerializeField]
    private Team _team;                           // The team this projectile belongs to
    [SerializeField]
    private OrdnanceType _ordnanceType;           // Type of ordnance 
    [SerializeField]
    private float _maxTravelDistance;             // Firing Range 
    [SerializeField]
    private int _shotDamage;                      // Damage dealt per shot 
    [SerializeField]
    private int _bonusDamage;                     // Bonus amount to apply to final damage if the ordnance type is effective against armor type
    [SerializeField]
    private float _bulletSpeed;                   // How fast the bullet travels (-1 = instant)
    private Vector3 _bulletDir;                   // The direction the projectile is moving in
    [SerializeField]
    private bool _bHasTrail;                      // True if the projectile leaves behind a trail
    //private bool _bUpgrade1Acquired;           // Whether the turret's projectile has Upgrade 1 or not
    //private bool _bUpgrade2Aquired;            // Whether the turret's projectile has Upgrade 2 or not
   
    [Header("Effects")]
    [SerializeField]
    private AudioClip[] _shotImpactSound = new AudioClip[3];           // Array Containing sound effects to play when the projectile hits the target impact point
    [SerializeField]
    private GameObject[] _shotImpactFX  = new GameObject[3];          // Visual effects to create when the projectile hits the target impact point

    [Header("SplashDamage")]
    [SerializeField]
    private bool _bDealsSplashDamage;                               // True if the projectile deals damage to units other than the intended target
    [SerializeField]
    private float _splashRadius;                                    // The radius of the sphere in which splash damage is applied.
    [SerializeField]
    private int _splashDamage;                                      // (Currently unused due to design) the damage dealt to targets other than the intended target
    [SerializeField]
    private bool _bFriendlyDamageEnabled;

    [Header("Homing Behavior")]
    [SerializeField]
    private bool _bFollowTarget;                  // True if the projectile follows the target
    [SerializeField]
    private float _homingBulletTurnRate;          // How quickly a homing bullet turns to follow its target (we may actually not need this) 

    private Vector3 _targetPoint;                 // The target impact point where the projectile always tries to get to
    private Vector3 _targetLocalPoint;            // The target impact point in the local space of the target. Used for homing projectiles
    private TrainGameObjScript _target;           // The main target of the projectile to perform damage calculation on.
    // Use this for initialization
    void Start ()
    {
	}

    /// <summary>
    /// Initialize the basic parameters of the projectile
    /// </summary>
    /// <param name="normalizedDirection">The initial direction of the projectile's movement</param>
    /// <param name="targetPoint">The intended impact point</param>
    /// <param name="target">The main target of the projectile</param>
    public void InitializeProjectileStats(Vector3 normalizedDirection, Vector3 targetPoint, TrainGameObjScript target)//, bool bUpgrade1Acquired, bool bUpgrade2Acquired)
    {
        //gameObject.transform.rotation = initialRotation;
        _bulletDir = normalizedDirection;
        _targetPoint = targetPoint;
        _target = target;
        _targetLocalPoint = _target.gameObject.transform.InverseTransformPoint(_targetPoint);
    }

    // Update is called once per frame
    void Update ()
    {
        UpdateMovement();
    }

    /// <summary>
    /// Mostly checking whether the projectile has reached the target impact point at the end of each tick
    /// </summary>
    void LateUpdate()
    {
        if (CheckIfProjectileHasReachedTargetPoint())
        {
            CreateImpactEffects();

            if (_target != null)
            {
                // Perform damage calculation on the target if it has not been destroyed yet
                _target.Damage_Additive(_ordnanceType, _shotDamage, _bonusDamage, true);
            }

            // Perform splash damage calculation if _bDealsSplashDamage is true
            if (_bDealsSplashDamage)
            {
                List<TrainGameObjScript> allAffectedTGO = 
                    BBBStatics.GetAllTargetsInBlastRadius(gameObject.transform.position, 
                                                            _splashRadius, 
                                                            _team, 
                                                            _bFriendlyDamageEnabled, 
                                                            _target);

                int count = allAffectedTGO.Count;
                for (int i = 0; i < count; i++)
                {
                    allAffectedTGO[i].Damage_Additive(_ordnanceType, _shotDamage, 0, false);
                }
            }

            // Detach the trail object from the projectile so that it can remain on the scene longer.
            Transform trailObj = gameObject.transform.Find("Trail");
            if (trailObj != null)
            {
                trailObj.parent = null;
                // Disable particle system's looping if the trail uses looping particles
                ParticleSystem ps = trailObj.gameObject.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.loop = false;
                }
            }
            // Destroy the projectile at the end
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Returns true if the projectile has reached the intended impact point
    /// </summary>
    /// <returns></returns>
    private bool CheckIfProjectileHasReachedTargetPoint()
    {
        if (!_bFollowTarget)
        {
            // If the projectile does not have homing effect, check if the projectile's position is the same as that of the intended impact point
            if (gameObject.transform.position == _targetPoint)
                return true;
            else
                return false;
        }
        else
        {
            if (_target != null)
            {
                // If the projectile has homing effect, check if the projectile's position is the same as the intended impact point on the target.
                if (gameObject.transform.position == _target.transform.TransformPoint(_targetLocalPoint))
                    return true;
                else
                    return false;
            }
            else
            {
                // Self-Destruct if the target is gone. TODO: Possibly switch target or continue heading to the previously known position of the target
                return true;
            }
        }
    }

    /// <summary>
    /// Update the position of the projectile
    /// </summary>
    private void UpdateMovement()
    {
        Vector3 tempTargetPoint = Vector3.zero;
        if (_bFollowTarget)
        {
            if (_target != null)
            {
                tempTargetPoint = _target.transform.TransformPoint(_targetLocalPoint);
                _bulletDir = (_target.transform.TransformPoint(_targetLocalPoint) - gameObject.transform.position).normalized;
            }
        } else
        {
            tempTargetPoint = _targetPoint;
        }

        if (_bulletSpeed > 0)
        {
            if (Vector3.Distance(gameObject.transform.position, tempTargetPoint) > _bulletSpeed * Time.deltaTime)
            {
                gameObject.transform.position += _bulletDir * _bulletSpeed * Time.deltaTime;
            }
            else
                gameObject.transform.position = tempTargetPoint;
        }
        else
        {
            gameObject.transform.position = tempTargetPoint;
        }
    }

    /// <summary>
    /// Spawn the effect of the impact onto the scene
    /// </summary>
    private void CreateImpactEffects()
    {
        // ID used for each impact case. 0 = default (against terrains/ obstacles), 1 = against light armor, 2 = against heavy armor.
        int FX_ID = 0;

        if (_target != null)
        {
            // Effects for Light or Heavy Armor types
            if (_target._armorType == ArmorType.Light)
            {
                FX_ID = 1;
            }
            else if (_target._armorType == ArmorType.Heavy)
            {
                FX_ID = 2;
            }
        }
        else
        {
            // default effect
            FX_ID = 0;
        }
        if (_shotImpactFX[FX_ID] != null && _shotImpactSound[FX_ID] != null)
        {
            // Create visual particle effects
            GameObject impact;
            if (_target != null)
            {
                impact = Instantiate(_shotImpactFX[FX_ID], _target.transform.TransformPoint(_targetLocalPoint), Quaternion.identity);
            }
            else
            {
                // If the projectile has homing effect, the effect would be spawned where the projectile is. If not, the effects would be spawned at the target impact point. This may change depending how the homing projectile works is changed
                if (_bFollowTarget)
                {
                    impact = Instantiate(_shotImpactFX[FX_ID], transform.position, Quaternion.identity);
                } else
                    impact = Instantiate(_shotImpactFX[FX_ID], _targetPoint, Quaternion.identity);
            }

            // Create the sound effect by adding a sound component to the effect
            AudioSource SFX = impact.AddComponent<AudioSource>();
            SFX.clip = _shotImpactSound[FX_ID];
            SFX.Play();
        }
    }
        
    /// <summary>
    /// Only required for homing or bullets with special trajectory. Currently unnecessary
    /// </summary>
    private void UpdateRotation()
    {
       
    }

    /// <summary>
    /// The base damage of the projectile
    /// </summary>
    public int BaseDamage
    {
        get
        {
            return _shotDamage;
        }
    }
    
    /// <summary>
    /// The possible bonus damage of the projectile
    /// </summary>
    public int BonusDamage
    {
        get
        {
            return _bonusDamage;
        }
    }

    /// <summary>
    /// The Ordnance type of the projectile
    /// </summary>
    public OrdnanceType DamageType
    {
        get
        {
            return _ordnanceType;
        }
    }

    /// <summary>
    /// Returns true if the projectile can deal splash damage to nearby units on impact
    /// </summary>
    public bool BDealsSplashDamage
    {
        get
        {
            return _bDealsSplashDamage;
        }
    }

    /// <summary>
    /// The radius of the sphere within which splash damage is applied
    /// </summary>
    public float SplashRadius
    {
        get
        {
            return _splashRadius;
        }
    
    }
}
