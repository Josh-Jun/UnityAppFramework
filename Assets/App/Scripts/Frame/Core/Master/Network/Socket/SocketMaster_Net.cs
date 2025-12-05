using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using App.Core.Tools;
using Google.Protobuf.Reflection;
using UnityEngine;

namespace App.Core.Master
{
    /// <summary>
    /// Socket Net
    /// </summary>
    public partial class SocketMaster : SingletonMonoEvent<SocketMaster>
    {
        private static readonly NetworkEventHandler NewEventHandler = new NetworkEventHandler();
        private static readonly Dictionary<PB_MSG_CMD, Type> S2CMessageMap = new Dictionary<PB_MSG_CMD, Type>();
        
        public void ConnectNetServer(string host, int port, Action callback = null)
        {
            NetSocketClient.Instance.Connect(host, port, callback);
            NetSocketClient.Instance.OnDisConnected += OnDisConnected;
            NetSocketClient.Instance.OnReceiveMessage += OnReceiveMessage;
        }

        public void SendMessage(PB_MSG_CMD cmd, IMessage message, Action<IMessage> callback = null, bool autoRemoveCallback = false)
        {
            NetSocketClient.Instance.Send((int)cmd, message);
            var str = cmd.ToString().Replace("C2S", "S2C");
            var scMsgCmd = (PB_MSG_CMD)Enum.Parse(typeof(PB_MSG_CMD), str, false);

            if (!S2CMessageMap.ContainsKey(scMsgCmd))
            {
                var msgTypeStr = message.GetType().ToString();
                var dseTypeStr = msgTypeStr.Replace("CS", "SC");
                var dseType = Type.GetType(dseTypeStr);

                S2CMessageMap.Add(scMsgCmd, dseType);
            }

            if (callback != null)
            {
                NewEventHandler.AddListener(cmd, callback, autoRemoveCallback);
            }
        }

        // 接收消息
        private void OnReceiveMessage(int msgId, byte[] buffer)
        {
            var cmd = (PB_MSG_CMD)msgId;
            IMessage msg = null;
            if (S2CMessageMap.TryGetValue(cmd, out var msgType))
            {
                var descriptor = (MessageDescriptor)msgType.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null);
                if (descriptor != null) msg = descriptor.Parser.ParseFrom(buffer);
            }

            if (msg == null) return;
            if (!NewEventHandler.HasCallback(cmd)) return;
            NewEventHandler.Invoke(cmd, msg);
        }
        
        public void AddListener<T>(PB_MSG_CMD cmd, Action<IMessage> callback, bool autoRemoveCallback = false) where T : IMessage
        {
            if (callback == null)
            {
                throw new Exception("callback is null");
            }

            if (!cmd.ToString().Contains("S2C"))
            {
                throw new Exception("cmd is not a S2C CMD");
            }

            if (typeof(T).ToString().Contains("CS"))
            {
                throw new Exception("generic type must be C2S message type");
            }

            if (!S2CMessageMap.ContainsKey(cmd))
            {
                S2CMessageMap.Add(cmd, typeof(T));
            }

            NewEventHandler.AddListener(cmd, callback, autoRemoveCallback);
        }

        public bool HasListener(PB_MSG_CMD cmd)
        {
            return S2CMessageMap.ContainsKey(cmd);
        }

        public void RemoveListener(PB_MSG_CMD cmd)
        {
            NewEventHandler.RemoveListener(cmd);
        }

        public void RemoveListener(PB_MSG_CMD cmd, Action<IMessage> callback)
        {
            NewEventHandler.RemoveListener(cmd, callback);
        }

        public void RemoveAllListener()
        {
            NewEventHandler.RemoveAllListener();
        }

        // 处理服务器断开事件,重连,退出程序
        private void OnDisConnected()
        {
        }

        private void OnApplicationQuit()
        {
            NetSocketClient.Instance.Close();
        }
    }
}