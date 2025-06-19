using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace App.Core.Master
{
	public enum LogLevel
	{
		None,
		Warn,
		Error,
		Info
	}

	public static class SocketTools
	{
		public static bool log = true;

		public static Action<string, int> logCallBack = null;

		public static byte[] PackageNetMsg<T>(T msg) where T : SocketMsg
		{
			return PackageLengthInfo(Serialize(msg));
		}

		public static byte[] PackageLengthInfo(byte[] data)
		{
			var num = data.Length;
			var array = new byte[num + 4];
			var bytes = BitConverter.GetBytes(num);
			bytes.CopyTo(array, 0);
			data.CopyTo(array, 4);
			return array;
		}

		public static byte[] PackageLengthInfo(string msg)
		{
			var data = Encoding.UTF8.GetBytes(msg.ToCharArray());
			var num = data.Length;
			var array = new byte[num + 4];
			var bytes = BitConverter.GetBytes(num);
			bytes.CopyTo(array, 0);
			data.CopyTo(array, 4);
			return array;
		}

		public static byte[] Serialize<T>(T pkg) where T : SocketMsg
		{
			using var memoryStream = new MemoryStream();
			var binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(memoryStream, pkg);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream.ToArray();
		}

		public static T DeSerialize<T>(byte[] bs) where T : SocketMsg
		{
			using var serializationStream = new MemoryStream(bs);
			var binaryFormatter = new BinaryFormatter();
			return (T)binaryFormatter.Deserialize(serializationStream);
		}

		public static void LogMsg(string msg, LogLevel lv = LogLevel.None)
		{
			if (!log)
			{
				return;
			}

			msg = DateTime.Now.ToLongTimeString() + " >> " + msg;
			if (logCallBack != null)
			{
				logCallBack(msg, (int)lv);
				return;
			}

			switch (lv)
			{
				case LogLevel.None:
					Console.WriteLine(msg);
					break;
				case LogLevel.Warn:
					Console.WriteLine("//--------------------Warn--------------------//");
					Console.WriteLine(msg);
					break;
				case LogLevel.Error:
					Console.WriteLine("//--------------------Error--------------------//");
					Console.WriteLine(msg);
					break;
				case LogLevel.Info:
					Console.WriteLine("//--------------------Info--------------------//");
					Console.WriteLine(msg);
					break;
				default:
					Console.WriteLine("//--------------------Error--------------------//");
					Console.WriteLine(msg + " >> Unknow Log Type\n");
					break;
			}
		}
	}
}