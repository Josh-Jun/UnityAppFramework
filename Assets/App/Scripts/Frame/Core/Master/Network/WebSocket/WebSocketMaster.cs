/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月4 8:55
 * function    :
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using App.Core.Tools;

namespace App.Core.Master
{
    public class WebSocketMaster : SingletonMonoEvent<WebSocketMaster>
    {
        private readonly Dictionary<string, WebSocketClient> _webSocketClients = new();
        private readonly Dictionary<string, Dictionary<string, string>> headerPairs = new();
        private void Awake()
        {
            TimeTaskMaster.Instance.AddTimeTask(() =>
            {
                foreach (var pair in _webSocketClients.Where(pair => pair.Value.ClientWebSocket.State is WebSocketState.None or WebSocketState.Closed))
                {
                    pair.Value.Connect(pair.Key);
                }
            }, 5, TimeUnit.Second, 0);
        }

        public void Connect(string url, Action onConnectCompleted = null, Action<string> onReceiveMessage = null, Action onConnectError = null)
        {
            if (!_webSocketClients.TryGetValue(url, out var client))
            {
                client = new WebSocketClient();
                _webSocketClients.Add(url, client);
                client.OnConnectCompleted += onConnectCompleted;
                client.OnReceiveMessage += onReceiveMessage;
                client.OnConnectError += onConnectError;
            }

            if (headerPairs.TryGetValue(url, out var headerPair))
            {
                foreach (var pair in headerPair)
                {
                    client.AddHeader(pair.Key, pair.Value);
                }
            }
            client.Connect(url);
        }
        
        public void AddHeader(string url, string key, string value)
        {
            if (!headerPairs.ContainsKey(url))
            {
                var pair = new Dictionary<string, string> { { key, value } };
                headerPairs.Add(url, pair);
            }
            else
            {
                if (!headerPairs[url].ContainsKey(key))
                {
                    headerPairs[url].Add(key, value);
                }
            }
        }

        public void Disconnect(string url)
        {
            if (_webSocketClients.TryGetValue(url, out var client))
            {
                client.Disconnect();
            }
            _webSocketClients.Remove(url);
        }

        public void Send(string url, byte[] bytes)
        {
            if (_webSocketClients.TryGetValue(url, out var client))
            {
                client.Send(bytes);
            }
        }

        public void Send(string url, string content)
        {
            if (_webSocketClients.TryGetValue(url, out var client))
            {
                client.Send(content);
            }
        }

        private void OnDestroy()
        {
            foreach (var client in _webSocketClients.Values)
            {
                client.Disconnect();
            }
        }
    }
}