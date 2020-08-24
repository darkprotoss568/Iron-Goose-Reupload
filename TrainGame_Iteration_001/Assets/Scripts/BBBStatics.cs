using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEngine;

class BBBStatics
{
	//https://answers.unity.com/questions/599393/angles-from-quaternionvector-problem.html
	public static float GetSignedAngle(Quaternion A, Quaternion B, Vector3 axis)
	{
		float angle = 0f;
		Vector3 angleAxis = Vector3.zero;
		(B * Quaternion.Inverse(A)).ToAngleAxis(out angle, out angleAxis);
		if (Vector3.Angle(axis, angleAxis) > 90f)
		{
			angle = -angle;
		}
		return Mathf.DeltaAngle(0f, angle);
	}

	public static float Map(float Value, float In1, float In2, float Out1, float Out2, bool bClamp)
	{
		// Clamp input values (reactivated 7-4-18)
		if (bClamp)
		{
			if (In2 > In1) Value = Mathf.Clamp(Value, In1, In2);
			else if (In2 < In1) Value = Mathf.Clamp(Value, In2, In1);
		}

		// Perform the map
		float result = Out1 + (Value - In1) * (Out2 - Out1) / (In2 - In1);

		// Clamp output values
		if (bClamp)
		{
			if (Out2 > Out1) return Mathf.Clamp(result, Out1, Out2);
			else if (Out2 < Out1) return Mathf.Clamp(result, Out2, Out1);
		}

		return result;
	}

	public static GameObject GetClosestGOFromListToVec(List<GameObject> inList, Vector3 toVec)
	{
		GameObject closestGO = null;
		float closestGODist = float.PositiveInfinity;

		for (int i = 0; i < inList.Count; ++i)
		{
			// 3D version
			//float dist = Vector3.Distance(inList[i].transform.position, toVec);
			// 2D version
			float dist = GetDistance2D(inList[i].transform.position, toVec);
			if (dist < closestGODist)
			{
				closestGO = inList[i];
				closestGODist = dist;
			}
		}

		return closestGO;
	}
	public static GameObject GetClosestGOFromListToVec(List<GameObject> inList, Vector3 toVec, float withinRange) // Overloaded
	{
		List<GameObject> inRangeList = new List<GameObject>();

		for (int i = 0; i < inList.Count; ++i)
		{
			// 3D version
			//float dist = Vector3.Distance(inList[i].transform.position, toVec);
			// 2D version
			float dist = GetDistance2D(inList[i].transform.position, toVec);
			if (dist <= withinRange)
			{
				inRangeList.Add(inList[i]);
			}
		}

		return GetClosestGOFromListToVec(inRangeList, toVec);
	}

	public static GameObject GetFarthestGOFromListToVec(List<GameObject> inList, Vector3 toVec)
	{
		GameObject farthestGO = null;
		float farthestGODist = 0.0f;

		for (int i = 0; i < inList.Count; ++i)
		{
			// 3D version
			float dist = Vector3.Distance(inList[i].transform.position, toVec);
			// 2D version
			//float dist = GetDistance2D(inList[i].transform.position, toVec);
			if (dist > farthestGODist)
			{
				farthestGO = inList[i];
				farthestGODist = dist;
			}
		}

		return farthestGO;
	}

	//

	public static Vector3 GetClosestVecFromListToVec(List<Vector3> inList, Vector3 toVec)
	{
		Vector3 closestVec = Vector3.zero;
		float closestVecDist = float.PositiveInfinity;

		for (int i = 0; i < inList.Count; ++i)
		{
			float dist = Vector3.Distance(inList[i], toVec);

			if (dist < closestVecDist)
			{
				closestVec = inList[i];
				closestVecDist = dist;
			}
		}

		return closestVec;
	}

	public static int GetClosestVecIndexFromListToVec(List<Vector3> inList, Vector3 toVec)
	{
		int idx = 0;
		float closestVecDist = float.PositiveInfinity;

        int count = inList.Count;
        for (int i = 0; i < count; ++i)
		{
			float dist = Vector3.Distance(inList[i], toVec);

			if (dist < closestVecDist)
			{
				closestVecDist = dist;
				idx = i;
			}
		}

		return idx;
	}

	//

	//https://answers.unity.com/questions/1032673/how-to-get-0-360-degree-from-two-points.html
	public static float FindDegree(int x, int y)
	{
		float value = ((Mathf.Atan2(x, y) / Mathf.PI) * 180f);
		if (value < 0) value += 360f;

		return value;
	}

