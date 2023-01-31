using System;
using System.IO;
using UnityEngine;

namespace AppFrame.Network
{
    public static class StreamHelper
    {
        /// <summary>
        /// 注意 从当前Postion开始读取 读取完毕后自动偏移 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static byte ReadByte(ref MemoryStream ms)
        {
            if (ms == null)
            {
                throw new NullReferenceException("StreamHelper.ReadByte , MemoryStream=null");
            }

            int num = (int)ms.Position + 1;
            if (num > ms.Length)
            {
                throw new IndexOutOfRangeException(
                    $"StreamHelper.ReadByte , 无法从 MemoryStream 读取 , Length:{ms.Length} Position:{ms.Position}");
            }

            ms.Position += 1;
            byte[] buffer = ms.GetBuffer();
            return buffer[num - 1];
        }

        /// <summary>
        /// BinaryReader.ReadInt16
        /// 注意 从当前Postion开始读取 读取完毕后自动偏移 
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static short ReadInt16(ref MemoryStream ms)
        {
            if (ms == null)
            {
                throw new NullReferenceException("StreamHelper.ReadByte , MemoryStream=null");
            }

            int num = (int)ms.Position + 2;
            if (num > ms.Length)
            {
                throw new IndexOutOfRangeException(
                    $"StreamHelper.ReadByte , 无法从 MemoryStream 读取 , Length:{ms.Length} Position:{ms.Position}");
            }

            ms.Position += 2;
            byte[] buffer = ms.GetBuffer();
            short value = (short)(buffer[num - 2] | buffer[num - 1] << 8);
            return System.Net.IPAddress.NetworkToHostOrder(value); //大端转换小端
        }

        /// <summary>
        /// BinaryReader.ReadInt32
        /// 注意 从当前Postion开始读取 读取完毕后自动偏移
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int ReadInt32(ref MemoryStream ms)
        {
            if (ms == null)
            {
                throw new NullReferenceException("StreamHelper.ReadInt32 , MemoryStream=null");
            }

            int num = (int)ms.Position + 4;
            if (num > ms.Length)
            {
                throw new IndexOutOfRangeException(
                    $"StreamHelper.ReadInt32 , 无法从 MemoryStream 读取 , Length:{ms.Length} Position:{ms.Position}");
            }

            ms.Position += 4;
            byte[] buffer = ms.GetBuffer();
            int value = (int)buffer[num - 4] |
                        (int)buffer[num - 3] << 8 |
                        (int)buffer[num - 2] << 16 |
                        (int)buffer[num - 1] << 24;
            return System.Net.IPAddress.NetworkToHostOrder(value); //大端转换小端
        }

        public static long ReadInt64(ref MemoryStream ms)
        {
            if (ms == null)
            {
                throw new NullReferenceException("StreamHelper.ReadInt64 , MemoryStream=null");
            }

            int num = (int)ms.Position + 8;
            if (num > ms.Length)
            {
                throw new IndexOutOfRangeException(
                    $"StreamHelper.ReadInt64 , 无法从 MemoryStream 读取 , Length:{ms.Length} Position:{ms.Position}");
            }

            ms.Position += 8;
            byte[] m_buffer = ms.GetBuffer();
            // long value = BitConverter.ToInt32(m_buffer, num - 8);
            long value = (long)(uint)((int)m_buffer[num - 4] |
                                      (int)m_buffer[num - 3] << 8 |
                                      (int)m_buffer[num - 2] << 16 |
                                      (int)m_buffer[num - 1] << 24) << 32 |
                         (long)(uint)((int)m_buffer[num - 8] |
                                      (int)m_buffer[num - 7] << 8 |
                                      (int)m_buffer[num - 6] << 16 |
                                      (int)m_buffer[num - 5] << 24);
            return System.Net.IPAddress.NetworkToHostOrder(value);
        }

        /// <summary>
        /// BinaryReader.ReadBytes
        /// 实测 执行10000000次
        ///     StreamHelper.ReadBytes ~= 453ms  节约new BinaryReader
        ///     BinaryReader.ReadBytes ~= 703ms
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static byte[] ReadBytes(ref MemoryStream ms, int count)
        {
            if (ms == null)
            { 
                throw new NullReferenceException("StreamHelper.ReadByte , MemoryStream=null");
            }

            int allLength = (int)ms.Position + count;
            if (allLength > ms.Length)
            {
                throw new IndexOutOfRangeException(
                    $"StreamHelper.ReadByte , 无法从 MemoryStream 读取 , Length:{ms.Length} Position:{ms.Position}");
            }

            {
                // BinaryReader.ReadBytes
                byte[] numArray = new byte[count];
                int length = 0;
                do
                {
                    int num = ms.Read(numArray, length, count);
                    if (num != 0)
                    {
                        length += num;
                        count -= num;
                    }
                    else
                        break;
                } while (count > 0);

                if (length != numArray.Length)
                {
                    byte[] dst = new byte[length];
                    // Buffer.InternalBlockCopy((Array) numArray, 0, (Array) dst, 0, length);
                    Buffer.BlockCopy((Array)numArray, 0, (Array)dst, 0, length);
                    numArray = dst;
                }

                return numArray;
            }
        }
    }
}