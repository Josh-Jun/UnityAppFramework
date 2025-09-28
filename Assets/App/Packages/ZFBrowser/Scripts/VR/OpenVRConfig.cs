/*
 * This file holds game-specific OpenVR input configuration.
 * Edit it.
 */
#if UNITY_2017_2_OR_NEWER

namespace ZenFulcrum.EmbeddedBrowser.VR {

partial class OpenVRInput {
	/// <summary>
	/// Direct or remappable (action) input mode?
	/// False: we are looking at controller buttons directly
	/// True: we are using the remappable action input system (SteamVR 2.x)
	/// </summary>
	private InputMode mode = InputMode.Direct;//may need to restart Unity and/or SteamVR if you switch to this mode after using the new system
//	private InputMode mode = InputMode.MappedActions;

	/*
	 * If you're using SteamVR 2.x with the new input mapping system change 
	 * the above to InputMode.MappedActions, then create or customize the inputs listed below:
	 */

	/// <summary>
	/// Direction we use to point at browsers. pose type
	/// Mandatory.
	/// </summary>
	private const string PointPose = "/actions/ui/in/UIPointer";

	/// <summary>
	/// Input path for "left" click. bool type
	/// Empty to disable.
	/// </summary>
	private const string LeftClickAction = "/actions/ui/in/InteractMain";

	/// <summary>
	/// Input path for "middle" click. bool type
	/// Empty to disable.
	/// </summary>
	private const string MiddleClickAction = "/actions/ui/in/InteractMiddle";

	/// <summary>
	/// Input path for "right" click. bool type
	/// Empty to disable.
	/// </summary>
	private const string RightClickAction = "/actions/ui/in/InteractContext";

	/// <summary>
	/// Joystick input for 2D scrolling. Vector2 type
	/// Move the joystick up/down/left/right and hold it to scroll. Push farther to scroll faster.
	/// Empty to disable.
	/// </summary>
	private const string JoystickScrollAction = "/actions/ui/in/JoystickScroll";

	/// <summary>
	/// Touchpad input for 2D scrolling. True when a finger is touching the touchpad. bool type
	/// Enables touch-pad style scrolling where moving your finger produces scrolling (not time-based).
	/// Also set up TouchpadScrollPosition. 
	/// Empty to disable.
	/// </summary>
	private const string TouchpadScrollTouch = "/actions/ui/in/TouchpadScrollTouch";

	/// <summary>
	/// Touchpad input for 2D scrolling. Position on the touchpad. Vector2 type
	/// Enables touch-pad style scrolling where moving your finger produces scrolling (not time-based).
	/// Also set up TouchpadScrollTouch. 
	/// Empty to disable.
	/// </summary>
	private const string TouchpadScrollPosition = "/actions/ui/in/TouchpadScrollPosition";

}

}

#endif