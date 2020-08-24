using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; /// Deactivate for final build
#endif

#if UNITY_EDITOR
[ExecuteInEditMode] /// Deactivate for final build
#endif
#if UNITY_EDITOR
public class TrailerCameraManager : MonoBehaviour
{
	Camera _cam;

	List<Vector3> _camPositions = new List<Vector3>();
	List<Quaternion> _camRotations = new List<Quaternion>();

	float t = 0.0f;

	//

	public bool _bSetLocAndRot = false;
	public bool _bClearLocsAndRots = false;

	void Start()
	{
		_cam = GetComponent<Camera>();

		if (_cam != null) _cam.enabled = true;
	}

	void Update()
	{
		/// Lerp between all of the _camPositions
		
		if (Application.isPlaying)
		{
			print("1");

			if (_camPositions.Count >= 2)
			{
				print("2");

				t += Time.deltaTime;

				_cam.transform.rotation = Quaternion.Lerp(_camRotations[0], _camRotations[1], t);
				_cam.transform.position = Vector3.Lerp(_camPositions[0], _camPositions[1], t);

				float dist = Vector3.Distance(_cam.transform.position, _camPositions[0]);
				if (dist < 0.1f)
				{
					print("3");

					_camPositions.RemoveAt(0);
					_camRotations.RemoveAt(0);

					t = 0.0f;
				}
			}
		}
		else if (Application.isEditor)
		{
			if (_bSetLocAndRot)
			{
				if (UnityEditor.SceneView.GetAllSceneCameras().Length > 0)
				{
                    
                    Vector3 camLoc = UnityEditor.SceneView.GetAllSceneCameras()[0].transform.position;
					Quaternion camRot = UnityEditor.SceneView.GetAllSceneCameras()[0].transform.rotation;
                    
                    _camPositions.Add(camLoc);
					_camRotations.Add(camRot);

					print("Added Loc: " + camLoc + " -- Added Rot: " + camRot.eulerAngles);
				}
				
				_bSetLocAndRot = false;
			}

			if (_bClearLocsAndRots)
			{
				_camPositions.Clear();
				_camRotations.Clear();

				_bClearLocsAndRots = false;
			}
		}
	}
}
#endif
