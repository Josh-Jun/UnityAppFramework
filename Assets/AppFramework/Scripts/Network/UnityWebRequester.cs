using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequester
{
    private UnityWebRequest uwr;
    private MonoBehaviour mono;
    private Dictionary<string, string> headerPairs = new Dictionary<string, string>();
    public UnityWebRequester(MonoBehaviour mono)
    {
        this.mono = mono;
        uwr = new UnityWebRequest();
    }
    /// <summary> 是否下载完 </summary>
    public bool IsDone
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
    /// <summary> 获取当前下载大小(b) </summary>
    public long DownloadedLength
    {
        get
        {
            if (uwr != null)
            {
                return (long)uwr.downloadedBytes;
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

    public void AddHeader(string key, string value)
    {
        if (!headerPairs.ContainsKey(key))
        {
            headerPairs.Add(key, value);
        }
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
    /// 文件下载，断点续传
    /// </summary>
    /// <param name="url"></param>
    /// <param name="filePath"></param>
    /// <param name="callBack"></param>
    public void DownloadFile(string url, string filePath, Action<long, float> callBack)
    {
        mono.StartCoroutine(IE_DownloadFile(url, filePath, callBack));
    }
    /// <summary>
    /// 文件下载，断点续传
    /// </summary>
    /// <param name="url"></param>
    /// <param name="filePath"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public IEnumerator IE_DownloadFile(string url, string filePath, Action<long, float> callBack)
    {
        //获取要下载的文件的总大小
        var head = UnityWebRequest.Head(url);
        yield return head.SendWebRequest();
        if (!string.IsNullOrEmpty(head.error))
        {
            callBack?.Invoke(-1, 0);
            Debug.LogError($"[Error:Download Head] {head.error}");
            yield break;
        }
        long totalLength = Convert.ToInt64(head.GetResponseHeader("Content-Length"));
        head.Dispose();
        //创建网络请求
        UnityWebRequest body = UnityWebRequest.Get(url);
        body.downloadHandler = new DownloadHandlerFile(filePath, true);

        FileInfo file = new FileInfo(filePath);
        var fileLength = file.Length;
        //设置开始下载文件从什么位置开始
        body.SetRequestHeader("Range", $"bytes={fileLength}-");//这句很重要
        float progress;//文件下载进度
        if (fileLength < totalLength)
        {
            body.SendWebRequest();
            if (!string.IsNullOrEmpty(body.error))
            {
                Debug.LogError($"[Error:Download Body] {body.error}");
            }
            while (!body.isDone)
            {
                progress = (float)((long)body.downloadedBytes + fileLength) / (float)totalLength;
                callBack?.Invoke(totalLength, progress);
                yield return new WaitForEndOfFrame();
            }
        }
        callBack?.Invoke(totalLength, 1);
        body.Dispose();
    }

    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult"></param>
    /// <param name="actionProgress"></param>
    public void Get(string url, Action<string> actionResult)
    {
        mono.StartCoroutine(IE_Get(url, actionResult));
    }


    /// <summary>
    /// 请求byte数据
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void GetBytes(string url, Action<byte[]> actionResult)
    {
        mono.StartCoroutine(IE_GetBytes(url, actionResult));
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
        mono.StartCoroutine(IE_GetTexture(url, actionResult));
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
        mono.StartCoroutine(IE_GetAssetBundle(url, actionResult));
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
        mono.StartCoroutine(IE_GetAudioClip(url, actionResult, audioType));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="lstformData">IMultipartFormSection表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, List<IMultipartFormSection> lstformData, Action<string> actionResult)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        mono.StartCoroutine(IE_Post(url, lstformData, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, WWWForm formData, Action<string> actionResult)
    {
        mono.StartCoroutine(IE_Post(url, formData, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formFields">字典</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, Dictionary<string, string> formFields, Action<string> actionResult)
    {
        mono.StartCoroutine(IE_Post(url, formFields, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">json字符串</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, string postData, Action<string> actionResult)
    {
        mono.StartCoroutine(IE_Post(url, postData, actionResult));
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">json字符串byte[]</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, byte[] postData, Action<string> actionResult)
    {
        mono.StartCoroutine(IE_Post(url, postData, actionResult));
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="contentBytes">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Put(string url, byte[] contentBytes, Action<string> actionResult)
    {
        mono.StartCoroutine(IE_Put(url, contentBytes, actionResult));
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="content">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Put(string url, string content, Action<string> actionResult)
    {
        mono.StartCoroutine(IE_Put(url, content, actionResult));
    }

    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url">请求地址,like 'http://www.my-server.com/ '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Get(string url, Action<string> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();
            uwr.downloadHandler = downloadHandler;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Get String] {uwr.error}");
            }
        }
    }

    /// <summary>
    /// 请求byte数据
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_GetBytes(string url, Action<byte[]> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            DownloadHandlerBuffer downloadHandlerBuffer = new DownloadHandlerBuffer();
            uwr.downloadHandler = downloadHandlerBuffer;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(downloadHandlerBuffer.data);
            }
            else
            {
                Debug.LogError($"[Error:Bytes] {url} {uwr.error}");
            }
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
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            DownloadHandlerTexture downloadHandlerTexture = new DownloadHandlerTexture();
            uwr.downloadHandler = downloadHandlerTexture;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(downloadHandlerTexture.texture);
            }
            else
            {
                Debug.LogError($"[Error:Texture2D] {uwr.error}");
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
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            DownloadHandlerAssetBundle downloadHandlerAssetBundle  = new DownloadHandlerAssetBundle(uwr.url, 0);
            uwr.downloadHandler = downloadHandlerAssetBundle ;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(downloadHandlerAssetBundle.assetBundle);
            }
            else
            {
                Debug.LogError($"[Error:AssetBundle] {uwr.error}");
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
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            DownloadHandlerAudioClip downloadHandlerAudioClip = new DownloadHandlerAudioClip(uwr.url, audioType);
            uwr.downloadHandler = downloadHandlerAudioClip;
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(downloadHandlerAudioClip.audioClip);
            }
            else
            {
                Debug.LogError($"[Error:AudioClip] {uwr.error}");
            }
        }
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="lstformData">IMultipartFormSection表单参数</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, List<IMultipartFormSection> lstformData, Action<string> actionResult)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            string json = LitJson.JsonMapper.ToJson(lstformData);
            byte[] postData = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(postData);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Post IMultipartFormSection] {uwr.error}");
            }
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
    public IEnumerator IE_Post(string url, WWWForm formData, Action<string> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            uwr.uploadHandler = new UploadHandlerRaw(formData.data);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Post WWWForm] {uwr.error}");
            }
        }
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">json字符串</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, string postData, Action<string> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Post String] {uwr.error}");
            }
        }
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">json字符串byte[]</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, byte[] postData, Action<string> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            uwr.uploadHandler = new UploadHandlerRaw(postData);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Post Bytes] {uwr.error}");
            }
        }
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formFields">字典</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Post(string url, Dictionary<string, string> formFields, Action<string> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            string json = LitJson.JsonMapper.ToJson(formFields);
            byte[] postData = Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(postData);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Post Dictionary] {uwr.error}");
            }
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
    public IEnumerator IE_Put(string url, byte[] contentBytes, Action<string> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPUT;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            uwr.uploadHandler = new UploadHandlerRaw(contentBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Put Bytes] {uwr.error}");
            }
        }
    }
    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="content">需要上传的字符串</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public IEnumerator IE_Put(string url, string content, Action<string> actionResult)
    {
        using (uwr)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPUT;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            uwr.uploadHandler = new UploadHandlerRaw(contentBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                actionResult?.Invoke(uwr.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Error:Put String] {uwr.error}");
            }
        }
    }
}
