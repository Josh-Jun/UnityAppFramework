using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AppFrame.Tools
{
    public class UnityWebRequester
    {
        private UnityWebRequest uwr;
        private Dictionary<string, string> headerPairs = new Dictionary<string, string>();
        private UnityWebRequestAsyncOperation uwrao;

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
        public void DownloadFile(string url, string filePath, Action<long, long> callBack)
        {
            //获取要下载的文件的总大小
            var head = UnityWebRequest.Head(url);
            UnityWebRequestAsyncOperation ao = head.SendWebRequest();
            if (!string.IsNullOrEmpty(head.error))
            {
                callBack?.Invoke(-1, 0);
                Debug.LogError($"[Error:Download Head] {head.error}");
                return;
            }

            ao.completed += (async) =>
            {
                long totalLength = Convert.ToInt64(head.GetResponseHeader("Content-Length"));
                head.Dispose();
                //创建网络请求
                uwr.url = url;
                uwr.method = UnityWebRequest.kHttpVerbGET;
                uwr.downloadHandler = new DownloadHandlerFile(filePath, true);

                FileInfo file = new FileInfo(filePath);
                var fileLength = file.Length;
                //设置开始下载文件从什么位置开始
                uwr.SetRequestHeader("Range", $"bytes={fileLength}-"); //这句很重要
                if (fileLength < totalLength)
                {
                    uwr.SendWebRequest();
                    if (!string.IsNullOrEmpty(uwr.error))
                    {
                        Debug.LogError($"[Error:Download Body] {uwr.error}");
                    }

                    callBack?.Invoke(totalLength, fileLength);
                    return;
                }

                callBack?.Invoke(totalLength, fileLength);
            };
        }

        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url">请求地址,like 'http://www.my-server.com/ '</param>
        /// <param name="actionResult">请求发起后处理回调结果的委托</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void Get(string url, Action<string> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[Error:Get String] {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 请求byte数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void GetBytes(string url, Action<byte[]> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.data);
                }
                else
                {
                    Debug.LogError($"[Error:Bytes] {url} {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 请求图片
        /// </summary>
        /// <param name="url">图片地址,like 'http://www.my-server.com/image.png '</param>
        /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的图片</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void GetTexture(string url, Action<Texture2D> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            DownloadHandlerTexture downloadHandlerTexture = new DownloadHandlerTexture();
            uwr.downloadHandler = downloadHandlerTexture;
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(downloadHandlerTexture.texture);
                }
                else
                {
                    Debug.LogError($"[Error:Texture2D] {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 请求AssetBundle
        /// </summary>
        /// <param name="url">AssetBundle地址,like 'http://www.my-server.com/myData.unity3d'</param>
        /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AssetBundle</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void GetAssetBundle(string url, Action<AssetBundle> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            DownloadHandlerAssetBundle downloadHandlerAssetBundle = new DownloadHandlerAssetBundle(uwr.url, 0);
            uwr.downloadHandler = downloadHandlerAssetBundle;
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(downloadHandlerAssetBundle.assetBundle);
                }
                else
                {
                    Debug.LogError($"[Error:AssetBundle] {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 请求服务器地址上的音效
        /// </summary>
        /// <param name="url">没有音频地址,like 'http://myserver.com/mymovie.mp3'</param>
        /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的MovieTexture</param>
        /// <param name="actionProgress"></param>
        /// <param name="audioType">音效类型</param>
        /// <returns></returns>
        public void GetAudioClip(string url, Action<AudioClip> callback, AudioType audioType = AudioType.MPEG)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbGET;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            DownloadHandlerAudioClip downloadHandlerAudioClip = new DownloadHandlerAudioClip(uwr.url, audioType);
            uwr.downloadHandler = downloadHandlerAudioClip;
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(downloadHandlerAudioClip.audioClip);
                }
                else
                {
                    Debug.LogError($"[Error:AudioClip] {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 向服务器提交post请求
        /// </summary>
        /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
        /// <param name="formData">form表单参数</param>
        /// <param name="actionResult">处理返回结果的委托</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void Post(string url, WWWForm formData, Action<string> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            uwr.uploadHandler = new UploadHandlerRaw(formData.data);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[Error:Post WWWForm] {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 向服务器提交post请求
        /// </summary>
        /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
        /// <param name="postData">json字符串</param>
        /// <param name="actionResult">处理返回结果的委托</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void Post(string url, string postData, Action<string> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            uwr.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            uwr.uploadHandler = new UploadHandlerRaw(bodyRaw);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[Error:Post String] {uwr.error}");
                }
            };
        }
        /// <summary>
        /// 向服务器提交post请求
        /// </summary>
        /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
        /// <param name="formFields">字典</param>
        /// <param name="actionResult">处理返回结果的委托</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void Post(string url, Dictionary<string, string> formFields, Action<string> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPOST;
            uwr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            WWWForm form = new WWWForm();
            foreach (var item in formFields)
            {
                form.AddField(item.Key, item.Value);
            }

            uwr.uploadHandler = new UploadHandlerRaw(form.data);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[Error:Post Dictionary] {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 通过PUT方式将字节流传到服务器
        /// </summary>
        /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
        /// <param name="contentBytes">需要上传的字节流</param>
        /// <param name="actionResult">处理返回结果的委托</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void Put(string url, byte[] contentBytes, Action<string> callback)
        {
            uwr.url = url;
            uwr.method = UnityWebRequest.kHttpVerbPUT;
            foreach (var header in headerPairs)
            {
                uwr.SetRequestHeader(header.Key, header.Value);
            }

            uwr.uploadHandler = new UploadHandlerRaw(contentBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[Error:Put Bytes] {uwr.error}");
                }
            };
        }

        /// <summary>
        /// 通过PUT方式将字节流传到服务器
        /// </summary>
        /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
        /// <param name="content">需要上传的字符串</param>
        /// <param name="actionResult">处理返回结果的委托</param>
        /// <param name="actionProgress"></param>
        /// <returns></returns>
        public void Put(string url, string content, Action<string> callback)
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
            uwrao = uwr.SendWebRequest();
            uwrao.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(uwr.error))
                {
                    callback?.Invoke(uwr.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[Error:Put String] {uwr.error}");
                }
            };
        }
    }
}