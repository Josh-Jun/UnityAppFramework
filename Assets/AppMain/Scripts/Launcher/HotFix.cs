using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace Launcher
{
    public class HotFix
    {
        private string LocalPath;
        private string LocalVersionConfigPath;

        private string ServerUrl;
        private string ServerVersionConfigPath;
        
        public HotFix()
        {
            
        }
        
        private void DownLoad(string url, Action<byte[]> callback)
        {
            var unityWebRequest = new UnityWebRequest();
            unityWebRequest.url = url;
            unityWebRequest.method = UnityWebRequest.kHttpVerbGET;
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            var async = unityWebRequest.SendWebRequest();
            async.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(unityWebRequest.error))
                {
                    callback?.Invoke(unityWebRequest.downloadHandler.data);
                }
                else
                {
                    Debug.LogError($"[Error:Bytes] {url} {unityWebRequest.error}");
                }
            };
        }
    }
}
