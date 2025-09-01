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
using Object = UnityEngine.Object;

namespace App.Modules
{
    public class RenderData
    {
        public GameObject Target;
        public RawImage Image;
        public Vector3 FollowOffset; // 跟随偏移
        public Vector3 LookAtOffset; // 看向偏移

        public bool CanRotate; // 是否可以旋转
        public bool CanScale; // 是否可以缩放
        public bool PreserveComposition; // 保持中间显示
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

        private void SetRenderRotationEnable(bool enable)
        {
            _renderData.CanRotate = enable;
        }

        private void SetRenderScaleEnable(bool enable)
        {
            _renderData.CanScale = enable;
        }

        private RenderData _renderData;

        private const float RotateSpeed = 40;

        private void OnDragModel(GameObject model, Vector2 delta)
        {
#if !UNITY_EDITOR
            switch (Input.touchCount)
            {
                // 单指旋转
                case 1:
                    if (_renderData.CanRotate)
                    {
                        model.transform.Rotate(0, -delta.x * RotateSpeed * Time.deltaTime, 0);
                    }
                    break;
                // 双指缩放
                case 2:
                    if (_renderData.CanScale)
                    {
                        var touchZero = Input.GetTouch(0);
                        var touchOne = Input.GetTouch(1);
                        var touchZeroPrev = touchZero.position - touchZero.deltaPosition;
                        var touchOnePrev = touchOne.position - touchOne.deltaPosition;
                        var prevMagnitude = (touchZeroPrev - touchOnePrev).magnitude;
                        var currentMagnitude = (touchZero.position - touchOne.position).magnitude;
                        var difference = currentMagnitude - prevMagnitude;

                        View.CinemachineCinemachineFollowZoom.m_Width -= difference * 0.0005f;
                    }

                    break;
            }
#endif
        }

#if UNITY_EDITOR
        private void Update(float time)
        {
            if (_renderData.CanScale)
            {
                var scale = Input.GetAxis("Mouse ScrollWheel");
                View.CinemachineCinemachineFollowZoom.m_Width -= scale * 10f;
            }

            if (_renderData.CanRotate)
            {
                if (Input.GetMouseButton(0))
                {
                    var delta = Input.GetAxis("Mouse X") * 10f;
                    _renderData.Target.transform.Rotate(0, -delta * RotateSpeed * Time.deltaTime, 0);
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
            View.RenderCamera.SetGameObjectActive();
            View.CinemachineCinemachineFollowZoom.m_Width = 11.5f;
            if (obj is RenderData data)
            {
                _renderData = data;
                // 相机设置
                var width = (int)data.Image.rectTransform.rect.width;
                var height = (int)data.Image.rectTransform.rect.height;
                RenderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                RenderTexture.active = RenderTexture;
                View.RenderCamera.depth = -99;
                View.RenderCamera.targetTexture = RenderTexture;
                View.RenderCamera.clearFlags = CameraClearFlags.SolidColor;
                View.RenderCamera.backgroundColor = Color.clear;
                View.RenderCamera.Render();
                View.OpenView();
                // 设置相机偏移
                View.CinemachineCinemachineVirtualCamera.GetCinemachineComponent<Cinemachine.CinemachineTransposer>().m_FollowOffset = data.FollowOffset;
                View.CinemachineCinemachineCameraOffset.m_Offset = data.LookAtOffset;
                View.CinemachineCinemachineCameraOffset.m_PreserveComposition = data.PreserveComposition;
                // 设置相机目标
                View.CinemachineCinemachineVirtualCamera.Follow = data.Target.transform;
                View.CinemachineCinemachineVirtualCamera.LookAt = data.Target.transform;
                // 设置RawImage
                data.Image.texture = RenderTexture;
                // 事件绑定
                EventListener.Get(data.Image).onDrag = (go, delta) => { OnDragModel(data.Target, delta); };
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
#if UNITY_EDITOR
            TimeUpdateMaster.Instance.EndTimer(_timeId);
#endif
        }

        #endregion
    }
}