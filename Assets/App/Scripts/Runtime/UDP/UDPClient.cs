/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年1月8 9:13
 * function    : 
 * ===============================================
 * */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace App.Runtime.UDP
{
    public static class UDPClient
    {
        private const int UdpPort = 23667;
        private const string GetServerIp = "GetServerIp";
        
        private static UdpClient client;
        private static IPEndPoint ipEndPoint;
        
        private static bool isConnected;

        public static async UniTask<ServerData> GetServerData()
        {
            client = new UdpClient(0);
            ipEndPoint = new IPEndPoint(IPAddress.Broadcast, UdpPort);
            isConnected = true;
            // 开始发送数据
            UniTask.Void(async ()=> await RequestServerIp());
            // 开始接收数据
            return await ReceiveMsg();
        }

        private static async UniTask RequestServerIp()
        {
            while (Global.ServerData == null)
            {
                var data = Encoding.UTF8.GetBytes(GetServerIp);
                await client.SendAsync(data, data.Length, ipEndPoint);
                Debug.Log("请求服务器IP地址");
                await UniTask.Delay(3000);
            }
        }

        private static async UniTask<ServerData> ReceiveMsg()
        {
            var json = "";
            while (isConnected)
            {
                await UniTask.DelayFrame(1);
                var result = await client.ReceiveAsync();
                var msg = Encoding.UTF8.GetString(result.Buffer);
                if (!msg.StartsWith(GetServerIp)) continue;
                json = msg.Split('=')[^1];
                Debug.Log(json);
                client.Close();
                isConnected = false;
            }
            return JsonUtility.FromJson<ServerData>(json);
        }
    }
    
    [Serializable]
    public class ServerData
    {
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string ServerIp;
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int ServerPort;
        /// <summary>
        /// 服务器响应时间戳（用于筛选最新响应）
        /// </summary>
        public long Timestamp;
    }
}
