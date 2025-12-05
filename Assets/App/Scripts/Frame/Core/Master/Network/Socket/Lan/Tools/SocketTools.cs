using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace App.Core.Master
{
	public static class SocketTools
	{
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
	}
}