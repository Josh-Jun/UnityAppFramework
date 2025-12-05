using System;
using System.Collections.Generic;
using App.Core.Tools;

namespace App.Core.Master
{
    /// <summary>
    /// Socket局域网UDP客户端
    /// </summary>
    public partial class SocketMaster : SingletonMonoEvent<SocketMaster>
    {
        public void ConnectUdpServer(int port)
        {
            LanSocketUdpClient.Instance.ConnectServer(port);
        }
        
        public void SendToUdpServer(string msg)
        {
            LanSocketUdpClient.Instance.SendMsg(msg);
        }
        
        public void ReceiveUdpServerMsg(string msg)
        {
            if (HasEvent("ReceiveUdpServerMsg"))
            {
                SendEventMsg("ReceiveUdpServerMsg", msg);
            }
        }
    }
}