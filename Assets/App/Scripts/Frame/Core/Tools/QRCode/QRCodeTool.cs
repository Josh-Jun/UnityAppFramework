/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年6月11 15:17
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ZXing;
using ZXing.QrCode;

namespace App.Core.Tools
{
    public class QRCodeTool
    {
        private static BarcodeReader reader = new BarcodeReader();
        
        public static Texture2D Create(string message, bool autoSave = false)
        {
            var texture = new Texture2D(256, 256);
            var color32 = Encode(message, texture.width, texture.height);
            texture.SetPixels32(color32);
            texture.Apply();
            var bytes = texture.EncodeToPNG();//把二维码转成byte数组，然后进行输出保存为png图片就可以保存下来生成好的二维码  
            if (!autoSave) return texture;
            if (!Directory.Exists(Application.persistentDataPath + "/Cache/QRCode"))//创建生成目录，如果不存在则创建目录  
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Cache/QRCode");
            }
            var fileName = Application.persistentDataPath + "/Cache/QRCode/" + message.GetHashCode() + ".png";
            File.WriteAllBytes(fileName, bytes);
            return texture;
        }

        public static Result Read(Texture2D texture)
        {
            var colorData = texture.GetPixels32();
            return reader.Decode(colorData, texture.width, texture.height);
        }
        
        public static Result Read(Color32[] colorData, int width, int height)
        {
            return reader.Decode(colorData, width, height);
        }
        
        private static Color32[] Encode(string textForEncoding, int width, int height)
        {
            var hints = new Dictionary<EncodeHintType, object> { { EncodeHintType.CHARACTER_SET, "UTF-8" } };
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                },
                Encoder = new QRCodeWriter()
            };
            var bitMatrix = writer.Encoder.encode(textForEncoding, BarcodeFormat.QR_CODE, width, height, hints);
            return writer.Write(bitMatrix);
        }
    }
}