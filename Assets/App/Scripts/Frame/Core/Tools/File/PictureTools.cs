using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace App.Core.Tools
{
    public enum PictureType
    {
        Jpg,
        Png,
        Exr,
        Tga,
    }
    /// <summary> 图片工具类 </summary>
    public static class PictureTools
    {
        /// <summary> 创建Sprite</summary>
        public static Texture2D CreateTexture(byte[] bytes, int width, int height)
        {
            //设置头像   
            var tex2d = new Texture2D(width, height);
            tex2d.LoadImage(bytes);
            return tex2d;
        }

        /// <summary> 创建Sprite</summary>
        public static Sprite CreateSprite(byte[] bytes, int width, int height)
        {
            //设置头像   
            var tex2d = CreateTexture(bytes, width, height);
            var sprite =
                Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height),
                    new Vector2(0.5f, 0.5f)); //后面Vector2就是你Anchors的Pivot的x/y属性值
            return sprite;
        }
        
        /// <summary> 创建Sprite</summary>
        public static Sprite CreateSprite(Texture2D texture)
        {
            //设置头像  
            var sprite =
                Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)); //后面Vector2就是你Anchors的Pivot的x/y属性值
            return sprite;
        }

        public static byte[] CreateByte(Sprite sp, PictureType type = PictureType.Jpg)
        {
            return CreateByte(sp.texture, type);
        }

        public static byte[] CreateByte(Texture2D texture, PictureType type = PictureType.Jpg)
        {
            //根据不同图片格式转换成bytes,如果不根据格式转，jpg转png时会出现byte比原始数据大的情况。
            byte[] textureByte = null;
            switch (type)
            {
                case PictureType.Jpg:
                    textureByte = texture.EncodeToJPG();
                    break;
                case PictureType.Png:
                    textureByte = texture.EncodeToPNG();
                    break;
                case PictureType.Exr:
                    textureByte = texture.EncodeToEXR();
                    break;
                case PictureType.Tga:
                    textureByte = texture.EncodeToTGA();
                    break;
            }
            return textureByte;
        }

        /// <summary>
        /// 图片自适应
        /// </summary>
        /// <param name="limitRange"></param>
        /// <param name="textureSize"></param>
        /// <returns></returns>
        public static Vector2 AdaptSize(Vector2 limitRange, Vector2 textureSize)
        {
            var size = textureSize;
            var standard_ratio = limitRange.x / limitRange.y;
            var ratio = size.x / size.y;
            if (ratio > standard_ratio)
            {
                //宽于标准宽度，宽固定
                var scale = size.x / limitRange.x;
                size.x = limitRange.x;
                size.y /= scale;
            }
            else
            {
                //高于标准宽度，高固定
                var scale = size.y / limitRange.y;
                size.y = limitRange.y;
                size.x /= scale;
            }

            return size;
        }

        /// <summary>
        /// 相机截图
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <param name="callback"></param>
        public static void TakePhoto(Camera camera, string path, Action<Texture2D, string> callback = null)
        {
            var filePath = $"{path}/Image_{DateTime.Now.ToTimeStamp()}.png";
            var item = TakeTransparentCapture(camera, camera.pixelWidth, camera.pixelHeight, filePath);
            callback?.Invoke(item.Item1, filePath);
        }
        
        public static (Texture2D, byte[]) TakeTransparentCapture(Camera camera, int width, int height, string savePath)
        {
            // Depending on your render pipeline, this may not work.
            var bak_cam_targetTexture = camera.targetTexture;
            var bak_cam_clearFlags = camera.clearFlags;
            var bak_RenderTexture_active = RenderTexture.active;

            var tex_transparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
            // Must use 24-bit depth buffer to be able to fill background.
            var render_texture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var grab_area = new Rect(0, 0, width, height);

            RenderTexture.active = render_texture;
            camera.targetTexture = render_texture;
            camera.clearFlags = CameraClearFlags.SolidColor;

            // Simple: use a clear background
            camera.backgroundColor = Color.clear;
            camera.Render();
            tex_transparent.ReadPixels(grab_area, 0, 0);
            tex_transparent.Apply();

            // Encode the resulting output texture to a byte array then write to the file
            var pngShot = tex_transparent.EncodeToPNG();
            
            var array = savePath.Split('/');
            var directory = savePath.Replace(array[^1], "");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(savePath, pngShot);

            camera.clearFlags = bak_cam_clearFlags;
            camera.targetTexture = bak_cam_targetTexture;
            RenderTexture.active = bak_RenderTexture_active;
            RenderTexture.ReleaseTemporary(render_texture);
            return (tex_transparent, pngShot);
        }
    }
}
