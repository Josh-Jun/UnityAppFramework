//Set up some defines for what we are running on or compiled for right
//now - not what Editor will compile for later!
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
	#define ON_WINDOWS
#elif UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
	#define ON_OS_X
#elif UNITY_EDITOR_LINUX || (UNITY_STANDALONE_LINUX && !UNITY_EDITOR)
	#define ON_LINUX
#endif

#define PROXY_BROWSER_API


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Assertions;
using System.Reflection;
using AOT;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#endif



// ReSharper disable InconsistentNaming

namespace ZenFulcrum.EmbeddedBrowser {

/**
 * Wrapper/native callbacks for CEF browser implementation.
 * When changing code in this file you may have to restart the Unity Editor for things to get working again.
 *
 * Note that callbacks given to the native side may be invoked on any thread.
 *
 * Make sure IntPtrs are pinned and callbacks are kept alive from GC while their object lives.
 */
public static class BrowserNative {
#if UNITY_EDITOR
	public const int DebugPort = 9848;
#else
	public const int DebugPort = 9849;
#endif

	public static bool NativeLoaded { get; private set; }

	public static bool SymbolsLoaded { get; private set; }

	/// <summary>
	/// Lock this object before touching any of the zfb_* functions outside the main thread.
	/// (While many of them are thread safe, the shared library can be unloaded, leading
	/// to a possible race condition at shutdown. For example thread A grabs the value of zfb_sendRequestData,
	/// the main thread unloads the shared library, then thread A tries to execute the pointer it has for
	/// zfb_sendRequestData, resulting in sadness.)
	/// </summary>
	public static readonly object symbolsLock = new object();

#if PROXY_BROWSER_API
	public const bool UsingAPIProxy = true;
#else
	public const bool UsingAPIProxy = false;
#endif

	/**
	 * List of command-line switches given to Chromium.
	 * http://peter.sh/experiments/chromium-command-line-switches/
	 *
	 * If you want to change this, be sure to change it before LoadNative gets called.
	 *
	 * Adding or removing flags may lead to instability and/or insecurity.
	 * Make sure you understand what a flag does before you use it.
	 * Be sure to test your use cases thoroughly after changing any flags as
	 * things are more likely to crash or break if you aren't using the default
	 * configuration.
	 *
	 * Extra non-Chromium arguments:
	 *   --zf-cef-log-verbose
	 *     if enabled, we'll write a lot more CEF/Chromium logging to your editor/player log file than usual
	 *   --zf-log-internal
	 *     If enabled, some extra logs will be dumped to the current working directory.
	 *
	 */
	public static List<string> commandLineSwitches = new List<string>() {
		//Smooth scrolling tends to make scrolling act wonky or break.
		"--disable-smooth-scrolling",

		//If you install the PPAPI version of Flash on your system this tells Chromium to try to use it.
		//(download at https://get.adobe.com/flashplayer/otherversions/)
		"--enable-system-flash",
		//For Linux use probably need something like this instead (see docs)
		//"--ppapi-flash-version=32.0.0.223", "--ppapi-flash-path=/usr/lib/adobe-flashplugin/libpepflashplayer.so",

		//getUserMedia (microphone/webcam).
		//Turning this on has security implications, it appears there's no
		//CEF API for authorizing access, it just allows it. (ergo, any website can record the user)
		//"--enable-media-stream",

		//Enable these to get a higher browser framerate at the expense of more CPU usage:
		// "--disable-gpu-vsync",

		//If you want to specify a proxy by hand:
		//"--proxy-server=localhost:8000",

		//Allow videos to autoplay with sound.
		//  See https://developers.google.com/web/updates/2017/09/autoplay-policy-changes
		//  https://cs.chromium.org/chromium/src/media/base/media_switches.cc?l=182&gsn=kNoUserGestureRequiredPolicy
		//  Ideally this enables it. Sometimes it doesn't seem to work.
		//"--autoplay-policy=no-user-gesture-required",

		//"--zf-log-cef-verbose",
		//"--zf-log-internal",
	};

	/**
	 * WebResources used to resolve local requests.
	 *
	 * This may be replaced with an implementation of your choice, but be sure to set it up before requesting
	 * any URLs.
	 */
	public static WebResources webResources;
	public static string LocalUrlPrefix { get { return "https://game.local/"; } }

	[MonoPInvokeCallback(typeof(MessageFunc))]
	private static void LogCallback(string message) {
		Debug.Log("ZFWeb: " + message);
	}

	/// <summary>
	/// Because AppDomain.CurrentDomain.IsFinalizingForUnload() doesn't work and we don't like crashing the
	/// Unity Editor.
	/// </summary>
	private static bool isAppDomainUnloading = false;

	private static string _profilePath = null;
	/**
	 * Where should we save the user's data and cookies? Leave null to not save them.
	 * Set before the browser system initializes. Also, restart the Editor to apply changes.
	 */
	public static string ProfilePath {
		get { return _profilePath; }
		set {
			if (NativeLoaded) throw new InvalidOperationException("ProfilePath must be set before initializing the browser system.");
			_profilePath = value;
		}
	}

