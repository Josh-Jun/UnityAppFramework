#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"
#import <AudioToolbox/AudioToolbox.h>


@implementation FrameworkLibAPI

id<NativeCallsProtocol> api = NULL;
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi
{
    api = aApi;
}

@end

@implementation PhotoManager
- ( void ) imageSaved: ( UIImage *) image didFinishSavingWithError:( NSError *)error
          contextInfo: ( void *) contextInfo
{
    NSLog(@"保存结束");
    if (error != nil) {
        NSLog(@"有错误");
    }
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
	void savePhoto(char *readAddr) {
		NSString *strReadAddr = [NSString stringWithUTF8String:readAddr];
		UIImage *img = [UIImage imageWithContentsOfFile:strReadAddr];
		PhotoManager *instance = [PhotoManager alloc];
		UIImageWriteToSavedPhotosAlbum(img, instance, @selector(imageSaved:didFinishSavingWithError:contextInfo:), nil);
	}
	void vibrate() {
	    AudioServicesPlaySystemSound(1520);
	}
}

