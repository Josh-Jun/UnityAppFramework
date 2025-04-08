/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : Test(Logic)
 * ===============================================
 * */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Threading.Tasks;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;

namespace App.Modules.Test
{
    [LogicOf(AssetPath.MainScene)]
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
            view = ViewMaster.Instance.GetView<TestView>();

            view.PlayGif();
            RunTest();
            TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    EventMaster.Instance.Execute("TestViewEvent", "Execute");
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    EventMaster.Instance.Execute("TestLogicEvent");
                }
            });
        }

        public void End()
        {
        }

        public void RunTest()
        {
            // LogText();
            // var excelPackageDatas = ExcelTools.ReadExcel($"F:/WorkSpace/UnityFramework/MyUnityFramework/Data/excel/test.xlsx");
            // for (int j = 0; j < excelPackageDatas.Count; j++)
            // {
            //     ExcelData excel = excelPackageDatas[j];
            //     if(excel.sheetName.Contains("#")) continue;
            //     for (int r = 1; r < excel.datas.GetLength(0); r++)
            //     {
            //         for (int c = 1; c < excel.datas.GetLength(1); c++)
            //         {
            //             Debug.Log($"{excel.sheetName}=>({r},{c})=>{excel.datas[r,c]}");
            //         }
            //     }
            // }
            Log.I("123", ("Test", this), ("111", 111), ("222", "222"));
            string formula= "20*(30-10)";
            var result = new System.Data.DataTable().Compute(formula, "");
            Log.W(result);
            Log.E(UserJsonConfig.Instance.Get(2).Test1[1]);
        }

        public async void LogText()
        {
            Log.I("-------------------1-------------------");
            await Task.Delay(TimeSpan.FromSeconds(3f));
            Log.I("-------------------2-------------------");
            TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                Log.I(time);
            });
        }
        public void Test()
        {
            //音频播放
            string audio_path = "";
            AudioClip audio = AssetsMaster.Instance.LoadAsset<AudioClip>(audio_path);
            AudioMaster.Instance.PlayBackgroundAudio(audio); //背景音乐，循环播放
            AudioMaster.Instance.PlayEffectAudio(audio); //特效音乐，播放一次，可叠加播放
            //视频播放（unity自带视频播放器）
            string vidio_path = "";
            VideoClip video = AssetsMaster.Instance.LoadAsset<VideoClip>(vidio_path);
            VideoMaster.Instance.PlayVideo(null, video); //播放视频
            VideoMaster.Instance.PlayVideo(null, "视频地址", () => { }); //播放视频
            //获取配置表
            // TableManager.Instance.GetTable<UserDataJson>(""); //获取配置表
            //时间任务
            int timeid1 = -1;
            timeid1 = TimeUpdateMaster.Instance.StartTimer((time) => { }); //一直执行 相当于Update
            TimeUpdateMaster.Instance.EndTimer(timeid1);
            int timeid2 = -1;
            timeid2 = TimeTaskMaster.Instance.AddTimeTask(() => { }, 1f, TimeUnit.Second, 1); //1秒后执行一次
            TimeTaskMaster.Instance.DeleteTimeTask(timeid2);
            //Animator动画播放
            Animator animator = view.GetOrAddComponent<Animator>(); //获取脚本组件，没有就自动添加
            animator.Play("动画名", () => { }); //播放动画
            animator.PlayBack("动画名", () => { }); //倒放动画
            //序列帧动画
            Image image = view.GetOrAddComponent<Image>(); //获取脚本组件，没有就自动添加
            image.PlayFrames(new List<Sprite>());
            //gameobject对象和脚本显隐
            view.SetGameObjectActive();
            view.SetComponentEnable<TestView>(false);
            //点击事件
            view.OnClick(() => { });
            view.gameObject.OnClick(() => { });
            view.GetOrAddComponent<Button>().BtnOnClick(() => { });
            view.gameObject.AddEventTrigger(EventTriggerType.PointerClick, (arg) => { });
            view.AddEventTrigger(EventTriggerType.PointerClick, (arg) => { });
            //平台管理类,不同平台对应的原生方法和属性
            PlatformMaster.Instance.GetDataPath("");
        }

        [Event("TestLogicEvent")]
        public void TestEvent()
        {
            Log.I("TestLogicEvent", ("Test", this));
        }

        private void TakePhoto()
        {
            // PictureTools.TakePhoto(Camera.main, PlatformMaster.Instance.GetDataPath("Screenshots"),
            //     (Texture2D texture, string filePath) =>
            //     {
            //         PlatformMaster.Instance.SavePhoto(filePath);
            //         view.SetRawImage(texture);
            //     });
        }

        private void ButtonQuitEvent()
        {
            //Root.GetLogicScript<AskLogic>().ShowAskView("ShowAskView", "确定退出程序?", PlatformManager.Instance.QuitUnityPlayer, null, null, null);
            SendEventMsg<string, Action, Action>("ShowAskView", "确定退出程序?", PlatformMaster.Instance.QuitUnityPlayer, null);
        }

        private int count1 = 0;
        private void ButtonEvent()
        {
            view.SetText("触发不带参数事件");
            // SendEventMsg("ShowTips", "触发不带参数事件", 1.2f);
            // Root.GetLogicScript<Ask.AskLogic>().ShowTips("123");
            SceneMaster.Instance.LoadScene(AssetPath.MainScene);
            // count1++;
            // ViewMaster.Instance.RefreshRedDotCount(RedDotMold.SystemMail, count1);
        }

        

        private int count2 = 0;
        private void ButtonParamsEvent(string value)
        {
            view.SetText(value);
            count2++;
            ViewMaster.Instance.RefreshRedDotCount(RedDotMold.FriendMail, count2);
            // string filePath = PlatformMaster.Instance.GetDataPath("App/meta.apk");
            // UnityWebRequester requester = NetcomMaster.Uwr;
            
            // int id = -1;
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

            // requester.DownloadFile("https://meta-oss.genimous.com/vr-ota/App/meta.apk", filePath, (totalLength, fileLength) =>
            // {
            //     id = TimeUpdateMaster.Instance.StartTimer((time) =>
            //     {
            //         float progress = (float)((long)requester.DownloadedLength + fileLength) / (float)totalLength;
            //         view.SetText(progress.ToString("F2"));
            //         if (progress >= 1)
            //         {
            //             PlatformMaster.Instance.InstallApp(filePath);
            //             TimeUpdateMaster.Instance.EndTimer(id);
            //             requester.Destory();
            //         }
            //     });
            // });
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