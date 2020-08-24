using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleActivationTestScript : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();
    public ParticleSystem VFX;
	// Use this for initialization
	void Start ()
    {
        _particleSystem = gameObject.GetComponent<ParticleSystem>();
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        /*if (_particleSystem.particleCount >= 5)
        {
            Debug.Log("Check");
            _particleSystem.Stop();
        }*/
        if (Input.GetMouseButtonDown(0))
        {
            if (_particleSystem.isStopped)
            {
                _particleSystem.Play();
                Debug.Log("stuff");
            }
        }
	}

    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(_particleSystem, other, _collisionEvents);
        for (int i = 0; i < _collisionEvents.Count; i++)
        {
            Instantiate(VFX, _collisionEvents[i].intersection, Quaternion.identity);
        }
    }
}
