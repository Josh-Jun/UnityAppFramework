using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/** 
 * Hooks to ensure that zfb gets shut down on app (or playmode) exit. 
 * (This used to be used only for builds, but now that we can cleanly shut down the browser 
 * system after every playmode run it's used in the Editor too.)
 */
class StandaloneShutdown : MonoBehaviour {
	public static void Create() {
		var go = new GameObject("ZFB Shutdown");
		go.AddComponent<StandaloneShutdown>();
		DontDestroyOnLoad(go);
	}

	public void OnApplicationQuit() {
		BrowserNative.UnloadNative();
	}
}

}
