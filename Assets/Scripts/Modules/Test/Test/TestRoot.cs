using System;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using TableData;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Test
{
    public class TestRoot : SingletonEvent<TestRoot>, IRoot
    {
        private TestWindow testWin;
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
            string prefab_TestPath = "Test/Assets/Windows/TestWindow";
            testWin = AssetsManager.Instance.LoadUIWindow<TestWindow>(prefab_TestPath);
            testWin.SetWindowActive();

            mobile = new RenderTexture(1920, 1080, 32);
            testWin.SetMobileCamera(mobile);
        }
        public void End()
        {
            
        }

        public void Test()
        {
            string audio_path = "";
            AudioClip audio = AssetsManager.Instance.LoadAsset<AudioClip>(audio_path);
            AudioManager.Instance.PlayBackgroundAudio(audio);//背景音乐，循环播放
            AudioManager.Instance.PlayEffectAudio(audio);//特效音乐，播放一次，可叠加播放
            
            string vidio_path = "";
            VideoClip video = AssetsManager.Instance.LoadAsset<VideoClip>(vidio_path);
            VideoManager.Instance.PlayVideo(null,video);//播放视频
            VideoManager.Instance.PlayVideo(null,"视频地址", () => { });//播放视频

            AVProManager.Instance.OpenMedia(MediaPathType.AbsolutePathOrURL, "视频地址");
            AVProManager.Instance.AddMediaPlayerEvent(MediaPlayerEvent.EventType.FinishedPlaying,() => { });//播放完成事件

            TableManager.Instance.GetTable<TestTableData>("");//获取配置表

            int timeid1 = -1;
            timeid1 = TimerManager.Instance.StartTimer((time) => { });//一直执行 相当于Update
            TimerManager.Instance.EndTimer(timeid1);
            int timeid2 = -1;
            timeid2 = TimerTaskManager.Instance.AddTimeTask(() => { }, 1f, TimeUnit.Second, 1);//1秒后执行一次
            TimerTaskManager.Instance.DeleteTimeTask(timeid2);

            Animator animator = testWin.TryGetComponent<Animator>();//获取脚本组件，没有就自动添加
            animator.Play("动画名", () => { });//播放动画
            animator.PlayBack("动画名", () => { });//倒放动画
            
            Image image = testWin.TryGetComponent<Image>();//获取脚本组件，没有就自动添加
            RawImage rawimage = testWin.TryGetComponent<RawImage>();//获取脚本组件，没有就自动添加

        }
        private void TakePhoto()
        {
            PictureManager.TakePhoto(testWin.renderCamera, PlatformManager.Instance.GetDataPath("Screenshots"), (Texture2D texture, string fileName) =>
            {
                PlatformManager.Instance.SavePhoto("Screenshots", fileName);
                testWin.SetRawImage(texture);
            });
        }

        private void ButtonQuitEvent()
        {
            //Ask.AskRoot.Instance.ShowAskWindow("确定退出程序？", () => { PlatformManager.Instance.QuitUnityPlayer(); });
            SendEventMsg<string, Action, Action>("ShowAskWindow", "确定退出程序？", () => { PlatformManager.Instance.QuitUnityPlayer(); }, null);
        }
        private void ButtonEvent()
        {
            testWin.SetText("触发不带参数事件");
            SendEventMsg("ShowTips","触发不带参数事件",1.2f);
            // Root.GetRootScript<Ask.AskRoot>().ShowTips("123");
        }
        private void ButtonParamsEvent(string value)
        {
            testWin.SetText(value);
        }

        public void AppPause(bool pause)
        {
            Debug.Log("AppPause = " + pause);
        }
        public void AppFocus(bool focus)
        {
            Debug.Log("AppFocus = " + focus);
        }
        public void AppQuit()
        {

        }
    }
}
