/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年2月1 8:18
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;

namespace Modules.SceneLoader
{
    [LogicOf(AssetPath.Global)]
    public class SceneLoaderLogic : SingletonEvent<SceneLoaderLogic>, ILogic
    {
        private Queue<string> loadSceneQueue = new Queue<string>();
        public string CurrentScene { get; private set; }
        
        public SceneLoaderLogic()
        {
            AddEventMsg<string>("LoadScene", LoadScene);
            AddEventMsg("GoBackScene", GoBackScene);
        }

        public void GoBackScene()
        {
            if (loadSceneQueue.Count > 0)
            {
                CurrentScene = loadSceneQueue.Dequeue();
                Assets.LoadSceneAsync(AssetPath.LoadingScene);
            }
            Log.W($"已经无路可退了：{loadSceneQueue.Count}");
        }

        public void LoadScene(string location)
        {
            if (!string.IsNullOrEmpty(CurrentScene))
            {
                loadSceneQueue.Enqueue(CurrentScene);
            }
            CurrentScene = location;
            Assets.LoadSceneAsync(AssetPath.LoadingScene);
        }

        public void Begin()
        {
            
        }
        public void End()
        {
            
        }
        public void AppPause(bool pause)
        {
            
        }
        public void AppFocus(bool focus)
        {
            
        }
        public void AppQuit()
        {
            
        }
    }
}