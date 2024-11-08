using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using AppFrame.Manager;
using AppFrame.Tools;
using AppFrame.Attribute;
using AppFrame.Enum;

namespace AppFrame.View
{
    public class ViewManager : SingletonMono<ViewManager>
    {
        #region Private Variable
        private GameObject GameObjectRoot; //3D游戏对象父物体

        private static Dictionary<string, ViewBase> ViewPairs = new Dictionary<string, ViewBase>();
        private Dictionary<string, Transform> GoNodePairs = new Dictionary<string, Transform>();
        private List<RectTransform> UIPanels = new List<RectTransform>();

        #endregion

        #region Public Variable

        #region Canvas2D
        
        public GameObject Canvas2D; // Canvas2D游戏对象
        public GameObject SafeArea2D; // 2DUI安全区对象

        /// <summary> Canvas2D组件 </summary>
        public Canvas UI2DCanvas
        {
            get { return Canvas2D.GetComponent<Canvas>(); }
            private set { }
        }

        /// <summary> 获取Canvas2D根RectTransform </summary>
        public RectTransform UI2DRectTransform
        {
            get { return Canvas2D.GetComponent<RectTransform>(); }
            private set { }
        }

        /// <summary> 获取Canvas2DScaler组件 </summary>
        public CanvasScaler UI2DCanvasScaler
        {
            get { return Canvas2D.GetComponent<CanvasScaler>(); }
            private set { }
        }

        /// <summary> 获取Canvas2D GraphicRaycaster组件 </summary>
        public GraphicRaycaster UI2DGraphicRaycaster
        {
            get { return Canvas2D.GetComponent<GraphicRaycaster>(); }
            private set { }
        }

        /// <summary> 获取UIRoot </summary>
        public RectTransform UISafeArea2D
        {
            get { return SafeArea2D.GetComponent<RectTransform>(); }
            private set { }
        }
        
        /// <summary> 获取UIPanels </summary>
        public List<RectTransform> UI2DPanels
        {
            get { return UIPanels; }
            private set { }
        }

        #endregion

        #region Canvas3D

        public GameObject Canvas3D; // Canvas3D游戏对象
        
        /// <summary> Canvas3D组件 </summary>
        public Canvas UI3DCanvas
        {
            get { return Canvas3D.GetComponent<Canvas>(); }
            private set { }
        }

        /// <summary> 获取Canvas3D根RectTransform </summary>
        public RectTransform UI3DRectTransform
        {
            get { return Canvas3D.GetComponent<RectTransform>(); }
            private set { }
        }

        /// <summary> 获取Canvas2DScaler组件 </summary>
        public CanvasScaler UI3DCanvasScaler
        {
            get { return Canvas3D.GetComponent<CanvasScaler>(); }
            private set { }
        }

        /// <summary> 获取Canvas3D GraphicRaycaster组件 </summary>
        public GraphicRaycaster UI3DGraphicRaycaster
        {
            get { return Canvas3D.GetComponent<GraphicRaycaster>(); }
            private set { }
        }

        #endregion

        /// <summary> 获取3D游戏对象根对象(全局) </summary>
        public Transform GoRoot
        {
            get { return GameObjectRoot.transform; }
            private set { }
        }
        #endregion

        /// <summary>
        /// UIRoot脚本初始化
        /// </summary>
        private void Awake()
        {
            Canvas2D = this.FindGameObject("UI Root/2D Canvas");
            Canvas3D = this.FindGameObject("UI Root/3D Canvas");
            
            SafeArea2D = this.FindGameObject("UI Root/2D Canvas/Safe Area");

            GameObjectRoot = this.FindGameObject("Go Root");
            
            InitUIPanels();
            
            SafeAreaAdjuster();
        }

        #region Private Function

        private void InitUIPanels()
        {
            foreach (RectTransform panel in UISafeArea2D)
            {
                UIPanels.Add(panel);
            }
        }
        
