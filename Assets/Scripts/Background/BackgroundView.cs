/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年10月23 11:39
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
    [ViewOf("Background", ViewMold.Go3D, AssetPath.BackgroundView, false, 0)]
    public class BackgroundView : ViewBase
    {
		private SpriteRenderer backgroundspriterenderer;

		public SpriteRenderer BackgroundSpriteRenderer { get { return backgroundspriterenderer; } }

        protected override void InitView()
        {
            base.InitView();
			backgroundspriterenderer = this.FindComponent<SpriteRenderer>("Background");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
    }
}
