using UnityEngine;
using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace App.Core.Tools
{
    /// <summary> 
    /// 功能:MD5工具 
    /// </summary>
    [Serializable]
    public class MD5Tools
    {
        /// <summary> 根据路径获取文件的MD5 </summary>
        public static string File2Md5(string fliePath)
        {
            string filemd5 = null;
            try
            {
                using (var fileStream = File.OpenRead(fliePath))
                {
                    var md5 = MD5.Create();
                    var fileMD5Bytes =
                        md5.ComputeHash(fileStream); //计算指定Stream 对象的哈希值                                     
                    filemd5 = FormatMD5(fileMD5Bytes);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }

            return filemd5;
        }

        /// <summary> 将byte[]装换成字符串 </summary>
        private static string FormatMD5(Byte[] data)
        {
            return System.BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        /// <summary> 根据字符串生成MD5 </summary>
        public static string Str2MD5(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();
            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }

            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary> 根据字符串生成MD5(加密) </summary>
        public static string EncryptStr2MD5(string con)
        {
            byte[] sor = Encoding.UTF8.GetBytes(con);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strbul = new StringBuilder(40);
            for (int i = 0; i < result.Length; i++)
            {
                strbul.Append(result[i].ToString("x2")); //加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
            }

            return strbul.ToString();
        }
    }
}
