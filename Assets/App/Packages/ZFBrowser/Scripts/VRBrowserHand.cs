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
///
/// PointerUIBase.FeedVRPointers will find us and read our state out to browsers.
/// </summary>
public class VRBrowserHand : MonoBehaviour {

	[Tooltip("Which hand we should look to track.")]
	public XRNode hand = XRNode.LeftHand;

	[Tooltip("Optional visualization of this hand. It should be a child of the VRHand object and will be set active when the controller is tracking.")]
	public GameObject visualization;


	[Tooltip("How much we must slide a finger/joystick before we start scrolling.")]
	public float scrollThreshold = .1f;

	[Tooltip(@"How fast the page moves as we move our finger across the touchpad.
Set to a negative number to enable that infernal ""natural scrolling"" that's been making so many trackpads unusable lately.")]
	public float trackpadScrollSpeed = .05f;
	[Tooltip("How fast the page moves as we scroll with a joystick.")]
	public float joystickScrollSpeed = 75f;

	private Vector2 lastTouchPoint;
	private bool touchIsScrolling;

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

	private XRNodeState nodeState;
	private VRInput input;

	public void OnEnable() {
		VRInput.Init();
		input = VRInput.Impl;

		//VR poses update after LateUpdate and before OnPreCull
		Camera.onPreCull += UpdatePreCull;
		if (visualization) visualization.SetActive(false);
	}

	public void OnDisable() {
		// ReSharper disable once DelegateSubtraction
		Camera.onPreCull -= UpdatePreCull;
	}

	public virtual void Update() {
		if (Time.frameCount < 5) return;//give the SteamVR SDK a chance to start up
		ReadInput();
	}

	protected virtual void ReadInput() {
		DepressedButtons = 0;
		ScrollDelta = Vector2.zero;

		if (!nodeState.tracked) return;

		var leftClick = input.GetAxis(nodeState, InputAxis.LeftClick);
		if (leftClick > .9f) DepressedButtons |= MouseButton.Left;

		var middleClick = input.GetAxis(nodeState, InputAxis.MiddleClick);
		if (middleClick > .5f) DepressedButtons |= MouseButton.Middle;

		var rightClick = input.GetAxis(nodeState, InputAxis.RightClick);
		if (rightClick > .5f) DepressedButtons |= MouseButton.Right;


		var joyTypes = input.GetJoypadTypes(nodeState);
		if ((joyTypes & JoyPadType.Joystick) != 0) ReadJoystick();
		if ((joyTypes & JoyPadType.TouchPad) != 0) ReadTouchpad();
	}

	protected virtual void ReadTouchpad() {
		var touchPoint = new Vector2(
			input.GetAxis(nodeState, InputAxis.TouchPadX), input.GetAxis(nodeState, InputAxis.TouchPadY)
		);

		var touchButton = input.GetAxis(nodeState, InputAxis.TouchPadTouch) > .5f;

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
				ScrollDelta += new Vector2(-delta.x, delta.y) * trackpadScrollSpeed;
				lastTouchPoint = touchPoint;
			}
		} else {
			lastTouchPoint = touchPoint;
			touchIsScrolling = false;
		}
	}

	protected virtual void ReadJoystick() {
		var position = new Vector2(
			-input.GetAxis(nodeState, InputAxis.JoyStickX),
			input.GetAxis(nodeState, InputAxis.JoyStickY)
		);

		position.x = Mathf.Abs(position.x) > scrollThreshold ? position.x - Mathf.Sign(position.x) * scrollThreshold : 0;
		position.y = Mathf.Abs(position.y) > scrollThreshold ? position.y - Mathf.Sign(position.y) * scrollThreshold : 0;

		position = position * position.magnitude * joystickScrollSpeed * Time.deltaTime;

		ScrollDelta += position;
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

			var pose = input.GetPose(nodeState);
			transform.localPosition = pose.pos;
			transform.localRotation = pose.rot;

			if (visualization) visualization.SetActive(Tracked = nodeState.tracked);
		}
	}
}
}
#endif