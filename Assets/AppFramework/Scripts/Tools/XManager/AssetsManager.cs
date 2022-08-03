using UnityEngine;
using EventController;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;
using XLuaFrame;

public class AssetsManager : SingletonMono<AssetsManager>
{
    public override void InitManager(Transform parent)
    {
        base.InitManager(parent);
    }
    
    #region 移动游戏对象到场景
    public void MoveGameObjectToScene(GameObject go, string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
    #endregion

    #region 加载场景
    public void LoadSceneAsync(string sceneName, Action<AsyncOperation> cb = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, mode);
            cb?.Invoke(asyncOperation);
            asyncOperation.completed += (AsyncOperation ao) =>
            {
                Scene scene = SceneManager.GetSceneByName(sceneName);
                SceneManager.SetActiveScene(scene);
                cb?.Invoke(asyncOperation);
            };
        }
    }
    public void UnLoadSceneAsync(string sceneName, Action<float> cb = null)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);
            asyncOperation.completed += (AsyncOperation ao) =>
            {
                cb?.Invoke(asyncOperation.progress);
            };
        }
    }
    #endregion

    #region 加载资源到场景
    /// <summary> 添加空的UI子物体(Image,RawImage,Text) </summary>
    public T AddChild<T>(GameObject parent) where T : Component
    {
        GameObject go = new GameObject(typeof(T).ToString().Split('.').Last(), typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rectParent = parent.transform as RectTransform;
        RectTransform rectTransform = go.TryGetComponent<RectTransform>();
        rectTransform.SetParent(rectParent, false);
        T t = go.TryGetComponent<T>();
        return t;
    }

    /// <summary> 添加预制体，返回GameObject </summary>
    public GameObject AddChild(string path, GameObject parent = null)
    {
        GameObject go = AddChild(path, parent.transform);
        return go;
    }
    /// <summary> 添加预制体，返回GameObject </summary>
    public GameObject AddChild(string path, Transform parent = null)
    {
        GameObject prefab = LoadAsset<GameObject>(path);
        GameObject go = Instantiate(prefab, parent);
        return go;
    }
    #endregion
    
    #region 加载窗口
    /// <summary> 
    /// 加载窗口 添加T(Component)类型脚本
    /// path = 窗口路径，不包含AssetsFolder
    /// state 初始化窗口是否显示
    /// </summary>
    public T LoadUIWindow<T>(string path, bool state = false) where T : Component
    {
        var go = LoadAsset<GameObject>(path);
        if (go == null) return null;
        go = Instantiate(go, GoRoot.Instance.UIRectTransform);
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.name = go.name.Replace("(Clone)", "");
        T t = null;
        if (Root.AppConfig.RunXLua)
        {
            var luaPath = $"{path.Split('/')[0]}/{go.name}";
            t = go.TryGetComponent<XLuaWindow>().Init(luaPath) as T;
        }
        else
        {
            var type = Assembly.GetExecutingAssembly().GetType(typeof(T).FullName);
            if (type != null)
            {
                t = (T)go.AddComponent(type);
            }
            else
            {
                Debug.LogError($"{typeof(T).FullName}:脚本已存在,");
            }
        }
        EventDispatcher.TriggerEvent(go.name, state);
        GoRoot.Instance.AddWindow<T>(t as WindowBase);
        return t;
    }
    /// <summary> 加载本地窗口 </summary>
    public T LoadLocalUIWindow<T>(string path, bool state = false) where T : Component
    {
        var go = Resources.Load<GameObject>(path);
        if (go == null) return null;
        go = Instantiate(go, GoRoot.Instance.UIRectTransform);
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.name = go.name.Replace("(Clone)", "");
        var type = Assembly.GetExecutingAssembly().GetType(typeof(T).FullName);
        var t = (T)go.AddComponent(type);
        EventDispatcher.TriggerEvent(go.name, state);
        GoRoot.Instance.AddWindow<T>(t as WindowBase);
        return t;
    }
    /// <summary> 
    /// 加载窗口 添加T(Component)类型脚本
    /// path = 窗口路径，不包含AssetsFolder
    /// state 初始化窗口是否显示
    /// </summary>
    public T LoadGoWindow<T>(string path, bool isTemp, bool state = false) where T : Component
    {
        var go = LoadAsset<GameObject>(path);
        if (go == null) return null;
        var parent = isTemp ? GoRoot.Instance.TempGoRoot : GoRoot.Instance.GlobalGoRoot;
        go = Instantiate(go, parent);
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.name = go.name.Replace("(Clone)", "");
        T t = null;
        if (Root.AppConfig.RunXLua)
        {
            var luaPath = $"{path.Split('/')[0]}/{go.name}";
            t = go.TryGetComponent<XLuaWindow>().Init(luaPath) as T;
        }
        else
        {
            var type = Assembly.GetExecutingAssembly().GetType(typeof(T).FullName);
            if (type != null)
            {
                t = (T)go.AddComponent(type);
            }
            else
            {
                Debug.LogError($"{typeof(T).FullName}:脚本已存在,");
            }
        }
        EventDispatcher.TriggerEvent(go.name, state);
        GoRoot.Instance.AddWindow<T>(t as WindowBase);
        return t;
    }
    /// <summary> 加载本地窗口 </summary>
    public T LoadLocalGoWindow<T>(string path, bool isTemp, bool state = false) where T : Component
    {
        var go = Resources.Load<GameObject>(path);
        if (go == null) return null;
        var parent = isTemp ? GoRoot.Instance.TempGoRoot : GoRoot.Instance.GlobalGoRoot;
        go = Instantiate(go, parent);
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.name = go.name.Replace("(Clone)", "");
        var type = Assembly.GetExecutingAssembly().GetType(typeof(T).FullName);
        var t = (T)go.AddComponent(type);
        EventDispatcher.TriggerEvent(go.name, state);
        GoRoot.Instance.AddWindow<T>(t as WindowBase);
        return t;
    }
    #endregion
    
    #region 加载资源
    /// <summary> 
    /// 加载ab包中的资源，返回T
    /// path = sceneName/folderName/assetName
    /// scnenName 打包资源的第一层文件夹名称
    /// folderName 打包资源的第二层文件夹名称
    /// assetName 资源名称
    /// </summary>
    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        if (Root.AppConfig.IsLoadAB)
        {
            string[] names = path.Split('/');
            return AssetBundleManager.Instance.LoadAsset<T>(names[0], names[1], names[names.Length - 1]);
        }
        else
        {
            return Resources.Load<T>(string.Format("AssetsFolder/{0}", path));
        }
    }
    #endregion
}