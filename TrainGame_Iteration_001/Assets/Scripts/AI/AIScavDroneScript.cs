using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DroneState
{
    FollowTrain,
    MoveToResource,
    PickUp,
    BackToSilo,
    DropOff
}

public class AIScavDroneScript : AIFXDroneScript
{
    private int _pathingDestNum;

    private GameObject _currResource;
    private DroneState _currState;
    private bool _bChunkNearby = false;
    public float _maxResourceSearchRange;

    private Vector3 _lastResourcePos;
    private Vector3 _lastLocoPos;

    private float _pickupDistXZ;
    //private float _pickupDistY;         

    private GameObject _pickUpFXArchetype;
    private AudioClip _pickUpSFX;

    private float _resourcePickupTimeout_time = 3.0f;
    private float _resourcePickupTimeout_curr = 0.0f;
    private float _dropOffTime;
    private float _dropOffTime_curr;
    private int _framesSinceDroppingOff;    

    public override void Start()
    {
        _specFX_archetype = Resources.Load("FX/ArcBeam002") as GameObject;        
        _pickUpFXArchetype = Resources.Load("FX/FX_001") as GameObject;
        _pickUpSFX = Resources.Load("Sounds/pickUpRes1") as AudioClip;

        base.Start();
        _team = Team.Friendly; // All scav drones must be friendlies        
        _currState = DroneState.FollowTrain;
        _currSilo = null;
        _currResource = null;
        _heldResources = 0;
        _maxHeldResources = 30;
        _maxResourceSearchRange = 100.0f;
        _lastResourcePos = Vector3.zero;
        _lastLocoPos = Vector3.zero;

        _pickupDistXZ = 2.5f; /// 1.0f until 28-7-18 - Mike
        //_pickupDistY = 10.0f; /// 6.0f until 28-7-18 - Mike

        _rotationRate = 4.0f;
        _maxFlightSpeed = 0.5f; // Now much lower due to MoveToDestination() not setting velocity
        _dropOffTime = 0.05f; /// Now per-resource
		_dropOffTime_curr = 0.0f;
        _framesSinceDroppingOff = 0;

        _worldScript.AllResourceDrones.Add(gameObject);
    }

    public new void FixedUpdate()
    {
        if (PauseMenu.isPaused) return;

        base.FixedUpdate();        
        StateManager();

        if (_framesSinceDroppingOff > 2)
        {
            _dropOffTime_curr = 0.0f; // Reset
        }

        ++_framesSinceDroppingOff;

        // End of Update()
    }

