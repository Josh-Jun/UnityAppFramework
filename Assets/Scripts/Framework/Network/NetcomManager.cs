using System.Text;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public partial class NetcomManager : SingletonEvent<NetcomManager>
{
    public static string ServerUrl = "https://www.shijunzh.com";
    public static string Token = "1837bc8456fe33e2df9a4fe17cf7ffb7cd392b0f63d77a9c";
    public static string Identifier = "10";
    public static string id = "e671d95cb4fb445e88c5e7540ad13177";
    public static int Port = 17888; //通信端口

    public string MakeUrl(string url, params object[] args)
    {
        StringBuilder sb = new StringBuilder(url);
        for (int i = 0; i < args.Length; i++)
        {
            sb.AppendFormat("/{0}", args[i]);
        }
        Debug.Log("URL : " + sb.ToString());
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
        Debug.Log("URL : " + sb.ToString());
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