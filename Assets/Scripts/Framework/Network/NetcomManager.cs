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
            string server = Root.AppConfig.Server.Value ? "" : "";//测试服务器:生产服务器
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
            int port = Root.AppConfig.Server.Value ? 17888 : 8080;//测试服务器:生产服务器
            return port;
        }
    }
    #region AB
    public static string ABUrl
    {
        private set { }
        get
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                return string.Format(@"{0}{1}/", Application.dataPath.Replace("Assets", ""), "AssetBundle");//本地AB包地址
            }
            else
            {
                return string.Format(@"{0}/{1}/", ServerUrl, "wp-content/uploads/AssetBundle/");//服务器AB包地址
            }
        }
    }
    #endregion

    public string MakeUrl(string url, params object[] args)
    {
        StringBuilder sb = new StringBuilder(url);
        for (int i = 0; i < args.Length; i++)
        {
            sb.AppendFormat("/{0}", args[i]);
        }
        Debuger.Log("URL : " + sb.ToString());
        return sb.ToString();
    }
    public string MakeUrlWithToken(string url, params object[] args)
    {
        StringBuilder sb = new StringBuilder(url);
        sb.AppendFormat("?token={0}&identifier={1}", Token, Identifier);
        for (int i = 0; i < args.Length; i++)
        {
            sb.AppendFormat("/{0}", args[i]);
        }
        Debuger.Log("URL : " + sb.ToString());
        return sb.ToString();
    }

    #region IPAddress
    public static string IPAddress
    {
        get
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
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
    #endregion
}