using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using App.Core.Helper;
using App.Core.Tools;
using Cysharp.Threading.Tasks;

namespace App.Core.Master
{
    public class ViewMaster : SingletonMono<ViewMaster>
    {
        #region Private Variable
        private GameObject GameObjectRoot; //3D游戏对象父物体

        private static readonly Dictionary<string, ViewBase> ViewPairs = new Dictionary<string, ViewBase>();
        private readonly Dictionary<string, Transform> GoNodePairs = new Dictionary<string, Transform>();
        private readonly List<RectTransform> UIPanels = new List<RectTransform>();

        private readonly Dictionary<RedDotMold, int> _redDotMap = new Dictionary<RedDotMold, int>();
        private readonly Dictionary<RedDotMold, RedDotView> _redDotViewMap = new Dictionary<RedDotMold, RedDotView>();

        private GameObject Canvas2D; // Canvas2D游戏对象
        private GameObject SafeArea2D; // 2DUI安全区对象
        private GameObject Background; // 背景图片对象

        private Image BackgroundImage => Background.GetComponent<Image>();
        private AspectRatioFitter AspectRatioFitter => Background.GetComponent<AspectRatioFitter>();

        private GameObject Canvas3D; // Canvas3D游戏对象

        #endregion

        #region Public Variable

        #region Canvas2D

        /// <summary> Canvas2D组件 </summary>
        public Canvas UI2DCanvas => Canvas2D.GetComponent<Canvas>();

        /// <summary> 获取Canvas2D根RectTransform </summary>
        public RectTransform UI2DRectTransform => Canvas2D.GetComponent<RectTransform>();

        /// <summary> 获取Canvas2DScaler组件 </summary>
        public CanvasScaler UI2DCanvasScaler => Canvas2D.GetComponent<CanvasScaler>();

        /// <summary> 获取Canvas2D GraphicRaycaster组件 </summary>
        public GraphicRaycaster UI2DGraphicRaycaster => Canvas2D.GetComponent<GraphicRaycaster>();

        /// <summary> 获取UIRoot </summary>
        public RectTransform UISafeArea2D => SafeArea2D.GetComponent<RectTransform>();

        /// <summary> 获取UIPanels </summary>
        public List<RectTransform> UI2DPanels => UIPanels;

        #endregion

        #region Canvas3D

        /// <summary> Canvas3D组件 </summary>
        public Canvas UI3DCanvas => Canvas3D.GetComponent<Canvas>();

        /// <summary> 获取Canvas3D根RectTransform </summary>
        public RectTransform UI3DRectTransform => Canvas3D.GetComponent<RectTransform>();

        /// <summary> 获取Canvas2DScaler组件 </summary>
        public CanvasScaler UI3DCanvasScaler => Canvas3D.GetComponent<CanvasScaler>();

        /// <summary> 获取Canvas3D GraphicRaycaster组件 </summary>
        public GraphicRaycaster UI3DGraphicRaycaster => Canvas3D.GetComponent<GraphicRaycaster>();

        #endregion

        /// <summary> 获取3D游戏对象根对象(全局) </summary>
        public Transform GoRoot => GameObjectRoot.transform;

        #endregion

