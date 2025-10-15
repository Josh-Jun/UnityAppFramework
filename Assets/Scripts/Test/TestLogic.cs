/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月1 11:43
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace App.Modules
{
    [LogicOf("Test", AssetPath.MainScene)]
    public class TestLogic : EventBase, ILogic
    {
        private TestView View => ViewMaster.Instance.GetView<TestView>();
        
        public TestLogic()
        {
	        AddEventMsg<object>("OpenTestView", OpenTestView);
	        AddEventMsg("CloseTestView", CloseTestView);

        }
        
        #region Life Cycle
        
        public void Begin()
        {
            View.OpenView();
            var target = GameObject.Find("Cube");
            ViewMaster.Instance.OpenView<Render3D2UIView>(new RenderData()
            {
                Target = target,
                Image = View.RawImageRawImage,
                FollowOffset = Vector3.back * 10,
                LookAtOffset = Vector3.up,
                CanRotate = true,
                CanScale = true,
                PreserveComposition = false,
            });
            TimeUpdateMaster.Instance.StartTimer(time =>
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    View.Move(Vector3.right * Screen.width, 0.2f).CloseView();
                }

                if (Input.GetKeyDown(KeyCode.W))
                {
                    View.Move(Vector3.zero, 0.2f).OpenView();
                }
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    View.Move(Vector3.left * Screen.width, 0.2f).CloseView();
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    View.Move(Vector3.zero, 0.2f).OpenView();
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ViewMaster.Instance.OpenView<WebView>(new WebData()
                    {
                        title = "百度",
                        url = "https://www.baidu.com"
                    });
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ViewMaster.Instance.GoBack();
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    EventMaster.Instance.Execute("TestEvent0");
                }
                if (Input.GetKeyDown(KeyCode.X))
                {
                    EventMaster.Instance.Execute("TestEvent1", 10086);
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    ViewMaster.Instance.SwitchScreen(1).Forget();
                }
            });
        }
        public void End()
        {
            
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
        
        #endregion

        #region Logic
        
        [Event("TestEvent1")]
        public void TestEvent1(int index)
        {
            Log.I($"TestEvent = {index}");
        }
        
        [Event("TestEvent0")]
        public void TestEvent0()
        {
            Log.I($"TestEvent = 0");
        }
        
        #endregion

        #region View Logic
        
        private void OpenTestView(object obj)
        {
            
        }
        private void CloseTestView()
        {
	        
        }
        
        #endregion
    }
}