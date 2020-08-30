using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public partial class NetcomManager : Singleton<NetcomManager>
{
    public static string ServerUrl
    {
        private set { }
        get
        {
            if (PlatformManager.Instance.IsEditor())
            {
                return string.Format("{0}{1}/", Application.dataPath.Replace("Assets", ""), "AssetBundle");
            }
            else
            {
                return "https://www.shijunzh.com/wp-content/uploads/2019/";//服务器地址
            }
        }
    }
    public static string Token = "1837bc8456fe33e2df9a4fe17cf7ffb7cd392b0f63d77a9c";
    public static string Identifier = "10";
    public static string id = "e671d95cb4fb445e88c5e7540ad13177";
    public static int Port = 17888; //通信端口

    private string MakeUrl(string url, params object[] args)
    {
        StringBuilder sb = new StringBuilder(url);
        for (int i = 0; i < args.Length; i++)
        {
            sb.AppendFormat("/{0}", args[i]);
        }
        Debug.Log("URL : " + sb.ToString());
        return sb.ToString();
    }
    private string MakeUrlWithToken(string url, params object[] args)
    {
        StringBuilder sb = new StringBuilder(url);
        for (int i = 0; i < args.Length; i++)
        {
            sb.AppendFormat("/{0}", args[i]);
        }
        sb.AppendFormat("?token={0}&identifier={1}", Token, Identifier);
        Debug.Log("URL : " + sb.ToString());
        return sb.ToString();
    }

    #region UnityWebRequest
    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult"></param>
    /// <param name="actionProgress"></param>
    public void Get(string url, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_Get(url, actionResult));
    }


    /// <summary>
    /// 请求byte数据
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void GetBytes(string url, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_GetBytes(url, actionResult));
    }

    /// <summary>
    /// 请求图片
    /// </summary>
    /// <param name="url">图片地址,like 'http://www.my-server.com/image.png '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的图片</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void GetTexture(string url, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_GetTexture(url, actionResult));
    }

    /// <summary>
    /// 请求AssetBundle
    /// </summary>
    /// <param name="url">AssetBundle地址,like 'http://www.my-server.com/myData.unity3d'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AssetBundle</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void GetAssetBundle(string url, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_GetAssetBundle(url, actionResult));
    }

    /// <summary>
    /// 请求服务器地址上的音效
    /// </summary>
    /// <param name="url">没有音效地址,like 'http://myserver.com/mysound.wav'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AudioClip</param>
    /// <param name="actionProgress"></param>
    /// <param name="audioType">音效类型</param>
    /// <returns></returns>
    public void GetAudioClip(string url, Action<NetcomData> actionResult, AudioType audioType = AudioType.WAV)
    {
        App.app.StartCoroutine(_GetAudioClip(url, actionResult, audioType));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="lstformData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, List<IMultipartFormSection> lstformData, Action<NetcomData> actionResult)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

        App.app.StartCoroutine(_Post(url, lstformData, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, WWWForm formData, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_Post(url, formData, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="formFields">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, Dictionary<string, string> formFields, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_Post(url, formFields, actionResult));
    }

    /// <summary>
    /// 向服务器提交post请求
    /// </summary>
    /// <param name="url">服务器请求目标地址,like "http://www.my-server.com/myform"</param>
    /// <param name="postData">form表单参数</param>
    /// <param name="actionResult">处理返回结果的委托,处理请求对象</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Post(string url, string postData, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_Post(url, postData, actionResult));
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="contentBytes">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Put(string url, byte[] contentBytes, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_Put(url, contentBytes, actionResult));
    }

    /// <summary>
    /// 通过PUT方式将字节流传到服务器
    /// </summary>
    /// <param name="url">服务器目标地址 like 'http://www.my-server.com/upload' </param>
    /// <param name="content">需要上传的字节流</param>
    /// <param name="actionResult">处理返回结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    public void Put(string url, string content, Action<NetcomData> actionResult)
    {
        App.app.StartCoroutine(_Put(url, content, actionResult));
    }

    /// <summary>
    /// GET请求
    /// </summary>
    /// <param name="url">请求地址,like 'http://www.my-server.com/ '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    IEnumerator _Get(string url, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
        }
    }

    /// <summary>
    /// 请求图片
    /// </summary>
    /// <param name="url"></param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的byte数组</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    IEnumerator _GetBytes(string url, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
        }
    }
    /// <summary>
    /// 请求图片
    /// </summary>
    /// <param name="url">图片地址,like 'http://www.my-server.com/image.png '</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的图片</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    IEnumerator _GetTexture(string url, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = null,
                data = null,
                texture = DownloadHandlerTexture.GetContent(uwr),
                audioclip = null,
                assetbundle = null
            });
        }
    }

    /// <summary>
    /// 请求AssetBundle
    /// </summary>
    /// <param name="url">AssetBundle地址,like 'http://www.my-server.com/myData.unity3d'</param>
    /// <param name="actionResult">请求发起后处理回调结果的委托,处理请求结果的AssetBundle</param>
    /// <param name="actionProgress"></param>
    /// <returns></returns>
    IEnumerator _GetAssetBundle(string url, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = null,
                data = null,
                texture = null,
                audioclip = null,
                assetbundle = DownloadHandlerAssetBundle.GetContent(uwr)
            });
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
    IEnumerator _GetAudioClip(string url, Action<NetcomData> actionResult, AudioType audioType = AudioType.WAV)
    {
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = null,
                data = null,
                texture = null,
                audioclip = DownloadHandlerAudioClip.GetContent(uwr),
                assetbundle = null
            });
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
    IEnumerator _Post(string url, List<IMultipartFormSection> lstformData, Action<NetcomData> actionResult)
    {
        //List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
        //formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));
        using (UnityWebRequest uwr = UnityWebRequest.Post(url, lstformData))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
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
    IEnumerator _Post(string url, WWWForm formData, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Post(url, formData))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
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
    IEnumerator _Post(string url, string postData, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Post(url, postData))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
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
    IEnumerator _Post(string url, Dictionary<string, string> formFields, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Post(url, formFields))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
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
    IEnumerator _Put(string url, byte[] contentBytes, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Put(url, contentBytes))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
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
    IEnumerator _Put(string url, string content, Action<NetcomData> actionResult)
    {
        using (UnityWebRequest uwr = UnityWebRequest.Put(url, content))
        {
            yield return uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                actionResult?.Invoke(new NetcomData
                {
                    progress = uwr.downloadProgress,
                    isDown = false,
                });
                yield return new WaitForEndOfFrame();
            }
            actionResult?.Invoke(new NetcomData
            {
                progress = 1,
                isDown = true,
                isError = !string.IsNullOrEmpty(uwr.error),
                error = uwr.error,
                text = uwr.downloadHandler.text,
                data = uwr.downloadHandler.data,
                texture = null,
                audioclip = null,
                assetbundle = null
            });
        }
    }
    #endregion

    #region IPAddress
    public static string IPAddress
    {
        get
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        //IPv4
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }
    }
    #endregion
}