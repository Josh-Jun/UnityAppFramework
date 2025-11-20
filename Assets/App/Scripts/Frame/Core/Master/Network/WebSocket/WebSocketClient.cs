/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年11月19 16:10
 * function    :
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace App.Core.Master
{
    public class WebSocketClient
    {
        private const int bufferSize = 1024 * 1024;
        private CancellationTokenSource cancellationTokenSource;
        private readonly Dictionary<string, string> headerPairs = new ();

        public ClientWebSocket ClientWebSocket { get; private set; }

        public Action OnConnectCompleted { get; set; }
        public Action<string> OnReceiveMessage { get; set; }
        public Action OnConnectError { get; set; }

        public void Connect(string url)
        {
            try
            {
                ClientWebSocket ??= new ClientWebSocket();
                cancellationTokenSource ??= new CancellationTokenSource();
                var uri = new Uri(url);
                foreach (var pair in headerPairs)
                {
                    ClientWebSocket.Options.SetRequestHeader(pair.Key, pair.Value);
                }
                ClientWebSocket.ConnectAsync(uri, cancellationTokenSource.Token).ContinueWith(async t =>
                {
                    OnConnectCompleted?.Invoke();
                    await StartListening();
                });
            }
            catch (WebSocketException ex)
            {
                Disconnect();
                OnConnectError?.Invoke();
                Debug.LogError("WebSocket exception: " + ex.Message);
            }
        }
        
        public void AddHeader(string key, string value)
        {
            headerPairs.TryAdd(key, value);
        }

        public void Send(string msg)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    if(ClientWebSocket is not { State: WebSocketState.Open }) return;
                    var bytes = Encoding.UTF8.GetBytes(msg);
                    var array = new ArraySegment<byte>(bytes);
                    await ClientWebSocket.SendAsync(array, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
                }
                catch (WebSocketException ex)
                {
                    Debug.LogError(ex.Message);
                }
            });
        }

        public void Send(byte[] bytes)
        {
            UniTask.Void(async () =>
            {
                try
                {
                    if(ClientWebSocket is not { State: WebSocketState.Open }) return;
                    if (bytes is not { Length: > 0 }) return;
                    var array = new ArraySegment<byte>(bytes);
                    await ClientWebSocket.SendAsync(array, WebSocketMessageType.Binary, true, cancellationTokenSource.Token); //发送数据
                }
                catch (WebSocketException ex)
                {
                    Debug.LogError(ex.Message);
                }
            });
        }

        private async UniTask StartListening()
        {
            try
            {
                while (true)
                {
                    if(ClientWebSocket is not { State: WebSocketState.Open }) return;
                    var buffer = new byte[bufferSize];
                    var message = new ArraySegment<byte>(buffer);
                    await ClientWebSocket.ReceiveAsync(message, cancellationTokenSource.Token);
                    var msg = Encoding.UTF8.GetString(buffer);
                    await UniTask.SwitchToMainThread();
                    OnReceiveMessage?.Invoke(msg);
                }
            }
            catch (WebSocketException ex)
            {
                Disconnect();
                OnConnectError?.Invoke();
                Debug.LogError("WebSocket exception: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            if (ClientWebSocket is { State: WebSocketState.Open })
            {
                ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", new CancellationToken(false));
                ClientWebSocket.Dispose();
                ClientWebSocket = null;
            }

            Debug.Log("ws:Disconnected.");
        }
    }
}