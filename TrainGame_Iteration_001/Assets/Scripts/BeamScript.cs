using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamScript : MonoBehaviour
{
    [SerializeField]
    private float _beamFXWidth;
    [SerializeField]
    private float _beamFXLifetime;
    private float _beamFXLifetimePassed = 0.0f;
    [SerializeField]
    private float _beamFX_UVAnimRate;
    [SerializeField]
    [Range(0, 1)]
    private float _maximumWidthMultiplier;
    private LineRenderer _renderer;
    private bool _active = true;
    // Use this for initialization
    void Start() {
        _renderer = gameObject.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        SimulateLiveBeamEffect();

    }

    private void SimulateLiveBeamEffect()
    {
        _beamFXLifetimePassed += Time.deltaTime;
        float lifeTimePercent = BBBStatics.Map(_beamFXLifetimePassed, 0.0f, _beamFXLifetime, 0.0f, _maximumWidthMultiplier, true);
        if (lifeTimePercent >= 1)
        {
            if (_active)
                _beamFXLifetime = 0;
            else
                Destroy(gameObject);
        }

        _renderer.widthMultiplier = _beamFXWidth * (1 - lifeTimePercent);
        _renderer.SetPosition(1, BBBStatics.BetweenAt(_renderer.GetPosition(0), _renderer.GetPosition(2), 0.5f));
        _renderer.material.SetTextureOffset("_MainTex", new Vector2(_beamFX_UVAnimRate, 0.0f) * Time.deltaTime);
    }

    public void KillBeam()
    {
        _active = false;
        _maximumWidthMultiplier = 1;
    }
}
