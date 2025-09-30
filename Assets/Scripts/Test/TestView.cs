/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月1 11:43
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.Modules
{
    [ViewOf("Test", ViewMold.UI2D, AssetPath.TestView, false, 3)]
    public class TestView : ViewBase
    {
		private RawImage rawimagerawimage;

		public RawImage RawImageRawImage { get { return rawimagerawimage; } }

        protected override void InitView()
        {
            base.InitView();
			rawimagerawimage = this.FindComponent<RawImage>("RawImage");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }

        [Event("TestEvent0")]
        public void TestEvent0()
        {
            Log.I("TestEvent");
        }
    }
}
