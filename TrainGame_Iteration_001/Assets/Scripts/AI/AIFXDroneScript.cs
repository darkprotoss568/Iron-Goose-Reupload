using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Drones which use the downwards beam effect should inherit from this class

public class AIFXDroneScript : AIDroneScript
{
	protected GameObject _specFX_archetype = null;
	protected GameObject _specFX_archetype_orig = null;
	protected GameObject _specFX_archetype_alt = null;

	protected GameObject _specFX_archetype_curr = null;

	protected List<GameObject> _specFX = new List<GameObject>();
	protected List<LineRenderer> _specFX_lr = new List<LineRenderer>();
	protected List<int> _specFX_vertIdx = new List<int>();

	protected List<Vector2> _specFX_UVOffset = new List<Vector2>();

	protected int _desiredSpecFXCount = 5;

	protected float timeSinceFXLastActive;
	protected int framesSinceFXLastActive;

	protected GameObject _fxSocket;

	//

	protected AudioSource _fxAudioSource;

	protected AudioClip _fxAudioClip1;
	protected AudioClip _fxAudioClip2;

	protected AudioClip _fxAudioClip_curr;

	public int _fxAudioInitialClip_num = 1;

	public override void Start()
	{
		base.Start();

		if (_specFX_archetype == null) _specFX_archetype = Resources.Load("FX/ArcBeam001") as GameObject; // Default FX
		if (_specFX_archetype_alt == null) _specFX_archetype_alt = Resources.Load("FX/ArcBeam002") as GameObject; // Alternative FX

		if (_specFX_archetype != null)
		{
			_specFX_archetype_orig = _specFX_archetype;
		}

		_specFX_archetype_curr = _specFX_archetype; // Keep an extra record

		InitialiseFXGO(_specFX_archetype_curr);

		timeSinceFXLastActive = 0.0f;
		framesSinceFXLastActive = 0;

		//print("_specFX count: " + _specFX.Count);

		if (transform.Find("FXSocket") == null) print("Error: FXSocket is missing -- AIFXDroneScript @ Start()");
		_fxSocket = transform.Find("FXSocket").gameObject;

		//

		_fxAudioSource = gameObject.AddComponent<AudioSource>();

		_fxAudioClip1 = Resources.Load("Sounds/cons1") as AudioClip;
		_fxAudioClip2 = Resources.Load("Sounds/cons2") as AudioClip;

		if (_fxAudioInitialClip_num == 1) InitialiseFXAudio(_fxAudioClip1);
		else if (_fxAudioInitialClip_num == 2) InitialiseFXAudio(_fxAudioClip2);

		// End of Start()
	}

	protected void InitialiseFXAudio(AudioClip _audioClip)
	{
		if (_audioClip != null)
		{
			_fxAudioSource.clip = _audioClip;
			_fxAudioSource.loop = true;
			_fxAudioSource.volume = 0.2f;
			_fxAudioSource.spatialBlend = 1.0f; // 3D
			_fxAudioSource.playOnAwake = true;
			//_fxAudioSource.Stop();
			_fxAudioSource.Pause();

			_fxAudioClip_curr = _audioClip;
		}
	}

	protected void InitialiseFXGO(GameObject _archetype)
	{
		if (_archetype != null)
		{
            int count = _specFX.Count;
            for (int i = 0; i < count; ++i) // Get rid of any pre-existing FX objects
			{
				Destroy(_specFX[i]);
			}

			_specFX.Clear();
			_specFX_lr.Clear();
			_specFX_UVOffset.Clear();
			_specFX_vertIdx.Clear();

			//
			for (int i = 0; i < _desiredSpecFXCount; ++i)
			{
				GameObject tempFX = Instantiate(_archetype, transform) as GameObject;
				if (tempFX != null)
				{
					tempFX.transform.parent = transform;
					tempFX.SetActive(false);

					LineRenderer tempLR = tempFX.GetComponent<LineRenderer>();
					if (tempLR != null)
					{
						_specFX_lr.Add(tempLR);
					}

					_specFX.Add(tempFX);
					_specFX_UVOffset.Add(new Vector2(10 * i, 0));

					_specFX_vertIdx.Add(50 * i);
				}
			}
		}
	}

	public new void FixedUpdate()
	{
		if (PauseMenu.isPaused) return;

		base.FixedUpdate();

        timeSinceFXLastActive += Time.deltaTime;
		framesSinceFXLastActive += 1;

		if (framesSinceFXLastActive > 2)
		{
            int count = _specFX.Count;
            for (int i = 0; i < count; ++i)
			{
				if (_specFX[i].activeInHierarchy)
				{
					_specFX_lr[i].material.SetTextureOffset("_MainTex", new Vector2(0, 0));
					_specFX[i].SetActive(false);
				}
			}
		}

		if (framesSinceFXLastActive > 5)
		{
			if (_fxAudioSource.isPlaying) _fxAudioSource.Pause();
		}
	}

	public void RunFX(List<Vector3> endLocPositions)
	{
		timeSinceFXLastActive = 0.0f;
		framesSinceFXLastActive = 0;

		if (!_fxAudioSource.enabled) _fxAudioSource.enabled = true; // Just in-case it ever gets disabled
		if (!_fxAudioSource.isPlaying) _fxAudioSource.Play();

        //

        Vector3 FXSocketPos = _fxSocket.transform.position;
        int count = _specFX.Count;
        for (int i = 0; i < count; ++i)
		{
			//_specFX_vertIdx[i] += 3;
			//if (_specFX_vertIdx[i] > cs.InitialVertCount) _specFX_vertIdx[i] = 0;

			if (!_specFX[i].activeInHierarchy) _specFX[i].SetActive(true);

			//Vector3 vertPos = cs.GetVert(_specFX_vertIdx[i]);
			Vector3 vertPos = endLocPositions[i];

			//

			Vector3 p = vertPos - FXSocketPos;
			p *= 0.1f; // 0.25f

			//float FXMaxDistFromFXSocketPos = 0.0f;
			//p.x = Mathf.Clamp(p.x, -FXMaxDistFromFXSocketPos / 2, FXMaxDistFromFXSocketPos / 2);
			//p.z = Mathf.Clamp(p.x, -FXMaxDistFromFXSocketPos / 2, FXMaxDistFromFXSocketPos / 2);

			//

			Vector3 start = new Vector3(FXSocketPos.x + p.x, FXSocketPos.y, FXSocketPos.z + p.z);
			Vector3 end = vertPos;

			_specFX_lr[i].widthMultiplier = 0.25f;

			_specFX_lr[i].SetPosition(0, start);
			_specFX_lr[i].SetPosition(1, BBBStatics.BetweenAt(start, end, 0.5f));
			_specFX_lr[i].SetPosition(2, end);

			_specFX_UVOffset[i] += new Vector2(10, 0) * Time.deltaTime; // new Vector2(50, 0) * Time.deltaTime;
			_specFX_lr[i].material.SetTextureOffset("_MainTex", _specFX_UVOffset[i]);
		}

		//cs.CurrItrVert += 1;
	}

	protected override void NotMoving()
	{
	}
}
