using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionRingObjScript : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        Hide();
	}
	
	// Update is called once per frame
	void Update ()
    {
        MaintainRotation();
	}

    public void Show()
    {
        gameObject.SetActive(true);

    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void MaintainRotation()
    {
        if (gameObject.transform.rotation != Quaternion.Euler(90, 0, 0))
        {
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
