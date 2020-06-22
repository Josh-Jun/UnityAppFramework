using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

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

        GetCanvas().renderMode = RenderMode.ScreenSpaceCamera;
        GetCanvas().worldCamera = uiCamera;

        GetCanvasScaler().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        GetCanvasScaler().referenceResolution = new Vector2(Screen.width, Screen.height);
        GetCanvasScaler().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        #endregion

        #region EventSystem
        eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetParent(transform);
        #endregion
    }

    /// <summary>
    /// 获取Canvas组件
    /// </summary>
    /// <returns></returns>
    public Canvas GetCanvas()
    {
        return canvasObject.GetComponent<Canvas>();
    }
    /// <summary>
    /// 获取Canvas根RectTransform
    /// </summary>
    /// <returns></returns>
    public RectTransform GetRectTransform()
    {
        return canvasObject.GetComponent<RectTransform>();
    }
    /// <summary>
    /// 获取CanvasScaler组件
    /// </summary>
    /// <returns></returns>
    public CanvasScaler GetCanvasScaler()
    {
        return canvasObject.GetComponent<CanvasScaler>();
    }
    /// <summary>
    /// 获取GraphicRaycaster组件
    /// </summary>
    /// <returns></returns>
    public GraphicRaycaster GetGraphicRaycaster()
    {
        return canvasObject.GetComponent<GraphicRaycaster>();
    }
    /// <summary>
    /// UGUI坐标
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <returns></returns>
    public Vector2 ScreenPointInRectangle(Vector2 mousePosition)
    {
        if (GetCanvas().worldCamera.orthographic == true)
        {
            return (mousePosition - new Vector2(Screen.width / 2, Screen.height / 2));
        }
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetRectTransform(), mousePosition, GetCanvas().worldCamera, out position);
        return position;
    }
    /// <summary>
    /// 判断是否点中UI
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
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
        this.GetGraphicRaycaster().Raycast(eventData, list);
        return list.Count > layer;
    }

    /// <summary>
    /// 添加空的UI子物体(Image,RawImage,Text)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parent"></param>
    /// <returns></returns>
    public T AddChild<T>(GameObject parent = null) where T : Component
    {
        RectTransform rectParent = parent ? parent.TryGetComponect<RectTransform>() : GetRectTransform();
        GameObject go = new GameObject(typeof(T).ToString().Split('.').Last(), typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rectTransform = go.TryGetComponect<RectTransform>();
        rectTransform.SetParent(rectParent, false);
        T t = go.TryGetComponect<T>();
        return t;
    }

    /// <summary>
    /// 添加UI预制体，返回GameObject
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject AddChild(GameObject prefab, GameObject parent = null)
    {
        RectTransform rectParent = parent ? parent.TryGetComponect<RectTransform>() : GetRectTransform();
        GameObject go = Instantiate(prefab, rectParent);
        return go;
    }

    /// <summary>
    /// 移动到某个场景
    /// </summary>
    /// <param name="scene"></param>
    public void MoveToScene(string sceneName)
    {
        AssetsManager.Instance.MoveGameObjectToScene(gameObject, sceneName);
    }
}
