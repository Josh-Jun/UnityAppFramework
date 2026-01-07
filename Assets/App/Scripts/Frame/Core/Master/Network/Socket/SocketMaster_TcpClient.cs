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
        /// <summary> 连接服务端 </summary>
        public void ConnectTcpServer(string ip, int port, Action<bool> callback)
        {
            LanSocketTcpClient.Instance.ConnectServer(ip, port, callback);
        }
        
        /// <summary> 自动重连服务端 </summary>
        public void AutoReconnectTcpServer(string ip, int port, Action<bool> callback)
        {
            LanSocketTcpClient.Instance.AutoReconnect(ip, port, callback);
        }

        /// <summary> 客户端发送消息给服务端 </summary>
        public void ClientSendMsgToServer(string eventName, string data)
        {
            var msg = GetClientGameMsg(LAN_CMD.CSMsg, eventName, data);
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        /// <summary> 客户端发送消息给所有客户端（包括自己） </summary>
        public void ClientSendMsgToAllClient(string eventName, string data)
        {
            var msg = GetClientGameMsg(LAN_CMD.CCMsg_All, eventName, data);
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        /// <summary> 客户端发送消息给其他客户端（不包括自己） </summary>
        public void ClientSendMsgToOtherClient(string eventName, string data)
        {
            var msg = GetClientGameMsg(LAN_CMD.CCMsg_UnSelf, eventName, data);
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        /// <summary> 客户端发送消息给客户端（一个） </summary>
        public void ClientSendMsgToClient(string eventName, string data, string client)
        {
            var msg = GetClientGameMsg(LAN_CMD.CCMsg_One, eventName, data, new List<string> { client });
            LanSocketTcpClient.Instance.SendMsg(msg);
        }

        /// <summary> 客户端发送消息给客户端列表（多个） </summary>
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

        /// <summary> 接收消息 </summary>
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

        public void CloseTcpClient()
        {
            LanSocketTcpClient.Instance.Close();
        }
    }
}