using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiChopScript : AIGroundUnitScript
{    
    protected List<Collider> _collidersInAvoidanceRange = new List<Collider>();
    protected float _maxAvoidSpeed = 15;
    protected float _minAvoidSpeed = 10;
    protected SphereCollider _ourAvoidanceTriggerCollider;
    protected ChopTurret _ChopTurret;
    protected Animator _chopAnimatorComp;
    protected Animator _walkAnimatorComp;
    [SerializeField]
    private Transform _leftAxe;
    [SerializeField]
    private Transform _rightAxe;

    // Use this for initialization
    public override void Start()
    {
        _bIsChop = true;
        _bPostStartRun = true;
        base.Start();
        _myType = GetType().Name;
        _ChopTurret = gameObject.GetComponentInChildren<ChopTurret>();
        _ChopTurret.AttachedTo = _myType;
        _ChopTurret.ConnectedParentObj = gameObject;
    }

    protected override void FindFollowRailPath(bool bAlsoSetFilledPaths)
    {
        if (bAlsoSetFilledPaths)
            _currFollowingRail.FilledPaths[_currFollowingRail_path] = false;

        if (_trainSideOn == TrainSideOn.Left)
        {
            if (_currFollowingRail.FilledPaths[6] == false)
                _currFollowingRail_path = 6;
            else if (_currFollowingRail.FilledPaths[7] == false)
                _currFollowingRail_path = 7;
            else
                _currFollowingRail.FilledPaths[_currFollowingRail_path] = false;
        }
        if (_trainSideOn == TrainSideOn.Right)
        {
            if (_currFollowingRail.FilledPaths[7] == false)
                _currFollowingRail_path = 7;
            else if (_currFollowingRail.FilledPaths[6] == false)
                _currFollowingRail_path = 6;
            else
                _currFollowingRail.FilledPaths[_currFollowingRail_path] = false;
        }

        if (bAlsoSetFilledPaths)
            _currFollowingRail.FilledPaths[_currFollowingRail_path] = true;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    private new void FixedUpdate()
    {
        if (PauseMenu.isPaused) return;
        base.FixedUpdate();
        _bPostStartRun = true;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<CarriageScript>())
        {
            _ChopTurret.HitTarget = true;
            if (_leftAxe.position.y < _rightAxe.position.y)
                _ChopTurret.ImpactPosition = _leftAxe.position;
            else if (_rightAxe.position.y < _leftAxe.position.y)
                _ChopTurret.ImpactPosition = _rightAxe.position;            
        }
    }
}


