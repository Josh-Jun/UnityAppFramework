﻿/// <summary>功能:网络会话</summary>
namespace App.Core.Master
{
    public class SessionUdp : SessionUdpBase
    {
        //建立连接
        protected override void OnConnected()
        {

        }

        //收到数据
        protected override void OnReciveMsg(string msg)
        {
            UdpMaster.Instance.AddMsgQueue(msg); //加入队列
        }

        //断开连接
        protected override void OnDisConnected()
        {

        }
    }
}
