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
    private GameObject canvasObject;//Canvas游戏对象
    private GameObject eventSystemObject;//EventSystem游戏对象
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

        UICanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        UICanvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        UICanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        #endregion

        #region EventSystem
        if (EventSystem.current == null)
        {
            eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(transform);
        }
        #endregion
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
    }
    public void Reset3DUIRoot(float dis, Camera camera3d = null)
    {
        Camera camera = camera3d == null ? Camera.main : camera3d;
        Vector3 target = camera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, dis));
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
    public T AddChild<T>(GameObject parent = null) where T : Component
    {
        RectTransform rectParent = parent ? parent.TryGetComponent<RectTransform>() : UIRectTransform;
        GameObject go = new GameObject(typeof(T).ToString().Split('.').Last(), typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rectTransform = go.TryGetComponent<RectTransform>();
        rectTransform.SetParent(rectParent, false);
        T t = go.TryGetComponent<T>();
        return t;
    }

    /// <summary> 添加UI预制体，返回GameObject </summary>
    public GameObject AddChild(GameObject prefab, GameObject parent = null)
    {
        RectTransform rectParent = parent ? parent.TryGetComponent<RectTransform>() : UIRectTransform;
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