using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using EventController;

public class UIRoot : SingletonMono<UIRoot>
{
    private Camera uiCamera;//UI相机
    private GameObject canvasObject;//Canvas游戏对象
    private GameObject eventSystemObject;//EventSystem游戏对象

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

    /// <summary> UGUI坐标 mousePosition</summary>
    public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
    {
        if (UICanvas.worldCamera.orthographic == true)
        {
            return (mousePosition - new Vector2(Screen.width / 2, Screen.height / 2));
        }
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(UIRectTransform, mousePosition, UICanvas.worldCamera, out position);
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
    public T LoadWindow<T>(string path, bool state = false) where T : Component
    {
        GameObject go = AssetsManager.Instance.LoadAsset<GameObject>(path);
        if (go != null)
        {
            go = Instantiate(go, UIRectTransform);
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = go.name.Replace("(Clone)", "");
            T t = go.AddComponent<T>();
            EventDispatcher.TriggerEvent(go.name, state);
            return t;
        }
        return null;
    }
}