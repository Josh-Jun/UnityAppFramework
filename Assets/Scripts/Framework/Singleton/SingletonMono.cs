using UnityEngine;

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
                    if(_Instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _Instance = go.AddComponent<T>();
                    }
                    else
                    {
                        Destroy(_Instance.gameObject);
                    }
                }
                else
                {
                    Destroy(_Instance.gameObject);
                }
            }
            return _Instance;
        }
    }
}