using UnityEngine;

namespace AppFrame.Tools
{
    public class SingletonMonoEvent<T> : EventBaseMono where T : SingletonMonoEvent<T>
    {
        private static T _Instance;
        private static readonly object SingletonLock = "MonoEventLock";
        private const string ParentName = "Manager";
        public static T Instance
        {
            get
            {
                lock (SingletonLock)
                {
                    Transform parent = App.App.app.transform.Find(ParentName);
                    if (parent == null)
                    {
                        parent = new GameObject(ParentName).transform;
                        parent.SetParent(App.App.app.transform);
                    }
                    if (_Instance == null)
                    {
                        _Instance = FindObjectOfType<T>();
                        if (_Instance == null)
                        {
                            GameObject go = new GameObject(typeof(T).Name);
                            go.transform.SetParent(parent);
                            _Instance = go.AddComponent<T>();
                        }
                        _Instance.OnSingletonMonoInit();
                    }

                    return _Instance;
                }
            }
        }

        protected virtual void OnSingletonMonoInit()
        {
            
        }
    }
}
