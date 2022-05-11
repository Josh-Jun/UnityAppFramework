using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class TcpManager : SingletonMonoEvent<TcpManager>
{
    private static Queue<byte[]> mDataQueue = new Queue<byte[]>();
    private Socket socket;
    private IPEndPoint socketEndPoint;
    private static int bufferSize = 2048 * 8;
    private Thread thread = null;
    public UnityAction<byte[]> HandOutMsg;
    public static object objLock = new object();

    private bool isConnect = false;
    public void StartAsClient(string ipAddress, int port, Action error = null)
    {
        socketEndPoint = new IPEndPoint(Dns.GetHostAddresses(ipAddress)[0], port);
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(socketEndPoint);
            if (socket.Connected)
            {
                socket.ReceiveBufferSize = bufferSize;
                socket.SendBufferSize = bufferSize;
            }
            Debug.Log("连接成功");
            isConnect = true;
            if (thread == null)
            {
                thread = new Thread(new ThreadStart(Receive));
                thread.Start();
            }
        }
        catch
        {
            bool bl = Application.internetReachability != NetworkReachability.NotReachable;
            string errInfo = string.Empty;
            if (bl)
            {
                errInfo = "服务器异常";
            }
            else
            {
                errInfo = "自身网络异常";
            }
            Debug.LogError(errInfo);
            error?.Invoke();
        }
    }
    private void Receive()
    {
        try
        {
            while (thread.ThreadState == ThreadState.Running && socket.Connected)
            {
                byte[] buffer = new byte[bufferSize];
                int nLen = socket.Receive(buffer);
                if (nLen == 0)
                {
                    Debug.LogWarning($"nLen == 0");
                    continue;
                }
                if (nLen < 0)
                {
                    Debug.LogError("Receive Error");
                    return;
                }

                List<byte[]> recvList = OnReceive(buffer, nLen);
                if (recvList == null || recvList.Count == 0)
                {
                    Debug.LogWarning($"recvList   {nLen}");
                    continue;
                }
                for (int i = 0; i < recvList.Count; i++)
                {
                    lock (objLock)
                        mDataQueue.Enqueue(recvList[i]);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"{ex.Message}   {ex.GetHashCode()}");
        }
    }
    private byte[] tempcon = null;
    //从服务器端接受返回信息
    public List<byte[]> OnReceive(byte[] recedata, int length)
    {
        try
        {
            List<byte[]> outList = new List<byte[]>();
            if (tempcon != null && tempcon.Length > 0)
            {
                byte[] temp = new byte[recedata.Length];
                Buffer.BlockCopy(recedata, 0, temp, 0, recedata.Length);
                recedata = new byte[temp.Length + tempcon.Length];
                Buffer.BlockCopy(tempcon, 0, recedata, 0, tempcon.Length);
                Buffer.BlockCopy(temp, 0, recedata, tempcon.Length, temp.Length);

                length += tempcon.Length;
                tempcon = null;
            }

            int start = 0;
            byte[] data = new byte[length];
            Buffer.BlockCopy(recedata, 0, data, 0, length);
            while (length - start >= 6)
            {
                int size = GetCmdDataLength(data) + 6;
                if (size <= 6)
                    break;

                if (size > length - start)
                {
                    int addlen = length - start;
                    tempcon = new byte[addlen];
                    Buffer.BlockCopy(recedata, start, tempcon, 0, addlen);
                    break;
                }
                else
                {
                    byte[] temp = new byte[size];
                    Buffer.BlockCopy(recedata, start, temp, 0, size);
                    lock (objLock)
                    {
                        outList.Add(temp);
                    }
                    start += size;
                    data = new byte[length - start];
                    Buffer.BlockCopy(recedata, start, data, 0, length - start);
                }
            }

            if (length - start < 6)
            {
                tempcon = new byte[length - start];
                Buffer.BlockCopy(recedata, start, tempcon, 0, length - start);
            }
            return outList;
        }
        catch (SocketException e)
        {
            Debug.Log(e.Message + " errorcode =" + e.ErrorCode);
            return null;
        }
    }
    private int GetCmdDataLength(byte[] data)
    {
        ByteBuffer buf = new ByteBuffer(data);
        buf.ReadShort();
        int len = buf.ReadInt();
        return len;
    }
    public void SendMsg(byte[] bytes, Action error = null)
    {
        if (!isConnect)
            return;
        try
        {
            socket.Send(bytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"消息发送失败: {ex.Message}");
            error?.Invoke();
        }
    }
    private void Update()
    {
        if (isConnect)
        {
            if (mDataQueue.Count > 0)
            {
                byte[] message = mDataQueue.Dequeue();
                HandOutMsg?.Invoke(message);
            }
        }
    }
    public void Quit()
    {
        Destroy(gameObject);
    }
    public void OnDestory()
    {
        isConnect = false;
        Debug.Log("~TcpManager destroyed");
        try
        {
            if (socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket = null;
            }
        }
        catch { }
        try
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }
        catch
        {
            Debug.Log("Thread异常关闭");
        }
    }
}
