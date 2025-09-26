using System.Reflection;
using UnityEngine;

public class UniWebViewStaticListener {
    
    public static string InvokeStaticMethod(string method, string parameters)
    {
        MethodInfo methodInfo = typeof(UniWebViewStaticListener)
            .GetMethod(method, BindingFlags.Static | BindingFlags.Public);
        var result = methodInfo.Invoke(null, new object[] { parameters });
        return result as string;
    }
    
    public static void DebugLog(string value) {
        var payload = JsonUtility.FromJson<UniWebViewNativeResultPayload>(value);
        switch (payload.resultCode) {
            case "0":
                Debug.Log(payload.data);
                break;
            case "1":
                Debug.Log(payload.data);
                break;
            case "2":
                Debug.LogWarning(payload.data);
                break;
            case "3":
                Debug.LogError(payload.data);
                break;
            case "4":
                Debug.LogError(payload.data);
                break;
            default:
                Debug.Log(payload.data);
                break;
        }
    }

    public static void CookieOperation(string value) {
        var payload = JsonUtility.FromJson<UniWebViewNativeResultPayload>(value);
        UniWebView.InternalCookieOperation(payload);
    }
}