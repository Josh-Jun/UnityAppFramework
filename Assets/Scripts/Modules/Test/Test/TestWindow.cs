using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class TestWindow : WindowBase
    {
        private Button btn;
        private Button btn_params;
        private Text text;
        protected override void InitWindow()
        {
            base.InitWindow();
            btn = this.FindComponent<Button>("Button");
            btn_params = this.FindComponent<Button>("ButtonParams");
            text = this.FindComponent<Text>("Image/Text");
            btn.transform.DORotate(new Vector3(0, 0, 90), 5).SetEase(Ease.Linear);
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            btn.BtnOnClick((object[] objs)=> { });
            btn.onClick.AddListener(() => { SendEventMsg("BtnEvent"); });
            btn_params.onClick.AddListener(() => { SendEventMsgParams("BtnParamsEvent", "触发带参数事件"); });
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

        public void SetText(string value)
        {
            text.text = value;
        }
    }
}
