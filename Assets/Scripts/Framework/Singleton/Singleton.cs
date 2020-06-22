public class Singleton<T> where T : class, new()
{
    private static T s_Instance;
    private static readonly object SingletonObject = "Object";
    public static T Instance
    {
        get
        {
            lock (SingletonObject)
            {
                if (null == s_Instance)
                {
                    s_Instance = new T();
                }
                return s_Instance;
            }
        }
    }
}
