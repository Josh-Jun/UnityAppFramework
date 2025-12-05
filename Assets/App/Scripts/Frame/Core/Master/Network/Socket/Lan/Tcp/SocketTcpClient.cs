/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年12月4 16:26
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using App.Core.Master;
using UnityEngine;

namespace App.Core.Master
{
    public class SocketTcpClient : SocketTcpBase
    {
        //建立连接
        protected override void OnConnected()
        {
            
        }

        //收到数据
        protected override void OnReceiveMsg(byte[] msg)
        {
            var data = SocketTools.DeSerialize<PushMsg>(msg);
            LanSocketTcpServer.Instance.AddMsgQueue(this, data); //服务端=>加入队列
        }

        //断开连接
        protected override void OnDisConnected()
        {
            
        }
    }
}
