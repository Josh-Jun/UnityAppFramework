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
    public class LanSocketUdpServer : SingletonEvent<LanSocketUdpServer>
    {
        private const string lockLanSocketUdpServer = "lockLanSocketUdpServer";
        private readonly Queue<string> msgQueue = new Queue<string>(); //消息队列
        private SocketUdp<SocketUdpClient> server;
        private int TIME_UPDATE_ID = -1;
        
        /// <summary> 初始化服务端 </summary>
        public void StartServer(int port)
        {
            TIME_UPDATE_ID = TimeUpdateMaster.Instance.StartTimer(Update);
            server = new SocketUdp<SocketUdpClient>();
            server.StartServer(port, success =>
            {
                Log.I($"StartUdpServer:{success}");
                if (success) return;
                server?.Close();
                StartServer(port);
            });
        }
        /// <summary>发送消息 </summary>
        public void SendMsg(string msg)
        {
            server.session.SendMsg(msg);
        }
        private void Update(float time)
        {
            lock (lockLanSocketUdpServer)
            {
                if (msgQueue.Count <= 0) return;
                HandOutMsg(msgQueue.Dequeue()); //取消息包 进行分发
            }
        }
        /// <summary>把消息加入队列 </summary>
        public void AddMsgQueue(string msg)
        {
            lock (lockLanSocketUdpServer)
            {
                msgQueue.Enqueue(msg);
            }
        }
        /// <summary>消息分发 </summary>
        private void HandOutMsg(string msg)
        {
            SocketMaster.Instance.ReceiveUdpClientMsg(msg);
        }
        /// <summary>退出Udp </summary>
        public void Close()
        {
            if (server is { session: not null })
            {
                server.session.Close();
                server.Close();
                server = null;
            }
            TimeUpdateMaster.Instance.EndTimer(TIME_UPDATE_ID);
        }
    }
}
