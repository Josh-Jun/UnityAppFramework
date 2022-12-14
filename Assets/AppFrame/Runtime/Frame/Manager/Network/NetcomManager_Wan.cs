using Google.Protobuf;
using System;
using System.Collections.Generic;
using AppFramework.Network.Wan.Tcp;
using AppFramework.Tools;
using UnityEngine;
/// <summary>
/// Socket广域网
/// </summary>
namespace AppFramework.Manager
{
    public partial class NetcomManager : SingletonMonoEvent<NetcomManager>
    {
        #region TCP

        public void StartWanTcpClient(string ip, Action<byte[]> cb)
        {
            WanTcpManager.Instance.StartAsClient(ip, SocketPort, (bool isConnect) => { });
            WanTcpManager.Instance.HandOutMsg += (byte[] bytes) => { cb?.Invoke(bytes); };
        }

        public void SendWanTcpMsg(byte[] bytes)
        {
            WanTcpManager.Instance.SendMsg(bytes);
        }

        public void WanTcpQuit()
        {
            WanTcpManager.Instance.Close();
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] Serialize(IMessage message)
        {
            return message.ToByteArray();
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packct"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(byte[] packct) where T : IMessage, new()
        {
            IMessage message = new T();
            try
            {
                return (T)message.Descriptor.Parser.ParseFrom(packct);
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}