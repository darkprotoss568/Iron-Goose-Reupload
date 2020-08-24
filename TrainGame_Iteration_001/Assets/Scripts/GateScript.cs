using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateScript : MonoBehaviour
{    
    private WorldScript _worldScript;
    private GameObject _guilo;
    private bool _bBeginOpen;
    private bool _bOpenGate;
    private bool _bCloseGate;
    private bool _bCountdown;
    [SerializeField]
    private float _height;
    [SerializeField]
    private int _speed;
    [SerializeField]
    private float _distance;
    [SerializeField]
    private ParticleSystem _particle;
    private Vector3 _endPosition;
    
    void Start ()
    {
        _worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();
        _guilo = gameObject.transform.Find("Guilo").gameObject;
        _bBeginOpen = false;
        _bOpenGate = false;
        _bCloseGate = false;
        _bCountdown = false;
}

    void FixedUpdate()
    {
        if (_bBeginOpen)
        {            
            _endPosition = _guilo.transform.position + new Vector3(0, _height);                        
            _bBeginOpen = false;
            _bOpenGate = true;
            if(_particle != null) _particle.Play();
        }
        if (_bOpenGate)
        {
            MoveGuilo();
            if (Mathf.Abs(_guilo.transform.position.y - _endPosition.y) < 0.5)
            {
                if (_particle != null) _particle.Stop();
                _bOpenGate = false;
                _bCountdown = true;
            }
        }
        if (_bCountdown)
        {            
            if (Vector3.Distance(_worldScript.LocomotiveObjectRef.transform.position, transform.position) > _distance)
            {
                _endPosition = _guilo.transform.position - new Vector3(0, _height);
                _bCountdown = false;
                _bCloseGate = true;
            }
        }
        if (_bCloseGate)
        {
            MoveGuilo();
            if (Mathf.Abs(_guilo.transform.position.y - _endPosition.y) < 0.5)
            {
                _bCloseGate = false;
            }
        }
	}

    public void MoveGuilo()
    {
        _guilo.transform.position = Vector3.Lerp(_guilo.transform.position, _endPosition, Time.deltaTime * _speed);
    }    

    public bool BeginOpen
    {
        get { return _bBeginOpen; }
        set { _bBeginOpen = value; }
    }
}
