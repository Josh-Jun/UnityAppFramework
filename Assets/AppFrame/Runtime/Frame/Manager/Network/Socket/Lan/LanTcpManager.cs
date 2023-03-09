using System;
using UnityEngine;
using System.Collections.Generic;
using AppFrame.Manager;
using AppFrame.Tools;

namespace AppFrame.Network.Lan.Tcp
{
    public class LanTcpManager : SingletonEvent<LanTcpManager>
    {
        private static readonly string lockNetTcp = "lockNetTcp"; //加锁

        private SocketTcp<SessionTcp> server; //Server Socket连接
        private Queue<MsgPackage> msgPackageQueue = new Queue<MsgPackage>(); //消息队列(Server)
        private Queue<SessionTcp> offLineQueue = new Queue<SessionTcp>(); //离线队列

        public Dictionary<SessionTcp, string>
            clientKeyValuePairs = new Dictionary<SessionTcp, string>(); //Client Session字典

        private SocketTcp<SessionTcp> client; //Client Socket连接
        private Queue<SocketMsg> msgQueue = new Queue<SocketMsg>(); //消息队列(Client)
        private int TIME_ID = -1;

        /// <summary>服务端网络初始化</summary>
        public void InitServerNet(string ip, int port, Action<bool> cb = null)
        {
            TIME_ID = TimeUpdateManager.Instance.StartTimer(Update);
            server = new SocketTcp<SessionTcp>();

            #region 网络日志

            //日志是否开启、日志的回调函数(内容，级别) 覆盖unity日志系统 查看网络错误
            server.SetLog(true, (string msg, int lv) =>
            {
                if (this == null)
                {
                    return;
                }

                switch (lv)
                {
                    case 0: //普通
                        msg = "Log:" + msg;
                        Debug.Log(msg);
                        break;
                    case 1: //警告
                        msg = "Warn:" + msg;
                        Debug.LogWarning(msg);
                        break;
                    case 2: //错误
                        msg = "Error:" + msg;
                        Debug.LogError(msg);
                        break;
                    case 3: //信息
                        msg = "Info:" + msg;
                        Debug.Log(msg);
                        break;
                }
            });

            #endregion

            server.StartAsServer(ip, port, cb); //开启服务端
        }

        /// <summary>网络服务初始化</summary>
        public void InitClientNet(string ip, int port, Action<bool> cb = null)
        {
            client = new SocketTcp<SessionTcp>();

            #region 网络日志

            //日志是否开启、日志的回调函数(内容，级别) 覆盖unity日志系统 查看网络错误
            client.SetLog(true, (string msg, int lv) =>
            {
                if (this == null)
                {
                    return;
                }

                switch (lv)
                {
                    case 0: //普通
                        msg = "Log:" + msg;
                        Debug.Log(msg);
                        break;
                    case 1: //警告
                        msg = "Warn:" + msg;
                        Debug.LogWarning(msg);
                        break;
                    case 2: //错误
                        msg = "Error:" + msg;
                        Debug.LogError(msg);
                        break;
                    case 3: //信息
                        msg = "Info:" + msg;
                        Debug.Log(msg);
                        break;
                }
            });

            #endregion

            client.StartAsClient_IP(ip, port, cb); //开启客户端
        }

        private void Update(float time)
        {
            #region Server

            if (msgPackageQueue.Count > 0)
            {
                lock (lockNetTcp)
                {
                    for (int i = 0; i < msgPackageQueue.Count; i++)
                    {
                        HandOutMsg(msgPackageQueue.Dequeue()); //取消息包 进行分发
                    }
                }
            }

            if (offLineQueue.Count > 0)
            {
                lock (lockNetTcp)
                {
                    for (int i = 0; i < offLineQueue.Count; i++)
                    {
                        RemoveSession(offLineQueue.Dequeue()); //移除离线
                    }
                }
            }

            #endregion

            #region Client

            if (msgQueue.Count > 0)
            {
                lock (lockNetTcp)
                {
                    for (int i = 0; i < msgQueue.Count; i++)
                    {
                        HandOutMsg(msgQueue.Dequeue()); //取消息包 进行分发
                    }
                }
            }

            #endregion
        }

