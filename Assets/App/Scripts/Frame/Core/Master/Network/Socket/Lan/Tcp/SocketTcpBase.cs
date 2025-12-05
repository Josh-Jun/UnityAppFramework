/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年12月4 15:14
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace App.Core.Master
{
    public class SocketTcpBase
    {
        private Socket _socket;
        private Action _clearCallback;

        public string Ip => ((IPEndPoint)_socket.RemoteEndPoint).Address.ToString();
        public int Port => ((IPEndPoint)_socket.RemoteEndPoint).Port;

        public void BeginReceiveData(Socket socket, Action callback)
        {
            try
            {
                _socket = socket;
                _clearCallback = callback;
                OnConnected();
                var package = new SocketPackage();
                _socket.BeginReceive(package.headBuffer, 0, SocketPackage.headLength, SocketFlags.None, ReceiveHeadData, package);
            }
            catch (Exception e)
            {
                Debug.LogError($"Tcp ReceiveData Error:{e.Message}");
            }
        }
        
        private void ReceiveHeadData(IAsyncResult ar)
        {
            try
            {
                var package = (SocketPackage)ar.AsyncState;
                if (_socket.Available == 0)
                {
                    OnDisConnected();
                    Clear();
                }
                else
                {
                    var num = _socket.EndReceive(ar);
                    if (num > 0)
                    {
                        package.headIndex += num;
                        if (package.headIndex < SocketPackage.headLength)
                        {
                            _socket.BeginReceive(package.headBuffer, package.headIndex,
                                SocketPackage.headLength - package.headIndex, SocketFlags.None, ReceiveHeadData, package);
                        }
                        else
                        {
                            package.InitBodyBuffer();
                            _socket.BeginReceive(package.bodyBuffer, 0, package.bodyLength, SocketFlags.None,
                                ReceiveBodyData, package);
                        }
                    }
                    else
                    {
                        OnDisConnected();
                        Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Tcp ReceiveHead Error:{e.Message}");
            }
        }

        private void ReceiveBodyData(IAsyncResult ar)
        {
            try
            {
                var package = (SocketPackage)ar.AsyncState;
                var num = _socket.EndReceive(ar);
                if (num > 0)
                {
                    package.bodyIndex += num;
                    if (package.bodyIndex < package.bodyLength)
                    {
                        _socket.BeginReceive(package.bodyBuffer, package.bodyIndex,
                            package.bodyLength - package.bodyIndex, SocketFlags.None, ReceiveBodyData, package);
                    }
                    else
                    {
                        OnReceiveMsg(package.bodyBuffer);
                        package.ResetData();
                        _socket.BeginReceive(package.headBuffer, 0, SocketPackage.headLength, SocketFlags.None,
                            ReceiveHeadData, package);
                    }
                }
                else
                {
                    OnDisConnected();
                    Clear();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Tcp ReceiveBody Error:{e.Message}");
            }
        }
        
        public void SendMsg(byte[] msg)
        {
            var data = SocketTools.PackageLengthInfo(msg);
            try
            {
                var networkStream = new NetworkStream(_socket);
                if (networkStream.CanWrite)
                {
                    networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Tcp SndMsg Error:{e.Message}");
            }
        }

        public void SendMsg(string msg)
        {
            var data = SocketTools.PackageLengthInfo(msg);
            try
            {
                var networkStream = new NetworkStream(_socket);
                if (networkStream.CanWrite)
                {
                    networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Tcp SndMsg Error:{e.Message}");
            }
        }
        
        private void SendCallBack(IAsyncResult ar)
        {
            using var networkStream = (NetworkStream)ar.AsyncState;
            try
            {
                networkStream.EndWrite(ar);
                networkStream.Flush();
                networkStream.Close();
            }
            catch (Exception e)
            {
                Debug.LogError($"Tcp SendCallBack Error:{e.Message}");
            }
        }
        
        public void Clear()
        {
            _clearCallback?.Invoke();
            _socket.Close();
        }
        
        protected virtual void OnConnected()
        {
            
        }

        protected virtual void OnReceiveMsg(byte[] msg)
        {
            
        }

        protected virtual void OnDisConnected()
        {
            
        }
    }
}
