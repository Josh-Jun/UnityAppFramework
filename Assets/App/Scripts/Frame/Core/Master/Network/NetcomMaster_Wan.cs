using Google.Protobuf;
using System;
using App.Core.Tools;

namespace App.Core.Master
{
    /// <summary>
    /// Socket广域网
    /// </summary>
    public partial class NetcomMaster : SingletonMonoEvent<NetcomMaster>
    {
        #region TCP

        public void StartWanTcpClient(string ip, Action<byte[]> cb)
        {
            WanTcpManager.Instance.StartAsClient(ip, Global.SocketPort, (bool isConnect) => { });
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
            return (T)message.Descriptor.Parser.ParseFrom(packct);
        }

        #endregion
    }
}