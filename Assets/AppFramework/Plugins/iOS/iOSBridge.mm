#import "iOSBridge.h"

NSString *data = [[NSString alloc]init];

// 获取ios界面数据
extern "C" const char* getAppData(char* key)
{
	return strdup([@data UTF8String]);
}
// 跳转iOS界面
extern "C" void quitUnityWindow(char *str)
{
	// 关闭Unity界面
    UnityPause(true);
     
    // 设置iOS界面
    [GetAppController() setupIOS];
     
    // GetAppController()获取appController，相当于self
    // 设置窗口的跟控制器为iOS的控制
    GetAppController().window.rootViewController = GetAppController().vc;
}