        /// <summary>
        /// UIRoot脚本初始化
        /// </summary>
        private void Awake()
        {
            Canvas2D = this.FindGameObject("UI Root/2D Canvas");
            Background = this.FindGameObject("UI Root/2D Canvas/Background/Image");
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
            var go = AssetsMaster.Instance.LoadAssetSync<GameObject>(attribute.Location);
            if (!go) return null;
            var parent = attribute.View switch
            {
                ViewMold.UI2D => UIPanels[attribute.Layer],
                ViewMold.UI3D => UI3DRectTransform,
                ViewMold.Go3D => GoRoot,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.View), attribute.View, null)
            };
            var view = Instantiate(go, parent);
            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one;
            view.name = view.name.Replace("(Clone)", "");
            var vb = view.AddComponent(type) as ViewBase;
            EventDispatcher.TriggerEvent(view.name, attribute.Active);
            return vb;
        }

        private void RefreshRedDotView()
        {
            foreach (var kvp in _redDotViewMap)
            {
                var count = _redDotMap.Where(data => (kvp.Key & data.Key) != 0).Sum(data => data.Value);
                kvp.Value.Refresh(count);
            }
        }

        #endregion

        #region Public Function

        /// <summary>
        /// 切换屏幕方向
        /// </summary>
        /// <param name="orientation">0: Portrait, 1: LandscapeLeft, 2: LandscapeLeft(AutoRotation)</param>
        public async UniTask SwitchScreen(int orientation)
        {
#if UNITY_EDITOR
            ChangeGameViewResolution(orientation);
#endif
            switch (orientation)
            {
                case 0:
                    Screen.orientation = ScreenOrientation.Portrait;
                    break;
                case 1:
                    Screen.orientation = ScreenOrientation.LandscapeLeft;
                    break;
                case 2:
                    Screen.orientation = ScreenOrientation.LandscapeLeft;
                    Screen.orientation = ScreenOrientation.AutoRotation;
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    Screen.autorotateToPortrait = false;
                    Screen.autorotateToPortraitUpsideDown = false;
                    break;
                default:
                    Screen.orientation = ScreenOrientation.Portrait;
                    break;
            }
            await UniTask.WaitForEndOfFrame(this);
            SafeAreaAdjuster();
        }

