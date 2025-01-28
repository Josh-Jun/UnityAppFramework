using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace App.Core.Tools
{
    /// <summary>
    /// 功能:AES加解密工具
    /// </summary>
    public class AESTools
    {
        #region AES带头 加、解密

        private static readonly string AESHead = "AESEncrypt";

        /// <summary>
        /// 文件加密，传入文件路径
        /// </summary>
        /// <param name="path"></param>
        private static void AESFileEncrypt(string path)
        {
            if (!File.Exists(path))
                return;

            try
            {
                using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                
                {
                    //读取字节头，判断是否已经加密过了
                    var headBuff = new byte[10];
                    var read = fs.Read(headBuff, 0, headBuff.Length);
                    var headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
                        Log.I(path + "已经加密过了！");
                        return;
                    }

                    //加密并且写入字节头
                    fs.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[fs.Length];
                    var i = fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    var headBuffer = Encoding.UTF8.GetBytes(AESHead);
                    fs.Write(headBuffer, 0, headBuffer.Length);
                    var EncBuffer = AESEncrypt(buffer);
                    fs.Write(EncBuffer, 0, EncBuffer.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 文件解密，传入文件路径（会改动加密文件，不适合运行时）
        /// </summary>
        /// <param name="path"></param>
        private static void AESFileDecrypt(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                
                {
                    var headBuff = new byte[10];
                    var read = fs.Read(headBuff, 0, headBuff.Length);
                    var headTag = Encoding.UTF8.GetString(headBuff);
                    var buffer = new byte[fs.Length - headBuff.Length];
                    if (headTag == AESHead)
                    {
                        var i = fs.Read(buffer, 0, Convert.ToInt32((int)(fs.Length - headBuff.Length)));
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.SetLength(0);
                        var DecBuffer = AESDecrypt(buffer);
                        fs.Write(DecBuffer, 0, DecBuffer.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// 文件界面，传入文件路径，返回字节
        /// </summary>
        /// <returns></returns>
        private static byte[] AESFileByteDecrypt(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] DecBuffer = null;
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                
                {
                    var headBuff = new byte[10];
                    var read = fs.Read(headBuff, 0, headBuff.Length);
                    var headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
                        var buffer = new byte[fs.Length - headBuff.Length];
                        var i = fs.Read(buffer, 0, Convert.ToInt32((int)(fs.Length - headBuff.Length)));
                        DecBuffer = AESDecrypt(buffer);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return DecBuffer;
        }

        #endregion

        #region AES加、解密

        private static readonly string encryptKey = "FRAMEWORK"; //密钥

        /// <summary>
        /// 根据路径加密
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static void AESEncryptByPath(string path)
        {
            if (!File.Exists(path))
                return;
            try
            {
                using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                
                {
                    //加密
                    var buffer = new byte[fs.Length];
                    var read = fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    var EncBuffer = AESEncrypt(buffer);
                    fs.Write(EncBuffer, 0, EncBuffer.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        /// <summary>
        /// 字符串加密  返回加密后的字符串
        /// </summary>
        /// <param name="EncryptString"></param>
        /// <returns></returns>
        public static string AESEncryptToString(string EncryptString)
        {
            return Convert.ToBase64String(AESEncrypt(Encoding.Default.GetBytes(EncryptString)));
        }

        /// <summary>
        /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="EncryptByte">待加密密文</param>
        public static byte[] AESEncrypt(byte[] EncryptByte)
        {
            if (EncryptByte.Length == 0)
            {
                throw (new Exception("明文不得为空"));
            }

            if (string.IsNullOrEmpty(encryptKey))
            {
                throw (new Exception("密钥不得为空"));
            }

            var m_strEncrypt = new byte[] { };
            var m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            var m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
            var m_AESProvider = Rijndael.Create();
            try
            {
                var m_stream = new MemoryStream();
                var pdb = new PasswordDeriveBytes(encryptKey, m_salt);
                var transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
                var m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                m_csstream.Write(EncryptByte, 0, EncryptByte.Length);
                m_csstream.FlushFinalBlock();
                m_strEncrypt = m_stream.ToArray();
                m_stream.Close();
                m_stream.Dispose();
                m_csstream.Close();
                m_csstream.Dispose();
            }
            catch (IOException ex)
            {
                Debug.LogError(ex);
            }
            catch (CryptographicException ex)
            {
                Debug.LogError(ex);
            }
            catch (ArgumentException ex)
            {
                Debug.LogError(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                m_AESProvider.Clear();
            }

            return m_strEncrypt;
        }



        /// <summary>
        ///  根据路径解密 返回解密后的Byte
        /// </summary>
        /// <returns></returns>
        public static byte[] AESDecryptToByte(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] DecBuffer = null;
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                
                {
                    var buffer = new byte[fs.Length];
                    var read = fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    DecBuffer = AESDecrypt(buffer);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return DecBuffer;
        }

        /// <summary>
        /// 字符串解密  返回解密后的字符串
        /// </summary>
        /// <param name="DecryptString"></param>
        /// <returns></returns>
        public static string AESDecryptToString(string DecryptString)
        {
            return Convert.ToBase64String(AESDecrypt(Encoding.Default.GetBytes(DecryptString)));
        }

        /// <summary>
        /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
        /// </summary>
        /// <param name="DecryptByte">待解密密文</param>
        public static byte[] AESDecrypt(byte[] DecryptByte)
        {
            if (DecryptByte.Length == 0)
            {
                throw (new Exception("密文不得为空"));
            }

            if (string.IsNullOrEmpty(encryptKey))
            {
                throw (new Exception("密钥不得为空"));
            }

            var m_strDecrypt = new byte[] { };
            var m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            var m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
            var m_AESProvider = Rijndael.Create();
            try
            {
                var m_stream = new MemoryStream();
                var pdb = new PasswordDeriveBytes(encryptKey, m_salt);
                var transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
                var m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
                m_csstream.Write(DecryptByte, 0, DecryptByte.Length);
                m_csstream.FlushFinalBlock();
                m_strDecrypt = m_stream.ToArray();
                m_stream.Close();
                m_stream.Dispose();
                m_csstream.Close();
                m_csstream.Dispose();
            }
            catch (IOException ex)
            {
                Debug.LogError(ex);
            }
            catch (CryptographicException ex)
            {
                Debug.LogError(ex);
            }
            catch (ArgumentException ex)
            {
                Debug.LogError(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                m_AESProvider.Clear();
            }

            return m_strDecrypt;
        }

        #endregion

        #region 异步解密

        public class AsyncData
        {
            public AESDecryptAsyncDelegate decryptAsyn;
            public Action<byte[]> cb;
        }

        //异步解密
        public delegate byte[] AESDecryptAsyncDelegate(byte[] bytes);

        //异步解密
        public static void AESDecryptAsync(byte[] bytes, Action<byte[]> cb)
        {
            AESDecryptAsyncDelegate decryptAsync = new AESDecryptAsyncDelegate(AESDecrypt);
            AsyncData asyncData = new AsyncData
            {
                decryptAsyn = decryptAsync,
                cb = cb,
            };
            decryptAsync.BeginInvoke(bytes, new AsyncCallback(GetABBytes), asyncData);
        }

        //异步获取解密后的字节数据
        public static void GetABBytes(IAsyncResult result)
        {
            AsyncData asyncData = (AsyncData)result.AsyncState;
            byte[] data = asyncData.decryptAsyn.EndInvoke(result);
            asyncData.cb?.Invoke(data);
        }

        #endregion
    }
}

