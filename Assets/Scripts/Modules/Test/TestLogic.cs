using System;
using System.Collections;
using System.Collections.Generic;
using Table.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Threading.Tasks;
using AppFramework.Data;
using AppFramework.Info;
using AppFramework.Interface;
using AppFramework.Manager;
using AppFramework.Tools;
using Table.Data;

namespace Modules.Test
{
    public class TestLogic : SingletonEvent<TestLogic>, ILogic
    {
        private TestView view;
        private RenderTexture mobile;

        public TestLogic()
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
            string prefab_TestPath = string.Format(AssetsPathConfig.TestView, AppInfo.AppConfig.TargetPackage);
            view = AssetsManager.Instance.LoadUIView<TestView>(prefab_TestPath);
            view.SetViewActive();

            // mobile = new RenderTexture(Screen.width, Screen.height, 32);
            // window.SetMobileCamera(mobile);
            view.PlayGif();
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
            TimerLogicer.Instance.StartTimer((time) =>
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
            timeid1 = TimerLogicer.Instance.StartTimer((time) => { }); //一直执行 相当于Update
            TimerLogicer.Instance.EndTimer(timeid1);
            int timeid2 = -1;
            timeid2 = TimerTasker.Instance.AddTimeTask(() => { }, 1f, TimeUnit.Second, 1); //1秒后执行一次
            TimerTasker.Instance.DeleteTimeTask(timeid2);
            //Animator动画播放
            Animator animator = view.TryGetComponent<Animator>(); //获取脚本组件，没有就自动添加
            animator.Play("动画名", () => { }); //播放动画
            animator.PlayBack("动画名", () => { }); //倒放动画
            //序列帧动画
            Image image = view.TryGetComponent<Image>(); //获取脚本组件，没有就自动添加
            image.PlayFrames(new List<Sprite>());
            //gameobject对象和脚本显隐
            view.SetGameObjectActive();
            view.SetComponentEnable<TestView>(false);
            //点击事件
            view.OnClick(() => { });
            view.gameObject.OnClick(() => { });
            view.TryGetComponent<Button>().BtnOnClick(() => { });
            view.gameObject.AddEventTrigger(EventTriggerType.PointerClick, (arg) => { });
            view.AddEventTrigger(EventTriggerType.PointerClick, (arg) => { });
            //平台管理类,不同平台对应的原生方法和属性
            PlatformManager.Instance.GetDataPath("");
        }

        private void TakePhoto()
        {
            PictureTools.TakePhoto(view.renderCamera, PlatformManager.Instance.GetDataPath("Screenshots"),
                (Texture2D texture, string fileName) =>
                {
                    PlatformManager.Instance.SavePhoto(PlatformManager.Instance.GetDataPath("Screenshots/") + fileName);
                    view.SetRawImage(texture);
                });
        }

        private void ButtonQuitEvent()
        {
            //Ask.AskRoot.Instance.ShowAskView("确定退出程序？", () => { PlatformManager.Instance.QuitUnityPlayer(); });
            SendEventMsg<string, Action, Action>("ShowAskView", "确定退出程序？",
                () => { PlatformManager.Instance.QuitUnityPlayer(); }, null);
        }

        private void ButtonEvent()
        {
            view.SetText("触发不带参数事件");
            SendEventMsg("ShowTips", "触发不带参数事件", 1.2f);
            // Root.GetLogicScript<Ask.AskLogic>().ShowTips("123");
        }

        

        private void ButtonParamsEvent(string value)
        {
            view.SetText(value);
            string filePath = PlatformManager.Instance.GetDataPath("App/meta.apk");
            UnityWebRequester requester = NetcomManager.Uwr;
            
            int id = -1;
            // if (FileTools.FileExist(filePath))
            // {
            //     PlatformManager.Instance.InstallApp(filePath);
            //     requester = null;
            // }
            // else
            // {
            //     requester.GetBytes("https://meta-oss.genimous.com/vr-ota/App/meta.apk", (bytes) =>
            //     {
            //         FileTools.CreateFile(filePath, bytes);
            //         PlatformManager.Instance.InstallApp(filePath);
            //         TimerLogicer.Instance.EndTimer(id);
            //         requester.Destory();
            //     });
            //     id = TimerLogicer.Instance.StartTimer((time) =>
            //     {
            //         window.SetText(requester.DownloadedProgress.ToString("F2"));
            //     });
            // }

            requester.DownloadFile("https://meta-oss.genimous.com/vr-ota/App/meta.apk", filePath, (totalLength, fileLength) =>
            {
                id = TimerLogicer.Instance.StartTimer((time) =>
                {
                    float progress = (float)((long)requester.DownloadedLength + fileLength) / (float)totalLength;
                    view.SetText(progress.ToString("F2"));
                    if (progress >= 1)
                    {
                        PlatformManager.Instance.InstallApp(filePath);
                        TimerLogicer.Instance.EndTimer(id);
                        requester.Destory();
                    }
                });
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