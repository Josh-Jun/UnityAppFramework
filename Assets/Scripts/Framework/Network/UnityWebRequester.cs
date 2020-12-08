using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequester
{
    private UnityWebRequest uwr;
    private MonoBehaviour mono;
    public UnityWebRequester(MonoBehaviour mono)
    {
        this.mono = mono;
    }
    /// <summary> 是否下载完 </summary>
    public bool IsDown
    {
        get
        {
            if (uwr != null)
            {
                return uwr.isDone;
            }
            return false;
        }
        private set { }
    }
    /// <summary> 获取当前下载大小(KB) </summary>
    public float DownloadedLength
    {
        get
        {
            if (uwr != null)
            {
                return uwr.downloadedBytes / 1024f;
            }
            return 0;
        }
        private set { }
    }
    /// <summary> 获取下载进度 </summary>
    public float DownloadedProgress
    {
        get
        {
            if (uwr != null)
            {
                return uwr.downloadProgress;
            }
            return 0;
        }
        private set { }
    }

    public void Destory()
    {
        if (uwr != null)
        {
            uwr.Dispose();
            uwr = null;
        }
    }

    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult"></param>
    /// <param name="actionProgress"></param>
    public void Get(string url, Action<UnityWebRequest> actionResult)
    {
        App.app.StartCoroutine(IE_Get(url, actionResult));
    }


    /// <summary>
    /// 请求byte数据
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void GetBytes(string url, Action<UnityWebRequest> actionResult)
    {
        App.app.StartCoroutine(IE_GetBytes(url, actionResult));
    }

    /// <summary>
    /// 请求图片
    /// </summary>
    /// <param name="url">图片地址,like 'http://www.my-server.com/image.png '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的图片</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void GetTexture(string url, Action<Texture2D> actionResult)
    {
        App.app.StartCoroutine(IE_GetTexture(url, actionResult));
    }

    /// <summary>
    /// 请求AssetBundle
    /// </summary>
    /// <param name="url">AssetBundle地址,like 'http://www.my-server.com/myData.unity3d'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AssetBundle</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void GetAssetBundle(string url, Action<AssetBundle> actionResult)
    {
        App.app.StartCoroutine(IE_GetAssetBundle(url, actionResult));
    }

    /// <summary>
    /// 请求服务器地址上的音效
    /// </summary>
    /// <param name="url">没有音效地址,like 'http://myserver.com/mysound.wav'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AudioClip</param>
    /// <param name="actionProgress"></param>
    /// <param name="audioType">音效类型</param>
    /// <returns></returns>
    public void GetAudioClip(string url, Action<AudioClip> actionResult, AudioType audioType = AudioType.WAV)
    {
        App.app.StartCoroutine(IE_GetAudioClip(url, actionResult, audioType));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="lstformData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, List<IMultipartFormSection> lstformData, Action<UnityWebRequest> actionResult)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        App.app.StartCoroutine(IE_Post(url, lstformData, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, WWWForm formData, Action<UnityWebRequest> actionResult)
    {
        App.app.StartCoroutine(IE_Post(url, formData, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formFields">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, Dictionary<string, string> formFields, Action<UnityWebRequest> actionResult)
    {
        App.app.StartCoroutine(IE_Post(url, formFields, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, string postData, Action<UnityWebRequest> actionResult)
    {
        App.app.StartCoroutine(IE_Post(url, postData, actionResult));
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="contentBytes">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Put(string url, byte[] contentBytes, Action<UnityWebRequest> actionResult)
    {
        App.app.StartCoroutine(IE_Put(url, contentBytes, actionResult));
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="content">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Put(string url, string content, Action<UnityWebRequest> actionResult)
    {
        App.app.StartCoroutine(IE_Put(url, content, actionResult));
    }

    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url">请求地址,like 'http://www.my-server.com/ '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Get(string url, Action<UnityWebRequest> actionResult)
    {
        using (uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }

    /// <summary>
    /// 请求byte数据
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_GetBytes(string url, Action<UnityWebRequest> actionResult)
    {
        using (uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }
    /// <summary>
    /// 请求图片
    /// </summary>
    /// <param name="url">图片地址,like 'http://www.my-server.com/image.png '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的图片</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_GetTexture(string url, Action<Texture2D> actionResult)
    {
        using (uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (!uwr.isNetworkError)
            {
                actionResult?.Invoke(DownloadHandlerTexture.GetContent(uwr));
            }
            else
            {
                Debug.LogErrorFormat("[Error:Texture2D] {0}", uwr.error);
            }
        }
    }

    /// <summary>
    /// 请求AssetBundle
    /// </summary>
    /// <param name="url">AssetBundle地址,like 'http://www.my-server.com/myData.unity3d'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AssetBundle</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_GetAssetBundle(string url, Action<AssetBundle> actionResult)
    {
        using (uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return uwr.SendWebRequest();
            if (!uwr.isNetworkError)
            {
                actionResult?.Invoke(DownloadHandlerAssetBundle.GetContent(uwr));
            }
            else
            {
                Debug.LogErrorFormat("[Error:AssetBundle] {0}", uwr.error);
            }
        }
    }

    /// <summary>
    /// 请求服务器地址上的音效
    /// </summary>
    /// <param name="url">没有音频地址,like 'http://myserver.com/mymovie.mp3'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的MovieTexture</param>
    /// <param name="actionProgress"></param>
    /// <param name="audioType">音效类型</param>
    /// <returns></returns>
    public IEnumerator IE_GetAudioClip(string url, Action<AudioClip> actionResult, AudioType audioType = AudioType.WAV)
    {
        using (uwr = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return uwr.SendWebRequest();
            if (!uwr.isNetworkError)
            {
                actionResult?.Invoke(DownloadHandlerAudioClip.GetContent(uwr));
            }
            else
            {
                Debug.LogErrorFormat("[Error:AudioClip] {0}", uwr.error);
            }
        }
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="lstformData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, List<IMultipartFormSection> lstformData, Action<UnityWebRequest> actionResult)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));
        using (uwr = UnityWebRequest.Post(url, lstformData))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, WWWForm formData, Action<UnityWebRequest> actionResult)
    {
        using (uwr = UnityWebRequest.Post(url, formData))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, string postData, Action<UnityWebRequest> actionResult)
    {
        using (uwr = UnityWebRequest.Post(url, postData))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formFields">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, Dictionary<string, string> formFields, Action<UnityWebRequest> actionResult)
    {
        using (uwr = UnityWebRequest.Post(url, formFields))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="contentBytes">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Put(string url, byte[] contentBytes, Action<UnityWebRequest> actionResult)
    {
        using (uwr = UnityWebRequest.Put(url, contentBytes))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }
    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="content">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Put(string url, string content, Action<UnityWebRequest> actionResult)
    {
        using (uwr = UnityWebRequest.Put(url, content))
        {
            yield return uwr.SendWebRequest();
            actionResult?.Invoke(uwr);
        }
    }
}
