using System.Text;

namespace App.Core.Master
{
    /// <summary>功能:网络会话</summary>
    public class SessionTcp : SessionTcpBase
    {
        //建立连接
        protected override void OnConnected()
        {
            //Debug.Log("有客户端连接成功...");
        }

        //收到数据
        protected override void OnReciveMsg(string msg)
        {
            var data = Encoding.UTF8.GetBytes(msg.ToCharArray());
            var gameMsg = SocketTools.DeSerialize<GameMsg>(data);
            LanTcpMaster.Instance.AddMsgQueue(this, gameMsg); //服务端=>加入队列
            LanTcpMaster.Instance.AddMsgQueue(gameMsg); //客户端=>加入队列
        }

        //断开连接
        protected override void OnDisConnected()
        {
            LanTcpMaster.Instance.AddOffLine(this);
        }
    }
}