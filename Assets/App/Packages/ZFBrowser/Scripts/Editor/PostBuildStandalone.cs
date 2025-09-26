using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Runtime.InteropServices;

namespace ZenFulcrum.EmbeddedBrowser {

/**
 * Getting CEF running on a build result requires some fiddling to get all the files in the right place.
 */
class PostBuildStandalone {

	static readonly List<string> byBinFiles = new List<string>() {
		"natives_blob.bin",
		"snapshot_blob.bin",
		"v8_context_snapshot.bin",
		"icudtl.dat",
	};

	[PostProcessBuild(10)]
	public static void PostprocessLinuxOrWindowsBuild(BuildTarget target, string buildFile) {
		//prereq
		var windows = target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64;
		var linux = target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinuxUniversal || target == BuildTarget.StandaloneLinux64;

		if (!windows && !linux) return;

		if (target == BuildTarget.StandaloneLinux || target == BuildTarget.StandaloneLinuxUniversal) {
			throw new Exception("ZFBrowser: Only x86_64 Linux is supported");
		}


		//base info
		string buildType;
		if (windows) buildType = "w" + (target == BuildTarget.StandaloneWindows64 ? "64" : "32");
		else buildType = "l64";

		Debug.Log("ZFBrowser: Post processing " + buildFile + " as " + buildType);

		string buildName;
		if (windows) buildName = Regex.Match(buildFile, @"/([^/]+)\.exe$").Groups[1].Value;
		else buildName = Regex.Match(buildFile, @"\/([^\/]+?)(\.x86(_64)?)?$").Groups[1].Value;

		var buildPath = Directory.GetParent(buildFile);
		var dataPath = buildPath + "/" + buildName + "_Data";
		var pluginsPath = dataPath + "/Plugins/";


		//start copying

		
		//can't use FileLocations because we may not be building the same type as the editor
		var platformPluginsSrc = ZFFolder + "/Plugins/" + buildType;

		//(Unity will copy the .dll and .so files for us)

		//Copy "root" .bin files
		foreach (var file in byBinFiles) {
			File.Copy(platformPluginsSrc + "/" + file, pluginsPath + file, true);
		}

		File.Copy(ZFFolder + "/ThirdPartyNotices.txt", pluginsPath + "/ThirdPartyNotices.txt", true);

		//Copy the needed resources
		var resSrcDir = platformPluginsSrc + "/CEFResources";
		foreach (var filePath in Directory.GetFiles(resSrcDir)) {
			var fileName = new FileInfo(filePath).Name;
			if (fileName.EndsWith(".meta")) continue;

			File.Copy(filePath, pluginsPath + fileName, true);
		}

		//Slave process (doesn't get automatically copied by Unity like the shared libs)
		var exeExt = windows ? ".exe" : "";
		File.Copy(
			platformPluginsSrc + "/" + FileLocations.SlaveExecutable + exeExt,
			pluginsPath + FileLocations.SlaveExecutable + exeExt,
			true
		);
		if (linux) MakeExecutable(pluginsPath + FileLocations.SlaveExecutable + exeExt);

		//Locales
		var localesSrcDir = platformPluginsSrc + "/CEFResources/locales";
		var localesDestDir = dataPath + "/Plugins/locales";
		Directory.CreateDirectory(localesDestDir);
		foreach (var filePath in Directory.GetFiles(localesSrcDir)) {
			var fileName = new FileInfo(filePath).Name;
			if (fileName.EndsWith(".meta")) continue;
			File.Copy(filePath, localesDestDir + "/" + fileName, true);
		}

		//Newer versions of Unity put the shared libs in the wrong place. Move them to where we expect them.
		if (linux && File.Exists(pluginsPath + "x86_64/zf_cef.so")) {
			foreach (var libFile in new[] {"zf_cef.so", "libEGL.so", "libGLESv2.so", "libwidevinecdmadapter.so", "libZFProxyWeb.so"}) {
				ForceMove(pluginsPath + "x86_64/" + libFile, pluginsPath + libFile);
			}
		}

		WriteBrowserAssets(dataPath + "/" + StandaloneWebResources.DefaultPath);
	}

