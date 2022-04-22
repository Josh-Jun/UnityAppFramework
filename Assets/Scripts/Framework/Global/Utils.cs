using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.UI;
using System.Collections.Generic;

public static class Utils
{
    public static int Random(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    public static float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    public static string GetGuid()
    {
        return System.Guid.NewGuid().ToString();
    }
    public static long GetTimeStamp()
    {
        TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
        return (long)ts.TotalMilliseconds;
    }
    /// <summary>
    /// Base64编码
    /// </summary>
    public static string Encode(string message)
    {
        byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
        return Convert.ToBase64String(bytes);
    }
    /// <summary>
    /// Base64解码
    /// </summary>
    public static string Decode(string message)
    {
        byte[] bytes = Convert.FromBase64String(message);
        return Encoding.GetEncoding("utf-8").GetString(bytes);
    }
    /// <summary>
    /// 网络可用
    /// </summary>
    public static bool NetAvailable
    {
        get
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
    /// <summary>
    /// 是否是无线
    /// </summary>
    public static bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }
    public static void UnloadAsset(GameObject gameObj)
    {
        if (gameObj != null)
        {
            UnloadAssetType<Text>(gameObj);
            UnloadAssetType<Image>(gameObj);
        }
    }
    static void UnloadAssetType<T>(GameObject gameObj)
    {
        var components = gameObj.GetComponentsInChildren<T>();
        if (components.Length > 0)
        {
            Type compType = typeof(T);
            var assets = new List<UnityEngine.Object>();
            for (int i = 0; i < components.Length; i++)
            {
                var c = components[i];
                if (compType == typeof(Image))
                {
                    var image = c as Image;
                    if (image != null && image.sprite != null && !assets.Contains(image.sprite.texture))
                    {
                        assets.Add(image.sprite.texture);
                    }
                }
                else if (compType == typeof(Text))
                {
                    var text = c as Text;
                    if (text != null && !assets.Contains(text.font))
                    {
                        assets.Add(text.font);
                    }
                }
            }
            for (int i = 0; i < assets.Count; i++)
            {
                if (assets[i] != null)
                {
                    Resources.UnloadAsset(assets[i]);
                }
            }
            assets = null;
        }
    }
    /// <summary>
    /// 生成一个Key名
    /// </summary>
    public static string GetKey(string key)
    {
        return Application.productName + "_" + key;
    }
    /// <summary>
    /// 取得数据
    /// </summary>
    public static string GetString(string key)
    {
        string name = GetKey(key);
        return PlayerPrefs.GetString(name);
    }
    /// <summary>
    /// 保存数据
    /// </summary>
    public static void SetString(string key, string value)
    {
        string name = GetKey(key);
        PlayerPrefs.DeleteKey(name);
        PlayerPrefs.SetString(name, value);
    }
    /// <summary>
    /// 删除数据
    /// </summary>
    public static void DeleteString(string key)
    {
        string name = GetKey(key);
        PlayerPrefs.DeleteKey(name);
    }
    /// <summary>
    /// 判断数字
    /// </summary>
    public static bool IsNumeric(string str)
    {
        if (str == null || str.Length == 0) return false;
        for (int i = 0; i < str.Length; i++)
        {
            if (!Char.IsNumber(str[i])) { return false; }
        }
        return true;
    }
    /// <summary>
    /// 判断角度
    /// </summary>
    public static float PointToAngle(Vector2 p1, Vector2 p2)
    {
        float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;

        if (angle >= 0 && angle <= 180)
        {
            return angle;
        }
        else
        {
            return 360 + angle;
        }
    }
    /// <summary>
    /// 清理内存
    /// </summary>
    public static void ClearMemory()
    {
        GC.Collect(); Resources.UnloadUnusedAssets();
    }
    /// <summary>
    /// 获取设备ID
    /// </summary>
    public static string GetDeviceId()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("device ---> " + deviceId);
        return deviceId;
    }
}