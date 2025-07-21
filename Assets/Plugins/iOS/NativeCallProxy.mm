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

extern "C" {
    void SendPermissionMsg(const char* permission, const char* code);
    void SendMsg(const char* methodName, const char* msg);
    void ReceiveUnityMsg(const char* msg);

    //退出unity显示原生界面
    void ShowHostMainWindow(const char* msg) {
		UnityPause(true);
        return [api showHostMainWindow:[NSString stringWithUTF8String:msg]];
    }
    //获取原生界面数据
    const char* GetNativeData(const char* key) {
        NSString* data = [api getAppData:[NSString stringWithUTF8String:key]];
        return strdup([data cStringUsingEncoding:NSUTF8StringEncoding]);
    }
	//震动效果
	void NativeVibrate() {
	    AudioServicesPlaySystemSound(1520);
	}
	//打开应用设置
	void OpenNativeSettings()
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
    bool HasNativeAuthorizedPermission(const char* permission){
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
    void RequestUserPermission(const char* permission){
        __block const char* code;
        if(permission == "Networking"){
            //应用首次启动自动请求
        }
        else if(permission == "Microphone"){
            [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(BOOL granted) {
                code = granted ? "1" : "0";
            }];
        }
        else if(permission == "Camera"){
            [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
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
    void SendPermissionMsg(const char* permission, const char* code){
        NSString* data = [NSString stringWithFormat:@"%s-%s", permission, code];
        const char* msg = strdup([data cStringUsingEncoding:NSUTF8StringEncoding]);
        SendMsg("iOSPermissionCallback", msg);
    }
    void SendMsg(const char* methodName, const char* msg){
        UnitySendMessage("Master", methodName, msg);
    }
   
    void ReceiveUnityMsg(const char* msg){
        
    }
}