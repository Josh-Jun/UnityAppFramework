using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace App.Core.Master
{
	public class SocketUdp<T> where T : SessionUdpBase, new()
	{
		public readonly T session = new();

		private readonly Socket socketUdp = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

		private IPEndPoint iep;

		private EndPoint ep;

		private Thread thread;

		public void StartAsServer(int port, Action<bool> cb = null)
		{
			try
			{
				socketUdp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
				iep = new IPEndPoint(IPAddress.Broadcast, port);
				ep = iep;
				thread = new Thread(StartServerReceive);
				thread.Start();
				SocketTools.LogMsg("Udp服务端开启成功!", LogLevel.Info);
				cb?.Invoke(true);
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("Udp服务端开启失败!" + ex.Message, LogLevel.Error);
				cb?.Invoke(false);
			}
		}

		private void StartServerReceive()
		{
			session.StartReceiveData(socketUdp, ep, thread);
			session.SendMsg();
			session.ReceiveData();
		}

		public void StartAsClient(int port, Action<bool> cb = null)
		{
			try
			{
				iep = new IPEndPoint(IPAddress.Any, port);
				socketUdp.Bind(iep);
				ep = iep;
				thread = new Thread(StartClientReceive);
				thread.Start();
				SocketTools.LogMsg("Udp客户端开启成功!", LogLevel.Info);
				cb?.Invoke(true);
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("Udp客户端开启失败!" + ex.Message, LogLevel.Error);
				cb?.Invoke(false);
			}
		}

		private void StartClientReceive()
		{
			session.StartReceiveData(socketUdp, ep, thread);
			session.ReceiveData();
		}

		public void Close()
		{
			if (thread != null)
			{
				thread.Interrupt();
				thread.Abort();
			}

			if (socketUdp != null)
			{
				socketUdp.Close();
			}

			SocketTools.LogMsg("UDP已关闭...", LogLevel.Info);
		}
	}
}