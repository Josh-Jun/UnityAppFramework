/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年6月19 10:42
 * function    : 
 * ===============================================
 * */
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

    }
}
