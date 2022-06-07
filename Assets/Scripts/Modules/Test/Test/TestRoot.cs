using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            AddEventMsgParams("BtnParamsEvent", (object[] args) => { ButtonParamsEvent((string)args[0]); });

            AddEventMsg("BtnTakePhotoEvent", TakePhoto);
            AddEventMsg("BtnQuitEvent", ButtonQuitEvent);
        }
        public void Begin()
        {
            //加载窗体
            string prefab_TestPath = "Test/Assets/Windows/TestWindow";
            testWin = this.LoadUIWindow<TestWindow>(prefab_TestPath, true);

            mobile = new RenderTexture(1920, 1080, 32);
            testWin.SetMobileCamera(mobile);
        }
        private void TakePhoto()
        {
            PictureManager.TakePhoto(testWin.renderCamera, PlatformManager.Instance.GetDataPath("Screenshots"), new Vector2(1920, 1080), (Texture2D texture, string fileName) =>
            {
                PlatformManager.Instance.SavePhoto("Screenshots", fileName);
                testWin.SetRawImage(texture);
            });
        }

        private void ButtonQuitEvent()
        {
            //Ask.AskRoot.Instance.ShowAskWindow("确定退出程序？", () => { PlatformManager.Instance.QuitUnityPlayer(); });
            SendEventMsgParams("ShowAskWindow", "确定退出程序？", (Action)(() => { PlatformManager.Instance.QuitUnityPlayer(); }), (Action)delegate { });
        }
        private void ButtonEvent()
        {
            testWin.SetText("触发不带参数事件");
        }
        private void ButtonParamsEvent(string value)
        {
            testWin.SetText(value);
        }

        public void End()
        {

        }
    }
}
