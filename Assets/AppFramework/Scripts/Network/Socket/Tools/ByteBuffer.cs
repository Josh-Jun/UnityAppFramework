using System.IO;
using System.Text;
using System;
public class ByteBuffer
{
    MemoryStream stream = null;
    BinaryWriter writer = null;
    BinaryReader reader = null;

    public ByteBuffer()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
    }

    public ByteBuffer(byte[] data)
    {
        if (data != null)
        {
            stream = new MemoryStream(data);
            stream.Position = 0;
            reader = new BinaryReader(stream);
        }
        else
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }
    }
    public int GetCurPos()
    {
        return (int)stream.Position;
    }

    public void Close()
    {
        if (writer != null) writer.Close();
        if (reader != null) reader.Close();

        stream.Close();
        writer = null;
        reader = null;
        stream = null;
    }

    public void WriteByte(int v)
    {
        writer.Write((byte)v);
    }
    public void WriteInt(int v)
    {
        //  writer.Write((int)v);
        int b = System.Net.IPAddress.HostToNetworkOrder(v);
        byte[] temp = BitConverter.GetBytes(b);
        writer.Write(BitConverter.ToInt32(temp, 0));
    }

    public void WriteShort(short v)
    {
        //writer.Write((short)v);
        short b = System.Net.IPAddress.HostToNetworkOrder(v);
        byte[] temp = BitConverter.GetBytes(b);
        writer.Write(BitConverter.ToInt16(temp, 0));
    }
    public void WriteUShort(ushort v)
    {
        // writer.Write((ushort)v);
        byte[] temp = BitConverter.GetBytes(v);
        Array.Reverse(temp);
        writer.Write(BitConverter.ToUInt16(temp, 0));
    }

    public void WriteLong(long v)
    {
        // writer.Write((long)v);
        // WriteString(v.ToString());
        long b = System.Net.IPAddress.HostToNetworkOrder(v);
        byte[] temp = BitConverter.GetBytes(b);
        writer.Write(BitConverter.ToInt64(temp, 0)); ;
    }

    public void WriteFloat(float v)
    {
        byte[] temp = BitConverter.GetBytes(v);
        Array.Reverse(temp);
        writer.Write(BitConverter.ToSingle(temp, 0));
    }

    public void WriteDouble(double v)
    {
        byte[] temp = BitConverter.GetBytes(v);
        Array.Reverse(temp);
        writer.Write(BitConverter.ToDouble(temp, 0));
    }

    public void WriteString(string v)
    {
        if (string.IsNullOrEmpty(v))
        {
            return;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(v);
        if (bytes.Length > 0)
            writer.Write(bytes);
    }

    public void WriteBytes(byte[] v)
    {
        if (v != null)
        {
            writer.Write(v);
        }
    }

    public void WriteShortArray(short[] values)
    {
        if (values != null)
        {
            foreach (short v in values)
            {
                writer.Write(v);
            }
        }
    }
    public void WriteIntArray(int[] values)
    {
        if (values != null)
        {
            foreach (int v in values)
            {
                writer.Write(v);
            }
        }
    }
    public void WriteLongArray(long[] values)
    {
        if (values != null)
        {
            foreach (long v in values)
            {
                writer.Write(v);
            }
        }
    }
    public void WriteFloatArray(float[] values)
    {
        if (values != null)
        {
            foreach (float v in values)
            {
                writer.Write(v);
            }
        }
    }
    public void WriteDoubleArray(double[] values)
    {
        if (values != null)
        {
            foreach (double v in values)
            {
                writer.Write(v);
            }
        }
    }
    public void WriteIntArrays(int[][] values)
    {
        if (values != null)
        {
            foreach (int[] v in values)
            {
                WriteIntArray(v);
            }
        }
    }


    public byte ReadByte()
    {
        return reader.ReadByte();
    }

    public int ReadInt()
    {
        int v = (int)reader.ReadInt32();
        return System.Net.IPAddress.NetworkToHostOrder(v);//网络字节转成本机
    }

    public short ReadShort()
    {
        //  return (short)reader.ReadInt16();
        short v = (short)reader.ReadInt16();
        return System.Net.IPAddress.NetworkToHostOrder(v);//大小端转换
    }
    public ushort ReadUShort()
    {
        // return (ushort)reader.ReadUInt16();

        short v = (short)reader.ReadInt16();
        return (ushort)System.Net.IPAddress.NetworkToHostOrder(v);
    }


    public long ReadLong()
    {
        // return (long)reader.ReadInt64();

        long v = (long)reader.ReadInt64();
        return System.Net.IPAddress.NetworkToHostOrder(v);
    }

    public float ReadFloat()
    {
        byte[] temp = BitConverter.GetBytes(reader.ReadSingle());
        Array.Reverse(temp);
        return BitConverter.ToSingle(temp, 0);
    }

    public double ReadDouble()
    {
        byte[] temp = BitConverter.GetBytes(reader.ReadDouble());
        Array.Reverse(temp);
        return BitConverter.ToDouble(temp, 0);
    }

    public string ReadString(int len)
    {
        byte[] buffer = new byte[len];
        buffer = reader.ReadBytes(len);
        return Encoding.UTF8.GetString(buffer);
        //return Encoding.Default.GetString(buffer); 
    }

    public byte[] ReadBytes(int len)
    {
        return reader.ReadBytes(len);
    }

    public short[] ReadShortArray(int len)
    {
        short[] data = new short[len];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (short)ReadShort();
        }
        return data;
    }
    public ushort[] ReadUShortArray(int len)
    {
        ushort[] data = new ushort[len];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (ushort)ReadShort();
        }
        return data;
    }
    public int[] ReadIntArray(int len)
    {
        int[] data = new int[len];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = ReadInt();
        }
        return data;
    }

    public long[] ReadLongArray(int len)
    {
        long[] data = new long[len];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = ReadLong();
        }
        return data;
    }
    public float[] ReadFloatArray(int len)
    {
        float[] data = new float[len];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = ReadFloat();
        }
        return data;
    }
    public double[] ReadDoubleArray(int len)
    {
        double[] data = new double[len];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = ReadDouble();
        }
        return data;
    }


    public byte[] ToBytes()
    {
        writer.Flush();
        return stream.ToArray();
    }

    public void Flush()
    {
        writer.Flush();
    }



}
