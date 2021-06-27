public class Singleton<T> where T : class, new()
{
    private static T _Instance;
    private static readonly object SingletonLock = "Lock";
    public static T Instance
    {
        get
        {
            lock (SingletonLock)
            {
                if (null == _Instance)
                {
                    _Instance = new T();
                }
                return _Instance;
            }
        }
    }
    public Singleton()
    {
        _Instance = this as T;
    }
}
