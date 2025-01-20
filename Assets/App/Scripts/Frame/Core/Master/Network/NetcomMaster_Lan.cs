using System;
using System.Collections.Generic;
using App.Core.Tools;

/// <summary>
/// Socket局域网
/// </summary>
namespace App.Core.Master
{
    public partial class NetcomMaster : SingletonMonoEvent<NetcomMaster>
    {
        #region Tcp

        #region Server

        /// <summary> 开启Tcp服务端 </summary>
        public void StartLanTcpServer(string ip, Action<bool> cb = null)
        {
            LanTcpMaster.Instance.InitServerNet(ip, Global.SocketPort, cb);
        }

        /// <summary> 给所有客户端发送消息 </summary>
        public void ServerSendToClientMsg_All(string eventName, string data)
        {
            GameMsg gameMsg = GetServerGameMsg(LAN_CMD.SCMsg_All, eventName, data);
            LanTcpMaster.Instance.SendMsgAll(gameMsg);
        }

        /// <summary> 给客户端列表发送消息 </summary>
        public void ServerSendToClientMsg_List(string eventName, List<string> list, string data)
        {
            GameMsg gameMsg = GetServerGameMsg(LAN_CMD.SCMsg_List, eventName, data);
            LanTcpMaster.Instance.SendMsgAll(gameMsg, list);
        }

        /// <summary> 给单个客户端发送消息 </summary>
        public void ServerSendToClientMsg_One(string eventName, SessionTcp session, string data)
        {
            GameMsg gameMsg = GetServerGameMsg(LAN_CMD.SCMsg_One, eventName, data);
            LanTcpMaster.Instance.SendMsg(session, gameMsg);
        }

        /// <summary> 给单个客户端发送消息 </summary>
        public void ServerSendToClientMsg_One(string eventName, string deviceId, string data)
        {
            SessionTcp session = LanTcpMaster.Instance.GetStudentSessionByDeviceId(deviceId);
            GameMsg gameMsg = GetServerGameMsg(LAN_CMD.SCMsg_One, eventName, data);
            LanTcpMaster.Instance.SendMsg(session, gameMsg);
        }

        /// <summary> 给客户端发送消息(移除自己) </summary>
        public void ServerSendToClientMsg_Unself(string eventName, string deviceId, string data)
        {
            SessionTcp session = LanTcpMaster.Instance.GetStudentSessionByDeviceId(deviceId);
            GameMsg gameMsg = GetServerGameMsg(LAN_CMD.SCMsg_UnSelf, eventName, data);
            LanTcpMaster.Instance.SendMsgAll(session, gameMsg);
        }

        /// <summary> 给客户端发送消息(移除自己) </summary>
        public void ServerSendToClientMsg_Unself(string eventName, SessionTcp session, string data)
        {
            GameMsg gameMsg = GetServerGameMsg(LAN_CMD.SCMsg_UnSelf, eventName, data);
            LanTcpMaster.Instance.SendMsgAll(session, gameMsg);
        }

        /// <summary> 获取消息包 </summary>
        private GameMsg GetServerGameMsg(LAN_CMD cmd, string eventName, string data)
        {
            return new GameMsg
            {
                cmd = (int)cmd,
                pushMsg = new PushMsg
                {
                    eventName = eventName,
                    data = data,
                }
            };
        }

        #endregion

        #region Client

        /// <summary> 开启Tcp客户端 </summary>
        public void StartLanTcpClient(string ip, Action<bool> cb = null)
        {
            LanTcpMaster.Instance.InitServerNet(ip, Global.SocketPort, cb);
        }

        /// <summary> 给服务端发送消息 </summary>
        public void ClientSendToServerMsg(string eventName, string data)
        {
            GameMsg gameMsg = GetClientGameMsg(LAN_CMD.CSMsg, null, eventName, data);
            LanTcpMaster.Instance.SendMsg(gameMsg);
        }

        /// <summary> 给客户端发送消息(包含自己) </summary>
        public void ClientSendToClientMsg_All(string eventName, string data)
        {
            GameMsg gameMsg = GetClientGameMsg(LAN_CMD.CCMsg_All, null, eventName, data);
            LanTcpMaster.Instance.SendMsg(gameMsg);
        }

        /// <summary> 给客户端发送消息(客户端列表) </summary>
        public void ClientSendToClientMsg_List(string eventName, List<string> list, string data)
        {
            GameMsg gameMsg = GetClientGameMsg(LAN_CMD.CCMsg_List, list, eventName, data);
            LanTcpMaster.Instance.SendMsg(gameMsg);
        }

