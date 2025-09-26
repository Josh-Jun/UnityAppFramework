#if UNITY_2017_2_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using ZenFulcrum.EmbeddedBrowser.VR;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Class for tracking (and optionally rendering) a tracked controller, usable for VR input to a browser.
/// 
/// Put two VRHand objects in the scene, one for each controller. Make sure they have the same transform parent
/// as the main camera so they will correctly move with the player.
/// (Also, make sure your main camera is centered on its local origin to start.)
/// 
/// If desired, you can also put some visible geometry under the VRHand. Set it as `visualization` and it will
/// move with the controller and disappear when untracked.
/// </summary>
public class VRBrowserHand : MonoBehaviour {

	[Tooltip("Which hand we should look to track.")]
	public XRNode hand = XRNode.LeftHand;

	[Tooltip("Optional visualization of this hand. It should be a child of the VRHand object and will be set active when the controller is tracking.")]
	public GameObject visualization;

	/// <summary>
	/// Are we currently tracking?
	/// </summary>
	public bool Tracked { get; private set; }
	/// <summary>
	/// Currently depressed buttons.
	/// </summary>
	public MouseButton DepressedButtons { get; private set; }
	/// <summary>
	/// How much we've scrolled since the last frame. Same units as Input.mouseScrollDelta.
	/// </summary>
	public Vector2 ScrollDelta { get; private set; }

	public string Name { get; private set; }

	private XRNodeState nodeState;

	public void OnEnable() {
		VRInput.Init();

		//VR poses update after LateUpdate and before OnPreCull
		Camera.onPreCull += UpdatePreCull;
		if (visualization) visualization.SetActive(false);
	}

	public void OnDisable() {
		// ReSharper disable once DelegateSubtraction
		Camera.onPreCull -= UpdatePreCull;
	}

	public virtual void Update() {
		ReadInput();
	}

	protected virtual void ReadInput() {
		DepressedButtons = 0;
		ScrollDelta = Vector2.zero;

		if (!nodeState.tracked) return;

		var leftClick = VRInput.GetAxis(nodeState, InputAxis.MainTrigger);
		if (leftClick > .9f) DepressedButtons |= MouseButton.Left;

		var rightClick = VRInput.GetAxis(nodeState, InputAxis.Grip);
		if (rightClick > .5f) DepressedButtons |= MouseButton.Right;

		switch (VRInput.GetJoypadType(nodeState)) {
			case JoyPadType.Joystick:
				ReadJoystick();
				break;
			case JoyPadType.TouchPad:
				ReadTouchpad();
				break;
		}
	}

	[Tooltip("How much we must slide a finger/joystick before we start scrolling.")] 
	public float scrollThreshold = .1f;

	[Tooltip(@"How fast the page moves as we move our finger across the touchpad.
Set to a negative number to enable that infernal ""natural scrolling"" that's been making so many trackpads unusable lately.")]
	public float trackpadScrollSpeed = .05f;
	[Tooltip("How fast the page moves as we scroll with a joystick.")]
	public float joystickScrollSpeed = 75f;

	private Vector2 lastTouchPoint;
	private bool touchIsScrolling;

	protected virtual void ReadTouchpad() {
		var touchPoint = new Vector2(VRInput.GetAxis(nodeState, InputAxis.JoypadX), VRInput.GetAxis(nodeState, InputAxis.JoypadY));

		var touchButton = VRInput.GetTouch(nodeState, InputAxis.Joypad) > .5f;

		if (touchButton) {
			var delta = touchPoint - lastTouchPoint;
			if (!touchIsScrolling) {
				if (delta.magnitude * trackpadScrollSpeed > scrollThreshold) {
					touchIsScrolling = true;
					lastTouchPoint = touchPoint;
				} else {
					//don't start updating the touch point yet
				}
			} else {
				ScrollDelta = new Vector2(-delta.x, delta.y) * trackpadScrollSpeed;
				lastTouchPoint = touchPoint;
			}
		} else {
			ScrollDelta = new Vector2();
			lastTouchPoint = touchPoint;
			touchIsScrolling = false;
		}
	}

	protected virtual void ReadJoystick() {
		ScrollDelta = new Vector2();

		var offset = new Vector2(
			-VRInput.GetAxis(nodeState, InputAxis.JoypadX), 
			VRInput.GetAxis(nodeState, InputAxis.JoypadY)
		);

		offset.x = Mathf.Abs(offset.x) > scrollThreshold ? offset.x - Mathf.Sign(offset.x) * scrollThreshold : 0;
		offset.y = Mathf.Abs(offset.y) > scrollThreshold ? offset.y - Mathf.Sign(offset.y) * scrollThreshold : 0;

		offset = offset * offset.magnitude * joystickScrollSpeed *  Time.deltaTime;

		ScrollDelta = offset;
	}

	private int lastFrame;
	private List<XRNodeState> states = new List<XRNodeState>();
	private bool hasTouchpad;

	private void UpdatePreCull(Camera cam) {
		if (lastFrame == Time.frameCount) return;
		lastFrame = Time.frameCount;

		InputTracking.GetNodeStates(states);

		for (int i = 0; i < states.Count; i++) {
			//Debug.Log("A thing: " + states[i].nodeType + " and " + InputTracking.GetNodeName(states[i].uniqueID));
			if (states[i].nodeType != hand) continue;

			nodeState = states[i];

			Vector3 pos;
			if (states[i].TryGetPosition(out pos)) transform.localPosition = pos;
			Quaternion rot;
			if (states[i].TryGetRotation(out rot)) transform.localRotation = rot;

			if (visualization) visualization.SetActive(Tracked = states[i].tracked);
		}
	}
}
}
#endif