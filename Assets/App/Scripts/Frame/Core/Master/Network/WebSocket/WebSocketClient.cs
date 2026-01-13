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
            UniTask.Void(async () =>
            {
                try
                {
                    ClientWebSocket = new ClientWebSocket();
                    ClientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                    cancellationTokenSource = new CancellationTokenSource();
                    var uri = new Uri(url);
                    foreach (var pair in headerPairs)
                    {
                        ClientWebSocket.Options.SetRequestHeader(pair.Key, pair.Value);
                    }
                    await ClientWebSocket.ConnectAsync(uri, cancellationTokenSource.Token);
                    OnConnectCompleted?.Invoke();
                    await StartListening();
                }
                catch (Exception ex)
                {
                    Debug.LogError("WebSocket connect exception: " + ex.Message);
                    OnConnectError?.Invoke();
                    Disconnect();
                }
            });
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
            var buffer = new byte[bufferSize];
            try
            {
                while (ClientWebSocket != null && 
                       ClientWebSocket.State == WebSocketState.Open &&
                       !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var segment = new ArraySegment<byte>(buffer);
                    var result = await ClientWebSocket.ReceiveAsync(segment, cancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("Server initiated close.");
                        await ClientWebSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "OK",
                            CancellationToken.None);
                        break;
                    }
                    // 正确使用 Count（必要时处理分片，这里假设消息不分片）
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await UniTask.SwitchToMainThread();
                    OnReceiveMessage?.Invoke(msg);
                }
            }
            catch (OperationCanceledException)
            {
                // 主动 Cancel 触发，不算错误
                Debug.Log("WebSocket receive canceled.");
            }
            catch (WebSocketException ex)
            {
                Debug.LogError("WebSocket exception: " + ex.Message);
                OnConnectError?.Invoke();
            }
        }

        public void Disconnect()
        {
            UniTask.Void(async () =>
            {
                try
                {
                    if (ClientWebSocket is { State: WebSocketState.Open or WebSocketState.CloseReceived })
                    {
                        try
                        {
                            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                            await ClientWebSocket.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                "Client closing",
                                ct.Token);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"CloseAsync failed: {e.Message}");
                        }
                    }
                    // 现在再取消内部 token，通知接收循环退出
                    cancellationTokenSource?.Cancel();
                    cancellationTokenSource = null;
                    ClientWebSocket?.Dispose();
                }
                finally
                {
                    OnConnectCompleted = null;
                    OnReceiveMessage = null;
                    OnConnectError = null;
                    Debug.Log("ws:Disconnected.");
                }
            });
        }
    }
}