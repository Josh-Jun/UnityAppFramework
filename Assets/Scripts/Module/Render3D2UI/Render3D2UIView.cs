using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.Modules
{
    [ViewOf("Render3D2UI", ViewMold.Go3D, AssetPath.Render3D2UIView, false, 0)]
    public class Render3D2UIView : ViewBase
    {
		public Camera RenderCamera;
		public CinemachineVirtualCamera CinemachineCinemachineVirtualCamera;
		public CinemachineCameraOffset CinemachineCinemachineCameraOffset;

        protected override void InitView()
        {
            base.InitView();
			RenderCamera = this.FindComponent<Camera>("Render");
			CinemachineCinemachineVirtualCamera = this.FindComponent<CinemachineVirtualCamera>("Cinemachine");
			CinemachineCinemachineCameraOffset = this.FindComponent<CinemachineCameraOffset>("Cinemachine");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
    }
}
