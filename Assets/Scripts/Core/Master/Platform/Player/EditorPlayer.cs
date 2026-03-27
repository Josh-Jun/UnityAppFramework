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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace App.Core.Master
{
    public partial class EditorPlayer
    {
        public override void OpenPhotoAlbum(Action<Texture2D> callback)
        {
#if UNITY_EDITOR
            var path = EditorUtility.OpenFilePanel("选择图片", "", "png,jpg,jpeg");
            if (string.IsNullOrEmpty(path)) return;
            var fileData = System.IO.File.ReadAllBytes(path);
            var texture = new Texture2D(512, 512);
            texture.LoadImage(fileData);
            callback?.Invoke(texture);
#endif
        }
    }
}