        private ViewBase CreateView(Type type, ViewOfAttribute attribute)
        {
            var path = $"{attribute.Module}/Views/{type.Name}";
            var go = AssetsManager.Instance.LoadAsset<GameObject>(path);
            if (go == null) return null;
            Transform parent = null;
            switch (attribute.View)
            {
                case ViewMold.UI2D:
                    parent = UIPanels[attribute.Layer];
                    break;
                case ViewMold.UI3D:
                    parent = UI3DRectTransform;
                    break;
                case ViewMold.Go3D:
                    parent = GoRoot;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attribute.View), attribute.View, null);
            }
            var view = Instantiate(go, parent);
            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one;
            view.name = view.name.Replace("(Clone)", "");
            var vb = view.AddComponent(type) as ViewBase;
            EventDispatcher.TriggerEvent(view.name, attribute.Active);
            return vb;
        }
        

        #endregion

        #region Public Function
        
        public void InitViewScripts()
        {
            var types = Utils.GetAssemblyTypes<ViewBase>();
            foreach (var type in types)
            {
                var la = type.GetCustomAttributes(typeof(ViewOfAttribute), false).First();
                if (la is not ViewOfAttribute attribute) continue;
                if(type.FullName == "Modules.Update.UpdateView") continue;
                var view = CreateView(type, attribute);
                ViewPairs.Add(type.FullName!, view);
            }
        }

        public T AddView<T>(GameObject go, ViewMold mold = ViewMold.UI2D, int layer = 0, bool state = false) where T : Component
        {
            if (go == null) return null;
            Transform parent = null;
            switch (mold)
            {
                case ViewMold.UI2D:
                    parent = UIPanels[layer];
                    break;
                case ViewMold.UI3D:
                    parent = UI3DRectTransform;
                    break;
                case ViewMold.Go3D:
                    parent = GoRoot;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mold), mold, null);
            }
            var view = Instantiate(go, parent);
            view.transform.localEulerAngles = Vector3.zero;
            view.transform.localScale = Vector3.one;
            view.name = view.name.Replace("(Clone)", "");
            var t =  view.AddComponent(typeof(T)) as T;

            EventDispatcher.TriggerEvent(view.name, state);
            ViewPairs.Add(typeof(T).FullName!, t as ViewBase);
            return t;
        }
        
        public void SafeAreaAdjuster()
        {
            UI2DCanvasScaler.referenceResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            UI2DCanvasScaler.matchWidthOrHeight = Screen.currentResolution.width < Screen.currentResolution.height ? 0 : 1;
            
            var bottomPixels = Screen.safeArea.y;
            var leftPixels = Screen.safeArea.x;

            var topPixel = Screen.safeArea.y + Screen.safeArea.height - Screen.currentResolution.height;
            var rightPixel = Screen.safeArea.x + Screen.safeArea.width - Screen.currentResolution.width;

            UISafeArea2D.offsetMin = new Vector2(leftPixels, bottomPixels);
            UISafeArea2D.offsetMax = new Vector2(rightPixel, topPixel);
        }
        
        /// <summary> UGUI坐标 mousePosition</summary>
        public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
        {
            var cam = UI2DCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : UI2DCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UI2DRectTransform, mousePosition, cam, out var position);
            return position;
        }

        public T GetView<T>() where T : class
        {
            var type = typeof(T);
            var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
            if (!ViewPairs.ContainsKey(scriptName!))
            {
                return null;
            }
            return ViewPairs[scriptName] as T;
        }

        public void RemoveView(ViewBase view)
        {
            var scriptName = view.GetType().FullName;
            if (!ViewPairs.TryGetValue(scriptName!, out var pair)) return;
            var go = pair.gameObject;
            EventDispatcher.RemoveEventListener(go.name);
            Destroy(go);
            ViewPairs.Remove(scriptName);
        }

        /// <summary> 添加3D对象预制体，返回GameObject </summary>
        public Transform TryGetEmptyNode(string nodeName)
        {
            if (GoNodePairs.TryGetValue(nodeName, out var emptyNode)) return emptyNode;
            var go = new GameObject(nodeName);
            go.transform.SetParent(GoRoot, false);
            GoNodePairs.Add(nodeName, go.transform);
            return go.transform;

        }

        #endregion
    }
}