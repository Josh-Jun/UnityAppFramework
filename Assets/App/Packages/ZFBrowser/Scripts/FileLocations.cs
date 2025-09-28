using System;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZenFulcrum.EmbeddedBrowser {

public static class FileLocations {
	public const string SlaveExecutable = "ZFGameBrowser";

	private static CEFDirs _dirs;
	public static CEFDirs Dirs {
		get { return _dirs ?? (_dirs = GetCEFDirs()); }
	}

	public class CEFDirs {
		/** Where to find cef.pak, et al */
		public string resourcesPath;
		/** Where to find .dll, .so, natives_blob.bin, etc */
		public string binariesPath;
		/** Where to find en-US.pak et al */
		public string localesPath;
		/** The executable to run for browser processes. */
		public string subprocessFile;
		/** Editor/application log file */
		public string logFile;
		/** True if the given log file is also the Unity log. */
		public bool logFileIsUnityLog = true;
	}

	/// <summary>
	/// Tries to mimic Unity's game/company name to directory name mapping.
	/// For log folders at least.
	/// </summary>
	private static string GetFolderName(string name) {
		//Folders:
		//Com`~!@#$%^&*()_+-=[ ]\{}|;':",./<>?←→‽pany
		// becomes the Windows log folder
		//Com`_!@#__^__()_+-=[ ]_{}_;'__,.____←→‽pany
		// though it looks like the Editor analytics go to
		//Com`~!@#$_^&_()_+-=[ ]_{}_;'__,_____←→‽pany
		// And in the registry nothing is escaped and the name literally gets split across sub-folders:
		//   «Com`~!@#$%^&*()_+-=[ ]» with a folder in it called «{}|;':",./<>?←→‽pany»
		// And on Linux the log folder is
		//Com`~!@#$_^&_()_+-=[ ]_{}_;'__,_____←→‽pany
		// And the Linux analytics folder is
		//Com`~!@#$%^&_()_+-=[ ]_{}_;'__,.____←→‽pany
		// BUT WAIT THERES MORE!
		// Under Linux Unity 2019.2 we get
		//Com`_!@#__^__()_+-=[ ]_{}_;'__,.____←→‽pany

		//Game name:
		//Gam`~!@#$%^&*()_+-=[ ]\{}|;':",./<>?←→‽e
		// get the log folder
		//Gam`_!@#__^__()_+-=[ ]_{}_;'__,.____←→‽e
		// becomes the executable
		//Ga`~!@#$%^&()_+-=[]{};',.←→‽me.exe
		// and the Linux the log folder is
		//Gam`~!@#$%^&_()_+-=[ ]_{}_;'__,.____←→‽e
		// And the Linux analytics folder is
		//Gam`~!@#$_^&_()_+-=[ ]_{}_;'__,_____←→‽e
		// and Linux Unity 2019.2
		//Gam`_!@#__^__()_+-=[ ]_{}_;'__,.____←→‽e


		//Gam`~!@#$%^&*()_+-=[ ]\{}|;':",./<>?←→‽e src
		//Gam`_!@#__^__()_+-=[ ]_{}_;'__,.____←→‽e Windows
		//Gam`~!@#$%^&_()_+-=[ ]_{}_;'__,.____←→‽e Linux
		//Gam`_!@#__^__()_+-=[ ]_{}_;'__,.____←→‽e Linux 2019.2+
		char[] nixChars = {
			#if (UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)) || (UNITY_STANDALONE_LINUX && UNITY_2019_2_OR_NEWER && !UNITY_EDITOR)
				'~', '$', '%', '&',
			#endif
			'*', '\\', '|', ':', '"', '/', '<', '>', '?',
		};

		return nixChars.Aggregate(name, (current, ch) => current.Replace(ch, '_'));
	}

	private static CEFDirs GetCEFDirs() {
		//Note on "Editor-browser.log" and "Player-browser.log":
		//Starting with 2019.1 Unity seems to use a different method to write to the log. It no longer appends
		//to the log at the current end, but just keeps writing from the last position.
		//This causes our CEF log messages to get written over, so we use a separate file for CEF.

#if UNITY_EDITOR
		//In the editor we don't know exactly where we are at, but we can look up one of our scripts and move from there
		var guids = AssetDatabase.FindAssets("EditorWebResources");
		if (guids.Length != 1) throw new FileNotFoundException("Failed to locate a single EditorWebResources file");
		string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);

		// ReSharper disable once PossibleNullReferenceException
		var baseDir = Directory.GetParent(scriptPath).Parent.FullName + "/Plugins";
		string resourcesPath, localesDir;
		var platformDir = baseDir;

		#if UNITY_EDITOR_WIN
			#if UNITY_EDITOR_64
				platformDir += "/w64";
			#else
				platformDir += "/w32";
			#endif

			resourcesPath = platformDir + "/CEFResources";
			localesDir = resourcesPath + "/locales";

			//Silly MS.
			resourcesPath = resourcesPath.Replace("/", "\\");
			localesDir = localesDir.Replace("/", "\\");
			platformDir = platformDir.Replace("/", "\\");

			var subprocessFile = platformDir + "/" + SlaveExecutable + ".exe";
			var logFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Unity/Editor/";

		#elif UNITY_EDITOR_LINUX
			platformDir += "/l64";

			resourcesPath = platformDir + "/CEFResources";
			localesDir = resourcesPath + "/locales";

			var subprocessFile = platformDir + "/" + SlaveExecutable;
			var logFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/unity3d/";

		#elif UNITY_EDITOR_OSX
			platformDir += "/m64";

			resourcesPath = platformDir + "/BrowserLib.app/Contents/Frameworks/Chromium Embedded Framework.framework/Resources";
			localesDir = resourcesPath;

			//Chromium's base::mac::GetAppBundlePath will walk up the tree until it finds an ".app" folder and start
			//looking for pieces from there. That's why everything is hidden in a fake "BrowserLib.app"
			//folder that's not actually an app.
			var subprocessFile = platformDir + "/BrowserLib.app/Contents/Frameworks/" + SlaveExecutable + ".app/Contents/MacOS/" + SlaveExecutable;

			var logFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Library/Logs/Unity/";
		#else
			//If you want to build your app without ZFBrowser on some platforms change this to an exception or
			//tweak the .asmdef
			#error ZFBrowser is not supported on this platform
		#endif


		return new CEFDirs() {
			resourcesPath = resourcesPath,
			binariesPath = platformDir,
			localesPath = localesDir,
			subprocessFile = subprocessFile,
			#if UNITY_2019_1_OR_NEWER
				logFile = logFile + "Editor-browser.log",
				logFileIsUnityLog = false,
			#else
				logFile = logFile + "Editor.log",
			#endif
		};

#elif UNITY_STANDALONE_WIN
		var resourcesPath = Application.dataPath + "/Plugins";
		var logFileIsUnityLog = true;

		var logFile = Application.dataPath + "/output_log.txt";
		#if UNITY_2017_2_OR_NEWER
			var appLowDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low/" + GetFolderName(Application.companyName) + "/" + GetFolderName(Application.productName);
			if (Directory.Exists(appLowDir)) {
				#if UNITY_2019_1_OR_NEWER
					logFile = appLowDir + "/Player-browser.log";
					logFileIsUnityLog = false;
				#else
					logFile = appLowDir + "/output_log.txt";
				#endif
			}
		#endif

		return new CEFDirs() {
			resourcesPath = resourcesPath,
			binariesPath = resourcesPath,
			localesPath = resourcesPath + "/locales",
			subprocessFile = resourcesPath + "/" + SlaveExecutable + ".exe",
			logFile = logFile,
			logFileIsUnityLog = logFileIsUnityLog,
		};
#elif UNITY_STANDALONE_LINUX
		var resourcesPath = Application.dataPath + "/Plugins";

		#if !UNITY_2017_2_OR_NEWER // Unity 2017.1 and older
			//We can write to a log file of our choice, but that doesn't stop CEF from writing a copy of messages to stderr,
			//which Unity puts in the main log file anyway. (And at the start no less - I can't stop this form happening.)
			//We'll pass on having a separate log file.
			var logFile = "/dev/null";
			var logFileIsUnityLog = false;
		#elif !UNITY_2019_1 // Unity 2017.2+, except 2019.1
			//Newer versions of Unity don't copy stderr into the log file. (But they do copy stdout. :-/)
			var logFile = Environment.GetEnvironmentVariable("HOME");
			var logFileIsUnityLog = false;
			if (string.IsNullOrEmpty(logFile)) logFile = "/dev/null";
			else {
				logFile += "/.config/unity3d/" + GetFolderName(Application.companyName) + "/" + GetFolderName(Application.productName) + "/Player-browser.log";
				logFileIsUnityLog = false;
			}
		#else //Unity 2019.1.0
			//And here Unity just...I'm not sure what's going on. Their log location isn't consistent with their documentation.
			var logFile = Environment.GetEnvironmentVariable("HOME");
			var logFileIsUnityLog = false;
			if (string.IsNullOrEmpty(logFile)) logFile = "/dev/null";
			else {
				logFile += "/.config/unity3d/Editor/Player-browser.log";
				logFileIsUnityLog = false;
			}
		#endif

		return new CEFDirs() {
			resourcesPath = resourcesPath,
			binariesPath = resourcesPath,
			localesPath = resourcesPath + "/locales",
			subprocessFile = resourcesPath + "/" + SlaveExecutable,
			logFile = logFile,
			logFileIsUnityLog = logFileIsUnityLog,
		};
#elif UNITY_STANDALONE_OSX
		return new CEFDirs() {
			resourcesPath = Application.dataPath + "/Frameworks/Chromium Embedded Framework.framework/Resources",
			binariesPath = Application.dataPath + "/Plugins",
			localesPath = Application.dataPath + "/Frameworks/Chromium Embedded Framework.framework/Resources",
			subprocessFile = Application.dataPath + "/Frameworks/ZFGameBrowser.app/Contents/MacOS/" + SlaveExecutable,
			#if UNITY_2019_1_OR_NEWER
				logFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Library/Logs/Unity/Player-browser.log",
				logFileIsUnityLog = false,
			#else
				logFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Library/Logs/Unity/Player.log",
			#endif
		};
#else
		#error Web textures are not supported on this platform
#endif
	}
}

}
