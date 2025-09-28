//#define OCULUS_SDK //<-- define this in your project settings if you have the OVR API
#if UNITY_2017_2_OR_NEWER

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using ZenFulcrum.EmbeddedBrowser.VR;
using ZenFulcrum.VR.OpenVRBinding;


namespace ZenFulcrum.EmbeddedBrowser.VR {

public enum InputAxis {
	LeftClick,//main trigger
	RightClick,//alt trigger
	MiddleClick,//alt alt trigger

	JoyStickX, JoyStickY,//joystick x/y position
	TouchPadTouch,//touchpad is touched
	TouchPadX, TouchPadY,//touchpad x/y position (only valid when TouchPadTouch > 0)
}

[Flags]
public enum JoyPadType {
	None = 0,
	Unknown = 1 << 1,
	Joystick = 1 << 2,
	TouchPad = 1 << 3,
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
/// 
/// 
/// 
/// ...and then there's SteamVR 2.x and its input system.
/// </summary>
public abstract class VRInput {
	public static VRInput Impl { private set; get; }

	public struct Pose {
		public Vector3 pos;
		public Quaternion rot;
	}

	public static void Init() {
		if (Impl == null) Impl = GetImpl();
	}


	public abstract float GetAxis(XRNodeState node, InputAxis axis);

	/// <summary>
	/// Returns where the hand is pointing right now. (z+ forward)
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public virtual Pose GetPose(XRNodeState node) {
		Pose ret = new Pose();
		node.TryGetPosition(out ret.pos);
		node.TryGetRotation(out ret.rot);
		return ret;
	}
	public virtual JoyPadType GetJoypadTypes(XRNodeState node) {
		return JoyPadType.None;
	}

	public virtual string GetNodeName(XRNodeState node) {
		return InputTracking.GetNodeName(node.uniqueID);
	}

	private static VRInput GetImpl() {
		if (XRSettings.loadedDeviceName == "OpenVR") {
			return new OpenVRInput();
		} else if (XRSettings.loadedDeviceName == "Oculus") {
			#if OCULUS_SDK
				return new OculusVRInput();
			#else
				Debug.LogError("To use the Oculus API for input, import the Oculus SDK, delete ZFBrowser/**.asmdef (2 files), and define OCULUS_SDK");
				return new NoVRInput();
			#endif
		} else {
			Debug.LogError("Unknown VR input system: " + XRSettings.loadedDeviceName);
			return new NoVRInput();
		}
	}
}

class NoVRInput : VRInput {
	public override float GetAxis(XRNodeState node, InputAxis axis) {
		return 0;
	}
}

partial class OpenVRInput : VRInput {
	enum InputMode {
		Direct,
		MappedActions,
	}

	/// <summary>
	/// Previous controller state (direct input mode)
	/// </summary>
	private VRControllerState_t direct_lastState;

	private ulong pointPose, leftClick, middleClick, rightClick;
	private ulong joystickScroll, touchPadScrollTouch, touchPadScrollPos;
	private ulong leftHand, rightHand;

	public OpenVRInput() {
		if (mode == InputMode.MappedActions) {
			if (OpenVR.Input == null) throw new ApplicationException("Cannot start VR input");

			pointPose = GetActionHandle(PointPose, "PointPose");

			leftClick = GetActionHandle(LeftClickAction, "LeftClickAction");
			middleClick = GetActionHandle(MiddleClickAction, "MiddleClickAction");
			rightClick = GetActionHandle(RightClickAction, "RightClickAction");

			joystickScroll = GetActionHandle(JoystickScrollAction, "JoystickScrollAction");
			touchPadScrollTouch = GetActionHandle(TouchpadScrollTouch, "TouchpadScrollTouch");
			touchPadScrollPos = GetActionHandle(TouchpadScrollPosition, "TouchpadScrollPosition");

			var res = OpenVR.Input.GetInputSourceHandle("/user/hand/left", ref leftHand);
			if (res != EVRInputError.None) throw new ApplicationException("Failed to find hand " + res);
			res = OpenVR.Input.GetInputSourceHandle("/user/hand/right", ref rightHand);
			if (res != EVRInputError.None) throw new ApplicationException("Failed to find hand "  + res);
		}
	}

