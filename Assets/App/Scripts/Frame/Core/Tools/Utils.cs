using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace App.Core.Tools
{
    public static class Utils
    {
        #region var

        /// <summary> 网络可用 </summary>
        public static bool NetAvailable => Application.internetReachability != NetworkReachability.NotReachable;

        /// <summary> 是否是无线 </summary>
        public static bool IsWifi => Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;

        /// <summary> 获取IP地址 </summary>
        public static string IPAddress
        {
            get
            {
                var output = "127.0.0.1";
                try
                {
                    // 获取本机所有网络接口
                    var host = Dns.GetHostEntry(Dns.GetHostName());

                    foreach (var ip in host.AddressList)
                    {
                        // 筛选IPv4地址且不是回环地址
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(ip))
                        {
                            output = ip.ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("获取IP地址失败: " + e.Message);
                }
                return output;
            }
        }

        /// <summary> 获取设备ID </summary>
        public static string DeviceId => SystemInfo.deviceUniqueIdentifier;

        /// <summary> 获取GUID </summary>
        public static string GUID => Guid.NewGuid().ToString();

        #endregion

        #region function
        
        /// <summary> 随机数 </summary>
        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary> 随机数 </summary>
        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary> Base64编码 </summary>
        public static string Encode(string message)
        {
            var bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
            return Convert.ToBase64String(bytes);
        }

        /// <summary> Base64解码 </summary>
        public static string Decode(string message)
        {
            var bytes = Convert.FromBase64String(message);
            return Encoding.GetEncoding("utf-8").GetString(bytes);
        }

        /// <summary> 判断角度 </summary>
        public static float CalculateAngle(Vector2 a, Vector2 b)
        {
            var direction = b - a;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return angle;
        }

        /// <summary> 清理内存 </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        public static Type[] GetObjsType(object[] args)
        {
            var types = new Type[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }
            return types;
        }
        #endregion
    }
}