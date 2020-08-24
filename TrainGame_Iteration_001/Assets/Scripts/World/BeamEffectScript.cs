using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamEffectScript : MonoBehaviour
{
	private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } }

	private List<LineRenderer> _lineRendererBeamEffects = new List<LineRenderer>();
	private List<float> _lineRendererBeamEffects_Lifetime = new List<float>();
	private List<float> _lineRendererBeamEffects_LifetimePassed = new List<float>();
	private List<Vector2> _lineRendererBeamEffects_UVOffset = new List<Vector2>();
	private List<Vector2> _lineRendererBeamEffects_UVAnimRate = new List<Vector2>();
	private List<float> _lineRendererBeamEffects_Width = new List<float>();
	private List<Vector3> _lineRendererBeamEffects_StartPnt = new List<Vector3>();
	private List<Vector3> _lineRendererBeamEffects_EndPnt = new List<Vector3>();
	private List<GameObject> _lineRendererBeamEffects_Owner = new List<GameObject>();
	private List<GameObject> _lineRendererBeamEffects_Target = new List<GameObject>();

	void Start()
	{
	}

	void Update()
	{
		if (PauseMenu.isPaused) return;

		ManageBeamEffects();
	}

	public void CreateBeamEffect(LineRenderer lr, float beamLifetime, float beamUVAnimRate, float beamWidth, Vector3 start, Vector3 end, GameObject ownerTurret, GameObject target)
	{
		_lineRendererBeamEffects.Add(lr);
		_lineRendererBeamEffects_Lifetime.Add(beamLifetime);
		_lineRendererBeamEffects_LifetimePassed.Add(0.0f);
		_lineRendererBeamEffects_UVOffset.Add(new Vector2(0.0f, 0.0f));
		_lineRendererBeamEffects_UVAnimRate.Add(new Vector2(beamUVAnimRate, 0.0f)); // x axis = along the beam
		_lineRendererBeamEffects_Width.Add(beamWidth);
		_lineRendererBeamEffects_StartPnt.Add(start);
		_lineRendererBeamEffects_EndPnt.Add(end);
		_lineRendererBeamEffects_Owner.Add(ownerTurret);
		_lineRendererBeamEffects_Target.Add(target);
	}

	private void ManageBeamEffects()
	{
		if (_lineRendererBeamEffects.Count == 0) return;

		// Don't keep null beam effects
		List<LineRenderer> temp_lineRendererBeamEffects = new List<LineRenderer>();
		List<float> temp_lineRendererBeamEffects_Lifetime = new List<float>();
		List<float> temp_lineRendererBeamEffects_LifetimePassed = new List<float>();
		List<Vector2> temp_lineRendererBeamEffects_UVOffset = new List<Vector2>();
		List<Vector2> temp_lineRendererBeamEffects_UVAnimRate = new List<Vector2>();
		List<float> temp_lineRendererBeamEffects_Width = new List<float>();
		List<Vector3> temp_lineRendererBeamEffects_StartPnt = new List<Vector3>();
		List<Vector3> temp_lineRendererBeamEffects_EndPnt = new List<Vector3>();
		List<GameObject> temp_lineRendererBeamEffects_Owner = new List<GameObject>();
		List<GameObject> temp_lineRendererBeamEffects_Target = new List<GameObject>();

		for (int i = 0; i < _lineRendererBeamEffects.Count; ++i)
		{
			//if (_lineRendererBeamEffects_Owner[i] == null) continue; // Its turret has been destroyed -- this doesn't destroy it, just de-refs it!
			if (_lineRendererBeamEffects[i] == null) continue; // It's already been destroyed but not yet removed from the list
			//if (_lineRendererBeamEffects_Target[i] == null) continue; // Its target has been destroyed -- necessary ?

			//float lifeTimePercent = Mathf.Clamp01(_lineRendererBeamEffects_LifetimePassed[i] / _lineRendererBeamEffects_Lifetime[i]);
			float lifeTimePercent = BBBStatics.Map(_lineRendererBeamEffects_LifetimePassed[i], 0.0f, _lineRendererBeamEffects_Lifetime[i], 0.0f, 1.0f, true);

			if (lifeTimePercent >= 1.0f || _lineRendererBeamEffects_Owner[i] == null) // Its turret has been destroyed
			{
				// It's dead
				Destroy(_lineRendererBeamEffects[i]);
				continue;
			}

			//

			// Keep it
			temp_lineRendererBeamEffects.Add(_lineRendererBeamEffects[i]);
			temp_lineRendererBeamEffects_Lifetime.Add(_lineRendererBeamEffects_Lifetime[i]);
			temp_lineRendererBeamEffects_LifetimePassed.Add(_lineRendererBeamEffects_LifetimePassed[i]);
			temp_lineRendererBeamEffects_UVOffset.Add(_lineRendererBeamEffects_UVOffset[i]);
			temp_lineRendererBeamEffects_UVAnimRate.Add(_lineRendererBeamEffects_UVAnimRate[i]);
			temp_lineRendererBeamEffects_Width.Add(_lineRendererBeamEffects_Width[i]);
			temp_lineRendererBeamEffects_StartPnt.Add(_lineRendererBeamEffects_StartPnt[i]);
			temp_lineRendererBeamEffects_EndPnt.Add(_lineRendererBeamEffects_EndPnt[i]);
			temp_lineRendererBeamEffects_Owner.Add(_lineRendererBeamEffects_Owner[i]);
			temp_lineRendererBeamEffects_Target.Add(_lineRendererBeamEffects_Target[i]);
		}

		// Copy the keepers back
		_lineRendererBeamEffects = temp_lineRendererBeamEffects;
		_lineRendererBeamEffects_Lifetime = temp_lineRendererBeamEffects_Lifetime;
		_lineRendererBeamEffects_LifetimePassed = temp_lineRendererBeamEffects_LifetimePassed;
		_lineRendererBeamEffects_UVOffset = temp_lineRendererBeamEffects_UVOffset;
		_lineRendererBeamEffects_UVAnimRate = temp_lineRendererBeamEffects_UVAnimRate;
		_lineRendererBeamEffects_Width = temp_lineRendererBeamEffects_Width;
		_lineRendererBeamEffects_StartPnt = temp_lineRendererBeamEffects_StartPnt;
		_lineRendererBeamEffects_EndPnt = temp_lineRendererBeamEffects_EndPnt;
		_lineRendererBeamEffects_Owner = temp_lineRendererBeamEffects_Owner;
		_lineRendererBeamEffects_Target = temp_lineRendererBeamEffects_Target;

		//

		if (_lineRendererBeamEffects.Count == 0) return;

		//

		// Do things with the living beam effects
		for (int i = 0; i < _lineRendererBeamEffects.Count; ++i)
		{
			_lineRendererBeamEffects_LifetimePassed[i] += Time.deltaTime;

			//float lifeTimePercent = Mathf.Clamp01(_lineRendererBeamEffects_LifetimePassed[i] / _lineRendererBeamEffects_Lifetime[i]);
			float lifeTimePercent = BBBStatics.Map(_lineRendererBeamEffects_LifetimePassed[i], 0.0f, _lineRendererBeamEffects_Lifetime[i], 0.0f, 1.0f, true);

			//_lineRendererBeamEffects[i].startWidth = 2.0f * (1 - lifeTimePercent); // These don't always work for some reason + we're now using more points along the beam
			//_lineRendererBeamEffects[i].endWidth = 2.0f * (1 - lifeTimePercent);

			//

			// Move the start of the beam so it continues to emit from the hardpoint it was fired from, even when that hardpoint moves
			TurretScriptParent tsp = _lineRendererBeamEffects_Owner[i].GetComponent<TurretScriptParent>();
			if (tsp != null)
			{
				// Make a record of how far the start point is about to move so we can move the end point by the same amount
				Vector3 startPntMovementAmnt = tsp.HardpointObjs[tsp.CurrHardpoint].transform.position - _lineRendererBeamEffects_StartPnt[i];

				_lineRendererBeamEffects_StartPnt[i] = tsp.HardpointObjs[tsp.CurrHardpoint].transform.position;

				// Also move the end point so that it maintains its original offset from the start point
				_lineRendererBeamEffects_EndPnt[i] += startPntMovementAmnt;

				_lineRendererBeamEffects[i].SetPosition(0, _lineRendererBeamEffects_StartPnt[i]);
				_lineRendererBeamEffects[i].SetPosition(1, BBBStatics.BetweenAt(_lineRendererBeamEffects_StartPnt[i], _lineRendererBeamEffects_EndPnt[i], 0.5f));
				_lineRendererBeamEffects[i].SetPosition(2, _lineRendererBeamEffects_EndPnt[i]);
			}

			//

			_lineRendererBeamEffects[i].widthMultiplier = _lineRendererBeamEffects_Width[i] * (1 - lifeTimePercent);

			_lineRendererBeamEffects_UVOffset[i] += (_lineRendererBeamEffects_UVAnimRate[i] * Time.deltaTime);

			_lineRendererBeamEffects[i].material.SetTextureOffset("_MainTex", _lineRendererBeamEffects_UVOffset[i]);
		}
	}
}
