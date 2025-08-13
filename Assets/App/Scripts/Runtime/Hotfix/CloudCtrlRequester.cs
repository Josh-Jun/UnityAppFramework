/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月13 11:15
 * function    :
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace App.Runtime.CloudCtrl
{
    public static class CloudCtrlRequester
    {
        private const string CloudCtrlAPI = "";

        public static void Post()
        {
            if (string.IsNullOrEmpty(CloudCtrlAPI)) return;
            var url = $"{Global.HttpServer}{CloudCtrlAPI}";
            var data = new CloudCtrlData()
            {
                bundleId = Application.identifier,
                platform = $"{Application.platform.ToString().ToLower()}",
                version = Application.version,
                versioncode = Application.version.Replace(".", "")
            };
            var json = JsonUtility.ToJson(data);
            UniTask.Void(async () =>
            {
                using var request = new UnityWebRequest();
                request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
                request.url = url;
                request.method = UnityWebRequest.kHttpVerbPOST;
                request.downloadHandler = new DownloadHandlerBuffer();
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (request.downloadHandler.text != null)
                    {
                        Global.CloudCtrl = JsonUtility.FromJson<CloudCtrl>(request.downloadHandler.text);
                    }
                    else
                    {
                        Debug.Log("RequestCloudCtrlData : request.downloadHandler.text = null");
                    }
                }
                else
                {
                    Debug.Log($"RequestCloudCtrlData : error: {request.error}");
                }
            });
        }

        public static void Get()
        {
            var url = $"{Global.CdnServer}/App/{Application.identifier}/{Global.PlatformName}/v{Application.version}/app.json?timestamp={DateTime.Now.Ticks}";
            UniTask.Void(async () =>
            {
                using var request = new UnityWebRequest();
                request.url = url;
                request.method = UnityWebRequest.kHttpVerbGET;
                request.downloadHandler = new DownloadHandlerBuffer();
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (request.downloadHandler.text != null)
                    {
                        Global.CloudCtrl = JsonUtility.FromJson<CloudCtrl>(request.downloadHandler.text);
                    }
                    else
                    {
                        Debug.Log("RequestCloudCtrlData : request.downloadHandler.text = null");
                    }
                }
                else
                {
                    Debug.Log($"RequestCloudCtrlData : error: {request.error}");
                }
            });
        }
    }
    /****************Josn格式*****************
    {
	    "IntData":[
	    	{"name":"test","value":1},
	    	{"name":"count","value":11}
	    ],
	    "BoolData":[
	    	{"name":"test","value":true},
	    	{"name":"Use","value":false}
	    ],
	    "FloatData":[
	    	{"name":"test","value":1.0},
	    	{"name":"speed","value":0.5}
	    ],
	    "StringData":[
	    	{"name":"test","value":"test"},
	    	{"name":"name","value":"Josh"}
        ]
    }
    ******************************************/

    [Serializable]
    public class CloudCtrlData
    {
        public string bundleId;
        public string platform;
        public string version;
        public string versioncode;
    }

    [Serializable]
    public class CloudCtrl
    {
        public List<IntData> IntData { get; set; } = new();
        public List<BoolData> BoolData { get; set; } = new();
        public List<FloatData> FloatData { get; set; } = new();
        public List<StringData> StringData { get; set; } = new();
    }

    [Serializable]
    public class IntData
    {
        public string name;
        public int value;
    }

    [Serializable]
    public class FloatData
    {
        public string name;
        public float value;
    }

    [Serializable]
    public class StringData
    {
        public string name;
        public string value;
    }

    [Serializable]
    public class BoolData
    {
        public string name;
        public bool value;
    }
}