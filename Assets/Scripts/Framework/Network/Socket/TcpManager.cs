using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TcpManager : SingletonMono<TcpManager>
{
    private Socket socket;
    private Action close;
    private readonly List<Socket> linkSockets = new List<Socket>();

    private static readonly string lockNetTcp = "lockNetTcp";//网络锁
    private Queue<Msg> msgQueue = new Queue<Msg>();//消息队列
    public Action<Msg> HandOutMsg;
    public Action OnConnectEvent;
    public Action OnDisConnectEvent;
    private void Awake()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    private void Update()
    {
        if (msgQueue.Count > 0)
        {
            lock (lockNetTcp)
            {
                for (int i = 0; i < msgQueue.Count; i++)
                {
                    HandOutMsg?.Invoke(msgQueue.Dequeue());//取消息包 进行分发
                }
            }
        }
    }

    private void OnDestroy()
    {

    }

    public void StartServer(string ip, int port)
    {
        try
        {
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));//绑定端口号
            socket.Listen(64);//设置监听，最大同时连接64台
            socket.BeginAccept(OnClientConnect, socket);
        }
        catch (Exception ex)
        {
            Debug.Log("Tcp服务端开启失败：" + ex.Message);
        }
    }

    private void OnClientConnect(IAsyncResult ar)
    {
        try
        {
            Socket client = socket.EndAccept(ar);
            StartReceiveData(client, delegate
            {
                //有客户端连接
                if (linkSockets.Contains(client))
                    linkSockets.Remove(client);
                Debug.Log("客户端断开连接......");
            });
            linkSockets.Add(client);
        }
        catch (Exception ex)
        {
            Debug.Log("Tcp服务端开启失败：" + ex.Message);
        }
        socket.BeginAccept(OnClientConnect, socket);
    }

    public void StartClient(string ip, int port)
    {
        try
        {
            socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), OnServerConnect, socket);
            Debug.Log("Tcp客户端启动成功！正在连接服务器......");
        }
        catch (Exception ex)
        {
            Debug.Log("Tcp客户端启动失败：" + ex.Message);
        }
    }

    private void OnServerConnect(IAsyncResult ar)
    {
        try
        {
            socket.EndConnect(ar);
            StartReceiveData(socket, delegate
            {
                Debug.Log("Tcp服务器断开连接......");
            });
            Debug.Log("Tcp连接服务器成功！正在接收数据......");
        }
        catch (Exception ex)
        {
            Debug.Log("Tcp客户端关闭：" + ex.Message);
        }
    }

    private void StartReceiveData(Socket socket, Action close)
    {
        this.close = close;
        OnConnected();
        MsgPackage msgPackage = new MsgPackage();
        socket.BeginReceive(msgPackage.headBuffer, 0, msgPackage.headLength, SocketFlags.None, ReceiveHeadData, msgPackage);
    }

    private void ReceiveHeadData(IAsyncResult ar)
    {
        try
        {
            MsgPackage msgPackage = (MsgPackage)ar.AsyncState;
            int num = socket.EndReceive(ar);
            if (num > 0)
            {
                msgPackage.headIndex += num;
                if (msgPackage.headIndex < msgPackage.headLength)
                {
                    socket.BeginReceive(msgPackage.headBuffer, msgPackage.headIndex, msgPackage.headLength - msgPackage.headIndex, SocketFlags.None, ReceiveHeadData, msgPackage);
                }
                else
                {
                    msgPackage.InitBodyBuff();
                    socket.BeginReceive(msgPackage.bodyBuffer, 0, msgPackage.bodyLength, SocketFlags.None, ReceiveBodyData, msgPackage);
                }
            }
            else
            {
                OnDisConnected();
                Close();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("ReceiveHeadDataError:" + ex.Message);
        }
    }

    private void ReceiveBodyData(IAsyncResult ar)
    {
        try
        {
            MsgPackage msgPackage = (MsgPackage)ar.AsyncState;
            int num = socket.EndReceive(ar);
            if (num > 0)
            {
                msgPackage.bodyIndex += num;
                if (msgPackage.bodyIndex < msgPackage.bodyLength)
                {
                    socket.BeginReceive(msgPackage.bodyBuffer, msgPackage.bodyIndex, msgPackage.bodyLength - msgPackage.bodyIndex, SocketFlags.None, ReceiveBodyData, msgPackage);
                }
                else
                {
                    Msg msg = DeSerialize<Msg>(msgPackage.bodyBuffer);
                    OnReciveMsg(msg);
                    msgPackage.ResetData();
                    socket.BeginReceive(msgPackage.headBuffer, 0, msgPackage.headLength, SocketFlags.None, ReceiveHeadData, msgPackage);
                }
            }
            else
            {
                OnDisConnected();
                Close();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("ReceiveBodyDataError:" + ex.Message);
        }
    }

    public void SendMsg(Msg msg)
    {
        byte[] data = PackLengthInfo(Serialize(msg));
        SendMsg(data);
    }

    public void SendMsg(byte[] data)
    {
        NetworkStream networkStream = null;
        try
        {
            networkStream = new NetworkStream(socket);
            if (networkStream.CanWrite)
            {
                networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("SendMsgError:" + ex.Message);
        }
    }

    public void SendAllMsg(Msg msg)
    {
        byte[] data = PackLengthInfo(Serialize(msg));
        SendAllMsg(data);
    }

    public void SendAllMsg(byte[] data)
    {
        NetworkStream networkStream = null;
        try
        {
            for (int i = 0; i < linkSockets.Count; i++)
            {
                networkStream = new NetworkStream(linkSockets[i]);
                if (networkStream.CanWrite)
                {
                    networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("SendMsgError:" + ex.Message);
        }
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
            Debug.Log("SendMsgError:" + ex.Message);
        }
    }

    private void OnConnected()
    {
        OnConnectEvent?.Invoke();
    }

    private void OnReciveMsg(Msg msg)
    {
        lock (lockNetTcp)
        {
            msgQueue.Enqueue(msg);
        }
    }

    private void OnDisConnected()
    {
        OnDisConnectEvent?.Invoke();
    }

    public void Close()
    {
        close?.Invoke();
        socket?.Close();
    }

    private byte[] PackLengthInfo(byte[] data)
    {
        int num = data.Length;
        byte[] array = new byte[num + 4];
        byte[] bytes = BitConverter.GetBytes(num);
        bytes.CopyTo(array, 0);
        data.CopyTo(array, 4);
        return array;
    }

    private byte[] Serialize<T>(T pkg) where T : Msg
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, pkg);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            return memoryStream.ToArray();
        }
    }

    private T DeSerialize<T>(byte[] bs) where T : Msg
    {
        using (MemoryStream serializationStream = new MemoryStream(bs))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            return (T)binaryFormatter.Deserialize(serializationStream);
        }
    }
}

/// <summary>
/// 
/// </summary>
/// 
[Serializable]
public class Msg
{

}
/// <summary>
/// 消息包 
/// </summary>
internal class MsgPackage
{
    public int headLength = 4;

    public byte[] headBuffer = null;

    public int headIndex = 0;

    public int bodyLength = 0;

    public byte[] bodyBuffer = null;

    public int bodyIndex = 0;

    public MsgPackage()
    {
        headBuffer = new byte[headLength];
    }

    public void InitBodyBuff()
    {
        bodyLength = BitConverter.ToInt32(headBuffer, 0);
        bodyBuffer = new byte[bodyLength];
    }

    public void ResetData()
    {
        headIndex = 0;
        bodyLength = 0;
        bodyBuffer = null;
        bodyIndex = 0;
    }
}