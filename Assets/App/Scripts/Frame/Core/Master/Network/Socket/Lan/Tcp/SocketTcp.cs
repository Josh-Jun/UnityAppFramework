using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace App.Core.Master
{
    public class SocketTcp<T> where T : SocketTcpBase, new()
    {
        private readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int timeout = 5000;
        private const int backlog = 100;

        public T server;
        private Action<bool> serverConnectCallback;
        public readonly List<T> clients = new();
        private Action<bool> clientConnectCallback;

        public void StartServer(string ip, int port, Action<bool> callback)
        {
            try
            {
                serverConnectCallback = callback;
                _socket.SendTimeout = timeout;
                _socket.ReceiveTimeout = timeout;
                _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                _socket.Listen(backlog);
                _socket.BeginAccept(ClientConnectCallBack, _socket);
                serverConnectCallback?.Invoke(true);
            }
            catch (Exception e)
            {
                serverConnectCallback?.Invoke(false);
                Debug.LogError($"Start TcpSocketServer Error:{e.Message}");
            }
        }

        private void ClientConnectCallBack(IAsyncResult ar)
        {
            try
            {
                var socket = _socket.EndAccept(ar);
                var client = new T();
                client.BeginReceiveData(socket, () =>
                {
                    if (clients.Contains(client))
                    {
                        clients.Remove(client);
                    }
                });
                clients.Add(client);
            }
            catch (Exception e)
            {
                serverConnectCallback?.Invoke(false);
                Debug.LogError($"Connect TcpSocketServer Error:{e.Message}");
                throw;
            }

            _socket.BeginAccept(ClientConnectCallBack, _socket);
        }

        public void ConnectServer(string ip, int port, Action<bool> callback)
        {
            try
            {
                clientConnectCallback = callback;
                _socket.SendTimeout = timeout;
                _socket.ReceiveTimeout = timeout;
                var ar = _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ServerConnectCallBack,
                    _socket);
                var flag = ar.AsyncWaitHandle.WaitOne(timeout, true);
                if (flag) return;
                clientConnectCallback?.Invoke(false);
                Close();
            }
            catch (Exception e)
            {
                clientConnectCallback?.Invoke(false);
                Debug.LogError($"Start TcpSocketClient Error:{e.Message}");
            }
        }

        private void ServerConnectCallBack(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                server = new T();
                server.BeginReceiveData(_socket, () => { clientConnectCallback?.Invoke(false); });
                clientConnectCallback?.Invoke(true);
            }
            catch (Exception e)
            {
                clientConnectCallback?.Invoke(false);
                Debug.LogError($"Start TcpSocketClient Error:{e.Message}");
            }
        }

        public void Close()
        {
            try
            {
                serverConnectCallback = null;
                clientConnectCallback = null;
                if (_socket == null) return;
                if (!_socket.Connected) return;
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception e)
            {
                Debug.LogError($"Close TcpSocketClient Error:{e.Message}");
            }
        }
    }
}