using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// Acts like a webserver for local files in Assets/../BrowserAssets.
/// To override this, extend the class and call `BrowserNative.webResources = myInstance`
/// before doing anything with Browsers.
/// 
/// Basic workflow: 
/// HandleRequest will get called when a browser needs something. From there you can either:
///   - Call SendPreamble, then SendData (any number of times), then SendEnd or
///   - Call one of the other Send* functions to send the whole response at once
/// Response sending is asynchronous, so you can do the above immediately, or after a delay.
/// 
/// Additionally, the Send* methods may be called from any thread given they are called in the right 
/// order and the right number of times.
/// 
/// </summary>
public abstract class WebResources {

	/// <summary>
	/// Mapping of file extension => HTTP mime type
	/// Treated as immutable.
	/// </summary>
	public static readonly Dictionary<string, string> extensionMimeTypes = new Dictionary<string, string>() {
		{"css", "text/css"},
		{"gif", "image/gif"},
		{"html", "text/html"},
		{"htm", "text/html"},
		{"jpeg", "image/jpeg"},
		{"jpg", "image/jpeg"},
		{"js", "application/javascript"},
		{"mp3", "audio/mpeg"},
		{"mpeg", "video/mpeg"},
		{"ogg", "application/ogg"},
		{"ogv", "video/ogg"},
		{"webm", "video/webm"},
		{"png", "image/png"},
		{"svg", "image/svg+xml"},
		{"txt", "text/plain"},
		{"xml", "application/xml"},

		//Need to add something? Some resources:
		// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Complete_list_of_MIME_types
		// http://www.iana.org/assignments/media-types/media-types.xhtml

		//Default/fallback
		{"*", "application/octet-stream"},
	};

	/// <summary>
	/// Mapping of status code to status text.
	/// Treated as immutable.
	/// </summary>
	public static readonly Dictionary<int, string> statusTexts = new Dictionary<int, string>() {
		// https://tools.ietf.org/html/rfc2616#section-10
		{100, "Continue"},
		{101, "Switching Protocols"},
		{200, "OK"},
		{201, "Created"},
		{202, "Accepted"},
		{203, "Non-Authoritative Information"},
		{204, "No Content"},
		{205, "Reset Content"},
		{206, "Partial Content"},
		{300, "Multiple Choices"},
		{301, "Moved Permanently"},
		{302, "Found"},
		{303, "See Other"},
		{304, "Not Modified"},
		{305, "Use Proxy"},
		{307, "Temporary Redirect"},
		{400, "Bad Request"},
		{401, "Unauthorized"},
		{402, "Payment Required"},
		{403, "Forbidden"},
		{404, "Not Found"},
		{405, "Method Not Allowed"},
		{406, "Not Acceptable"},
		{407, "Proxy Authentication Required"},
		{408, "Request Timeout"},
		{409, "Conflict"},
		{410, "Gone"},
		{411, "Length Required"},
		{412, "Precondition Failed"},
		{413, "Request Entity Too Large"},
		{414, "Request-URI Too Long"},
		{415, "Unsupported Media Type"},
		{416, "Requested Range Not Satisfiable"},
		{417, "Expectation Failed"},
		{500, "Internal Server Error"},
		{501, "Not Implemented"},
		{502, "Bad Gateway"},
		{503, "Service Unavailable"},
		{504, "Gateway Timeout"},
		{505, "HTTP Version Not Supported"},

		//Default/fallback
		{-1, ""},
	};

	public class ResponsePreamble {
		/// <summary>
		/// HTTP Status code (e.g. 200 for ok, 404 for not found)
		/// </summary>
		public int statusCode = 200;
		/// <summary>
		/// HTTP Status text ("OK", "Not Found", etc.)
		/// </summary>
		public string statusText = null;
		/// <summary>
		/// Content mime-type.
		/// </summary>
		public string mimeType = "text/plain; charset=UTF-8";
		/// <summary>
		/// Number of bytes in the response. -1 if unknown.
		/// If set >= 0, the number of bytes in the result need to match.
		/// </summary>
		public int length = -1;
		/// <summary>
		/// Any additional headers you'd like to send with the request
		/// </summary>
		public Dictionary<string, string> headers = new Dictionary<string, string>();
	}

	/// <summary>
	/// Called when a resource is requested. (Only GET requests are supported at present.)
	/// After this is called, eventually call one or more of the Send* functions with the given id
	/// to send the response (see class docs).
	/// </summary>
	/// <param name="id"></param>
	/// <param name="url"></param>
	public abstract void HandleRequest(int id, string url);

	/// <summary>
	/// Sends the full binary response to a request.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="data"></param>
	/// <param name="mimeType"></param>
	protected virtual void SendResponse(int id, byte[] data, string mimeType = "application/octet-stream") {
		var pre = new ResponsePreamble {
			headers = null,
			length = data.Length,
			mimeType = mimeType,
			statusCode = 200,
		};
		SendPreamble(id, pre);
		SendData(id, data);
		SendEnd(id);
	}

