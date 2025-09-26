using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_2017_3_OR_NEWER
using UnityEngine.Networking;
#endif

namespace ZenFulcrum.EmbeddedBrowser {

/**
 * WebResources implementation that grabs resources directly from Assets/../BrowserAssets.
 */
class EditorWebResources : WebResources {
	protected string basePath;

	public EditorWebResources() {
		//NB: If you try to read Application.dataPath later you may not be on the main thread and it won't work.
		basePath = Path.GetDirectoryName(Application.dataPath) + "/BrowserAssets"; 
	}

	/// <summary>
	/// Looks for "../asdf", "asdf/..\asdf", etc.
	/// </summary>
	private readonly Regex matchDots = new Regex(@"(^|[/\\])\.[2,]($|[/\\])");

	public override void HandleRequest(int id, string url) {
		var parsedURL = new Uri(url);

		#if UNITY_2017_3_OR_NEWER
			var path = UnityWebRequest.UnEscapeURL(parsedURL.AbsolutePath);
		#else
			var path = WWW.UnEscapeURL(parsedURL.AbsolutePath);
		#endif

		if (matchDots.IsMatch(path)) {
			SendError(id, "Invalid path", 400);
			return;
		}

		var file = new FileInfo(Application.dataPath + "/../BrowserAssets/" + path);

		if (!file.Exists) {
			SendError(id, "Not found", 404);
			return;
		}

		//fixme: send 404 if file capitalization doesn't match. (Unfortunately, not a quick one-liner)

		SendFile(id, file);
	}

}
}
