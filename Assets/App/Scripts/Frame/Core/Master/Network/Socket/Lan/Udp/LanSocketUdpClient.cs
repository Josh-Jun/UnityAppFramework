/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年12月5 9:30
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    public class LanSocketUdpClient : SingletonEvent<LanSocketUdpClient>
    {
        private const string lockLanSocketUdpClient = "lockLanSocketUdpClient";
        private readonly Queue<string> msgQueue = new Queue<string>(); //消息队列
        private SocketUdp<SocketUdpServer> client;
        private int TIME_UPDATE_ID = -1;

        /// <summary> 初始化客户端 </summary>
        public void ConnectServer(int port)
        {
            
            TIME_UPDATE_ID = TimeUpdateMaster.Instance.StartTimer(Update);
            client = new SocketUdp<SocketUdpServer>();
            client.ConnectServer(port, success =>
            {
                if (success) return;
                client?.Close();
                ConnectServer(port);
            });
        }

        /// <summary>发送消息 </summary>
        public void SendMsg(string msg)
        {
            client.session.SendMsg(msg);
        }
        private void Update(float time)
        {
            lock (lockLanSocketUdpClient)
            {
                if (msgQueue.Count <= 0) return;
                HandOutMsg(msgQueue.Dequeue()); //取消息包 进行分发
            }
        }
        /// <summary>把消息加入队列 </summary>
        public void AddMsgQueue(string msg)
        {
            lock (lockLanSocketUdpClient)
            {
                msgQueue.Enqueue(msg);
            }
        }
        /// <summary>消息分发 </summary>
        private void HandOutMsg(string msg)
        {
            
        }
        /// <summary>退出Udp </summary>
        public void Close()
        {
            if (client != null && client.session != null)
            {
                client.session.Close();
                client.Close();
                client = null;
            }
            TimeUpdateMaster.Instance.EndTimer(TIME_UPDATE_ID);
        }
    }
}
