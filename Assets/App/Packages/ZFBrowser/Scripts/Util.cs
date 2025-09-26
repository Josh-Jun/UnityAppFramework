using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ZenFulcrum.EmbeddedBrowser {

public static class Util {

	/**
	 * Sometimes creating a culture in a different thread causes Mono to crash
	 * with mono_class_vtable_full.
	 *
	 * This variant of StartsWith won't try to use a culture.
	 */
	public static bool SafeStartsWith(this string check, string starter) {
		if (check == null || starter == null) return false;

		if (check.Length < starter.Length) return false;

		for (int i = 0; i < starter.Length; ++i) {
			if (check[i] != starter[i]) return false;
		}

		return true;
	}

	/// <summary>
	/// Converts a UTF8-encoded null-terminated string to a CLR string.
	/// </summary>
	/// <param name="strIn"></param>
	/// <returns></returns>
	public static string PtrToStringUTF8(IntPtr strIn) {
		if (strIn == IntPtr.Zero) return null;
		int strLen = 0;
		while (Marshal.ReadByte(strIn, strLen) != 0) ++strLen;
		var buffer = new byte[strLen];
		Marshal.Copy(strIn, buffer, 0, strLen);
		return Encoding.UTF8.GetString(buffer);
	}
}

public class JSException : Exception {
	public JSException(string what) : base(what) {}
}

public enum KeyAction {
	Press, Release, PressAndRelease
}

public class BrowserFocusState {
	public bool hasKeyboardFocus;
	public bool hasMouseFocus;

	public string focusedTagName;
	public bool focusedNodeEditable;
}

public class BrowserNavState {
	public bool canGoForward, canGoBack, loading;
	public string url = "";
}

}
