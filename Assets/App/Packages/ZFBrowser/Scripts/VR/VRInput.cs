//#define OCULUS_SDK //<-- define this in your project settings if you have the OVR API
#if UNITY_2017_2_OR_NEWER

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using ZenFulcrum.VR.OpenVRBinding;


namespace ZenFulcrum.EmbeddedBrowser.VR {

public enum InputAxis {
	MainTrigger,//main index finger trigger
	Grip,//Vive squeeze/Oculus hand trigger
	JoypadX, JoypadY,//touchpad/joystick x/y position
	Joypad,//touchpad/joystick click/press
	Application,//application meanu/start button
}

public enum JoyPadType {
	None,
	Joystick,
	TouchPad,
}


/// <summary>
/// Unity's VR input system is sorely lacking in usefulness.
/// We can finally get pose data in a more-or-less straightforward manner (InputTracking.GetNodeStates,
/// state.TryGetPosition, etc.), but getting the axis and buttons is an awful mess.
///
/// This page https://docs.unity3d.com/Manual/OpenVRControllers.html says we can access the inputs via
/// the input system! And, you can look at (of all things) the Joystick names to see which joystick
/// is a VR controller at runtime. But guess what? There's no Unity API to fetch the value of a given
/// axis on a given controller! Extra-stupid, right? You can define a specific input in the input menu, 
/// but that doesn't help when you switch to a different computer and the joystick numbers change!
/// Short of defining an "input" for every possible controller * every axis you use, there's not a 
/// way to get the controller axis inputs in a way that will work reliably across different machines.
///
/// (We can fetch buttons manually via Input.GetKey(KeyCode.Joystick1Button0 + buttonId + joystickIdx * 20),
/// but this don't address the axis issue at all.)
///
/// Anyway, this is a workaround. Around another Unity problem. Implemented by hooking into backend APIs directly.
/// </summary>
public class VRInput {
	private static VRInput impl;

	public static void Init() {
		if (impl == null) impl = GetImpl();
	}

	/// <summary>
	/// Returns the state of the given button/axis.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="axis"></param>
	/// <returns></returns>
	public static float GetAxis(XRNodeState node, InputAxis axis) {
		if (impl == null) impl = GetImpl();
		return impl.GetAxisValue(node, axis);
	}

	/// <summary>
	/// If the controller is capable, returns if (and sometimes how closely) the player is touching
	/// the given control.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="axis"></param>
	/// <returns></returns>
	public static float GetTouch(XRNodeState node, InputAxis axis) {
		if (impl == null) impl = GetImpl();
		return impl.GetTouchValue(node, axis);
	}

	protected virtual float GetAxisValue(XRNodeState node, InputAxis axis) {
		return 0;
	}

	protected virtual float GetTouchValue(XRNodeState node, InputAxis axis) {
		return 0;
	}

	private static Dictionary<ulong, JoyPadType> nodeTypes = new Dictionary<ulong, JoyPadType>();
	public static JoyPadType GetJoypadType(XRNodeState node) {
		JoyPadType ret;
		if (!nodeTypes.TryGetValue(node.uniqueID, out ret)) {
			ret = JoyPadType.None;

			if (impl == null) impl = GetImpl();
			var name = impl.GetNodeName(node);

			if (name.Contains("Oculus Touch Controller") || name.StartsWith("Oculus Rift CV1")) {
				//OpenVR gives us "Oculus Rift CV1 (Left Controller)" etc. where I wish it would mention the type of controller (Touch)
				ret = JoyPadType.Joystick;
			} else if (name.StartsWith("Vive Controller")) {
				ret = JoyPadType.TouchPad;
			} else {
				Debug.LogWarning("Unknown controller type: " + name);
			}

			nodeTypes[node.uniqueID] = ret;
		}
		return ret;
	}

	public virtual string GetNodeName(XRNodeState node) {
		return InputTracking.GetNodeName(node.uniqueID);
	}


//	public virtual JoyPadType JoypadTypeValue(XRNodeState node) { return JoyPadType.None; }

	private static VRInput GetImpl() {
		if (XRSettings.loadedDeviceName == "OpenVR") {
			return new OpenVRInput();
		} else if (XRSettings.loadedDeviceName == "Oculus") {
#if OCULUS_SDK
			return new OculusVRInput();
#else
			Debug.LogError("To use the Oculus API for input, import the Oculus SDK and define OCULUS_SDK");
			return new VRInput();
#endif

		} else {
			Debug.LogError("Unknown VR input system: " + XRSettings.loadedDeviceName);
			return new VRInput();
		}
	}
}

class OpenVRInput : VRInput {

	protected VRControllerState_t lastState;