	private ulong GetActionHandle(string handleName, string name) {
		if (string.IsNullOrEmpty(handleName)) return 0;

		ulong ret = 0;
		var res = OpenVR.Input.GetActionHandle(handleName, ref ret);
		if (res != EVRInputError.None) {
			throw new ApplicationException("Failed to set up VR action " + res);
		}
		return ret;
	}

	public static string GetStringProperty(uint deviceId, ETrackedDeviceProperty prop) {
		var  buffer = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
		ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;

		OpenVR.System.GetStringTrackedDeviceProperty(
			deviceId, prop,
			buffer, OpenVR.k_unMaxPropertyStringSize, ref err
		);

		if (err != ETrackedPropertyError.TrackedProp_Success) {
			throw new Exception("Failed to get property " + prop + " on device " + deviceId + ": " + err);
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


	protected void Direct_ReadState(XRNodeState node) {
		if (OpenVR.System == null) {
			Debug.LogWarning("OpenVR not active");
			direct_lastState = default(VRControllerState_t);
			return;
		}

		var controllerId = GetDeviceId(node);
		if (controllerId < 0) {
			direct_lastState = default(VRControllerState_t);
			return;
		}

		//Debug.Log("Id is " + controllerId);

		var res = OpenVR.System.GetControllerState(
			(uint)controllerId, ref direct_lastState,
			(uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t))
		);

		if (!res) {
			Debug.LogWarning("Failed to get controller state");
		}
	}

	private bool GetDigitalInput(ulong action, XRNodeState node) {
		InputDigitalActionData_t data = new InputDigitalActionData_t();

		var res = OpenVR.Input.GetDigitalActionData(
			action, ref data,
			(uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputDigitalActionData_t)),
			node.nodeType == XRNode.LeftHand ? leftHand : rightHand
		);
		if (res != EVRInputError.None) throw new ApplicationException("Failed to get digital input data " + node.nodeType + ": " + res);

		return data.bState;
	}

	private Vector2 GetVector2Input(ulong action, XRNodeState node) {
		InputAnalogActionData_t data = new InputAnalogActionData_t();

		var res = OpenVR.Input.GetAnalogActionData(
			action, ref data,
			(uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputAnalogActionData_t)),
			node.nodeType == XRNode.LeftHand ? leftHand : rightHand
		);
		if (res != EVRInputError.None) throw new ApplicationException("Failed to get vector input data " + node.nodeType + ": " + res);

		return new Vector2(data.x, data.y);
	}

	private Pose GetPoseInput(ulong action, XRNodeState node) {
		InputPoseActionData_t data = new InputPoseActionData_t();

		var res = OpenVR.Input.GetPoseActionData(
			action, 
			XRDevice.GetTrackingSpaceType() == TrackingSpaceType.RoomScale ? ETrackingUniverseOrigin.TrackingUniverseStanding : ETrackingUniverseOrigin.TrackingUniverseSeated, 
			0,
			ref data,
			(uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputPoseActionData_t)),
			node.nodeType == XRNode.LeftHand ? leftHand : rightHand
		);
		if (res != EVRInputError.None) throw new ApplicationException("Failed to get pose input data " + node.nodeType + ": " + res);

		var matRaw = data.pose.mDeviceToAbsoluteTracking;
		var mat = new Matrix4x4(
			new Vector4(matRaw.m0, matRaw.m1, matRaw.m2, 0),
			new Vector4(matRaw.m4, matRaw.m5, matRaw.m6, 0),
			new Vector4(matRaw.m8, matRaw.m9, matRaw.m10, 0),
			new Vector4(0, 0, 0, 1)
		);
		var rot = mat.rotation;
//		rot.x *= -1;
		rot.z *= -1;

		return new Pose {
			pos = new Vector3(matRaw.m3, matRaw.m7, -matRaw.m11),
			rot = rot
		};
	}

