using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace Test
{
    public class TestWindow : WindowBase
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
        public Camera renderCamera { private set; get; }
        protected override void InitWindow()
        {
            base.InitWindow();
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
            btn.transform.DORotate(new Vector3(0, 0, 90), 5).SetEase(Ease.Linear);
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            btn.BtnOnClick((object[] objs) => { });
            btn.onClick.AddListener(() => { SendEventMsg("BtnEvent"); });
            btn_take_photo.onClick.AddListener(() => { SendEventMsg("BtnTakePhotoEvent"); });
            btn_params.onClick.AddListener(() => { SendEventMsgParams("BtnParamsEvent", "触发带参数事件"); });
            btn_quit.onClick.AddListener(() => { SendEventMsg("BtnQuitEvent"); });
        }

        protected override void OpenWindow()
        {
            base.OpenWindow();
            Debug.Log("OpenWindow");
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();

        }
        public void SetRawImage(Texture2D texture)
        {
            rawImage.texture = texture;
        }

        public void SetMobileCamera(RenderTexture renderTexture)
        {
            mobileCamera.targetTexture = renderTexture;
            mobileImageRoot.FindComponent<RawImage>("MobileImage").texture = renderTexture;
            mobileCamera.TryGetComponent<ParentConstraint>().AddSource(new ConstraintSource { sourceTransform = PicoXRManager.Instance.RightController.transform, weight = 1 });
            mobileCamera.TryGetComponent<ParentConstraint>().SetTranslationOffset(0, new Vector3(0f, 0.1f, 5f));
            mobileCamera.TryGetComponent<ParentConstraint>().SetRotationOffset(0, new Vector3(0f, 180f, 0f));
            selfStick.TryGetComponent<ParentConstraint>().AddSource(new ConstraintSource { sourceTransform = PicoXRManager.Instance.RightController.transform, weight = 1 });
            selfStick.TryGetComponent<ParentConstraint>().SetTranslationOffset(0, Vector3.zero);
            selfStick.Find("Cube/Quad").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = renderTexture;
            mobileImageRoot.TryGetComponent<ParentConstraint>().AddSource(new ConstraintSource { sourceTransform = PicoXRManager.Instance.MainCamera.transform, weight = 1 });
        }
        public void SetText(string value)
        {
            text.text = value;
        }
    }
}
