using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Simple no-nonsense FPS controller for use in demos. It's not very refined, but it's simple.
///
/// (The 5.x and newer controllers are pretty heavy to include for a demo.)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SimpleFPSController : MonoBehaviour {

	public float lookSpeed = 100;
	public float moveSpeed = 10;
	public float moveForce = 20000;
	public float jumpForce = 50;
	public float dampening = 2;

	private Vector3 bottom = new Vector3(0, -1, 0);

	private Camera head;
	private Rigidbody body;

	private float lookPitch;

	public void Awake() {
		head = GetComponentInChildren<Camera>();
		body = GetComponent<Rigidbody>();
	}

	public void Update() {
		//look
		var lookDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * lookSpeed;

		var deltaYaw = Quaternion.AngleAxis(lookDelta.x, Vector3.up);
		transform.localRotation *= deltaYaw;

		lookPitch += -lookDelta.y;
		lookPitch = Mathf.Clamp(lookPitch, -90, 90);

		head.transform.localRotation = Quaternion.Euler(lookPitch, 0, 0);

		//jump
		if (Input.GetButtonDown("Jump") && Grounded) {
			body.AddForce(-Physics.gravity.normalized * jumpForce, ForceMode.Impulse);
		}
	}

	public void FixedUpdate() {
		if (Time.frameCount < 5) return;

		var grounded = Grounded;

		if (grounded) body.drag = dampening;
		else body.drag = 0;

		var horizVel = body.velocity;
		horizVel.y = 0;

		//move
		var moveWish = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		if (moveWish.magnitude > 1) moveWish = moveWish.normalized;
		if (horizVel.magnitude > moveSpeed) return;

		//local to global
		moveWish = transform.TransformVector(moveWish);

		var force = moveWish * moveForce * Time.deltaTime;

		if (force.magnitude > 0) {
			if (!grounded) force *= .5f;
			body.AddForce(force, ForceMode.Force);
		}

	}

	public bool Grounded {
		get {
			var hits = Physics.SphereCastAll(new Ray(transform.position + bottom + transform.up * .01f, Physics.gravity.normalized), .1f, .1f);
			for (int i = 0; i < hits.Length; i++) {
				if (hits[i].rigidbody == body) continue;
				return true;
			}
			return false;
		}

	}

}

}