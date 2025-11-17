using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZenFulcrum.EmbeddedBrowser {

/**
 * A very simple controller for a Browser.
 * Call GoToURLInput() to go to the URL typed in urlInput
 */
[RequireComponent(typeof(Browser))]
public class SimpleController : MonoBehaviour {

	private Browser browser;
	public InputField urlInput;

	public void Start() {
		browser = GetComponent<Browser>();

		//Update text field when the browser loads a page
		browser.onNavStateChange += () => {
			if (!urlInput.isFocused) {
				//but only if it's not focused (don't steal kill something the user is typing)
				urlInput.text = browser.Url;
			}
		};

		urlInput.onEndEdit.AddListener(v => {
			if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) {
				//only nav if they hit enter, not just because they unfocused it
				urlInput.DeactivateInputField();
				GoToURLInput();
			} else {
				//revert text to URL if it updated while they were typing
				urlInput.text = browser.Url;
			}
		});
	}

	public void GoToURLInput() {
		browser.Url = urlInput.text;
	}

}

}