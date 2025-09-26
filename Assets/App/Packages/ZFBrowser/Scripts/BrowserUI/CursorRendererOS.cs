using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Handles rendering a browser's cursor by letting using t he OS-level mouse cursor 
/// (and changing it as needed).
/// </summary>
public class CursorRendererOS : CursorRendererBase {
	[Tooltip("If true, the mouse cursor should be visible when it's not on a browser.")]
	public bool cursorNormallyVisible = true;

	protected override void CursorChange() {
		if (!cursor.HasMouse) {
			//no browser, do default
			Cursor.visible = cursorNormallyVisible;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		} else {
			if (cursor.Texture != null) {
				//browser, show this cursor
				Cursor.visible = true;
				Cursor.SetCursor(
					cursor.Texture, cursor.Hotspot, 
#if UNITY_STANDALONE_OSX
					//Not sure why, but we get really ugly looking "garbled" shadows around the mouse cursor.
					//I hate latency, but a software cursor is probably less irritating than looking at
					//that ugly stuff.
					CursorMode.ForceSoftware
#else
					CursorMode.Auto
#endif
				);
			} else {
				//browser, so no cursor
				Cursor.visible = false;
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}
		}
	}
}

}