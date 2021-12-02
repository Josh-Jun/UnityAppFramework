using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using EventController;
using System.Reflection;
using XLuaFrame;
using System;

public class UIRoot : SingletonMono<UIRoot>
{
    #region Private Variable
    private Camera uiCamera;//UI相机
    private GameObject canvasObject;//Canvas游戏对象
    private GameObject eventSystemObject;//EventSystem游戏对象
    private GameObject obj3DRoot;//3D游戏对象根对象
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
    /// <summary> 获取3D游戏对象根对象 </summary>
    public Transform Obj3DRoot { get { return obj3DRoot.transform; } private set { } }
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
        uiCamera.depth = 1024;
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
        if(EventSystem.current == null)
        {
            eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(transform);
        }
        #endregion

        #region Obj3DRoot
        obj3DRoot = new GameObject("GoRoot");
        #endregion
    }

    #region Private Function

    #endregion

    #region Public Function
    public void SetObj3DRootActive(bool active = true)
    {
        obj3DRoot.SetGameObjectActive(active);
    }
    /// <summary> 添加3D对象预制体，返回GameObject </summary>
    public GameObject AddObj3DChild(GameObject prefab, Transform parent = null)
    {
        Transform objParent = parent ? parent : Obj3DRoot;
        GameObject go = Instantiate(prefab, objParent);
        return go;
    }
    public void Init3DUIRoot(Camera camera3d = null)
    {
        Camera camera = camera3d == null ? Camera.main : camera3d;
        UICanvas.renderMode = RenderMode.WorldSpace;
        UIRectTransform.localPosition = Vector3.forward * 5;
        UIRectTransform.localScale = Vector3.one * 0.0025f;
        UIRectTransform.sizeDelta = new Vector2(1920, 1080);
        UICanvas.worldCamera = camera;
        UICanvasScaler.referencePixelsPerUnit = 50;
    }
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
    /// <summary> 添加UI预制体，返回GameObject </summary>
    public GameObject AddChild(GameObject prefab, RectTransform parent = null)
    {
        RectTransform rectParent = parent ? parent : UIRectTransform;
        GameObject go = Instantiate(prefab, rectParent);
        return go;
    }
    #endregion
}