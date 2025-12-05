using System;
using System.Collections.Generic;
using System.Linq;
using App.Core.Tools;

namespace App.Core.Master
{
    /// <summary>
    /// Socket局域网TCP客户端
    /// </summary>
    public partial class SocketMaster : SingletonMonoEvent<SocketMaster>
    {
        public void ConnectTcpServer(string ip, int port)
        {
            LanSocketTcpClient.Instance.ConnectServer(ip, port);
        }

        public void ClientSendMsgToServer(string eventName, string data)
        {
            var msg = GetClientGameMsg(LAN_CMD.CSMsg, eventName, data);
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        public void ClientSendMsgToAllClient(string eventName, string data)
        {
            var msg = GetClientGameMsg(LAN_CMD.CCMsg_All, eventName, data);
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        public void ClientSendMsgToOtherClient(string eventName, string data)
        {
            var msg = GetClientGameMsg(LAN_CMD.CCMsg_UnSelf, eventName, data);
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        public void ClientSendMsgToClient(string eventName, string data, string client)
        {
            var msg = GetClientGameMsg(LAN_CMD.CCMsg_One, eventName, data, new List<string> { client });
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        public void ClientSendMsgToClientList(string eventName, string data, List<string> clients)
        {
            var msg = GetClientGameMsg(LAN_CMD.CCMsg_List, eventName, data, clients);
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        /// <summary> 获取消息包 </summary>
        private PushMsg GetClientGameMsg(LAN_CMD cmd, string eventName, string data, List<string> clients = null)
        {
            return new PushMsg
            {
                cmd = (int)cmd,
                eventName = eventName,
                data = data,
                clients = clients
            };
        }

        public void ReceiveServerMsg(PushMsg msg)
        {
            var eventName = msg.eventName;
            if (string.IsNullOrEmpty(eventName))
            {
                Log.E("消息事件名不可为空！");
                return;
            }
            if (msg.data == null)
            {
                SendEventMsg(eventName);
            }
            else
            {
                SendEventMsg(eventName, msg.data);
            }
        }
    }
}