using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace App.Core.Master
{
    public class SocketUdp<T> where T : SocketUdpBase, new()
    {
        private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IPEndPoint iep;

        private EndPoint ep;

        private Thread thread;

        public T session;
        
        public void StartServer(int port, Action<bool> callback)
        {
            try
            {
                session = new T();
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                iep = new IPEndPoint(IPAddress.Broadcast, port);
                ep = iep;
                thread = new Thread(StartServerReceive);
                thread.Start();
                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                callback?.Invoke(false);
                Debug.LogError($"Start TcpSocketServer Error:{e.Message}");
            }
        }

        private void StartServerReceive()
        {
            session.BeginReceiveData(_socket, ep, thread);
            session.SendMsg("");
            session.ReceiveData();
        }

        public void ConnectServer(int port, Action<bool> callback)
        {
            try
            {
                session = new T();
                iep = new IPEndPoint(IPAddress.Any, port);
                _socket.Bind(iep);
                ep = iep;
                thread = new Thread(StartClientReceive);
                thread.Start();
                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                callback?.Invoke(false);
                Debug.LogError($"Start TcpSocketClient Error:{e.Message}");
            }
        }

        private void StartClientReceive()
        {
            session.BeginReceiveData(_socket, ep, thread);
            session.ReceiveData();
        }

        public void Close()
        {
            if (thread != null)
            {
                thread.Interrupt();
                thread.Abort();
            }
            _socket?.Close();
        }
    }
}