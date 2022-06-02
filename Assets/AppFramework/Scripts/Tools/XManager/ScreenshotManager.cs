using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class ScreenshotManager : SingletonMono<ScreenshotManager>
{
    private Coroutine coroutine;
    /// <summary>
    ///  截取屏幕像素，不带特效，自定义大小
    /// </summary>
    public void TakePhoto(string path, Size size, Action<Texture2D, string> callback = null)
    {
        if (coroutine == null)
        {
            coroutine = StartCoroutine(Shot(path, size, callback));
        }
    }
    /// <summary>
    /// 截取某个相机渲染的画面，不带特效，自定义大小
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="size"></param>
    public void TakePhoto(Camera camera, string path, Size size, Action<Texture2D, string> callback = null)
    {
        if (coroutine == null)
        {
            coroutine = StartCoroutine(Shot(camera, path, size, callback));
        }
    }
    /// <summary>
    /// 截屏(截取某个相机渲染的画面)
    /// </summary>
    /// <returns></returns>
    private IEnumerator Shot(Camera camera, string path, Size size, Action<Texture2D, string> callback = null)
    {
        Texture2D texture = CaptureCamera(camera, new Rect(Screen.width / 2 - size.w / 2, Screen.height / 2 - size.h / 2, size.w, size.h));
        yield return new WaitForEndOfFrame();
        var img = texture.EncodeToPNG();
        if (!FileManager.FolderExist(path))//创建生成目录，如果不存在则创建目录  
        {
            FileManager.CreateFolder(path);
        }
        string imageName = string.Format("Image{0}.png", Utils.GetTimeStamp);
        string file = string.Format("{0}/{1}", path, imageName);
        File.WriteAllBytes(file, img);
        yield return new WaitForEndOfFrame();
        coroutine = null;
        callback?.Invoke(texture, imageName);
    }
    /// <summary>
    /// 截屏(截取屏幕像素)
    /// </summary>
    /// <returns></returns>
    private IEnumerator Shot(string path, Size size, Action<Texture2D, string> callback = null)
    {
        Texture2D texture = new Texture2D(size.w, size.h, TextureFormat.RGB24, false);
        yield return new WaitForEndOfFrame();
        texture.ReadPixels(new Rect(Screen.width / 2 - size.w / 2, Screen.height / 2 - size.h / 2, size.w, size.h), 0, 0, false);
        texture.Apply();
        var img = texture.EncodeToPNG();
        if (!FileManager.FolderExist(path))//创建生成目录，如果不存在则创建目录  
        {
            FileManager.CreateFolder(path);
        }
        string imageName = string.Format("Image{0}.png", Utils.GetTimeStamp);
        string file = string.Format("{0}/{1}", path, imageName);
        File.WriteAllBytes(file, img);
        yield return new WaitForEndOfFrame();
        coroutine = null;
        callback?.Invoke(texture, imageName);
    }
    /// <summary>  
    /// 对相机截图。   
    /// </summary>  
    /// <returns>The screenshot2.</returns>  
    /// <param name="camera">Camera.要被截屏的相机</param>  
    /// <param name="rect">Rect.截屏的区域</param>  
    private Texture2D CaptureCamera(Camera camera, Rect rect)
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
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素  
        screenShot.Apply();
        // 重置相关参数，以使用camera继续在屏幕上显示  
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;  
        RenderTexture.active = null; // JC: added to avoid errors  
        GameObject.Destroy(rt);
        return screenShot;
    }
}
/// <summary>
/// 大小
/// </summary>
[Serializable]
public struct Size
{
    public int w;
    public int h;
    public Size(int width, int height)
    {
        w = width;
        h = height;
    }
}