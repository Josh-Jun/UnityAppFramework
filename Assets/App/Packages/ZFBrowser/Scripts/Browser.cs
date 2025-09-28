using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace ZenFulcrum.EmbeddedBrowser {

/** Represents a browser "tab". */
public partial class Browser : MonoBehaviour {
	private static int lastUpdateFrame;

	public static string LocalUrlPrefix { get { return BrowserNative.LocalUrlPrefix; } }

	/**
	 * List of possible actions when a new window is opened.
	 */
	[Flags]
	public enum NewWindowAction {
		/** Ignore attempts to open new windows. */
		Ignore = 1,
		/** Navigate the current window to the new window's URL. */
		Redirect,
		/**
		 * Create a new Browser instance to handle rendering the new window in the scene.
		 * For this to be useful, you'll need to supply an INewWindowHandler with an
		 * implementation of your choosing.
		 * (If you set this behavior in the inspector, it won't take effect until you call SetNewWindowHandler.)
		 */
		NewBrowser,
		/**
		 * Create a new OS window, outside the game, to show the page.
		 * Controlling and interacting with the new window outside is limited, though you can use JavaScript calls
		 * from the parent.
		 * OS-level windows may have unexpected or incomplete behavior. Using this outside of debugging/testing
		 * is not officially supported. Doesn't work with OS X+il2cpp.
		 */
		NewWindow,
	}

	protected IBrowserUI _uiHandler;
	protected bool uiHandlerAssigned = false;
	/**
	 * Input handler.
	 * If you don't assign anything, it will default to something useful, but you can replace
	 * it or null it as desired.
	 *
	 * If do you want to use your own or disable it, be sure to assign something (or null) before WhenReady fires.
	 */
	public IBrowserUI UIHandler {
		get { return _uiHandler; }
		set {
			uiHandlerAssigned = true;
			_uiHandler = value;
		}
	}

	[Tooltip("Initial URL to load.\n\nTo change at runtime use browser.Url to load a page.")]
	[SerializeField] private string _url = "";

	[Tooltip("Initial size.\n\nTo change at runtime use browser.Resize.")]
	[SerializeField] private int _width = 512, _height = 512;

	[Tooltip(@"Generate mipmaps?

Generating mipmaps tends to be somewhat expensive, especially when updating a large texture every frame. Instead of
generating mipmaps, try using one of the ""emulate mipmap"" shader variants.

To change at runtime modify this value and call browser.Resize.")]
	public bool generateMipmap = false;

	[Tooltip(@"Base background color to use for pages.

The texture will be cleared to this color until the page has rendered. Additionally, if baseColor.a is not
fully opaque the browser will render transparently. (Don't forget to use an appropriate material for transparency.)

Don't change this after the first Update() tick. (You can still tweak a page via EvalJS and CSS.)")]
	[FormerlySerializedAs("backgroundColor")]
	public Color32 baseColor = new Color32(0, 0, 0, 0);//default to transparent


	[Tooltip(@"Initial browser ""zoom level"". Negative numbers are smaller, zero is normal, positive numbers are larger.
The size roughly doubles/halves for every four units added/removed.
Note that zoom level is shared by all pages on the some domain.
Also note that this zoom level may be persisted across runs.

To change at runtime use browser.Zoom.")]
//TODO: prefer deviceScale (not yet implemented) for DPI-style size changes.
	[SerializeField] private float _zoom = 0;

	/**
	 * Fired when we get a console.log/warn/error from the page.
	 * args: (message, source)
	 *
	 * (CEF's console event leaves a lot to be desired, we are unable to get the log level or additional arguments.)
	 */
	public event Action<string, string> onConsoleMessage = (s, s1) => {};

	[Tooltip(@"Allow right-clicking to show a context menu on what parts of the page?

May be changed at any time.
")]
	[FlagsField]
	public BrowserNative.ContextMenuOrigin allowContextMenuOn = BrowserNative.ContextMenuOrigin.Editable;

	[Tooltip(@"What should we do when a user/the page tries to open a new window?

For ""New Browser"" to work, you need to assign NewWindowHandler to a handler of your creation.

Don't use ""New Window"" outside debugging and testing.

Use SetNewWindowHandler to adjust at runtime.
")]
	[SerializeField]
	private NewWindowAction newWindowAction = NewWindowAction.Redirect;

	[Obsolete("Use SetNewWindowHandler", true)]
	public INewWindowHandler NewWindowHandler { get; set; }

	/** If false, the texture won't be updated with new changes. */
	public bool EnableRendering { get; set; }
	/** If false, we won't process input with the UIHandler. */
	public bool EnableInput { get; set; }

	public CookieManager CookieManager { get; private set; }

	/** Handle to the native browser. */
	[NonSerialized]
	internal protected int browserId;
	/** Same as browserId, but will be set before the browser is ready and remain set even after it's disposed */
	private int unsafeBrowserId;
	/** Have we requested a native handle yet? (It may take a moment for the native browser to be ready.) */
	protected bool browserIdRequested = false;
	protected Texture2D texture;
	public Texture2D Texture { get { return texture; } }
	/** Called when the image canvas has changed or resized. */
	public event Action<Texture2D> afterResize = t => { };
	protected bool textureIsOurs = false;
	protected bool forceNextRender = true;
	protected bool isPopup = false;

	/** List of tasks to execute on the main thread. May be used on any thread, but lock before touching. */
	protected List<Action> thingsToDo = new List<Action>();
	/** List of callbacks to call when the page loads next. */
	protected List<Action> onloadActions = new List<Action>();

	/**
	 * We pass delegates/closures to the native level. We must ensure that they don't get GC'd
	 * while the native object still exists and might use them, so we keep track of them here
	 * to prevent that.
	 */
	protected List<object> thingsToRemember = new List<object>();
	/**
	 * And, to make it more complicated, in some cases we can get GC'd (along with thingsToRemember and the
	 * generated trampolines) before the native browser finishes shutting down.
	 *
	 * We use this to make sure {this} stays alive until the native side is done.
	 *
	 * Used across threads, lock before touching.
	 */
	protected static Dictionary<int, List<object>> allThingsToRemember = new Dictionary<int, List<object>>();

	/** A callback. {args} is a JSON node with the top-level type of array. */
	public delegate void JSCallback(JSONNode args);
	protected delegate void JSResultFunc(JSONNode value, bool isError);

	private int nextCallbackId = 1;
	/** Registered callbacks that JS can call to us with. */
	protected Dictionary<int, JSResultFunc> registeredCallbacks = new Dictionary<int, JSResultFunc>();


	/**
	 * We can't do much (go to url, navigate, etc) until the native browser is ready.
	 * Most these actions will be queued for you and fired when we are ready.
	 *
	 * See also: WhenReady()
	 */
	protected event BrowserNative.ReadyFunc onNativeReady;

	/**
	 * Called when the page's onload fires. (Top frame only.)
	 * loadData['status'] contains the status code, loadData['url'] the url
	 */
	public event Action<JSONNode> onLoad = loadData => {};
	/**
	 * Called when the top-level page has been fetched (but not necessarily parsed and run).
	 * loadData['status'] contains the status code, loadData['url'] the url
	 * (Top frame only.)
	 */
	[Obsolete("Doesn't fire reliably due to its design. Consider using onLoad or onNavStateChange.")]
	public event Action<JSONNode> onFetch = loadData => {};
	/**
	 * Called when a page fails to load.
	 * Use QueuePageReplacer to inject a custom error page.
	 * (Top frame only.)
	 * 
	 * Try visiting http://255.255.255.255/ to test.
	 */
	public event Action<JSONNode> onFetchError = errCode => {};
	/**
	 * Called when an SSL cert fails checks.
	 * Use QueuePageReplacer to inject a custom error page.
	 * (Top frame only.)
	 * 
	 * Try visiting https://wrong.host.badssl.com/ to test.
	 */
	public event Action<JSONNode> onCertError = errInfo => {};
	/**
	 * Called when a renderer process dies/is killed.
	 * Use QueuePageReplacer to inject a custom error page; you might also choose to try reloading once or twice.
	 * 
	 * Try visiting chrome://checkcrash/ to test.
	 */
	public event Action onSadTab = () => {};
	/**
	 * Called after the browser's texture/image data is updated.
	 */
	public event Action onTextureUpdated = () => {};

	/// <summary>
	/// Called when the browser's nav state changes.
	/// Presently these are considered nav state changes, but other things may be added in the future:
	///   - URL change
	///   - canGoForward/Back change
	///   - loading started or completed
	/// </summary>
	public event Action onNavStateChange = () => {};

	/**
	 * Called when a download is started.
	 * See BrowserNative.ChangeType.CHT_DOWNLOAD_STARTED for a list and explanation of
	 * the elements in the JSON object.
	 *
	 * If a handler is given it should call DownloadCommand() to start or cancel the download (eventually).
	 * Once a download is started onDownloadStatus will be called from time-to-time. Additionally, you can use
	 * DownloadCommand to cancel, pause, or resume a running download.
	 *
	 * If this is null, no downloading will happen.
	 */
	public Action<int, JSONNode> onDownloadStarted = null;

	/**
	 * Called when a download has a status update.
	 * See BrowserNative.ChangeType.CHT_DOWNLOAD_STATUS for a list and explanation of
	 * the elements in the JSON object.
	 *
	 * NB: You may get status reports on downloads that haven't triggered onDownloadStarted yet.
	 */
	public event Action<int, JSONNode> onDownloadStatus = (downloadId, info) => {};

	/**
	 * Called when the element in the page with keyboard focus changes.
	 * If tagName == "", then focus has been lost.
	 */
	public event Action<string, bool, string> onNodeFocus = (tagName, editable, value) => {};

	/// <summary>
	/// Called when the browser (as a whole) gains/loses keyboard or mouse focus.
	/// </summary>
	public event Action<bool, bool> onBrowserFocus = (mouseFocused, keyboardFocused) => {};

	[HideInInspector]
	public readonly BrowserFocusState focusState = new BrowserFocusState();

	/// <summary>
	/// Called when any browser is created.
	/// </summary>
	public static event Action<Browser> onAnyBrowserCreated = browser => {};
	/// <summary>
	/// Called when any browser is destroyed.
	/// </summary>
	public static event Action<Browser> onAnyBrowserDestroyed = browser => {};


	private BrowserInput browserInput;
	private Browser overlay;
	/** We have to load a blank page before we can inject HTML. If we load a blank page, don't count it as the "loading". */
	private bool skipNextLoad;
	/** There may be a short moment between requesting a URL and when IsLoadedRaw turns false. We use this flag to help cope. */
	private bool loadPending;

	private BrowserNavState navState = new BrowserNavState();

	private bool newWindowHandlerSet = false;
	/// <summary>
	/// If
	/// </summary>
	private INewWindowHandler newWindowHandler;

	/**
	 * This will sometimes contain an inner Browser that handles tasks such as
	 * rendering alert()s and such.
	 */
	protected DialogHandler dialogHandler;

	protected void Awake() {
		EnableRendering = true;
		EnableInput = true;
		CookieManager = new CookieManager(this);

		browserInput = new BrowserInput(this);

		if (!newWindowHandlerSet) {
			//(if another component calls SetNewWindowHandler in its Awake, we'll overwrite that
			//so only do this if it's not been called yet)
			SetNewWindowHandler(newWindowAction == NewWindowAction.NewBrowser ? NewWindowAction.Ignore : newWindowAction, null);
		}

		onNativeReady += id => {
			if (!uiHandlerAssigned) {
				var meshCollider = GetComponent<MeshCollider>();
				if (meshCollider) {
					var ui = gameObject.AddComponent<PointerUIMesh>();
					gameObject.AddComponent<CursorRendererOS>();
					UIHandler = ui;
				}
			}

			Resize(_width, _height);

			Zoom = _zoom;

			if (!isPopup && !string.IsNullOrEmpty(_url)) Url = _url;
		};

		onConsoleMessage += (message, source) => {
			var text = source + ": " + message;
			Debug.Log(text, this);
		};

		onFetchError += err => {
			//don't show anything if the error is a load abort
			if (err["error"] == "ERR_ABORTED") return;

			QueuePageReplacer(() => {
				LoadDataURI(ErrorGenerator.GenerateFetchError(err));
			}, -1000);
		};

		onCertError += err => {
			QueuePageReplacer(() => {
				LoadHTML(ErrorGenerator.GenerateCertError(err), Url);
			}, -900);
		};

		onSadTab += () => {
			// Try visiting chrome://checkcrash
			QueuePageReplacer(() => {
				//LoadHTML sometimes works, but LoadDataURI works more reliably
				LoadDataURI(ErrorGenerator.GenerateSadTabError());
			}, -1000);
		};

		onAnyBrowserCreated(this);
	}

	/** Returns true if the browser is ready to take orders. Most actions will be internally delayed until it is. */
	public bool IsReady {
		get { return browserId != 0; }
	}

	/**
	 * The given callback will be called when the browser is ready to start taking commands.
	 */
	public void WhenReady(Action callback) {
		if (IsReady) {
			//Call it later instead of now to help head off some subtle bugs that can be produced by such a scheme.
			//Call it at next update. (Since our script order is a little bit later than everyone else this usually will add no latency.)
			lock (thingsToDo) thingsToDo.Add(callback);
		} else {
			BrowserNative.ReadyFunc func = null;
			func = id => {
				try {
					callback();
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
				onNativeReady -= func;
			};
			onNativeReady += func;
		}
	}

	/** Fires the given callback during th next Update/LateUpdate tick on the main thread. This may be called from any thread. */
	public void RunOnMainThread(Action callback) {
		lock (thingsToDo) thingsToDo.Add(callback);
	}

	/**
	 * Calls the given callback the next time the page is loaded.
	 * This will not fire right away if IsLoaded is true, it will wait for a new page to load.
	 * Callbacks won't be fired yet if the url is some type of blank url ("", "about:blank", etc).
	 */
	public void WhenLoaded(Action callback) {
		onloadActions.Add(callback);
	}

	/**
	 * Sets up a new native browser.
	 * If newBrowserId is zero, allocates a new browser and sets it up.
	 * If newBrowserId is nonzero, takes ownership of that allocated browser and sets it up.
	 *
	 * Internal use only.
	 */
	internal void RequestNativeBrowser(int newBrowserId = 0) {
		if (browserId != 0 || browserIdRequested) return;

		browserIdRequested = true;

		try {
			BrowserNative.LoadNative();
		} catch {
			gameObject.SetActive(false);
			throw;
		}

		int newId;
		if (newBrowserId == 0) {
			var settings = new BrowserNative.ZFBSettings() {
				bgR = baseColor.r,
				bgG = baseColor.g,
				bgB = baseColor.b,
				bgA = baseColor.a,
				offscreen = 1,
			};
			newId = BrowserNative.zfb_createBrowser(settings);
		} else {
			newId = newBrowserId;
			isPopup = true;//don't nav to our to URL, it will be loaded by the backend
		}

		unsafeBrowserId = newId;
		allBrowsers[unsafeBrowserId] = this;

		//Debug.Log("Requested browser for " + name + " " + newId);

		//We have a native browser, but it is invalid to do anything with it until it's ready.
		//Therefore, we don't set browserId until it's ready.

		//But we will put all our callbacks in place.

		//Don't let our remember list get destroyed until we are ready for that.
		lock (allThingsToRemember) allThingsToRemember[newId] = thingsToRemember;

		//Set up callbacks:
		BrowserNative.ForwardJSCallFunc forwardCall = CB_ForwardJSCallFunc;
		thingsToRemember.Add(forwardCall);
		BrowserNative.zfb_registerJSCallback(newId, forwardCall);

		BrowserNative.ChangeFunc changeCall = CB_ChangeFunc;
		thingsToRemember.Add(changeCall);
		BrowserNative.zfb_registerChangeCallback(newId, changeCall);

		BrowserNative.DisplayDialogFunc dialogCall = CB_DisplayDialogFunc;
		thingsToRemember.Add(dialogCall);
		BrowserNative.zfb_registerDialogCallback(newId, dialogCall);

		BrowserNative.ShowContextMenuFunc contextCall = CB_ShowContextMenuFunc;
		thingsToRemember.Add(contextCall);
		BrowserNative.zfb_registerContextMenuCallback(newId, contextCall);

		BrowserNative.ConsoleFunc consoleCall = CB_ConsoleFunc;
		thingsToRemember.Add(consoleCall);
		BrowserNative.zfb_registerConsoleCallback(newId, consoleCall);

		BrowserNative.ReadyFunc readyCall = CB_ReadyFunc;
		thingsToRemember.Add(readyCall);
		BrowserNative.zfb_setReadyCallback(newId, readyCall);

		BrowserNative.NavStateFunc navStateCall = CB_NavStateFunc;
		thingsToRemember.Add(navStateCall);
		BrowserNative.zfb_registerNavStateCallback(newId, navStateCall);
	}

	protected void OnItemChange(BrowserNative.ChangeType type, string arg1) {
		//Debug.Log("ChangeType " + name + " " + type + " arg " + arg1 + " loaded " + IsLoaded);
		switch (type) {
			case BrowserNative.ChangeType.CHT_CURSOR:
				UpdateCursor();
				break;
			case BrowserNative.ChangeType.CHT_BROWSER_CLOSE:
				//handled directly on the calling thread, nothing to do here
				break;
			case BrowserNative.ChangeType.CHT_FETCH_FINISHED:
#pragma warning disable 618
				onFetch(JSONNode.Parse(arg1));
#pragma warning restore 618
				break;
			case BrowserNative.ChangeType.CHT_FETCH_FAILED:
				onFetchError(JSONNode.Parse(arg1));
				break;
			case BrowserNative.ChangeType.CHT_LOAD_FINISHED:
				if (skipNextLoad) {
					//deal with extra step we have to do to load HTML to an empty page
					skipNextLoad = false;
					return;
				}

				loadPending = false;

				navState.loading = false;//we'll get the event to update this in a moment, but we need to not be "loading" now

				if (onloadActions.Count != 0) {
					foreach (var action in onloadActions) action();
					onloadActions.Clear();
				}

				onLoad(JSONNode.Parse(arg1));
				break;
			case BrowserNative.ChangeType.CHT_CERT_ERROR:
				onCertError(JSONNode.Parse(arg1));
				break;
			case BrowserNative.ChangeType.CHT_SAD_TAB:
				onSadTab();
				break;
			case BrowserNative.ChangeType.CHT_DOWNLOAD_STARTED: {
				var info = JSONNode.Parse(arg1);
				if (onDownloadStarted != null) {
					onDownloadStarted(info["id"], info);
				} else {
					DownloadCommand(info["id"], BrowserNative.DownloadAction.Cancel);
				}
				break;
			}
			case BrowserNative.ChangeType.CHT_DOWNLOAD_STATUS: {
				var info = JSONNode.Parse(arg1);
				onDownloadStatus(info["id"], info);
				break;
			}
			case BrowserNative.ChangeType.CHT_FOCUSED_NODE: {
				var info = JSONNode.Parse(arg1);
				focusState.focusedTagName = info["TagName"];
				focusState.focusedNodeEditable = info["editable"];
				onNodeFocus(info["tagName"], info["editable"], info["value"]);
				break;
			}

		}
	}

	protected void CreateDialogHandler() {
		if (dialogHandler != null) return;

		DialogHandler.DialogCallback dialogCallback = (affirm, text1, text2) => {
			CheckSanity();
			BrowserNative.zfb_sendDialogResults(browserId, affirm, text1, text2);
		};
		DialogHandler.MenuCallback contextCallback = commandId => {
			CheckSanity();
			BrowserNative.zfb_sendContextMenuResults(browserId, commandId);
		};

		dialogHandler = DialogHandler.Create(this, dialogCallback, contextCallback);
	}

	/**
	 * Call this before you do any native things with our browser instance.
	 * If something terribly stupid is going on this may be able to bail out with an exception instead of
	 * crashing everything.
	 */
	protected void CheckSanity() {
		if (browserId == 0) throw new InvalidOperationException("No native browser allocated");
		if (!BrowserNative.SymbolsLoaded) throw new InvalidOperationException("Browser .dll not loaded");
	}

	/**
	 * If we aren't ready, queues the given action to happen later and returns true.
	 * Else calls CheckSanity and returns false.
	 */
	internal bool DeferUnready(Action ifNotReady) {
		if (browserId == 0) {
			WhenReady(ifNotReady);
			return true;
		} else {
			CheckSanity();
			return false;
		}
	}

	protected void OnDisable() {
		//note: if you want a browser to stop, load a different page or destroy it
		//The browser will continue to run until destroyed.
	}

	protected void OnDestroy() {
		onAnyBrowserDestroyed(this);

		if (browserId == 0) return;

		if (dialogHandler) DestroyImmediate(dialogHandler.gameObject);
		dialogHandler = null;

		if (BrowserNative.SymbolsLoaded) BrowserNative.zfb_destroyBrowser(browserId);
		if (textureIsOurs) Destroy(texture);
		//(Don't remove from allBrowsers here, there's some callbacks that need to happen first.)

		browserId = 0;
		texture = null;
	}

	public string Url {
		/**
		 * Gets the current browser URL.
		 * Note that if you just set the URL and the page hasn't loaded, this won't return the new value.
		 * It always returns the current URL of the browser as we are most recently aware of.
		 */
		get {
			return navState.url;
		}
		/** Shortcut for LoadURL(value, true) */
		set {
			LoadURL(value, true);
		}
	}

	/**
	 * Navigates to the given URL. If force is true, it will go there right away.
	 * If force is false, pages that wish to can prompt the user and possibly cancel the
	 * navigation.
	 */
	public void LoadURL(string url, bool force) {
		if (string.IsNullOrEmpty(url)) {
			//If we ask CEF to load "" it will crash. Try Url = "about:blank" or LoadHTML() instead.
			throw new ArgumentException("URL must be non-empty", "value");
		}

		if (DeferUnready(() => LoadURL(url, force))) return;

		const string magicPrefix = "localGame://";

		if (url.StartsWith(magicPrefix)) {
			url = LocalUrlPrefix + url.Substring(magicPrefix.Length);
		}

		loadPending = true;

		BrowserNative.zfb_goToURL(browserId, url, force);
	}

	/**
	 * Loads the given HTML string as if it were the given URL.
	 * For the URL use http://-like porotocols or else things may not work right. (In particular, the backend
	 * might sanitize it to "about:blank" and things won't work right because it appears a page isn't loaded.)
	 *
	 * Note that, instead of using this, you can also load "data:" URIs into this.Url.
	 * This allows pretty much any type of content to be loaded as the whole page.
	 */
	public void LoadHTML(string html, string url = null) {
		if (DeferUnready(() => LoadHTML(html, url))) return;

		//Debug.Log("Load HTML " + html);

		loadPending = true;

		if (string.IsNullOrEmpty(url)) {
			url = LocalUrlPrefix + "custom";
		}

		if (string.IsNullOrEmpty(this.Url)) {
			//Nothing will happen if we don't have an initial page, so cause one.
			this.Url = "about:blank";
			skipNextLoad = true;
		}

		BrowserNative.zfb_goToHTML(browserId, html, url);
	}

	/// <summary>
	/// Generates a data URI for the given content and loads that URI.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="mimeType"></param>
	public void LoadDataURI(string text, string mimeType = "text/html") {
		if (mimeType.StartsWith("text/") && !mimeType.Contains(";")) mimeType = mimeType + ";charset=UTF-8";
		LoadDataURI(Encoding.UTF8.GetBytes(text), mimeType);
	}

	/// <summary>
	/// Generates a data URI for the given content and loads that URI.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="mimeType"></param>
	public void LoadDataURI(byte[] data, string mimeType) {
		var dataStr = Convert.ToBase64String(data);
		this.Url = "data:" + mimeType + ";base64," + dataStr;
	}

	/// <summary>
	/// Sets how new popup windows are handled.
	/// </summary>
	/// <param name="action"></param>
	/// <param name="newWindowHandler">
	/// If action==NewBrowser, this handler will be invoked to create the browser in the scene.
	/// May be null otherwise.
	/// </param>
	public void SetNewWindowHandler(NewWindowAction action, INewWindowHandler newWindowHandler) {
		newWindowHandlerSet = true;
		if (action == NewWindowAction.NewBrowser && newWindowHandler == null) {
			throw new Exception("No new window handler supplied for NewBrowser action");
		}

		if (DeferUnready(() => SetNewWindowHandler(action, newWindowHandler))) return;

		var settings = new BrowserNative.ZFBSettings() {
			bgR = baseColor.r,
			bgG = baseColor.g,
			bgB = baseColor.b,
			bgA = baseColor.a,
		};

		this.newWindowHandler = newWindowHandler;
		this.newWindowAction = action;
		BrowserNative.NewWindowFunc cb = CB_NewWindowFunc;
		thingsToRemember.Add(cb);
		BrowserNative.zfb_registerPopupCallback(browserId, (BrowserNative.NewWindowAction)action, settings, cb);
	}

	/**
	 * Sends a command such as "select all", "undo", or "copy"
	 * to the currently focused frame in th browser.
	 */
	public void SendFrameCommand(BrowserNative.FrameCommand command) {
		if (DeferUnready(() => SendFrameCommand(command))) return;

		BrowserNative.zfb_sendCommandToFocusedFrame(browserId, command);
	}

	private Action pageReplacer;
	private float pageReplacerPriority;
	/**
	 * Queues a function to replace the current page.
	 *
	 * This is used mostly in error handling. Namely, the default error handler will queue an error page at a low
	 * priority, but your onLoadError callback can call this to queue its own error page.
	 *
	 * At the end of the tick, the {replacePage} callback with the highest priority will
	 * be called. Typically {replacePage} will call LoadHTML to change things around.
	 *
	 * Default error handles will have a priority of less than -100.
	 */
	public void QueuePageReplacer(Action replacePage, float priority) {
		if (pageReplacer == null || priority >= pageReplacerPriority) {
			pageReplacer = replacePage;
			pageReplacerPriority = priority;
		}
	}

	public bool CanGoBack {
		get {
			return navState.canGoBack;
		}
	}

	public void GoBack() {
		if (!IsReady) return;
		CheckSanity();
		BrowserNative.zfb_doNav(browserId, -1);
	}

	public bool CanGoForward {
		get {
			return navState.canGoForward;
		}
	}

	public void GoForward() {
		if (!IsReady) return;
		CheckSanity();
		BrowserNative.zfb_doNav(browserId, 1);
	}

	/**
	 * Returns true if the browser is loading a page.
	 * Unlike IsLoaded, this does not account for special case urls.
	 */
	public bool IsLoadingRaw {
		get {
			return navState.loading;
		}
	}

	/**
	 * Returns true if we have a page and it's loaded.
	 * This will not return true if we haven't gone to a URL or we are on "about:blank"
	 */
	public bool IsLoaded {
		get {
			if (!IsReady || loadPending) return false;
			if (navState.loading) return false;

			var url = Url;
			var urlIsBlank = string.IsNullOrEmpty(url) || url == "about:blank";

			return !urlIsBlank;
		}
	}

	public void Stop() {
		if (!IsReady) return;
		CheckSanity();
		BrowserNative.zfb_changeLoading(browserId, BrowserNative.LoadChange.LC_STOP);
	}

	/**
	 * Reloads the current page.
	 * If force is true, the cache will be skipped.
	 */
	public void Reload(bool force = false) {
		if (!IsReady) return;
		CheckSanity();
		if (force) BrowserNative.zfb_changeLoading(browserId, BrowserNative.LoadChange.LC_FORCE_RELOAD);
		else BrowserNative.zfb_changeLoading(browserId, BrowserNative.LoadChange.LC_RELOAD);
	}


	/**
	 * Show the development tools for the current page.
	 *
	 * If {show} is false the dev tools will be hidden, if possible.
	 *
	 * Like NewWindowAction.NewWindow using this outside of debugging/testing
	 * is not officially supported. Doesn't work with OS X+il2cpp.
	 */
	public void ShowDevTools(bool show = true) {
		if (DeferUnready(() => ShowDevTools(show))) return;

		BrowserNative.zfb_showDevTools(browserId, show);
	}

	public Vector2 Size {
		get { return new Vector2(_width, _height); }
	}

	protected void _Resize(Texture2D newTexture, bool newTextureIsOurs) {

		var width = newTexture.width;
		var height = newTexture.height;

		if (textureIsOurs && texture && newTexture != texture) {
			Destroy(texture);
		}

		_width = width;
		_height = height;

		if (IsReady) BrowserNative.zfb_resize(browserId, width, height);
		else WhenReady(() => BrowserNative.zfb_resize(browserId, width, height));

		texture = newTexture;
		textureIsOurs = newTextureIsOurs;

		var renderer = GetComponent<Renderer>();
		if (renderer) renderer.material.mainTexture = texture;

		afterResize(texture);

		if (overlay) overlay.Resize(Texture);

		forceNextRender = true;
	}

	/**
	 * Creates a new texture of the given size and starts rendering to that.
	 */
	public void Resize(int width, int height) {
		var newTexture = new Texture2D(width, height, TextureFormat.ARGB32, generateMipmap);
		if (generateMipmap) newTexture.filterMode = FilterMode.Trilinear;
		newTexture.wrapMode = TextureWrapMode.Clamp;

		//Clear it to a color:
		var pixelCount = width * height;
		if (newTexture.mipmapCount > 1) {
			//generateMipmap doesn't tell us how many or how big, so quick hack to look it up:
			for (int i = 1; i < newTexture.mipmapCount; i++) {
				pixelCount += newTexture.GetPixels32(i).Length;
			}
		}
		BrowserNative.LoadSymbols();
		var pixelData = BrowserNative.zfb_flatColorTexture(
			pixelCount, baseColor.r, baseColor.g, baseColor.b, baseColor.a
		);
		newTexture.LoadRawTextureData(pixelData, pixelCount * 4);
		newTexture.Apply();
		BrowserNative.zfb_free(pixelData);

		_Resize(newTexture, true);
	}

	/** Tells the Browser to render to the given ARGB32 texture. */
	public void Resize(Texture2D newTexture) {
		Assert.AreEqual(TextureFormat.ARGB32, newTexture.format);
		_Resize(newTexture, false);
	}

	/** Sets and gets the current zoom level/DPI scaling factor. */
	public float Zoom {
		get { return _zoom; }
		set {
			if (DeferUnready(() => Zoom = value)) return;

			BrowserNative.zfb_setZoom(browserId, value);
			_zoom = value;
		}
	}


	/// <summary>
	/// Evaluates JavaScript in the browser.
	///
	/// (This is JavaScript. Not UnityScript. If you try to feed this UnityScript it will choke and die.)
	///
	///  If IsLoaded is false, the script will be deferred until IsLoaded is true.
	///
	/// The script is asynchronously executed in a separate process.
	/// A promise (see the docs) is returned which you can use to inspect the last evaluated value.
	/// For example:
	///   browser.EvalJS("var a = 3; a + 3;").Then(ret => Debug.Log("Result: " + (int)ret).Done();
	///
	/// To see script errors and debug issues, call ShowDevTools and use the inspector window to tackle
	/// your problems. Also, keep an eye on console output (which gets forwarded to Debug.Log).
	///
	/// If desired, you can fill out scriptURL with a URL for the file you are reading from. This can help fill out errors
	/// with the correct filename and in some cases allow you to view the source in the inspector.
	///
	/// If the page you are viewing has a Content Security Policy (CSP) that prevents evaluating scripts
	/// don't use this function, use EvalJSCSP instead.
	/// </summary>
	/// <param name="script"></param>
	/// <param name="scriptURL"></param>
	/// <returns></returns>
	public IPromise<JSONNode> EvalJS(string script, string scriptURL = "scripted command") {
		//Debug.Log("Asked to EvalJS " + script + " loaded state: " + IsLoaded);
		var promise = new Promise<JSONNode>();
		var id = nextCallbackId++;

		var jsonScript = new JSONNode(script).AsJSON;
		var resultJS = @"try {"+
				"_zfb_event(" + id + ", JSON.stringify(eval(" + jsonScript + " )) || 'null');" +
			"} catch(ex) {" +
				"_zfb_event(" + id + ", 'fail-' + (JSON.stringify(ex.stack) || 'null'));" +
			"}"
		;

		registeredCallbacks.Add(id, (val, isError) => {
			registeredCallbacks.Remove(id);
			if (isError) promise.Reject(new JSException(val));
			else promise.Resolve(val);
		});

		if (!IsLoaded) WhenLoaded(() => _EvalJS(resultJS, scriptURL));
		else _EvalJS(resultJS, scriptURL);

		return promise;
	}

	/// <summary>
	/// Like EvalJS, but for pages with a Content Security Policy (CSP) that prevents evaluating scripts.
	/// This will also work on regular pages, but it has some disadvantages, keep reading.
	///
	/// Unlike EvalJS:
	///   If your script has syntax errors, you won't get the error in the returned promise, it will just not work.
	///   To inspect a result you must `return` it:
	///     browser.EvalJS("var a = 3; return a + 3;").Then(ret => Debug.Log("Result: " + (int)ret).Done();
	///
	/// This version of the function doens't use eval() to run the script. This keeps it from getting blocked by a
	/// Content Security Policy, but we will be unable to handle and report syntax errors.
	///
	/// Don't forget to respect the user's privacy and any applicable ToS terms for the page you are manipulating.
	/// </summary>
	/// <param name="script"></param>
	/// <param name="scriptURL"></param>
	/// <returns></returns>
	public IPromise<JSONNode> EvalJSCSP(string script, string scriptURL = "scripted command") {
		//Debug.Log("Asked to EvalJSCSP " + script + " loaded state: " + IsLoaded);
		var promise = new Promise<JSONNode>();
		var id = nextCallbackId++;

		var resultJS = @"try {"+
				"_zfb_event(" + id + ", JSON.stringify( (function() {" + script + "})() ) || 'null');" +
			"} catch(ex) {" +
				"_zfb_event(" + id + ", 'fail-' + (JSON.stringify(ex.stack) || 'null'));" +
			"}"
		;

		registeredCallbacks.Add(id, (val, isError) => {
			registeredCallbacks.Remove(id);
			if (isError) promise.Reject(new JSException(val));
			else promise.Resolve(val);
		});

		if (!IsLoaded) WhenLoaded(() => _EvalJS(resultJS, scriptURL));
		else _EvalJS(resultJS, scriptURL);

		return promise;
	}

	protected void _EvalJS(string script, string scriptURL) {
		BrowserNative.zfb_evalJS(browserId, script, scriptURL);
	}


	/**
	 * Looks up {name} by evaluating it as JavaScript code, then calls it with the given arguments.
	 *
	 * If IsLoaded is false, the call will be deferred until IsLoaded is true.
	 *
	 * Because {name} is evaluated, you can use lookups like "MyGUI.show" or "Foo.getThing().doBob"
	 *
	 * The call itself is run asynchronously in a separate process. To get the value returned by the JS back, yield
	 * on the promise CallFunction returns (in a coroutine) then take a look at promise.Value.
	 *
	 * Note that because JSONNode is implicitly convertible from many different types, you can often just
	 * dump the values in directly when you call this:
	 *   int x = 5, y = 47;
	 *   browser.CallFunction("Menu.setPosition", x, y);
	 *   browser.CallFunction("Menu.setTitle", "Super Game");
	 *
	 */
	public IPromise<JSONNode> CallFunction(string name, params JSONNode[] arguments) {
		var js = name + "(";

		var sep = "";
		foreach (var arg in arguments) {
			js += sep + (arg ?? JSONNode.NullNode).AsJSON;
			sep = ", ";
		}

		js += ");";

		return EvalJS(js);
	}

	/**
	 * Registers a JavaScript function in the Browser. When called, the given Mono {callback} will be executed.
	 *
	 * If IsLoaded is false, the in-page registration will be deferred until IsLoaded is true.
	 *
	 * The callback will be executed with one argument: a JSONNode array representing the arguments to the function
	 * given in the browser. (Access the first argument with args[0], second with args[1], etc.)
	 *
	 * The arguments sent back-and forth must be JSON-able.
	 *
	 * The JavaScript process runs asynchronously. Callbacks triggered will be collected and fired during the next Update().
	 *
	 * {name} is evaluate-assigned JavaScript. You can use values like "myCallback", "MySystem.myCallback" (only if MySystem
	 * already exists), or "GetThing().bobFunc" (if GetThing() returns an object you can use later).
	 *
	 */
	public void RegisterFunction(string name, JSCallback callback) {
		var id = nextCallbackId++;
		registeredCallbacks.Add(id, (value, error) => {
			//(we shouldn't be able to get an error here)
			callback(value);
		});

		var js = name + " = function() { _zfb_event(" + id + ", JSON.stringify(Array.prototype.slice.call(arguments))); };";
		EvalJS(js);
	}

	protected List<Action> thingsToDoClone = new List<Action>();
	protected void ProcessCallbacks() {
		while (thingsToDo.Count != 0) {
			Profiler.BeginSample("Browser.ProcessCallbacks", this);

			//It's not uncommon for some callbacks to add other callbacks
			//To keep from altering thingsToDo while iterating, we'll make a quick copy and use that.
			lock (thingsToDo) {
				thingsToDoClone.AddRange(thingsToDo);
				thingsToDo.Clear();
			}

			foreach (var thingToDo in thingsToDoClone) thingToDo();

			thingsToDoClone.Clear();

			Profiler.EndSample();
		}
	}

	protected void Update() {
		ProcessCallbacks();

		if (browserId == 0) {
			RequestNativeBrowser();
			return;//not ready yet or not loaded
		}

		if (!BrowserNative.SymbolsLoaded) return;

		HandleInput();
	}

	protected void LateUpdate() {
		//Note: we use LateUpdate here in hopes that commands issued during (anybody's) Update()
		//will have a better chance of being completed before we push the render

		if (lastUpdateFrame != Time.frameCount && BrowserNative.NativeLoaded) {
			Profiler.BeginSample("Browser.NativeTick");
			BrowserNative.zfb_tick();
			Profiler.EndSample();
			lastUpdateFrame = Time.frameCount;
		}


		if (browserId == 0) return;

		ProcessCallbacks();

		if (pageReplacer != null) {
			pageReplacer();
			pageReplacer = null;
		}

		if (browserId == 0) return;//not ready yet or not loaded
		if (EnableRendering) Render();
	}

	private Color32[] colorBuffer = null;

	protected void Render() {
		if (!BrowserNative.SymbolsLoaded) return;
		CheckSanity();

		BrowserNative.RenderData renderData;

		Profiler.BeginSample("Browser.UpdateTexture.zfb_getImage", this);
		try {
			renderData = BrowserNative.zfb_getImage(browserId, forceNextRender);
			forceNextRender = false;

			if (renderData.pixels == IntPtr.Zero) return;//no changes

			if (renderData.w != texture.width || renderData.h != texture.height) {
				//Mismatch. Can happen, for example, when we resize but got an "old" image at the old resolution. (IPC is async.)
				return;
			}
		} finally {
			Profiler.EndSample();
		}


		if (texture.mipmapCount == 1) {
			Profiler.BeginSample("Browser.UpdateTexture.LoadRawTextureData", this);
			texture.LoadRawTextureData(renderData.pixels, renderData.w * renderData.h * 4);
			Profiler.EndSample();
		} else {
			//whelp, this is gonna be slow.
			//First, having Unity calculate mipmaps is slow. Second, we can't just LoadRawTextureData because it doesn't
			//contain mip levels. Third, LoadRawTextureData and Color32 have different in-memory byte orders (for our texture type).

			if (colorBuffer == null || colorBuffer.Length != renderData.w * renderData.h) {
				colorBuffer = new Color32[renderData.w * renderData.h];
			}

			Profiler.BeginSample("Browser.UpdateTexture.CopyData", this);
			var handle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
			BrowserNative.zfb_copyToColor32(renderData.pixels, handle.AddrOfPinnedObject(), renderData.w * renderData.h);
			handle.Free();
			Profiler.EndSample();

			Profiler.BeginSample("Browser.UpdateTexture.SetPixels32", this);
			texture.SetPixels32(colorBuffer);
			Profiler.EndSample();
		}

		Profiler.BeginSample("Browser.UpdateTexture.Apply", this);
		{
			texture.Apply(true);
		}
		Profiler.EndSample();

		onTextureUpdated();
	}

	/**
	 * Adds the given browser as an overlay of this browser.
	 *
	 * The overlaid browser will appear transparently over the top of us on our texture.
	 * {overlayBrowser} must not have an overlay and must be sized exactly the same as {this}.
	 * Additionally, overlayBrowser.EnableRendering must be false. You still need to
	 * do something to handle getting input to the right places. Overlays take a notable performance
	 * hit on rendering (CPU alpha compositing).
	 *
	 * Overlays are used internally to implement context menus and pop-up dialogs (alert, onbeforeunload).
	 * If the page causes any type of dialog, the overlay will be replaced.
	 *
	 * Overlays will be resized onto our texture when we are resized. The sizes must always match exactly.
	 *
	 * Remove the overlay (SetOverlay(null)) before closing either browser.
	 *
	 * (Note: though you can't set B as an overlay to A when B has an overlay, you can set
	 * an overlay on B /while/ it is the overlay for A. For an example of this, try
	 * right-clicking on the text area inside a prompt() popup. The context menu that
	 * appears is an overlay to the overlay to the actual browser.)
	 */
	public void SetOverlay(Browser overlayBrowser) {
		if (DeferUnready(() => SetOverlay(overlayBrowser))) return;
		if (overlayBrowser && overlayBrowser.DeferUnready(() => SetOverlay(overlayBrowser))) return;

		if (!overlayBrowser) {
			BrowserNative.zfb_setOverlay(browserId, 0);
			overlay = null;
		} else {
			overlay = overlayBrowser;

			if (
				!overlay.Texture ||
				(overlay.Texture.width != Texture.width || overlay.Texture.height != Texture.height)
			) {
				overlay.Resize(Texture);
			}

			BrowserNative.zfb_setOverlay(browserId, overlayBrowser.browserId);
		}
	}

	protected void HandleInput() {
		if (_uiHandler == null || !EnableInput) return;
		CheckSanity();

		browserInput.HandleInput();
	}

	protected void OnApplicationFocus(bool focus) {
		if (!focus && browserInput != null) browserInput.HandleFocusLoss();
	}

	protected void OnApplicationPause(bool paused) {
		if (paused && browserInput != null) browserInput.HandleFocusLoss();
	}

	/**
	 * Updates the cursor on our UIHandler.
	 * Usually you don't need to call this, but if you are sharing input with an overlay, call this any time the
	 * "focused" overlay changes.
	 */
	public void UpdateCursor() {
		if (UIHandler == null) return;
		if (DeferUnready(UpdateCursor)) return;

		int w, h;
		var cursorType = BrowserNative.zfb_getMouseCursor(browserId, out w, out h);
		if (cursorType != BrowserNative.CursorType.Custom) {
			UIHandler.BrowserCursor.SetActiveCursor(cursorType);
		} else {
			if (w == 0 && h == 0) {
				//bad cursor
				UIHandler.BrowserCursor.SetActiveCursor(BrowserNative.CursorType.None);
				return;
			}

			var buffer = new Color32[w * h];
			int hx, hy;

			var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			BrowserNative.zfb_getMouseCustomCursor(browserId, handle.AddrOfPinnedObject(), w, h, out hx, out hy);
			handle.Free();

			var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
			tex.SetPixels32(buffer);
			//in-memory only, no Apply()

			UIHandler.BrowserCursor.SetCustomCursor(tex, new Vector2(hx, hy));
			DestroyImmediate(tex);
		}
	}

	/**
	 * Take an action on a download.
	 *
	 * At the outset:
	 *   Begin: Starts the download. Saves to the given file if given. If fileName is null, the user will be prompted.
	 *   Cancel: Does nothing with a download.
	 *
	 * After starting a download:
	 *   Pause, Cancel, Resume: Does what it says on the tin.
	 *
	 * Once a download is finished or canceled it is not valid to call this function for that download any more.
	 *
	 * fileName is ignored except when beginning a download.
	 */
	public void DownloadCommand(int downloadId, BrowserNative.DownloadAction action, string fileName = null) {
		CheckSanity();

		BrowserNative.zfb_downloadCommand(browserId, downloadId, action, fileName);
	}

	/// <summary>
	/// Injects the given unicode text to the browser as if it had been typed.
	/// (No key press events are generated.)
	/// </summary>
	/// <param name="text"></param>
	public void TypeText(string text) {
		for (int i = 0; i < text.Length; i++) {
			var ev = new Event() {
				type = EventType.KeyDown,
				keyCode = 0,
				character = text[i],
			};
			browserInput.extraEventsToInject.Add(ev);
		}
	}

	/// <summary>
	/// Sends key presses/releases to the browser.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="action"></param>
	public void PressKey(KeyCode key, KeyAction action = KeyAction.PressAndRelease) {
		if (action == KeyAction.Press || action == KeyAction.PressAndRelease) {
			var ev = new Event() {
				type = EventType.KeyDown,
				keyCode = key,
				character = (char)0,
			};
			browserInput.extraEventsToInject.Add(ev);
		}

		if (action == KeyAction.Release || action == KeyAction.PressAndRelease) {
			var ev = new Event() {
				type = EventType.KeyUp,
				keyCode = key,
				character = (char)0,
			};
			browserInput.extraEventsToInject.Add(ev);
		}

	}

	internal void _RaiseFocusEvent(bool mouseIsFocused, bool keyboardIsFocused) {
		focusState.hasMouseFocus  = mouseIsFocused;
		focusState.hasKeyboardFocus = keyboardIsFocused;
		onBrowserFocus(mouseIsFocused, keyboardIsFocused);
	}

}

}
