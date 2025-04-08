/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年4月8 15:16
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using App.Core.Tools;

namespace App.Core.Master
{
    public class SceneMaster : SingletonMonoEvent<SceneMaster>
    {
        private readonly Queue<string> LoadedSceneQueue = new Queue<string>();
        public string TargetScene { get; private set; }
        public string CurrentScene { get; private set; }
        
        public delegate void LoadSceneEvent(string location);

        public LoadSceneEvent BeforeLoadScene;
        public LoadSceneEvent AfterLoadScene;

        private void Awake()
        {
            AddEventMsg<string>("BeforeLoadSceneEvent", (scene) => { BeforeLoadScene?.Invoke(scene); });
            AddEventMsg<string>("AfterLoadSceneEvent", (scene) => { AfterLoadScene?.Invoke(scene); });
        }

        public void GoBackScene()
        {
            if (LoadedSceneQueue.Count > 0)
            {
                CurrentScene = TargetScene;
                TargetScene = LoadedSceneQueue.Dequeue();
                Assets.LoadSceneAsync(AssetPath.LoadingScene);
            }
            Log.W($"已经无路可退了：{LoadedSceneQueue.Count}");
        }

        public void LoadScene(string location)
        {
            if (!string.IsNullOrEmpty(TargetScene))
            {
                CurrentScene = TargetScene;
                LoadedSceneQueue.Enqueue(CurrentScene);
            }
            TargetScene = location;
            Assets.LoadSceneAsync(AssetPath.LoadingScene);
        }
    }
}
