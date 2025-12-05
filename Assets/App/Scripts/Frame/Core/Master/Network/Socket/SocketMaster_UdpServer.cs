using System;
using System.Collections.Generic;
using App.Core.Tools;

namespace App.Core.Master
{
    /// <summary>
    /// Socket局域网UDP服务器
    /// </summary>
    public partial class SocketMaster : SingletonMonoEvent<SocketMaster>
    {
        public void StartUdpServer(int port)
        {
            LanSocketUdpServer.Instance.StartServer(port);
        }
        
        public void SendToUdpClient(string msg)
        {
            LanSocketUdpServer.Instance.SendMsg(msg);
        }
        
        public void ReceiveUdpClientMsg(string msg)
        {
            if (HasEvent("ReceiveUpdClientMsg"))
            {
                SendEventMsg("ReceiveUpdClientMsg", msg);
            }
        }
    }
}