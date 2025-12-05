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
using System.Text;
using UnityEngine;

namespace App.Core.Master
{
    public class SocketUdpServer : SocketUdpBase
    {
        //建立连接
        protected override void OnConnected()
        {
            
        }

        //收到数据
        protected override void OnReceiveMsg(byte[] msg)
        {
            var data = Encoding.UTF8.GetString(msg);
            LanSocketUdpClient.Instance.AddMsgQueue(data); //客户端=>加入队列
        }

        //断开连接
        protected override void OnDisConnected()
        {
            
        }
    }
}
