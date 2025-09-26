using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ZenFulcrum.EmbeddedBrowser {

/** 
 * Helper class for tracking and managing downloads.
 * You can manage and handle downloads without this, but you may find it useful for dealing with the more 
 * common file downloading use cases.
 * 
 * Usage: create one and call manager.ManageDownloads(browser) on a browser you want it to handle.
 * or throw one in the scene and set "manageAllBrowsers" to true (before loading the scene) for it 
 * to automatically hook into all browsers that start in the scene.
 */
public class DownloadManager : MonoBehaviour {

	[Tooltip("If true, this will find all the browser in the scene at startup and take control of their downloads.")]
	public bool manageAllBrowsers = false;

	[Tooltip("If true, a \"Save as\" style dialog will be given for all downloads.")]
	public bool promptForFileNames;

	[Tooltip("Where to save files. If null or blank, defaults to the user's downloads directory.")]
	public string saveFolder;

	[Tooltip("If given this text element will be updated with download status info.")] 
	public Text statusBar;

	public class Download {
		public Browser browser;
		public int downloadId;
		public string name;
		public string path;
		public int speed;
		public int percent;
		public string status;
	}

	public List<Download> downloads = new List<Download>();

	public void Awake() {
		if (manageAllBrowsers) {
			foreach (var browser in FindObjectsOfType<Browser>()) {
				ManageDownloads(browser);
			}
		}
	}

	public void ManageDownloads(Browser browser) {
		browser.onDownloadStarted = (id, info) => {
			HandleDownloadStarted(browser, id, info);
		};
		browser.onDownloadStatus += (id, info) => {
			HandleDownloadStatus(browser, id, info);
		};
	}


	private void HandleDownloadStarted(Browser browser, int downloadId, JSONNode info) {
		//Debug.Log("Download requested: " + info.AsJSON);

		var download = new Download {
			browser = browser,
			downloadId = downloadId,
			name = info["suggestedName"],
		};


		if (promptForFileNames) {
			browser.DownloadCommand(downloadId, BrowserNative.DownloadAction.Begin, null);
		} else {
			DirectoryInfo downloadFolder;
			if (string.IsNullOrEmpty(saveFolder)) {
				downloadFolder = new DirectoryInfo(GetUserDownloadFolder());
			} else {
				downloadFolder = new DirectoryInfo(saveFolder);
				if (!downloadFolder.Exists) downloadFolder.Create();
			}

			var filePath = downloadFolder.FullName + "/" + new FileInfo(info["suggestedName"]).Name;
			while (File.Exists(filePath)) {
				var ext = Path.GetExtension(filePath);
				var left = Path.GetFileNameWithoutExtension(filePath);

				var time = DateTime.Now.ToString("yyyy-MM-dd hh_mm_ss");
				filePath = downloadFolder.FullName + "/" + left + " " + time + ext;
			}
			browser.DownloadCommand(downloadId, BrowserNative.DownloadAction.Begin, filePath);
		}
		
		downloads.Add(download);
	}

	private void HandleDownloadStatus(Browser browser, int downloadId, JSONNode info) {
		//Debug.Log("Download status: " + info.AsJSON);

		for (int i = 0; i < downloads.Count; i++) {
			if (downloads[i].browser != browser || downloads[i].downloadId != downloadId) continue;

			var download = downloads[i];

			download.status = info["status"];
			download.speed = info["speed"];
			download.percent = info["percentComplete"];
			if (!string.IsNullOrEmpty(info["fullPath"])) download.name = Path.GetFileName(info["fullPath"]);

			break;
		}
	}

	public void Update() {
		if (statusBar) {
			statusBar.text = Status;
		}
	}

	public void PauseAll() {
		for (int i = 0; i < downloads.Count; i++) {
			if (downloads[i].status == "working") {
				downloads[i].browser.DownloadCommand(downloads[i].downloadId, BrowserNative.DownloadAction.Pause);
			}
		}
	}


	public void ResumeAll() {
		for (int i = 0; i < downloads.Count; i++) {
			if (downloads[i].status == "working") {
				downloads[i].browser.DownloadCommand(downloads[i].downloadId, BrowserNative.DownloadAction.Resume);
			}
		}
	}
	
	public void CancelAll() {
		for (int i = 0; i < downloads.Count; i++) {
			if (downloads[i].status == "working") {
				downloads[i].browser.DownloadCommand(downloads[i].downloadId, BrowserNative.DownloadAction.Cancel);
			}
		}
	}	

	public void ClearAll() {
		CancelAll();
		downloads.Clear();
	}

	private StringBuilder sb = new StringBuilder();
	/** Returns a string summarizing things that are downloading. */
	public string Status {
		get {
			if (downloads.Count == 0) return "";

			sb.Length = 0;
			var rate = 0;
			for (int i = downloads.Count - 1; i >= 0; i--) {
				if (sb.Length > 0) sb.Append(", ");
				sb.Append(downloads[i].name);

				if (downloads[i].status == "working") {
					if (downloads[i].percent >= 0) sb.Append(" (").Append(downloads[i].percent).Append("%)");
					else sb.Append(" (??%)");
					rate += downloads[i].speed;
				} else {
					sb.Append(" (").Append(downloads[i].status).Append(")");
				}
			}

			var ret = "Downloads";
			if (rate > 0) {
				ret += " (" + Mathf.Round(rate / (1024f * 1024) * 100) / 100f + "MiB/s)";
			}

			return ret + ": " + sb.ToString();
		}
	}

	/** Gets the user's download folder, creating it if needed. */
	public static string GetUserDownloadFolder() {
			switch (System.Environment.OSVersion.Platform) {
			case PlatformID.Win32NT: {

				IntPtr path;
				var r = SHGetKnownFolderPath(
					new Guid("{374DE290-123F-4565-9164-39C4925E467B}"), //downloads
					0x8000, //KF_FLAG_CREATE
					IntPtr.Zero, //current user
					out path
				);

				if (r == 0) {
					var ret = Marshal.PtrToStringUni(path);
					Marshal.FreeCoTaskMem(path);
					return ret;
				} else {
					throw new Exception(
						"Failed to get user download directory",
						new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error())
					);
				}
			}
			case PlatformID.Unix: { 
				var path = System.Environment.GetEnvironmentVariable("HOME") + "/Downloads";
				var di = new DirectoryInfo(path);
				if (!di.Exists) di.Create();
				return path;
			}
			case PlatformID.MacOSX:
				throw new NotImplementedException();
			default:
				throw new NotImplementedException();
		}
	}

	[DllImport("Shell32.dll")]
	private static extern int SHGetKnownFolderPath(
		[MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken,
		out IntPtr ppszPath
	);
}

}