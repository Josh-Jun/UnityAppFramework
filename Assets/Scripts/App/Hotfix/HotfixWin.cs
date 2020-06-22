using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Data;

namespace Hotfix
{
    public class HotfixWin : UIWindowBase
    {
        private RectTransform progressBarPanel;
        private Image image_Progress;
        private Text text_Tips;
        private Text text_Speed;
        private Text text_Progress;

        private RectTransform updateTipsPanel;
        private Text text_Content;
        private Button btn_NextTime;
        private Button btn_UpdateNow;
        protected override void InitEvent()
        {
            base.InitEvent();

            progressBarPanel = this.FindComponent<RectTransform>("ProgressBarPanel");
            image_Progress = this.FindComponent<Image>("ProgressBarPanel/Progress");
            text_Tips = this.FindComponent<Text>("ProgressBarPanel/TipsText");
            text_Speed = this.FindComponent<Text>("ProgressBarPanel/SpeedText");
            text_Progress = this.FindComponent<Text>("ProgressBarPanel/ProgressText");

            updateTipsPanel = this.FindComponent<RectTransform>("UpdateTipsPanel");
            text_Content = this.FindComponent<Text>("UpdateTipsPanel/Scroll View Tips/Viewport/ContentText");
            btn_NextTime = this.FindComponent<Button>("UpdateTipsPanel/NextTime");
            btn_UpdateNow = this.FindComponent<Button>("UpdateTipsPanel/UpdateNow");
        }
        protected override void RegisterEvent(bool isRemove = true)
        {
            base.RegisterEvent(isRemove);

        }
        protected override void OpenWindow()
        {
            base.OpenWindow();

            updateTipsPanel.SetGameObjectActive(false);
            progressBarPanel.SetGameObjectActive(false);
            text_Tips.text = "";
            text_Speed.text = "";
            text_Progress.text = "";
            text_Content.text = "";
        }
        protected override void CloseWindow()
        {
            base.CloseWindow();

        }
        /// <summary>
        /// 开始检测更新
        /// </summary>
        public void StartHotfix()
        {
            HotfixManager.Instance.StartHotfix((bool isUpdate, List<Scene> scenes) =>
            {
                if (isUpdate)
                {
                    string des = "";
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        des += string.IsNullOrEmpty(des) ? scenes[i].Des : "\n" + scenes[i].Des;
                    }
                    text_Content.text = des;
                    updateTipsPanel.SetGameObjectActive(true);
                    btn_NextTime.BtnOnClick((object[] objs) =>
                    {
                        updateTipsPanel.SetGameObjectActive(false);
                        HotfixManager.Instance.DeleteLocalVersionXml();
                        Application.Quit();
                    });
                    btn_UpdateNow.BtnOnClick((object[] objs) =>
                    {
                        updateTipsPanel.SetGameObjectActive(false);
                        float time = 0;
                        float previousSize = 0;
                        float speed = 0;
                        text_Tips.text = "检查更新中...";
                        HotfixManager.Instance.DownLoadAssetBundle(scenes, (float progress) =>
                        {
                            if (time == 0)
                            {
                                previousSize = HotfixManager.Instance.LoadedSize;
                            }
                            if (time > 0.1f)
                            {
                                time = 0;
                                speed = (HotfixManager.Instance.LoadedSize - previousSize) / 0.1f;
                            }
                            time += Time.deltaTime;
                            text_Speed.text = string.Format("{0}M/S", speed.ToString("F2"));
                            text_Progress.text = string.Format("{0}M/{1}M", HotfixManager.Instance.LoadedSize.ToString("F2"), HotfixManager.Instance.LoadTotalSize.ToString("F2"));
                            image_Progress.fillAmount = progress;
                            text_Tips.text = "下载中...";
                            if (progress == 1)
                            {
                                text_Tips.text = "正在加载资源...";
                                //更新下载完成，加载AB包
                                LoadAssetBundle();
                            }
                        });
                    });
                }
                else
                {
                    text_Tips.text = "正在加载资源...";
                    //无更新加载AB包
                    LoadAssetBundle();
                }
            });
        }
        /// <summary>
        /// 加载AB包
        /// </summary>
        private void LoadAssetBundle()
        {
            progressBarPanel.SetGameObjectActive(true);
            HotfixManager.Instance.LoadAssetBundle((bool isEnd, string bundleName, float bundleProgress) =>
            {
                text_Tips.text = string.Format("正在加载资源:{0}资源包...", bundleName);
                if (isEnd && bundleProgress == 1)
                {
                    //AB包加载完成
                    AssetsManager.Instance.LoadSceneAsync("Test1/Scene1/Test1");
                    SetWindowActive(false);
                }
            });
        }
    }
}