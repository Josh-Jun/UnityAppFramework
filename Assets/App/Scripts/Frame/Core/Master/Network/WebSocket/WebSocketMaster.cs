/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月4 8:55
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using App.Core.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace App.Core.Master
{
    public class WebSocketMaster : SingletonMonoEvent<WebSocketMaster>
    {
        private ClientWebSocket _clientWebSocket;
        private CancellationTokenSource _cancellationToken;
        private bool _isConnecting = false;
        public Action<string> OnReceiveMessage { get; set; }

        public void Connect(string url, Action callback = null)
        {
            _clientWebSocket = new ClientWebSocket();
            _cancellationToken = new CancellationTokenSource();
            var uri = new Uri(url);
            _clientWebSocket.ConnectAsync(uri, _cancellationToken.Token).ContinueWith(async t =>
            {
                callback?.Invoke();
                _isConnecting = true;
                await Receive();
            });
        }
        public void Close()
        {
            if (!_isConnecting) return;
            _isConnecting = false;
            _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            _clientWebSocket.Dispose();
            _cancellationToken.Cancel();
        }

        public void Send(string msg)
        {
            UniTask.Void(async () =>
            {
                if (!_isConnecting) return;
                if (_clientWebSocket.State != WebSocketState.Open) return;
                var bytes = Encoding.UTF8.GetBytes(msg);
                var array = new ArraySegment<byte>(bytes);
                await _clientWebSocket.SendAsync(array, WebSocketMessageType.Text, true, _cancellationToken.Token);
            });
        }

        public void Send(byte[] bytes)
        {
            UniTask.Void(async () =>
            {
                if (!_isConnecting) return;
                if (_clientWebSocket.State != WebSocketState.Open) return;
                if (bytes == null || bytes.Length <= 0) return;
                var array = new ArraySegment<byte>(bytes);
                await _clientWebSocket.SendAsync(array, WebSocketMessageType.Binary, true, _cancellationToken.Token);
            });
        }

        private async UniTask Receive()
        {
            while (true)
            {
                if (!_isConnecting) return;
                var result = new byte[1024];
                var message = new ArraySegment<byte>(result);
                await _clientWebSocket.ReceiveAsync(message, new CancellationToken());
                var msg = Encoding.UTF8.GetString(result);
                await UniTask.SwitchToMainThread();
                OnReceiveMessage?.Invoke(msg);
            }
        }

        private void OnDestroy()
        {
            Close();
        }
    }
}
