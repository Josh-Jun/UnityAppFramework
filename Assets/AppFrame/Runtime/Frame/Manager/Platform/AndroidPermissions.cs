namespace AppFrame.Manager
{
    public class AndroidPermissions
    {
        // 允许一个程序访问CellID或 WiFi热点来获取粗略的位置
        public const string ACCESS_COARSE_LOCATION = "android.permission.ACCESS_COARSE_LOCATION";
        // 允许一个程序访问CellID或 WiFi热点来获取粗略的位置
        public const string ACCESS_FINE_LOCATION = "android.permission.ACCESS_FINE_LOCATION";
        // 允许程序获取网络信息状态，如当前的网络连接是否有效
        public const string ACCESS_NETWORK_STATE = "android.permission.ACCESS_NETWORK_STATE";
        // 允许程序获取当前WiFi接入的状态以及WLAN热点的信息
        public const string ACCESS_WIFI_STATE = "android.permission.ACCESS_WIFI_STATE";
        // 允许程序更新手机电池统计信息
        public const string BATTERY_STATS = "android.permission.BATTERY_STATS";
        // 允许程序连接配对过的蓝牙设备
        public const string BLUETOOTH = "android.permission.BLUETOOTH";
        // 允许程序进行发现和配对新的蓝牙设备
        public const string BLUETOOTH_ADMIN = "android.permission.BLUETOOTH_ADMIN";
        // 允许程序访问摄像头进行拍照
        public const string CAMERA = "android.permission.CAMERA";
        // 允许应用程序请求安装包
        public const string REQUEST_INSTALL_PACKAGES = "android.permission.REQUEST_INSTALL_PACKAGES";
        // 允许程序访问网络连接，可能产生GPRS流量
        public const string INTERNET = "android.permission.INTERNET";
        // 允许程序录制声音通过手机或耳机的麦克
        public const string RECORD_AUDIO = "android.permission.RECORD_AUDIO";
        // 允许程序振动
        public const string VIBRATE = "android.permission.VIBRATE";
        // 允许程序写入外部存储,如SD卡上读写文件
        public const string WRITE_EXTERNAL_STORAGE = "android.permission.WRITE_EXTERNAL_STORAGE";

    }
}