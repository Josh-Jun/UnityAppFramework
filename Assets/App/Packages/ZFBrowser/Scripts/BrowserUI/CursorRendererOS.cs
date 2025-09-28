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
					CursorMode.Auto
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