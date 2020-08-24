using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GridGenerator : Editor {
    public GameObject GridObject;
	// Use this for initialization
	void Start () {
		if(GridObject == null)
        {
            GridObject = GameObject.Find("GridObject");
        }
	}
	
	// Update is called once per frame

}
