using System.IO;
using System.Text;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public partial class NetcomManager : SingletonMonoEvent<NetcomManager>
{
    public const string HttpAppID = "";
    public const string HttpAppSecret = "";
    /// <summary>
    /// Token验证
    /// </summary>
    public static string Token { get; set; }
    /// <summary>
    /// HTTP服务器地址
    /// </summary>
    public static string HttpAddress
    {
        private set { }
        get
        {
            string server = Root.AppConfig.IsProServer ? "" : "";//测试服务器:生产服务器
            return server;
        }
    }
    /// <summary>
    /// Socket服务器地址
    /// </summary>
    public static string SocketAddress
    {
        private set { }
        get
        {
            string server = Root.AppConfig.IsProServer ? "" : "";//测试服务器:生产服务器
            return server;
        }
    }
    /// <summary>
    /// Socket服务器端口
    /// </summary>
    public static int SocketPort
    {
        private set { }
        get
        {
            int port = Root.AppConfig.IsProServer ? 17888 : 8080;//测试服务器:生产服务器
            return port;
        }
    }
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
                return string.Format(@"{0}/{1}/", HttpAddress, "wp-content/uploads/AssetBundle/");//服务器AB包地址
            }
        }
    }
    public void InitManager()
    {
        transform.SetParent(App.app.transform);
    }
}