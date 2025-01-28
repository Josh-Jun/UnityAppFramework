using System;
using System.Net.Sockets;
using System.Text;

namespace App.Core.Master
{
    public class SessionTcpBase
    {
        private Socket socketTcp;

        private Action closeCallBack;

        public void StartRcvData(Socket socket, Action closeCallBack)
        {
            try
            {
                this.socketTcp = socket;
                this.closeCallBack = closeCallBack;
                OnConnected();
                var pEPkg = new SocketPackage();
                socket.BeginReceive(pEPkg.headBuffer, 0, pEPkg.headLength, SocketFlags.None, ReceiveHeadData, pEPkg);
            }
            catch (Exception ex)
            {
                SocketTools.LogMsg("StartRcvData:" + ex.Message, LogLevel.Error);
            }
        }

        private void ReceiveHeadData(IAsyncResult ar)
        {
            try
            {
                var package = (SocketPackage)ar.AsyncState;
                if (socketTcp.Available == 0)
                {
                    OnDisConnected();
                    Clear();
                }
                else
                {
                    var num = socketTcp.EndReceive(ar);
                    if (num > 0)
                    {
                        package.headIndex += num;
                        if (package.headIndex < package.headLength)
                        {
                            socketTcp.BeginReceive(package.headBuffer, package.headIndex,
                                package.headLength - package.headIndex, SocketFlags.None, ReceiveHeadData, package);
                        }
                        else
                        {
                            package.InitBodyBuffer();
                            socketTcp.BeginReceive(package.bodyBuffer, 0, package.bodyLength, SocketFlags.None,
                                ReceiveBodyData, package);
                        }
                    }
                    else
                    {
                        OnDisConnected();
                        Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                SocketTools.LogMsg("ReceiveHeadError:" + ex.Message, LogLevel.Error);
            }
        }

        private void ReceiveBodyData(IAsyncResult ar)
        {
            try
            {
                var package = (SocketPackage)ar.AsyncState;
                var num = socketTcp.EndReceive(ar);
                if (num > 0)
                {
                    package.bodyIndex += num;
                    if (package.bodyIndex < package.bodyLength)
                    {
                        socketTcp.BeginReceive(package.bodyBuffer, package.bodyIndex,
                            package.bodyLength - package.bodyIndex, SocketFlags.None, ReceiveBodyData, package);
                    }
                    else
                    {
                        var msg = Encoding.UTF8.GetString(package.bodyBuffer);
                        OnReciveMsg(msg);
                        package.ResetData();
                        socketTcp.BeginReceive(package.headBuffer, 0, package.headLength, SocketFlags.None,
                            ReceiveHeadData, package);
                    }
                }
                else
                {
                    OnDisConnected();
                    Clear();
                }
            }
            catch (Exception ex)
            {
                SocketTools.LogMsg("RcvBodyError:" + ex.Message, LogLevel.Error);
            }
        }

        public void SendMsg(string msg)
        {
            var data = SocketTools.PackageLengthInfo(msg);
            try
            {
                var networkStream = new NetworkStream(socketTcp);
                if (networkStream.CanWrite)
                {
                    networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
                }
            }
            catch (Exception ex)
            {
                SocketTools.LogMsg("SndMsgError:" + ex.Message, LogLevel.Error);
            }
        }

        public void SendMsg(byte[] _data)
        {
            var data = SocketTools.PackageLengthInfo(_data);
            try
            {
                var networkStream = new NetworkStream(socketTcp);
                if (networkStream.CanWrite)
                {
                    networkStream.BeginWrite(data, 0, data.Length, SendCallBack, networkStream);
                }
            }
            catch (Exception ex)
            {
                SocketTools.LogMsg("SndMsgError:" + ex.Message, LogLevel.Error);
            }
        }

        private void SendCallBack(IAsyncResult ar)
        {
            using var networkStream = (NetworkStream)ar.AsyncState;
            try
            {
                networkStream.EndWrite(ar);
                networkStream.Flush();
                networkStream.Close();
            }
            catch (Exception ex)
            {
                SocketTools.LogMsg("SndMsgError:" + ex.Message, LogLevel.Error);
            }
        }

        public void Clear()
        {
            closeCallBack?.Invoke();
            socketTcp.Close();
        }

        protected virtual void OnConnected()
        {
            SocketTools.LogMsg("New Session Connected.", LogLevel.Info);
        }

        protected virtual void OnReciveMsg(string msg)
        {
            SocketTools.LogMsg("Receive Network Message.", LogLevel.Info);
        }

        protected virtual void OnDisConnected()
        {
            SocketTools.LogMsg("Session Disconnected.", LogLevel.Info);
        }
    }
}