	/**
	 * Loads the shared library and the function symbols so we can call zfb_* functions.
	 */
	public static void LoadSymbols() {
		if (SymbolsLoaded) return;

		if (isAppDomainUnloading) {
			throw new Exception("Tried to start up browser backend while unloading app domain.");
		}

		var dirs = FileLocations.Dirs;

		HandLoadSymbols(dirs.binariesPath);
	}


	public static void LoadNative() {
		if (NativeLoaded) return;

		Profiler.BeginSample("BrowserNative.LoadNative");

		if (webResources == null) {
			//if the user hasn't given us a WebResources to use, use the default
#if UNITY_EDITOR
			webResources = new EditorWebResources();
#else
			var swr = new StandaloneWebResources(Application.dataPath + "/" + StandaloneWebResources.DefaultPath);
			swr.LoadIndex();
			webResources = swr;
#endif
		}


		//For Editor/debug builds, we'll open a port you can just http:// to inspect pages.
		//Don't do this for real builds, though. It makes it really, really easy for the end user to call
		//random JS in the page, potentially affecting or bypassing game logic.
		var debugPort = Debug.isDebugBuild ? DebugPort : 0;


		var dirs = FileLocations.Dirs;

		if (!dirs.logFileIsUnityLog) {
			//Unity doesn't rotate this log file for us, so nix the old, if any.
			var file = new FileInfo(dirs.logFile);
			try {
				if (file.Exists) file.Delete();
			} catch {
				//we'll just deal with it getting bigger
			}
		}

#if ON_OS_X || ON_LINUX
		FixProcessPermissions(dirs);
#endif


#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
		//make sure the child processes can be started (their dependent .dlls are next to the main .exe, not in the Plugins folder)
		var loadDir = Directory.GetParent(Application.dataPath).FullName;
		var path = Environment.GetEnvironmentVariable("PATH");
		path += ";" + loadDir;
		Environment.SetEnvironmentVariable("PATH", path);
#elif UNITY_EDITOR_WIN
		//help it find our .dlls
		var path = Environment.GetEnvironmentVariable("PATH");
		path += ";" + dirs.binariesPath;
		Environment.SetEnvironmentVariable("PATH", path);
#endif

		LoadSymbols();

		StandaloneShutdown.Create();

		//There never should be any, but just in case, destroy any existing browsers on a re-init
		zfb_destroyAllBrowsers();

		//Caution: Careful with these functions you pass to native. The Unity Editor will
		//reload assemblies, leaving the function pointers dangling. If any native calls try to use them
		//before we load back up and re-register them we can crash.
		//To prevent this, we call zfb_setCallbacksEnabled to disable callbacks before we get unloaded.
		zfb_setDebugFunc(LogCallback);
		zfb_setLocalRequestHandler(NewRequestCallback);
		zfb_setCallbacksEnabled(true);

		var settings = new ZFBInitialSettings() {
			cefPath = dirs.resourcesPath,
			localePath = dirs.localesPath,
			subprocessFile = dirs.subprocessFile,
			userAgent = UserAgent.GetUserAgent(),
			logFile = dirs.logFile,
			profilePath = _profilePath,
			debugPort = debugPort,
			multiThreadedMessageLoop = 0,//this argument is pretty much defunct, the slave just blocks on CefRunMessageLoop on all platforms
		};

		foreach (var arg in commandLineSwitches) zfb_addCLISwitch(arg);

		var initRes = zfb_init(settings);
		if (!initRes) throw new Exception("Failed to initialize browser system.");

		NativeLoaded = true;
		Profiler.EndSample();

		AppDomain.CurrentDomain.DomainUnload += (sender, args) => {
			isAppDomainUnloading = true;

			//Shutdown should happen StandaloneShutdown, but in some cases, like the Unity Editor
			//reloading assemblies, we don't get OnApplicationQuit because we didn't "quit", even though
			//everything gets shut down.
			//Make sure the backend shuts down, in this case, or it will crash when we try to start it again.
			UnloadNative();
		};
	}

	private static void FixProcessPermissions(FileLocations.CEFDirs dirs) {
		/*
		 * The package we get from the Asset Store probably won't have the right executable permissions for
		 * ZFGameBrowser for OS X, so let's fix that for the user right now.
		 */

		var attrs = (uint)File.GetAttributes(dirs.subprocessFile);

		//From https://github.com/mono/mono/blob/master/mono/io-layer/io.c under SetFileAttributes() (also noted in FileAttributes.cs):
		//"Currently we only handle one *internal* case [...]: 0x80000000, which means `set executable bit'"
		//Let's use that now.
		attrs |= 0x80000000;

		//Make it executable.
		File.SetAttributes(dirs.subprocessFile, unchecked((FileAttributes)attrs));
	}


