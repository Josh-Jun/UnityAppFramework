/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年6月17 11:48
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using App.Core.Tools;
using Google.Protobuf;
using UnityEngine;

namespace App.Core.Master
{
    [Serializable]
    public class NetMsgData
    {
        public int msgId;
        public byte[] data;
    }
    public class NetSocketClient : SingletonEvent<NetSocketClient>
    {
        private TcpClient _client;
        private NetworkStream _network;
        private readonly MemoryStream _memory;
        private readonly BinaryReader _reader;
        private const int timeout = 5000;
        private const int MaxRead = 8192;
        private readonly byte[] _byteBuffer = new byte[MaxRead];

        public Action<int, byte[]> OnReceiveMessage { get; set; }

        public Action OnDisConnected { set; get; }

        public NetSocketClient()
        {
            _memory = new MemoryStream();
            _reader = new BinaryReader(_memory);
        }

        private int _timeId = -1;
        private const string _lock = "lockNetSocket";
        private readonly Queue<NetMsgData> _messageQueue = new Queue<NetMsgData>();

        public void Connect(string host, int port, Action callback = null)
        {
            TimeUpdateMaster.Instance.EndTimer(_timeId);
            _timeId = TimeUpdateMaster.Instance.StartTimer(Update);
            
            _client = null;
            _client = new TcpClient();
            _client.SendTimeout = timeout;
            _client.ReceiveTimeout = timeout;
            _client.NoDelay = true;
            try
            {
                _client.BeginConnect(host, port, (ar) => {
                    _network = _client.GetStream();
                    _network.BeginRead(_byteBuffer, 0, MaxRead, OnRead, null);
                    callback?.Invoke();
                }, null);
            }
            catch (Exception e)
            {
                Log.E($"Connect--->>> {e.Message}");
                this.Close();
                Debug.LogError(e.Message);
            }
        }

        private void Update(float time)
        {
            if (_messageQueue.Count <= 0) return;
            lock (_lock)
            {
                var data = _messageQueue.Dequeue();
                OnReceiveMessage(data.msgId, data.data);
            }
        }

        private void OnRead(IAsyncResult result)
        {
            try
            {
                var bytesRead = 0;
                lock (_network)
                {
                    // 读取字节流到缓冲区
                    bytesRead = _network.EndRead(result);
                }
                if (bytesRead < 1)
                {
                    // 包尺寸有问题，断线处理
                    Close();
                    OnDisConnected?.Invoke();
                    return;
                }
                OnReceive(_byteBuffer, bytesRead);   //分析数据包内容，抛给逻辑层
                lock (_network)
                {
                    // 分析完，再次监听服务器发过来的新消息
                    Array.Clear(_byteBuffer, 0, _byteBuffer.Length);   //清空数组
                    _network.BeginRead(_byteBuffer, 0, MaxRead, OnRead, null);
                }
            }
            catch (Exception e)
            {
                Log.E($"OnRead--->>> {e.Message}");
                Close();
                OnDisConnected?.Invoke();
            }
        }

        private void OnReceive(byte[] bytes, int length)
        {
            _memory.Seek(0, SeekOrigin.End);
            _memory.Write(bytes, 0, length);
            //Reset to beginning
            _memory.Seek(0, SeekOrigin.Begin);
            while (_memory.Length - _memory.Position > 2)
            {
                var messageLen = _reader.ReadUInt16();
                if (_memory.Length - _memory.Position >= messageLen)
                {
                    var ms = new MemoryStream();
                    var writer = new BinaryWriter(ms);
                    writer.Write(_reader.ReadBytes(messageLen));
                    ms.Seek(0, SeekOrigin.Begin);
                    OnReceivedMessage(ms);
                }
                else
                {
                    _memory.Position -= 2;
                    break;
                }
            }
            var leftover = _reader.ReadBytes((int)(_memory.Length - _memory.Position));
            _memory.SetLength(0);
            _memory.Write(leftover, 0, leftover.Length);
        }

        private void OnReceivedMessage(MemoryStream ms)
        {
            var reader = new BinaryReader(ms);
            var message = reader.ReadBytes((int)(ms.Length - ms.Position));
            var buffer = new ByteBuffer(message);
            int msgId = buffer.ReadUShort();
            var length = message.Length - 2;
            var data = buffer.ReadBytes(length);
            _messageQueue.Enqueue(new NetMsgData { msgId = msgId, data = data });
        }

        private void WriteMessage(byte[] message)
        {
            MemoryStream ms = null;
            using (ms = new MemoryStream())
            {
                ms.Position = 0;
                var writer = new BinaryWriter(ms);
                writer.Write(message);
                writer.Flush();
                if (_client is { Connected: true })
                {
                    var payload = ms.ToArray();
                    _network.BeginWrite(payload, 0, payload.Length, OnWrite, null);
                }
                else
                {
                    Log.E($"WriteMessage--->>> 未连接服务器");
                }
            }
        }

        private void OnWrite(IAsyncResult ar)
        {
            try
            {
                _network.EndWrite(ar);
            }
            catch (Exception e)
            {
                Log.E($"OnWrite--->>> {e.Message}");
            }
        }

        public void Send(int cmd, IMessage message)
        {
            var buffer = new ByteBuffer();
            byte[] result;
            using (var ms = new MemoryStream())
            {
                message.WriteTo(ms);
                result = ms.ToArray();
            }

            var length = (ushort)(result.Length + 2);
            buffer.WriteUShort(length);
            buffer.WriteUShort((ushort)cmd);
            buffer.WriteBytes(result);
            WriteMessage(buffer.ToBytes());
            buffer.Close();
        }

        public void Close()
        {
            TimeUpdateMaster.Instance.EndTimer(_timeId);
            if (_client != null)
            {
                if (_client.Connected) _client.Close();
                _client = null;
            }
            _reader.Close();
            _memory.Close();
        }
    }
}
