using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; /// Deactivate for final build
#endif

#if UNITY_EDITOR
[ExecuteInEditMode] /// Deactivate for final build
#endif

public class KeepAboveGndScript : MonoBehaviour
{
#if UNITY_EDITOR
    public bool _bActive = true;
    public float _height = 3.0f;

    private EnvProcGenScript _parentProcGenScr;

    private Vector3 _lastPos = Vector3.zero;

    void Awake()
    {
        _lastPos = transform.position;
    }

    void Start()
    {
        _lastPos = transform.position;
    }

    void Update()
    {
        if (!Application.isEditor || Application.isPlaying) return;

        bool bHasMoved = _lastPos != transform.position;

        if (_bActive && bHasMoved) // transform.hasChanged)
        {
            //print("bHasMoved");

            //transform.hasChanged = false;
            bHasMoved = false;

            if (_parentProcGenScr == null && transform.parent != null)
            {
                _parentProcGenScr = transform.parent.GetComponent<EnvProcGenScript>();
            }

            MaintainHeight();
            RunAnyParentProcGenScr();
        }

        if (_lastPos != transform.position) _lastPos = transform.position;
    }

    void MaintainHeight()
    {
        Vector3 normal;
        Vector3 v = BBBStatics.CheckForGroundV_V2(transform.position, 100.0f, out normal);
        if (v == Vector3.zero) return;
        transform.position = new Vector3(v.x, v.y + _height, v.z);
    }

    void RunAnyParentProcGenScr()
    {
        //if (Application.isPlaying) return;

        //print("RunAnyParentProcGenScr()");

        if (_parentProcGenScr != null && _parentProcGenScr._bRunOnVertMove) _parentProcGenScr.Run();
    }
#endif
}
