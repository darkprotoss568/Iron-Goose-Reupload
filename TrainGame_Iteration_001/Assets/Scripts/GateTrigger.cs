using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateTrigger : TriggerVolumeScript
{
    public GameObject _gate;

	public override void Start()
    {
        base.Start();
	}

    public override void OnTriggerEnter(Collider other)
    {
        if (_worldScript == null) return;
        if (_worldScript.LocomotiveObjectRef == null) return;

        if (other.gameObject != null && other.gameObject == _worldScript.LocomotiveObjectRef && _gate != null)
        {
            if (_gate.GetComponent<GateScript>())
            {
                _gate.GetComponent<GateScript>().BeginOpen = true;
            }

            base.OnTriggerEnter(other);
        }
    }
}
