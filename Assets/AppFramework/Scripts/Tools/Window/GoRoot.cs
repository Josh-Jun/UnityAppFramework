using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using EventController;
using System.Reflection;
using XLuaFrame;
using System;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class GoRoot : SingletonMono<GoRoot>
{
    #region Private Variable
    private GameObject canvasObject;//Canvas游戏对象
    private GameObject eventSystemObject;//EventSystem游戏对象
    private GameObject goObj;//3D游戏对象父物体
    
    private static Dictionary<string, WindowBase> windowPairs = new Dictionary<string, WindowBase>();
    private Dictionary<string, Transform> rootPairs = new Dictionary<string, Transform>();

    #endregion

    #region Public Variable
    /// <summary> Canvas组件 </summary>
    public Canvas UICanvas { get { return canvasObject.GetComponent<Canvas>(); } private set { } }
    /// <summary> 获取Canvas根RectTransform </summary>
    public RectTransform UIRectTransform { get { return canvasObject.GetComponent<RectTransform>(); } private set { } }
    /// <summary> 获取CanvasScaler组件 </summary>
    public CanvasScaler UICanvasScaler { get { return canvasObject.GetComponent<CanvasScaler>(); } private set { } }
    /// <summary> 获取GraphicRaycaster组件 </summary>
    public GraphicRaycaster UIGraphicRaycaster { get { return canvasObject.GetComponent<GraphicRaycaster>(); } private set { } }
    
    /// <summary> 获取3D游戏对象根对象 </summary>
    public Transform GoTransform { get { return goObj.transform; } private set { } }
    #endregion

    /// <summary>
    /// UIRoot脚本初始化
    /// </summary>
    void Awake()
    {
        #region UI Canvas
        canvasObject = new GameObject("UI Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform);
        canvasObject.layer = 5;

        UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UICanvas.worldCamera = Camera.main;

        UICanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        UICanvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        UICanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        #endregion
        
        goObj = new GameObject("GameObjects");
        goObj.transform.SetParent(transform);

        #region EventSystem
        if (EventSystem.current == null)
        {
            eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(transform);
        }
        #endregion

        if(Root.AppConfig.TargetPackage == TargetPackage.Pico)
        {
            Init3DUIRoot(Camera.main);
            Reset3DUIRoot();
        }
    }

    #region Private Function

    #endregion

    #region Public Function
    public void Init3DUIRoot(Camera camera3d = null)
    {
        Camera camera = camera3d == null ? Camera.main : camera3d;
        UICanvas.renderMode = RenderMode.WorldSpace;
        UIRectTransform.localScale = Vector3.one * 0.0025f;
        UIRectTransform.sizeDelta = new Vector2(1920, 1080);
        UICanvas.worldCamera = camera;
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
    /// <summary> UGUI坐标 mousePosition</summary>
    public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(UIRectTransform, mousePosition, Camera.main, out Vector2 position);
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
    public T AddChild<T>(GameObject parent) where T : Component
    {
        RectTransform rectParent = parent ? parent.TryGetComponent<RectTransform>() : UIRectTransform;
        GameObject go = new GameObject(typeof(T).ToString().Split('.').Last(), typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rectTransform = go.TryGetComponent<RectTransform>();
        rectTransform.SetParent(rectParent, false);
        T t = go.TryGetComponent<T>();
        return t;
    }

    /// <summary> 添加预制体，返回GameObject </summary>
    public GameObject AddChild(GameObject prefab, GameObject parent)
    {
        GameObject go = AddChild(prefab,parent.transform);
        return go;
    }
    /// <summary> 添加预制体，返回GameObject </summary>
    public GameObject AddChild(GameObject prefab, Transform parent)
    {
        GameObject go = Instantiate(prefab, parent);
        return go;
    }
    public void AddWindow<T>(WindowBase window)
    {
        var type = typeof(T);
        var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
        if (!windowPairs.ContainsKey(scriptName))
        {
            windowPairs.Add(scriptName, window);
        }
    }
    public T GetWindow<T>() where T : class
    {
        var type = typeof(T);
        var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
        if (!windowPairs.ContainsKey(scriptName))
        {
            return null;
        }
        return windowPairs[scriptName] as T;
    }
    /// <summary> 添加3D对象预制体，返回GameObject </summary>
    public Transform TryGetEmptyNode(string name)
    {
        if (!rootPairs.ContainsKey(name)) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(GoTransform, false);
            rootPairs.Add(name, go.transform);
            return go.transform;
        } else {
            return rootPairs[name];
        }
    }
    #endregion
}