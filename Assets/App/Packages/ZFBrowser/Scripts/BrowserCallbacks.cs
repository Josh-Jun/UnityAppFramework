
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AOT;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Callbacks used by Browser.cs
/// Man, that file's getting big. (Breaking it up would likely involve backwards-incompatible API changes, so 
/// let it be for now.)
/// </summary>
public partial class Browser {
	/// <summary>
	/// Map of browserId => Browser fro looking up C# objects when we get a native callback.
	/// 
	/// Note that this reflects the native state, that is, we include Browsers that havne't fully initialized yet, 
	/// but instead list what we need to lookup callbacks coming from the native side.
	/// 
	/// (We used to rely on closures and generated trampolines for this data, but il2cpp doens't support that.)
	/// </summary>
	internal static Dictionary<int, Browser> allBrowsers = new Dictionary<int, Browser>();
	
	private static Browser GetBrowser(int browserId) {
		lock (allBrowsers) {
			Browser ret;
			if (allBrowsers.TryGetValue(browserId, out ret)) return ret;
		}

		Debug.LogWarning("Got a callback for brower id " + browserId + " which doesn't exist.");
		return null;
	}

	[MonoPInvokeCallback(typeof(BrowserNative.ForwardJSCallFunc))]
	private static void CB_ForwardJSCallFunc(int browserId, int callbackId, string data, int size) {
		var browser = GetBrowser(browserId);
		if (!browser) return;

		lock (browser.thingsToDo) browser.thingsToDo.Add(() => {

			JSResultFunc cb;
			if (!browser.registeredCallbacks.TryGetValue(callbackId, out cb)) {
				Debug.LogWarning("Got a JS callback for event " + callbackId + ", but no such event is registered.");
				return;
			}

			var isError = false;
			if (data.StartsWith("fail-")) {
				isError = true;
				data = data.Substring(5);
			}

			JSONNode node;
			try {
				node = JSONNode.Parse(data);
			} catch (SerializationException) {
				Debug.LogWarning("Invalid JSON sent from browser: " + data);
				return;
			}

			try {
				cb(node, isError);
			} catch (Exception ex) {
				//user's function died, log it and carry on
				Debug.LogException(ex);
				return;
			}
		});
	}

	[MonoPInvokeCallback(typeof(BrowserNative.ChangeFunc))]
	private static void CB_ChangeFunc(int browserId, BrowserNative.ChangeType changeType, string arg1) {
		//Note: we may have been Object.Destroy'd at this point, so guard against that.
		//That means we can't trust that `browser == null` means we don't have a browser, we may have an object that 
		//is destroyed, but not actually null.
		Browser browser;
		lock (allBrowsers) {
			if (!allBrowsers.TryGetValue(browserId, out browser)) return;
		}

		if (changeType == BrowserNative.ChangeType.CHT_BROWSER_CLOSE) {
			//We can't continue if the browser is closed, so goodbye.

			//At this point, we may or may not be destroyed, but if not, become destroyed.
			//Debug.Log("Got close notification for " + unsafeBrowserId);
			if (browser) {
				//Need to be destroyed.
				lock (browser.thingsToDo) browser.thingsToDo.Add(() => {
					Destroy(browser.gameObject);
				});
			} else {
				//If we are (Unity) destroyed, we won't get another update, so we can't rely on thingsToDo
				//That said, there's not anything else for us to do but step out of allThingsToRemember.
			}

			//The native side has acknowledged it's done, now we can finally let the native trampolines be GC'd
			lock (allThingsToRemember) allThingsToRemember.Remove(browser.unsafeBrowserId);

			//Now that we know we can allow ourselves to be GC'd and destroyed (without leaking to allThingsToRemember)
			//go ahead and allow it. (And we won't get any more native callbacks at this point.)
			lock (allBrowsers) allBrowsers.Remove(browserId);

			//Just in case someone tries to call something, make sure CheckSanity and such fail.
			browser.browserId = 0;
		} else if (browser) {
			lock (browser.thingsToDo) browser.thingsToDo.Add(() => browser.OnItemChange(changeType, arg1));
		}
	}

