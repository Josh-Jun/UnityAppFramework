using UnityEngine;
using AppFrame.Interface;

namespace AppFrame.Tools
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _Instance;
        private static readonly object SingletonLock = "MonoLock";

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
                            GameObject go = new GameObject(typeof(T).Name);
                            _Instance = go.AddComponent<T>();
                        }
                        else
                        {
                            return _Instance;
                        }
                    }

                    return _Instance;
                }
            }
        }

        public virtual void InitParent(Transform parent)
        {
            transform.SetParent(parent);
        }
    }
}