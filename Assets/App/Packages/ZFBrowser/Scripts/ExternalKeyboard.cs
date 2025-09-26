#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#define ZF_OSX
#endif
using System;
using System.Collections;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Helper/worker class for displaying an external keyboard and
/// sending the input to a browser.
/// 
/// Don't put this script on your target browser directly, add a separate browser 
/// to the scene and attach it to that instead.
/// 
/// </summary>
[RequireComponent(typeof(Browser))]
[RequireComponent(typeof(PointerUIBase))]
public class ExternalKeyboard : MonoBehaviour {

	[Tooltip("Set to true before startup to have the keyboard automatically hook to the browser with the most recently focused text field.")]
	public bool automaticFocus;

	[Tooltip("Browser to start as the focused browser for this keyboard. Not really needed if automaticFocus is on.")]
	public Browser initialBrowser;

	[Tooltip("Set to true to have the keyboard automatically hide when we don't have a text entry box to type into.")]
	public bool hideWhenUnneeded = true;

	protected PointerUIBase activeBrowserUI;
	protected Browser keyboardBrowser;
	protected bool forcingFocus;

	protected Browser _activeBrowser;
	/// <summary>
	/// Browser that gets input if we press keys on the keyboard.
	/// </summary>
	protected virtual Browser ActiveBrowser {
		get { return _activeBrowser; }
		set {
			_SetActiveBrowser(value);
			DoFocus(_activeBrowser);
		}
	}

	protected void _SetActiveBrowser(Browser browser) {
		if (ActiveBrowser) {
			if (activeBrowserUI && forcingFocus) {
				activeBrowserUI.ForceKeyboardHasFocus(false);
				forcingFocus = false;
			}
		}

		_activeBrowser = browser;
		activeBrowserUI = ActiveBrowser.GetComponent<PointerUIBase>();
		if (!activeBrowserUI) {
			//We can't focus the browser when we type, so the typed letters won't appear as we type.
			Debug.LogWarning("Browser does not haver a PointerUI, external keyboard may not work properly.");
		}
	}

	/// <summary>
	/// Called when the focus of the keyboard changes.
	/// The browser is the browser we are focused on (may or may not be different), editable will be true if we
	/// are expected to type in the new focus, false if not.
	/// </summary>
	public event Action<Browser, bool> onFocusChange = (browser, editable) => {};
	
	public void Awake() {
		var keyboardPage = Resources.Load<TextAsset>("Browser/Keyboard").text;

		keyboardBrowser = GetComponent<Browser>();

		keyboardBrowser.onBrowserFocus += OnBrowserFocus;
		keyboardBrowser.LoadHTML(keyboardPage);
		keyboardBrowser.RegisterFunction("textTyped", TextTyped);
		keyboardBrowser.RegisterFunction("commandEntered", CommandEntered);

		if (initialBrowser) ActiveBrowser = initialBrowser;

		if (automaticFocus) { 
			StartCoroutine(FindAndListenForBrowsers());
		}
	}

	protected IEnumerator FindAndListenForBrowsers() {
		yield return null;
		foreach (var browser in FindObjectsOfType<Browser>()) {
			if (browser == keyboardBrowser) continue;
			ObserveBrowser(browser);
		}
		Browser.onAnyBrowserCreated += ObserveBrowser;
		//in theory we shouldn't need to deal with browsers being destroyed since the whole callback chain should get cleaned up
		//(might need some more work if you repeatedly destroy and recreate keyboards, though)
	}

	protected void ObserveBrowser(Browser browser) {
		browser.onNodeFocus += (tagName, editable, value) => {
			if (!this) return;
			if (!browser.focusState.hasMouseFocus) return;

			DoFocus(browser);
		};

		var pointerUI = browser.GetComponent<PointerUIBase>();
		if (pointerUI) {
			pointerUI.onClick += () => {
				DoFocus(browser);
			};
		}
	}

