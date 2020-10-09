using UnityEngine;

public class SingletonMonoEvent<T> : EventBaseMono where T : SingletonMonoEvent<T>
{
    private static T _Instance;
    private static readonly object SingletonLock = "MonoEventLock";
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
                        GameObject go = new GameObject(typeof(T).ToString());
                        _Instance = go.AddComponent<T>();
                    }
                }
            }
            return _Instance;
        }
    }
}
