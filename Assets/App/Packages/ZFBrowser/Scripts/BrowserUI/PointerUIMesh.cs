using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// A BrowserUI that tracks pointer interaction through a camera to a mesh of some sort.
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class PointerUIMesh : PointerUIBase {
	protected MeshCollider meshCollider;

	protected Dictionary<int, RaycastHit> rayHits = new Dictionary<int, RaycastHit>();

	[Tooltip("Which layers should UI rays collide with (and be able to hit)?")]
	public LayerMask layerMask = -1;

	public override void Awake() {
		base.Awake();
		meshCollider = GetComponent<MeshCollider>();
	}

	protected override Vector2 MapPointerToBrowser(Vector2 screenPosition, int pointerId) {
		var camera = viewCamera ? viewCamera : Camera.main;
		return MapRayToBrowser(camera.ScreenPointToRay(screenPosition), pointerId);
	}

	protected override Vector2 MapRayToBrowser(Ray worldRay, int pointerId) {
		RaycastHit hit;
		var rayHit = Physics.Raycast(worldRay, out hit, maxDistance, layerMask);

		//store hit data for GetCurrentHitLocation
		rayHits[pointerId] = hit;

		if (!rayHit || hit.collider.transform != meshCollider.transform) {
			//not aimed at it
			return new Vector3(float.NaN, float.NaN);
		} else {
			return hit.textureCoord;
		}
	}

	public override void GetCurrentHitLocation(out Vector3 pos, out Quaternion rot) {
		if (currentPointerId == 0) {
			//no pointer
			pos = new Vector3(float.NaN, float.NaN, float.NaN);
			rot = Quaternion.identity;
			return;
		}

		var hitInfo = rayHits[currentPointerId];

		//We need to know which way is up, so the cursor has the correct "up".
		//There's a couple ways to do this:
		//1. Use the barycentric coordinates and some math to figure out what direction the collider's
		//  v (from the uv) is getting bigger/smaller, then do some math to find out what direction
		//  that is in world space.
		//2. Just use the collider's local orientation's up. This isn't accurate on highly
		//  distorted meshes, but is much simpler to calculate.
		//For now, we use method 2.
		var up = hitInfo.collider.transform.up;

		pos = hitInfo.point;
		rot = Quaternion.LookRotation(-hitInfo.normal, up);
	}

}

}
