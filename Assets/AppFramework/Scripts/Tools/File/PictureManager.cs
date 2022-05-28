using UnityEngine;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System;
using System.Runtime.InteropServices;
/// <summary>
/// 图片工具类
/// </summary>
public class PictureManager : Singleton<PictureManager>
{
    /// <summary> 创建Sprite</summary>
    public Texture2D CreateTexture(byte[] bytes, int width, int height)
    {
        //设置头像   
        Texture2D tex2d = new Texture2D(width, height);
        tex2d.LoadImage(bytes);
        return tex2d;
    }
    /// <summary> 创建Sprite</summary>
    public Sprite CreateSprite(byte[] bytes, int width, int height)
    {
        //设置头像   
        Texture2D tex2d = CreateTexture(bytes, width, height);
        Sprite sprite = Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height), new Vector2(0.5f, 0.5f));//后面Vector2就是你Anchors的Pivot的x/y属性值
        return sprite;
    }
    public byte[] CreateByte(Sprite sp)
    {
        //转换成Texture
        Texture2D temp = sp.texture;
        //在转换成bytes
        byte[] textureByte = temp.EncodeToPNG();
        return textureByte;
    }
    public byte[] CreateByte(Texture2D texture)
    {
        //在转换成bytes
        byte[] textureByte = texture.EncodeToPNG();
        return textureByte;
    }
    public List<Texture2D> Gif2Texture2Ds(string path)
    {
        List<Texture2D> list = new List<Texture2D>();
        Image image = Image.FromFile(path);
        if (image != null)
        {
            FrameDimension fd = new FrameDimension(image.FrameDimensionsList[0]);
            int frameCount = image.GetFrameCount(fd);
            for (int i = 0; i < frameCount; i++)
            {
                image.SelectActiveFrame(fd, i);
                Bitmap bitmap = new Bitmap(image.Width, image.Height);
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.DrawImage(image, Point.Empty);
                }
                Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.ARGB32, true);
                // 申请目标位图的变量，并将其内存区域锁定
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                // 获取bmpData的内存起始位置
                IntPtr intPtr = bitmapData.Scan0;
                byte[] bytes = new byte[bitmap.Width * bitmap.Height];//原始数据
                                                                      // 将数据复制到byte数组中，
                Marshal.Copy(intPtr, bytes, 0, bitmap.Width * bitmap.Height);
                //解锁内存区域  
                bitmap.UnlockBits(bitmapData);
                texture.LoadImage(bytes);
                list.Add(texture);
            }
        }
        return list;
    }
    public List<Sprite> Gif2Sprites(string path)
    {
        List<Sprite> list = new List<Sprite>();
        Image image = Image.FromFile(path);
        if (image != null)
        {
            FrameDimension fd = new FrameDimension(image.FrameDimensionsList[0]);
            int frameCount = image.GetFrameCount(fd);
            for (int i = 0; i < frameCount; i++)
            {
                image.SelectActiveFrame(fd, i);
                Bitmap bitmap = new Bitmap(image.Width, image.Height);
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    graphics.DrawImage(image, Point.Empty);
                }
                Texture2D texture = new Texture2D(bitmap.Width, bitmap.Height, TextureFormat.ARGB32, true);
                // 申请目标位图的变量，并将其内存区域锁定
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                // 获取bmpData的内存起始位置
                IntPtr intPtr = bitmapData.Scan0;
                byte[] bytes = new byte[bitmap.Width * bitmap.Height];//原始数据
                                                                      // 将数据复制到byte数组中，
                Marshal.Copy(intPtr, bytes, 0, bitmap.Width * bitmap.Height);
                //解锁内存区域  
                bitmap.UnlockBits(bitmapData);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));//后面Vector2就是你Anchors的Pivot的x/y属性值
                list.Add(sprite);
            }
        }
        return list;
    }
    /// <summary>
    /// 图片自适应
    /// </summary>
    /// <param name="limitRange"></param>
    /// <param name="textureSize"></param>
    /// <returns></returns>
    public Vector2 AdaptSize(Vector2 limitRange, Vector2 textureSize)
    {
        Vector2 size = textureSize;
        float standard_ratio = limitRange.x / limitRange.y;
        float ratio = size.x / size.y;
        if (ratio > standard_ratio)
        {
            //宽于标准宽度，宽固定
            float scale = size.x / limitRange.x;
            size.x = limitRange.x;
            size.y /= scale;
        }
        else
        {
            //高于标准宽度，高固定
            float scale = size.y / limitRange.y;
            size.y = limitRange.y;
            size.x /= scale;
        }
        return size;
    }
}
