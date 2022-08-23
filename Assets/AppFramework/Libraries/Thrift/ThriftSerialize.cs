using System;
using System.Collections.Generic;
using System.Text;
using Thrift.Protocol;
using Thrift.Transport;
using System.IO;

namespace Communication
{
    /// <summary>
    /// thrift对象序列化工具
    /// </summary>
    public class ThriftSerialize
    {
        public static byte[] Serialize(TBase tbase)
        {
            if (tbase == null)
            {
                return null;
            }
            using (Stream outputStream = new MemoryStream(64))
            {
                TStreamTransport transport = new TStreamTransport(null, outputStream);
                TProtocol protocol = new TCompactProtocol(transport);
                tbase.Write(protocol);
                byte[] bytes = new byte[outputStream.Length];
                outputStream.Position = 0;
                outputStream.Read(bytes, 0, bytes.Length);
                return bytes;
            }
        }

        public static void DeSerialize(TBase tbase, byte[] bytes)
        {
            if (tbase == null || bytes == null)
            {
                return;
            }
            using (Stream inputStream = new MemoryStream(64))
            {
                inputStream.Write(bytes, 0, bytes.Length);
                inputStream.Position = 0;
                TStreamTransport transport = new TStreamTransport(inputStream, null);
				TProtocol protocol = new TCompactProtocol(transport);
                tbase.Read(protocol);
            }
        }
    }
}
