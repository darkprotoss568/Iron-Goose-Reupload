using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScript : TrainGameObjScript
{
	//protected is like private except accessible in child classes
	protected Vector3 LastTFormPos = new Vector3(0, 0, 0);
	protected List<GameObject> CollidingObjs = new List<GameObject>();
	protected CharacterController characterController;
	public float MoveSpeedMult = 1.0f;
	public GameObject MainMesh;
	protected Vector2 MoveInput = new Vector2();

	protected Vector3 MovementOverride = new Vector3(0, 0, 0);
	protected Vector3 RotationOverride = new Vector3(0, 0, 0);

	protected bool bIsPlayerChar = false;

	protected Vector3 InFrontPoint = new Vector3(0, 0, 0);

	public GameObject Torch;

	new public void Start()
	{
		base.Start();

		characterController = GetComponent<CharacterController>();

		if (!bIsPlayerChar && Torch == null)
		{
			print("Error: Torch == null -- CharacterScript -- an AI character");
		}
	}

	new public void Update()
	{
		base.Update();

		GetInFrontPoint();

		ManageMovement();
		ManageRotation();

		if (!bIsPlayerChar) ManageTorch();

		//

		AfterUpdate();
	}

	public void AfterUpdate()
	{
		//MovementOverride = new Vector3(0, 0, 0);

		LastTFormPos = transform.position; // Save it for the next frame

		//print("LastTFormPos:" + LastTFormPos + " at " + Time.time);
	}

	public void ManageTorch()
	{
		if (_worldScript.DayNightCycleScript.WorldState == WorldState.Night)
		{
			if (!Torch.activeInHierarchy) Torch.SetActive(true);
		}
		else if (_worldScript.DayNightCycleScript.WorldState == WorldState.Day)
		{
			if (Torch.activeInHierarchy) Torch.SetActive(false);
		}
	}

	protected void GetInFrontPoint()
	{
		/// Example of a rotated vector in Unity
		InFrontPoint = transform.position + (MainMesh.transform.rotation * new Vector3(0, 0, -2));
		//Debug.DrawLine(InFrontPoint, InFrontPoint + new Vector3(0, 2, 0), Color.yellow, Time.deltaTime, false); // Remember to turn on 'Gizmos' in game view!
	}

	public new void LateUpdate()
	{
		//ManageMovement();

		base.LateUpdate();
	}

	public void OnGUI() // For Debug Labels
	{

	}

	public void OnCollisionStay(Collision collision)
	{
		// Called every frame when we are colliding with something

		//print("OnCollisionStay(Collision collision)");

		//bCollidingWithAnything = true;

		CollidingObjs = new List<GameObject>(); // Empty/reinstatiate the list

		ContactPoint[] cps = collision.contacts;
		for (int i = 0; i < cps.Length; ++i)
		{
			if (!CollidingObjs.Contains(cps[i].otherCollider.gameObject))
			{
				CollidingObjs.Add(cps[i].otherCollider.gameObject);
			}
		}
	}

	void ManageRotation()
	{
		//print("------------------");

		//if (!bIsPlayerChar)
		//{
		//	print("LastTFormPos: " + LastTFormPos + " at " + Time.time);
		//	print("transform.position: " + transform.position + " at " + Time.time);
		//}

		if (LastTFormPos == transform.position) // We're not moving
		{
			if (RotationOverride == new Vector3(0, 0, 0)) // Not using rotation override
			{
				return;
			}
		}

		//if (!bIsPlayerChar) print("ManageRotation() called at [moving]: " + Time.time);

		float MovingAngle = Vector3.Angle(
			Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized,
			Vector3.ProjectOnPlane(LastTFormPos - transform.position, Vector3.up).normalized);

		Vector3 crossA = Vector3.Cross(
			Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized,
			Vector3.ProjectOnPlane(LastTFormPos - transform.position, Vector3.up).normalized);

		if (crossA.y < 0) MovingAngle *= -1;

		Vector3 CER = MainMesh.transform.rotation.eulerAngles; // Current Euler Rotation

		//Quaternion MovingDirection = Quaternion.Euler(0, MovingAngle, 0);

		if (MoveInput.x != 0 || MoveInput.y != 0 || !bIsPlayerChar) //? AI characters don't use MoveInput
		{
			if (MainMesh != null)
			{
				float AngleToUse = MovingAngle;

				if (RotationOverride != new Vector3(0,0,0))
				{
					AngleToUse = RotationOverride.y;
					RotationOverride = new Vector3(0, 0, 0);
				}

				//

				float DiffAngle = BBBStatics.GetSignedAngle(MainMesh.transform.rotation, Quaternion.Euler(0, AngleToUse, 0), Vector3.up);

				//float Mult = Mathf.Clamp01(Mathf.Abs(DiffAngle));
				float Mult = BBBStatics.Map(Mathf.Abs(DiffAngle), 45.0f, 0.0f, 1.0f, 0.0f, false);
				Mult = Mathf.Clamp01(Mult);

				//print("Mult: " + Mult);
				//print("DiffAngle: " + DiffAngle);

				float RotRate = 8.0f * Mult;

				// Closest so far - 2-3-18 @ 08:56
				//if (DiffAngle > 0)
				//	MainMesh.transform.rotation = Quaternion.Euler(0, CER.y - RotRate, 0);
				//else if (DiffAngle < 0)
				//	MainMesh.transform.rotation = Quaternion.Euler(0, CER.y + RotRate, 0);

				if (DiffAngle < 0)
					MainMesh.transform.rotation = Quaternion.Euler(0, CER.y - RotRate, 0);
				else if (DiffAngle > 0)
					MainMesh.transform.rotation = Quaternion.Euler(0, CER.y + RotRate, 0);
			}
			else
			{
				print("Error: MainMesh in PlayerScript is null!");
			}
		}
	}

	void ManageMovement()
	{
		//MoveInput.x = Input.GetAxis("Horizontal");
		//MoveInput.y = Input.GetAxis("Vertical");

		//transform.Translate(new Vector3(MoveInput.y * PlayerMoveSpeedMult, 0, MoveInput.x * PlayerMoveSpeedMult * -1.0f));

		// So diagonal speed isn't any faster ?
		Vector3 Movement = Vector3.ClampMagnitude(new Vector3(MoveInput.y * MoveSpeedMult, 0.0f, -MoveInput.x * MoveSpeedMult), MoveSpeedMult);

		//Vector3 Movement = new Vector3(MoveInput.y * MoveSpeedMult, 0.0f, -MoveInput.x * MoveSpeedMult);

		if (MovementOverride != new Vector3(0, 0, 0))
		{
			Movement = MovementOverride;
		}

		// The only actual use for characterController AFAIK
		//characterController.Move((Movement + Physics.gravity) * Time.deltaTime);
		characterController.SimpleMove(Movement);

		MovementOverride = new Vector3(0, 0, 0);
	}
}