	/// <summary>
	/// Sends the full HTML or text response to a request.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="text"></param>
	/// <param name="mimeType"></param>
	protected virtual void SendResponse(int id, string text, string mimeType = "text/html; charset=UTF-8") {
		var data = Encoding.UTF8.GetBytes(text);

		var pre = new ResponsePreamble {
			headers = null,
			length = data.Length,
			mimeType = mimeType,
			statusCode = 200,
		};
		SendPreamble(id, pre);
		SendData(id, data);
		SendEnd(id);
	}

	/// <summary>
	/// Sends an HTML formatted error message.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="html"></param>
	/// <param name="errorCode"></param>
	protected virtual void SendError(int id, string html, int errorCode = 500) {
		var data = Encoding.UTF8.GetBytes(html);

		var pre = new ResponsePreamble {
			headers = null,
			length = data.Length,
			mimeType = "text/html; charset=UTF-8",
			statusCode = errorCode,
		};

		SendPreamble(id, pre);
		SendData(id, data);
		SendEnd(id);
	}

	protected virtual void SendFile(int id, FileInfo file, bool forceDownload = false) {
		new Thread(() => {
			try {
				if (!file.Exists) {
					SendError(id, "<h2>File not found</h2>", 404);
					return;
				}

				FileStream fileStream = null;
				try {
					fileStream = file.OpenRead();
				} catch (Exception ex) {
					Debug.LogException(ex);
					SendError(id, "<h2>File unavailable</h2>", 500);
					return;
				}

				string mimeType;
				var ext = file.Extension;
				if (ext.Length > 0) ext = ext.Substring(1).ToLowerInvariant();
				if (!extensionMimeTypes.TryGetValue(ext, out mimeType)) {
					mimeType = extensionMimeTypes["*"];
				}
				//Debug.Log("response type: " + mimeType + " extension " + file.Extension);

				var pre = new ResponsePreamble {
					headers = new Dictionary<string, string>(),
					length = (int)file.Length,
					mimeType = mimeType,
					statusCode = 200,
				};

				if (forceDownload) {
					pre.headers["Content-Disposition"] = "attachment; filename=\"" + file.Name.Replace("\"", "\\\"") + "\"";
				}

				SendPreamble(id, pre);

				int readCount = -1;
				byte[] buffer = new byte[Math.Min(pre.length, 32 * 1024)];
				while (readCount != 0) {
					readCount = fileStream.Read(buffer, 0, buffer.Length);
					SendData(id, buffer, readCount);
				}
				SendEnd(id);

				fileStream.Close();

			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}).Start();

	}

	/// <summary>
	/// Sends headers, status code, content-type, etc. for a request.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="pre"></param>
	protected void SendPreamble(int id, ResponsePreamble pre) {
		var headers = new JSONNode(JSONNode.NodeType.Object);
		if (pre.headers != null) {
			foreach (var kvp in pre.headers) {
				headers[kvp.Key] = kvp.Value;
			}
		}

		if (pre.statusText == null) {
			if (!statusTexts.TryGetValue(pre.statusCode, out pre.statusText)) {
				pre.statusText = statusTexts[-1];
			}
		}
		headers[":status:"] = pre.statusCode.ToString();
		headers[":statusText:"] = pre.statusText;
		headers["Content-Type"] = pre.mimeType;

		//Debug.Log("response headers " + headers.AsJSON);

		lock (BrowserNative.symbolsLock) {
			BrowserNative.zfb_sendRequestHeaders(id, pre.length, headers.AsJSON);
		}
	}

	/// <summary>
	/// Sends response body for the request. 
	/// Call as many times as you'd like.
	/// If you specified a length in the preamble make sure all writes add up to exactly that number of bytes.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="data"></param>
	/// <param name="length">How much of data to write, or -1 to send it all</param>
	protected void SendData(int id, byte[] data, int length = -1) {
		if (data == null || data.Length == 0 || length == 0) return;
		if (length < 0) length = data.Length;
		if (length > data.Length) throw new IndexOutOfRangeException();

		var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
		lock (BrowserNative.symbolsLock) {
			BrowserNative.zfb_sendRequestData(id, handle.AddrOfPinnedObject(), length);
		}
		handle.Free();
	}

	/// <summary>
	/// Call this after you are done calling SendData and you are ready to complete the response.
	/// </summary>
	/// <param name="id"></param>
	protected void SendEnd(int id) {
		lock (BrowserNative.symbolsLock) {
			BrowserNative.zfb_sendRequestData(id, IntPtr.Zero, 0);
		}
	}

}

}
