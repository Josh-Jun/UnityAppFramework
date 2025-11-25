/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年11月25 16:29
 * function    :
 * ===============================================
 * */

public class iOS
{
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern string GetNativeData(string key);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ShowHostMainWindow(string msg);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void NativeVibrate();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void OpenNativeSettings();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern bool HasNativeUserAuthorizedPermission(string permission);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void RequestNativeUserPermission(string permission);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ReceiveUnityMsg(string msg);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void IOSRegisterWxApi(string WXAppKey, string WXAppUniversalLink);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void IOSWxPay(string openID, string partnerId, string prepayId, string nonceStr, string timeStamp, string package, string sign);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void IOSAliPay(string payOrder, string scheme);
#endif
}