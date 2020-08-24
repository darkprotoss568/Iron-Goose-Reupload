using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailerCamera: MonoBehaviour {
    public Camera[] Cameras;
    private int counter;
	// Use this for initialization
	void Awake () {
        counter = 0;
        foreach (Camera x in Cameras)
        {
            x.enabled = false;
        }
        Cameras[0].enabled = true;
	}

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyUp(KeyCode.L))
        {
            if (counter == 0)
            {
                Cameras[0].enabled = false;
                Cameras[1].enabled = true;
                counter = 1;
            }
            else
            {
                Cameras[1].enabled = true;
                Cameras[0].enabled = false;
            }
        }
    }
}
