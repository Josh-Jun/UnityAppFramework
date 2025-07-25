using System;
using UnityEngine;
using System.Linq;
using App.Core.Helper;
using App.Core.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace App.Core.Master
{
    public class AssetsMaster : SingletonMono<AssetsMaster>
    {
        private void Awake()
        {
            LoadConfigs();
        }

        private void LoadConfigs()
        {
            var types = AppHelper.GetAssemblyTypes<IConfig>("App.Core");
            foreach (var type in types)
            {
                var la = type.GetCustomAttributes(typeof(ConfigAttribute), false).First();
                if (la is not ConfigAttribute attribute) continue;
                var obj = Activator.CreateInstance(type);
                var config = obj as IConfig;
                config?.Load();
            }
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

        #region 加载资源到场景

        /// <summary> 添加空的UI子物体(Image,RawImage,Text) </summary>
        public T AddChild<T>(GameObject parent) where T : Component
        {
            var go = new GameObject(typeof(T).ToString().Split('.').Last(), typeof(RectTransform),
                typeof(CanvasRenderer));
            var rectParent = parent.transform as RectTransform;
            var rectTransform = go.GetOrAddComponent<RectTransform>();
            rectTransform.SetParent(rectParent, false);
            var t = go.GetOrAddComponent<T>();
            return t;
        }

        /// <summary> 添加预制体，返回GameObject </summary>
        public async UniTask<GameObject> AddChildAsync(string path, GameObject parent = null)
        {
            var prefab = await LoadAssetAsync<GameObject>(path);
            var go = Instantiate(prefab, parent?.transform);
            return go;
        }

        /// <summary> 添加预制体，返回GameObject </summary>
        public async UniTask<GameObject> AddChildAsync(string path, Transform parent = null)
        {
            var prefab = await LoadAssetAsync<GameObject>(path);
            var go = Instantiate(prefab, parent);
            return go;
        }

        /// <summary> 添加预制体，返回GameObject </summary>
        public GameObject AddChildSync(string path, GameObject parent = null)
        {
            var prefab = LoadAssetSync<GameObject>(path);
            var go = Instantiate(prefab, parent?.transform);
            return go;
        }

        /// <summary> 添加预制体，返回GameObject </summary>
        public GameObject AddChildSync(string path, Transform parent = null)
        {
            var prefab = LoadAssetSync<GameObject>(path);
            var go = Instantiate(prefab, parent);
            return go;
        }

        /// <summary> 添加预制体，返回GameObject </summary>
        public GameObject AddChild(GameObject prefab, GameObject parent = null)
        {
            var go = Instantiate(prefab, parent?.transform);
            return go;
        }

        /// <summary> 添加预制体，返回GameObject </summary>
        public GameObject AddChild(GameObject prefab, Transform parent = null)
        {
            var go = Instantiate(prefab, parent);
            return go;
        }
        #endregion

        #region 加载资源

        /// <summary> 
        /// 同步加载资源，返回T
        /// </summary>
        public T LoadAssetSync<T>(string location) where T : UnityEngine.Object
        {
            var assetPackage = AssetPackage.HotfixPackage;
            if (location.Split('/').Length >= 3)
            {
                assetPackage = (AssetPackage)System.Enum.Parse(typeof(AssetPackage), $"{location.Split('/')[2]}Package");
            }
            else
            {
                Log.W("location.Split('/').Length not long enough");
            }
            return Assets.LoadAssetSync<T>(location, assetPackage);
        }

        /// <summary> 
        /// 异步加载资源，返回T
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            var assetPackage = AssetPackage.HotfixPackage;
            if (location.Split('/').Length >= 3)
            {
                assetPackage = (AssetPackage)System.Enum.Parse(typeof(AssetPackage), $"{location.Split('/')[2]}Package");
            }
            else
            {
                Log.W("location.Split('/').Length not long enough");
            }
            var t = await Assets.LoadAssetAsync<T>(location, assetPackage);
            return t;
        }

        #endregion
    }
}