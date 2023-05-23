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
                    if (_Instance == null)
                    {
                        _Instance = FindObjectOfType<T>();
                        if (_Instance == null)
                        {
                            var parent = GameObject.Find(ParentName);
                            var go = new GameObject(typeof(T).Name);
                            if (parent != null)
                            {
                                go.transform.SetParent(parent.transform);
                            }
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
