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

        public readonly List<T> clients = new();

        public void StartServer(string ip, int port)
        {
            try
            {
                _socket.SendTimeout = timeout;
                _socket.ReceiveTimeout = timeout;
                _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                _socket.Listen(backlog);
                _socket.BeginAccept(ClientConnectCallBack, _socket);
            }
            catch (Exception e)
            {
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
                Console.WriteLine(e);
                throw;
            }

            _socket.BeginAccept(ClientConnectCallBack, _socket);
        }

        public void ConnectServer(string ip, int port)
        {
            try
            {
                _socket.SendTimeout = timeout;
                _socket.ReceiveTimeout = timeout;
                var ar = _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ServerConnectCallBack, null);
                var flag = ar.AsyncWaitHandle.WaitOne(timeout, true);
                if (!flag)
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Start TcpSocketClient Error:{e.Message}");
            }
        }

        private void ServerConnectCallBack(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                server = new T();
                server.BeginReceiveData(_socket, () =>
                {
                    
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Start TcpSocketClient Error:{e.Message}");
            }
        }

        public void Close()
        {
            try
            {
                if (_socket == null) return;
                if (_socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }

                _socket.Close();
            }
            catch (Exception e)
            {
                Debug.LogError($"Close TcpSocketClient Error:{e.Message}");
            }
        }
    }
}