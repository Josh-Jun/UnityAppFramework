using System;
using System.Collections.Generic;
using System.Linq;
using App.Core.Tools;

namespace App.Core.Master
{
    /// <summary>
    /// Socket局域网TCP服务器
    /// </summary>
    public partial class SocketMaster : SingletonMonoEvent<SocketMaster>
    {
        /// <summary> 开启Tcp服务端 </summary>
        public void StartTcpServer(string ip, int port, Action<bool> callback)
        {
            LanSocketTcpServer.Instance.StartServer(ip, port, callback);
        }
        /// <summary> 自动重启服务端 </summary>
        public void AutoRestartTcpServer(string ip, int port, Action<bool> callback)
        {
            LanSocketTcpServer.Instance.AutoRestart(ip, port, callback);
        }
        /// <summary> 给所有客户端发送消息 </summary>
        public void ServerSendMsgToAllClient(string eventName, string data = "")
        {
            var msg = GetServerGameMsg(LAN_CMD.SCMsg_All, eventName, data);
            var list = LanSocketTcpServer.Instance.GetAllClient();
            foreach (var item in list)
            {
                LanSocketTcpServer.Instance.SendMsg(item, msg);
            }
        }

        /// <summary> 给客户端列表发送消息 </summary>
        public void ServerSendMsgToClientList(string eventName, List<string> clients, string data = "")
        {
            clients = clients.Distinct(StringComparer.Ordinal).ToList();
            var msg = GetServerGameMsg(LAN_CMD.SCMsg_All, eventName, data);
            var list = LanSocketTcpServer.Instance.GetClients(clients);
            foreach (var item in list)
            {
                LanSocketTcpServer.Instance.SendMsg(item, msg);
            }
        }

        /// <summary> 给客户端列表发送消息 </summary>
        public void ServerSendMsgToClientList(string eventName, List<SocketTcpClient> clients, string data = "")
        {
            var msg = GetServerGameMsg(LAN_CMD.SCMsg_All, eventName, data);
            foreach (var item in clients)
            {
                LanSocketTcpServer.Instance.SendMsg(item, msg);
            }
        }

        /// <summary> 给单个客户端发送消息 </summary>
        public void ServerSendMsgToClient(string eventName, SocketTcpClient client, string data = "")
        {
            var msg = GetServerGameMsg(LAN_CMD.SCMsg_All, eventName, data);
            LanSocketTcpServer.Instance.SendMsg(client, msg);
        }

        /// <summary> 给单个客户端发送消息 </summary>
        public void ServerSendMsgToClient(string eventName, string client, string data = "")
        {
            var msg = GetServerGameMsg(LAN_CMD.SCMsg_All, eventName, data);
            var list = LanSocketTcpServer.Instance.GetClients(new List<string>{ client });
            foreach (var item in list)
            {
                LanSocketTcpServer.Instance.SendMsg(item, msg);
            }
        }

        /// <summary> 获取消息包 </summary>
        private PushMsg GetServerGameMsg(LAN_CMD cmd, string eventName, string data)
        {
            return new PushMsg
            {
                cmd = (int)cmd,
                eventName = eventName,
                data = data,
            };
        }
        
        /// <summary> 接受消息 </summary>
        public void ReceiveClientMsg(PushMsg msg)
        {
            var eventName = msg.eventName;
            if (string.IsNullOrEmpty(eventName))
            {
                Log.E("消息事件名不可为空！");
                return;
            }
            if (!HasEvent(eventName))
            {
                Log.W($"消息事件名:[{eventName}]未找到");
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
        
        /// <summary> 客户端转发客户端(1人) </summary>
        public void ForwardClientMsgToClient(PushMsg msg)
        {
            var list = LanSocketTcpServer.Instance.GetClients(msg.clients);
            foreach (var client in list)
            {
                LanSocketTcpServer.Instance.SendMsg(client, msg);
            }
        }
        
        /// <summary> 客户端转发客户端(多人) </summary>
        public void ForwardClientMsgToClientList(PushMsg msg)
        {
            var list = LanSocketTcpServer.Instance.GetClients(msg.clients);
            foreach (var client in list)
            {
                LanSocketTcpServer.Instance.SendMsg(client, msg);
            }
        }
        
        /// <summary> 客户端转发客户端(移除自己) </summary>
        public void ForwardClientMsgToOtherClient(SocketTcpClient session, PushMsg msg)
        {
            var list = LanSocketTcpServer.Instance.GetAllClient();
            foreach (var client in list.Where(item => item != session))
            {
                LanSocketTcpServer.Instance.SendMsg(client, msg);
            }
        }
        
        /// <summary> 客户端转发客户端(所有人) </summary>
        public void ForwardClientMsgToAllClient(PushMsg msg)
        {
            var list = LanSocketTcpServer.Instance.GetAllClient();
            foreach (var client in list)
            {
                LanSocketTcpServer.Instance.SendMsg(client, msg);
            }
        }

        public void CloseTcpServer()
        {
            LanSocketTcpServer.Instance.Close();
        }
    }
}