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
        SocketTCPManager.Instance.StartAsServer(ip, Port, (bool isConnect)=> { });
        SocketTCPManager.Instance.HandOutMsg += (byte[] bytes)=> { cb?.Invoke(bytes); };
    }
    #endregion
    #region Client
    public void StartSocketTcpClient(string ip, Action<byte[]> cb)
    {
        SocketTCPManager.Instance.StartAsClient(ip, Port, (bool isConnect) => { });
        SocketTCPManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    #endregion
    public void SendTcpMsg(byte[] bytes)
    {
        SocketTCPManager.Instance.SendMsg(bytes);
    }
    public void SocketTcpQuit()
    {
        SocketTCPManager.Instance.Close();
    }
    #endregion
    #region UDP
    #region Server
    public void StartSocketUdpServer(Action<byte[]> cb)
    {
        SocketUDPManager.Instance.StartAsServer(Port, (bool isConnect) => { });
        SocketUDPManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    #endregion
    #region Client
    public void StartSocketUdpClient(Action<byte[]> cb)
    {
        SocketUDPManager.Instance.StartAsClient(Port, (bool isConnect) => { });
        SocketUDPManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    #endregion
    public void SendUdpMsg(byte[] bytes)
    {
        SocketUDPManager.Instance.SendMsg(bytes);
    }
    public void SocketUdpQuit()
    {
        SocketUDPManager.Instance.Close();
    }
    #endregion
}