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

namespace App.Modules
{
    public enum FitMode { Cover, Contain, Stretch }
    public enum PivotAlignment { Center, Top, Bottom, Left, Right }

    // distanceFromCamera: Sprite与相机的距离，影响显示大小
    // fieldOfViewMargin: 视野边距，确保完全覆盖屏幕
    // autoCalculateDistance: 自动计算最佳距离
    // billboard: 是否始终面向相机
    [Serializable]
    public class BackgroundData
    {
        public Sprite sprite;
        public FitMode fitMode = FitMode.Cover;
        public PivotAlignment pivotAlignment = PivotAlignment.Center;
        public float distanceFromCamera = 10f;
        public float sizeMultiplier = 1.0f;
        public bool useWorldSpace = true;
        public bool billboard = true;
    }
    
    [LogicOf("Background", AssetPath.Global)]
    public class BackgroundLogic : EventBase, ILogic
    {
        private BackgroundView View => ViewMaster.Instance.GetView<BackgroundView>();
        private BackgroundData backgroundData;
        public BackgroundLogic()
        {
	        AddEventMsg<object>("OpenBackgroundView", OpenBackgroundView);
	        AddEventMsg("CloseBackgroundView", CloseBackgroundView);

        }
        
        #region Life Cycle
        
        public void Begin()
        {
            
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

        private void UpdatePosition()
        {
            var camTransform = ViewMaster.Instance.UICamera3D.transform;

            // 基础位置：相机前方指定距离
            var basePosition = camTransform.position + camTransform.forward * backgroundData.distanceFromCamera;
            // 相对于相机的局部位置
            var offsetPosition = camTransform.TransformPoint(Vector3.forward * backgroundData.distanceFromCamera);
            var position = backgroundData.useWorldSpace ? basePosition : offsetPosition;
            View.BackgroundSpriteRenderer.transform.position = position;
        }

        private void UpdateScale()
        {
            var sprite = View.BackgroundSpriteRenderer.sprite;
            var spriteWidth = sprite.bounds.size.x;
            var spriteHeight = sprite.bounds.size.y;

            // 计算透视投影下的可见尺寸
            var visibleHeight = 2.0f * backgroundData.distanceFromCamera * Mathf.Tan(ViewMaster.Instance.UICamera3D.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var visibleWidth = visibleHeight * ViewMaster.Instance.UICamera3D.aspect;

            var scale = CalculateScale(spriteWidth, spriteHeight, visibleWidth, visibleHeight);

            // 应用缩放和乘数
            View.BackgroundSpriteRenderer.transform.localScale = new Vector3(scale.x * backgroundData.sizeMultiplier, scale.y * backgroundData.sizeMultiplier, 1f);

            // 应用对齐偏移
            ApplyAlignmentOffset(scale.x * spriteWidth, scale.y * spriteHeight, visibleWidth, visibleHeight);
        }

        private Vector2 CalculateScale(float spriteWidth, float spriteHeight, float visibleWidth, float visibleHeight)
        {
            var scaleX = visibleWidth / spriteWidth;
            var scaleY = visibleHeight / spriteHeight;

            switch (backgroundData.fitMode)
            {
                case FitMode.Cover:
                    var coverScale = Mathf.Max(scaleX, scaleY);
                    return new Vector2(coverScale, coverScale);

                case FitMode.Contain:
                    var containScale = Mathf.Min(scaleX, scaleY);
                    return new Vector2(containScale, containScale);

                case FitMode.Stretch:
                    return new Vector2(scaleX, scaleY);

                default:
                    return Vector2.one;
            }
        }

        private void ApplyAlignmentOffset(float scaledWidth, float scaledHeight, float visibleWidth, float visibleHeight)
        {
            var offset = Vector3.zero;
            var widthDifference = visibleWidth - scaledWidth;
            var heightDifference = visibleHeight - scaledHeight;

            switch (backgroundData.pivotAlignment)
            {
                case PivotAlignment.Top:
                    offset.y = -heightDifference * 0.5f;
                    break;
                case PivotAlignment.Bottom:
                    offset.y = heightDifference * 0.5f;
                    break;
                case PivotAlignment.Left:
                    offset.x = widthDifference * 0.5f;
                    break;
                case PivotAlignment.Right:
                    offset.x = -widthDifference * 0.5f;
                    break;
                case PivotAlignment.Center:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // 将偏移转换到世界空间
            if (backgroundData.useWorldSpace)
            {
                var camTransform = ViewMaster.Instance.UICamera3D.transform;
                offset = camTransform.right * offset.x + camTransform.up * offset.y;
            }

            View.BackgroundSpriteRenderer.transform.position += offset;
        }

        private void UpdateBillboarding()
        {
            if (backgroundData.billboard)
            {
                // 使Sprite始终面向相机
                View.BackgroundSpriteRenderer.transform.rotation = ViewMaster.Instance.UICamera3D.transform.rotation;
            }
        }
        
        #endregion

        #region View Logic

        private void OpenBackgroundView(object obj)
        {
            if (obj is BackgroundData data)
            {
                backgroundData = data;
                View.BackgroundSpriteRenderer.SetGameObjectActive();
                View.BackgroundSpriteRenderer.sprite = backgroundData.sprite;
                UpdatePosition();
                UpdateScale();
                UpdateBillboarding();
        
#if UNITY_EDITOR
                SendEventMsg("SetBackgroundData", data);
#endif
            }
            else
            {
                Log.W("OpenRender3D2UIView obj is not RenderData");
            }
        }
        private void CloseBackgroundView()
        {
            View.BackgroundSpriteRenderer.sprite = null;
            View.BackgroundSpriteRenderer.SetGameObjectActive(false);
        }
        
        #endregion
    }
}