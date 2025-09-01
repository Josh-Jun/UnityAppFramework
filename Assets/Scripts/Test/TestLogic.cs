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