	[PostProcessBuild(10)]
	public static void PostprocessMacBuild(BuildTarget target, string buildFile) {
#if UNITY_2017_3_OR_NEWER
		if (target != BuildTarget.StandaloneOSX) return;
#else
		if (target == BuildTarget.StandaloneOSXUniversal || target == BuildTarget.StandaloneOSXIntel) {
			throw new Exception("Only x86_64 is supported");
		}
		if (target != BuildTarget.StandaloneOSXIntel64) return;
#endif

		Debug.Log("Post processing " + buildFile);

		//var buildName = Regex.Match(buildFile, @"\/([^\/]+?)\.app$").Groups[1].Value;
		var buildPath = buildFile;
		var platformPluginsSrc = ZFFolder + "/Plugins/m64";

		//Copy app bits
		CopyDirectory(
			platformPluginsSrc + "/BrowserLib.app/Contents/Frameworks/Chromium Embedded Framework.framework",
			buildPath + "/Contents/Frameworks/Chromium Embedded Framework.framework"
		);
		CopyDirectory(
			platformPluginsSrc + "/BrowserLib.app/Contents/Frameworks/ZFGameBrowser.app",
			buildPath + "/Contents/Frameworks/ZFGameBrowser.app"
		);

		MakeExecutable(buildPath + "/BrowserLib.app/Contents/Frameworks/ZFGameBrowser.app/Contents/MacOS/ZFGameBrowser");

		if (!Directory.Exists(buildPath + "/Contents/Plugins")) Directory.CreateDirectory(buildPath + "/Contents/Plugins");
		File.Copy(platformPluginsSrc + "/libZFProxyWeb.dylib", buildPath + "/Contents/Plugins/libZFProxyWeb.dylib", true);

		File.Copy(ZFFolder + "/ThirdPartyNotices.txt", buildPath + "/ThirdPartyNotices.txt", true);

		//BrowserAssets
		WriteBrowserAssets(buildPath + "/Contents/" + StandaloneWebResources.DefaultPath);
	}


	private static void WriteBrowserAssets(string path) {
		//Debug.Log("Writing browser assets to " + path);

		var htmlDir = Application.dataPath + "/../BrowserAssets";
		var allData = new Dictionary<string, byte[]>();
		if (Directory.Exists(htmlDir)) {
			foreach (var file in Directory.GetFiles(htmlDir, "*", SearchOption.AllDirectories)) {
				var localPath = file.Substring(htmlDir.Length).Replace("\\", "/");
				allData[localPath] = File.ReadAllBytes(file);
			}
		}

		var wr = new StandaloneWebResources(path);
		wr.WriteData(allData);
	}

	private static void ForceMove(string src, string dest) {
		if (File.Exists(dest)) File.Delete(dest);
		File.Move(src, dest);
	}

	private static string ZFFolder {
		get {
			var path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
			path = Directory.GetParent(path).Parent.Parent.FullName;
			return path;
		}
	}

	private static void CopyDirectory(string src, string dest) {
		foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.AllDirectories)) {
			Directory.CreateDirectory(dir.Replace(src, dest));
		}

		foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories)) {
			if (file.EndsWith(".meta")) continue;
			File.Copy(file, file.Replace(src, dest), true);
		}
	}

	private static void MakeExecutable(string fileName) {
#if UNITY_EDITOR_WIN
		Debug.LogWarning("ZFBrowser: Be sure to mark the file \"" + fileName + "\" as executable (chmod +x) when you distribute it. If it's not executable the browser won't work.");
#else
		//dec 493 = oct 755 = -rwxr-xr-x
		chmod(fileName, 493);
#endif
	}


	[DllImport("__Internal")] static extern int symlink(string destStr, string symFile);
	[DllImport("__Internal")] static extern int chmod(string file, int mode);

}

}
