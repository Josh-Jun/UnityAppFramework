using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class TestRoot : SingletonEvent<TestRoot>, IRoot
    {
        private TestWindow testWin;
        public TestRoot()
        {
            //添加事件系统，不带参数
            AddEventMsg("BtnEvent", ButtonEvent);
            //添加事件系统，带参数
            AddEventMsgParams("BtnParamsEvent", (object[] args)=> { ButtonParamsEvent((string)args[0]); });
        }
        public void Begin()
        {
            //加载窗体
            string prefab_TestPath = "Test/Assets/Windows/TestWindow";
            testWin = this.LoadWindow<TestWindow>(prefab_TestPath, true);
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
