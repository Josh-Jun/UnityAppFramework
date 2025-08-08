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
    [ViewOf(ViewMold.Go3D, AssetPath.Render3D2UIView, false, 0)]
    public class Render3D2UIView : ViewBase
    {
		private Transform modeltransform;
		private Camera rendercamera;

		public Transform ModelTransform { get { return modeltransform; } }
		public Camera RenderCamera { get { return rendercamera; } }

        protected override void InitView()
        {
            base.InitView();
			modeltransform = this.FindComponent<Transform>("Model");
			rendercamera = this.FindComponent<Camera>("Render");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
    }
}
