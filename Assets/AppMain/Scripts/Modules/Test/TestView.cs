/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : Test(View)
 * ===============================================
 * */

using DG.Tweening;
using AppFrame.Tools;
using AppFrame.View;
// using Pico.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Test
{
    public class TestView : ViewBase
    {
        private Button btn;
        private Button btn_params;
        private Button btn_take_photo;
        private Button btn_quit;
        private Text text;
        private RawImage rawImage;
        private RectTransform mobileImageRoot;
        private Transform selfStick;
        private Camera mobileCamera;
        private GifPlayer gifPlayer;
        public Camera renderCamera { private set; get; }

        protected override void InitView()
        {
            base.InitView();
            btn = this.FindComponent<Button>("Button");
            btn_params = this.FindComponent<Button>("ButtonParams");
            btn_take_photo = this.FindComponent<Button>("BtnTakePhoto");
            btn_quit = this.FindComponent<Button>("BtnQuit");
            text = this.FindComponent<Text>("Image/Text");
            rawImage = this.FindComponent<RawImage>("RawImage");
            mobileImageRoot = this.FindComponent<RectTransform>("MobileImageRoot");
            mobileCamera = this.FindComponent<Camera>("MobileCamera");
            renderCamera = mobileCamera.FindComponent<Camera>("RenderCamera");
            selfStick = this.FindComponent<Transform>("SelfStick");
            gifPlayer = this.FindComponent<GifPlayer>("GifImage");
            btn.transform.DORotate(new Vector3(0, 0, 90), 5).SetEase(Ease.Linear);
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            btn.BtnOnClick(() => { });
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

        public void SetRawImage(Texture2D texture)
        {
            rawImage.texture = texture;
        }

        public void SetMobileCamera(RenderTexture renderTexture)
        {
            // mobileCamera.targetTexture = renderTexture;
            // mobileImageRoot.FindComponent<RawImage>("MobileImage").texture = renderTexture;
            // mobileCamera.TryGetComponent<ParentConstraint>().AddSource(new ConstraintSource { sourceTransform = PicoXRManager.Instance.RightController.transform, weight = 1 });
            // mobileCamera.TryGetComponent<ParentConstraint>().SetTranslationOffset(0, new Vector3(0f, 0.1f, 5f));
            // mobileCamera.TryGetComponent<ParentConstraint>().SetRotationOffset(0, new Vector3(0f, 180f, 0f));
            // selfStick.TryGetComponent<ParentConstraint>().AddSource(new ConstraintSource { sourceTransform = PicoXRManager.Instance.RightController.transform, weight = 1 });
            // selfStick.TryGetComponent<ParentConstraint>().SetTranslationOffset(0, Vector3.zero);
            // selfStick.Find("Cube/Quad").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = renderTexture;
            // mobileImageRoot.TryGetComponent<ParentConstraint>().AddSource(new ConstraintSource { sourceTransform = PicoXRManager.Instance.MainCamera.transform, weight = 1 });
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