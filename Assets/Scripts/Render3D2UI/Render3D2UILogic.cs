/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月4 13:31
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace App.Modules
{
    public class RenderData
    {
        public GameObject gameObject;
        public RawImage image;
        public Vector3 cameraOffset; // 相机偏移
        public Vector3 cameraAngle; // 相机角度

        public bool orthographic; // 相机是否是2D相机

        public bool canRotate; // 是否可以旋转
        public bool canScale; // 是否可以缩放
        public bool isOffsetY; // 是否偏移Y轴
        public bool isClear = true; // 是否删除
    }

    [LogicOf("Render3D2UI", AssetPath.Global)]
    public class Render3D2UILogic : SingletonEvent<Render3D2UILogic>, ILogic
    {
        private Render3D2UIView View => ViewMaster.Instance.GetView<Render3D2UIView>();

        private RenderTexture RenderTexture { get; set; }

        public Render3D2UILogic()
        {
            AddEventMsg<object>("OpenRender3D2UIView", OpenRender3D2UIView);
            AddEventMsg("CloseRender3D2UIView", CloseRender3D2UIView);

            AddEventMsg<bool>("SetRenderRotationEnable", SetRenderRotationEnable);
            AddEventMsg<bool>("SetRenderScaleEnable", SetRenderScaleEnable);

            AddEventMsg<Vector3, float, Action>("SetRenderCameraOffsetAnimation", SetRenderCameraOffsetAnimation);
            AddEventMsg<Vector3>("SetRenderCameraOffset", SetRenderCameraOffset);
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

        public void SetRenderCameraOffsetAnimation(Vector3 offset, float time, Action callback)
        {
            View.RenderCamera.transform.DOLocalMove(offset, time).SetEase(Ease.OutExpo).OnComplete(() => { callback?.Invoke(); });
        }

        public void SetRenderCameraOffset(Vector3 offset)
        {
            View.RenderCamera.transform.localPosition = offset;
        }

        public void SetRenderRotationEnable(bool enable)
        {
            _renderData.canRotate = enable;
        }

        public void SetRenderScaleEnable(bool enable)
        {
            _renderData.canScale = enable;
        }

        private RenderData _renderData;

        private const float RotateSpeed = 40;

        private void OnDragModel(GameObject model, Vector2 delta)
        {
            var touchCount = 1;
#if !UNITY_EDITOR
            touchCount = Input.touchCount;
#endif
            // 单指旋转
            if (touchCount == 1)
            {
                if (_renderData.canRotate)
                {
                    model.transform.Rotate(0, -delta.x * RotateSpeed * Time.deltaTime, 0);
                }
            }
            // 双指缩放
            if (touchCount == 2)
            {
                if (_renderData.canScale)
                {
                    var touchZero = Input.GetTouch(0);
                    var touchOne = Input.GetTouch(1);
                    var touchZeroPrev = touchZero.position - touchZero.deltaPosition;
                    var touchOnePrev = touchOne.position - touchOne.deltaPosition;
                    var prevMagnitude = (touchZeroPrev - touchOnePrev).magnitude;
                    var currentMagnitude = (touchZero.position - touchOne.position).magnitude;
                    var difference = currentMagnitude - prevMagnitude;
                    model.transform.localScale += Vector3.one * difference * 0.005f;
                    model.transform.localScale = Vector3.one * Mathf.Clamp(model.transform.localScale.x, 1f, 3f);
                    if (_renderData.isOffsetY)
                    {
                        model.transform.localPosition -= Vector3.up * difference * 0.0075f;
                        model.transform.localPosition = Vector3.up * Mathf.Clamp(model.transform.localPosition.y, -3f, 0f);
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void Update(float time)
        {
            if (_renderData.canScale)
            {
                if (View.ModelTransform.childCount <= 0) return;
                var scale = Input.GetAxis("Mouse ScrollWheel");
                View.ModelTransform.GetChild(0).localScale += Vector3.one * scale * 1f;
                View.ModelTransform.GetChild(0).localScale = Vector3.one * Mathf.Clamp(View.ModelTransform.GetChild(0).localScale.x, 1f, 3f);

                if (_renderData.isOffsetY)
                {
                    View.ModelTransform.GetChild(0).localPosition -= Vector3.up * scale * 1.5f;
                    View.ModelTransform.GetChild(0).localPosition = Vector3.up * Mathf.Clamp(View.ModelTransform.GetChild(0).localPosition.y, -3f, -0f);
                }
            }
        }
#endif

        #endregion

        #region View Logic

#if UNITY_EDITOR
        private int _timeId;
#endif

        private void OpenRender3D2UIView(object obj)
        {
            // View.transform.localPosition = Vector3.down * 1000f;
            View.RenderCamera.SetGameObjectActive();

            if (obj is RenderData data)
            {
                _renderData = data;
                // 相机设置
                var width = (int)data.image.rectTransform.rect.width;
                var height = (int)data.image.rectTransform.rect.height;
                RenderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                RenderTexture.active = RenderTexture;
                View.RenderCamera.targetTexture = RenderTexture;
                View.RenderCamera.clearFlags = CameraClearFlags.SolidColor;
                View.RenderCamera.backgroundColor = Color.clear;
                View.RenderCamera.orthographic = data.orthographic;
                View.RenderCamera.transform.SetLocalPositionAndRotation(data.cameraOffset, Quaternion.Euler(data.cameraAngle));
                View.RenderCamera.Render();
                View.OpenView();
                // 模型设置位置
                data.gameObject.transform.SetParent(View.ModelTransform);
                data.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                data.image.texture = RenderTexture;
                // 事件绑定
                EventListener.Get(data.image).onDrag = (go, delta) => { OnDragModel(data.gameObject, delta); };
            }
            else
            {
                Log.W("OpenRender3D2UIView obj is not RenderData");
            }
#if UNITY_EDITOR
            _timeId = TimeUpdateMaster.Instance.StartTimer(Update);
#endif
        }
        private void CloseRender3D2UIView()
        {
            if (!View) return;
            View.RenderCamera.SetGameObjectActive(false);
            View.RenderCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(RenderTexture);
            if (_renderData.isClear)
            {
                foreach (Transform child in View.ModelTransform)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
#if UNITY_EDITOR
            TimeUpdateMaster.Instance.EndTimer(_timeId);
#endif
        }

        #endregion
    }
}