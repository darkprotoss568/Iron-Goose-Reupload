using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopTurret : TurretScriptParent
{
    [Header("Chop Damage")]
    [SerializeField]
    private OrdnanceType _damageType;
    [SerializeField]
    private int _damage;
    [SerializeField]
    private int _bonusDamage;

    private bool _bHitTarget;
    private float _currAttackDelay;


    [Header("Effects")]
    [SerializeField]
    private AudioClip _chopSwing;
    [SerializeField]
    private AudioClip _chopImpactSound;         
    [SerializeField]
    private GameObject _chopImpactFX;
    private Vector3 _impactPos;
    // The object this turret is attached to
    protected GameObject _torsoRotatorObj;

 

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        _horizontalRotatorObj = transform.Find("turret_y").gameObject;
        _verticalRotatorObj = _horizontalRotatorObj.transform.Find("turret_x").gameObject;
        _torsoRotatorObj = transform.parent.gameObject;
        //AutoChooseTarget();

        _currAttackDelay = _shotDelay;
        ParentTargetCarriage();
        _bHitTarget = false;
    }

    // Update is called once per frame
    public override void Update()
    {
        if (PauseMenu.isPaused) return;
        base.Update();

        _currAttackDelay += Time.deltaTime;
        if (_bHitTarget)
        {
            CreateImpactEffects();            
            _bHitTarget = false;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (_target != null)
        {
            RotateToTarget(_target.CommSocketObj.transform.position);
        }
    }

    void ParentTargetCarriage()
    {
        if (_connectedParentObj != null)
        {
            AiChopScript gus = _connectedParentObj.GetComponent<AiChopScript>();
            if (gus != null && gus.TargetCarriage != null)
            {
                _target = gus.TargetCarriage;
            }
        }
    }

    protected override bool CheckCanHitTarget()
    {
        if (_target != null)
        {
            float dist = Vector3.Distance(_target.transform.position, transform.position);
            if (BBBStatics.CheckGameObjectColliderWithinRange(transform.position, _firingRange, _target.gameObject))
            {
                return true;
            }             
        }
        return false;
    }

    void RotateToTarget(Vector3 target) 
    {        
        float desired_angle_y = BBBStatics.GetAngleOnPlaneToFrom(target, _torsoRotatorObj.transform.position);
        float curr_angle_y = _torsoRotatorObj.transform.rotation.eulerAngles.y;
        float diff_angle_y = BBBStatics.AngleDiff(curr_angle_y, desired_angle_y);

        float addition = -(80.0f * Time.deltaTime); 
        if (diff_angle_y > 0.0f) addition *= -1;

        float slowDownAngle = 2.0f;
        if (Mathf.Abs(diff_angle_y) < slowDownAngle) addition *= BBBStatics.Map(Mathf.Abs(diff_angle_y), slowDownAngle, 0.0f, 1.0f, 0.0f, true);

        Vector3 torsoRot = _torsoRotatorObj.transform.rotation.eulerAngles;
        _torsoRotatorObj.transform.rotation = Quaternion.Euler(new Vector3(torsoRot.x, torsoRot.y, torsoRot.z + addition));

    }

    protected override void Fire_Simplified()
    {
        /*
        if (_currAttackDelay >= _shotDelay && CheckCanHitTarget())
        {
            //_target.Damage_Additive(_damageType, _damage, _bonusDamage, true);
            _currAttackDelay = 0.0f;
            AudioSource.PlayClipAtPoint(_chopSwing, transform.position);
        }        
        */
        if (_bHitTarget)
            _target.Damage_Additive(_damageType, _damage, _bonusDamage, true);
    }

    public void CreateImpactEffects()
    {
        if (_chopImpactFX != null && _chopImpactSound != null)
        {
            // Create visual particle effects
            GameObject VFX;
            VFX = Instantiate(_chopImpactFX, _impactPos, Quaternion.identity);
            // Create the sound effect by adding a sound component to the effect
            AudioSource SFX = VFX.AddComponent<AudioSource>();
            SFX.clip = _chopImpactSound;
            SFX.Play();
        }
    }

    public bool HitTarget
    {
        get { return _bHitTarget; }
        set { _bHitTarget = value; }
    }

    public Vector3 ImpactPosition
    {
        get { return _impactPos; }
        set { _impactPos = value; }
    }

}
