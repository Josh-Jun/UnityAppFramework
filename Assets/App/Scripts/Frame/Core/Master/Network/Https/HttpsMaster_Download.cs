using System;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    /// <summary>
    /// Download
    /// </summary>
    public partial class HttpsMaster : SingletonMonoEvent<HttpsMaster>
    {
        private const string TextureCachePath = "Cache/Textures/";
        private const string AudioCachePath = "Cache/Audios/";


        public string DownloadTexture(string url, Action<Texture2D> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.I($"DownloadTexture : url = {url}");
                callback?.Invoke(null);
                return null;
            }

            var basePath = PlatformMaster.Instance.GetDataPath(TextureCachePath);
            var filename = url.Split('?')[0].Split('/')[^1];
            var localPath = $"{basePath}{filename}";
            var uwr = Uwr;
            if (FileTools.FileExist(localPath))
            {
                uwr.GetTexture($"file://{localPath}", callback);
            }
            else
            {
                uwr.GetBytes(url, bytes =>
                {
                    FileTools.CreateFile(localPath, bytes);
                    uwr = Uwr;
                    uwr.GetTexture($"file://{localPath}", callback);
                });
            }
            return localPath;
        }
        public string DownloadSprite(string url, Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.I($"DownloadSprite : url = {url}");
                callback?.Invoke(null);
                return null;
            }

            var basePath = PlatformMaster.Instance.GetDataPath(TextureCachePath);
            var filename = url.Split('?')[0].Split('/')[^1];
            var localPath = $"{basePath}{filename}";
            var uwr = Uwr;
            if (FileTools.FileExist(localPath))
            {
                uwr.GetTexture($"file://{localPath}", (texture) =>
                {
                    var sprite = PictureTools.CreateSprite(texture);
                    callback?.Invoke(sprite);
                });
            }
            else
            {
                uwr.GetBytes(url, bytes =>
                {
                    FileTools.CreateFile(localPath, bytes);
                    uwr = Uwr;
                    uwr.GetTexture($"file://{localPath}", texture =>
                    {
                        var sprite = PictureTools.CreateSprite(texture);
                        callback?.Invoke(sprite);
                    });
                });
            }
            return localPath;
        }
        
        public string DownloadAudio(string url, AudioType audioType, Action<AudioClip> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.I($"DownloadAudio : url = {url}");
                callback?.Invoke(null);
                return null;
            }

            var basePath = PlatformMaster.Instance.GetDataPath(AudioCachePath);
            var filename = url.Split('?')[0].Split('/')[^1];
            var localPath = $"{basePath}{filename}";
            var uwr = Uwr;
            if (FileTools.FileExist(localPath))
            {
                uwr.GetAudioClip($"file://{localPath}", callback, audioType);
            }
            else
            {
                uwr.GetBytes(url, bytes =>
                {
                    FileTools.CreateFile(localPath, bytes);
                    uwr = Uwr;
                    uwr.GetAudioClip($"file://{localPath}", callback, audioType);
                });
            }
            return localPath;
        }
    }
}