using System;
using System.Net;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

public class VRBrowserPanel : MonoBehaviour, INewWindowHandler {
	public Browser contentBrowser, controlBrowser;
	public Transform keyboardLocation;

	public void Awake() {
		//If the content browser is externally closed, make sure we go too.
		var dd = contentBrowser.gameObject.AddComponent<DestroyDetector>();
		dd.onDestroy += CloseBrowser;

		contentBrowser.SetNewWindowHandler(Browser.NewWindowAction.NewBrowser, this);
		contentBrowser.onLoad += data => controlBrowser.CallFunction("setURL", data["url"]);

		controlBrowser.RegisterFunction("demoNavForward", args => contentBrowser.GoForward());
		controlBrowser.RegisterFunction("demoNavBack", args => contentBrowser.GoBack());
		controlBrowser.RegisterFunction("demoNavRefresh", args => contentBrowser.Reload());
		controlBrowser.RegisterFunction("demoNavClose", args => CloseBrowser());
		controlBrowser.RegisterFunction("goTo", args => contentBrowser.LoadURL(args[0], false));

		VRMainControlPanel.instance.keyboard.onFocusChange += OnKeyboardOnOnFocusChange;
	}

	public void OnDestroy() {
		VRMainControlPanel.instance.keyboard.onFocusChange -= OnKeyboardOnOnFocusChange;
	}

	private void OnKeyboardOnOnFocusChange(Browser browser, bool editable) {
		if (!editable || !browser) VRMainControlPanel.instance.MoveKeyboardUnder(null);
		else if (browser == contentBrowser || browser == controlBrowser) VRMainControlPanel.instance.MoveKeyboardUnder(this);
	}

	public void CloseBrowser() {
		if (!this || !VRMainControlPanel.instance) return;

		VRMainControlPanel.instance.DestroyPane(this);
	}

	public Browser CreateBrowser(Browser parent) {
		var newPane = VRMainControlPanel.instance.OpenNewTab(this);
		newPane.transform.position = transform.position;
		newPane.transform.rotation = transform.rotation;
		return newPane.contentBrowser;
	}
}

internal class DestroyDetector : MonoBehaviour {
	public event Action onDestroy = () => {};

	public void OnDestroy() {
		onDestroy();
	}

}

}