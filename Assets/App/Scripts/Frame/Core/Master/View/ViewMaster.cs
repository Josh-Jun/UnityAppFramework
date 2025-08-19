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
            
            InitBackgroundImage(BackgroundImage.sprite);
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
            var layer = Mathf.Clamp(attribute.Layer, 0, UIPanels.Count - 1);
            var parent = attribute.View switch
            {
                ViewMold.UI2D => UIPanels[layer],
                ViewMold.UI3D => UI3DRectTransform,
                ViewMold.Go3D => GoRoot,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.View), attribute.View, null)
            };
            var view = Instantiate(go, parent);
            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one;
            view.name = view.name.Replace("(Clone)", "");
            var vb = view.AddComponent(type) as ViewBase;
            view.SetActive(false);
            ViewPairs.Add(type.FullName!, vb);
            if(attribute.Active) vb?.OpenView();
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
            await UniTask.Delay(500);
            SafeAreaAdjuster();
        }

#if UNITY_EDITOR
        private void ChangeGameViewResolution(int orientation)
        {
            var width = orientation == 0 ? 1170 : 2532;
            var height = orientation == 0 ? 2532 : 1170;
            SetGameViewSize(width, height);
        }
        private void ClearGameViewCustomSize()
        {
            // 获取 GameViewSizes 单例实例
            var gameViewSizesType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
            var scriptableSingletonType = typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
            var instanceProp = scriptableSingletonType.GetProperty("instance");
            var gameViewSizesInstance = instanceProp.GetValue(null, null);

            // 获取当前分组
            var currentGroupProp = gameViewSizesInstance.GetType().GetProperty("currentGroup");
            var currentGroup = currentGroupProp.GetValue(gameViewSizesInstance, null);

            // 获取所有尺寸
            var getTotalCount = currentGroup.GetType().GetMethod("GetTotalCount");
            var getBuiltinCount = currentGroup.GetType().GetMethod("GetBuiltinCount");
            int totalCount = (int)getTotalCount.Invoke(currentGroup, null);
            int builtinCount = (int)getBuiltinCount.Invoke(currentGroup, null);

            // 反向遍历并删除自定义尺寸
            for (int i = totalCount - 1; i >= builtinCount; i--)
            {
                var getGameViewSize = currentGroup.GetType().GetMethod("GetGameViewSize");
                var size = getGameViewSize.Invoke(currentGroup, new object[] { i });

                var sizeType = size.GetType().GetProperty("sizeType");
                var typeValue = (int)sizeType.GetValue(size, null);

                // 类型1是自定义分辨率
                if (typeValue == 1)
                {
                    var removeCustomSize = currentGroup.GetType().GetMethod("RemoveCustomSize");
                    removeCustomSize.Invoke(currentGroup, new object[] { i });
                }
            }
        }
        private void AddGameViewCustomSize(int width, int height, string displayName)
        {
            // 获取 GameViewSizes 单例实例
            var gameViewSizesType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
            var scriptableSingletonType = typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
            var instanceProp = scriptableSingletonType.GetProperty("instance");
            var gameViewSizesInstance = instanceProp.GetValue(null, null);

            // 获取当前分组
            var currentGroupProp = gameViewSizesInstance.GetType().GetProperty("currentGroup");
            var currentGroup = currentGroupProp.GetValue(gameViewSizesInstance, null);

            // 创建新的 GameViewSize
            var gameViewSizeType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameViewSize");
            var sizeTypeEnum = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameViewSizeType");
            var ctor = gameViewSizeType.GetConstructor(
                new Type[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });

            // 1 表示自定义分辨率类型
            var newSize = ctor.Invoke(new object[] { 1, width, height, displayName });

            // 添加新尺寸
            var addCustomSize = currentGroup.GetType().GetMethod("AddCustomSize");
            addCustomSize.Invoke(currentGroup, new object[] { newSize });
            UnityEditor.EditorUtility.RequestScriptReload();
        }
        private void SetGameViewSize(int width, int height)
        {
            var gameViewType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var gameView = UnityEditor.EditorWindow.GetWindow(gameViewType);

            // 查找匹配的分辨率索引
            var gameViewSizesInstance = typeof(UnityEditor.ScriptableSingleton<>)
                .MakeGenericType(typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes"))
                .GetProperty("instance")
                .GetValue(null, null);

            var currentGroup = gameViewSizesInstance.GetType()
                .GetProperty("currentGroup")
                .GetValue(gameViewSizesInstance);

            var getTotalCount = currentGroup.GetType().GetMethod("GetTotalCount");
            int totalCount = (int)getTotalCount.Invoke(currentGroup, null);
            var index = -1;
            for (int i = 0; i < totalCount; i++)
            {
                var getGameViewSize = currentGroup.GetType().GetMethod("GetGameViewSize");
                var gameViewSize = getGameViewSize.Invoke(currentGroup, new object[] { i });

                var sizeWidth = (int)gameViewSize.GetType().GetProperty("width").GetValue(gameViewSize);
                var sizeHeight = (int)gameViewSize.GetType().GetProperty("height").GetValue(gameViewSize);

                if (sizeWidth == width && sizeHeight == height)
                {
                    // 找到匹配的分辨率
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                var str = width > height ? "Landscape" : "Portrait";
                AddGameViewCustomSize(width, height, $"{width}x{height} {str}");
            }

            var setSizeMethod = gameViewType.GetMethod("SizeSelectionCallback");
            setSizeMethod.Invoke(gameView, new object[] { index, null });
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

        public void AddRedDotView(GameObject target, RedDotMold mold = RedDotMold.SystemMail, bool showCount = true, RedDotAnchor anchor = RedDotAnchor.UpperRight, Vector2 offset = default, int size = 30)
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
            UI2DCanvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            UI2DCanvasScaler.matchWidthOrHeight = Screen.width < Screen.height ? 0 : 1;

            var bottomPixels = Screen.safeArea.y;
            var leftPixels = Screen.safeArea.x;

            var topPixel = Screen.safeArea.y + Screen.safeArea.height - Screen.height;
            var rightPixel = Screen.safeArea.x + Screen.safeArea.width - Screen.width;

            UISafeArea2D.offsetMin = new Vector2(leftPixels, bottomPixels);
            UISafeArea2D.offsetMax = new Vector2(rightPixel, topPixel);
            
            // 左右匹配安全区域
            UIPanels[^3].offsetMin = new Vector2(leftPixels, -bottomPixels);
            UIPanels[^3].offsetMax = new Vector2(rightPixel, -topPixel);
            
            // 上下匹配安全区域
            UIPanels[^2].offsetMin = new Vector2(-leftPixels, bottomPixels);
            UIPanels[^2].offsetMax = new Vector2(-rightPixel, topPixel);

            // 不匹配安全区域
            UIPanels[^1].offsetMin = new Vector2(-leftPixels, -bottomPixels);
            UIPanels[^1].offsetMax = new Vector2(-rightPixel, -topPixel);
        }

        /// <summary> UGUI坐标 mousePosition</summary>
        public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
        {
            var cam = UI2DCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : UI2DCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UI2DRectTransform, mousePosition, cam, out var position);
            return position;
        }

        public void OpenView<T>(object obj = null) where T : ViewBase
        {
            var view = GetView<T>();
            if (view == null)
            {
                Log.W($"View {typeof(T).FullName} has no view");
                return;
            }
            view.OpenView(obj);
        }

        public void OpenView(string scriptName, object obj = null)
        {
            var view = GetView(scriptName);
            if (view == null)
            {
                Log.W($"View {scriptName} has no view");
                return;
            }
            view.OpenView(obj);
        }

        public void CloseView<T>(bool isClear = false) where T : ViewBase
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
                view.CloseView();
            }
        }

        public void CloseView(string scriptName, bool isClear = false)
        {
            var view = GetView(scriptName);
            if (view == null)
            {
                Log.W($"View {scriptName} has no view");
                return;
            }
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
            if(type == null) return null;
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
            if(type == null) return null;
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