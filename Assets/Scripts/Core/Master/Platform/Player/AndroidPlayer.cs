/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年3月16 10:54
 * function    : 
 * ===============================================
 * */

using System;
using UnityEngine;
using UnityEngine.Android;

namespace App.Core.Master
{
    public partial class AndroidPlayer
    {
        private int versionNumber = -1;
        private int VersionNumber
        {
            get
            {
                if (versionNumber != -1) return versionNumber;
                // 提取版本号
                var startIndex = SystemInfo.operatingSystem.IndexOf("Android OS ", StringComparison.Ordinal) + "Android OS ".Length;
                var endIndex = SystemInfo.operatingSystem.IndexOf(" /", startIndex, StringComparison.Ordinal);
                if (endIndex <= startIndex) return versionNumber;
                var androidVersion = SystemInfo.operatingSystem.Substring(startIndex, endIndex - startIndex);
                // 转换版本号为数字
                if (int.TryParse(androidVersion, out versionNumber))
                {
                    Debug.Log($"Android版本号: {versionNumber}");
                }
                return versionNumber;
            }
        }
        public override void OpenPhotoAlbum(Action<Texture2D> callback)
        {
            // 根据Android版本选择不同的权限检查
            var hasPermission = false;
             if (VersionNumber >= 13)
             {
                 // Android 13及以上使用 READ_MEDIA_IMAGES
                 hasPermission = Permission.HasUserAuthorizedPermission(AndroidPermissions.READ_MEDIA_IMAGES);
                 Debug.Log("使用 READ_MEDIA_IMAGES 权限检查");
             }
             else
             {
                 // Android 13以下使用 EXTERNAL_STORAGE
                 hasPermission = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
                 Debug.Log("使用 EXTERNAL_STORAGE 权限检查");
             }

            // 如果没有权限，请求相应的权限
            if (!hasPermission)
            {
                var permission = VersionNumber >= 13 ? AndroidPermissions.READ_MEDIA_IMAGES : Permission.ExternalStorageRead;
                var PermissionCallback = new PermissionCallbacks();
                //同意权限
                PermissionCallback.PermissionGranted += p =>
                {
                    GetImageFromGallery(callback);
                };
                //拒绝权限
                PermissionCallback.PermissionDenied += p =>
                {
                    callback?.Invoke(null);
                };
                //无法获取权限，打开设置
                PermissionCallback.PermissionDeniedAndDontAskAgain += p =>
                {
                    NativeGallery.OpenSettings();
                };
                RequestUserPermission(permission);
            }
            else
            {
                GetImageFromGallery(callback);
            }
            
        }
        
        private void GetImageFromGallery(Action<Texture2D> callback)
        {
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (path != null)
                {
                    var texture = NativeGallery.LoadImageAtPath(path, 512, false);
                    if (!texture)
                    {
                        Log.I("Couldn't load texture from " + path);
                        return;
                    }
                    callback?.Invoke(texture);
                }
                else
                {
                    Log.I("选择图片失败");
                    callback?.Invoke(null);
                }
            }, "选择一张图片", "image/png");
        }
    }
}
