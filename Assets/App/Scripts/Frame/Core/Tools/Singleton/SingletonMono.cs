using UnityEngine;

namespace App.Core.Tools
{
    public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        private static T _Instance;
        private static readonly object SingletonLock = "MonoLock";
        private const string GoName = "Master";
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