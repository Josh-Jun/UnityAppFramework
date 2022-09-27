using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class UnityWebRequester
{
    private UnityWebRequest uwr;
    private Dictionary<string, string> headerPairs = new Dictionary<string, string>();
    public UnityWebRequester()
    {
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
    /// <returns></returns>
    public async void DownloadFile(string url, string filePath, Action<long, float> callBack)
    {
        //获取要下载的文件的总大小
        var head = UnityWebRequest.Head(url);
        await head.SendWebRequest();
        if (!string.IsNullOrEmpty(head.error))
        {
            callBack?.Invoke(-1, 0);
            Debug.LogError($"[Error:Download Head] {head.error}");
            return;
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
                await Task.Yield();
            }
        }
        callBack?.Invoke(totalLength, 1);
        body.Dispose();
    }
    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url">请求地址,like 'http://www.my-server.com/ '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    private async void Get(string url, Action<string> callback)
    {
        Task<string> task = Get(url);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// 请求byte数据
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void GetBytes(string url, Action<byte[]> callback)
    {
        Task<byte[]> task = GetBytes(url);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// 请求图片
    /// </summary>
    /// <param name="url">图片地址,like 'http://www.my-server.com/image.png '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的图片</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void GetTexture(string url, Action<Texture2D> callback)
    {
        Task<Texture2D> task = GetTexture(url);
        await task;
        callback?.Invoke(task.Result);
    }

    /// <summary>
    /// 请求AssetBundle
    /// </summary>
    /// <param name="url">AssetBundle地址,like 'http://www.my-server.com/myData.unity3d'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AssetBundle</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void GetAssetBundle(string url, Action<AssetBundle> callback)
    {
        Task<AssetBundle> task = GetAssetBundle(url);
        await task;
        callback?.Invoke(task.Result);
    }

    /// <summary>
    /// 请求服务器地址上的音效
    /// </summary>
    /// <param name="url">没有音频地址,like 'http://myserver.com/mymovie.mp3'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的MovieTexture</param>
    /// <param name="actionProgress"></param>
    /// <param name="audioType">音效类型</param>
    /// <returns></returns>
    public async void GetAudioClip(string url, Action<AudioClip> callback, AudioType audioType = AudioType.WAV)
    {
        Task<AudioClip> task = GetAudioClip(url, audioType);
        await task;
        callback?.Invoke(task.Result);
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="lstformData">IMultipartFormSection表单参数</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void Post(string url, List<IMultipartFormSection> lstformData, Action<string> callback)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));
        Task<string> task = Post(url, lstformData);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void Post(string url, WWWForm formData, Action<string> callback)
    {
        Task<string> task = Post(url, formData);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">json字符串</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void Post(string url, string postData, Action<string> callback)
    {
        Task<string> task = Post(url, postData);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">json字符串byte[]</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void Post(string url, byte[] postData, Action<string> callback)
    {
        Task<string> task = Post(url, postData);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formFields">字典</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void Post(string url, Dictionary<string, string> formFields, Action<string> callback)
    {
        Task<string> task = Post(url, formFields);
        await task;
        callback?.Invoke(task.Result);
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="contentBytes">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void Put(string url, byte[] contentBytes, Action<string> callback)
    {
        Task<string> task = Put(url, contentBytes);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="content">需要上传的字符串</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async void Put(string url, string content, Action<string> callback)
    {
        Task<string> task = Put(url, content);
        await task;
        callback?.Invoke(task.Result);
    }
    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url">请求地址,like 'http://www.my-server.com/ '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    private async Task<string> Get(string url)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Get String] {uwr.error}");
            }
            return downloadHandler.text;
        }
    }
    /// <summary>
    /// 请求byte数据
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async Task<byte[]> GetBytes(string url)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Bytes] {url} {uwr.error}");
            }
            return downloadHandlerBuffer.data;
        }
    }
    /// <summary>
    /// 请求图片
    /// </summary>
    /// <param name="url">图片地址,like 'http://www.my-server.com/image.png '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的图片</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async Task<Texture2D> GetTexture(string url)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Texture2D] {uwr.error}");
            }
            return downloadHandlerTexture.texture;
        }
    }

    /// <summary>
    /// 请求AssetBundle
    /// </summary>
    /// <param name="url">AssetBundle地址,like 'http://www.my-server.com/myData.unity3d'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AssetBundle</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public async Task<AssetBundle> GetAssetBundle(string url)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:AssetBundle] {uwr.error}");
            }
            return downloadHandlerAssetBundle.assetBundle;
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
    public async Task<AudioClip> GetAudioClip(string url, AudioType audioType = AudioType.WAV)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:AudioClip] {uwr.error}");
            }
            return downloadHandlerAudioClip.audioClip;
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
    public async Task<string> Post(string url, List<IMultipartFormSection> lstformData)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Post IMultipartFormSection] {uwr.error}");
            }
            return uwr.downloadHandler.text;
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
    public async Task<string> Post(string url, WWWForm formData)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Post WWWForm] {uwr.error}");
            }
            return uwr.downloadHandler.text;
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
    public async Task<string> Post(string url, string postData)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Post String] {uwr.error}");
            }
            return uwr.downloadHandler.text;
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
    public async Task<string> Post(string url, byte[] postData)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Post Bytes] {uwr.error}");
            }
            return uwr.downloadHandler.text;
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
    public async Task<string> Post(string url, Dictionary<string, string> formFields)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Post Dictionary] {uwr.error}");
            }
            return uwr.downloadHandler.text;
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
    public async Task<string> Put(string url, byte[] contentBytes)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Put Bytes] {uwr.error}");
            }
            return uwr.downloadHandler.text;
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
    public async Task<string> Put(string url, string content)
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
            await uwr.SendWebRequest();
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Error:Put String] {uwr.error}");
            }
            return uwr.downloadHandler.text;
        }
    }
}