        /// <summary> 通过学生设备id获取当前学生Session </summary>
        public SessionTcp GetStudentSessionByDeviceId(string deviceId)
        {
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                if (clientKeyValuePairs[session] == deviceId)
                {
                    return session;
                }
            }

            return null;
        }

        #region Server 添加消息，进行分发

        /// <summary>把消息加入队列</summary>
        public void AddMsgQueue(SessionTcp session, SocketMsg msg)
        {
            if (server != null)
            {
                lock (lockNetTcp)
                {
                    msgPackageQueue.Enqueue(new MsgPackage(session, msg));
                }
            }
        }

        /// <summary>消息分发</summary>
        public void HandOutMsg(MsgPackage msgPack)
        {
            switch (msgPack.msg.cmd)
            {
                case (int)LAN_CMD.None:
                    break;
                case (int)LAN_CMD.SCMsg_All:
                    break;
                case (int)LAN_CMD.SCMsg_One:
                    break;
                case (int)LAN_CMD.SCMsg_List:
                    break;
                case (int)LAN_CMD.SCMsg_UnSelf:
                    break;
                case (int)LAN_CMD.CSMsg:
                    NetcomManager.Instance.ReceiveMsg((GameMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_All:
                    NetcomManager.Instance.CCPushMsg_All((GameMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_One:
                    NetcomManager.Instance.CCPushMsg_One((GameMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_List:
                    NetcomManager.Instance.CCPushMsg_List((GameMsg)msgPack.msg);
                    break;
                case (int)LAN_CMD.CCMsg_UnSelf:
                    NetcomManager.Instance.CCPushMsg_UnSelf(msgPack.session, (GameMsg)msgPack.msg);
                    break;
                default:
                    break;
            }
        }

        /// <summary> 添加离线 </summary>
        public void AddOffLine(SessionTcp sessionTcp)
        {
            offLineQueue.Enqueue(sessionTcp);
        }

        /// <summary> 移除所有Session</summary>
        public void RemoveAllStudentSession()
        {
            clientKeyValuePairs.Clear();
        }

        /// <summary> 移除Session 掉线</summary>
        public void RemoveSession(SessionTcp session)
        {
            if (session != null)
            {
                if (clientKeyValuePairs.ContainsKey(session))
                {
                    clientKeyValuePairs.Remove(session);
                }
            }
            else
            {
                Debug.LogError("NetTcpSvc.RemoveSession(SessionTcp session)为空 ");
            }
        }

        /// <summary>消息包 </summary>
        public class MsgPackage
        {
            public SessionTcp session; //同一个回话
            public SocketMsg msg;

            public MsgPackage(SessionTcp session, SocketMsg msg)
            {
                this.session = session;
                this.msg = msg;
            }
        }

        #endregion

        #region Server 发送消息 To Client

        /// <summary>发送消息</summary>
        public void SendMsg(SessionTcp session, string msg)
        {
            if (session != null)
            {
                try
                {
                    byte[] bytes = SocketTools.PackageLengthInfo(msg);
                    session.SendMsg(bytes);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>发送消息</summary>
        public void SendMsg(SessionTcp session, SocketMsg msg)
        {
            if (session != null)
            {
                try
                {
                    byte[] bytes = SocketTools.PackageNetMsg(msg);
                    session.SendMsg(bytes);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>发送消息</summary>
        public void SendMsg(SessionTcp session, byte[] bytes)
        {
            if (session != null)
            {
                try
                {
                    session.SendMsg(SocketTools.PackageLengthInfo(bytes));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>发送消息(部分客户端) deviceIdList:客户端列表</summary>
        public void SendMsgAll(SocketMsg msg)
        {
            byte[] bytes = SocketTools.PackageNetMsg(msg);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                session.SendMsg(bytes);
            }
        }

        /// <summary>发送消息(部分客户端) deviceIdList:客户端列表</summary>
        public void SendMsgAll(byte[] bytes)
        {
            byte[] data = SocketTools.PackageLengthInfo(bytes);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                session.SendMsg(data);
            }
        }

        /// <summary>发送消息(部分客户端) deviceIdList:客户端列表</summary>
        public void SendMsgAll(string msg)
        {
            byte[] data = SocketTools.PackageLengthInfo(msg);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                session.SendMsg(data);
            }
        }

        /// <summary>发送消息(部分客户端) deviceIdList:客户端列表</summary>
        public void SendMsgAll(SocketMsg msg, List<string> deviceIdList)
        {
            byte[] bytes = SocketTools.PackageNetMsg(msg);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                if (deviceIdList.Contains(clientKeyValuePairs[session]))
                {
                    session.SendMsg(bytes);
                }
            }
        }

        /// <summary>发送消息(部分客户端) deviceIdList:客户端列表</summary>
        public void SendMsgAll(byte[] bytes, List<string> deviceIdList)
        {
            byte[] data = SocketTools.PackageLengthInfo(bytes);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                if (deviceIdList.Contains(clientKeyValuePairs[session]))
                {
                    session.SendMsg(data);
                }
            }
        }

        /// <summary>发送消息(部分客户端) deviceIdList:客户端列表</summary>
        public void SendMsgAll(string msg, List<string> deviceIdList)
        {
            byte[] bytes = SocketTools.PackageLengthInfo(msg);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                if (deviceIdList.Contains(clientKeyValuePairs[session]))
                {
                    session.SendMsg(bytes);
                }
            }
        }

        /// <summary>发送消息(所有学生端) sessionTcp：除去某一个学生端</summary>
        public void SendMsgAll(SessionTcp sessionTcp, SocketMsg gameMsg)
        {
            byte[] bytes = SocketTools.PackageNetMsg(gameMsg);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                if (session != sessionTcp)
                {
                    session.SendMsg(bytes);
                }
            }
        }

        /// <summary>发送消息(所有学生端) sessionTcp：除去某一个学生端</summary>
        public void SendMsgAll(SessionTcp sessionTcp, byte[] bytes)
        {
            byte[] data = SocketTools.PackageLengthInfo(bytes);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                if (session != sessionTcp)
                {
                    session.SendMsg(data);
                }
            }
        }

        /// <summary>发送消息(所有学生端) sessionTcp：除去某一个学生端</summary>
        public void SendMsgAll(SessionTcp sessionTcp, string msg)
        {
            byte[] bytes = SocketTools.PackageLengthInfo(msg);
            foreach (SessionTcp session in clientKeyValuePairs.Keys)
            {
                if (session != sessionTcp)
                {
                    session.SendMsg(bytes);
                }
            }
        }

        #endregion

        #region Client 添加消息，进行分发

        /// <summary>把消息加入队列</summary>
        public void AddMsgQueue(SocketMsg msg)
        {
            if (client != null)
            {
                lock (lockNetTcp)
                {
                    msgQueue.Enqueue(msg);
                }
            }
        }

        /// <summary>消息分发</summary>
        public void HandOutMsg(SocketMsg msg)
        {
            NetcomManager.Instance.ReceiveMsg((GameMsg)msg);
        }

        #endregion

        #region Client 发送消息 To Client

        /// <summary>发送消息</summary>
        public void SendMsg(SocketMsg msg)
        {
            if (client != null && client.session != null)
            {
                try
                {
                    client.session.SendMsg(SocketTools.PackageNetMsg(msg));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>发送消息</summary>
        public void SendMsg(string msg)
        {
            if (client != null && client.session != null)
            {
                try
                {
                    client.session.SendMsg(SocketTools.PackageLengthInfo(msg));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        /// <summary>发送消息</summary>
        public void SendMsg(byte[] bytes)
        {
            if (client != null && client.session != null)
            {
                try
                {
                    client.session.SendMsg(SocketTools.PackageLengthInfo(bytes));
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        #endregion

        /// <summary>关闭Tcp</summary>
        public void Close()
        {
            if (client != null && client.session != null)
            {
                client.session.Clear();
                client.Close();
                client = null;
            }

            if (server != null && server.session != null)
            {
                server.session.Clear();
                server.Close();
                server = null;
            }

            TimeUpdateManager.Instance.EndTimer(TIME_ID);
        }
    }
}