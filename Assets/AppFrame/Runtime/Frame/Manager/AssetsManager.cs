using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using AppFrame.Tools;
using AppFrame.Config;
using UnityEngine.SceneManagement;

namespace AppFrame.Manager
{
    public class AssetsManager : SingletonMono<AssetsManager>
    {
        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
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

        /// <summary> 
        /// 加载场景
        /// path = moduleName/folderName/assetName
        /// moduleName 打包资源的第一层文件夹名称
        /// folderName 打包资源的第二层文件夹名称
        /// assetName 场景名称
        /// </summary>
        public void LoadSceneAsync(string sceneName, Action cb = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            string[] names = sceneName.Split('/'); //names[0], names[1], names[names.Length - 1]
            if (Global.AppConfig.LoadAssetsMold != LoadAssetsMold.Native)
            {
                AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle(names[0], $"{names[1]}Scene");
            }

            if (!string.IsNullOrEmpty(names[names.Length - 1]))
            {
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(names[names.Length - 1], mode);
                asyncOperation.completed += (AsyncOperation ao) =>
                {
                    Scene scene = SceneManager.GetSceneByName(names[names.Length - 1]);
                    SceneManager.SetActiveScene(scene);
                    cb?.Invoke();
                };
            }
        }

        public void LoadingSceneAsync(string sceneName, Action<float> cb = null,
            LoadSceneMode mode = LoadSceneMode.Single)
        {
            string[] names = sceneName.Split('/'); //names[0], names[1], names[names.Length - 1]
            if (Global.AppConfig.LoadAssetsMold != LoadAssetsMold.Native)
            {
                AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle(names[0], $"{names[1]}Scene");
            }

            if (!string.IsNullOrEmpty(names[names.Length - 1]))
            {
                StartCoroutine(Loading(names[names.Length - 1], mode, (progress) => { cb?.Invoke(progress); }));
            }
        }

        private IEnumerator Loading(string name, LoadSceneMode mode, Action<float> callback)
        {
            yield return new WaitForEndOfFrame();
            AsyncOperation async = SceneManager.LoadSceneAsync(name, mode);
            async.allowSceneActivation = false;
            int displayProgress = 0;
            int toProgress;
            float LoadingProgress = 0;
            while (async.progress < 0.9f)
            {
                toProgress = (int)async.progress * 100;
                while (displayProgress < toProgress)
                {
                    ++displayProgress;
                    LoadingProgress = displayProgress / 100f;
                    callback?.Invoke(LoadingProgress);
                    yield return new WaitForEndOfFrame();
                }

                yield return new WaitForEndOfFrame();
            }

            toProgress = 99;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                LoadingProgress = displayProgress / 100f;
                callback?.Invoke(LoadingProgress);
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitUntil(() => LoadingProgress >= 0.99f);
            callback?.Invoke(0.9999f);
            async.allowSceneActivation = true;
            async.completed += (AsyncOperation ao) =>
            {
                Scene scene = SceneManager.GetSceneByName(name);
                SceneManager.SetActiveScene(scene);
                callback?.Invoke(1);
            };
        }

        public void UnLoadSceneAsync(string sceneName, Action<float> cb = null)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);
                asyncOperation.completed += (AsyncOperation ao) => { cb?.Invoke(asyncOperation.progress); };
            }
        }

        #endregion

        #region 加载资源到场景

        /// <summary> 添加空的UI子物体(Image,RawImage,Text) </summary>
        public T AddChild<T>(GameObject parent) where T : Component
        {
            GameObject go = new GameObject(typeof(T).ToString().Split('.').Last(), typeof(RectTransform),
                typeof(CanvasRenderer));
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

        /// <summary> 添加预制体，返回GameObject </summary>
        public GameObject AddChild(GameObject prefab, GameObject parent = null)
        {
            GameObject go = AddChild(prefab, parent.transform);
            return go;
        }

        /// <summary> 添加预制体，返回GameObject </summary>
        public GameObject AddChild(GameObject prefab, Transform parent = null)
        {
            GameObject go = Instantiate(prefab, parent);
            return go;
        }
        #endregion

        #region 加载资源

        /// <summary> 
        /// 加载ab包中的资源，返回T
        /// path = moduleName/folderName/assetName
        /// moduleName 打包资源的第一层文件夹名称
        /// folderName 打包资源的第二层文件夹名称
        /// assetName 资源名称
        /// </summary>
        public T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            if (Global.AppConfig.LoadAssetsMold != LoadAssetsMold.Native)
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
}