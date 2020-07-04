using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Socket
/// </summary>
public partial class NetcomManager : Singleton<NetcomManager>
{
    #region Tcp

    #region Server
    /// <summary> 开启Tcp服务端 </summary>
    public void StartTcpServer(string ip)
    {
        TcpManager.Instance.InitServerNet(ip, Port);
    }
    /// <summary> 给所有客户端发送消息 </summary>
    public void ServerSendToClientMsg_All(string eventName, params object[] objs)
    {
        GameMsg gameMsg = GetServerGameMsg(CMD.SCMsg_All, eventName, objs);
        TcpManager.Instance.SendMsgAll(gameMsg);
    }
    /// <summary> 给客户端列表发送消息 </summary>
    public void ServerSendToClientMsg_List(string eventName, List<string> list, params object[] objs)
    {
        GameMsg gameMsg = GetServerGameMsg(CMD.SCMsg_List, eventName, objs);
        TcpManager.Instance.SendMsgAll(gameMsg, list);
    }
    /// <summary> 给单个客户端发送消息 </summary>
    public void ServerSendToClientMsg_One(string eventName, SessionTcp session, params object[] objs)
    {
        GameMsg gameMsg = GetServerGameMsg(CMD.SCMsg_One, eventName, objs);
        TcpManager.Instance.SendMsg(session, gameMsg);
    }
    /// <summary> 给单个客户端发送消息 </summary>
    public void ServerSendToClientMsg_One(string eventName, string deviceId, params object[] objs)
    {
        SessionTcp session = TcpManager.Instance.GetStudentSessionByDeviceId(deviceId);
        GameMsg gameMsg = GetServerGameMsg(CMD.SCMsg_One, eventName, objs);
        TcpManager.Instance.SendMsg(session, gameMsg);
    }
    /// <summary> 给客户端发送消息(移除自己) </summary>
    public void ServerSendToClientMsg_Unself(string eventName, string deviceId, params object[] objs)
    {
        SessionTcp session = TcpManager.Instance.GetStudentSessionByDeviceId(deviceId);
        GameMsg gameMsg = GetServerGameMsg(CMD.SCMsg_UnSelf, null, eventName, objs);
        TcpManager.Instance.SendMsgAll(session, gameMsg);
    }
    /// <summary> 给客户端发送消息(移除自己) </summary>
    public void ServerSendToClientMsg_Unself(string eventName, SessionTcp session, params object[] objs)
    {
        GameMsg gameMsg = GetServerGameMsg(CMD.SCMsg_UnSelf, null, eventName, objs);
        TcpManager.Instance.SendMsgAll(session, gameMsg);
    }
    /// <summary> 获取消息包 </summary>
    private GameMsg GetServerGameMsg(CMD cmd, string eventName, params object[] objs)
    {
        return new GameMsg
        {
            cmd = (int)cmd,
            pushMsg = new PushMsg
            {
                eventName = eventName,
                objs = objs,
            }
        };
    }
    #endregion

