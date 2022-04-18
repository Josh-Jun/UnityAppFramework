using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class WanTcpManager : SingletonMonoEvent<WanTcpManager>
{
    private Socket socketTcp = null;
    public int backlog = 100;
    private int overtime = 5000;

    private Action<bool> serverCallBack;
    private Action<bool> clientCallBack;
    private Action closeCallBack;

    private Queue<byte[]> msgQueue = new Queue<byte[]>();//消息队列
    private static readonly string lockNetTcp = "lockNetTcp";//加锁

    public delegate void BytesValueDelegate(byte[] value);
    public event BytesValueDelegate HandOutMsg;

    // Start is called before the first frame update
    public void Awake()
    {
        socketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }
    #region Server
    public void StartAsServer(string ip, int port, Action<bool> cb = null)
    {
        try
        {
            serverCallBack = cb;
            socketTcp.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socketTcp.Listen(backlog);
            socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
            Debug.Log("Tcp服务端开启成功！正在等待连接......");
            serverCallBack?.Invoke(true);
        }
        catch (Exception ex)
        {
            Debug.LogError("Tcp服务端开启失败：" + ex.Message);
            serverCallBack?.Invoke(false);
        }
    }

    private void ClientConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = socketTcp.EndAccept(ar);
            StartReceiveMsg(socket, delegate
            {
                Debug.Log("客户端断开连接......");
            });
            Debug.Log("Tcp连接客户端成功！正在接收数据......");
        }
        catch (Exception ex)
        {
            Debug.LogError("Tcp服务器关闭:" + ex.Message);
            serverCallBack?.Invoke(false);
        }
        socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
    }
    #endregion
    #region Client
    public void StartAsClient(string ip, int port, Action<bool> cb = null)
    {
        try
        {
            clientCallBack = cb;
            IAsyncResult asyncResult = socketTcp.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ServerConnectCallBack, socketTcp);
            bool flag = asyncResult.AsyncWaitHandle.WaitOne(overtime, exitContext: true);
            if (!flag)
            {
                Close();
                SocketTools.LogMsg("Tcp客户端连接超时", LogLevel.Error);
                clientCallBack?.Invoke(flag);
            }
        }
        catch (Exception ex)
        {
            SocketTools.LogMsg("Tcp客户端启动失败：" + ex.Message, LogLevel.Error);
            clientCallBack?.Invoke(false);
        }
    }
    private void ServerConnectCallBack(IAsyncResult ar)
    {
        try
        {
            socketTcp.EndConnect(ar);
            StartReceiveMsg(socketTcp, delegate
            {
                SocketTools.LogMsg("Tcp服务器断开连接......", LogLevel.Info);
                clientCallBack?.Invoke(false);
            });
            SocketTools.LogMsg("Tcp连接服务器成功！正在接收数据......", LogLevel.Info);
            clientCallBack?.Invoke(true);
        }
        catch (Exception ex)
        {
            SocketTools.LogMsg("Tcp客户端关闭：" + ex.Message, LogLevel.Error);
            clientCallBack?.Invoke(false);
        }
    }
    #endregion
    private void StartReceiveMsg(Socket socket, Action closeCallBack = null)
    {
        try
        {
            this.closeCallBack = closeCallBack;
            SocketPackage pEPkg = new SocketPackage();
            socket.BeginReceive(pEPkg.headBuffer, 0, pEPkg.headLength, SocketFlags.None, ReceiveHeadData, pEPkg);
        }
        catch (Exception ex)
        {
            Debug.LogError("StartRcvData:" + ex.Message);
        }
    }

    private void ReceiveHeadData(IAsyncResult ar)
    {
        try
        {
            SocketPackage package = (SocketPackage)ar.AsyncState;
            if (socketTcp.Available == 0)
            {
                Clear();
            }
            else
            {
                int num = socketTcp.EndReceive(ar);
                if (num > 0)
                {
                    package.headIndex += num;
                    if (package.headIndex < package.headLength)
                    {
                        socketTcp.BeginReceive(package.headBuffer, package.headIndex, package.headLength - package.headIndex, SocketFlags.None, ReceiveHeadData, package);
                    }
                    else
                    {
                        package.InitBodyBuffer();
                        socketTcp.BeginReceive(package.bodyBuffer, 0, package.bodyLength, SocketFlags.None, ReceiveBodyData, package);
                    }
                }
                else
                {
                    Clear();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("ReceiveHeadError:" + ex.Message);
        }
    }

    private void ReceiveBodyData(IAsyncResult ar)
    {
        try
        {
            SocketPackage package = (SocketPackage)ar.AsyncState;
            int num = socketTcp.EndReceive(ar);
            if (num > 0)
            {
                package.bodyIndex += num;
                if (package.bodyIndex < package.bodyLength)
                {
                    socketTcp.BeginReceive(package.bodyBuffer, package.bodyIndex, package.bodyLength - package.bodyIndex, SocketFlags.None, ReceiveBodyData, package);
                }
                else
                {
                    //string msg = Encoding.UTF8.GetString(package.bodyBuffer);
                    AddMsgQueue(package.bodyBuffer);
                    package.ResetData();
                    socketTcp.BeginReceive(package.headBuffer, 0, package.headLength, SocketFlags.None, ReceiveHeadData, package);
                }
            }
            else
            {
                Clear();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("RcvBodyError:" + ex.Message);
        }
    }
    /// <summary>把消息加入队列</summary>
    private void AddMsgQueue(byte[] bytes)
    {
        lock (lockNetTcp)
        {
            msgQueue.Enqueue(bytes);
        }
    }
    // Update is called once per frame
    public void Update()
    {
        if (msgQueue.Count > 0)
        {
            lock (lockNetTcp)
            {
                for (int i = 0; i < msgQueue.Count; i++)
                {
                    HandOutMsg(msgQueue.Dequeue());//取消息包 进行分发
                }
            }
        }
    }

    public void SendMsg(byte[] _data)
    {
        byte[] data = PackageLengthInfo(_data);
        NetworkStream networkStream = null;
        try
        {
            networkStream = new NetworkStream(socketTcp);
            if (networkStream.CanWrite)
            {
                networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMsgError:" + ex.Message);
        }
    }
    private byte[] PackageLengthInfo(byte[] data)
    {
        int num = data.Length;
        byte[] array = new byte[num + 4];
        byte[] bytes = BitConverter.GetBytes(num);
        bytes.CopyTo(array, 0);
        data.CopyTo(array, 4);
        return array;
    }

    private void SendCallBack(IAsyncResult ar)
    {
        NetworkStream networkStream = (NetworkStream)ar.AsyncState;
        try
        {
            networkStream.EndWrite(ar);
            networkStream.Flush();
            networkStream.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("SendMsgError:" + ex.Message);
        }
    }
    private void Clear()
    {
        closeCallBack?.Invoke();
        socketTcp.Close();
    }
    /// <summary>关闭Tcp</summary>
    public bool Close()
    {
        try
        {
            if (socketTcp != null)
            {
                if (socketTcp.Connected)
                {
                    socketTcp.Shutdown(SocketShutdown.Both);
                }
                socketTcp.Close();
            }
            return true;
        }
        catch (Exception arg)
        {
            Debug.LogError("Tcp关闭Socket错误：" + arg);
            return false;
        }
    }
}