	[MonoPInvokeCallback(typeof(BrowserNative.DisplayDialogFunc))]
	private static void CB_DisplayDialogFunc(
		int browserId, BrowserNative.DialogType dialogType, IntPtr textPtr,
		IntPtr promptTextPtr, IntPtr sourceURL
	) {
		var browser = GetBrowser(browserId);
		if (!browser) return;

		var text = Util.PtrToStringUTF8(textPtr);
		var promptText = Util.PtrToStringUTF8(promptTextPtr);
		//var url = Util.PtrToStringUTF8(urlPtr);

		lock (browser.thingsToDo) browser.thingsToDo.Add(() => {
			browser.CreateDialogHandler();
			browser.dialogHandler.HandleDialog(dialogType, text, promptText);
		});
	}

	[MonoPInvokeCallback(typeof(BrowserNative.ShowContextMenuFunc))]
	private static void CB_ShowContextMenuFunc(
		int browserId, string json, int x, int y, BrowserNative.ContextMenuOrigin origin
	) {
		var browser = GetBrowser(browserId);
		if (!browser) return;

		if (json != null && (browser.allowContextMenuOn & origin) == 0) {
			//ignore this
			BrowserNative.zfb_sendContextMenuResults(browserId, -1);
			return;
		}

		lock (browser.thingsToDo) browser.thingsToDo.Add(() => {
			if (json != null) browser.CreateDialogHandler();
			if (browser.dialogHandler != null) browser.dialogHandler.HandleContextMenu(json, x, y);
		});
	}

	[MonoPInvokeCallback(typeof(BrowserNative.ConsoleFunc))]
	private static void CB_ConsoleFunc(int browserId, string message, string source, int line) {
		var browser = GetBrowser(browserId);
		if (!browser) return;

		lock (browser.thingsToDo) browser.thingsToDo.Add(() => {
			browser.onConsoleMessage(message, source + ":" + line);
		});
	}

	[MonoPInvokeCallback(typeof(BrowserNative.ReadyFunc))]
	private static void CB_ReadyFunc(int browserId) {
		var browser = GetBrowser(browserId);
		if (!browser) return;

		//We could be on any thread at this time, so schedule the callbacks to fire during the next InputUpdate
		lock (browser.thingsToDo) browser.thingsToDo.Add(() => {
			browser.browserId = browserId;

			// ReSharper disable once PossibleNullReferenceException
			browser.onNativeReady(browserId);
		});
	}
	
	[MonoPInvokeCallback(typeof(BrowserNative.NavStateFunc))]
	private static void CB_NavStateFunc(int browserId, bool canGoBack, bool canGoForward, bool lodaing, IntPtr urlRaw) {
		var browser = GetBrowser(browserId);
		if (!browser) return;

		var url = Util.PtrToStringUTF8(urlRaw);

		lock (browser.thingsToDo) browser.thingsToDo.Add(() => {
			browser.navState.canGoBack = canGoBack;
			browser.navState.canGoForward = canGoForward;
			browser.navState.loading = lodaing;
			browser.navState.url = url;

			browser._url = url;//update the inspector

			browser.onNavStateChange();
		});
	}

	[MonoPInvokeCallback(typeof(BrowserNative.NewWindowFunc))]
	private static void CB_NewWindowFunc(int creatorBrowserId, int newBrowserId, IntPtr urlPtr) {
		var browser = GetBrowser(creatorBrowserId);
		if (!browser) return;

		#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
			var url = Util.PtrToStringUTF8(urlPtr);
			if (url == "about:inspector" || browser.newWindowAction == NewWindowAction.NewWindow) lock (browser.thingsToDo) {
				browser.thingsToDo.Add(() => {
					PopUpBrowser.Create(newBrowserId);
				});
				return;
			}
		#endif

		lock (browser.thingsToDo) {
			browser.thingsToDo.Add(() => {
				var newBrowser = browser.newWindowHandler.CreateBrowser(browser);
				newBrowser.RequestNativeBrowser(newBrowserId);
			});
		}
	}
}

}