using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZenFulcrum.EmbeddedBrowser {

/** Attach this script to a GUI Image to use a browser on it. */
[RequireComponent(typeof(RawImage))]
public class PointerUIGUI :
	PointerUIBase,
	IBrowserUI,
	ISelectHandler, IDeselectHandler,
	IPointerEnterHandler, IPointerExitHandler,
	IPointerDownHandler
{
	protected RawImage myImage;

	public bool enableInput = true;
	public bool automaticResize = true;

	public override void Awake() {
		base.Awake();
		myImage = GetComponent<RawImage>();

		browser.afterResize += UpdateTexture;
//		BrowserCursor.cursorChange += () => {
//			SetCursor(BrowserCursor);
//		};

		rTransform = GetComponent<RectTransform>();
	}

	protected void OnEnable() {
		if (automaticResize) StartCoroutine(WatchResize());
	}

	/** Automatically resizes the browser to match the size of this object. */
	private IEnumerator WatchResize() {
		Rect currentSize = new Rect();

		while (enabled) {
			var rect = rTransform.rect;

			if (rect.size.x <= 0 || rect.size.y <= 0) rect.size = new Vector2(512, 512);
			if (rect.size != currentSize.size) {
				browser.Resize((int)rect.size.x, (int)rect.size.y);
				currentSize = rect;
			}

			yield return null;
		}
	}

	protected void UpdateTexture(Texture2D texture) {
		myImage.texture = texture;
		myImage.uvRect = new Rect(0, 0, 1, 1);
	}

	protected BaseRaycaster raycaster;
	protected RectTransform rTransform;
//	protected List<RaycastResult> raycastResults = new List<RaycastResult>();

	protected override Vector2 MapPointerToBrowser(Vector2 screenPosition, int pointerId) {
		if (!raycaster) raycaster = GetComponentInParent<BaseRaycaster>();

		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			(RectTransform)transform, screenPosition, raycaster.eventCamera, out pos
		);
		pos.x = pos.x / rTransform.rect.width + rTransform.pivot.x;
		pos.y = pos.y / rTransform.rect.height + rTransform.pivot.y;

		if (pos.x < 0 || pos.x > 1) pos.x = float.NaN;
		if (pos.y < 0 || pos.y > 1) pos.x = float.NaN;
		return pos;
	}

	protected override Vector2 MapRayToBrowser(Ray worldRay, int pointerId) {
		var evs = EventSystem.current;
		if (!evs) return new Vector2(float.NaN, float.NaN);

		//todo: world-space GUI
		return new Vector2(float.NaN, float.NaN);
	}

	public override void GetCurrentHitLocation(out Vector3 pos, out Quaternion rot) {
		//todo: world space GUI
		pos = new Vector3(float.NaN, float.NaN, float.NaN);
		rot = Quaternion.identity;
	}


	protected bool _mouseHasFocus;
	public override bool MouseHasFocus {
		get { return _mouseHasFocus && enableInput; } 
		protected set { _mouseHasFocus = value; }
	}
	protected bool _keyboardHasFocus;

	public override bool KeyboardHasFocus {
		get {
			if (!enableInput) return false;
			return _keyboardHasFocus || focusForceCount > 0;
		}
	}

	public void OnSelect(BaseEventData eventData) {
		_keyboardHasFocus = true;
		Input.imeCompositionMode = IMECompositionMode.Off;//CEF will handle the IME
	}

	public void OnDeselect(BaseEventData eventData) {
		_keyboardHasFocus = false;
		Input.imeCompositionMode = IMECompositionMode.Auto;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		_mouseHasFocus = true;
//		SetCursor(BrowserCursor);
	}

	public void OnPointerExit(PointerEventData eventData) {
		_mouseHasFocus = false;
//		SetCursor(null);
	}


	public void OnPointerDown(PointerEventData eventData) {
		EventSystem.current.SetSelectedGameObject(gameObject);
	}
}

}
