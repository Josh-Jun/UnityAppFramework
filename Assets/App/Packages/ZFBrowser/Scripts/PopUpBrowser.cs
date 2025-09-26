#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Manages a browser that's been opened in an OS-native window outside the game's main window.
/// (Presently only used for OS X.)
/// </summary>
public class PopUpBrowser : MonoBehaviour, IBrowserUI {
	private int windowId;
	private int browserId;

	private List<string> messages = new List<string>();
	private Browser browser;
	private BrowserNative.WindowCallbackFunc callbackRef;//don't let this get GC'd
	private Vector2 delayedResize;

	public static void Create(int possibleBrowserId) {
		var go = new GameObject("Pop up browser");
		var browser = go.AddComponent<Browser>();
		browser.RequestNativeBrowser(possibleBrowserId);

		var pop = go.AddComponent<PopUpBrowser>();
		pop.callbackRef = pop.HandleWindowMessage;
		pop.windowId = BrowserNative.zfb_windowCreate("", pop.callbackRef);
		pop.browserId = possibleBrowserId;
		pop.BrowserCursor = new BrowserCursor();
		pop.KeyEvents = new List<Event>();
		pop.browser = browser;

		pop.StartCoroutine(pop.FixFocus());

		pop.InputSettings = new BrowserInputSettings();

		browser.UIHandler = pop;
		browser.EnableRendering = false;//rendering is done differently, so don't run the usual code
	}

	private void HandleWindowMessage(int windowId, IntPtr data) {
		var msgJSON = Util.PtrToStringUTF8(data);
		//if (!msgJSON.Contains("mouseMove")) Debug.Log("Window message: " + msgJSON);

		lock (messages) messages.Add(msgJSON);
	}

	public void OnDestroy() {
		if (!BrowserNative.SymbolsLoaded) return;
		BrowserNative.zfb_windowClose(windowId);
	}


	private IEnumerator FixFocus() {
		//OS X: Magically, new browser windows that are focused don't think they are focused, even though we told them so.
		//Hack workaround.
		yield return null;
		BrowserNative.zfb_setFocused(browserId, false);
		yield return null;
		BrowserNative.zfb_setFocused(browserId, true);
		yield return null;
		BrowserNative.zfb_setFocused(browserId, KeyboardHasFocus);
	}

	public void InputUpdate() {
		MouseScroll = Vector2.zero;
		KeyEvents.Clear();
		delayedResize = new Vector2(float.NaN, float.NaN);

		lock (messages) {
			for (int i = 0; i < messages.Count; i++) {
				HandleMessage(messages[i]);
			}
			messages.Clear();
		}

		if (!float.IsNaN(delayedResize.x)) {
			browser.Resize((int)delayedResize.x, (int)delayedResize.y);
		}
	}

	private void HandleMessage(string message) {
		var msg = JSONNode.Parse(message);


		switch ((string)msg["type"]) {
			case "mouseDown":
			case "mouseUp": {
				int button = msg["button"];
				MouseButton flag = 0;
				if (button == 0) flag = MouseButton.Left;
				else if (button == 1) flag = MouseButton.Right;
				else if (button == 2) flag = MouseButton.Middle;

				if (msg["type"] == "mouseDown") MouseButtons |= flag;
				else MouseButtons &= ~flag;

				break;
			}
			case "mouseMove": {
				var screenPos = new Vector2(msg["x"], msg["y"]);
				screenPos.x = screenPos.x / browser.Size.x;
				screenPos.y = screenPos.y / browser.Size.y;
				MousePosition = screenPos;
				//Debug.Log("mouse now at " + screenPos);
				break;
			}
			case "mouseFocus":
				MouseHasFocus = msg["focus"];
				break;
			case "keyboardFocus":
				KeyboardHasFocus = msg["focus"];
				break;
			case "mouseScroll": {
				const float mult = 1 / 3f;
				MouseScroll += new Vector2(msg["x"], msg["y"]) * mult;
				break;
			}
			case "keyDown":
			case "keyUp": {
				var ev = new Event();
				ev.type = msg["type"] == "keyDown" ? EventType.KeyDown : EventType.KeyUp;
				ev.character = (char)0;
				ev.keyCode = KeyMappings.GetUnityKeyCode(msg["code"]);
				SetMods(ev);

				//Debug.Log("Convert wkc " + (int)msg["code"] + " to ukc " + ev.keyCode);

				KeyEvents.Add(ev);
				break;
			}
			case "keyPress": {
				string characters = msg["characters"];
				foreach (char c in characters) {
					var ev = new Event();
					ev.type = EventType.KeyDown;
					SetMods(ev);
					ev.character = c;
					ev.keyCode = 0;

					KeyEvents.Add(ev);
				}
				break;
			}
			case "resize":
				//on OS X (editor at least), resizing hangs the update loop, so we suddenly end up with a bajillion resize
				//messages we were unable to process. Just record it here and when we've processed everything we'll resize
				delayedResize.x = msg["w"];
				delayedResize.y = msg["h"];
				break;
			case "close":
				Destroy(gameObject);
				break;
			default:
				Debug.LogWarning("Unknown window event: " + msg.AsJSON);
				break;
		}
	}


	private bool shiftDown, controlDown, altDown, commandDown;
	private void SetMods(Event ev) {
		switch (ev.keyCode) {
			case KeyCode.LeftShift: case KeyCode.RightShift:
				shiftDown = ev.type == EventType.KeyDown;
				break;
			case KeyCode.LeftControl: case KeyCode.RightControl:
				controlDown = ev.type == EventType.KeyDown;
				break;
			case KeyCode.LeftAlt: case KeyCode.RightAlt:
				altDown = ev.type == EventType.KeyDown;
				break;
			case KeyCode.LeftCommand: case KeyCode.RightCommand:
			case KeyCode.LeftWindows: case KeyCode.RightWindows:
				commandDown = ev.type == EventType.KeyDown;
				break;
		}

		ev.shift = shiftDown;
		ev.control = controlDown;
		ev.alt = altDown;
		ev.command = commandDown;
	}

	public void Update() {
		if (!BrowserNative.SymbolsLoaded) return;
		BrowserNative.zfb_windowRender(windowId, browserId);
	}

	public bool MouseHasFocus { get; private set; }
	public Vector2 MousePosition { get; private set; }
	public MouseButton MouseButtons { get; private set; }
	public Vector2 MouseScroll { get; private set; }
	public bool KeyboardHasFocus { get; private set; }
	public List<Event> KeyEvents { get; private set; }
	public BrowserCursor BrowserCursor { get; private set; }
	public BrowserInputSettings InputSettings { get; private set; }
}

}
#endif