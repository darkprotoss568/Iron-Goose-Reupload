using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsPlatformScript : TrainGameObjScript
{
	private GameObject _fixture;

	private Color _lightColourClear = new Color(0, 1, 0, 1);
	private Color _lightColourFilled = new Color(1, 0, 0, 1);

	//private MeshRenderer _meshRenderer;

	public bool _bOnlyAllowBuildFixtures = true;
	public bool _bOnlyAllowBuildMobile = false;
	//
	//public GameObject[] _consArchetypesStatic;
	//
	public List<GameObject> _consArchetypes; // [Michael, 30-5-18]

	public GameObject _constructionMenu;

	//private CarriageScript _parentCarriage; 
	//public CarriageScript ParentCarriage { get { return _parentCarriage; } }
	public CarriageScript _parentCarriage { get; private set; }


	public override void Start()
	{
		base.Start();

		_maxHealth = -1; // Invincible
		_bCanBeAITarget = false;

		_fixture = null;

		//_meshRenderer = GetComponent<MeshRenderer>();

		_worldScript.GenerateArcheList(this);
		//if (_meshRenderer != null)
		//{
		//	_meshRenderer.materials[1].color = lightColourClear;
		//}

		_parentCarriage = transform.parent.GetComponent<CarriageScript>();
	}

	public override void Update()
	{
		if (PauseMenu.isPaused) return;

		base.Update();

		ManageConstruction();

		//ManageLightColour();

		//GetSideOfCarriageOn(); /// TEST
	}

	public string GetSideOfCarriageOn()
	{
		if (_parentCarriage != null)
		{
			if (transform.localPosition.x > 0.0f)
			{
				//Debug.DrawLine(transform.position, transform.position + new Vector3(0, 10, 0), Color.red, Time.deltaTime);
				return "left";
			}

			if (transform.localPosition.x < 0.0f)
			{
				//Debug.DrawLine(transform.position, transform.position + new Vector3(0, 10, 0), Color.yellow, Time.deltaTime);
				return "right";
			}
		}

		//Debug.DrawLine(transform.position, transform.position + new Vector3(0, 10, 0), Color.cyan, Time.deltaTime);
		return "centre";
	}

	public void ManageLightColour()
	{
		if (_meshRenderer != null)
		{
			if (_fixture != null)
			{
				if (_meshRenderer.materials.Length > 1) _meshRenderer.materials[1].SetColor("_EmissionColor", _lightColourFilled);
				//_meshRenderer.materials[1].SetColor("_Color", _lightColourFilled);
			}
			else
			{
				if (_meshRenderer.materials.Length > 1) _meshRenderer.materials[1].SetColor("_EmissionColor", _lightColourClear);
				//_meshRenderer.materials[1].SetColor("_Color", _lightColourClear);
			}
		}
	}

	private void ManageConstruction()
	{
		if (_bIsSelected && !_worldScript.ConstructionManager.bIsConsMenuOpen && _fixture == null)
		{
			_worldScript.ConstructionManager.OpenConstructionMenu(this);
		}
	}

    public ConeRangeProjectorScript CreateRangeProjector()
    {
        ConstructionManagerScript consManager = _worldScript.ConstructionManager;
        GameObject projector = Instantiate(consManager._rangeProjectorPrefab, transform.position, transform.rotation, transform);
        projector.transform.localPosition = new Vector3(0, 3, 0);
        ConeRangeProjectorScript result = projector.GetComponent<ConeRangeProjectorScript>();
        projector.SetActive(false);
        return result;
    }

    public void BeginBuildObject(GameObject archetype)
	{
		int _resourcesNeeded = BBBStatics.TGO(archetype).BuildCost;
		if (_worldScript.GameplayScript.PlayerResources >= _resourcesNeeded)
		{
            _bCanBeSelectedByPlayer = false;
			int cost = archetype.GetComponent<TrainGameObjScript>().BuildCost;
			_worldScript.GameplayScript.SubtractResources(cost);

			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._resourcesSpent += cost;

			AIDynamicObjScript ados = archetype.GetComponent<AIDynamicObjScript>();
			if (ados != null)
			{
				// Build the object directly
				BuildObject(archetype);

				// Building an object directly costs here -- cons sites cost later -- no longer the case as of 31-5-18
				//_worldScript.GameplayScript.SubtractResources(archetype.GetComponent<TrainGameObjScript>().BuildCost);
			}
			else
			{
				//_worldScript.GameplayScript.SubtractResources(archetype.GetComponent<TrainGameObjScript>().BuildCost); // [Michael, 31-5-18]

				///

				/// Check if the cons platform is on the left or right of the train

				Quaternion rot = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

				string side = GetSideOfCarriageOn();
				if (side == "left")
				{
					rot = transform.rotation;
				}
				if (side == "right")
				{
					rot = transform.rotation;
				}

				///

				// Create a cons site for the object so cons drones can build it
				GameObject consSite = Instantiate(Resources.Load("DefaultConsSite"), transform.position, rot, transform) as GameObject; // transform.rotation
				consSite.GetComponent<ConsSite>().Archetype = archetype;
				consSite.GetComponent<ConsSite>().ConsPlatform = this;

				//consSite.transform.parent = transform;

				//consSite.transform.localRotation = rot;

				_fixture = consSite;
			}
		}
	}

	public void BuildObject(GameObject archetype)
	{
		ConsSite csite = null;
		if (_fixture != null) csite = _fixture.GetComponent<ConsSite>();

		if (_fixture == null || csite != null)
		{
			//TODO: Add special fx

			Quaternion rot = transform.rotation;
			if (csite != null) rot = csite.transform.rotation;

			GameObject go = Instantiate(archetype, transform.position, rot);

			TrainGameObjScript tgo = go.GetComponent<TrainGameObjScript>();
			tgo._team = Team.Friendly; // Anything we build is friendly
			tgo.SetAllChildrenToMatchTeam();

			AIDynamicObjScript ados = go.GetComponent<AIDynamicObjScript>();
			if (ados == null)
			{
				// Not an AIDynamicObjScript -- can attach to this platform

				go.transform.parent = transform;
				_fixture = go;

				Module tsp = go.GetComponent<Module>();
				if (tsp != null)
				{
					tsp.ConnectedParentObj = gameObject;
				}
			}
		}
	}

	public GameObject Fixture
	{
		get { return _fixture; }
	}

	public List<GameObject> GetConstructionArchetypesInList()
	{
		//return new List<GameObject>(_consArchetypesStatic);
		return _consArchetypes; // [Michael, 30-5-18]
	}


}
