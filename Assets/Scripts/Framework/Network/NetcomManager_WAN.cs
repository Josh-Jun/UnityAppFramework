using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Socket
/// </summary>
public partial class NetcomManager : SingletonEvent<NetcomManager>
{
    #region TCP
    #region Server
    public void StartSocketTcpServer(string ip, Action<byte[]> cb)
    {
        WanTcpManager.Instance.StartAsServer(ip, Port, (bool isConnect)=> { });
        WanTcpManager.Instance.HandOutMsg += (byte[] bytes)=> { cb?.Invoke(bytes); };
    }
    #endregion
    #region Client
    public void StartSocketTcpClient(string ip, Action<byte[]> cb)
    {
        WanTcpManager.Instance.StartAsClient(ip, Port, (bool isConnect) => { });
        WanTcpManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    #endregion
    public void SendTcpMsg(byte[] bytes)
    {
        WanTcpManager.Instance.SendMsg(bytes);
    }
    public void SocketTcpQuit()
    {
        WanTcpManager.Instance.Close();
    }
    #endregion
    #region UDP
    #region Server
    public void StartSocketUdpServer(Action<byte[]> cb)
    {
        WanUdpManager.Instance.StartAsServer(Port, (bool isConnect) => { });
        WanUdpManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    #endregion
    #region Client
    public void StartSocketUdpClient(Action<byte[]> cb)
    {
        WanUdpManager.Instance.StartAsClient(Port, (bool isConnect) => { });
        WanUdpManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    #endregion
    public void SendUdpMsg(byte[] bytes)
    {
        WanUdpManager.Instance.SendMsg(bytes);
    }
    public void SocketUdpQuit()
    {
        WanUdpManager.Instance.Close();
    }
    #endregion
}