	protected void DoFocus(Browser browser) {
		if (browser != ActiveBrowser) {
			_SetActiveBrowser(browser);
		}

		bool visible;
		if (browser) visible = browser.focusState.focusedNodeEditable;
		else visible = false;
		SetVisible(visible);
		
		onFocusChange(_activeBrowser, visible);
	}

	protected void SetVisible(bool visible) {
		var renderer = GetComponent<Renderer>();
		if (renderer) renderer.enabled = visible;
		var collider = GetComponent<Collider>();
		if (collider) collider.enabled = visible;
	}

	protected void OnBrowserFocus(bool mouseFocused, bool kbFocused) {
		//when our keyboard is focused, focus the browser we will be typing into.
		if (!activeBrowserUI) return;

		if ((mouseFocused || kbFocused) && !forcingFocus) {
			activeBrowserUI.ForceKeyboardHasFocus(true);
			forcingFocus = true;
		}

		if (!(mouseFocused || kbFocused) && forcingFocus) {
			activeBrowserUI.ForceKeyboardHasFocus(false);
			forcingFocus = false;
		}
	}

	protected void CommandEntered(JSONNode args) {
		if (!ActiveBrowser) return;

		string command = args[0];
		bool shiftPressed = args[1];

		if (shiftPressed) ActiveBrowser.PressKey(KeyCode.LeftShift, KeyAction.Press);

		
#if ZF_OSX
		const KeyCode wordShifter = KeyCode.LeftAlt;
#else
		const KeyCode wordShifter = KeyCode.LeftControl;
#endif

		switch (command) {
			case "backspace":
				ActiveBrowser.PressKey(KeyCode.Backspace);
				break;
			case "copy":
				ActiveBrowser.SendFrameCommand(BrowserNative.FrameCommand.Copy);
				break;
			case "cut":
				ActiveBrowser.SendFrameCommand(BrowserNative.FrameCommand.Cut);
				break;
			case "delete":
				ActiveBrowser.PressKey(KeyCode.Delete);
				break;
			case "down":
				ActiveBrowser.PressKey(KeyCode.DownArrow);
				break;
			case "end":
				ActiveBrowser.PressKey(KeyCode.End);
				break;
			case "home":
				ActiveBrowser.PressKey(KeyCode.Home);
				break;
			case "insert":
				ActiveBrowser.PressKey(KeyCode.Insert);
				break;
			case "left":
				ActiveBrowser.PressKey(KeyCode.LeftArrow);
				break;
			case "pageDown":
				ActiveBrowser.PressKey(KeyCode.PageDown);
				break;
			case "pageUp":
				ActiveBrowser.PressKey(KeyCode.PageUp);
				break;
			case "paste":
				ActiveBrowser.SendFrameCommand(BrowserNative.FrameCommand.Paste);
				break;
			case "redo":
				ActiveBrowser.SendFrameCommand(BrowserNative.FrameCommand.Redo);
				break;
			case "right":
				ActiveBrowser.PressKey(KeyCode.RightArrow);
				break;
			case "selectAll":
				ActiveBrowser.SendFrameCommand(BrowserNative.FrameCommand.SelectAll);
				break;
			case "undo":
				ActiveBrowser.SendFrameCommand(BrowserNative.FrameCommand.Undo);
				break;
			case "up":
				ActiveBrowser.PressKey(KeyCode.UpArrow);
				break;
			case "wordLeft":
				ActiveBrowser.PressKey(wordShifter, KeyAction.Press);
				ActiveBrowser.PressKey(KeyCode.LeftArrow);
				ActiveBrowser.PressKey(wordShifter, KeyAction.Release);
				break;
			case "wordRight":
				ActiveBrowser.PressKey(wordShifter, KeyAction.Press);
				ActiveBrowser.PressKey(KeyCode.RightArrow);
				ActiveBrowser.PressKey(wordShifter, KeyAction.Release);
				break;
		}

		if (shiftPressed) ActiveBrowser.PressKey(KeyCode.LeftShift, KeyAction.Release);
	}

	protected void TextTyped(JSONNode args) {
		if (!ActiveBrowser) return;

		ActiveBrowser.TypeText(args[0]);
	}
}

}