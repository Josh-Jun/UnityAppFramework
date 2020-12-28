using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class SocketTcp<T> where T : SessionTcpBase, new()
{
	private Socket socketTcp = null;

	public T session = null;

	public int backlog = 10;

	private List<T> sessionList = new List<T>();

	public SocketTcp()
	{
		socketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	}

	public void StartAsServer(string ip, int port)
	{
		try
		{
			socketTcp.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
			socketTcp.Listen(backlog);
			socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
			SocketTools.LogMsg("Tcp服务端开启成功！正在等待连接......", LogLevel.Info);
		}
		catch (Exception ex)
		{
			SocketTools.LogMsg("Tcp服务端开启失败：" + ex.Message, LogLevel.Error);
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
		}
		socketTcp.BeginAccept(ClientConnectCallBack, socketTcp);
	}

	public void StartAsClient(string ip, int port)
	{
		try
		{
			socketTcp.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), ServerConnectCallBack, socketTcp);
			SocketTools.LogMsg("Tcp客户端启动成功！正在连接服务器......", LogLevel.Info);
		}
		catch (Exception ex)
		{
			SocketTools.LogMsg("Tcp客户端启动失败：" + ex.Message, LogLevel.Error);
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
			});
			SocketTools.LogMsg("Tcp连接服务器成功！正在接收数据......", LogLevel.Info);
		}
		catch (Exception ex)
		{
			SocketTools.LogMsg("Tcp客户端关闭：" + ex.Message, LogLevel.Error);
		}
	}

	public void Close()
	{
		if (socketTcp != null)
		{
			socketTcp.Close();
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