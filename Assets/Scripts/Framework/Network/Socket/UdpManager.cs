using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpManager : SingletonMono<UdpManager>
{
    private Socket socket;
    private IPEndPoint ipEndPoint;
    private EndPoint endPoint;
    private Thread thread;

    private Queue<string> msgQueue = new Queue<string>();//消息队列
    private static readonly string lockNetUdp = "lockNetUdp";//加锁
    public Action<string> HandOutMsg;
    public Action OnConnectEvent;
    public Action OnDisConnectEvent;
    private void Awake()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    private void Update()
    {
        if (msgQueue.Count > 0)
        {
            lock (lockNetUdp)
            {
                HandOutMsg?.Invoke(msgQueue.Dequeue());//取消息包 进行分发
            }
        }
    }

    private void OnDestroy()
    {
        Quit();
    }

    public void StartServer(int port)
    {
        try
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            ipEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            endPoint = ipEndPoint;
            thread = new Thread(StartServerReceive);
            thread.Start();
            Debug.Log("Udp服务端开启成功!");
        }
        catch (Exception ex)
        {
            Debug.Log("Udp服务端开启失败!" + ex.Message);
        }
    }

    private void StartServerReceive()
    {
        OnConnected();
        SendMsg();
        ReceiveData();
    }

    public void StartClient(int port)
    {
        try
        {
            ipEndPoint = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ipEndPoint);
            endPoint = ipEndPoint;
            thread = new Thread(StartClientReceive);
            thread.Start();
            Debug.Log("Udp客户端开启成功!");
        }
        catch (Exception ex)
        {
            Debug.Log("Udp客户端开启失败!" + ex.Message);
        }
    }

    private void StartClientReceive()
    {
        OnConnected();
        ReceiveData();
    }

    public void ReceiveData()
    {
        while (true)
        {
            try
            {
                byte[] array = new byte[1024];
                int count = socket.ReceiveFrom(array, ref endPoint);
                string @string = Encoding.UTF8.GetString(array, 0, count);
                OnReciveMsg(@string);
                Thread.Sleep(100);
            }
            catch
            {
                return;
            }
        }
    }

    public void SendMsg(string msg = "")
    {
        try
        {
            byte[] array = new byte[1024];
            array = Encoding.UTF8.GetBytes(msg);
            socket.SendTo(array, array.Length, SocketFlags.None, endPoint);
        }
        catch (Exception ex)
        {
            Debug.Log("SndMsgError:" + ex.Message);
        }
    }

    public void SendMsg(byte[] data)
    {
        try
        {
            socket.SendTo(data, data.Length, SocketFlags.None, endPoint);
        }
        catch (Exception ex)
        {
            Debug.Log("SndMsgError:" + ex.Message);
        }
    }

    public void Quit()
    {
        if (thread != null)
        {
            thread.Interrupt();
            thread.Abort();
        }
        if (socket != null)
        {
            socket?.Close();
        }
        OnDisConnected();
    }

    private void OnConnected()
    {
        OnConnectEvent?.Invoke();
    }

    private void OnReciveMsg(string msg)
    {
        lock (lockNetUdp)
        {
            msgQueue.Enqueue(msg);
        }
    }

    private void OnDisConnected()
    {
        OnDisConnectEvent?.Invoke();
    }
}
