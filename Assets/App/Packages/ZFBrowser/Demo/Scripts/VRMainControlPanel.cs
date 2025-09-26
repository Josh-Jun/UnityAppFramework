using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Handler class for doing open/close/move of browsers and the animations for such.
/// </summary>
public class VRMainControlPanel : MonoBehaviour {
	public static VRMainControlPanel instance;
	public GameObject browserPrefab;

	public float moveSpeed = .01f;

	public float height = 1.5f, radius = 2f;

	public int browsersToFit = 8;

	protected List<VRBrowserPanel> allBrowsers = new List<VRBrowserPanel>();

	public ExternalKeyboard keyboard;
	private Vector3 baseKeyboardScale;

	public void Awake() {
		instance = this;
		var browser = GetComponent<Browser>();
		browser.RegisterFunction("openNewTab", args => OpenNewTab());
		browser.RegisterFunction("shiftTabs", args => ShiftTabs(args[0]));
		baseKeyboardScale = keyboard.transform.localScale;

		OpenNewTab();
	}

	private void ShiftTabs(int direction) {
		allBrowsers = allBrowsers.Select((t, i) => allBrowsers[(i + direction + allBrowsers.Count) % allBrowsers.Count]).ToList();
	}

	public VRBrowserPanel OpenNewTab(VRBrowserPanel nextTo = null) {
		var tabGO = Instantiate(browserPrefab);
		var tab = tabGO.GetComponent<VRBrowserPanel>();

		var insertIndex = -1;
		if (nextTo) insertIndex = allBrowsers.FindIndex(x => x == nextTo);
		if (insertIndex > 0) allBrowsers.Insert(insertIndex + 1, tab);
		else allBrowsers.Insert(allBrowsers.Count / 2, tab);

		tab.transform.position = transform.position;
		tab.transform.rotation = transform.rotation;
		tab.transform.localScale = Vector3.zero;

		return tab;
	}

	private VRBrowserPanel keyboardTarget;
	public void MoveKeyboardUnder(VRBrowserPanel panel) {
		keyboardTarget = panel;
	}

	public void DestroyPane(VRBrowserPanel pane) {
		StartCoroutine(_DestroyBrowser(pane));
	}

	private IEnumerator _DestroyBrowser(VRBrowserPanel pane) {
		//drop the pane and destroy it
		allBrowsers.Remove(pane);
		if (!pane) yield break;

		var targetPos = pane.transform.position;
		targetPos.y = 0;

		var t0 = Time.time;
		while (Time.time < t0 + 3) {
			if (!pane) yield break;
			MoveToward(pane.transform, targetPos, pane.transform.rotation, Vector3.zero);
			yield return null;
		}

		Destroy(pane.gameObject);
	}

	private void MoveToward(Transform t, Vector3 pos, Quaternion rot, Vector3 scale) {
		t.position = Vector3.Lerp(t.position, pos, moveSpeed);
		t.rotation = Quaternion.Slerp(t.rotation, rot, moveSpeed);
		t.localScale = Vector3.Lerp(t.localScale, scale, moveSpeed);
	}

	public void Update() {
		//animate all panes to their respective positions

		var p = Mathf.Clamp01((allBrowsers.Count - 1) / (float)browsersToFit);

		var radialAmount = Mathf.Lerp(0, 360, p);
		var radialOffset = Mathf.Lerp(0, 180, p);
		var countOffset = Mathf.Lerp(1, 0, p);

		for (int i = 0; i < allBrowsers.Count; i++) {
			var angle = i / (allBrowsers.Count - countOffset) * radialAmount - radialOffset;
			if (i == 0 && allBrowsers.Count == 1) angle = radialAmount / 2f - radialOffset;

			angle *= Mathf.Deg2Rad;
			var pos = new Vector3(Mathf.Sin(angle) * radius, height, Mathf.Cos(angle) * radius);
			var rotation = Quaternion.LookRotation(new Vector3(pos.x, 0, pos.z), Vector3.up);

			MoveToward(allBrowsers[i].transform, pos, rotation, Vector3.one);
		}

		{
			var pos = keyboardTarget ? keyboardTarget.keyboardLocation.position : Vector3.zero;
			var rot = keyboardTarget ? keyboardTarget.keyboardLocation.rotation : Quaternion.LookRotation(Vector3.down, Vector3.forward);
			var scale = keyboardTarget ? baseKeyboardScale : Vector3.zero;
			MoveToward(keyboard.transform, pos, rot, scale);
		}

	}

}

}