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
#if UNITY_EDITOR
            AddEventMsg<BackgroundData>("SetBackgroundData", SetBackgroundData);
#endif
        }
        #if UNITY_EDITOR
        private BackgroundData backgroundData;
        private void SetBackgroundData(BackgroundData data)
        {
            backgroundData = data;
        }
        // 调试可视化
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || backgroundData == null) return;
            Gizmos.color = Color.green;
            var mainCamera = ViewMaster.Instance.UICamera3D;
            var visibleHeight = 2.0f * backgroundData.distanceFromCamera * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var visibleWidth = visibleHeight * mainCamera.aspect;
            var center = mainCamera.transform.position + mainCamera.transform.forward * backgroundData.distanceFromCamera;
            Gizmos.DrawWireCube(center, new Vector3(visibleWidth, visibleHeight, 0.1f));
        }
        #endif
    }
}
