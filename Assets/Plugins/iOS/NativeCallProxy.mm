#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"
#import <AudioToolbox/AudioToolbox.h>
#import <AVFoundation/AVFoundation.h>
#import <AVFoundation/AVCaptureDevice.h>
#import <AssetsLibrary/AssetsLibrary.h>
#import <Photos/Photos.h>
#import <CoreTelephony/CTCellularData.h>
#import <Accelerate/Accelerate.h>

// 日志宏定义
#define LOG_INFO(msg, ...) NSLog(@"[INFO] " msg, ##__VA_ARGS__)
#define LOG_ERROR(msg, ...) NSLog(@"[ERROR] " msg, ##__VA_ARGS__)

@implementation FrameworkLibAPI

static id<NativeCallsProtocol> api = nil;
static dispatch_once_t onceToken;

+ (void)registerAPIforNativeCalls:(id<NativeCallsProtocol>)aApi {
    dispatch_once(&onceToken, ^{
        api = aApi;
        LOG_INFO(@"NativeCallsProtocol registered successfully");
    });
}

@end

extern "C" {
    // 内部使用的消息发送函数
extern void SendUnityMessage(const char *method, const char *msg) {
        if (UnityGetMainWindow() == nil) {
            LOG_ERROR(@"Unity window not available when sending message");
            return;
        }
        
        UnitySendMessage("Master", method, msg ? msg : "");
    }
    
extern void SendMsg(const char *msg) {
        SendUnityMessage("ReceiveNativeMsg", msg);
    }
    
extern void ReceiveUnityMsg(const char *msg) {
        LOG_INFO(@"Received Unity message: %s", msg);
        // 可添加消息处理逻辑
    }

    // 退出unity显示原生界面
extern void ShowHostMainWindow(const char *msg) {
        UnityPause(true);
        @try {
            if (api && [api respondsToSelector:@selector(showHostMainWindow:)]) {
                NSString *message = [NSString stringWithUTF8String:msg ? msg : ""];
                [api showHostMainWindow:message];
                LOG_INFO(@"Show host main window with message: %@", message);
            } else {
                LOG_ERROR(@"API not registered or method not implemented");
            }
        } @catch (NSException *exception) {
            LOG_ERROR(@"Exception in ShowHostMainWindow: %@", exception);
        }
    }
    
    // 获取原生界面数据
extern const char * GetNativeData(const char *key) {
        @try {
            if (api && [api respondsToSelector:@selector(getAppData:)]) {
                NSString *keyStr = [NSString stringWithUTF8String:key ? key : ""];
                NSString *data = [api getAppData:keyStr];
                LOG_INFO(@"Get native data for key: %@", keyStr);
                return data ? strdup([data UTF8String]) : strdup("");
            }
            LOG_ERROR(@"API not registered or method not implemented");
            return strdup("");
        } @catch (NSException *exception) {
            LOG_ERROR(@"Exception in GetNativeData: %@", exception);
            return strdup("");
        }
    }
    
    // 震动效果
extern void NativeVibrate() {
        @try {
            if (@available(iOS 10.0, *)) {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                [generator impactOccurred];
                LOG_INFO(@"Triggered haptic feedback");
            } else {
                AudioServicesPlaySystemSound(1520);
                LOG_INFO(@"Triggered vibration (legacy)");
            }
        } @catch (NSException *exception) {
            LOG_ERROR(@"Exception in NativeVibrate: %@", exception);
        }
    }
    
    // 打开应用设置
extern void OpenNativeSettings() {
        @try {
            NSURL *url = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
            UIApplication *app = [UIApplication sharedApplication];
            
            if ([app canOpenURL:url]) {
                if (@available(iOS 10.0, *)) {
                    [app openURL:url options:@{} completionHandler:^(BOOL success) {
                        LOG_INFO(@"Open settings %@", success ? @"successful" : @"failed");
                    }];
                } else {
                    [app openURL:url];
                    LOG_INFO(@"Open settings (legacy)");
                }
            } else {
                LOG_ERROR(@"Cannot open settings URL");
            }
        } @catch (NSException *exception) {
            LOG_ERROR(@"Exception in OpenNativeSettings: %@", exception);
        }
    }
    
    // 判断是否有相关权限
    extern bool HasNativeUserAuthorizedPermission(const char *permission) {
        @try {
            NSString *permStr = [NSString stringWithUTF8String:permission];
            
            if ([permStr isEqualToString:@"Networking"]) {
                CTCellularData *cellularData = [[CTCellularData alloc] init];
                CTCellularDataRestrictedState status = cellularData.restrictedState;
                return (status == kCTCellularDataNotRestricted);
            }
            else if ([permStr isEqualToString:@"Microphone"]) {
                AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
                return (status == AVAuthorizationStatusAuthorized);
            }
            else if ([permStr isEqualToString:@"Camera"]) {
                AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
                return (status == AVAuthorizationStatusAuthorized);
            }
            else if ([permStr isEqualToString:@"Photo"]) {
                PHAuthorizationStatus status = [PHPhotoLibrary authorizationStatus];
                return (status == PHAuthorizationStatusAuthorized);
            }
            
            LOG_ERROR(@"Unknown permission type: %@", permStr);
            return false;
        } @catch (NSException *exception) {
            LOG_ERROR(@"Exception in HasNativeAuthorizedPermission: %@", exception);
            return false;
        }
    }
    
    // 请求相关权限
extern void RequestNativeUserPermission(const char *permission) {
        @try {
            NSString *permStr = [NSString stringWithUTF8String:permission];
                
            if ([permStr isEqualToString:@"Networking"]) {
                // 网络权限在iOS上无法主动请求，只能检测状态
                int status = HasNativeUserAuthorizedPermission(permission) ? 1 : 0;
                NSString *data = [NSString stringWithFormat:@"{\"Name\":\"%@\",\"Data\":%d}", permStr, status];
                SendMsg([data UTF8String]);
            }
            else if ([permStr isEqualToString:@"Microphone"]) {
                [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(BOOL granted) {
                    int result = granted ? 1 : 0;
                    NSString *data = [NSString stringWithFormat:@"{\"Name\":\"%@\",\"Data\":%d}", permStr, result];
                    SendMsg([data UTF8String]);
                    LOG_INFO(@"Microphone permission %@", granted ? @"granted" : @"denied");
                }];
            }
            else if ([permStr isEqualToString:@"Camera"]) {
                [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
                    int result = granted ? 1 : 0;
                    NSString *data = [NSString stringWithFormat:@"{\"Name\":\"%@\",\"Data\":%d}", permStr, result];
                    SendMsg([data UTF8String]);
                    LOG_INFO(@"Camera permission %@", granted ? @"granted" : @"denied");
                }];
            }
            else if ([permStr isEqualToString:@"Photo"]) {
                [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
                    int result = (status == PHAuthorizationStatusAuthorized) ? 1 : 0;
                    NSString *data = [NSString stringWithFormat:@"{\"Name\":\"%@\",\"Data\":%d}", permStr, result];
                    SendMsg([data UTF8String]);
                    LOG_INFO(@"Photo library access %@", (result == 1) ? @"granted" : @"denied");
                }];
            }
            else {
                LOG_ERROR(@"Unknown permission type: %@", permStr);
                SendMsg("{\"Name\":\"Unknown\",\"Data\":0}");
            }
        } @catch (NSException *exception) {
            LOG_ERROR(@"Exception in RequestUserPermission: %@", exception);
            SendMsg("{\"Name\":\"Exception\",\"Data\":0}");
        }
    }
}
