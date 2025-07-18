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
using System.Linq;
using App.Core.Tools;

namespace App.Core.Master
{
    public enum LoadSceneMold
    {
        YAScene,
        ABScene,
    }
    public class LoadSceneData
    {
        public string Location;
        public string Name;
        public LoadSceneMold Mold;
    }
    public class SceneMaster : SingletonMonoEvent<SceneMaster>
    {
        private readonly Queue<LoadSceneData> LoadedSceneQueue = new();
        public LoadSceneData TargetScene { get; private set; }
        public LoadSceneData CurrentScene { get; private set; }

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

        public void LoadScene(string location, LoadSceneMold mold = LoadSceneMold.YAScene)
        {
            if (TargetScene != null)
            {
                CurrentScene = TargetScene;
                LoadedSceneQueue.Enqueue(CurrentScene);
            }
            TargetScene = new LoadSceneData
            {
                Location = location,
                Name = location.Split('/').Last().Split('.').First(),
                Mold = mold,
            };
            Assets.LoadSceneAsync(AssetPath.LoadingScene);
        }
    }
}
