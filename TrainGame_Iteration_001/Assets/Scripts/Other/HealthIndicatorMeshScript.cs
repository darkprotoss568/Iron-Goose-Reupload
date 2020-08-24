using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIndicatorMeshScript : MonoBehaviour
{
	private TrainGameObjScript _parent = null;

	private MeshRenderer mr = null;

    

	void Start()
	{
		mr = GetComponent<MeshRenderer>();

		// Either we find a parent with a TGO or we find nothing

		Transform currTF = transform;
		while (currTF.parent != null)
		{
			_parent = currTF.parent.gameObject.GetComponent<TrainGameObjScript>();
			if (_parent != null)
			{
				break;
			}
			currTF = currTF.parent; // Shift up a level
		}
	}

    void Update()
    {

        if (_parent == null)
        {
            Destroy(gameObject);
            return;
        }

        float pcnt = _parent.GetHealth0to1(); // GetHealth0to1 // GetOverallHealth0to1

        transform.localScale = new Vector3(1, pcnt, 1);

        if (pcnt < 0.3f)
            mr.materials[0].SetColor("_EmissionColor", new Color(1, 0, 0));
        else if (pcnt < 0.75f)
            mr.materials[0].SetColor("_EmissionColor", new Color(1, 1, 0));
        else
            mr.materials[0].SetColor("_EmissionColor", new Color(0, 1, 0));
    }
}
