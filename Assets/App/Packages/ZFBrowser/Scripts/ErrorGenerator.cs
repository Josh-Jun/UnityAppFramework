namespace ZenFulcrum.EmbeddedBrowser {
/// <summary>
/// Generates HTML for errors for page load failures.
/// 
/// In the past we had a single static HTML page that we'd load, followed by a CallFunction() invocation to set the error
/// details. The JS call can be iffy at times when an error is going on, so now we just go with straight HTML and no JavaScript.
/// 
/// The Real Solution™ is to have some form of page interstitials and/or a proper error page concept.
/// </summary>
public static class ErrorGenerator {
	private const string htmlIntro = @"<!DOCTYPE html><html><head><meta charset=""UTF-8"">
<style type='text/css'>
	body { background: rgba(255, 255, 255, .7); color: black; }
	#loadError .detail { font-size: 90%; color: #444; }
</style></head><body>
";

	private const string htmlOutro = @"</body></html>";

	private const string loadTemplate = @"<div id='loadError'>
	<h1>Load Error</h1>
	<p class='mainError'>{mainError}</p>
	<p class='detail'>{detail}</p>
</div>";

	private const string sadTemplate = @"<div id='sadTab'>
	<h1>Page Crashed</h1>
	<p>The renderer process for this page is no longer running.</p>
</div>";

	/// <summary>
	/// Quick and dirty htmlspecialchars.
	/// (Given we don't have access to System.Net.WebUtility.HtmlDecode.)
	/// </summary>
	private static string Encode(string text) {
		return text.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
	}

	public static string GenerateFetchError(JSONNode err) {
		return htmlIntro + 
			loadTemplate
				.Replace("{mainError}", Encode("Failed to load " + err["url"]))
				.Replace("{detail}", Encode(err["error"])) + 
			htmlOutro
		;
	}

	public static string GenerateCertError(JSONNode err) {
		return htmlIntro + 
			loadTemplate
				.Replace("{mainError}", Encode("Failed to load " + err["url"]))
				.Replace("{detail}", Encode(err["error"])) + 
			htmlOutro
		;
	}

	public static string GenerateSadTabError() {
		return htmlIntro + sadTemplate + htmlOutro;
	}
}

}