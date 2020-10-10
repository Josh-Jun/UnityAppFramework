using UnityEngine;
/// <summary>
/// 
/// </summary>
public partial class NetcomManager : Singleton<NetcomManager>
{
    private static readonly bool IsLocalPath = false;
    public static string ABUrl
    {
        private set { }
        get
        {
            if (IsLocalPath)
            {
                return string.Format(@"{0}{1}/", Application.dataPath.Replace("Assets", ""), "AssetBundle");//本地AB包地址
            }
            else
            {
                return string.Format(@"{0}/{1}/", ServerUrl, "wp-content/uploads/AssetBundle/");//服务器AB包地址
            }
        }
    }
}
