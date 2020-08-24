using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class UnitController : MonoBehaviour {
    public float UnitHealth;
    public bool GodMode;
    public float UnitArmor;
    public float UnitResistance;
    public bool objectSelected;
    private NavMeshAgent agent;
	// Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update () {
        if (objectSelected)
        {
            if (Input.GetMouseButton(1))
            {
                RaycastHit hit;
                Ray MousePointRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(MousePointRay, out hit);
                agent.SetDestination(hit.point);
            }
        }
    }
    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            objectSelected = true;
        }
    }
}
