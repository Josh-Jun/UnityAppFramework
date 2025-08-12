/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月4 13:31
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace App.Modules
{
    public class RenderData
    {
        public GameObject prefab;
        public RawImage image;
    }

    [LogicOf(AssetPath.Global)]
    public class Render3D2UILogic : SingletonEvent<Render3D2UILogic>, ILogic
    {
        private Render3D2UIView View => ViewMaster.Instance.GetView<Render3D2UIView>();

        private RenderTexture RenderTexture { get; set; }

        public Render3D2UILogic()
        {
            AddEventMsg<object>("OpenRender3D2UIView", OpenRender3D2UIView);
            AddEventMsg("CloseRender3D2UIView", CloseRender3D2UIView);

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

        private const float RotateSpeed = 40;

        private void AddModel(GameObject prefab, RawImage image)
        {
            View.OpenView();
            var model = Object.Instantiate(prefab, View.ModelTransform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            image.texture = RenderTexture;
            EventListener.Get(image).onDrag = (go, delta) => { OnDragModel(model, delta); };
        }
        private void OnDragModel(GameObject model, Vector2 delta)
        {
            model.transform.Rotate(0, -delta.x * RotateSpeed * Time.deltaTime, 0);
        }

        #endregion

        #region View Logic

        private void OpenRender3D2UIView(object obj)
        {
            View.RenderCamera.SetGameObjectActive();

            if (obj is RenderData data)
            {
                var width = (int)data.image.rectTransform.rect.width;
                var height = (int)data.image.rectTransform.rect.height;
                RenderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                RenderTexture.active = RenderTexture;
                View.RenderCamera.targetTexture = RenderTexture;
                View.RenderCamera.clearFlags = CameraClearFlags.SolidColor;
                View.RenderCamera.backgroundColor = Color.clear;
                View.RenderCamera.Render();
                AddModel(data.prefab, data.image);
            }
            else
            {
                Log.W("OpenRender3D2UIView obj is not RenderData");
            }
        }
        private void CloseRender3D2UIView()
        {
            if (!View) return;
            View.RenderCamera.SetGameObjectActive(false);
            View.RenderCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(RenderTexture);
            foreach (Transform child in View.ModelTransform)
            {
                Object.Destroy(child.gameObject);
            }
        }

        #endregion
    }
}