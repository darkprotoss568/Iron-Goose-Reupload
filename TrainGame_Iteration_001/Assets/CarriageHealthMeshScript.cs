using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarriageHealthMeshScript : MonoBehaviour {

    private TrainGameObjScript _parent = null;

    private MeshRenderer mr = null;

    private bool _checkflash;
    float timer = 0;
    bool timerReached = false;
    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        _checkflash = true;
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
        
        if (!timerReached)
            timer += Time.deltaTime;

        if (!timerReached && timer > 0.3f)
        {
            FlashingHealth();
            timer = 0;
        } 
    }


    void FlashingHealth()
    {
        float pcnt = _parent.GetHealth0to1(); // GetHealth0to1 // GetOverallHealth0to1
        if (pcnt < 0.3f)
        {
            
                if (_checkflash)
                {
                    //mr.materials[0].SetColor("_EmissionColor", new Color(0, 0, 0));
                    mr.materials[0].SetColor("_Color", new Color(1, 0, 0));
                    _checkflash = false;
                }
                else
                {
                    //mr.materials[0].SetColor("_EmissionColor", new Color(1, 1, 1));
                    mr.materials[0].SetColor("_Color", new Color(0, 0, 0));
                    _checkflash = true;
                }
                
            
        }

        else
        {
            mr.materials[0].SetColor("_EmissionColor", new Color(0, 0, 0));
            mr.materials[0].SetColor("_Color", new Color(0, 0, 0));
        }
    }
}