	public override float GetAxis(XRNodeState node, InputAxis axis) {
		if (mode == InputMode.Direct) { 
			Direct_ReadState(node);
			switch (axis) {
				case InputAxis.LeftClick:
					return direct_lastState.rAxis1.x;
				case InputAxis.MiddleClick:
					return (direct_lastState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_Axis0)) != 0 ? 1 : 0;
				case InputAxis.RightClick:
					return (direct_lastState.ulButtonPressed & (1ul << (int)EVRButtonId.k_EButton_Grip)) != 0 ? 1 : 0;

				//We don't use joystck and touchpad at the same time, GetJoypadTypes returns only one type in this mode
				case InputAxis.JoyStickX:
				case InputAxis.TouchPadX:
					return direct_lastState.rAxis0.x;
				case InputAxis.JoyStickY:
				case InputAxis.TouchPadY:
					return direct_lastState.rAxis0.y;

				case InputAxis.TouchPadTouch:
					return (direct_lastState.ulButtonTouched & (1ul << (int)EVRButtonId.k_EButton_SteamVR_Touchpad)) != 0 ? 1 : 0;
				default:
					throw new ArgumentOutOfRangeException("axis", axis, null);
			}
		} else {
			switch (axis) {
				case InputAxis.LeftClick:
					if (leftClick == 0) return 0;
					return GetDigitalInput(leftClick, node) ? 1 : 0;
				case InputAxis.MiddleClick:
					if (middleClick == 0) return 0;
					return GetDigitalInput(middleClick, node) ? 1 : 0;
				case InputAxis.RightClick:
					if (rightClick == 0) return 0;
					return GetDigitalInput(rightClick, node) ? 1 : 0;
				case InputAxis.JoyStickX:
				case InputAxis.JoyStickY: {
					if (joystickScroll == 0) return 0;
					var v = GetVector2Input(joystickScroll, node);
					if (axis == InputAxis.JoyStickX) return v.x;
					else return v.y;
				}
				case InputAxis.TouchPadX:
				case InputAxis.TouchPadY: {
					if (touchPadScrollPos == 0) return 0;
					var v = GetVector2Input(touchPadScrollPos, node);
					if (axis == InputAxis.TouchPadX) return v.x;
					else return v.y;
				}
				case InputAxis.TouchPadTouch:
					if (touchPadScrollTouch == 0) return 0;
					return GetDigitalInput(touchPadScrollTouch, node) ? 1 : 0;
				default:
					throw new ArgumentOutOfRangeException("axis", axis, null);
			}
		}
	}

	public override Pose GetPose(XRNodeState node) {
		if (mode == InputMode.Direct) {
			return base.GetPose(node);
		} else {
			return GetPoseInput(pointPose, node);
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

	private JoyPadType jpType = JoyPadType.Unknown;
	public override JoyPadType GetJoypadTypes(XRNodeState node) {
		if (jpType != JoyPadType.Unknown) return jpType;

		if (mode == InputMode.Direct) {
			var name = GetNodeName(node);

			if (name.Contains("Oculus Touch Controller") || name.StartsWith("Oculus Rift CV1")) {
				//OpenVR gives us "Oculus Rift CV1 (Left Controller)" etc. where I wish it would mention the type of controller (Touch)
				return jpType = JoyPadType.Joystick;
			} else if (name.StartsWith("Vive Controller")) {
				return jpType = JoyPadType.TouchPad;
			} else {
				Debug.LogWarning("Unknown controller type: " + name);
				return jpType = JoyPadType.None;
			}
		} else {
			return jpType = (JoyPadType.Joystick | JoyPadType.TouchPad);
		}
	}

}

#if OCULUS_SDK
class OculusVRInput : VRInput {

	public override float GetAxis(XRNodeState node, InputAxis axis) {
		var controller = GetController(node);
		OVRPlugin.ControllerState4 state = OVRPlugin.GetControllerState4((uint)controller);

		switch (axis) {
			case InputAxis.LeftClick:
				return controller == OVRInput.Controller.LTouch ? state.LIndexTrigger : state.RIndexTrigger;
			case InputAxis.RightClick:
				return controller == OVRInput.Controller.LTouch ? state.LHandTrigger : state.RHandTrigger;
			case InputAxis.MiddleClick: {
				var buttonId = controller == OVRInput.Controller.LTouch ? 0x00000400 : 0x00000004; //see enum ovrButton_ in OVR_CAPI.h
				return (state.Buttons & buttonId) != 0 ? 1 : 0;
			}
			case InputAxis.JoyStickX:
			case InputAxis.JoyStickY: {
				var joy = controller == OVRInput.Controller.LTouch ? state.LThumbstick : state.RThumbstick;
				return axis == InputAxis.JoyStickX ? joy.x : joy.y;
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

	public override JoyPadType GetJoypadTypes(XRNodeState node) {
		//assuming Oculus Touch
		return JoyPadType.Joystick;
	}
}
#endif

}


#endif