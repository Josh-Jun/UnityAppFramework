using System;
using System.Text;
using UnityEngine;

public class Log
{
    public static bool Enabled = true;

    private const string Info = "<color=#ffffff>[Info] </color>";
    private const string Warning = "<color=#ffff00>[Warning] </color>";
    private const string Error = "<color=#ff0000>[Error] </color>";

    private const string Space = " => ";

    private const string FormatStr    = " \"{0}\" : \"{1}\" ,";
    private const string FormatStrEnd = " \"{0}\" : \"{1}\"";

    private static StringBuilder cacheStringBuilder = new StringBuilder(1024);

    #region Info

    public static void I(object obj)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(obj);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T>(string msg, ValueTuple<string, T> args1)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args1.Item1, args1.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T1, T2>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args2.Item1, args2.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T1, T2, T3>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args3.Item1, args3.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T1, T2, T3, T4>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args4.Item1, args4.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T1, T2, T3, T4, T5>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args5.Item1, args5.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T1, T2, T3, T4, T5, T6>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args6.Item1, args6.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T1, T2, T3, T4, T5, T6, T7>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6, ValueTuple<string, T7> args7)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args6.Item1, args6.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args7.Item1, args7.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T1, T2, T3, T4, T5, T6, T7, T8>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6, ValueTuple<string, T7> args7,
        ValueTuple<string, T8> args8)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args6.Item1, args6.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args7.Item1, args7.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args8.Item1, args8.Item2);
        Debug.Log(cacheStringBuilder.ToString());
    }

    #endregion

    #region Warning

    public static void W(object obj)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(obj);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T>(string msg, ValueTuple<string, T> args1)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args1.Item1, args1.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T1, T2>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args2.Item1, args2.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T1, T2, T3>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args3.Item1, args3.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T1, T2, T3, T4>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args4.Item1, args4.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T1, T2, T3, T4, T5>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args5.Item1, args5.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T1, T2, T3, T4, T5, T6>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args6.Item1, args6.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T1, T2, T3, T4, T5, T6, T7>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6, ValueTuple<string, T7> args7)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args6.Item1, args6.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args7.Item1, args7.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    public static void W<T1, T2, T3, T4, T5, T6, T7, T8>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6, ValueTuple<string, T7> args7,
        ValueTuple<string, T8> args8)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args6.Item1, args6.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args7.Item1, args7.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args8.Item1, args8.Item2);
        Debug.LogWarning(cacheStringBuilder.ToString());
    }

    #endregion

    #region Error

    public static void E(object obj)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(obj);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T>(string msg, ValueTuple<string, T> args1)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args1.Item1, args1.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T1, T2>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args2.Item1, args2.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T1, T2, T3>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args3.Item1, args3.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T1, T2, T3, T4>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args4.Item1, args4.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T1, T2, T3, T4, T5>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args5.Item1, args5.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T1, T2, T3, T4, T5, T6>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args6.Item1, args6.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T1, T2, T3, T4, T5, T6, T7>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6, ValueTuple<string, T7> args7)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args6.Item1, args6.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args7.Item1, args7.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    public static void E<T1, T2, T3, T4, T5, T6, T7, T8>(string msg, ValueTuple<string, T1> args1,
        ValueTuple<string, T2> args2, ValueTuple<string, T3> args3, ValueTuple<string, T4> args4,
        ValueTuple<string, T5> args5, ValueTuple<string, T6> args6, ValueTuple<string, T7> args7,
        ValueTuple<string, T8> args8)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Error);
        cacheStringBuilder.Append(msg);
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStr, args1.Item1, args1.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args2.Item1, args2.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args3.Item1, args3.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args4.Item1, args4.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args5.Item1, args5.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args6.Item1, args6.Item2);
        cacheStringBuilder.AppendFormat(FormatStr, args7.Item1, args7.Item2);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args8.Item1, args8.Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    #endregion
}