        /// <summary> 给客户端发送消息(一个客户端) </summary>
        public void ClientSendToClientMsg_One(string eventName, string deviceId, string data)
        {
            GameMsg gameMsg = GetClientGameMsg(LAN_CMD.CCMsg_One, deviceId, eventName, data);
            LanTcpMaster.Instance.SendMsg(gameMsg);
        }

        /// <summary> 给客户端发送消息(移除自己) </summary>
        public void ClientSendToClientMsg_UnSelf(string eventName, string data)
        {
            GameMsg gameMsg = GetClientGameMsg(LAN_CMD.CCMsg_UnSelf, null, eventName, data);
            LanTcpMaster.Instance.SendMsg(gameMsg);
        }

        /// <summary> 获取消息包 </summary>
        private GameMsg GetClientGameMsg(LAN_CMD cmd, object obj, string eventName, string data)
        {
            return new GameMsg
            {
                cmd = (int)cmd,
                pushMsg = new PushMsg
                {
                    Param = obj,
                    eventName = eventName,
                    data = data,
                }
            };
        }

        #endregion

        /// <summary> 接受消息 </summary>
        public void ReceiveMsg(GameMsg gameMsg)
        {
            string eventName = gameMsg.pushMsg.eventName;
            if (string.IsNullOrEmpty(eventName))
            {
                Log.E("消息事件名不可为空！");
                return;
            }

            string data = gameMsg.pushMsg.data;
            if (data == null)
            {
                SendEventMsg(eventName);
            }
            else
            {
                SendEventMsg(eventName, data);
            }
        }

        #region 消息转发

        /// <summary> 客户端转发客户端(1人) </summary>
        public void CCPushMsg_One(GameMsg gameMsg)
        {
            string deviceId = (string)gameMsg.pushMsg.Param;
            SessionTcp session = LanTcpMaster.Instance.GetStudentSessionByDeviceId(deviceId);
            gameMsg.pushMsg.Param = null;
            LanTcpMaster.Instance.SendMsg(session, gameMsg);
        }

        /// <summary> 客户端转发客户端(多人) </summary>
        public void CCPushMsg_List(GameMsg gameMsg)
        {
            List<string> list = (List<string>)gameMsg.pushMsg.Param;
            gameMsg.pushMsg.Param = null;
            LanTcpMaster.Instance.SendMsgAll(gameMsg, list);
        }

        /// <summary> 客户端转发客户端(移除自己) </summary>
        public void CCPushMsg_UnSelf(SessionTcp session, GameMsg gameMsg)
        {
            LanTcpMaster.Instance.SendMsgAll(session, gameMsg);
        }

        /// <summary> 客户端转发客户端(所有人) </summary>
        public void CCPushMsg_All(GameMsg gameMsg)
        {
            LanTcpMaster.Instance.SendMsgAll(gameMsg);
        }

        #endregion

        #endregion

        #region Udp

        public delegate void StringValueDelegate(string value);

        public event StringValueDelegate ReceiveUdpMsg;

        #region Server

        public void StartLanUdpServer(Action<bool> cb = null)
        {
            LanUdpMaster.Instance.InitServerNet(Global.SocketPort, cb);
        }

        #endregion

        #region Client

        public void StartLanUdpClient(Action<bool> cb = null)
        {
            LanUdpMaster.Instance.InitClientNet(Global.SocketPort, cb);
        }

        #endregion

        public void SendMsg(string msg)
        {
            LanUdpMaster.Instance.SendMsg(msg);
        }

        public void ReceiveMsg(string msg)
        {
            Log.I(msg);
            ReceiveUdpMsg?.Invoke(msg);
        }

        #endregion
    }

    //功能：网络消息序列化
    [Serializable]
    public class GameMsg : SocketMsg
    {
        public PushMsg pushMsg;
    }

    /// <summary> 推送消息 </summary>
    [Serializable]
    public class PushMsg
    {
        public object Param; //外部参数
        public string eventName; //事件名
        public string data; //参数数组
    }

    public enum LAN_CMD
    {
        None = 0, //默认
        SCMsg_All = 1, //服务端=>客户端(all)
        SCMsg_One = 2, //服务端=>客户端(one)
        SCMsg_List = 3, //服务端=>客户端(list)
        SCMsg_UnSelf = 4, //服务端=>客户端(!self)
        CSMsg = 5, //客户端<=>服务端
        CCMsg_All = 6, //客户端=>客户端(all)
        CCMsg_One = 7, //客户端=>客户端(one)
        CCMsg_List = 8, //客户端=>客户端(list)
        CCMsg_UnSelf = 9, //客户端=>客户端(!self)
    }
}