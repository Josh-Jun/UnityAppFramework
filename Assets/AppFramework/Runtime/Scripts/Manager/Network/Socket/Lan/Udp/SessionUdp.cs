/// <summary>功能:网络会话</summary>
namespace AppFramework.Network.Lan.Udp
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
            LanUdpManager.Instance.AddMsgQueue(msg); //加入队列
        }

        //断开连接
        protected override void OnDisConnected()
        {

        }
    }
}
