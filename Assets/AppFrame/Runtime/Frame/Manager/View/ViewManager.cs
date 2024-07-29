using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using AppFrame.Config;
using AppFrame.Tools;
using UnityEngine.InputSystem.UI;
#if PICO_XR_SETTING
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

namespace AppFrame.View
{
    public class ViewManager : SingletonMono<ViewManager>
    {
        #region Private Variable

        private GameObject CanvasObject; //Canvas游戏对象
        private GameObject UIRootObject; //UIView Root跟对象
        private GameObject EventSystemObject; //EventSystem游戏对象
        private GameObject GameObjectRoot; //3D游戏对象父物体

        private static Dictionary<string, ViewBase> viewPairs = new Dictionary<string, ViewBase>();
        private Dictionary<string, Transform> rootPairs = new Dictionary<string, Transform>();
        private List<RectTransform> uiPanels = new List<RectTransform>();

        #endregion

        #region Public Variable

        /// <summary> Canvas组件 </summary>
        public Canvas UICanvas
        {
            get { return CanvasObject.GetComponent<Canvas>(); }
            private set { }
        }

        /// <summary> 获取Canvas根RectTransform </summary>
        public RectTransform UIRectTransform
        {
            get { return CanvasObject.GetComponent<RectTransform>(); }
            private set { }
        }

        /// <summary> 获取CanvasScaler组件 </summary>
        public CanvasScaler UICanvasScaler
        {
            get { return CanvasObject.GetComponent<CanvasScaler>(); }
            private set { }
        }

        /// <summary> 获取GraphicRaycaster组件 </summary>
        public GraphicRaycaster UIGraphicRaycaster
        {
            get { return CanvasObject.GetComponent<GraphicRaycaster>(); }
            private set { }
        }

        /// <summary> 获取3D游戏对象根对象(全局) </summary>
        public Transform GoRoot
        {
            get { return GameObjectRoot.transform; }
            private set { }
        }

        /// <summary> 获取UIRoot </summary>
        public RectTransform UIRoot
        {
            get { return UIRootObject.GetComponent<RectTransform>(); }
            private set { }
        }
        
        /// <summary> 获取UIPanels </summary>
        public List<RectTransform> UIPanels
        {
            get { return uiPanels; }
            private set { }
        }

        #endregion

        /// <summary>
        /// UIRoot脚本初始化
        /// </summary>
        void Awake()
        {
            #region UI Canvas

            CanvasObject = new GameObject("UI Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            CanvasObject.transform.SetParent(transform);
            CanvasObject.layer = 5;

            UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            UICanvas.worldCamera = Camera.main;
            UICanvas.vertexColorAlwaysGammaSpace = true;

            UICanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            UICanvasScaler.referenceResolution = Global.AppConfig.UIReferenceResolution;
            UICanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            UICanvasScaler.matchWidthOrHeight =
                Global.AppConfig.UIReferenceResolution.x < Global.AppConfig.UIReferenceResolution.y ? 0 : 1;

            #endregion

            GameObjectRoot = new GameObject("Go Root");
            GameObjectRoot.transform.SetParent(transform);
            
            UIRootObject = new GameObject("UI Root", typeof(RectTransform));
            UIRootObject.transform.SetParent(UIRectTransform);
            UIRootObject.layer = 5;
            
            #region EventSystem

            if (EventSystem.current == null)
            {
                Type t = null;
#if PICO_XR_SETTING
                t = AppInfo.AppConfig.TargetPackage == TargetPackage.Mobile
                    ? typeof(InputSystemUIInputModule)
                    : typeof(XRUIInputModule);
#endif
                t = typeof(InputSystemUIInputModule);
                EventSystemObject = new GameObject("EventSystem", typeof(EventSystem), t);
                EventSystemObject.transform.SetParent(transform);
            }

            #endregion

#if PICO_XR_SETTING
            if (AppInfo.AppConfig.TargetPackage == TargetPackage.XR)
            {
                Init3DUIRoot(Camera.main);
                Reset3DUIRoot();
            }
#endif
            SafeAreaAdjuster(Global.AppConfig.UIOffset);
            
            InitUIPanel();
        }

        #region Private Function

        private void SafeAreaAdjuster(MarginOffset offset)
        {
            UIRoot.anchorMin = Vector2.zero;
            UIRoot.anchorMax = Vector2.one;
            UIRoot.offsetMin = new Vector2(-offset.Right, offset.Bottom);
            UIRoot.offsetMax = new Vector2(offset.Left, -offset.Top);
        }

        private void InitUIPanel()
        {
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                GameObject go = new GameObject($"UIPanel{index}",typeof(RectTransform));
                go.transform.SetParent(UIRoot);
                go.layer = 5;
                RectTransform rt = go.transform as RectTransform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                uiPanels.Add(rt);
            }
        }

        #endregion

        #region Public Function

#if PICO_XR_SETTING
        public void Init3DUIRoot(Camera camera3d = null)
        {
            if (camera3d == null) return;
            UICanvas.renderMode = RenderMode.WorldSpace;
            UIRectTransform.localScale = Vector3.one * 0.0025f;
            UIRectTransform.sizeDelta = new Vector2(1920, 1080);
            UICanvas.worldCamera = camera3d;
            UICanvasScaler.referencePixelsPerUnit = 100;
            UICanvas.TryGetComponent<TrackedDeviceGraphicRaycaster>();
        }

        public void Reset3DUIRoot(float dis = 5f, float hight = 1f, Camera camera3d = null)
        {
            Camera camera = camera3d == null ? Camera.main : camera3d;
            Vector3 target = new Vector3(0, hight, dis);
            UIRectTransform.transform.position = target;
            UIRectTransform.transform.eulerAngles = camera.transform.eulerAngles;
        }
#endif
        /// <summary> UGUI坐标 mousePosition</summary>
        public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UIRectTransform, mousePosition, Camera.main,
                out Vector2 position);
            return position;
        }

        /// <summary> 判断是否点中UI </summary>
        public bool CheckUIRaycastObjects(int layer = 0)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
            {
                eventData.pressPosition = Input.mousePosition;
                eventData.position = Input.mousePosition;
            }
            else if (Application.platform == RuntimePlatform.Android ||
                     Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (Input.touchCount > 0)
                {
                    eventData.pressPosition = Input.GetTouch(0).position;
                    eventData.position = Input.GetTouch(0).position;
                }
            }

            List<RaycastResult> list = new List<RaycastResult>();
            this.UIGraphicRaycaster.Raycast(eventData, list);
            return list.Count > layer;
        }

        public void AddView<T>(ViewBase view)
        {
            var type = typeof(T);
            var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
            if (!viewPairs.ContainsKey(scriptName))
            {
                viewPairs.Add(scriptName, view);
            }
        }

        public T GetView<T>() where T : class
        {
            var type = typeof(T);
            var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
            if (!viewPairs.ContainsKey(scriptName))
            {
                return null;
            }

            return viewPairs[scriptName] as T;
        }

        public void RemoveView(ViewBase view)
        {
            var scriptName = view.GetType().FullName;
            if (viewPairs.ContainsKey(scriptName))
            {
                GameObject go = viewPairs[scriptName].gameObject;
                Destroy(go);
                viewPairs.Remove(scriptName);
            }
        }

        /// <summary> 添加3D对象预制体，返回GameObject </summary>
        public Transform TryGetEmptyNode(string name)
        {
            if (!rootPairs.ContainsKey(name))
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(GoRoot, false);
                rootPairs.Add(name, go.transform);
                return go.transform;
            }
            else
            {
                return rootPairs[name];
            }
        }

        #endregion
    }
}