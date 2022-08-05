using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Loading
{
    public class LoadingWindow : WindowBase
    {
        private Slider loadingSlider;
        private AsyncOperation async;
        protected override void InitWindow()
        {
            base.InitWindow();
            loadingSlider = this.FindComponent<Slider>("LoadingSlider");
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }

        protected override void OpenWindow()
        {
            base.OpenWindow();
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();

        }

        public IEnumerator StartLoadScene(string sceneName, Action callback)
        {
            loadingSlider.value = 0f;
            string[] names = sceneName.Split('/');//names[0], names[1], names[names.Length - 1]
            if (Root.AppConfig.IsLoadAB)
            {
                AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle(names[0], names[1]);
            }
            yield return new WaitForEndOfFrame();
            int displayProgress = 0;
            int toProgress;
            async = SceneManager.LoadSceneAsync(names[names.Length - 1]);
            async.allowSceneActivation = false;
            while (async.progress < 0.9f)
            {
                toProgress = (int)async.progress * 100;
                while (displayProgress < toProgress)
                {
                    ++displayProgress;
                    loadingSlider.value = displayProgress / 100f;
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForEndOfFrame();
            }
            toProgress = 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                loadingSlider.value = displayProgress / 100f;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitUntil(()=> loadingSlider.value == 1);
            async.allowSceneActivation = true;
            async.completed += (AsyncOperation ao) => 
            { 
                Root.InitRootBegin(sceneName, callback); 
                SetWindowActive(false);
            };
        }
    }
}