    private void StateManager()
    {
        switch (_currState)
        {
            case DroneState.FollowTrain:                                
                if (_heldResources > 0 && _worldScript.GameplayScript.PlayerResources < _worldScript.GameplayScript.PlayerMaxResources) // Player no longer has max resources
                {
                    _currState = DroneState.BackToSilo;
                    break;
                }
                if (_bChunkNearby && _heldResources < _maxHeldResources) // Chunk nearby
                {
                    _currState = DroneState.MoveToResource;
                    break;
                }
                FollowTrain(); // Follow a random path near the train                
                break;

            case DroneState.MoveToResource:
                // Reset _currSilo to detach in Pickup State
                if (_currSilo != null)
                {
                    _currSilo.GetComponent<ResourceModule>().CurrDrone = null;
                    _currSilo = null;
                }
                if (_heldResources >= _maxHeldResources) // Max held resources, proceed to drop resources
                {                                        
                    _currState = DroneState.BackToSilo;
                    break;
                }
                Dispatch(); // Find resources and move towards them
                if (CheckForPickup()) // Above the targeted resource, proceed to picking it up
                {                    
                    _currState = DroneState.PickUp;
                }
                if (_currResource == null && !_bChunkNearby) // The resource goes missing for good(resource collected)/bad(not good) reason and check if there is anymore resources nearby
                {
                    if (_heldResources > 0)
                    {                        
                        _currState = DroneState.BackToSilo;
                        break;
                    }
                    else
                    {                        
                        _currState = DroneState.FollowTrain;
                        break;
                    }
                }
                break;

            case DroneState.PickUp:
                PickUp(); // Manage pick up process
                if (_heldResources >= _maxHeldResources) // Resources full, head back to silo
                {                    
                    _currState = DroneState.BackToSilo;
                    break;
                }
                if (_currResource == null) // Picked up the resource
                {
                    if (_heldResources > 0)
                    {
                        if (_bChunkNearby && _heldResources < _maxHeldResources) // Can still collect more resources and there are chunk(s) nearby
                        {                            
                            _currState = DroneState.MoveToResource;                         
                        }
                        else // No more chunks or held max resources
                        {
                            _currState = DroneState.BackToSilo;
                        }
                    }
                    else // The targeted resource went missing for no reason. No good
                    {
                        _currState = DroneState.FollowTrain;
                    }
                    break;
                }                
                break;

            case DroneState.BackToSilo:
                _lastResourcePos = Vector3.zero;
                _lastLocoPos = Vector3.zero; // Reset the loco pos to zero so the drone will not blink to the loco
                if (_heldResources == 0) // Payload goes missing magically? Back to idle (just in case)
                {                    
                    _currState = DroneState.FollowTrain;
                }
                if (_worldScript.GameplayScript.PlayerResources < _worldScript.GameplayScript.PlayerMaxResources) // Move towards silo if player resource is not full yet
                {
                    ReturnToDropOff(); 
                    if(CheckForDropOff()) _currState = DroneState.DropOff; // Hovering above the silo, proceed to drop off state
                }
                else
                {
                    // The player already has a full load of resources                    
                    // No point in dropping off if the player resources is full
                    // Back to Idle untill player spend their resources
                    if (_currSilo != null)
                    {
                        _currSilo.GetComponent<ResourceModule>().CurrDrone = null;
                        _currSilo = null;
                    }
                    _currState = DroneState.FollowTrain;
                }
                break;

            case DroneState.DropOff:
                if (_heldResources == 0) // The entire payload is dropped off, back to idle
                {
                    _currState = DroneState.FollowTrain;
                    break;
                }
                if (_worldScript.GameplayScript.PlayerResources < _worldScript.GameplayScript.PlayerMaxResources) // Unloading resource
                {
                    ReturnToDropOff(); // So the drone will stay hovering above the silo
                    DropOff(); // Manage Drop off process
                }
                else // Player resources is full
                {
                    _currState = DroneState.FollowTrain;
                }
                break;

            default:
                print("Sum Sing Is Wong Here");
                _currState = DroneState.FollowTrain;
                break;
        }
    }

    /// <summary>
    /// Get random path beside the train for the drone pathing destination
    /// </summary>
    private void FollowTrain()
    {
        if (_worldScript.RandomisationScript.Get_RandTime003_AvailableThisTurn())
        {
            _pathingDestNum = BBBStatics.RandInt(0, _worldScript.GameplayScript.RandOffsetsFromLoco.Count - 2);
        }
        _pathingDestination = _worldScript.GameplayScript.RandOffsetsFromLoco[_pathingDestNum];
    }

