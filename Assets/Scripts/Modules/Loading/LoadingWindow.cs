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
            loadingSlider.value = 0f;
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();

        }

        public IEnumerator StartLoadScene()
        {
            yield return new WaitForEndOfFrame();
            int displayProgress = 0;
            int toProgress;
            async = SceneManager.LoadSceneAsync(Root.LoadingScene.Name);
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
            while (loadingSlider.value != 1) { yield return new WaitForEndOfFrame(); }
            yield return new WaitForEndOfFrame();
            async.allowSceneActivation = true;
            async.completed += (AsyncOperation ao) => { Root.InitRootBegin(Root.LoadingScene.Name, Root.LoadingScene.Callback); };
        }
    }
}
