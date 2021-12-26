using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    public class TestWindow : UIWindowBase
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
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            btn.onClick.AddListener(() => { SendEventMsg("BtnEvent"); });
            btn_params.onClick.AddListener(() => { SendEventMsgParams("BtnParamsEvent", "触发带参数事件"); });
        }

        protected override void OpenWindow()
        {
            base.OpenWindow();

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
