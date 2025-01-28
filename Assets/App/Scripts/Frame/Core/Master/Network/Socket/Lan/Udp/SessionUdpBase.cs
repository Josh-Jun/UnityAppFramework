using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace App.Core.Master
{
	public class SessionUdpBase
	{
		private Socket socketUdp;

		private EndPoint ep;

		private Thread thread;

		public void StartReceiveData(Socket socket, EndPoint endPoint, Thread _thread)
		{
			this.socketUdp = socket;
			this.ep = endPoint;
			this.thread = _thread;
			OnConnected();
		}

		public void ReceiveData()
		{
			while (true)
			{
				try
				{
					var array = new byte[1024];
					var count = socketUdp.ReceiveFrom(array, ref ep);
					var @string = Encoding.UTF8.GetString(array, 0, count);
					OnReciveMsg(@string);
					Thread.Sleep(100);
				}
				catch
				{
					return;
				}
			}
		}

		public void SendMsg(string msg = "")
		{
			try
			{
				var array = Encoding.UTF8.GetBytes(msg);
				socketUdp.SendTo(array, array.Length, SocketFlags.None, ep);
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("SndMsgError:" + ex.Message, LogLevel.Error);
			}
		}

		public void SendMsg(byte[] data)
		{
			try
			{
				socketUdp.SendTo(data, data.Length, SocketFlags.None, ep);
			}
			catch (Exception ex)
			{
				SocketTools.LogMsg("SndMsgError:" + ex.Message, LogLevel.Error);
			}
		}

		public void SocketQuit()
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

			OnDisConnected();
			SocketTools.LogMsg("UDP已关闭...", LogLevel.Info);
		}

		protected virtual void OnConnected()
		{
			SocketTools.LogMsg("New Seesion Connected.", LogLevel.Info);
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