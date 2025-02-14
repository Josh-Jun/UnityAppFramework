using System;
using System.Text;
using UnityEngine;

public class Log
{
    public static bool Enabled = true;

    private const string Info = "<color=#ffffff><b>[DEBUG] </b>\u25ba </color>";
    private const string Warning = "<color=#ffff00><b>[DEBUG] </b>\u25ba </color>";
    private const string Error = "<color=#ff0000><b>[DEBUG] </b>\u25ba </color>";

    private const string Space = "<color=#00ff00> \u25ba </color>";

    private const string FormatStr    = " \"{0}\" : \"{1}\" ,";
    private const string FormatStrEnd = " \"{0}\" : \"{1}\"";

    private static readonly StringBuilder cacheStringBuilder = new StringBuilder(1024);

    #region Info

    public static void I(object obj)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(obj);
        Debug.Log(cacheStringBuilder.ToString());
    }

    public static void I<T>(string msg, params ValueTuple<string, T>[] args)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Info);
        cacheStringBuilder.Append(msg);
        for (var i = 0; i < args.Length - 1; i++)
        {
            cacheStringBuilder.Append(Space);
            cacheStringBuilder.AppendFormat(FormatStr, args[i].Item1, args[i].Item2);
        }
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args[^1].Item1, args[^1].Item2);
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

    public static void W<T>(string msg, params ValueTuple<string, T>[] args)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        for (var i = 0; i < args.Length - 1; i++)
        {
            cacheStringBuilder.Append(Space);
            cacheStringBuilder.AppendFormat(FormatStr, args[i].Item1, args[i].Item2);
        }
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args[^1].Item1, args[^1].Item2);
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

    public static void E<T>(string msg, params ValueTuple<string, T>[] args)
    {
        if (!Enabled) return;
        cacheStringBuilder.Length = 0;
        cacheStringBuilder.Append(Warning);
        cacheStringBuilder.Append(msg);
        for (var i = 0; i < args.Length - 1; i++)
        {
            cacheStringBuilder.Append(Space);
            cacheStringBuilder.AppendFormat(FormatStr, args[i].Item1, args[i].Item2);
        }
        cacheStringBuilder.Append(Space);
        cacheStringBuilder.AppendFormat(FormatStrEnd, args[^1].Item1, args[^1].Item2);
        Debug.LogError(cacheStringBuilder.ToString());
    }

    #endregion
}