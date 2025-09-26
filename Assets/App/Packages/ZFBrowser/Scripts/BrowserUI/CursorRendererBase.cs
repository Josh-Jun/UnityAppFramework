using System.Collections;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// These classes handle rendering the cursor of a browser.
/// 
/// Using one is optional. You can opt not to show a cursor.
/// </summary>
[RequireComponent(typeof(PointerUIBase))]
abstract public class CursorRendererBase : MonoBehaviour {
	protected BrowserCursor cursor;

	public virtual void OnEnable() {
		StartCoroutine(Setup());
	}

	private IEnumerator Setup() {
		if (cursor != null) yield break;

		yield return null;//wait a frame to let the browser UI get set up

		cursor = GetComponent<Browser>().UIHandler.BrowserCursor;
		cursor.cursorChange += CursorChange;
	}

	protected abstract void CursorChange();
}

}