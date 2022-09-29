using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
//功能:AES加解密工具
public class AESTools
{
    #region AES带头 加、解密
    private static string AESHead = "AESEncrypt";

    /// <summary>
    /// 文件加密，传入文件路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="EncrptyKey"></param>
    private static void AESFileEncrypt(string path)
    {
        if (!File.Exists(path))
            return;

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (fs != null)
                {
                    //读取字节头，判断是否已经加密过了
                    byte[] headBuff = new byte[10];
                    fs.Read(headBuff, 0, headBuff.Length);
                    string headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
                        Debug.Log(path + "已经加密过了！");
                        return;
                    }
                    //加密并且写入字节头
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    byte[] headBuffer = Encoding.UTF8.GetBytes(AESHead);
                    fs.Write(headBuffer, 0, headBuffer.Length);
                    byte[] EncBuffer = AESEncrypt(buffer);
                    fs.Write(EncBuffer, 0, EncBuffer.Length);
                }
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
    /// <param name="EncrptyKey"></param>
    private static void AESFileDecrypt(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (fs != null)
                {
                    byte[] headBuff = new byte[10];
                    fs.Read(headBuff, 0, headBuff.Length);
                    string headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
                        byte[] buffer = new byte[fs.Length - headBuff.Length];
                        fs.Read(buffer, 0, Convert.ToInt32(fs.Length - headBuff.Length));
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.SetLength(0);
                        byte[] DecBuffer = AESDecrypt(buffer);
                        fs.Write(DecBuffer, 0, DecBuffer.Length);
                    }
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
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fs != null)
                {
                    byte[] headBuff = new byte[10];
                    fs.Read(headBuff, 0, headBuff.Length);
                    string headTag = Encoding.UTF8.GetString(headBuff);
                    if (headTag == AESHead)
                    {
                        byte[] buffer = new byte[fs.Length - headBuff.Length];
                        fs.Read(buffer, 0, Convert.ToInt32(fs.Length - headBuff.Length));
                        DecBuffer = AESDecrypt(buffer);
                    }
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
    private static string encryptKey = "FRAMEWORK";//密钥

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
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                if (fs != null)
                {
                    //加密
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    byte[] EncBuffer = AESEncrypt(buffer);
                    fs.Write(EncBuffer, 0, EncBuffer.Length);
                }
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
    /// <param name="EncryptString">待加密密文</param>
    public static byte[] AESEncrypt(byte[] EncryptByte)
    {
        if (EncryptByte.Length == 0) { throw (new Exception("明文不得为空")); }
        if (string.IsNullOrEmpty(encryptKey)) { throw (new Exception("密钥不得为空")); }
        byte[] m_strEncrypt;
        byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
        byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(encryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(EncryptByte, 0, EncryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strEncrypt = m_stream.ToArray();
            m_stream.Close();
            m_stream.Dispose();
            m_csstream.Close();
            m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
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
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fs != null)
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
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
    /// <param name="DecryptString">待解密密文</param>
    public static byte[] AESDecrypt(byte[] DecryptByte)
    {
        if (DecryptByte.Length == 0) { throw (new Exception("密文不得为空")); }
        if (string.IsNullOrEmpty(encryptKey)) { throw (new Exception("密钥不得为空")); }
        byte[] m_strDecrypt;
        byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
        byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(encryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(DecryptByte, 0, DecryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strDecrypt = m_stream.ToArray();
            m_stream.Close();
            m_stream.Dispose();
            m_csstream.Close();
            m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
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

