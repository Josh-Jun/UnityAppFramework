using System;
using System.Collections.Generic;
using App.Core.Tools;

namespace App.Core.Master
{
    public class LanUdpMaster : SingletonEvent<LanUdpMaster>
    {
        private SocketUdp<SessionUdp> server;
        private SocketUdp<SessionUdp> client;
        private int TIME_ID = -1;

        /// <summary> 初始化服务端 </summary>
        public void InitServerNet(int port, Action<bool> cb = null)
        {
            TIME_ID = TimeUpdateMaster.Instance.StartTimer(Update);
            server = new SocketUdp<SessionUdp>();
            server.StartAsServer(port, cb);
        }

        /// <summary> 初始化客户端 </summary>
        public void InitClientNet(int port, Action<bool> cb = null)
        {
            client = new SocketUdp<SessionUdp>();
            client.StartAsClient(port, cb);
        }

        #region 发送消息

        /// <summary>发送消息 </summary>
        public void SendMsg(string msg)
        {
            server?.session?.SendMsg(msg);
            client?.session?.SendMsg(msg);
        }

        #endregion

        private readonly Queue<string> msgQueue = new Queue<string>(); //消息队列
        private static readonly string lockNetUdp = "lockNetUdp"; //加锁

        #region 客户端接收消息

        private void Update(float time)
        {
            lock (lockNetUdp)
            {
                if (msgQueue.Count > 0)
                {
                    lock (lockNetUdp)
                    {
                        HandOutMsg(msgQueue.Dequeue()); //取消息包 进行分发
                    }
                }
            }
        }

        /// <summary>把消息加入队列 </summary>
        public void AddMsgQueue(string msg)
        {
            lock (lockNetUdp)
            {
                msgQueue.Enqueue(msg);
            }
        }

        /// <summary>消息分发 </summary>
        private void HandOutMsg(string msg)
        {
            NetcomMaster.Instance.ReceiveMsg(msg);
        }

        #endregion

        #region 关闭连接

        /// <summary>退出Udp </summary>
        public void Close()
        {
            if (server != null && server.session != null)
            {
                server.session.SocketQuit();
                server.Close();
                client = null;
            }

            if (client != null && client.session != null)
            {
                client.session.SocketQuit();
                client.Close();
                server = null;
            }

            TimeUpdateMaster.Instance.EndTimer(TIME_ID);
        }

        #endregion
    }
}
