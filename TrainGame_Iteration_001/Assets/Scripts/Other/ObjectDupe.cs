using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDupe : MonoBehaviour {
    public int dupDistance;
    public enum DupeDirection {positiveX,negativeX,positiveY,negativeY,positiveZ,negativeZ};
    public DupeDirection Direction;
    public int amount;
    public int changeDistance;
    public bool OnDupe;
	// Use this for initialization
	public GameObject dupeObject()
    {
        Vector3 newDupePos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        if(Direction == DupeDirection.positiveX)
        {
            newDupePos.x += changeDistance;
        }
        else if(Direction == DupeDirection.negativeX)
        {
            newDupePos.x -= changeDistance;
        }
        else if (Direction == DupeDirection.positiveY)
        {
            newDupePos.y += changeDistance;
        }
        else if (Direction == DupeDirection.negativeY)
        {
            newDupePos.y -= changeDistance;
        }
        else if (Direction == DupeDirection.positiveZ)
        {
            newDupePos.z += changeDistance;
        }
        else if (Direction == DupeDirection.negativeZ)
        {
            newDupePos.z -= changeDistance;
        }
        changeDistance += dupDistance;
        GameObject x = GameObject.Instantiate(gameObject, newDupePos, gameObject.transform.rotation);
        x.GetComponent<ObjectDupe>().removeComponent();
        return x;
    }
    public void reset()
    {
        changeDistance = dupDistance;
    }
    public void removeComponent()
    {
        DestroyImmediate(GetComponent<ObjectDupe>());
    }
}
