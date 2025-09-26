using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Renders a browser's cursor by rendering something in the center of the screen.
/// </summary>
public class CursorRendererOverlay : CursorRendererBase {

	[Tooltip("How large should we render the cursor?")]
	public float scale = .5f;

	protected override void CursorChange() {}

	public void OnGUI() {
		if (cursor == null) return;

		if (!cursor.HasMouse || !cursor.Texture) return;

		var tex = cursor.Texture;

		var pos = new Rect(Screen.width / 2f, Screen.height / 2f, tex.width * scale, tex.height * scale);
		pos.x -= cursor.Hotspot.x * scale;
		pos.y -= cursor.Hotspot.y * scale;

		GUI.DrawTexture(pos, tex);
	}
}

}