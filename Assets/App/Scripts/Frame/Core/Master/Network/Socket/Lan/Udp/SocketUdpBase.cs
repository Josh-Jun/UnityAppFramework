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
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace App.Core.Master
{
    public class SocketUdpBase
    {
        private Socket _socket;

        private EndPoint _endPoint;

        private Thread _thread;
        
        private const int bufferSize = 1024*1024;
        
        public void BeginReceiveData(Socket socket, EndPoint endPoint, Thread thread)
        {
            _socket = socket;
            _endPoint = endPoint;
            _thread = thread;
            OnConnected();
        }
        
        public void ReceiveData()
        {
            while (true)
            {
                try
                {
                    var buffer = new byte[bufferSize];
                    _socket.ReceiveFrom(buffer, ref _endPoint);
                    OnReceiveMsg(buffer);
                    Thread.Sleep(100);
                }
                catch
                {
                    return;
                }
            }
        }
        
        public void SendMsg(byte[] msg)
        {
            try
            {
                _socket.SendTo(msg, msg.Length, SocketFlags.None, _endPoint);
            }
            catch (Exception e)
            {
                Debug.LogError($"Udp SndMsg Error:{e.Message}");
            }
        }

        public void SendMsg(string msg)
        {
            try
            {
                var buffer = Encoding.UTF8.GetBytes(msg);
                _socket.SendTo(buffer, buffer.Length, SocketFlags.None, _endPoint);
            }
            catch (Exception e)
            {
                Debug.LogError($"Udp SndMsg Error:{e.Message}");
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
                Debug.LogError($"Udp SendCallBack Error:{e.Message}");
            }
        }
        
        public void Close()
        {
            if (_thread != null)
            {
                _thread.Interrupt();
                _thread.Abort();
            }
            _socket?.Close();
            OnDisConnected();
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
