/// <summary>
/// Socket
/// </summary>
public partial class NetcomManager : Singleton<NetcomManager>
{
    #region Tcp
    public void StartTcpServer(string ip, int port)
    {
        TcpManager.Instance.StartServer(ip, port);
        AddTcpEvent();
    }
    public void StartTcpClient(string ip, int port)
    {
        TcpManager.Instance.StartClient(ip, port);
        AddTcpEvent();
    }
    private void AddTcpEvent()
    {
        TcpManager.Instance.HandOutMsg += OnTcpHandOutMsg;
        TcpManager.Instance.OnConnectEvent += OnTcpConnectEvent;
        TcpManager.Instance.OnDisConnectEvent += OnTcpDisConnectEvent;
    }
    private void OnTcpHandOutMsg(Msg msg)
    {

    }
    private void OnTcpConnectEvent()
    {

    }
    private void OnTcpDisConnectEvent()
    {

    }
    #endregion

    #region Udp
    public void StartUdpServer(int port)
    {
        UdpManager.Instance.StartServer(port);
        AddUdpEvent();
    }
    public void StartUdpClient(int port)
    {
        UdpManager.Instance.StartClient(port);
        AddUdpEvent();
    }
    private void AddUdpEvent()
    {
        UdpManager.Instance.HandOutMsg += OnUdpHandOutMsg;
        UdpManager.Instance.OnConnectEvent += OnUdpConnectEvent;
        UdpManager.Instance.OnDisConnectEvent += OnUdpDisConnectEvent;
    }
    private void OnUdpHandOutMsg(string msg)
    {

    }
    private void OnUdpConnectEvent()
    {

    }
    private void OnUdpDisConnectEvent()
    {

    }
    #endregion
}