	//https://forum.unity.com/threads/camera-worldtoscreenpoint-bug.85311/
	public static Vector2 WorldToScreenPointProjected(Camera camera, Vector3 worldPos)
	{
		Vector3 camNormal = camera.transform.forward;
		Vector3 vectorFromCam = worldPos - camera.transform.position;
		float camNormDot = Vector3.Dot(camNormal, vectorFromCam);
		if (camNormDot <= 0)
		{
			// we are behind the camera forward facing plane, project the position in front of the plane
			Vector3 proj = (camNormal * camNormDot * 1.01f);
			worldPos = camera.transform.position + (vectorFromCam - proj);
		}

		return RectTransformUtility.WorldToScreenPoint(camera, worldPos);
	}

	//https://forum.unity.com/threads/camera-worldtoscreenpoint-bug.85311/
	public static Vector3 ScreenPointEdgeClamp(Vector2 screenPos, float edgeBuffer) //, out float angleDeg)
	{
		// Take the direction of the screen point from the screen center to push it out to the edge of the screen
		// Use the shortest distance from projecting it along the height and width
		Vector2 screenCenter = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);
		Vector2 screenDir = (screenPos - screenCenter).normalized;
		float angleRad = Mathf.Atan2(screenDir.x, screenDir.y);
		float distHeight = Mathf.Abs((screenCenter.y - edgeBuffer) / Mathf.Cos(angleRad));
		float distWidth = Mathf.Abs((screenCenter.x - edgeBuffer) / Mathf.Cos(angleRad + (Mathf.PI * 0.5f)));
		float dist = Mathf.Min(distHeight, distWidth);
		//angleDeg = angleRad * Mathf.Rad2Deg;
		return screenCenter + (screenDir * dist);
	}

	public static Vector3 VXZ(Vector3 inVec)
	{
		return new Vector3(inVec.x, 0.0f, inVec.z);
	}

	public static Vector3 BetweenAt(Vector3 v1, Vector3 v2, float pointPercentage)
	{
		return v1 * pointPercentage + (1 - pointPercentage) * v2;
	}
	//public static Vector2 BetweenAt(Vector2 v1, Vector2 v2, float pointPercentage) // Overloaded
	//{
	//	return v1 * pointPercentage + (1 - pointPercentage) * v2;
	//}

	public static bool Is3DVecOnScreen(Vector3 vec)
	{
		float edgeLeeway = 0.0f;

		Vector3 screenPos = Camera.main.WorldToScreenPoint(vec);
		Vector2 sp2D = new Vector2(screenPos.x - (Screen.width / 2), screenPos.y - (Screen.height / 2));

		if (Mathf.Abs(sp2D.x) < ((Screen.width / 2) - edgeLeeway) && Mathf.Abs(sp2D.y) < ((Screen.height / 2) - edgeLeeway))
		{
			return true;
		}

		return false;
	}

	public static Vector2 GetPointOnBezierCurve(Vector2 startPnt, Vector2 ctrlPnt, Vector2 endPnt, float t)
	{
		Vector2 result = Vector2.zero;

		result.x = (1 - t) * (1 - t) * startPnt.x + 2 * (1 - t) * t * ctrlPnt.x + t * t * endPnt.x;
		result.y = (1 - t) * (1 - t) * startPnt.y + 2 * (1 - t) * t * ctrlPnt.y + t * t * endPnt.y;

		return result;
	}
	public static Vector3 GetPointOnBezierCurve(Vector3 startPnt, Vector3 ctrlPnt, Vector3 endPnt, float t) // Overloaded Vector3 version
	{
		Vector3 result = Vector3.zero;

		result.x = (1 - t) * (1 - t) * startPnt.x + 2 * (1 - t) * t * ctrlPnt.x + t * t * endPnt.x;
		result.z = (1 - t) * (1 - t) * startPnt.z + 2 * (1 - t) * t * ctrlPnt.z + t * t * endPnt.z;

		result.y = startPnt.y;

		return result;
	}

    //

    /// These were all moved out of TurretScriptParent on 11-4-18 now that turrets are no longer the only unit type that can attack targets (KamiBombs)

	public static TrainGameObjScript ChooseClosestTarget(Vector3 toPos, float detectionRange, WorldScript ws, Team ourTeam, TurretScriptParent turretScript, string unitType = "Undefined") 
	{
		List<GameObject> possibleTargets = GetPossibleTargets(detectionRange, ws, ourTeam, toPos, turretScript.TurretCanShootGround, turretScript.TurretCanShootAir);
        
		if (possibleTargets.Count > 0 && turretScript != null && turretScript._maxRotAngle < 360)
		{

            List<GameObject> turrets = new List<GameObject>();
            List<GameObject> silos = new List<GameObject>();
            List<GameObject> carriages = new List<GameObject>();
            List<GameObject> locomotive = new List<GameObject>();
            List<GameObject> enemies = new List<GameObject>();

            for (int i = 0; i < possibleTargets.Count; ++i)
            {
                if (turretScript.IsObjWithinMaxTargetingAngle(possibleTargets[i]))
                {
                    if (possibleTargets[i].tag == "Turret")
                    {
                        turrets.Add(possibleTargets[i]);
                    }
                    else if (possibleTargets[i].tag == "ResourceSilo")
                    {
                        silos.Add(possibleTargets[i]);
                    }
                    else if (possibleTargets[i].tag == "Carriage")
                    {
                        carriages.Add(possibleTargets[i]);
                    }
                    else if (possibleTargets[i].tag == "Locomotive")
                    {
                        locomotive.Add(possibleTargets[i]);
                    }
                    else
                    {
                        if (possibleTargets[i].tag != "ResourceDrone") {
                            enemies.Add(possibleTargets[i]);
                        }
                    }
                }
            }

            possibleTargets = turrets.Any() ?
                turrets : silos.Any() ?
                silos : carriages.Any() ?
                carriages : locomotive.Any() ?
                locomotive : enemies;
        }

        if (possibleTargets.Any())
        {
            GameObject chosenTarget = BBBStatics.GetClosestGOFromListToVec(possibleTargets, toPos);
            if (chosenTarget != null)
            {
                return chosenTarget.GetComponent<TrainGameObjScript>();
            }
            else
            {
                ChooseClosestTarget(toPos, detectionRange, ws, ourTeam, turretScript, unitType); // Recur until we find something to attack
            }
        }

        return null;
    }

   /* public static TrainGameObjScript ChooseRandomTarget(Vector3 ourPos, float detectionRange, WorldScript ws, Team ourTeam, bool bTurretCanShootGround, bool bTurretCanShootAir)
	{
		List<GameObject> possibleTargets = GetPossibleTargets(detectionRange, ws, ourTeam, ourPos, bTurretCanShootGround, bTurretCanShootAir);

		// Get random target (should separate and override)
		if (possibleTargets.Count > 0)
		{
			return possibleTargets[RandInt(0, possibleTargets.Count - 1)].transform.gameObject.GetComponent<TrainGameObjScript>();
		}

		return null;
	}
    */
	// Get all objects that can be targeted (has TrainGameObjScript component)
	public static List<GameObject> GetPossibleTargets(float detectionRange, WorldScript ws, Team ourTeam, Vector3 ourPos, bool bTurretCanShootGround, bool bTurretCanShootAir)
	{
		//TrainGameObjScript[] allObjs = (TrainGameObjScript[])FindObjectsOfType(typeof(TrainGameObjScript));
		//List<TrainGameObjScript> allObjs = new List<TrainGameObjScript>((TrainGameObjScript[])FindObjectsOfType(typeof(TrainGameObjScript)));
		List<TrainGameObjScript> allObjs = ws.GetAllTGOsInWorld();

		// Create a list of potential targets
		List<GameObject> possibleTargets = new List<GameObject>();

		// Check within allObjs list targets that can be targetted by AI and not on the same team as the current turret
		for (int i = 0; i < allObjs.Count; ++i)
		{
			if (allObjs[i]._bCanBeAITarget && allObjs[i]._maxHealth >= 0 && !allObjs[i].bDamageAmntFrozen) // Don't target invincible units [16-4-18]
			{
				if (((ourTeam == Team.Enemy && allObjs[i]._team == Team.Friendly) || (ourTeam == Team.Friendly && allObjs[i]._team == Team.Enemy)) && allObjs[i].IsTargettableByTurretType(bTurretCanShootGround, bTurretCanShootAir)) 
				{
					// Check if the distance of between the turret and the target is within detection range
					if(CheckGameObjectColliderWithinRange(ourPos, detectionRange, allObjs[i].gameObject))
						possibleTargets.Add(allObjs[i].gameObject);

				}
			}
		}

		return possibleTargets;
	}

    public static Vector3 Pos(GameObject go) // Get the commSocket pos or tform pos if there is no commSocket
	{
		if (go == null) return Vector3.zero;

		Vector3 p = go.transform.position;
		TrainGameObjScript tgo = TGO(go);
		if (tgo != null && tgo.CommSocketObj != null)
		{
			p = tgo.CommSocketObj.transform.position;
		}
		return p;
	}
	public static Vector3 Pos(TrainGameObjScript tgo) // Overloaded
	{
		Vector3 p = tgo.transform.position;
		if (tgo.CommSocketObj != null)
		{
			p = tgo.CommSocketObj.transform.position;
		}
		return p;
	}

	public static TrainGameObjScript TGO(GameObject go)
	{
		if (go == null) return null;

		TrainGameObjScript tgo = go.GetComponent<TrainGameObjScript>();
		if (tgo != null)
		{
			return tgo;
		}
		return null;
	}

	public static int WrapAround(int val, int max)
	{
		if (val <= max) return val; // Nothing to do here

		while (val > max)
		{
			val -= max;
			if (val <= max) return val;
		}

		return 0;
	}
	public static float WrapAround(float val, float max) // Overloaded
	{
		if (val <= max) return val; // Nothing to do here

		while (val > max)
		{
			val -= max;
			if (val <= max) return val;
		}

		return 0;
	}

	public static float WrapAroundT2(float val, float max)
	{
		while (val > max)
		{
			val -= max;
			if (val <= max) return val;
		}

		while (val < 0)
		{
			val += max;
			if (val >= 0) return val;
		}

		return 0;
	}

	public static float WrapAroundT3(float val, float min, float max)
	{
		while (val > max)
		{
			val -= (max - min);
		}

		while (val < min)
		{
			val += (max - min);
		}

		return val;
	}

	public static Vector3 GetVert(int index, List<Vector3> verts, Transform endObjTForm)
	{
		index = WrapAround(index, verts.Count - 1);

		Vector3 final = endObjTForm.position + (endObjTForm.rotation * verts[index]);

		return final;
	}

	public static AudioClip RandomlyPickAudioClip(List<AudioClip> acs)
	{
		List<AudioClip> validACs = new List<AudioClip>();
		for (int i = 0; i < acs.Count; ++i)
		{
			if (acs[i] != null) validACs.Add(acs[i]);
		}

		return validACs[RandInt(0, validACs.Count - 1)];
	}

	public static void DrawDebugCube(Vector3 pos, float size, Color colour, float time)
	{
		size /= 2;

		//time = 1.0f; // Test

		Vector3 top1 = new Vector3(size, size, size);
		Vector3 top2 = new Vector3(-size, size, size);
		Vector3 top3 = new Vector3(-size, size, -size);
		Vector3 top4 = new Vector3(size, size, -size);

		Vector3 btm1 = new Vector3(size, -size, size);
		Vector3 btm2 = new Vector3(-size, -size, size);
		Vector3 btm3 = new Vector3(-size, -size, -size);
		Vector3 btm4 = new Vector3(size, -size, -size);

		Debug.DrawLine(pos + top1, pos + top2, colour, time);
		Debug.DrawLine(pos + top2, pos + top3, colour, time);
		Debug.DrawLine(pos + top3, pos + top4, colour, time);
		Debug.DrawLine(pos + top4, pos + top1, colour, time);

		Debug.DrawLine(pos + btm1, pos + btm2, colour, time);
		Debug.DrawLine(pos + btm2, pos + btm3, colour, time);
		Debug.DrawLine(pos + btm3, pos + btm4, colour, time);
		Debug.DrawLine(pos + btm4, pos + btm1, colour, time);

		Debug.DrawLine(pos + top1, pos + btm1, colour, time);
		Debug.DrawLine(pos + top2, pos + btm2, colour, time);
		Debug.DrawLine(pos + top3, pos + btm3, colour, time);
		Debug.DrawLine(pos + top4, pos + btm4, colour, time);
	}

	// TODO: Verify that this works properly [16-4-18]
	public static List<GameObject> GetAllChildObjs(GameObject ofGO) // Regardless of depth in the hierarchy
	{
		List<GameObject> allChildren = new List<GameObject> { ofGO };

		bool bFoundANewOne = true;
		while (bFoundANewOne)
		{
			bFoundANewOne = false;

			for (int i = 0; i < allChildren.Count; ++i)
			{
				for (int j = 0; j < allChildren[i].transform.childCount; ++j)
				{
					Transform ct = allChildren[i].transform.GetChild(j);
					if (ct.gameObject != null && !allChildren.Contains(ct.gameObject))
					{
						allChildren.Add(ct.gameObject);
						bFoundANewOne = true;
					}
				}
			}
		}

		return allChildren;
	}

	private static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

	// Return a random integer between a min and max value.
	public static int RandInt(int min, int max) // Min inclusive, Max inclusive
	{
		if (min == max) return min;

		uint scale = uint.MaxValue;
		while (scale == uint.MaxValue)
		{
			// Get four random bytes.
			byte[] four_bytes = new byte[4];
			rng.GetBytes(four_bytes);

			// Convert that into an uint.
			scale = BitConverter.ToUInt32(four_bytes, 0);
		}

		// Add min to the scaled difference between max and min.
		return (int)(min + (max - min) * (scale / (double)uint.MaxValue));
	}

	public static double RandDbl(double min, double max) // Min inclusive, Max inclusive
	{
		if (min == max) return min;

		uint scale = uint.MaxValue;
		while (scale == uint.MaxValue)
		{
			// Get four random bytes.
			byte[] four_bytes = new byte[4];
			rng.GetBytes(four_bytes);

			// Convert that into an uint.
			scale = BitConverter.ToUInt32(four_bytes, 0);
		}

		// Add min to the scaled difference between max and min.
		return (min + (max - min) * (scale / (double)uint.MaxValue));
	}

	public static float RandFlt(float min, float max) // Min inclusive, Max inclusive
	{
		return (float)RandDbl(min, max);
	}

	public static float AngleDiff(float a1, float a2)
	{
		float d = a2 - a1;
		while (d < -180) d += 360;
		while (d > 180) d -= 360;
		return d;
	}

	public static Vector2 RotateByAngle(Vector2 originalPoint, float degrees)
	{
		float radianAngle = degrees * Mathf.Deg2Rad;
		float sine = Mathf.Sin(radianAngle);
		float cosine = Mathf.Cos(radianAngle);
		Vector2 result = new Vector2(originalPoint.x * cosine + originalPoint.y * sine, originalPoint.y * cosine - originalPoint.x * sine);
		return result;
	}

    public static Vector3 RotateByAngleOnXZPlane(Vector3 originalVector, float degrees)
    {
        float radianAngle = degrees * Mathf.Deg2Rad;
        float sine = Mathf.Sin(radianAngle);
        float cosine = Mathf.Cos(radianAngle);
        Vector3 result = new Vector3(originalVector.x * cosine + originalVector.z * sine, originalVector.y, originalVector.z * cosine - originalVector.x * sine);
        return result;
    }
	// Raycast all and then sort the results in order of distance
	public static bool CheckIfRaycastAllHitsObjFirst(Ray r, float range, List<GameObject> ignoredObjs, GameObject CheckIfHitObj)
	{
		if (CheckIfHitObj == null) return false;

		// Check if CheckIfHitObj is the first thing hit -- RaycastAll returns hits in random order so we need to sort them by distance first

		List<RaycastHit> hitsInOrder = GetAllRaycastAllHitsInDistOrder(r, range, ignoredObjs);

		if (hitsInOrder.Count > 0)
		{
			//Debug.DrawLine(hitsInOrder[0].point, hitsInOrder[0].point + new Vector3(0, 6, 0), Color.red);

			//? Note: CurrClosestHit.transform returns the parent's transform, not the child's. Use CurrClosestHit.collider.transform to get the hit child's transform.
			if (hitsInOrder[0].transform.gameObject == CheckIfHitObj) // Is the given object the first thing hit?
			{
				//Debug.DrawLine(hitsInOrder[0].point, hitsInOrder[0].point + new Vector3(0, 12, 0), Color.cyan);
				return true;
			}
		}

		return false;
	}

	// Raycast all and then sort the results in order of distance
	public static List<RaycastHit> GetAllRaycastAllHitsInDistOrder(Ray r, float range, List<GameObject> ignoredObjs)
	{
		RaycastHit[] hits = Physics.RaycastAll(r, range);
		//, LayerMask.NameToLayer("Default"));
		//xDebug.DrawRay(r.origin, r.direction * range, Color.magenta);
		//Debug.DrawLine(r.origin, r.origin + new Vector3(0, 5, 0), Color.yellow);

		List<RaycastHit> hitsNotInOrder = new List<RaycastHit>(hits);
		List<RaycastHit> hitsInOrder = new List<RaycastHit>();

		// Recursively get the closest hit until we run out of hits
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

			hitsNotInOrder.Remove(CurrClosestHit);

			if (ignoredObjs != null && ignoredObjs.Contains(CurrClosestHit.transform.gameObject)) continue;

			hitsInOrder.Add(CurrClosestHit);
		}

		if (hitsInOrder.Count > 0)
		{
			return hitsInOrder;
		}

		return new List<RaycastHit>();
	}

	public static Vector3 MoveVecIn2DDir(float distance, float direction, Vector3 v)
	{
		direction = direction * Mathf.Deg2Rad;

		v.x = v.x + Mathf.Cos(direction) * distance;
		v.z = v.z + Mathf.Sin(direction) * distance;

		return v;
	}

	public static Vector3 GetRandomOffsetOnGround(Vector3 from, float maxOffset, float minDistAway)
	{
		for (int i = 0; i < 1000; ++i) // To avoid chance of infinite loops with while
		{
			Vector3 v = from + new Vector3(RandFlt(-maxOffset, maxOffset), 0.0f, RandFlt(-maxOffset, maxOffset));
			Vector3 hit = Vector3.zero;
			if (CheckForGround(v, 20.0f, out hit))
			{
				float dist = Vector3.Distance(from, hit);
				if (dist > minDistAway)
				{
					return hit;
				}
			}
		}

		return from;
	}

	public static Vector3 GetRandomOffsetOnGroundInDirection(Vector3 from, Quaternion direction, float maxOffset, float minDistAway)
	{
		for (int i = 0; i < 1000; ++i) // To avoid chance of infinite loops with while
		{
			//Vector3 v = from + new Vector3(RandFlt(-maxOffset, maxOffset), 0.0f, RandFlt(-maxOffset, maxOffset));
			Vector3 v = from + (direction * new Vector3(0, 0, RandFlt(0.0f, maxOffset)));

			Vector3 hit = Vector3.zero;
			if (CheckForGround(v, 20.0f, out hit))
			{
				float dist = Vector3.Distance(from, hit);
				if (dist > minDistAway)
				{
					return hit;
				}
			}
		}

		return from;
	}

	public static bool CheckForGround(Vector3 at, float startHeight, out Vector3 hitPoint)
	{
		hitPoint = Vector3.zero;

		Ray r = new Ray { origin = at + new Vector3(0, startHeight, 0), direction = Vector3.down };
		List<RaycastHit> rch = GetAllRaycastAllHitsInDistOrder(r, startHeight * 2.0f, null); // startHeight

		// If any of the hits are on a 'ground' layer object
		for (int i = 0; i < rch.Count; ++i)
		{
			if (rch[i].transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
			{
				hitPoint = rch[i].point;
				return true;
			}
		}

		return false;
	}

	public static Vector3 CheckForGroundV(Vector3 at, float startHeight)
	{
		Ray r = new Ray { origin = at + new Vector3(0, startHeight, 0), direction = Vector3.down };
		List<RaycastHit> rch = GetAllRaycastAllHitsInDistOrder(r, startHeight * 2.0f, null); // startHeight

		// If any of the hits are on a 'ground' layer object
		for (int i = 0; i < rch.Count; ++i)
		{
			if (rch[i].transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
			{
				return rch[i].point;
			}
		}

		return at;
	}

	public static Vector3 CheckForGroundV_V2(Vector3 at, float startHeight, out Vector3 normal)
	{
		normal = Vector3.zero;

		Ray r = new Ray { origin = at + new Vector3(0, startHeight, 0), direction = Vector3.down };
		List<RaycastHit> rch = GetAllRaycastAllHitsInDistOrder(r, startHeight * 2.0f, null); // startHeight

		// If any of the hits are on a 'ground' layer object
		for (int i = 0; i < rch.Count; ++i)
		{
			if (rch[i].collider != null)
			{
				normal = rch[i].normal;

				//Debug.Log("normal: " + normal);

				return rch[i].point;
			}
		}

		return at;
	}

	public static float GetAngleOnPlaneToFrom(Vector3 to, Vector3 from)
	{
		float angle = Vector3.Angle(Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized, Vector3.ProjectOnPlane(from - to, Vector3.up).normalized);
		Vector3 crossA = Vector3.Cross(Vector3.ProjectOnPlane(Vector3.forward, Vector3.right).normalized, Vector3.ProjectOnPlane(from - to, Vector3.up).normalized);
		if (crossA.y < 0) angle *= -1;

		return angle;
	}

	public static List<Vector3> MakeVectorGrid(Vector3 atLoc, int points, float distBetween)
	{
		List<Vector3> grid = new List<Vector3>();

		if (points < 4) return grid; // We need at least 4 points
		if (Mathf.Sqrt(points) % 1 != 0) return grid; // The number of points must have a whole-number square root (such as 4[2] or 9[3])

		int sqrt = (int)Mathf.Sqrt(points);

		float fNumberOfTraces = sqrt;

		float CurrXPos = fNumberOfTraces;
		float CurrYPos = fNumberOfTraces;

		for (int i = 0; i < points; ++i)
		{
			float CurrXPosB = Map(CurrXPos, 0.0f, fNumberOfTraces, -(fNumberOfTraces / 2), (fNumberOfTraces / 2), true);
			float CurrYPosB = Map(CurrYPos, 0.0f, fNumberOfTraces, -(fNumberOfTraces / 2), (fNumberOfTraces / 2), true);

			Vector3 nextPnt = atLoc + new Vector3((distBetween * CurrXPosB) - (distBetween / 2), 0.0f, (distBetween * CurrYPosB) - (distBetween / 2));
			grid.Add(nextPnt);

			CurrXPos--;

			if (CurrXPos < 1)
			{
				CurrYPos--;
				CurrXPos = fNumberOfTraces; // Reset
			}
		}

		return grid;
	}

	public static List<Vector3> MakeVectorGrid_OnGround(Vector3 atLoc, int points, float distBetween)
	{
		List<Vector3> list = MakeVectorGrid(atLoc, points, distBetween);

		for (int i = 0; i < list.Count; ++i)
		{
			list[i] = CheckForGroundV(list[i], 20.0f);
		}

		return list;
	}

	public static float GetDistance2D(Vector3 a, Vector3 b)
	{
		Vector3 c = new Vector3(a.x, 0, a.z);
		Vector3 d = new Vector3(b.x, 0, b.z);
		float result = Vector3.Distance(c, d);
		return result;
	}

	public static List<TrainGameObjScript> GetAllTargetsInBlastRadius(Vector3 center, float radius, Team team, bool bFriendlyDamageEnabled, TrainGameObjScript ignoredCenterTarget)
	{
		List<TrainGameObjScript> result = new List<TrainGameObjScript>();
		//Consider using LayerMasks
		Collider[] collidersWithinRange = Physics.OverlapSphere(center, radius);
		for (int i = 0; i < collidersWithinRange.Length; i++)
		{
			TrainGameObjScript tgoScript = collidersWithinRange[i].gameObject.GetComponent<TrainGameObjScript>();
			if (tgoScript != null)
			{
				if (ignoredCenterTarget != tgoScript)
				{
					if (bFriendlyDamageEnabled)
					{
						if (tgoScript._maxHealth > 0)
							result.Add(tgoScript);
					}
					else
					{
						if (tgoScript._team != team && tgoScript._maxHealth > 0)
						{
							result.Add(tgoScript);
						}
					}
				}
			}
		}

		return result;
	}

	public static int RoundUpToNearestMultiple(int NumToRound, int Multiple)
	{
		if (Multiple == 0) return NumToRound;

		int Remainder = Mathf.Abs(NumToRound) % Multiple;

		if (Remainder == 0) return NumToRound;
		if (NumToRound < 0) return -(Mathf.Abs(NumToRound) - Remainder);

		return NumToRound + Multiple - Remainder;
	}

	public static int RoundUpToNearestMultiple(float NumToRound, int Multiple)
	{
		return RoundUpToNearestMultiple(Mathf.RoundToInt(NumToRound), Multiple);
	}

	public static Vector3 V3(Vector2 V2)
	{
		return new Vector3(V2.x, 0.0f, V2.y);
	}

	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
	{
		return Quaternion.Euler(angles) * (point - pivot) + pivot;
	}

	public static float GetDistance2DToObjCollider(Vector3 point, GameObject obj)
	{
		Collider coll = obj.GetComponent<Collider>();

		return GetDistance2D(point, coll.ClosestPointOnBounds(point));
	}
	public static bool CheckGameObjectColliderWithinRange(Vector3 point, float range, GameObject obj)
	{
		Collider coll = obj.GetComponent<Collider>();
		if (coll != null)
		{
			if (GetDistance2DToObjCollider(point, obj) <= range)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return false;
		}
	}

	//? FAULTY - Returns incorrect results
	//https://stackoverflow.com/questions/217578/how-can-i-determine-whether-a-2d-point-is-within-a-polygon
	public static bool IsPointInPolygon(Vector2 p, List<Vector2> polygon)
	{
		double minX = polygon[0].x;
		double maxX = polygon[0].x;
		double minY = polygon[0].y;
		double maxY = polygon[0].y;
		for (int i = 1; i < polygon.Count; ++i)
		{
			Vector2 q = polygon[i];
			minX = Math.Min(q.x, minX);
			maxX = Math.Max(q.x, maxX);
			minY = Math.Min(q.y, minY);
			maxY = Math.Max(q.y, maxY);
		}

		if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY)
		{
			return false;
		}

		// http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
		bool inside = false;
		for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = ++i)
		{
			if ((polygon[i].y > p.y) != (polygon[j].y > p.y) &&
				  p.x < (polygon[j].x - polygon[i].x) * (p.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
			{
				inside = !inside;
			}
		}

		return !inside; // '!' Added by BBB
	}

	//? NOT FUNCTIONAL
	public static bool IsPointInPolygon_T2(Vector2 p, List<Vector2> polygon)
	{
		// Compute the oriented sum of angles between the point p and each of the polygon apices. 
		// If the total oriented angle is 360 degrees, the point is inside. If the total is 0, the point is outside.

		float ang = 0.0f;

		float lastAng = ang;
		for (int i = 0; i < polygon.Count; ++i)
		{
			lastAng = ang;
			ang += AngleTo_T2(p, polygon[i]);
			ang -= lastAng;
		}

		//Debug.Log("ang" + ang);

		if (ang == 360.0f) return true;

		if (ang == 0.0f) return false;

		return false;
	}

	public static bool IsPointInPolygon_T3(Vector2 p, List<Vector2> polygon)
	{
		List<float> vertx = new List<float>();
		List<float> verty = new List<float>();

		for (int i = 0; i < polygon.Count; ++i)
		{
			vertx.Add(polygon[i].x);
			verty.Add(polygon[i].y);
		}

		return pnpoly(polygon.Count, vertx, verty, p.x, p.y);
	}

	// Mathetmatical ray-cast - runs across the polygon and switches 'c' whenever it intersects with the interior of the polygon (I believe)
	//nvert: Number of vertices in the polygon. Whether to repeat the first vertex at the end has been discussed in the article referred above.
	//vertx, verty: Arrays containing the x- and y-coordinates of the polygon's vertices.
	//testx, testy: X- and y-coordinate of the test point.
	public static bool pnpoly(int nvert, List<float> vertx, List<float> verty, float testx, float testy)
	{
		bool c = false;
		for (int i = 0, j = nvert - 1; i < nvert; j = i++)
		{
			if (((verty[i] > testy) != (verty[j] > testy)) && (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
			{
				c = !c;
			}
		}
		return c;
	}

	public static float AngleTo(Vector2 from, Vector2 to)
	{
		Vector2 direction = to - from;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		if (angle < 0f) angle += 360f;
		return angle;
	}

	public static float AngleTo_T2(Vector2 from, Vector2 to)
	{
		Vector2 direction = to - from;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		if (angle < 0f) angle += 360f;
		return angle;
	}

	public static Vector3 AveragePos(List<Vector3> vecs)
	{
		Vector3 result = Vector3.zero;
		for (int i = 0; i < vecs.Count; ++i)
		{
			result += vecs[i];
		}
		return result / vecs.Count;
	}

	public static Vector3 GetFarthestVec(Vector3 from, List<Vector3> vecs)
	{
		Vector3 farthest = from;
		float farthestDist = 0.0f;

		for (int i = 0; i < vecs.Count; ++i)
		{
			float dist = Vector3.Distance(from, vecs[i]);
			if (dist > farthestDist)
			{
				farthestDist = dist;
				farthest = vecs[i];
			}
		}

		return farthest;
	}

	public static int TimesNumInNum(float numIn, float num)
	{
		int count = 0;

		while (num > 0)
		{
			num -= numIn;
			++count;
		}

		return count;
	}

	public static AudioSource PlayClipAtPoint_BBB(AudioClip audioClip, Vector3 pos, float volume, float pitch)
	{
		GameObject go = new GameObject("TempAudio");
		AudioSource audioSource = go.AddComponent<AudioSource>();
		go.transform.position = pos;
		audioSource.clip = audioClip;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.Play();
		UnityEngine.Object.Destroy(go, audioClip.length);
		return audioSource; 
	}

    public static bool CheckPositionTooFarFromTrain(Vector3 pos, LocomotiveScript locomotive, bool isReferencePointMidTrain, float distance)
    {
        if (isReferencePointMidTrain)
        {
            // If isReferencePointMidTrain is true, use the center point of the train as the center
            // Check 2D distance between pos and the center point of the train
            if (GetDistance2D(pos, locomotive.TrainMidPoint) > distance)
            {
                return true;
            }
            else
                return false;
        }
        else
        {
            // if isReferencePointMidTrain is false, use the locomotive as the center/reference point instead
            // Check 2D distance between pos and the locomotive's position
            if (GetDistance2D(pos, locomotive.gameObject.transform.position) > distance)
            {
                return true;
            }
            else
                return false;
        }
    }

    public static Mesh CreateFieldOfViewConeMesh(Vector3 centerVector, float angle)
    {
        Mesh newMesh = new Mesh();
        Vector3[] vertices = new Vector3[(int)angle+2];
        int[] newTriangles = new int[(int)angle * 3];
        Vector3[] normals = new Vector3[vertices.Length];
        //Vector2[] uvs = new Vector2[vertices.Length];
        // The first vertex is Vector3(0, 0, 0)
        vertices[0] = Vector3.zero;
        
        // The second vertex is the center vector, rotated by -angle/2 (to the left side)
        vertices[1] = RotateByAngleOnXZPlane(centerVector, -angle / 2);

        for (int i = 2; i < vertices.Length; i++)
        {
            vertices[i] = RotateByAngleOnXZPlane(vertices[i - 1], 1);
        }

        newTriangles[0] = 1;
        newTriangles[1] = 2;
        newTriangles[2] = 0;
        for (int i = 3; i < newTriangles.Length; i += 3)
        {
            newTriangles[i] = newTriangles[i-2];
            newTriangles[i+1] = newTriangles[i-2]+1;
            newTriangles[i + 2] = 0;
        }

        int length = normals.Length;
        for (int i = 0; i < length; i++)
        {
            normals[i].Set(0, 1, 0);
        }

        newMesh.vertices = vertices;
        newMesh.triangles = newTriangles;
        //newMesh.normals = normals;
        
        return newMesh;
    }

    public static float GetTimeScaleIndependentDelta()
    {
        float result = Time.deltaTime;
        if (Time.timeScale != 0)
        {
            result /= Time.timeScale;
        }

        return result;
    }
}