using System.IO;
using System.Text;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public partial class NetcomManager : SingletonEvent<NetcomManager>
{
    public static string Token = "1837bc8456fe33e2df9a4fe17cf7ffb7cd392b0f63d77a9c";
    public static string Identifier = "10";
    public static string id = "e671d95cb4fb445e88c5e7540ad13177";
    /// <summary>
    /// 服务器地址
    /// </summary>
    public static string ServerUrl
    {
        private set { }
        get
        {
            string server = Root.AppConfig.IsProServer ? "" : "";//测试服务器:生产服务器
            return server;
        }
    }
    /// <summary>
    /// 服务器地址
    /// </summary>
    public static int Port
    {
        private set { }
        get
        {
            int port = Root.AppConfig.IsProServer ? 17888 : 8080;//测试服务器:生产服务器
            return port;
        }
    }
    #region AB
    public static string ABUrl
    {
        private set { }
        get
        {
            if (PlatformManager.Instance.IsEditor)
            {
                return Path.Combine(Application.dataPath.Replace("Assets", ""), "AssetBundle/");//本地AB包地址
            }
            else
            {
                return string.Format(@"{0}/{1}/", ServerUrl, "wp-content/uploads/AssetBundle/");//服务器AB包地址
            }
        }
    }
    #endregion
}