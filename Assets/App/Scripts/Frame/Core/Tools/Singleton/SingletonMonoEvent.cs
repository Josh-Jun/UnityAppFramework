using UnityEngine;

namespace App.Core.Tools
{
    public class SingletonMonoEvent<T> : EventBaseMono where T : SingletonMonoEvent<T>
    {
        private static T _Instance;
        private const string SingletonLock = "MonoEventLock";
        private const string GoName = "Master";
        public static T Instance
        {
            get
            {
                lock (SingletonLock)
                {
                    if (_Instance == null)
                    {
                        var go = GameObject.Find(GoName) ?? new GameObject(GoName);
                        _Instance = go.GetComponent<T>();
                        if (_Instance == null)
                        {
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