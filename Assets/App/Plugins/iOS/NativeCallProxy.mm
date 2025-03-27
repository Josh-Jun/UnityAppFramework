#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"
#import <AudioToolbox/AudioToolbox.h>
#import <AVFoundation/AVFoundation.h>
#import <AVFoundation/AVCaptureDevice.h>
#import <AssetsLibrary/AssetsLibrary.h>
#import <Photos/Photos.h>
#import <CoreTelephony/CTCellularData.h>


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
    void ShowHostMainWindow(const char* msg) {
		UnityPause(true);
        return [api showHostMainWindow:[NSString stringWithUTF8String:msg]];
    }
    //获取原生界面数据
    const char* GetAppData(const char* key) {
        NSString* data = [api getAppData:[NSString stringWithUTF8String:key]];
        return strdup([data cStringUsingEncoding:NSUTF8StringEncoding]);
    }
    //保存图片到相册
	void SavePhoto(char *readAddr) {
		NSString *strReadAddr = [NSString stringWithUTF8String:readAddr];
		UIImage *img = [UIImage imageWithContentsOfFile:strReadAddr];
		PhotoManager *instance = [PhotoManager alloc];
		UIImageWriteToSavedPhotosAlbum(img, instance, @selector(imageSaved:didFinishSavingWithError:contextInfo:), nil);
	}
	//震动效果
	void Vibrate() {
	    AudioServicesPlaySystemSound(1520);
	}
	//打开应用设置
	void OpenAppSettings()
    {
        NSURL*url=[NSURL URLWithString:UIApplicationOpenSettingsURLString];
        if([[UIApplication sharedApplication]canOpenURL:url]){
            if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 10.0) {
            //设备系统为IOS 10.0或者以上的
            [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:nil];
            }else{
                //设备系统为IOS 10.0以下的
                    [[UIApplication sharedApplication]openURL:url];
            }
        }
    }
    //判断是否有相关权限
    bool HasAuthorizedPermission(char* permission){
        if(permission == "Networking"){
            CTCellularData *cellularData = [[CTCellularData alloc]init];
            CTCellularDataRestrictedState status = cellularData.restrictedState;
            return (status == kCTCellularDataRestricted);
        }
        else if(permission == "Microphone"){
            AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
            return (status == AVAuthorizationStatusAuthorized);
        }
        else if(permission == "Camera"){
            AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
            return (status == AVAuthorizationStatusAuthorized);
        }
        else if(permission == "Photo"){
            PHAuthorizationStatus status = [PHPhotoLibrary authorizationStatus];
            return (status == PHAuthorizationStatusAuthorized);
        }
    }
    //请求相关权限
    void RequestUserPermission(char* permission){
        char* code;
        if(permission == "Networking"){
            //应用首次启动自动请求
        }
        else if(permission == "Microphone"){
            [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(bool granted) {
                code = granted ? "1" : "0";
            }];
        }
        else if(permission == "Camera"){
            AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(bool granted) {
                code = granted ? "1" : "0";
            }];
        }
        else if(permission == "Photo"){
            [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
                code = (status == PHAuthorizationStatusAuthorized) ? "1" : "0";
            }];
        }
        SendPermissionMsg(permission, code);
    }
    void SendPermissionMsg(char* permission, char* code){
        SendMsg("iOSPermissionCallback", permission+"-"+msg);
    }
    void SendMsg(char* methodName, char* msg){
        UnitySendMessage("Master", methodName, msg);
    }
}