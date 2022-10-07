﻿using System;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using TableData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Threading.Tasks;

namespace Test
{
    public class TestRoot : SingletonEvent<TestRoot>, IRoot
    {
        private TestWindow window;
        private RenderTexture mobile;

        public TestRoot()
        {
            //添加事件系统，不带参数
            AddEventMsg("BtnEvent", ButtonEvent);
            //添加事件系统，带参数
            AddEventMsg<string>("BtnParamsEvent", ButtonParamsEvent);

            AddEventMsg("BtnTakePhotoEvent", TakePhoto);
            AddEventMsg("BtnQuitEvent", ButtonQuitEvent);
        }

        public void Begin()
        {
            //加载窗体
            string prefab_TestPath = string.Format(AssetsPathConfig.TestWindow, Root.AppConfig.TargetPackage);
            window = AssetsManager.Instance.LoadUIWindow<TestWindow>(prefab_TestPath);
            window.SetWindowActive();

            // mobile = new RenderTexture(Screen.width, Screen.height, 32);
            // window.SetMobileCamera(mobile);
            window.PlayGif();
            Debug.Log(TableManager.Instance.GetTable<TestThriftTable>("TestThrift").Data.Count);
            Debug.Log(TableManager.Instance.GetTable<TestJson>("TestJson").Boss.Count);
            RunTest();
        }

        public void End()
        {
        }

        public void RunTest()
        {
            LogText();
        }

        public async void LogText()
        {
            Debug.Log("-------------------1-------------------");
            await Task.Delay(TimeSpan.FromSeconds(3f));
            Debug.Log("-------------------2-------------------");
            TimerServe.Instance.StartTimer((time) =>
            {
                Debug.Log(time);
            });
        }
        public void Test()
        {
            //音频播放
            string audio_path = "";
            AudioClip audio = AssetsManager.Instance.LoadAsset<AudioClip>(audio_path);
            AudioManager.Instance.PlayBackgroundAudio(audio); //背景音乐，循环播放
            AudioManager.Instance.PlayEffectAudio(audio); //特效音乐，播放一次，可叠加播放
            //视频播放（unity自带视频播放器）
            string vidio_path = "";
            VideoClip video = AssetsManager.Instance.LoadAsset<VideoClip>(vidio_path);
            VideoManager.Instance.PlayVideo(null, video); //播放视频
            VideoManager.Instance.PlayVideo(null, "视频地址", () => { }); //播放视频
            //获取配置表
            TableManager.Instance.GetTable<TestXml>(""); //获取配置表
            //时间任务
            int timeid1 = -1;
            timeid1 = TimerServe.Instance.StartTimer((time) => { }); //一直执行 相当于Update
            TimerServe.Instance.EndTimer(timeid1);
            int timeid2 = -1;
            timeid2 = TimerTaskServe.Instance.AddTimeTask(() => { }, 1f, TimeUnit.Second, 1); //1秒后执行一次
            TimerTaskServe.Instance.DeleteTimeTask(timeid2);
            //Animator动画播放
            Animator animator = window.TryGetComponent<Animator>(); //获取脚本组件，没有就自动添加
            animator.Play("动画名", () => { }); //播放动画
            animator.PlayBack("动画名", () => { }); //倒放动画
            //序列帧动画
            Image image = window.TryGetComponent<Image>(); //获取脚本组件，没有就自动添加
            image.PlayFrames(new List<Sprite>());
            //gameobject对象和脚本显隐
            window.SetGameObjectActive();
            window.SetComponentEnable<TestWindow>(false);
            //点击事件
            window.OnClick(() => { });
            window.gameObject.OnClick(() => { });
            window.TryGetComponent<Button>().BtnOnClick(() => { });
            window.gameObject.AddEventTrigger(EventTriggerType.PointerClick, (arg) => { });
            window.AddEventTrigger(EventTriggerType.PointerClick, (arg) => { });
            //平台管理类,不同平台对应的原生方法和属性
            PlatformManager.Instance.GetDataPath("");
        }

        private void TakePhoto()
        {
            PictureTools.TakePhoto(window.renderCamera, PlatformManager.Instance.GetDataPath("Screenshots"),
                (Texture2D texture, string fileName) =>
                {
                    PlatformManager.Instance.SavePhoto(PlatformManager.Instance.GetDataPath("Screenshots/") + fileName);
                    window.SetRawImage(texture);
                });
        }

        private void ButtonQuitEvent()
        {
            //Ask.AskRoot.Instance.ShowAskWindow("确定退出程序？", () => { PlatformManager.Instance.QuitUnityPlayer(); });
            SendEventMsg<string, Action, Action>("ShowAskWindow", "确定退出程序？",
                () => { PlatformManager.Instance.QuitUnityPlayer(); }, null);
        }

        private void ButtonEvent()
        {
            window.SetText("触发不带参数事件");
            SendEventMsg("ShowTips", "触发不带参数事件", 1.2f);
            // Root.GetRootScript<Ask.AskRoot>().ShowTips("123");
        }

        

        private void ButtonParamsEvent(string value)
        {
            window.SetText(value);
            string filePath = PlatformManager.Instance.GetDataPath("App/meta.apk");
            UnityWebRequester requester = new UnityWebRequester();
            
            // int id = -1;
            // if (FileManager.FileExist(filePath))
            // {
            //     PlatformManager.Instance.InstallApp(filePath);
            //     requester = null;
            // }
            // else
            // {
            //     requester.GetBytes("https://meta-oss.genimous.com/vr-ota/App/meta.apk", (bytes) =>
            //     {
            //         FileManager.CreateFile(filePath, bytes);
            //         PlatformManager.Instance.InstallApp(filePath);
            //         TimerManager.Instance.EndTimer(id);
            //         requester.Destory();
            //     });
            //     id = TimerManager.Instance.StartTimer((time) =>
            //     {
            //         window.SetText(requester.DownloadedProgress.ToString("F2"));
            //     });
            // }

            requester.DownloadFile("https://meta-oss.genimous.com/vr-ota/App/meta.apk", filePath, (size, progress) =>
            {
                window.SetText(progress.ToString("F2"));
                if (progress >= 1)
                {
                    PlatformManager.Instance.InstallApp(filePath);
                }
            });
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