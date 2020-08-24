using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeRangeProjectorScript : MonoBehaviour
{

    private float _currAngle;
    private MeshFilter _coneMesh;
    private LineRenderer _outlineRenderer;

	// Use this for initialization
	void Awake ()
    {
        _coneMesh = gameObject.transform.GetChild(0).GetComponent<MeshFilter>();
        _outlineRenderer = gameObject.transform.GetChild(1).GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void AdjustCone(float range, float angle)
    {
        _coneMesh.mesh = BBBStatics.CreateFieldOfViewConeMesh(new Vector3(0, 0, -range), angle);
        _currAngle = angle;

        if (_currAngle < 360)
        {
            // Set the three points needed for the outline render. 
            //As the points will be rendered by the order of 0, 1, 2, we must set (0,0,0) as the second point to use it as the corner
            _outlineRenderer.SetPosition(1, Vector3.zero);
            // Calculate the other two points by rotating (0,0, range), which is the straight forward aiming angle, by angle/2 in either side.
            _outlineRenderer.SetPosition(0, BBBStatics.RotateByAngleOnXZPlane(new Vector3(0, 0, -range), -angle / 2));
            _outlineRenderer.SetPosition(2, BBBStatics.RotateByAngleOnXZPlane(new Vector3(0, 0, -range), angle / 2));
        }
        else
        {
            _outlineRenderer.SetPosition(1, Vector3.zero);
            _outlineRenderer.SetPosition(0, Vector3.zero);
            _outlineRenderer.SetPosition(2, Vector3.zero);
        }
    }
}
