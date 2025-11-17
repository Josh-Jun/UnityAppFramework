#if UNITY_5_3_OR_NEWER
#define SCENE_MANAGER
using UnityEngine.SceneManagement;
#endif

using System;
using System.Collections;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/** Handles mouse hiding/locking and pause menu for the demo. */
public class HUDManager : MonoBehaviour {
	public static HUDManager Instance { get; private set; }

	private bool haveMouse = false;

	public PointerUIGUI hud;

	public Browser HUDBrowser { get; private set; }

	public void Awake() {
		Instance = this;
	}

	public void Start() {
		HUDBrowser = hud.GetComponent<Browser>();
		HUDBrowser.RegisterFunction("unpause", args => Unpause());
		HUDBrowser.RegisterFunction("browserMode", args => LoadBrowseLevel(true));
		HUDBrowser.RegisterFunction("quit", args => Application.Quit());

		Unpause();

		#if UNITY_STANDALONE_LINUX
		StartCoroutine(Rehide());
		#endif

		//Update coin count on hud when user gets one
		PlayerInventory.Instance.coinCollected += count => HUDBrowser.CallFunction("setCoinCount", count);
	}

	private IEnumerator Rehide() {
		//Unity has bugs. Here's another workaround for another Unity bug.
#if UNITY_5_5_OR_NEWER
		while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
#else
		while (Application.isShowingSplashScreen) yield return null;
#endif
		Cursor.visible = false;
		yield return new WaitForSeconds(.2f);
		Cursor.visible = true;
		yield return new WaitForSeconds(.2f);
		Cursor.visible = false;
	}

	public void Unpause() {
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		EnableUserControls(true);

		Time.timeScale = 1;

		haveMouse = true;
		HUDBrowser.CallFunction("setPaused", false);
	}

	public void Pause() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		haveMouse = false;

		Time.timeScale = 0;

		EnableUserControls(false);

		HUDBrowser.CallFunction("setPaused", true);
	}

	public void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (haveMouse) Pause();
			else Unpause();
		}
	}

	public void Say(string html, float dwellTime) {
		HUDBrowser.CallFunction("say", html, dwellTime);
	}

	protected void EnableUserControls(bool enableIt) {
//fixme: demo still uses the old input system
#pragma warning disable 618
		FPSCursorRenderer.Instance.EnableInput = enableIt;
#pragma warning restore 618

		var fpsInput = GetComponent<SimpleFPSController>();
		fpsInput.enabled = enableIt;

		hud.enableInput = !enableIt;
	}

	public void LoadBrowseLevel(bool force = false) {
		StartCoroutine(LoadLevel(force));
	}

	private IEnumerator LoadLevel(bool force = false) {
		if (!force) {
			yield return new WaitUntil(() => SayWordsOnTouch.ActiveSpeakers == 0);
		}

		Pause();
#if SCENE_MANAGER
		SceneManager.LoadScene("SimpleBrowser");
#else
		Application.LoadLevel("SimpleBrowser");
#endif
	}

}

}