#if UNITY_EDITOR
        private static void ChangeGameViewResolution(int orientation)
        {
            var index = orientation == 0 ? 24 : 23;
            var assembly = typeof (UnityEditor.EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var gameView = UnityEditor.EditorWindow.GetWindow(type);
            var method = type.GetMethod("SizeSelectionCallback");

            method?.Invoke(gameView, new object[2] { index, null });
        }
        private void OnDestroy()
        {
            ChangeGameViewResolution(0);
        }
#endif

        public void InitBackgroundImage(Sprite sprite = null)
        {
            Background.SetActive(sprite);
            if (!sprite) return;
            BackgroundImage.sprite = sprite;
            var ratio = Screen.width > Screen.height ?
                sprite.rect.height / sprite.rect.width :
                sprite.rect.width / sprite.rect.height;
            AspectRatioFitter.aspectRatio = ratio;
        }

        public void InitViewScripts()
        {
            var types = Utils.GetAssemblyTypes<ViewBase>();
            foreach (var type in types)
            {
                if (ViewPairs.ContainsKey(type.FullName!)) continue;
                var obj = type.GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
                if (obj is not ViewOfAttribute attribute) continue;
                var view = CreateView(type, attribute);
                ViewPairs.Add(type.FullName!, view);
            }
            InitRedDotView();
        }

        public void InitRedDotView()
        {
            var redDotViews = gameObject.GetComponentsInChildren<RedDotView>(true);
            foreach (var redDotView in redDotViews)
            {
                Log.I(redDotView);
                _redDotViewMap.TryAdd(redDotView.RedDotMold, redDotView);
            }
        }

        public void AddRedDotView(GameObject target, RedDotMold mold = RedDotMold.SystemMail, bool showCount = true, RedDotAnchor anchor = RedDotAnchor.UpperRight, Vector2 offset = default, int size = 30)
        {
            var view = target.GetOrAddComponent<RedDotView>();
            view.RedDotMold = mold;
            view.ShowCount = showCount;
            _redDotViewMap.TryAdd(view.RedDotMold, view);
        }

        public T AddView<T>(GameObject go, ViewMold mold = ViewMold.UI2D, int layer = 0, bool state = false) where T : Component
        {
            if (!go) return null;
            var parent = mold switch
            {
                ViewMold.UI2D => UIPanels[layer],
                ViewMold.UI3D => UI3DRectTransform,
                ViewMold.Go3D => GoRoot,
                _ => throw new ArgumentOutOfRangeException(nameof(mold), mold, null)
            };
            var view = Instantiate(go, parent);
            view.transform.localEulerAngles = Vector3.zero;
            view.transform.localScale = Vector3.one;
            view.name = view.name.Replace("(Clone)", "");
            var t = view.AddComponent(typeof(T)) as T;

            EventDispatcher.TriggerEvent(view.name, state);
            ViewPairs.Add(typeof(T).FullName!, t as ViewBase);
            return t;
        }

        public T AddView<T>(GameObject go, Transform parent, bool state = false) where T : Component
        {
            if (!go) return null;
            var viewparent = !parent ? UIPanels[0] : parent;
            var view = Instantiate(go, viewparent);
            view.transform.localEulerAngles = Vector3.zero;
            view.transform.localScale = Vector3.one;
            view.name = view.name.Replace("(Clone)", "");
            var t = view.AddComponent(typeof(T)) as T;

            EventDispatcher.TriggerEvent(view.name, state);
            ViewPairs.Add(typeof(T).FullName!, t as ViewBase);
            return t;
        }

        public void SafeAreaAdjuster()
        {
            UI2DCanvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            UI2DCanvasScaler.matchWidthOrHeight = Screen.width < Screen.height ? 0 : 1;

            var bottomPixels = Screen.safeArea.y;
            var leftPixels = Screen.safeArea.x;

            var topPixel = Screen.safeArea.y + Screen.safeArea.height - Screen.height;
            var rightPixel = Screen.safeArea.x + Screen.safeArea.width - Screen.width;

            UISafeArea2D.offsetMin = new Vector2(leftPixels, bottomPixels);
            UISafeArea2D.offsetMax = new Vector2(rightPixel, topPixel);

            InitBackgroundImage(BackgroundImage.sprite);
        }

        /// <summary> UGUI坐标 mousePosition</summary>
        public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
        {
            var cam = UI2DCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : UI2DCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UI2DRectTransform, mousePosition, cam, out var position);
            return position;
        }

        public void OpenView<T>() where T : ViewBase
        {
            var view = GetView<T>();
            if (view == null)
            {
                Log.W($"View {typeof(T).FullName} has no view");
                return;
            }
            view.SetViewActive();
        }

        public void OpenView<T>(object obj) where T : ViewBase
        {
            var view = GetView<T>();
            if (view == null)
            {
                Log.W($"View {typeof(T).FullName} has no view");
                return;
            }
            view.SetViewActive();
        }

        public void CLoseView<T>(bool isClear = false) where T : ViewBase
        {
            var view = GetView<T>();
            if (view == null)
            {
                Log.W($"View {typeof(T).FullName} has no view");
                return;
            }
            if (isClear)
            {
                RemoveView(view);
            }
            else
            {
                view.SetViewActive(false);
            }
        }

        public T GetView<T>() where T : ViewBase
        {
            var type = typeof(T);
            var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
            if (ViewPairs.ContainsKey(scriptName!)) return ViewPairs[scriptName] as T;
            var obj = type.GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
            if (obj is ViewOfAttribute attribute) return CreateView(type, attribute) as T;
            Log.W($"View {type.FullName} has no {nameof(T)}");
            return null;
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

        public void RefreshRedDotCount(RedDotMold mold, int count)
        {
            _redDotMap.TryAdd(mold, count);
            _redDotMap[mold] = count;
            RefreshRedDotView();
        }

        public void AddRedDotCount(RedDotMold mold, int count)
        {
            if (!_redDotMap.TryAdd(mold, count))
            {
                _redDotMap[mold] += count;
            }
            RefreshRedDotView();
        }

        public void SubRedDotCount(RedDotMold mold, int count)
        {
            _redDotMap.TryAdd(mold, count);
            if (_redDotMap[mold] < count)
            {
                _redDotMap[mold] = 0;
            }
            else
            {
                _redDotMap[mold] -= count;
            }
            RefreshRedDotView();
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