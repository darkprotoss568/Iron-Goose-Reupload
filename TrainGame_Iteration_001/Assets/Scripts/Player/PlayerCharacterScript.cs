using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType
{
	Human,
	Monster
}

public partial class PlayerCharacterScript : CharacterScript
{
	public PlayerType playerType = PlayerType.Human;
	//private float PlayerMoveSpeedMult = 0.25f;
	//private bool bCollidingWithAnything = false;

	public Camera ourCamera;
	private Vector3 ourCameraInitialLocalPos;
	private Vector3 ourCameraInitialRotation;

	private Vector3 ourCameraOverheadLocalPos = new Vector3(0, 9.5f, 0); // Vector3(0, 9.5f, 0);
	private Vector3 ourCameraOverheadRotation = new Vector3(90, 90, 0); // Vector3(90, 90, 0);

	private float ourCameraLerpAmnt = 0.0f;
	public float ourCameraLerpSpeedMult = 2.0f;

	//public float ourCameraLerp_SlightDelayTime = 0.2f;

	//private float ourCameraLerp_SlightDelayTime_Add = 0.0f;
	//private float ourCameraLerp_SlightDelayTime_Sub = 0.0f;

	//private float ourCameraLerp_SlightDelayTime_Both = 0.0f;

	///private bool Last_bTraceHitPlayerObjFirst = false;

	//private Quaternion NextCamRot_Stored;
	//private Vector3 NextCamPos_Stored;

	new void Start()
	{
		bIsPlayerChar = true;

		base.Start(); // Call the super-class function

		if (ourCamera == null) print("Error: ourCamera is null -- PlayerScript");
		else
		{
			ourCameraInitialLocalPos = ourCamera.transform.localPosition;
			ourCameraInitialRotation = ourCamera.transform.rotation.eulerAngles;
		}

		//ourCameraLerp_SlightDelayTime_Add = ourCameraLerp_SlightDelayTime;
		//ourCameraLerp_SlightDelayTime_Sub = ourCameraLerp_SlightDelayTime;

		//ourCameraLerp_SlightDelayTime_Both = ourCameraLerp_SlightDelayTime;

		//NextCamRot_Stored = ourCamera.transform.rotation;
		//NextCamPos_Stored = ourCamera.transform.localPosition;
	}

	new void Update()
	{
		// Need to be before the base class function is called
		MoveInput.x = Input.GetAxis("Horizontal");
		MoveInput.y = Input.GetAxis("Vertical");

		base.Update(); // Call the super-class function

		ManageCamera();

		//print("CollidingObjs.Count: " + CollidingObjs.Count);

		//base.AfterUpdate();
	}

	//

