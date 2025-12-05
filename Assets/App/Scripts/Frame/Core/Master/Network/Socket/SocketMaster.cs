/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年6月19 10:42
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Tools;
using Google.Protobuf;
using UnityEngine;

namespace App.Core.Master
{
    public partial class SocketMaster : SingletonMonoEvent<SocketMaster>
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private byte[] Serialize(IMessage message)
        {
            return message.ToByteArray();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private T DeSerialize<T>(byte[] buffer) where T : IMessage, new()
        {
            IMessage message = new T();
            return (T)message.Descriptor.Parser.ParseFrom(buffer);
        }
        
        /// <summary>
        /// 网络回调监听类
        /// </summary>
        private class NetworkEventHandler
        {
            private readonly Dictionary<PB_MSG_CMD, List<NetworkEventHandleItem>>
                _callbacks = new Dictionary<PB_MSG_CMD, List<NetworkEventHandleItem>>();

            private class NetworkEventHandleItem
            {
                public bool AutoRemove;
                public Action<IMessage> Callback;
            }

            public bool HasCallback(PB_MSG_CMD cmd)
            {
                return _callbacks.ContainsKey(cmd) && _callbacks[cmd].Count > 0;
            }

            public void AddListener(PB_MSG_CMD cmd, Action<IMessage> listener, bool autoRemove)
            {
                if (!_callbacks.ContainsKey(cmd))
                {
                    var list = new List<NetworkEventHandleItem>();
                    _callbacks.Add(cmd, list);
                }

                var item = new NetworkEventHandleItem()
                    { AutoRemove = autoRemove, Callback = listener };

                Log.W("Add listener for " + cmd + " autoRemove = " + autoRemove);

                _callbacks[cmd].Add(item);
            }

            public void RemoveListener(PB_MSG_CMD cmd, Action<IMessage> listener)
            {
                if (!_callbacks.TryGetValue(cmd, out var callback)) return;
                var targets = callback.FindAll(i => i.Callback == listener);
                if (targets.Count > 0)
                {
                    targets.ForEach(item => { _callbacks[cmd].Remove(item); });

                    Log.W(targets.Count + " listener(s) removed for " + cmd);
                }
                else
                {
                    Log.W("Zero listener removed for msgDef: " + cmd + " with listener: " + listener);
                }
            }

            public void RemoveListener(PB_MSG_CMD cmd)
            {
                if (!_callbacks.TryGetValue(cmd, out var targets)) return;
                if (targets.Count <= 0) return;
                targets.ForEach(item => { _callbacks[cmd].Remove(item); });

                Log.W(targets.Count + " listener(s) removed for " + cmd);
            }

            public void RemoveAllListener()
            {
                _callbacks.Clear();
            }

            public void Invoke(PB_MSG_CMD cmd, IMessage message)
            {
                if (!_callbacks.ContainsKey(cmd)) return;
                for (var i = _callbacks[cmd].Count - 1; i >= 0; i--)
                {
                    var item = _callbacks[cmd][i];
                    item.Callback?.Invoke(message);

                    if (item.AutoRemove)
                    {
                        _callbacks[cmd].RemoveAt(i);
                    }
                }
            }
        }
    }
}
