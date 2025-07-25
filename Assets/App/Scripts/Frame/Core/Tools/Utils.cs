using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

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
                var output = "";
                foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
                {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                    NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                    if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) &&
                        item.OperationalStatus == OperationalStatus.Up)
#endif
                    {
                        foreach (var ip in item.GetIPProperties().UnicastAddresses)
                        {
                            //IPv4
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
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
            if (args == null || args.Length == 0) return null;
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