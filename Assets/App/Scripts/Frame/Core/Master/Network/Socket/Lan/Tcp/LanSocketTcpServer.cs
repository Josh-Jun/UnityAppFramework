/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年12月4 16:38
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    public class LanSocketTcpServer : SingletonEvent<LanSocketTcpServer>
    {
        private const string lockLanSocketServer = "lockLanSocketServer";

        private SocketTcp<SocketTcpClient> server;
        private readonly Queue<MsgPackage> msgPackageQueue = new(); //消息队列(Server)

        private int TIME_UPDATE_ID = -1;

        public List<SocketTcpClient> GetClients(List<string> ips) => server.clients.FindAll(c => ips.Contains(c.Ip));
        public List<SocketTcpClient> GetAllClient() => server.clients;

        public void StartServer(string ip, int port, Action<bool> callback)
        {
            TIME_UPDATE_ID = TimeUpdateMaster.Instance.StartTimer(Update);
            server = new SocketTcp<SocketTcpClient>();
            server.StartServer(ip, port, success =>
            {
                callback?.Invoke(success);
                if (success) return;
                AutoRestart(ip, port, callback);
            });
        }
        
        public void AutoRestart(string ip, int port, Action<bool> callback)
        {
            TimeTaskMaster.Instance.AddTimeTask(() =>
            {
                Close();
                server = new SocketTcp<SocketTcpClient>();
                server.StartServer(ip, port, success =>
                {
                    TimeTaskMaster.Instance.AddFrameTask(() =>
                    {
                        callback?.Invoke(success);
                        if (success) return;
                        AutoRestart(ip, port, callback);
                    }, 1);
                });
            }, 1);
        }

        private void Update(float time)
        {
            lock (lockLanSocketServer)
            {
                if (msgPackageQueue.Count <= 0) return;
                for (var i = 0; i < msgPackageQueue.Count; i++)
                {
                    HandOutMsg(msgPackageQueue.Dequeue()); //取消息包 进行分发
                }
            }
        }

        /// <summary>消息分发</summary>
        private void HandOutMsg(MsgPackage msgPack)
        {
            switch (msgPack.msg.cmd)
            {
                case (int)LAN_CMD.None:
                case (int)LAN_CMD.SCMsg_All:
                case (int)LAN_CMD.SCMsg_One:
                case (int)LAN_CMD.SCMsg_List:
                case (int)LAN_CMD.SCMsg_UnSelf:
                    break;
                case (int)LAN_CMD.CSMsg:
                    SocketMaster.Instance.ReceiveClientMsg((PushMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_All:
                    SocketMaster.Instance.ForwardClientMsgToAllClient((PushMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_One:
                    SocketMaster.Instance.ForwardClientMsgToClient((PushMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_List:
                    SocketMaster.Instance.ForwardClientMsgToClientList((PushMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_UnSelf:
                    var client = msgPack.session as SocketTcpClient;
                    SocketMaster.Instance.ForwardClientMsgToOtherClient(client, (PushMsg)msgPack.msg);
                    break;
                default:
                    break;
            }
        }

        /// <summary>把消息加入队列</summary>
        public void AddMsgQueue(SocketTcpBase session, SocketMsg msg)
        {
            if (server == null) return;
            lock (lockLanSocketServer)
            {
                msgPackageQueue.Enqueue(new MsgPackage(session, msg));
            }
        }
        
        /// <summary>发送消息(SocketMsg)</summary>
        public void SendMsg(SocketTcpClient client, SocketMsg msg)
        {
            if (client == null) return;
            try
            {
                client.SendMsg(msg);
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }
        
        /// <summary>发送消息(string)</summary>
        public void SendMsg(SocketTcpClient client, string msg)
        {
            if (client == null) return;
            try
            {
                client.SendMsg(msg);
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }
        
        /// <summary>发送消息(byte[])</summary>
        public void SendMsg(SocketTcpClient client, byte[] bytes)
        {
            if (client == null) return;
            try
            {
                client.SendMsg(bytes);
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }

        public void Close()
        {
            server?.Close();
            TimeUpdateMaster.Instance.EndTimer(TIME_UPDATE_ID);
        }
    }
}