    #region Client
    /// <summary> 开启Tcp客户端 </summary>
    public void StartTcpClient(string ip)
    {
        TcpManager.Instance.InitServerNet(ip, Port);
    }
    /// <summary> 给服务端发送消息 </summary>
    public void ClientSendToServerMsg(string eventName, params object[] objs)
    {
        GameMsg gameMsg = GetClientGameMsg(CMD.CSMsg, null, eventName, objs);
        TcpManager.Instance.SendMsg(gameMsg);
    }
    /// <summary> 给客户端发送消息(包含自己) </summary>
    public void ClientSendToClientMsg_All(string eventName, params object[] objs)
    {
        GameMsg gameMsg = GetClientGameMsg(CMD.CCMsg_All, null, eventName, objs);
        TcpManager.Instance.SendMsg(gameMsg);
    }
    /// <summary> 给客户端发送消息(客户端列表) </summary>
    public void ClientSendToClientMsg_List(string eventName, List<string> list, params object[] objs)
    {
        GameMsg gameMsg = GetClientGameMsg(CMD.CCMsg_List, list, eventName, objs);
        TcpManager.Instance.SendMsg(gameMsg);
    }
    /// <summary> 给客户端发送消息(一个客户端) </summary>
    public void ClientSendToClientMsg_One(string eventName, string deviceId, params object[] objs)
    {
        GameMsg gameMsg = GetClientGameMsg(CMD.CCMsg_One, deviceId, eventName, objs);
        TcpManager.Instance.SendMsg(gameMsg);
    }
    /// <summary> 给客户端发送消息(移除自己) </summary>
    public void ClientSendToClientMsg_UnSelf(string eventName, params object[] objs)
    {
        GameMsg gameMsg = GetClientGameMsg(CMD.CCMsg_UnSelf, null, eventName, objs);
        TcpManager.Instance.SendMsg(gameMsg);
    }

    /// <summary> 获取消息包 </summary>
    private GameMsg GetClientGameMsg(CMD cmd, object obj, string eventName, params object[] objs)
    {
        return new GameMsg
        {
            cmd = (int)cmd,
            pushMsg = new PushMsg
            {
                Param = obj,
                eventName = eventName,
                objs = objs,
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
            Debug.LogError("消息事件名不可为空！");
            return;
        }
        object[] objs = gameMsg.pushMsg.objs;

    }

    #region 消息转发
    /// <summary> 客户端转发客户端(1人) </summary>
    public void CCPushMsg_One(GameMsg gameMsg)
    {
        string deviceId = (string)gameMsg.pushMsg.Param;
        SessionTcp session = TcpManager.Instance.GetStudentSessionByDeviceId(deviceId);
        gameMsg.pushMsg.Param = null;
        TcpManager.Instance.SendMsg(session, gameMsg);
    }

    /// <summary> 学生端转发学生端(多人) </summary>
    public void CCPushMsg_List(GameMsg gameMsg)
    {
        List<string> list = (List<string>)gameMsg.pushMsg.Param;
        gameMsg.pushMsg.Param = null;
        TcpManager.Instance.SendMsgAll(gameMsg, list);
    }
    /// <summary> 学生端转发学生端(移除自己) </summary>
    public void CCPushMsg_UnSelf(SessionTcp session, GameMsg gameMsg)
    {
        TcpManager.Instance.SendMsgAll(session, gameMsg);
    }

    /// <summary> 学生端转发学生端(所有人) </summary>
    public void CCPushMsg_All(GameMsg gameMsg)
    {
        TcpManager.Instance.SendMsgAll(gameMsg);
    }
    #endregion
    #endregion

    #region Udp
    #region Server
    public void StartUdpServer()
    {
        UdpManager.Instance.InitServerNet(Port);
    }
    #endregion
    #region Client
    public void StartUdpClient()
    {
        UdpManager.Instance.InitClientNet(Port);
    }
    #endregion
    public void SendMsg(string msg)
    {
        UdpManager.Instance.SendMsg(msg);
    }
    public void ReceiveMsg(string msg)
    {
        Debug.Log(msg);

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
    public object Param;//外部参数
    public string eventName;//事件名
    public object[] objs;//参数数组
}
public enum CMD
{
    None = 0,//默认
    SCMsg_All = 1,//服务端=>客户端(all)
    SCMsg_One = 2,//服务端=>客户端(one)
    SCMsg_List = 3,//服务端=>客户端(list)
    SCMsg_UnSelf = 4,//服务端=>客户端(!self)
    CSMsg = 5,//客户端<=>服务端
    CCMsg_All = 6,//客户端=>客户端(all)
    CCMsg_One = 7,//客户端=>客户端(one)
    CCMsg_List = 8,//客户端=>客户端(list)
    CCMsg_UnSelf = 9,//客户端=>客户端(!self)
}