	void ManageCamera()
	{
		bool bTraceHitPlayerObjFirst = false;

		Vector3 TraceFromPos = ourCamera.transform.position; // ourCamera.transform.position // NextCamPos_Stored
		Vector3 PlPos = transform.position;

		//
		List<GameObject> IgnoredObjs = new List<GameObject>();

		Ray rb = new Ray { origin = TraceFromPos, direction = PlPos - TraceFromPos };
		if (CheckIfRaycastAllHitsPlayerFirst(rb, IgnoredObjs)) // Test from the marker to the player
		{
			bTraceHitPlayerObjFirst = true;
		}

		//

		if (bTraceHitPlayerObjFirst) // Player is visible
		{
			//print("bTraceHitPlayerObjFirst == true: " + Time.time);

			//ourCamera.transform.localPosition = ourCameraInitialLocalPos;
			//ourCamera.transform.rotation = Quaternion.Euler(ourCameraInitialRotation);

			//if (ourCameraLerp_SlightDelayTime_Add <= 0.0f)
			//{
			//	ourCameraLerpAmnt -= Time.deltaTime * ourCameraLerpSpeedMult;
			//	ourCameraLerpAmnt = Mathf.Clamp01(ourCameraLerpAmnt);

			//ourCameraLerp_SlightDelayTime_Sub = ourCameraLerp_SlightDelayTime;
			//}

			//ourCameraLerp_SlightDelayTime_Add -= Time.deltaTime;

			//

			///

			/* Instead of moving the cam backwards the very frame that the player is hit, we need to take the cam's projected position if it were to move back and then run the trace from there.
			- If this trace doesn't hit the player, we don't move as we're already in the right place
			- If it does hit the player, we allow normal behaviour */
			float ourCameraLerpAmnt_Projection = ourCameraLerpAmnt - Time.deltaTime;
			ourCameraLerpAmnt_Projection = Mathf.Clamp01(ourCameraLerpAmnt_Projection);

			Vector3 ProjectedNextCamPos = transform.position + Vector3.Slerp(ourCameraInitialLocalPos, ourCameraOverheadLocalPos, ourCameraLerpAmnt_Projection);

			Ray rb2 = new Ray { origin = ProjectedNextCamPos, direction = PlPos - ProjectedNextCamPos };
			if (CheckIfRaycastAllHitsPlayerFirst(rb2, IgnoredObjs)) // Test from the marker to the player
			{
				ourCameraLerpAmnt -= Time.deltaTime * ourCameraLerpSpeedMult;
				ourCameraLerpAmnt = Mathf.Clamp01(ourCameraLerpAmnt);
			}

			///

			//ourCameraLerpAmnt -= Time.deltaTime * ourCameraLerpSpeedMult;
			//ourCameraLerpAmnt = Mathf.Clamp01(ourCameraLerpAmnt);
		}
		else // Player is not visible
		{
			//print("bTraceHitPlayerObjFirst == false: " + Time.time);

			//ourCamera.transform.localPosition = ourCameraOverheadLocalPos;
			//ourCamera.transform.rotation = Quaternion.Euler(ourCameraOverheadRotation);

			//if (ourCameraLerp_SlightDelayTime_Sub <= 0.0f)
			//{
			//	ourCameraLerpAmnt += Time.deltaTime * ourCameraLerpSpeedMult;
			//	ourCameraLerpAmnt = Mathf.Clamp01(ourCameraLerpAmnt);

			//ourCameraLerp_SlightDelayTime_Add = ourCameraLerp_SlightDelayTime;
			//}

			//ourCameraLerp_SlightDelayTime_Sub -= Time.deltaTime;

			//

			ourCameraLerpAmnt += Time.deltaTime * ourCameraLerpSpeedMult;
			ourCameraLerpAmnt = Mathf.Clamp01(ourCameraLerpAmnt);
		}

		Quaternion NextCamRot = Quaternion.Slerp( /// Slerp
			Quaternion.Euler(ourCameraInitialRotation),
			Quaternion.Euler(ourCameraOverheadRotation),
			ourCameraLerpAmnt);

		Vector3 NextCamPos = Vector3.Slerp( /// Slerp
			ourCameraInitialLocalPos,
			ourCameraOverheadLocalPos,
			ourCameraLerpAmnt);

		ourCamera.transform.rotation = NextCamRot;
		ourCamera.transform.localPosition = NextCamPos;

		///// Use the last frame's stored rot and pos
		//ourCamera.transform.rotation = NextCamRot_Stored;
		//ourCamera.transform.localPosition = NextCamPos_Stored;

		//NextCamRot_Stored = NextCamRot;
		//NextCamPos_Stored = NextCamPos;

		//print("ourCamera.transform.rotation.eulerAngles: " + ourCamera.transform.rotation.eulerAngles);
		//print("ourCamera.transform.localPosition: " + ourCamera.transform.localPosition);

		//

		///Last_bTraceHitPlayerObjFirst = bTraceHitPlayerObjFirst;
	}

	//

	new void LateUpdate()
	{
		base.LateUpdate(); // Call the super-class function


	}

	new void OnGUI() // For Debug Labels
	{
		base.OnGUI(); // Call the super-class function


	}

	//

	bool CheckIfRaycastAllHitsPlayerFirst(Ray r, List<GameObject> IgnoredObjs)
	{
		// Make sure that the player is the first thing hit -- remember, RaycastAll returns hits in random order - need to sort them by distance first

		RaycastHit[] hits = Physics.RaycastAll(r, 100.0f);

		List<RaycastHit> hitsNotInOrder = new List<RaycastHit>(hits);
		List<RaycastHit> hitsInOrder = new List<RaycastHit>();

		//! Recursively get the closest hit until we run out of hits
		while (hitsNotInOrder.Count > 0)
		{
			// Get the closest to the start point, add it to hitsInOrder and remove it from hitsNotInOrder

			RaycastHit CurrClosestHit = new RaycastHit();
			float CurrClosestHitDist = float.MaxValue;

			for (int j = 0; j < hitsNotInOrder.Count; ++j)
			{
				float HitDist = hitsNotInOrder[j].distance;
				if (HitDist < CurrClosestHitDist)
				{
					CurrClosestHitDist = HitDist;
					CurrClosestHit = hitsNotInOrder[j];
				}
			}

			//print("CurrClosestHit:" + CurrClosestHit.collider.gameObject.name);

			hitsNotInOrder.Remove(CurrClosestHit);

			if (!IgnoredObjs.Contains(CurrClosestHit.collider.gameObject))
			{
				hitsInOrder.Add(CurrClosestHit);
			}
		}

		if (hitsInOrder.Count > 0 && hitsInOrder[0].collider == GetComponent<CharacterController>()) // Is the player the first thing hit?
		{
			return true;
		}

		return false;
	}
}

