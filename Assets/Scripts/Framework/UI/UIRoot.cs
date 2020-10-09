using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using EventController;
using System.Reflection;
using XLuaFrame;

public class UIRoot : SingletonMono<UIRoot>
{
    #region Private Variable
    private Camera uiCamera;//UI相机
    private GameObject canvasObject;//Canvas游戏对象
    private GameObject eventSystemObject;//EventSystem游戏对象
    #endregion

    #region Public Variable
    /// <summary> UI相机 </summary>
    public Camera UICamera { get { return uiCamera; } private set { } }
    /// <summary> Canvas组件 </summary>
    public Canvas UICanvas { get { return canvasObject.GetComponent<Canvas>(); } private set { } }
    /// <summary> 获取Canvas根RectTransform </summary>
    public RectTransform UIRectTransform { get { return canvasObject.GetComponent<RectTransform>(); } private set { } }
    /// <summary> 获取CanvasScaler组件 </summary>
    public CanvasScaler UICanvasScaler { get { return canvasObject.GetComponent<CanvasScaler>(); } private set { } }
    /// <summary> 获取GraphicRaycaster组件 </summary>
    public GraphicRaycaster UIGraphicRaycaster { get { return canvasObject.GetComponent<GraphicRaycaster>(); } private set { } }

    #endregion

    /// <summary>
    /// UIRoot脚本初始化
    /// </summary>
    void Awake()
    {
        #region Camera
        uiCamera = gameObject.AddComponent<Camera>();
        uiCamera.clearFlags = CameraClearFlags.Depth;
        uiCamera.cullingMask = 1 << 5;
        uiCamera.orthographic = true;
        uiCamera.nearClipPlane = 0;
        uiCamera.useOcclusionCulling = false;
        uiCamera.allowMSAA = false;
        uiCamera.allowHDR = false;
        #endregion

        #region UI Canvas
        canvasObject = new GameObject("UI Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform);
        canvasObject.layer = 5;

        UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        UICanvas.worldCamera = uiCamera;

        UICanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        UICanvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        UICanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        #endregion

        #region EventSystem
        eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetParent(transform);
        #endregion
    }

    #region Private Function

    #endregion

    #region Public Function
    /// <summary> UGUI坐标 mousePosition</summary>
    public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(UIRectTransform, mousePosition, UICanvas.worldCamera, out Vector2 position);
        return position;
    }
    /// <summary> 判断是否点中UI </summary>
    public bool CheckUIRaycastObjects(int layer = 0)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            eventData.pressPosition = Input.mousePosition;
            eventData.position = Input.mousePosition;
        }
        else if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
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

    /// <summary> 添加空的UI子物体(Image,RawImage,Text) </summary>
    public T AddChild<T>(GameObject parent = null) where T : Component
    {
        RectTransform rectParent = parent ? parent.TryGetComponect<RectTransform>() : UIRectTransform;
        GameObject go = new GameObject(typeof(T).ToString().Split('.').Last(), typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rectTransform = go.TryGetComponect<RectTransform>();
        rectTransform.SetParent(rectParent, false);
        T t = go.TryGetComponect<T>();
        return t;
    }

    /// <summary> 添加UI预制体，返回GameObject </summary>
    public GameObject AddChild(GameObject prefab, GameObject parent = null)
    {
        RectTransform rectParent = parent ? parent.TryGetComponect<RectTransform>() : UIRectTransform;
        GameObject go = Instantiate(prefab, rectParent);
        return go;
    }

    /// <summary> 
    /// 加载窗口 添加T(Component)类型脚本
    /// path = sceneName/folderName/assetName
    /// scnenName 打包资源的第一层文件夹名称
    /// folderName 打包资源的第二层文件夹名称
    /// assetName 资源名称
    /// state 初始化窗口是否显示
    /// </summary>
    public UIWindowBase LoadWindow(string path, bool state = false)
    {
        GameObject go = AssetsManager.Instance.LoadAsset<GameObject>(path);
        if (go != null)
        {
            go = Instantiate(go, UIRectTransform);
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = go.name.Replace("(Clone)", "");
            UIWindowBase window = null;
            string wndName = path.Split('/')[path.Split('/').Length - 1];
            if (go.TryGetComponect<UIWindowBase>())
            {
                Debug.LogErrorFormat("{0}:脚本已存在,", wndName);
            }
            else
            {
                if (XLuaManager.Instance.IsLuaFileExist(path))
                {
                    window = go.AddComponent<XLuaUIWindow>().Init(path);//加载lua文件
                }
                else
                {
                    window = (UIWindowBase)go.AddComponent(Assembly.GetExecutingAssembly().GetType(wndName));
                }
            }
            EventDispatcher.TriggerEvent(go.name, state);
            return window;
        }
        return null;
    }
    /// <summary> 加载本地窗口 </summary>
    public UIWindowBase LoadLocalWindow(string path, bool state = false)
    {
        GameObject go = Resources.Load<GameObject>(path);
        if (go != null)
        {
            go = Instantiate(go, UIRectTransform);
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = go.name.Replace("(Clone)", "");
            UIWindowBase window = null;
            string wndName = path.Split('/')[path.Split('/').Length - 1];
            if (go.TryGetComponect<UIWindowBase>())
            {
                Debug.LogErrorFormat("{0}:脚本已存在,", wndName);
            }
            else
            {
                if (XLuaManager.Instance.IsLuaFileExist(path))
                {
                    window = go.AddComponent<XLuaUIWindow>().Init(path);//加载lua文件
                }
                else
                {
                    window = (UIWindowBase)go.AddComponent(Assembly.GetExecutingAssembly().GetType(wndName));
                }
            }
            EventDispatcher.TriggerEvent(go.name, state);
            return window;
        }
        return null;
    }
    #endregion
}