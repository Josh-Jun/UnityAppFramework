using System;
using System.Collections.Generic;
using System.Threading;
using AOT;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

public class CookieManager {
	internal readonly Browser browser;


	public CookieManager(Browser browser) {
		this.browser = browser;
	}

	private class CookieFetch {
		public BrowserNative.GetCookieFunc nativeCB;
		public Promise<List<Cookie>> promise;
		public CookieManager manager;
		public List<Cookie> result;
	}

	private static CookieFetch currentFetch;

	/**
	 * Returns a list of all cookies in the browser across all domains.
	 *
	 * Note that cookies are shared between browser instances.
	 *
	 * If the browser is not ready yet (browser.IsReady or WhenReady()) this will return an empty list.
	 * 
	 * This method is not reentrant! You must wait for the returned promise to resolve before calling it again,
	 * even on a differnet object.
	 */
	public IPromise<List<Cookie>> GetCookies() {
		if (currentFetch != null) {
			//This method Wait for the previous promise to resolve, then make your call.
			//If this limitation actually affects you, let me know.
			throw new InvalidOperationException("GetCookies is not reentrant");
		}

		Cookie.Init();

		var result = new List<Cookie>();
		if (!browser.IsReady || !browser.enabled) return Promise<List<Cookie>>.Resolved(result);
		var promise = new Promise<List<Cookie>>();

		BrowserNative.GetCookieFunc cookieFunc = CB_GetCookieFunc;
		BrowserNative.zfb_getCookies(browser.browserId, cookieFunc);

		currentFetch = new CookieFetch {
			promise = promise,
			nativeCB = cookieFunc,
			manager = this,
			result = result,
		};

		return promise;
	}

	[MonoPInvokeCallback(typeof(BrowserNative.GetCookieFunc))]
	private static void CB_GetCookieFunc(BrowserNative.NativeCookie cookie) {
		try {
			if (cookie == null) {
				var result = currentFetch.result;
				var promise = currentFetch.promise;
				currentFetch.manager.browser.RunOnMainThread(() => promise.Resolve(result));
				currentFetch = null;
				return;
			}

			currentFetch.result.Add(new Cookie(currentFetch.manager, cookie));

		} catch (Exception ex) {
			Debug.LogException(ex);
		}
	}

	/**
	 * Deletes all cookies in the browser.
	 */
	public void ClearAll() {
		if (browser.DeferUnready(ClearAll)) return;

		BrowserNative.zfb_clearCookies(browser.browserId);
	}




}

}
