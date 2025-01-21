/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : Test(View)
 * ===============================================
 * */

using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace App.Modules.Test
{
    [ViewOf(ViewMold.UI2D, AssetPath.TestView, true, 0)]
    public class TestView : ViewBase
    {
        private Button btn;
        private Button btn_params;
        private Button btn_take_photo;
        private Button btn_quit;
        private Text text;
        private RawImage rawImage;
        private GifPlayer gifPlayer;

        protected override void InitView()
        {
            base.InitView();
            btn = this.FindComponent<Button>("Button");
            btn_params = this.FindComponent<Button>("ButtonParams");
            btn_take_photo = this.FindComponent<Button>("BtnTakePhoto");
            btn_quit = this.FindComponent<Button>("BtnQuit");
            text = this.FindComponent<Text>("Image/Text");
            rawImage = this.FindComponent<RawImage>("RawImage");
            gifPlayer = this.FindComponent<GifPlayer>("GifImage");
            btn.transform.DORotate(new Vector3(0, 0, 90), 5).SetEase(Ease.Linear);
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            btn.BtnOnClick(() => {  });
            btn.onClick.AddListener(() => { SendEventMsg("BtnEvent"); });
            btn_take_photo.onClick.AddListener(() => { SendEventMsg("BtnTakePhotoEvent"); });
            btn_params.onClick.AddListener(() => { SendEventMsg("BtnParamsEvent", "触发带参数事件"); });
            btn_quit.onClick.AddListener(() => { SendEventMsg("BtnQuitEvent"); });
        }

        protected override void OpenView()
        {
            base.OpenView();
        }

        protected override void CloseView()
        {
            base.CloseView();
        }

        [Event("TestViewEvent")]
        public void TestEvent(string param)
        {
            Log.I("TestViewEvent", ("Test", param));
        }
        public void SetRawImage(Texture2D texture)
        {
            rawImage.texture = texture;
        }

        public void SetText(string value)
        {
            if (text == null) return;
            text.text = value;
        }

        public void PlayGif()
        {
            gifPlayer.FileName = "GifPlayerExampes/GIFPlayerExampe1.gif";
            gifPlayer.Init();
            gifPlayer.Play();
        }
    }
}