using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using UnityEngine.VR;
#endif

namespace ZenFulcrum.EmbeddedBrowser {

public class VRMode : MonoBehaviour {

	public bool enableVR;

#if UNITY_2017_2_OR_NEWER
	private bool oldState;
	public void OnEnable() {
		oldState = XRSettings.enabled;
		XRSettings.enabled = enableVR;
		if (XRSettings.enabled) {
			//Debug.Log("VR system: " + XRSettings.loadedDeviceName + " device: " + XRDevice.model);

			//Unity is drunk again. This time it likes to give us y=0=floor for OpenVR and y=0=standing height
			//for Oculus SDK unless we call this:
			XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
		}
	}

	public void OnDisable() {
		XRSettings.enabled = oldState;
	}
#else
	private bool oldState;
	public void OnEnable() {
		oldState = VRSettings.enabled;
		VRSettings.enabled = enableVR;
	}

	public void OnDisable() {
		VRSettings.enabled = oldState;
	}
#endif
}

}