	private static IntPtr moduleHandle;
	/// <summary>
	/// Loads the browser symbols.
	///
	/// We don't use DllImport for a few reasons, historically for DEEPBIND support and multiple CEF versions in the same process,
	/// but now mostly so we can unload the .dll whenever we want and to simplify picking and loading our shared library how we want.
	/// </summary>
	/// <param name="binariesPath"></param>
	private static void HandLoadSymbols(string binariesPath) {
		Profiler.BeginSample("BrowserNative.HandLoadSymbols");

#if PROXY_BROWSER_API
		var coreType = "ZFProxyWeb";
#else
		var coreType = "ZFEmbedWeb";
#endif

#if ON_OS_X
		var libFile = binariesPath + "/lib" + coreType + ".dylib";
#elif ON_LINUX
		var libFile = binariesPath + "/lib" + coreType + ".so";
#elif ON_WINDOWS
		var libFile = binariesPath + "/" + coreType + ".dll";
#else
	#error Unknown OS.
#endif

		//Debug.Log("Loading " + libFile);

		moduleHandle = OpenLib(libFile);

		//Now go through and fill our functions with life.
		int i = 0;
		var fields = typeof(BrowserNative).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (var field in fields) {
			if (!field.Name.StartsWith("zfb_")) continue;

			var fp = GetFunc(moduleHandle, field.Name);

			var func = Marshal.GetDelegateForFunctionPointer(fp, field.FieldType);
			field.SetValue(null, func);
			++i;
		}

		//Debug.Log("Loaded " + i + " symbols");

		SymbolsLoaded = true;

		Profiler.EndSample();
	}

	/// <summary>
	/// Clears out the symbols so, if the shared library has been unloaded, we get null exceptions instead
	/// of crashes.
	/// </summary>
	private static void ClearSymbols() {
		SymbolsLoaded = false;

		var fields = typeof(BrowserNative).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (var field in fields) {
			if (!field.Name.StartsWith("zfb_")) continue;
			field.SetValue(null, null);
		}
	}


	private static string GetLibError() {
#if ON_WINDOWS
		return new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
#else
		return Marshal.PtrToStringAnsi(dlerror());
#endif
	}

	private static IntPtr OpenLib(string name) {
#if ON_WINDOWS
		var handle = LoadLibraryW(name);
		if (handle == IntPtr.Zero) {
//			throw new DllNotFoundException("ZFBrowser failed to load " + name + ": " + Marshal.GetLastWin32Error());
			throw new DllNotFoundException("ZFBrowser failed to load " + name + ": " +
				new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message
			);
		}
		return handle;
#else
		//Call this now because running a DllImport method the first time will end up calling dlerror
		//which will clear the error we were trying to get from dlerror.
		dlerror();

		var handle = dlopen(name, (int)(DLFlags.RTLD_LAZY));
		if (handle == IntPtr.Zero) {
			throw new DllNotFoundException("ZFBrowser failed to load " + name + ": " + getDlError());
		}
		return handle;
#endif
	}

	private static void CloseLib() {
		if (moduleHandle == IntPtr.Zero) return;

		ClearSymbols();

#if ON_WINDOWS
		var success = FreeLibrary(moduleHandle);
#else
		var success = dlclose(moduleHandle) == 0;
#endif

		if (!success) {
			throw new DllNotFoundException(
				"Failed to unload library: " +
				GetLibError()
			);
		}

		//Debug.Log("Unloaded shared library");

		moduleHandle = IntPtr.Zero;
	}