	public static string GetStringProperty(uint deviceId, ETrackedDeviceProperty prop) {
		var  buffer = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
		ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;

		OpenVR.System.GetStringTrackedDeviceProperty(
			deviceId, prop,
			buffer, OpenVR.k_unMaxPropertyStringSize, ref err
		);

		if (err != ETrackedPropertyError.TrackedProp_Success) {
			throw new Exception("Failed to get property " + prop + " on  device " + deviceId + ": " + err);
		}

		return buffer.ToString();
	}

	public override string GetNodeName(XRNodeState node) {
		var deviceId = (uint)GetDeviceId(node);

		try {
			return GetStringProperty(deviceId, ETrackedDeviceProperty.Prop_ModelNumber_String);
		} catch (Exception ex) {
			Debug.LogError("Failed to get device name for device " + deviceId + ": " + ex.Message);
			return base.GetNodeName(node);
		}
	}


	protected void ReadState(XRNodeState node) {
		if (OpenVR.System == null) {
			Debug.LogWarning("OpenVR not active");
			lastState = default(VRControllerState_t);
			return;
		}

		var controllerId = GetDeviceId(node);
		if (controllerId < 0) {
			lastState = default(VRControllerState_t);
			return;
		}

		//Debug.Log("Id is " + controllerId);

		var res = OpenVR.System.GetControllerState(
			(uint)controllerId, ref lastState,
			(uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t))
		);

		if (!res) {
			Debug.LogWarning("Failed to get controller state");
		}
	}

	protected override float GetAxisValue(XRNodeState node, InputAxis axis) {
		ReadState(node);

		switch (axis) {
			case InputAxis.MainTrigger:
				return lastState.rAxis1.x;
			case InputAxis.Grip:
				return (lastState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_Grip)) != 0 ? 1 : 0;
			case InputAxis.JoypadX:
				return lastState.rAxis0.x;
			case InputAxis.JoypadY:
				return lastState.rAxis0.y;
			case InputAxis.Joypad:
				return (lastState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad)) != 0 ? 1 : 0;
			case InputAxis.Application:
				return (lastState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_ApplicationMenu)) != 0 ? 1 : 0;
			default:
				throw new ArgumentOutOfRangeException("axis", axis, null);
		}
	}

	protected override float GetTouchValue(XRNodeState node, InputAxis axis) {
		ReadState(node);

		switch (axis) {
			case InputAxis.Joypad:
				return (lastState.ulButtonTouched & (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad)) != 0 ? 1 : 0;
			default:
				return 0;
		}
	}

	private int GetDeviceId(XRNodeState node) {
		var targetRole = node.nodeType == XRNode.LeftHand ? ETrackedControllerRole.LeftHand : ETrackedControllerRole.RightHand;
		for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++) {
			var role = OpenVR.System.GetControllerRoleForTrackedDeviceIndex(i);
			if (role == targetRole) return (int)i;
		}

		return -1;
	}
}

#if OCULUS_SDK
class OculusVRInput : VRInput {

	protected override float GetAxisValue(XRNodeState node, InputAxis axis) {
		var controller = GetController(node);
		OVRPlugin.ControllerState4 state = OVRPlugin.GetControllerState4((uint)controller);

		switch (axis) {
			case InputAxis.MainTrigger:
				return controller == OVRInput.Controller.LTouch ? state.LIndexTrigger : state.RIndexTrigger;
			case InputAxis.Grip:
				return controller == OVRInput.Controller.LTouch ? state.LHandTrigger : state.RHandTrigger;
			case InputAxis.JoypadX:
			case InputAxis.JoypadY: {
				var joy = controller == OVRInput.Controller.LTouch ? state.LThumbstick : state.RThumbstick;
				return axis == InputAxis.JoypadX ? joy.x : joy.y;
			}
			case InputAxis.Joypad: {
				var buttonId = controller == OVRInput.Controller.LTouch ? 0x00000400 : 0x00000004; //see enum ovrButton_ in OVER_CAPI.h
				return (state.Buttons & buttonId) != 0 ? 1 : 0;
			}
			default:
				return 0;
		}
	}

	private OVRInput.Controller GetController(XRNodeState node) {
		switch (node.nodeType) {
			case XRNode.LeftHand:
				return OVRInput.Controller.LTouch;
			case XRNode.RightHand:
				return OVRInput.Controller.RTouch;
			default:
				return OVRInput.Controller.None;
		}
	}

	protected override float GetTouchValue(XRNodeState node, InputAxis axis) {
		//nothing touch-related is presently used for Oculus Touch controllers, so nothing is all we need here
		return 0;
	}
}
#endif

}

#endif