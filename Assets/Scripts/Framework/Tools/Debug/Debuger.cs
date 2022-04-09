
public class Debuger
{
    private static bool IsDebug = true;
    public static void Init(bool isDebug = true)
    {
        IsDebug = isDebug;
        Log("Debuger Init : {0}", IsDebug);
    }
    public static void Log(object obj)
    {
        if (!IsDebug) return;
        UnityEngine.Debug.Log(obj);
    }
    public static void Log(string format, params object[] args)
    {
        if (!IsDebug) return;
        UnityEngine.Debug.LogFormat(format, args);
    }
    public static void LogWarning(object obj)
    {
        if (!IsDebug) return;
        UnityEngine.Debug.LogWarning(obj);
    }
    public static void LogWarning(string format, params object[] args)
    {
        if (!IsDebug) return;
        UnityEngine.Debug.LogWarningFormat(format, args);
    }
    public static void LogError(object obj)
    {
        if (!IsDebug) return;
        UnityEngine.Debug.LogError(obj);
    }
    public static void LogError(string format, params object[] args)
    {
        if (!IsDebug) return;
        UnityEngine.Debug.LogErrorFormat(format, args);
    }
}