	private static IntPtr GetFunc(IntPtr libHandle, string fnName) {
#if ON_WINDOWS
		var addr = GetProcAddress(libHandle, fnName);
		if (addr == IntPtr.Zero) {
			throw new DllNotFoundException("ZFBrowser failed to load method " + fnName + ": " + Marshal.GetLastWin32Error());
		}
		return addr;
#else
		var fp = dlsym(libHandle, fnName);
		if (fp == IntPtr.Zero) {
			throw new DllNotFoundException("ZFBrowser failed to load method " + fnName + ": " + getDlError());
		}
		return fp;
#endif
	}

#if !ON_WINDOWS
	[Flags]
	public enum DLFlags {
		RTLD_LAZY = 1,
		RTLD_NOW = 2,
		RTLD_DEEPBIND = 8,
	}
	[DllImport("__Internal")] static extern IntPtr dlopen(string filename, int flags);
	[DllImport("__Internal")] static extern IntPtr dlsym(IntPtr handle, string symbol);
	[DllImport("__Internal")] static extern int dlclose(IntPtr handle);
	[DllImport("__Internal")] static extern IntPtr dlerror();
	private static string getDlError() {
		var err = dlerror();
		return Marshal.PtrToStringAnsi(err);
	}

#else
	[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
	static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)]string lpFileName);
	[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
	[DllImport("kernel32", SetLastError = true)]
	private static extern bool FreeLibrary(IntPtr hModule);
#endif

	[MonoPInvokeCallback(typeof(NewRequestFunc))]
	private static void NewRequestCallback(int requestId, string url) {
		webResources.HandleRequest(requestId, url);
	}


	/** Shuts down the native browser library and CEF. */
	public static void UnloadNative() {
		if (!NativeLoaded) return;

		lock (symbolsLock) {
			//Debug.Log("Stop CEF");

			zfb_destroyAllBrowsers();
			zfb_shutdown();
			zfb_setCallbacksEnabled(false);
			NativeLoaded = false;
			CloseLib();
		}
	}


	/** Call this with a message to debug it to a console somewhere. */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void MessageFunc(string message);

	/**
	 * Callback for starting a new local request.
	 * url is the url requested
	 * At present only GET requests with no added headers are supported.
	 * After this is called, you are responsible for calling zfb_sendRequestHeaders (once)
	 * then zfb_sendRequestData (as much as needed) to finish up the request.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void NewRequestFunc(int requestId, string url);


	/** Called when the native backend is ready to start receiving orders. */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ReadyFunc(int browserId);

	/** Called on console.log, console.err, etc. */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ConsoleFunc(int browserId, string message, string source, int line);

	/**
	 * Called when JS calls back to us.
	 * callbackId is the first argument,
	 * data (UTF-8 null-terminated string) (and its included size) are the second argument.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ForwardJSCallFunc(int browserId, int callbackId, string data, int size);

	/**
	 * Called when a browser opens a new window.
	 * creatorBrowserId - id of the browser that cause the window to be created
	 * newBrowserId - a newly created (as if by zfb_createBrowser) browser tab
	 *
	 * May be called on any thread.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void NewWindowFunc(int creatorBrowserId, int newBrowserId, IntPtr initialURL);

	/**
	 * Called when an item from ChangeType happens.
	 * See the documentation for the given ChangeType for info on what the args mean or how to get more information.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ChangeFunc(int browserId, ChangeType changeType, string arg1);

	/**
	 * This is called when the browser wants to display a dialog of some sort.
	 * dialogType - the type, or DLT_HIDE to hide any existing dialogs.
	 * dialogText - main text for the dialog, usually from in-page JavaScript
	 * initialPromptText - if we are doing a JavaScript prompt(), the default text to display
	 * sourceURL - the URL of the page that is causing the dialog
	 *
	 * Once the user has responded to the dialog (if we were showing one), call zfb_sendDialogResults
	 * with the user's input.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void DisplayDialogFunc(
		int browserId, DialogType dialogType, IntPtr dialogText,
		IntPtr initialPromptText, IntPtr sourceURL
	);

	/**
	 * Called by the backend when a context menu should be shown or hidden.
	 * If menuJSON is null, hide the context menu.
	 * If it's not, show the given menu and eventually call zfb_sendContextMenuResults.
	 * For more information on the menu format, look at BrowserDialogs.html
	 *
	 * x and y report the position the menu was summoned, relative to the top-left of the view.
	 * origin indicates on what type of item the context menu was created.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ShowContextMenuFunc(int browserId, string menuJSON, int x, int y, ContextMenuOrigin origin);

	/**
	 * Used with zfb_getCookies, this will be called once for each cookie.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void GetCookieFunc(NativeCookie cookie);

	/**
	 * Called when nav state (can go back/forward, loaded, url) changes.
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void NavStateFunc(int browserId, bool canGoBack, bool canGoForward, bool lodaing, IntPtr url);

	/**
	 * Called by a native OS windows gets an event like mouse move click, etc.
	 * data contains json details on the event
	 */
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void WindowCallbackFunc(int windowId, IntPtr data);



	public enum LoadChange : int {
		LC_STOP = 1,
		LC_RELOAD,
		LC_FORCE_RELOAD,
	}

	public enum MouseButton : int {
		MBT_LEFT = 0,
		MBT_MIDDLE,
		MBT_RIGHT,
	}

	public enum ChangeType : int {
		/** The cursor has changed. Use zfb_getMouseCursor/zfb_getMouseCustomCursor to see what it is now. */
		CHT_CURSOR = 0,
		/** The browser has been closed and can no longer receive commands. */
		CHT_BROWSER_CLOSE,
		/**
		 * We have the HTML for the top-level page.
		 * arg1 (JSON) contains HTTP {status} code and the {url}
		 * Note that successfully fetching errors from a server (404, 500) are treated
		 * as successful, CHT_FETCH_FAILED won't be triggered.
		 */
		CHT_FETCH_FINISHED,
		/**
		 * Failed to fetch a page (timeout, network issues, etc)
		 * arg1 (JSON) contains an {error} code and the {url}
		 */
		CHT_FETCH_FAILED,
		/**
		 * The page has reached onload
		 * arg1 (JSON) contains HTTP {status} code and the {url}
		 */
		CHT_LOAD_FINISHED,
		/** SSL certificate error. arg1 has some JSON about the issue. Often followed by a CHT_FETCH_FAILED */
		CHT_CERT_ERROR,
		/** Renderer process crashed/was killed/etc. */
		CHT_SAD_TAB,
		/**
		 * The user/page has initialized a download.
		 * arg1 (JSON) contains:
		 *  download {id}
		 *  {mimeType}
		 *  {url} of the download
		 *  {originalURL} of the download before redirection
		 *  {suggestedName} you might save the file as
		 *  {size} number of bytes in the download (if known)
		 *  {contentDisposition}
		 *
		 * Call zfb_downloadCommand(browserId, download["id"], DownloadAction.xxyy, fileName) to cancel or save the file
		 * (and afterward control it).
		 */
		CHT_DOWNLOAD_STARTED,
		/**
		 * Progress/status update on a download.
		 * arg1 (JSON) contains:
		 *  download {id}
		 *  {speed} in bytes/sec
		 *  {percentComplete} int in [0, 100], or -1 if unknown
		 *  {received} number of bytes received
		 *  {statusStr} download status. One of: complete, canceled, working, unknown
		 *  {fullPath} we are saving to. (If you had the user pick the destination you can get it here.)
		 *
		 * Call zfb_downloadCommand(browserId, download["id"], DownloadAction.xxyy, null) to cancel/pause/resume the download.
		 */
		CHT_DOWNLOAD_STATUS,
		/**
		 * The element with keyboard focus has changed.
		 * You can use this to show/hide a keyboard when needed.
		 * arg1 (JSON) contains:
		 *  {tagName} of the focused node, or empty of no node is focused (focus loss)
		 *  {editable} true if it's some sort of editable text
		 *  textual {value} of the node, if it's simple (doesn't work for things like ContentEditable nodes)
		 */
		CHT_FOCUSED_NODE,
	}

	public enum DownloadAction {
		Begin,
		Cancel,
		Pause,
		Resume,
	}

	/** @see cef_cursor_type_t in cef_types.h */
	public enum CursorType : int {
		Pointer = 0,
		Cross,
		Hand,
		IBeam,
		Wait,
		Help,
		EastResize,
		NorthResize,
		NorthEastResize,
		NorthWestResize,
		SouthResize,
		SouthEastResize,
		SouthWestResize,
		WestResize,
		NorthSouthResize,
		EastWestResize,
		NorthEastSouthWestResize,
		NorthWestSouthEastResize,
		ColumnResize,
		RowResize,
		MiddlePanning,
		EastPanning,
		NorthPanning,
		NorthEastPanning,
		NorthWestPanning,
		SouthPanning,
		SouthEastPanning,
		SouthWestPanning,
		WestPanning,
		Move,
		VerticalText,
		Cell,
		ContextMenu,
		Alias,
		Progress,
		NoDrop,
		Copy,
		None,
		NotAllowed,
		ZoomIn,
		ZoomOut,
		Grab,
		Grabbing,
		Custom,
	}

	public enum DialogType {
		DLT_HIDE = 0,
		DLT_ALERT,
		DLT_CONFIRM,
		DLT_PROMPT,
		DLT_PAGE_UNLOAD,
		DLT_PAGE_RELOAD,//like unload, but the user is just refreshing the page
		DLT_GET_AUTH,
	};

	public enum NewWindowAction {
		NWA_IGNORE = 1,
		NWA_REDIRECT,
		NWA_NEW_BROWSER,
		NWA_NEW_WINDOW,
	};

	[Flags]
	public enum ContextMenuOrigin {
		Editable = 1 << 1,
		Image = 1 << 2,
		Selection = 1 << 3,
		Other = 1 << 0,
	}

	public enum FrameCommand {
		Undo,
		Redo,
		Cut,
		Copy,
		Paste,
		Delete,
		SelectAll,
		ViewSource,
	};

	public enum CookieAction {
		Delete,
		Create,
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ZFBInitialSettings {
		public string cefPath, localePath, subprocessFile, userAgent, logFile, profilePath;
		public int debugPort, multiThreadedMessageLoop;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ZFBSettings {
		public int bgR, bgG, bgB, bgA;
		public int offscreen;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RenderData {
		public IntPtr pixels;
		public int w, h;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class NativeCookie {
		public string name, value, domain, path;
		public string creation, lastAccess, expires;
		public byte secure, httpOnly;
	}



	/*
	 * See HandLoadSymbols() for an explanation of what's going on here and why we use a bunch of delegates instead
	 * of DllImport.
	 *
	 * Don't use this API directly unless you want to deal with things breaking.
	 * Though it is accessible, it's not considered part of the public API for versioning purposes.
	 * That, and you can shoot yourself in the foot and crash your app.
	 *
	 * Also, if you want to call any of these functions off the main thread make sure:
	 *   - It's documented as supporting such.
	 *   - To acquire a lock on symbolsLock first. (See its docs for why.)
	 */

	/** Does nothing. */
	public delegate void Calltype_zfb_noop();
	public static Calltype_zfb_noop zfb_noop;


	/**
	 * Allocates and initializes a block of memory suitable for use with LoadRawTextureData to clear a texture
	 * to the given color.
	 * Call zfb_free on the pointer when your are done using it.
	 * Does not require the browser system to be initialized, thread safe.
	 */
	public delegate IntPtr Calltype_zfb_flatColorTexture(int pixelCount, int r, int g, int b, int a);
	public static Calltype_zfb_flatColorTexture zfb_flatColorTexture;

	/**
	 * Copies from a zfb_getImage buffer to a Color32[] buffer.
	 * Does not require the browser system to be initialized, thread safe.
	 */
	public delegate void Calltype_zfb_copyToColor32(IntPtr src, IntPtr dest, int pixelCount);
	public static Calltype_zfb_copyToColor32 zfb_copyToColor32;

	/**
	 * Some functions allocate memory to give you a response (see their docs). Call this to free it.
	 * Does not require the browser system to be initialized, thread safe.
	 */
	public delegate void Calltype_zfb_free(IntPtr mem);
	public static Calltype_zfb_free zfb_free;


	/**
	 * Plain old memcpy. Because sometimes Marshal.Copy falls short of our needs.
	 * Does not require the browser system to be initialized, thread safe.
	 */
	public delegate void Calltype_zfb_memcpy(IntPtr dst, IntPtr src, int size);
	public static Calltype_zfb_memcpy zfb_memcpy;


	/**
	 * Returns the Chrome(ium) version as a static C string.
	 * Does not require the browser system to be initialized, thread safe.
	 */
	public delegate IntPtr Calltype_zfb_getVersion();
	public static Calltype_zfb_getVersion zfb_getVersion;


	/** Sets a function to call for Debug.Log-style messages. */
	public delegate void Calltype_zfb_setDebugFunc(MessageFunc debugFunc);
	public static Calltype_zfb_setDebugFunc zfb_setDebugFunc;


	/** Sets callbacks for when a local (https://game.local/) request is started. */
	public delegate void Calltype_zfb_setLocalRequestHandler(NewRequestFunc requestFunc);
	public static Calltype_zfb_setLocalRequestHandler zfb_setLocalRequestHandler;

	/**
	 * Sends the headers for a response.
	 * responseLength should be the number of bytes in the response, or -1 if unknown.
	 * headersJSON should contain a string:string map of headers, JSON encoded
	 *    - You should include a "Content-Type" header.
	 *		- You may also include a ":status:" pseudoheader to set the status to a non-200 value
	 *		- You may also include a ":statusText:" pseudoheader to set the status text
	 * After calling this, call zfb_sendRequestData to send the actual response body.
	 *
	 * May be called from any thread.
	 */
	public delegate void Calltype_zfb_sendRequestHeaders(int requestId, int responseLength, string headersJSON);
	public static Calltype_zfb_sendRequestHeaders zfb_sendRequestHeaders;

	/**
	 * Sends the body for a response after calling zfb_sendRequestHeaders.
	 *
	 * You must always write at least one byte except as described below.
	 *
	 * If you sent a responseLength >= 0, make sure all calls to this function add up to exactly that value.
	 * If you sent a responseLength < 0, call this a final time with size = 0 to signify the
	 * end of the response.
	 *
	 * May be called from any thread.
	 */
	public delegate void Calltype_zfb_sendRequestData(int requestId, IntPtr data, int dataSize);
	public static Calltype_zfb_sendRequestData zfb_sendRequestData;



	/** Enabled/disables user callbacks. Useful for disabling all callbacks when mono assemblies reload. */
	public delegate void Calltype_zfb_setCallbacksEnabled(bool enabled);
	public static Calltype_zfb_setCallbacksEnabled zfb_setCallbacksEnabled;


	/** Destroys all browser instances. */
	public delegate void Calltype_zfb_destroyAllBrowsers();
	public static Calltype_zfb_destroyAllBrowsers zfb_destroyAllBrowsers;


	/** Adds a command-line switch to Chromium, must call before zfb_init */
	public delegate void Calltype_zfb_addCLISwitch(string value);
	public static Calltype_zfb_addCLISwitch zfb_addCLISwitch;


	/** Initializes the system so we can start making browsers. */
	public delegate bool Calltype_zfb_init(ZFBInitialSettings settings);
	public static Calltype_zfb_init zfb_init;


	/** Shuts down the system. It cannot be re-initialized. */
	public delegate void Calltype_zfb_shutdown();
	public static Calltype_zfb_shutdown zfb_shutdown;


	/**
	 * Creates a new browser, returning the id.
	 * Call zfb_setReadyCallback and wait for it to fire before doing anything else.
	 */
	public delegate int Calltype_zfb_createBrowser(ZFBSettings settings);
	public static Calltype_zfb_createBrowser zfb_createBrowser;


	/** Reports the number of un-destroyed browsers. Slow. */
	public delegate int Calltype_zfb_numBrowsers();
	public static Calltype_zfb_numBrowsers zfb_numBrowsers;


	/**
	 * Closes and cleans up a browser instance.
	 */
	public delegate void Calltype_zfb_destroyBrowser(int id);
	public static Calltype_zfb_destroyBrowser zfb_destroyBrowser;


	/** Call once per frame if the multi-threaded message loop isn't enabled. */
	public delegate void Calltype_zfb_tick();
	public static Calltype_zfb_tick zfb_tick;


	/**
	 * Registers a function to call when the browser instance is ready to start taking orders.
	 * {cb} may be executed immediately or on any thread.
	 */
	public delegate void Calltype_zfb_setReadyCallback(int id, ReadyFunc cb);
	public static Calltype_zfb_setReadyCallback zfb_setReadyCallback;


	/** Resizes the browser. */
	public delegate void Calltype_zfb_resize(int id, int w, int h);
	public static Calltype_zfb_resize zfb_resize;


	/**
	 * Adds the given browser {overlayBrowserId} as an overlay of this browser {browserId}.
	 * The overlaid browser will appear transparently over the top of {browser}.
	 * {overlayBrowser} must not have an overlay and must be sized exactly the same as {browser}.
	 * Remove the overlay before closing either browser.
	 *
	 * While {overlayBrowser} is overlaying another browser, do not call zfb_getImage on it.
	 */
	public delegate void Calltype_zfb_setOverlay(int browserId, int overlayBrowserId);
	public static Calltype_zfb_setOverlay zfb_setOverlay;


	/**
	 * Gets the image data for the current frame.
	 * Do not hang onto the returned data across frames or resizes.
	 *
	 * If there are no changes since last call, the pixel data will be null (unless you specify forceDirty).
	 */
	public delegate RenderData Calltype_zfb_getImage(int id, bool forceDirty);
	public static Calltype_zfb_getImage zfb_getImage;

	/**
	 * Registers a callback for nav state updates.
	 * Keep track of what it tells you to answer questions like what the current URL is and if we can go back/forward.
	 * (The URL overlaps a bit with CHT_FETCH_*, but this should fire earlier (when we start) as opposed to when it's done.)
	 */
	public delegate void Calltype_zfb_registerNavStateCallback(int id, NavStateFunc callback);
	public static Calltype_zfb_registerNavStateCallback zfb_registerNavStateCallback;

	/**
	 * Navigates to the given URL. If force it ture, it will go there right away.
	 * If force is false, the pages that wish to can prompt the user and possibly cancel the
	 * navigation.
	 */
	public delegate void Calltype_zfb_goToURL(int id, string url, bool force);
	public static Calltype_zfb_goToURL zfb_goToURL;

	/**
	 * Loads the given HTML string as if it were the given URL.
	 * Use http://-like porotocols or else things may not work right.
	 */
	public delegate void Calltype_zfb_goToHTML(int id, string html, string url);
	public static Calltype_zfb_goToHTML zfb_goToHTML;

	/** Go back (-1) or forward (1) */
	public delegate void Calltype_zfb_doNav(int id, int direction);
	public static Calltype_zfb_doNav zfb_doNav;


	public delegate void Calltype_zfb_setZoom(int id, double zoom);
	public static Calltype_zfb_setZoom zfb_setZoom;


	/** Stop, refresh, or force-refresh */
	public delegate void Calltype_zfb_changeLoading(int id, LoadChange what);
	public static Calltype_zfb_changeLoading zfb_changeLoading;


	public delegate void Calltype_zfb_showDevTools(int id, bool show);
	public static Calltype_zfb_showDevTools zfb_showDevTools;


	/**
	 * Informs the browser if it's focused for keyboard input.
	 * Among other things, this controls if the blinking text cursor appears in an active text field.
	 */
	public delegate void Calltype_zfb_setFocused(int id, bool focused);
	public static Calltype_zfb_setFocused zfb_setFocused;


	/**
	 * Reports the mouse's current location.
	 * x and y are in the range [0,1]. (0, 0) is top-left, (1, 1) is bottom-right
	 */
	public delegate void Calltype_zfb_mouseMove(int id, float x, float y);
	public static Calltype_zfb_mouseMove zfb_mouseMove;


	public delegate void Calltype_zfb_mouseButton(int id, MouseButton button, bool down, int clickCount);
	public static Calltype_zfb_mouseButton zfb_mouseButton;


	/** Reports a mouse scroll. One "tick" of a scroll wheel is generally around 120 units. */
	public delegate void Calltype_zfb_mouseScroll(int id, int deltaX, int deltaY);
	public static Calltype_zfb_mouseScroll zfb_mouseScroll;


	/**
	 * Report a key down/up event. Repeated "virtual" keystrokes are simulated by repeating the down event without
	 * an interveneing up event.
	 */
	public delegate void Calltype_zfb_keyEvent(int id, bool down, int windowsKeyCode);
	public static Calltype_zfb_keyEvent zfb_keyEvent;


	/**
	 * Report a typed character. This typically interleaves with calls to zfb_keyEvent
	 */
	public delegate void Calltype_zfb_characterEvent(int id, int character, int windowsKeyCode);
	public static Calltype_zfb_characterEvent zfb_characterEvent;


	/** Register a function to call when console.log etc. is called in the browser. */
	public delegate void Calltype_zfb_registerConsoleCallback(int id, ConsoleFunc callback);
	public static Calltype_zfb_registerConsoleCallback zfb_registerConsoleCallback;


	public delegate void Calltype_zfb_evalJS(int id, string script, string scriptURL);
	public static Calltype_zfb_evalJS zfb_evalJS;


	/** Registers a callback to call when window._zfb_event(int, string) is called in the browser. */
	public delegate void Calltype_zfb_registerJSCallback(int id, ForwardJSCallFunc cb);
	public static Calltype_zfb_registerJSCallback zfb_registerJSCallback;


	/** Registers a callback that is called when something from ChangeType happens. */
	public delegate void Calltype_zfb_registerChangeCallback(int id, ChangeFunc cb);
	public static Calltype_zfb_registerChangeCallback zfb_registerChangeCallback;


	/**
	 * Gets the current mouse cursor. If the type is CursorType.Custom, width and height will be filled with
	 * the width and height of the custom cursor.
	 */
	public delegate CursorType Calltype_zfb_getMouseCursor(int id, out int width, out int height);
	public static Calltype_zfb_getMouseCursor zfb_getMouseCursor;


	/**
	 * Call this if zfb_getMouseCursor tells you there's a custom cursor.
	 * This will fill buffer (RGBA bottom-top, 4 bytes * width * height) with the contents of the cursor.
	 * Use the size you got from zfb_getMouseCursor.
	 * If width or height don't match the results from zfb_getMouseCursor, does nothing.
	 *
	 * {hotX} and {hoyY} will be filled with the cursor's hotspot.
	 */
	public delegate void Calltype_zfb_getMouseCustomCursor(int id, IntPtr buffer, int width, int height, out int hotX, out int hotY);
	public static Calltype_zfb_getMouseCustomCursor zfb_getMouseCustomCursor;


	/** Registers a DisplayDialogFunc for this browser. */
	public delegate void Calltype_zfb_registerDialogCallback(int id, DisplayDialogFunc cb);
	public static Calltype_zfb_registerDialogCallback zfb_registerDialogCallback;


	/** Callback for a dialog. See the docs on DisplayDialogFunc. */
	public delegate void Calltype_zfb_sendDialogResults(int id, bool affirmed, string text1, string text2);
	public static Calltype_zfb_sendDialogResults zfb_sendDialogResults;


	/** Registers a NewWindowFunc for pop ups. */
	public delegate void Calltype_zfb_registerPopupCallback(int id, NewWindowAction windowAction, ZFBSettings baseSettings, NewWindowFunc cb);
	public static Calltype_zfb_registerPopupCallback zfb_registerPopupCallback;


	/** Registers a ShowContextMenuFunc for the context menu. */
	public delegate void Calltype_zfb_registerContextMenuCallback(int id, ShowContextMenuFunc cb);
	public static Calltype_zfb_registerContextMenuCallback zfb_registerContextMenuCallback;


	/**
	 * After your ShowContextMenuFunc has been called,
	 * call this to report what item the user selected.
	 * If the menu was canceled, send -1.
	 */
	public delegate void Calltype_zfb_sendContextMenuResults(int id, int commandId);
	public static Calltype_zfb_sendContextMenuResults zfb_sendContextMenuResults;


	/**
	 * Sends a command, such as copy, paste, or select to the focused frame in the given browser.
	 */
	public delegate void Calltype_zfb_sendCommandToFocusedFrame(int id, FrameCommand command);
	public static Calltype_zfb_sendCommandToFocusedFrame zfb_sendCommandToFocusedFrame;


	/** Fetches all the cookies, calling the given callback for every cookie. */
	public delegate void Calltype_zfb_getCookies(int id, GetCookieFunc cb);
	public static Calltype_zfb_getCookies zfb_getCookies;


	/** Alters the given cookie as specified. */
	public delegate void Calltype_zfb_editCookie(int id, NativeCookie cookie, CookieAction action);
	public static Calltype_zfb_editCookie zfb_editCookie;


	/**
	 * Deletes all the cookies.
	 * (Though it takes a browser, this will typically clear all cookies for all browsers.)
	 */
	public delegate void Calltype_zfb_clearCookies(int id);
	public static Calltype_zfb_clearCookies zfb_clearCookies;

	/**
	 * Take an action on a download.
	 * fileName is ignored except when beginning a download.
	 * At the outset:
	 *   Begin: Starts the download. Saves to the given file if given. If fileName is null, the user will be prompted.
	 *   Cancel: Does nothing with a download.
	 * After starting a download:
	 *   Pause, Cancel, Resume: Does what it says on the tin.
	 * Once a download is finished or canceled it is not valid to call this function for that download any more.
	 */
	public delegate void Calltype_zfb_downloadCommand(int id, int downloadId, DownloadAction command, string fileName);
	public static Calltype_zfb_downloadCommand zfb_downloadCommand;


#if ON_OS_X
	/**
	 * Creates a new OS-native window in this process, returning an id.
	 */
	public delegate int Calltype_zfb_windowCreate(String title, WindowCallbackFunc eventHandler);
	public static Calltype_zfb_windowCreate zfb_windowCreate;

	/**
	 * Renders the contents of the given browser into the given OS window.
	 */
	public delegate void Calltype_zfb_windowRender(int windowId, int browserId);
	public static Calltype_zfb_windowRender zfb_windowRender;

	/**
	 * Closes the given window.
	 * Pass -1 for the id to close all windows.
	 */
	public delegate void Calltype_zfb_windowClose(int windowId);
	public static Calltype_zfb_windowClose zfb_windowClose;
#endif
}

}
