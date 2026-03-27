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

namespace App.Core.Master
{
    public partial class IPhonePlayer
    {
        public override void OpenPhotoAlbum(Action<Texture2D> callback)
        {
            NativeGallery.RequestPermissionAsync((permission) =>
            {
                // 根据权限结果执行不同的代码
                switch (permission)
                {
                    case NativeGallery.Permission.Granted:
                        Debug.Log("权限已授予，开始选择图片");
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
                        break;

                    case NativeGallery.Permission.ShouldAsk:
                        Debug.Log("权限被拒绝，但可以再次请求");
                        callback?.Invoke(null);
                        break;

                    case NativeGallery.Permission.Denied:
                        Debug.Log("权限被永久拒绝，需要手动到设置中开启");
                        NativeGallery.OpenSettings(); // 可选：直接打开设置页面
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
                }

            }, NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        }
    }
}
