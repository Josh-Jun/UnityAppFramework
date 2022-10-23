using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using EventController;
using System.Reflection;
using System;
using UnityEngine.InputSystem.UI;
#if PICO_XR_SETTING
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

public class GoRoot : SingletonMono<GoRoot>
{
    #region Private Variable
    private GameObject CanvasObject;//Canvas游戏对象
    private GameObject EventSystemObject;//EventSystem游戏对象
    private GameObject GlobalGameObject;//3D游戏对象父物体(全局)
    private GameObject TempGameObject;//3D游戏对象父物体(临时)
    
    private static Dictionary<string, ViewBase> windowPairs = new Dictionary<string, ViewBase>();
    private Dictionary<string, Transform> rootPairs = new Dictionary<string, Transform>();

    #endregion

    #region Public Variable
    /// <summary> Canvas组件 </summary>
    public Canvas UICanvas { get { return CanvasObject.GetComponent<Canvas>(); } private set { } }
    /// <summary> 获取Canvas根RectTransform </summary>
    public RectTransform UIRectTransform { get { return CanvasObject.GetComponent<RectTransform>(); } private set { } }
    /// <summary> 获取CanvasScaler组件 </summary>
    public CanvasScaler UICanvasScaler { get { return CanvasObject.GetComponent<CanvasScaler>(); } private set { } }
    /// <summary> 获取GraphicRaycaster组件 </summary>
    public GraphicRaycaster UIGraphicRaycaster { get { return CanvasObject.GetComponent<GraphicRaycaster>(); } private set { } }
    
    /// <summary> 获取3D游戏对象根对象(全局) </summary>
    public Transform GlobalGoRoot { get { return GlobalGameObject.transform; } private set { } }
    /// <summary> 获取3D游戏对象根对象(临时) </summary>
    public Transform TempGoRoot 
    {
        get
        {
            if (TempGameObject == null)
            {
                TempGameObject = new GameObject("TempGoRoot");
            }
            return TempGameObject.transform;
        } 
        private set { } 
    }
    #endregion

    /// <summary>
    /// UIRoot脚本初始化
    /// </summary>
    void Awake()
    {
        transform.SetParent(App.app.transform);
        
        #region UI Canvas
        CanvasObject = new GameObject("UI Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        CanvasObject.transform.SetParent(transform);
        CanvasObject.layer = 5;

        UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UICanvas.worldCamera = Camera.main;

        UICanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        UICanvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        UICanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        #endregion
        
        GlobalGameObject = new GameObject("GlobalGoRoot");
        GlobalGameObject.transform.SetParent(transform);

        #region EventSystem
        if (EventSystem.current == null)
        {
            Type t = null;
#if PICO_XR_SETTING
            t = Root.AppConfig.TargetPackage == TargetPackage.Mobile ? typeof(InputSystemUIInputModule) : typeof(XRUIInputModule);
#endif
            t = typeof(InputSystemUIInputModule);
            EventSystemObject = new GameObject("EventSystem", typeof(EventSystem), t);
            EventSystemObject.transform.SetParent(transform);
        }
        #endregion

#if PICO_XR_SETTING
        if(Root.AppConfig.TargetPackage == TargetPackage.Pico)
        {
            Init3DUIRoot(Camera.main);
            Reset3DUIRoot();
        }
#endif
    }

    #region Private Function

    #endregion

    #region Public Function
#if PICO_XR_SETTING
    public void Init3DUIRoot(Camera camera3d = null)
    {
        if(camera3d == null) return;
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

    public void AddWindow<T>(ViewBase view)
    {
        var type = typeof(T);
        var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
        if (!windowPairs.ContainsKey(scriptName))
        {
            windowPairs.Add(scriptName, view);
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
            go.transform.SetParent(GlobalGoRoot, false);
            rootPairs.Add(name, go.transform);
            return go.transform;
        } else {
            return rootPairs[name];
        }
    }
    #endregion
}