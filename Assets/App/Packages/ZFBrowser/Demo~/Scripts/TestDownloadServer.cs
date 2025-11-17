using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {
/**
 * This runs s simple HTTP server to serve downloads. 
 * You could do this simpler by overriding WebResources except:
 *   You can't stream data (we need to test download rates/pause/resume).
 *   Don't want this example to override localGame either.
 */
public class TestDownloadServer : MonoBehaviour {
	private HttpListener server;
	public int port = 8083;

	private volatile bool serverEnabled = true;

	public void OnEnable() {
		server = new HttpListener();
		server.Prefixes.Add("http://localhost:" + port + "/");
		server.Start();
		serverEnabled = true;
		new Thread(ListenThread).Start();
	}

	private void ListenThread() {
		while (serverEnabled) {
			var context = server.GetContext();
			new Thread(ResponseThread).Start(context);
		}
	}

	private void ResponseThread(object obj) {
		var context = (HttpListenerContext)obj;

//		Debug.Log("request for " + context.Request.Url.AbsolutePath);
		var res = context.Response;
		res.StatusCode = 200;
		var output = new StreamWriter(res.OutputStream);

		Action sendSomeData = () => {
			var str = "Lorem ipsum dolor sit amet.\n";
			var count = 1024;
			res.AddHeader("Content-length", (str.Length * count).ToString());
			res.AddHeader("Content-type", "application/octet-stream");

			for (int i = 0; i < count; i++) {
				output.Write(str);
				Thread.Sleep(1);
			}
		};

		var path = context.Request.Url.AbsolutePath;
		switch (path) {
			case "/basicFile":
				sendSomeData();
				break;
			case "/bigFile": { 
				var str = "Lorem ipsum dolor sit amet.\n";
				long count = 1024 * 1024 * 100;

				res.AddHeader("Content-length", (str.Length * count).ToString());
				res.AddHeader("Content-type", "application/octet-stream");

				//For speed, prep a buffer to bulk move from.
				var strBytes = Encoding.ASCII.GetBytes(str);
				var buf = new byte[1024 * strBytes.Length];
				for (int i = 0; i < 1024; i++) Array.Copy(strBytes, 0, buf, i * strBytes.Length, strBytes.Length);

				//Send data
				for (int i = 0; i < count / 1024; i++) res.OutputStream.Write(buf, 0, buf.Length);
				break;
			}

			case "/slowFile":
			case "/slowPage": { 
				var str = "Lorem ipsum dolor sit amet.\n";
				var count = 1024 * 1024;

				res.AddHeader("Content-length", (str.Length * count).ToString());
				res.AddHeader("Content-type", path == "/slowFile" ? "application/octet-stream" : "text/plain");

				for (int i = 0; i < count; i++) {
					output.Write(str);
					Thread.Sleep(1);
				}
				break;
			}

			case "/textFile": { 
				res.AddHeader("Content-type", "text/plain");
				for (int i = 0; i < 100; i++) output.Write("This is some text!\n");
				break;
			}

			case "/textFileDownload": { 
				res.AddHeader("Content-type", "text/plain");
				res.AddHeader("Content-Disposition", "attachment; filename=\"A Great Document Full of Text.txt\"");
				for (int i = 0; i < 100; i++) output.Write("This is some text!\n");
				break;
			}

			case "/ǝpoɔıun«ñämé»":
			case "/%C7%9Dpo%C9%94%C4%B1un%C2%AB%C3%B1%C3%A4m%C3%A9%C2%BB":
				sendSomeData();
				break;

			case "/redirectedFile":
				res.StatusCode = 302;
				res.AddHeader("Location", "/some/other/file/i/was/redirected/to/redirectedResult");
				break;

			case "/some/other/file/i/was/redirected/to/redirectedResult":
				sendSomeData();
				break;

			default:
				context.Response.StatusCode = 404;
				output.Write("Not found");
				break;
		}

		output.Close();
	}

	public void OnDisable() {
		serverEnabled = false;
		server.Stop();
	}
}

}