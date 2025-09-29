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
    public class StepData
    {
        public string ViewName;
        public object obj;
    }

    public class ViewMaster : SingletonMono<ViewMaster>
    {
        #region Private Variable

        private static readonly Dictionary<string, ViewBase> ViewPairs = new Dictionary<string, ViewBase>();

        private static readonly Stack<StepData> ViewStepStack = new();

        #region Go3D

        private GameObject GameObjectRoot; //3D游戏对象父物体
        private readonly Dictionary<string, Transform> GoNodePairs = new Dictionary<string, Transform>();

        #endregion

        #region UI2D

        private readonly Dictionary<RedDotMold, int> _redDotMap = new Dictionary<RedDotMold, int>();
        private readonly Dictionary<RedDotMold, RedDotView> _redDotViewMap = new Dictionary<RedDotMold, RedDotView>();

        private GameObject Canvas2D; // Canvas2D游戏对象
        private GameObject SafeArea2D; // 2DUI安全区对象
        private GameObject Background2D; // 背景图片对象

        private readonly List<RectTransform> UIPanels = new();

        private Image BackgroundImage2D => Background2D.GetComponent<Image>();
        private AspectRatioFitter AspectRatioFitter => Background2D.GetComponent<AspectRatioFitter>();

        #endregion

        #region UI3D

        private GameObject Canvas3D; // Canvas3D游戏对象

        private readonly List<RectTransform> RectTransform3Ds = new();
        private readonly List<Canvas> Canvas3Ds = new();
        private readonly List<CanvasScaler> CanvasScaler3Ds = new();
        private readonly List<GraphicRaycaster> GraphicRaycaster3Ds = new();

        #endregion

        #endregion

        #region Public Variable

        #region Canvas2D

        public Camera UICamera2D => UICanvas2D.worldCamera;

        /// <summary> Canvas2D组件 </summary>
        public Canvas UICanvas2D => Canvas2D.GetComponent<Canvas>();

        /// <summary> 获取Canvas2D根RectTransform </summary>
        public RectTransform UIRectTransform2D => Canvas2D.GetComponent<RectTransform>();

        /// <summary> 获取Canvas2DScaler组件 </summary>
        public CanvasScaler UICanvasScaler2D => Canvas2D.GetComponent<CanvasScaler>();

        /// <summary> 获取Canvas2D GraphicRaycaster组件 </summary>
        public GraphicRaycaster UIGraphicRaycaster2D => Canvas2D.GetComponent<GraphicRaycaster>();

        /// <summary> 获取UIRoot </summary>
        public RectTransform UISafeArea2D => SafeArea2D.GetComponent<RectTransform>();

        /// <summary> 获取UIPanels </summary>
        public List<RectTransform> UIPanels2Ds => UIPanels;

        #endregion

        #region Canvas3D

        /// <summary> Canvas3D父物体 </summary>
        public Transform UIRoot3D => Canvas3D.transform;

        /// <summary> UI3D相机（一般为主相机） </summary>
        public Camera UICamera3D => UICanvas3Ds[0].worldCamera;

        /// <summary> 所有Canvas3D根RectTransform </summary>
        public List<RectTransform> UIRectTransform3Ds => RectTransform3Ds;

        /// <summary> 所有Canvas3D组件 </summary>
        public List<Canvas> UICanvas3Ds => Canvas3Ds;

        /// <summary> 所有Canvas3D CanvasScaler组件 </summary>
        public List<CanvasScaler> UICanvasScaler3Ds => CanvasScaler3Ds;

        /// <summary> 所有Canvas3D GraphicRaycaster组件 </summary>
        public List<GraphicRaycaster> UIGraphicRaycaster3Ds => GraphicRaycaster3Ds;

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
            Background2D = this.FindGameObject("UI Root/2D Canvas/Background/Image");
            Canvas3D = this.FindGameObject("UI Root/3D Canvas");

            SafeArea2D = this.FindGameObject("UI Root/2D Canvas/Safe Area");

            GameObjectRoot = this.FindGameObject("Go Root");

            InitUICanvas();

            SafeAreaAdjuster();

            InitBackgroundImage(BackgroundImage2D.sprite);
        }

        #region Private Function

        private const int Panel2DCount = 8;
        private const int Canvas3DCount = 2;

        private void InitUICanvas()
        {
            #region UI2D

            var item2D = UISafeArea2D.GetChild(0).gameObject;
            foreach (RectTransform panel in UISafeArea2D)
            {
                UIPanels.Add(panel);
            }

            for (var i = UISafeArea2D.childCount; i < Panel2DCount; i++)
            {
                var go = Instantiate(item2D, UISafeArea2D);
                var p = go.GetComponent<RectTransform>();
                UIPanels.Add(p);
                go.name = $"Panel[{i}]";
            }

            #endregion

            #region UI3D

            var item3D = UIRoot3D.GetChild(0).gameObject;
            foreach (RectTransform rectTransform in UIRoot3D)
            {
                RectTransform3Ds.Add(rectTransform);
                var canvas = rectTransform.GetComponent<Canvas>();
                Canvas3Ds.Add(canvas);
                var canvasScaler = rectTransform.GetComponent<CanvasScaler>();
                CanvasScaler3Ds.Add(canvasScaler);
                var graphicRaycaster = rectTransform.GetComponent<GraphicRaycaster>();
                GraphicRaycaster3Ds.Add(graphicRaycaster);
            }

            for (var i = UIRoot3D.childCount; i < Canvas3DCount; i++)
            {
                var go = Instantiate(item3D, UIRoot3D);
                var rectTransform = go.GetComponent<RectTransform>();
                RectTransform3Ds.Add(rectTransform);
                go.name = $"Canvas[{i}]";
                var canvas = go.GetComponent<Canvas>();
                Canvas3Ds.Add(canvas);
                var canvasScaler = go.GetComponent<CanvasScaler>();
                CanvasScaler3Ds.Add(canvasScaler);
                var graphicRaycaster = go.GetComponent<GraphicRaycaster>();
                GraphicRaycaster3Ds.Add(graphicRaycaster);
            }

            #endregion
        }

        private ViewBase CreateView(Type type, ViewOfAttribute attribute)
        {
            var go = AssetsMaster.Instance.LoadAssetSync<GameObject>(attribute.Location);
            if (!go) return null;
            var layer = attribute.View switch
            {
                ViewMold.UI2D => Mathf.Clamp(attribute.Layer, 0, UIPanels2Ds.Count - 1),
                ViewMold.UI3D => Mathf.Clamp(attribute.Layer, 0, UIRectTransform3Ds.Count - 1),
                ViewMold.Go3D => attribute.Layer,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.View), attribute.View, null)
            };
            var parent = attribute.View switch
            {
                ViewMold.UI2D => UIPanels2Ds[layer],
                ViewMold.UI3D => UIRectTransform3Ds[layer],
                ViewMold.Go3D => GoRoot,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.View), attribute.View, null)
            };
            var view = Instantiate(go, parent);
            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one;
            view.name = view.name.Replace("(Clone)", "");
            var layerName = attribute.View switch
            {
                ViewMold.UI2D => "UI",
                ViewMold.UI3D => "UI3D",
                ViewMold.Go3D => "Default",
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.View), attribute.View, null)
            };
            view.SetLayer(layerName);
            var vb = view.AddComponent(type) as ViewBase;
            view.SetActive(false);
            ViewPairs.Add(type.FullName!, vb);
            if (attribute.Active) vb?.OpenView();
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

        private void AddViewStep(ViewBase view, object args)
        {
            var obj = view.GetType().GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
            if (obj is not ViewOfAttribute attribute) return;
            if (attribute.Name is "Ask" or "Update" or "Loading") return;
            ViewStepStack.Push(new StepData()
            {
                ViewName = view.GetType().FullName,
                obj = obj
            });
        }

        private void RemoveViewStep(ViewBase view)
        {
            var obj = view.GetType().GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
            if (obj is not ViewOfAttribute attribute) return;
            if (attribute.Name is "Ask" or "Update" or "Loading") return;
            
            if (ViewStepStack.Count > 0)
            {
                ViewStepStack.Pop();
            }
        }

        #endregion

        #region Public Function

        public void PutUICanvas3D(GameObject canvas3D)
        {
            var rectTransform = canvas3D.GetComponent<RectTransform>();
            RectTransform3Ds.Add(rectTransform);
            var canvas = canvas3D.GetComponent<Canvas>();
            Canvas3Ds.Add(canvas);
            var canvasScaler = canvas3D.GetComponent<CanvasScaler>();
            CanvasScaler3Ds.Add(canvasScaler);
            var graphicRaycaster = canvas3D.GetComponent<GraphicRaycaster>();
            GraphicRaycaster3Ds.Add(graphicRaycaster);
        }

        /// <summary>
        /// 切换屏幕方向
        /// </summary>
        /// <param name="orientation">0: Portrait, 1: LandscapeLeft, 2: LandscapeLeft(AutoRotation)</param>
        public async UniTask SwitchScreen(int orientation)
        {
#if UNITY_EDITOR
            const string script = "App.Editor.Helper.EditorHelper";
            const string assembly = "App.Editor";
            const string function = "ChangeGameViewResolution";
            var type = AppHelper.GetAssemblyType(script, assembly);
            var helper = Activator.CreateInstance(type);
            var method = type.GetMethod(function);
            method?.Invoke(helper, new object[] { orientation });
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

            await UniTask.Delay(500);
            SafeAreaAdjuster();
        }

        public void InitBackgroundImage(Sprite sprite = null)
        {
            Background2D.SetActive(sprite);
            if (!sprite) return;
            BackgroundImage2D.sprite = sprite;
            var ratio = sprite.rect.width / sprite.rect.height;
            AspectRatioFitter.aspectRatio = ratio;
        }

        public void InitViewScripts()
        {
            var types = AppHelper.GetAssemblyTypes<ViewBase>();
            foreach (var type in types)
            {
                if (ViewPairs.ContainsKey(type.FullName!)) continue;
                var obj = type.GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
                if (obj is not ViewOfAttribute attribute) continue;
                if (!AppHelper.GetBoolData(attribute.Name)) continue;
                CreateView(type, attribute);
            }

            InitRedDotView();
        }

        public void InitRedDotView()
        {
            var redDotViews = gameObject.GetComponentsInChildren<RedDotView>(true);
            foreach (var redDotView in redDotViews)
            {
                _redDotViewMap.TryAdd(redDotView.RedDotMold, redDotView);
            }
        }

        public void AddRedDotView(GameObject target, RedDotMold mold = RedDotMold.SystemMail, bool showCount = true,
            RedDotAnchor anchor = RedDotAnchor.UpperRight, Vector2 offset = default, int size = 30)
        {
            var view = target.GetOrAddComponent<RedDotView>();
            view.RedDotMold = mold;
            view.ShowCount = showCount;
            _redDotViewMap.TryAdd(view.RedDotMold, view);
        }

        public T AddView<T>(string viewName, string location, ViewMold mold, int layer, bool state) where T : ViewBase
        {
            var type = AppHelper.GetAssemblyType<T>();
            var attribute = new ViewOfAttribute(viewName, mold, location, state, layer);
            if (!AppHelper.GetBoolData(attribute.Name)) return null;
            var view = CreateView(type, attribute);
            return view as T;
        }

        public List<ViewBase> GetAllView()
        {
            return ViewPairs.Values.ToList();
        }

        /// <summary>
        /// 0:上下左右
        /// 1:左右
        /// 2:上下
        /// 3:无
        /// </summary>
        public void SafeAreaAdjuster()
        {
            UICanvasScaler2D.referenceResolution = new Vector2(Screen.width, Screen.height);
            UICanvasScaler2D.matchWidthOrHeight = Screen.width < Screen.height ? 0 : 1;

            var bottomPixels = Screen.safeArea.y;
            var leftPixels = Screen.safeArea.x;

            var topPixel = Screen.safeArea.y + Screen.safeArea.height - Screen.height;
            var rightPixel = Screen.safeArea.x + Screen.safeArea.width - Screen.width;

            UISafeArea2D.offsetMin = new Vector2(leftPixels, bottomPixels);
            UISafeArea2D.offsetMax = new Vector2(rightPixel, topPixel);

            List<(Vector2 min, Vector2 max)> safeAreas = new()
            {
                (Vector2.zero, Vector2.zero),
                (Vector2.down * bottomPixels, Vector2.down * topPixel),
                (Vector2.left * leftPixels, Vector2.left * rightPixel),
                (new Vector2(-leftPixels, -bottomPixels), new Vector2(-rightPixel, -topPixel)),
            };

            for (var i = 0; i < UIPanels.Count; i++)
            {
                var index = i % 4;
                UIPanels[i].offsetMin = safeAreas[index].min;
                UIPanels[i].offsetMax = safeAreas[index].max;
            }
        }

        /// <summary> UGUI坐标 mousePosition</summary>
        public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
        {
            var cam = UICanvas2D.renderMode == RenderMode.ScreenSpaceOverlay ? null : UICanvas2D.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UIRectTransform2D, mousePosition, cam,
                out var position);
            return position;
        }

        public void OpenView<T>(object obj = null) where T : ViewBase
        {
            var view = GetView<T>();
            if (!view)
            {
                Log.W($"View {typeof(T).FullName} has no view");
                return;
            }

            AddViewStep(view, obj);
            view.OpenView(obj);
        }

        public void OpenView(string scriptName, object obj = null)
        {
            var view = GetView(scriptName);
            if (!view)
            {
                Log.W($"View {scriptName} has no view");
                return;
            }

            AddViewStep(view, obj);
            view.OpenView(obj);
        }

        public void CloseView<T>(bool isClear = false) where T : ViewBase
        {
            var view = GetView<T>();
            if (!view)
            {
                Log.W($"View {typeof(T).FullName} has no view");
                return;
            }

            RemoveViewStep(view);

            if (isClear)
            {
                RemoveView(view);
            }
            else
            {
                view.CloseView();
            }
        }

        public void CloseView(string scriptName, bool isClear = false)
        {
            var view = GetView(scriptName);
            if (!view)
            {
                Log.W($"View {scriptName} has no view");
                return;
            }

            RemoveViewStep(view);

            if (isClear)
            {
                RemoveView(scriptName);
            }
            else
            {
                view.CloseView();
            }
        }

        public void CloseAllView(bool isClear = false)
        {
            foreach (var view in ViewPairs)
            {
                if (isClear)
                {
                    RemoveView(view.Value);
                }
                else
                {
                    if (view.Value.ViewActive)
                    {
                        view.Value.CloseView();
                    }
                }
            }
        }

        public T GetView<T>() where T : ViewBase
        {
            var type = AppHelper.GetAssemblyType<T>();
            if (type == null) return null;
            var obj = type.GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
            if (obj is not ViewOfAttribute attribute)
            {
                Log.W($"Get View {type.FullName} is not extends ViewBase");
                return null;
            }

            if (!AppHelper.GetBoolData(attribute.Name)) return null;
            var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
            if (ViewPairs.ContainsKey(scriptName!)) return ViewPairs[scriptName] as T;
            var view = CreateView(type, attribute);
            return view as T;
        }

        public ViewBase GetView(string scriptName)
        {
            var type = AppHelper.GetAssemblyType(scriptName);
            if (type == null) return null;
            var obj = type.GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
            if (obj is not ViewOfAttribute attribute)
            {
                Log.W($"Get View {scriptName} is not extends ViewBase");
                return null;
            }

            if (!AppHelper.GetBoolData(attribute.Name)) return null;
            if (ViewPairs.ContainsKey(scriptName!)) return ViewPairs[scriptName];
            var view = CreateView(type, attribute);
            return view;
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

        public void RemoveView(string scriptName)
        {
            if (!ViewPairs.TryGetValue(scriptName!, out var pair)) return;
            var go = pair.gameObject;
            EventDispatcher.RemoveEventListener(go.name);
            Destroy(go);
            ViewPairs.Remove(scriptName);
        }

        public void GoBack()
        {
            if (ViewStepStack.Count <= 0) return;

            var current = ViewStepStack.Pop();
            var currentView = GetView(current.ViewName);
            if (!currentView)
            {
                Log.W($"View {current.ViewName} has no view");
                return;
            }

            currentView.CloseView();

            if (ViewStepStack.Count <= 0) return;
            var step = ViewStepStack.Peek();
            var view = GetView(step.ViewName);
            if (!view)
            {
                Log.W($"View {step.ViewName} has no view");
                return;
            }

            view.OpenView(step.obj);
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