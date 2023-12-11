using System;
using System.IO;
using UnityEngine;
/// <summary>
/// 图片工具类
/// </summary>
namespace AppFrame.Tools
{
    public enum PictureType
    {
        Jpg,
        Png,
        Exr,
        Tga,
    }
    public static class PictureTools
    {
        /// <summary> 创建Sprite</summary>
        public static Texture2D CreateTexture(byte[] bytes, int width, int height)
        {
            //设置头像   
            Texture2D tex2d = new Texture2D(width, height);
            tex2d.LoadImage(bytes);
            return tex2d;
        }

        /// <summary> 创建Sprite</summary>
        public static Sprite CreateSprite(byte[] bytes, int width, int height)
        {
            //设置头像   
            Texture2D tex2d = CreateTexture(bytes, width, height);
            Sprite sprite =
                Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height),
                    new Vector2(0.5f, 0.5f)); //后面Vector2就是你Anchors的Pivot的x/y属性值
            return sprite;
        }
        
        /// <summary> 创建Sprite</summary>
        public static Sprite CreateSprite(Texture2D texture)
        {
            //设置头像  
            Sprite sprite =
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

        /// <summary>
        /// 屏幕截图
        /// </summary>
        /// <param name="path"></param>
        /// <param name="size"></param>
        /// <param name="callback"></param>
        public static void TakePhoto(string path, Vector2 size, Action<Texture2D, string> callback = null)
        {
            Texture2D texture = new Texture2D((int)size.x, (int)size.y, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0, false);
            texture.Apply();
            var img = texture.EncodeToPNG();
            string imageName = string.Format("Image{0}.png", Utils.GetTimeStamp);
            string file = string.Format("{0}/{1}", path, imageName);
            if (!FileTools.FolderExist(path)) //创建生成目录，如果不存在则创建目录  
            {
                FileTools.CreateFolder(path);
            }

            FileTools.CreateFile(file, img);
            callback?.Invoke(texture, imageName);
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
            Texture2D texture = CaptureCamera(camera,
                new Rect(Screen.width / 2 - camera.pixelWidth / 2, Screen.height / 2 - camera.pixelHeight / 2,
                    camera.pixelWidth, camera.pixelHeight));
            var img = texture.EncodeToPNG();
            string imageName = string.Format("Image{0}.png", Utils.GetTimeStamp);
            string file = string.Format("{0}/{1}", path, imageName);
            if (!FileTools.FolderExist(path)) //创建生成目录，如果不存在则创建目录  
            {
                FileTools.CreateFolder(path);
            }

            FileTools.CreateFile(file, img);
            callback?.Invoke(texture, imageName);
        }

        /// <summary>  
        /// 对相机截图。   
        /// </summary>  
        /// <returns>The screenshot2.</returns>  
        /// <param name="camera">Camera.要被截屏的相机</param>  
        /// <param name="rect">Rect.截屏的区域</param>  
        private static Texture2D CaptureCamera(Camera camera, Rect rect)
        {
            // 创建一个RenderTexture对象  
            RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0);
            // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机  
            camera.targetTexture = rt;
            camera.Render();
            //ps: --- 如果这样加上第二个相机，可以实现只截图某几个指定的相机一起看到的图像。  
            //ps: camera2.targetTexture = rt;  
            //ps: camera2.Render();  
            //ps: -------------------------------------------------------------------  
            // 激活这个rt, 并从中中读取像素。  
            RenderTexture.active = rt;
            Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(rect, 0, 0); // 注：这个时候，它是从RenderTexture.active中读取像素  
            screenShot.Apply();
            // 重置相关参数，以使用camera继续在屏幕上显示  
            camera.targetTexture = null;
            //ps: camera2.targetTexture = null;  
            RenderTexture.active = null; // JC: added to avoid errors  
            GameObject.Destroy(rt);
            return screenShot;
        }
    }
}