	/// <summary>
	/// Looks for a resource to pick up
	/// If it finds one, send the drone to hover over it and pick it up
	/// If not, just resume following the train
	/// </summary>
	private void Dispatch()
	{
		if (_currResource == null)
		{			
			_lastResourcePos = Vector3.zero;
			FlightAltitude_Curr = FlightAltitude_Regular;

			if (_worldScript.RandomisationScript.Get_RandTime005_AvailableThisTurn()) // Every 0.1-0.3 seconds
			{
                if (_worldScript.AllChunks.Count > 0)
                {
                    List<GameObject> cgos = new List<GameObject>();

                    int count = _worldScript.AllChunks.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (_worldScript.AllChunks[i].GetComponent<ChunkScript>().CurrDrone == null) // Is something else already trying to pick it up?
                        {
                            float dist = Vector3.Distance(transform.position, _worldScript.AllChunks[i].transform.position);
                            if (dist <= _maxResourceSearchRange)
                            {
                                cgos.Add(_worldScript.AllChunks[i]);
                            }
                        }
                    }           

					if (cgos.Count > 0)
					{						
						_currResource = BBBStatics.GetClosestGOFromListToVec(cgos, transform.position);
						_currResource.GetComponent<ChunkScript>().CurrDrone = gameObject;
					}
				}
                if (_currResource == null) _bChunkNearby = false;
            }            
		}
		else
		{
			/// Try to make sure that we never get stuck on a single resource (which may be somehow inaccessible) - [Mike, 31-7-18]
			/// Will recheck for the closest resource every _resourcePickupTimeout_time seconds
			_resourcePickupTimeout_curr += Time.deltaTime;
			if (_resourcePickupTimeout_curr >= _resourcePickupTimeout_time)
			{
				_currResource = null;
				_resourcePickupTimeout_curr = 0.0f;
				return;
			}

			// Go to the resource			
			_pathingDestination = _currResource.transform.position;

			Vector2 v1 = new Vector2(transform.position.x, transform.position.z);
			Vector2 v2 = new Vector2(_currResource.transform.position.x, _currResource.transform.position.z);

			float distXZ = Vector2.Distance(v1, v2);

			if (distXZ < 5.0f)
			{				
				_worldAltitudeOverride = _currResource.transform.position.y + 6.0f;
			}			

			// Directly match speed with the resource -- just in-case it's on the train
			bool bMatchSpeed = true;
			if (bMatchSpeed && _lastResourcePos != Vector3.zero)
			{
				Vector3 diff = _currResource.transform.position - _lastResourcePos;

				//transform.Translate(diff); // Takes rotation into account
				transform.position += diff;
			}

			_lastResourcePos = _currResource.transform.position;
		}
	}

	/// <summary>
	/// Check if we're in range of our target resource so we can pick it up
	/// </summary>
	private bool CheckForPickup()
	{
		if (_currResource == null) return false;
		if (BBBStatics.TGO(_currResource) == null) return false;
		if (BBBStatics.TGO(_currResource).GetWorldScript() == null) return false; // Hasn't had time to actually get the WorldScript yet

		// Get the current Vector2 positions of the Scavenger Drone and the current locked-on resource
		Vector2 v1 = new Vector2(transform.position.x, transform.position.z);
		Vector2 v2 = new Vector2(_currResource.transform.position.x, _currResource.transform.position.z);

		// Calculate the 2D distance between the scavenger drone and the locked-on resource
		float distXZ = Vector2.Distance(v1, v2);
        // Calculate the altitude difference between the pick up position (6.0f above the resource position) and the current altitude.
        //float distY = Mathf.Abs(transform.position.y - (_currResource.transform.position.y + 6.0f));

        //print("Scav distXZ: " + distXZ);
        //print("Scav distY: " + distY);

        // Check if the drone is within pickup range of the locked-on resource
        if (distXZ <= _pickupDistXZ) // && distY <= _pickupDistY)
        {
            // Trigger pickup state
            return true;
        }
        else
        {
            return false;
        }
	}

	/// <summary>
	/// Manage the resource pickup process
	/// </summary>
	private void PickUp()
	{
		// Exit method if no resource locked on
		if (_currResource == null) return;
		if (BBBStatics.TGO(_currResource) == null) return;
		if (BBBStatics.TGO(_currResource).GetWorldScript() == null) return; // Hasn't had time to actually get the WorldScript yet

		ChunkScript chs = _currResource.GetComponent<ChunkScript>();

		// Check if the drone has taken enough time to pick up the resource chunk
		if (chs.PickUpCurrTime >= chs.PickUpTime)
		{
			// Add the picked up resource to the drone's pool.
			_heldResources += chs.ChunkResourceValue;
			_heldResources = Mathf.Clamp(_heldResources, 0, _maxHeldResources);

			_worldScript.ExplosionScript.Explosion(_currResource.transform.position, _pickUpFXArchetype, _pickUpSFX, 0, 0, null, "", 1.0f);

			// Destroy the locked-on resource in the game scene after pickup
			_currResource.GetComponent<TrainGameObjScript>().BeginDestroy(false, false);

			//Debug.DrawLine(transform.position, _currResource.transform.position, Color.cyan, 5.0f);
		}
		else
		{
			// Increment PickUpCurrTime
			chs.PickUpCurrTime += Time.deltaTime;

			// Manage pickup VFX
			MeshFilter mf = _currResource.GetComponent<MeshFilter>();
			if (mf != null)
			{
				List<Vector3> tempFXList = new List<Vector3>();
				for (int i = 0; i < _desiredSpecFXCount; ++i)
				{
					_specFX_vertIdx[i] += 3;

					Vector3 vertPos = BBBStatics.GetVert(_specFX_vertIdx[i], new List<Vector3>(mf.mesh.vertices), chs.transform);
					tempFXList.Add(vertPos);
				}

				RunFX(tempFXList);
			}
		}
	}

	/// <summary>
	/// Control the drone to return to a resource drop off point
	/// </summary>
	private void ReturnToDropOff()
	{
		// If no _currSilo has been designated, assign the nearest silo that has not been claimed to _currSilo
		if (_currSilo == null) _currSilo = GetNearestUnclaimedResourceSilo();

		if (_currSilo != null)
		{			
			// Lock the _currSilo to this Scavenger Drone
			if (_currSilo.GetComponent<ResourceModule>().CurrDrone != gameObject) _currSilo.GetComponent<ResourceModule>().CurrDrone = gameObject;

			//_pathingDestination = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().ResourceDropOffPoint.transform.position;
			// Set path destination
			_pathingDestination = _currSilo.transform.position;

			// Get the drop off altitude 6.0f above the silo's position
			_worldAltitudeOverride = _pathingDestination.y + 6.0f;

			// Directly match speed with the train
			if (_lastLocoPos != Vector3.zero)
			{
				transform.position += (_worldScript.LocomotiveObjectRef.transform.position - _lastLocoPos);
			}
			_lastLocoPos = _worldScript.LocomotiveObjectRef.transform.position;
		}
		else
		{
            // Couldn't find a silo - just follow the train
            _currState = DroneState.FollowTrain;
		}
	}

	/// <summary>
	/// Check if the player has reached the resource drop off position
	/// </summary>
	private bool CheckForDropOff()
	{
		if (_currSilo != null)
		{			
			//Vector3 dropOffPos = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().ResourceDropOffPoint.transform.position;
			Vector3 dropOffPos = _currSilo.transform.position;

			// Get the 2D positions of the Scavenger Drone and the drop off location
			Vector2 v1 = new Vector2(transform.position.x, transform.position.z);
			Vector2 v2 = new Vector2(dropOffPos.x, dropOffPos.z);

			// Calculate the 2D distance between the scavenger drone and the drop off location
			float distXZ = Vector2.Distance(v1, v2);
			// Calculate the altitude difference between the current altitude and the drop off position, which is 6.0f above the drop off location
			//float distY = Mathf.Abs(transform.position.y - (dropOffPos.y + 6.0f));

			//print("distXZ: " + distXZ);
			//print("distY: " + distY);

			// Check if the Scavenger Drone is within the range for resource drop off.
			if (distXZ <= _pickupDistXZ) // && distY <= _pickupDistY)
			{
				// Initiate the resource drop off process				
                return true;
			}
		}
        return false;
	}

	/// <summary>
	/// Manage the resource drop off process
	/// </summary>
	private void DropOff()
	{
		_framesSinceDroppingOff = 0;

		// Check if the Drone has finished dropping off resources
		if (_dropOffTime_curr < _dropOffTime)
		{
			// Increment the resource drop timer
			_dropOffTime_curr += Time.deltaTime;

			//GameObject rdp = _worldScript.LocomotiveObjectRef.GetComponent<LocomotiveScript>().ResourceDropOffPoint;
			GameObject rdp = _currSilo.transform.Find("ResourceDropOffPoint").gameObject;

			// Manage resource drop off special effects
			MeshFilter mf = rdp.GetComponent<MeshFilter>();
			if (mf != null)
			{
				List<Vector3> tempFXList = new List<Vector3>();
				for (int i = 0; i < _desiredSpecFXCount; ++i)
				{
					_specFX_vertIdx[i] += 3;

					Vector3 vertPos = BBBStatics.GetVert(_specFX_vertIdx[i], new List<Vector3>(mf.mesh.vertices), rdp.transform);
					tempFXList.Add(vertPos);
				}

				RunFX(tempFXList);
			}
		}
		else
		{
			//Add the resource to the player's pool while taking from the drone's pool.
			_worldScript.GameplayScript.AddResources(1);
			_heldResources -= 1;

			_worldScript.StatsRec.Initiate(); // Make sure it has been initiated
			_worldScript.StatsRec.Rec.CurrPt._resourcesGathered += 1;

			// Reset the resource drop off process timer
			_dropOffTime_curr = 0.0f;
		}
	}

    public bool ChunkNearby
    {
        get { return _bChunkNearby; }
        set { _bChunkNearby = value; }
    }

    public override void AITask_Idle()
    {
    }

    public override void AITask_FollowTrain()
    {
    }

    public override void AITask_Patrol()
    {
    }
}
