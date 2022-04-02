using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Socket
/// </summary>
public partial class NetcomManager : SingletonEvent<NetcomManager>
{
    #region TCP
    public void StartWanTcpClient(string ip, Action<byte[]> cb)
    {
        WanTcpManager.Instance.StartAsClient(ip, Port, (bool isConnect) => { });
        WanTcpManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
    }
    public void SendWanTcpMsg(byte[] bytes)
    {
        WanTcpManager.Instance.SendMsg(bytes);
    }
    public void WanTcpQuit()
    {
        WanTcpManager.Instance.Close();
    }
    #endregion
}