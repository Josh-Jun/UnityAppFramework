/// <summary>功能:网络会话</summary>
public class SessionTcp : SessionTcpBase<SocketMsg>
{
    //建立连接
    protected override void OnConnected()
    {
        //Debug.Log("有客户端连接成功...");
    }

    //收到数据
    protected override void OnReciveMsg(SocketMsg msg)
    {
        TcpManager.Instance.AddMsgQueue(this, msg);//服务端=>加入队列
        TcpManager.Instance.AddMsgQueue(msg);//客户端=>加入队列
    }

    //断开连接
    protected override void OnDisConnected()
    {
        TcpManager.Instance.AddOffLine(this);
    }
}