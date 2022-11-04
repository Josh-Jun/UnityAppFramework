using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace AppFramework.Network.Lan.Tcp
{
	public class SocketTcp<T> where T : SessionTcpBase, new()
	{
		private Socket socketTcp = null;

		public T session = null;

		public int backlog = 100;

		private List<T> sessionList = new List<T>();

		private int overtime = 5000;

		private Action<bool> serverCallBack;

		private Action<bool> clientCallBack;

		public SocketTcp()
		{
			socketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public void StartAsServer(string ip, int port, Action<bool> cb = null)
		{
			try
			{
				serverCallBack = cb;
				socketTcp.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
				socketTcp.Listen(backlog);
				socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
				SocketTools.LogMsg("Tcp服务端开启成功！正在等待连接......", LogLevel.Info);
				serverCallBack?.Invoke(true);
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("Tcp服务端开启失败：" + ex.Message, LogLevel.Error);
				serverCallBack?.Invoke(false);
			}
		}

		private void ClientConnectCallBack(IAsyncResult ar)
		{
			try
			{
				Socket socket = socketTcp.EndAccept(ar);
				T session = new T();
				session.StartRcvData(socket, delegate
				{
					if (sessionList.Contains(session))
					{
						sessionList.Remove(session);
						SocketTools.LogMsg("客户端断开连接......", LogLevel.Info);
					}
				});
				sessionList.Add(session);
				SocketTools.LogMsg("Tcp连接客户端成功！正在接收数据......", LogLevel.Info);
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("Tcp服务器关闭：" + ex.Message, LogLevel.Error);
				serverCallBack?.Invoke(false);
			}

			socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
		}

		public void StartAsClient_IP(string ip, int port, Action<bool> cb = null)
		{
			try
			{
				clientCallBack = cb;
				IAsyncResult asyncResult = socketTcp.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port),
					ServerConnectCallBack, socketTcp);
				bool flag = asyncResult.AsyncWaitHandle.WaitOne(overtime, exitContext: true);
				if (!flag)
				{
					Close();
					SocketTools.LogMsg("Tcp客户端连接超时", LogLevel.Error);
					clientCallBack?.Invoke(flag);
				}
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("Tcp客户端启动失败：" + ex.Message, LogLevel.Error);
				clientCallBack?.Invoke(obj: false);
			}
		}

		public void StartAsClient_Name(string name, int port, Action<bool> cb = null)
		{
			try
			{
				clientCallBack = cb;
				IAsyncResult asyncResult = socketTcp.BeginConnect(
					new IPEndPoint(Dns.GetHostEntry(name).AddressList[0], port), ServerConnectCallBack, socketTcp);
				bool flag = asyncResult.AsyncWaitHandle.WaitOne(overtime, exitContext: true);
				if (!flag)
				{
					Close();
					SocketTools.LogMsg("Tcp客户端连接超时", LogLevel.Error);
					clientCallBack?.Invoke(flag);
				}
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("Tcp客户端启动失败：" + ex.Message, LogLevel.Error);
				clientCallBack?.Invoke(false);
			}
		}

		private void ServerConnectCallBack(IAsyncResult ar)
		{
			try
			{
				socketTcp.EndConnect(ar);
				session = new T();
				session.StartRcvData(socketTcp, delegate
				{
					SocketTools.LogMsg("Tcp服务器断开连接......", LogLevel.Info);
					clientCallBack?.Invoke(false);
				});
				SocketTools.LogMsg("Tcp连接服务器成功！正在接收数据......", LogLevel.Info);
				clientCallBack?.Invoke(true);
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("Tcp客户端关闭：" + ex.Message, LogLevel.Error);
				clientCallBack?.Invoke(false);
			}
		}

		public bool Close()
		{
			try
			{
				serverCallBack = null;
				clientCallBack = null;
				if (socketTcp != null)
				{
					if (socketTcp.Connected)
					{
						socketTcp.Shutdown(SocketShutdown.Both);
					}

					socketTcp.Close();
				}

				return true;
			}
			catch (Exception arg)
			{
				SocketTools.LogMsg("Tcp关闭Socket错误：" + arg, LogLevel.Error);
				return false;
			}
		}

		public void SetLog(bool log = true, Action<string, int> logCallBack = null)
		{
			if (!log)
			{
				SocketTools.log = false;
			}

			if (logCallBack != null)
			{
				SocketTools.logCallBack = logCallBack;
			}
		}
	}
}