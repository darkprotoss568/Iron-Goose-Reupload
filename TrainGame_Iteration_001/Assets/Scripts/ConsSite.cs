using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsSite : MonoBehaviour
{
	protected WorldScript _worldScript;

	protected Team _team = Team.Neutral; // Only needed so we know which team to give cancallation resources back to

	protected ConsPlatformScript _consPlatform;
	protected GameObject _archetype = null;

	protected float _percentComplete; public float PercentComplete { get { return _percentComplete; } set { _percentComplete = value; } }

	protected int _resourcesAcquired;
	protected int _resourcesNeeded;

	protected GameObject _currDrone = null;

	//

	//protected MeshFilter _meshFilter;
	//protected MeshRenderer _meshRenderer;

	protected List<GameObject> _meshObjects;

	protected bool _bPostStartComplete;

	protected int _currItrVert;

	protected bool _bHasEverHadADroneAssigned = false;

	protected Material _consSiteMaterial;

	protected List<Vector3> _localSpaceVertices = new List<Vector3>();

	protected GameObject _goToCopy;

	protected float _autoBuildTime = 5.0f;

	public virtual void Start()
	{
		_worldScript = GameObject.Find("WorldScriptHolder").GetComponent<WorldScript>();

		_resourcesNeeded = BBBStatics.TGO(_archetype).BuildCost;
		_resourcesAcquired = 0;

		_percentComplete = 0.0f;

		//_meshFilter = GetComponent<MeshFilter>();
		//_meshRenderer = GetComponent<MeshRenderer>();

		_meshObjects = new List<GameObject>();

		_bPostStartComplete = false;

		//_currDrone = null; // It seems that Start() is actually called the next tick after something is instantiated, so it's not a constructor -- that's probably what Awake() is

		_currItrVert = 0;

		_consSiteMaterial = Resources.Load("Materials/consSiteMat") as Material; // Materials/consSiteMat // Materials/testMat1

		_goToCopy = _archetype;
	}

	public virtual void Update()
	{
		if (PauseMenu.isPaused) return;

		_percentComplete += (Time.deltaTime / _autoBuildTime) * 100.0f;

		//CheckPercentComplete();

		if (!_bPostStartComplete)
		{
			//bool bUseSingleMF = false;
			//if (bUseSingleMF)
			//{
			//	MeshFilter mf = _archetype.GetComponent<MeshFilter>();
			//	if (mf != null && mf.sharedMesh != null && _meshRenderer != null)
			//	{
			//		_meshFilter.mesh = mf.sharedMesh;

			//		//MeshRenderer mr = _archetype.GetComponent<MeshRenderer>();
			//		//_meshRenderer.materials[0] = _consSiteMaterial;
			//		_meshRenderer.material = _consSiteMaterial;
			//	}
			//}
			//else
			//{

			// Get all of the meshes from the archetype and then create a copy of each of them with the cons site material
			//List<MeshFilter> mfs = new List<MeshFilter>(_archetype.GetComponentsInChildren<MeshFilter>());
			//List<MeshRenderer> mrs = new List<MeshRenderer>(_archetype.GetComponentsInChildren<MeshRenderer>());

			List<Transform> tfs = _goToCopy.GetComponent<TrainGameObjScript>().GetAllChildTForms();

			// Get each gameobject which also has a MeshFilter component in the archetype
			// Then create copies of those gameobjects with the same mesh but a different material
			for (int i = 0; i < tfs.Count; ++i)
			{
				MeshFilter mf = tfs[i].gameObject.GetComponent<MeshFilter>();
				if (mf != null)
				{
					GameObject newMeshObj = new GameObject();
					MeshFilter newMF = newMeshObj.AddComponent<MeshFilter>();
					newMF.mesh = mf.sharedMesh;

					MeshRenderer newMR = newMeshObj.AddComponent<MeshRenderer>();
					newMR.material = _consSiteMaterial;

					///newMeshObj.transform.position = transform.position + (tfs[i].transform.position - tfs[0].transform.position);
					newMeshObj.transform.position = transform.position + (transform.rotation * (tfs[i].transform.position - tfs[0].transform.position));

					//newMeshObj.transform.rotation = tfs[i].transform.rotation;
					//newMeshObj.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
					newMeshObj.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + (tfs[i].transform.rotation.eulerAngles - tfs[0].transform.rotation.eulerAngles));

					newMeshObj.transform.parent = transform;

					_meshObjects.Add(newMeshObj);
				}
			}

			//print("_meshObjects.Count: " + _meshObjects.Count);
			//}

			//

			for (int i = 0; i < _meshObjects.Count; ++i)
			{
				_localSpaceVertices.AddRange(_meshObjects[i].GetComponent<MeshFilter>().mesh.vertices);
			}

			//print("_localSpaceVertices.Count: " + _localSpaceVertices.Count);

			_bPostStartComplete = true;
		}
        

		ManageMaterial();
		CheckForComplete();

		//

		if (_currDrone != null && !_bHasEverHadADroneAssigned) _bHasEverHadADroneAssigned = true;

		//

		if (CurrItrVert > _localSpaceVertices.Count)
		{
			CurrItrVert = 0;
		}
	}

	/*protected void CheckDroneStatus_RTS()
	{
		if (_worldScript.gameType == GameType.RTS)
		{
			if (_bHasEverHadADroneAssigned) // This should have been set to true quickly after instantiation when the drone was assigned
			{
				if (_currDrone != null)
				{
					// Destroy this cons site if there is no drone assigned to it
					AIConsDroneScript cds = _currDrone.GetComponent<AIConsDroneScript>();
					if (cds)
					{
						if (_currDrone != null && cds.CurrConsSite == null && cds.CurrRecySite == null) // Drone has no cons or recy site
						{
							_currDrone = null;
						}

						if (_currDrone != null && cds.CurrConsSite != null && cds.CurrConsSite != gameObject) // Drone has a different cons site
						{
							_currDrone = null;
						}

						if (_currDrone != null && cds.CurrRecySite != null && cds.CurrRecySite != gameObject) // Drone has a different cons site
						{
							_currDrone = null;
						}
					}
				}
				else // There is no drone assigned to us
				{
					if (Team == Team.Friendly && _percentComplete < 100.0f) // Incomplete build
					{
						_worldScript.GameplayScript.AddResources(_resourcesNeeded); // Cancel build and restore value // Note: A recy site will have _resourcesNeeded = 0
					}

					//Destroy(gameObject);
					BeginDestroy();
				}
			}
		}
	}*/

	//protected void CheckPercentComplete()
	//{
	//	float _resourcesAcquired_f = _resourcesAcquired;
	//	float _resourcesNeeded_f = _resourcesNeeded;

	//	float f = (_resourcesAcquired_f / _resourcesNeeded_f) * 100.0f;

	//	_percentComplete = Mathf.RoundToInt(f);
	//}

	protected virtual void ManageMaterial()
	{
		if (_meshObjects.Count > 0)
		{
			//_meshRenderer.materials[0].SetColor("_Color", new Color(1, 1, 1, 1.0f * _percentComplete));

			//Color c = _meshRenderer.material.color;
			Color c = _meshObjects[0].GetComponent<MeshRenderer>().material.color;
			//float alpha = BBBStatics.Map(_percentComplete, 0.0f, 75.0f, 0.0f, 2.0f, true);
			float alpha = BBBStatics.Map(_percentComplete, 0.0f, 100.0f, 0.02f, 2.0f, true);
			Color next = new Color(c.r, c.g, c.b, alpha); // 100.0f
			//_meshRenderer.material.SetColor("_Color", next);

			for (int i = 0; i < _meshObjects.Count; ++i)
			{
				_meshObjects[i].GetComponent<MeshRenderer>().material.SetColor("_Color", next);
			}

			//_meshRenderer.materials[0].SetColor("_EmissionColor", new Color(0, 1, 1, 1.0f * _percentComplete));
		}
	}

	protected virtual void CheckForComplete()
	{
		if (_percentComplete >= 100.0f)
		{
			if (_currDrone != null) _currDrone.GetComponent<AIConsDroneScript>().CurrConsSite = null;

		    _consPlatform.GetComponent<ConsPlatformScript>().BuildObject(_archetype); // Attaches the new object to the cons platform


			//Destroy(gameObject);
			BeginDestroy();
		}
	}

	public GameObject Archetype
	{
		get { return _archetype; }
		set { _archetype = value; }
	}

	public ConsPlatformScript ConsPlatform
	{
		get { return _consPlatform; }
		set { _consPlatform = value; }
	}

	public GameObject CurrDrone
	{
		get { return _currDrone; }
		set { _currDrone = value; }
	}

	//public Vector3[] GetVertsInLocalSpace()
	//{
	//	return _meshFilter.mesh.vertices;
	//}

	public Vector3 GetNextVert(int skipNum, int offset)
	{
		_currItrVert += (skipNum + 1) + offset;
		if (_currItrVert >= _localSpaceVertices.Count)
		{
			_currItrVert = 0;
		}

		return transform.position + _localSpaceVertices[_currItrVert];
	}

	public int CurrItrVert
	{
		get { return _currItrVert; }
		set { _currItrVert = value; }
	}

	public int ResourcesAcquired
	{
		get { return _resourcesAcquired; }
		set { _resourcesAcquired = value; }
	}

	public int ResourcesNeeded
	{
		get { return _resourcesNeeded; }
		set { _resourcesNeeded = value; }
	}

	public Team Team
	{
		get { return _team; }
		set { _team = value; }
	}

	public List<Vector3> LocalSpaceVertices { get { return _localSpaceVertices; } set { _localSpaceVertices = value; } }

	public void AddResourcesToCSite(int res)
	{
		ResourcesAcquired += res;
		ResourcesAcquired = Mathf.Clamp(ResourcesAcquired, 0, ResourcesNeeded); // Necessary ?
	}

	public void BuildObject(GameObject archetype)
	{
		//TODO: Add special fx

		GameObject go = Instantiate(archetype, transform.position, transform.rotation);

		TrainGameObjScript tgo = go.GetComponent<TrainGameObjScript>();
		tgo._team = _currDrone.GetComponent<TrainGameObjScript>()._team;
		tgo.SetAllChildrenToMatchTeam();

		if (_consPlatform != null)
		{
			AIDynamicObjScript ados = go.GetComponent<AIDynamicObjScript>();
			if (ados == null)
			{
				// Not an AIDynamicObjScript -- can attach to cons platform

				go.transform.parent = _consPlatform.transform;

				TurretScriptParent tsp = go.GetComponent<TurretScriptParent>();
				if (tsp != null)
				{
					tsp.ConnectedParentObj = _consPlatform.gameObject;
				}
			}
		}
	}

	public virtual void BeginDestroy()
	{
		for (int i = 0; i < _meshObjects.Count; ++i)
		{
			Destroy(_meshObjects[i]);
		}

		Destroy(gameObject);
	}
}