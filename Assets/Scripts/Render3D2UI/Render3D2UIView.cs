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
		private Camera rendercamera;
		private CinemachineVirtualCamera cinemachinecinemachinevirtualcamera;
		private CinemachineFollowZoom cinemachinecinemachinefollowzoom;
		private CinemachineCameraOffset cinemachinecinemachinecameraoffset;

		public Camera RenderCamera { get { return rendercamera; } }
		public CinemachineVirtualCamera CinemachineCinemachineVirtualCamera { get { return cinemachinecinemachinevirtualcamera; } }
		public CinemachineFollowZoom CinemachineCinemachineFollowZoom { get { return cinemachinecinemachinefollowzoom; } }
		public CinemachineCameraOffset CinemachineCinemachineCameraOffset { get { return cinemachinecinemachinecameraoffset; } }

        protected override void InitView()
        {
            base.InitView();
			rendercamera = this.FindComponent<Camera>("Render");
			cinemachinecinemachinevirtualcamera = this.FindComponent<CinemachineVirtualCamera>("Cinemachine");
			cinemachinecinemachinefollowzoom = this.FindComponent<CinemachineFollowZoom>("Cinemachine");
			cinemachinecinemachinecameraoffset = this.FindComponent<CinemachineCameraOffset>("Cinemachine");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
    }
}
