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
    public class LanSocketTcpClient : SingletonEvent<LanSocketTcpClient>
    {
        private const string lockLanSocketClient = "lockLanSocketClient";

        private SocketTcp<SocketTcpServer> client;
        private readonly Queue<SocketMsg> msgPackageQueue = new(); //消息队列(Server)

        private int TIME_UPDATE_ID = -1;
        
        public void ConnectServer(string ip, int port)
        {
            TIME_UPDATE_ID = TimeUpdateMaster.Instance.StartTimer(Update);
            client = new SocketTcp<SocketTcpServer>();
            client.ConnectServer(ip, port);
        }

        private void Update(float time)
        {
            lock (lockLanSocketClient)
            {
                if (msgPackageQueue.Count <= 0) return;
                for (var i = 0; i < msgPackageQueue.Count; i++)
                {
                    HandOutMsg(msgPackageQueue.Dequeue()); //取消息包 进行分发
                }
            }
        }

        /// <summary>消息分发</summary>
        private void HandOutMsg(SocketMsg msgPack)
        {
            SocketMaster.Instance.ReceiveServerMsg((PushMsg)msgPack);
        }

        /// <summary>把消息加入队列</summary>
        public void AddMsgQueue(SocketMsg msg)
        {
            if (client == null) return;
            lock (lockLanSocketClient)
            {
                msgPackageQueue.Enqueue(msg);
            }
        }
        
        /// <summary>发送消息(SocketMsg)</summary>
        public void SendMsg(SocketMsg msg)
        {
            if (client == null) return;
            try
            {
                var bytes = SocketTools.PackageNetMsg(msg);
                client.server.SendMsg(bytes);
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }
        
        /// <summary>发送消息(string)</summary>
        public void SendMsg(string msg)
        {
            if (client == null) return;
            try
            {
                var bytes = SocketTools.PackageLengthInfo(msg);
                client.server.SendMsg(bytes);
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }
        
        /// <summary>发送消息(byte[])</summary>
        public void SendMsg(byte[] bytes)
        {
            if (client == null) return;
            try
            {
                client.server.SendMsg(SocketTools.PackageLengthInfo(bytes));
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }

        public void Close()
        {
            if (client is { server: not null })
            {
                client.clients.Clear();
                client.server.Clear();
                client.Close();
                client = null;
            }

            TimeUpdateMaster.Instance.EndTimer(TIME_UPDATE_ID);
        }
    }
}