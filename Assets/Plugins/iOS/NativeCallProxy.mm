#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"


@implementation FrameworkLibAPI

id<NativeCallsProtocol> api = NULL;
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi
{
    api = aApi;
}

@end


extern "C" {
    //退出unity显示原生界面
    void showHostMainWindow(const char* msg) {
		UnityPause(true);
        return [api showHostMainWindow:[NSString stringWithUTF8String:msg]];
    }
    //获取原生界面数据
    const char* getAppData(const char* key) {
        NSString* data = [api getAppData:[NSString stringWithUTF8String:key]];
        return strdup([data cStringUsingEncoding:NSUTF8StringEncoding]);
    }
}

