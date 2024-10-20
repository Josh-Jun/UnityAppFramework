using UnityEngine;

namespace AppFrame.Tools
{
    public class SingletonMonoEvent<T> : EventBaseMono where T : SingletonMonoEvent<T>
    {
        private static T _Instance;
        private static readonly object SingletonLock = "MonoEventLock";
        private const string GoName = "Manager";
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
                            var go = GameObject.Find(GoName);
                            if (go == null)
                            {
                                go = new GameObject(GoName);
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
