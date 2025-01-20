using System;
using App.Core.Tools;
using UnityEngine;
/// <summary>
/// Https
/// </summary>

namespace App.Core.Master
{
    public partial class NetcomMaster : SingletonMonoEvent<NetcomMaster>
    {
        private const string TextureCachePath = "Cache/Textures/";
        private const string AudioCachePath = "Cache/Audios/";


        public void DownloadTexture(string url, Action<Texture2D> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.I($"DownloadTexture : url = {url}");
                callback?.Invoke(null);
                return;
            }

            var basePath = PlatformMaster.Instance.GetDataPath(TextureCachePath);
            var suffix = url.Split('.')[url.Split('.').Length - 1];
            var localPath = $"{basePath}{url.GetHashCode()}.{suffix}";
            var uwr = Uwr;
            if (FileTools.FileExist(localPath))
            {
                uwr.GetTexture(localPath, callback);
            }
            else
            {
                uwr.GetBytes(url, bytes =>
                {
                    FileTools.CreateFile(localPath, bytes);
                    uwr = Uwr;
                    uwr.GetTexture(localPath, callback);
                });
            }
        }
        public void DownloadSprite(string url, Action<Sprite> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.I($"DownloadSprite : url = {url}");
                callback?.Invoke(null);
                return;
            }

            var basePath = PlatformMaster.Instance.GetDataPath(TextureCachePath);
            var suffix = url.Split('.')[url.Split('.').Length - 1];
            var localPath = $"{basePath}{url.GetHashCode()}.{suffix}";
            var uwr = Uwr;
            if (FileTools.FileExist(localPath))
            {
                uwr.GetTexture(localPath, (texture) =>
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
                    uwr.GetTexture(localPath, texture =>
                    {
                        var sprite = PictureTools.CreateSprite(texture);
                        callback?.Invoke(sprite);
                    });
                });
            }
        }
        
        public void DownloadAudio(string url, AudioType audioType, Action<AudioClip> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.I($"DownloadAudio : url = {url}");
                callback?.Invoke(null);
                return;
            }

            var basePath = PlatformMaster.Instance.GetDataPath(AudioCachePath);
            var suffix = url.Split('.')[url.Split('.').Length - 1];
            var localPath = $"{basePath}{url.GetHashCode()}.{suffix}";
            var uwr = Uwr;
            if (FileTools.FileExist(localPath))
            {
                uwr.GetAudioClip(localPath, callback, audioType);
            }
            else
            {
                uwr.GetBytes(url, bytes =>
                {
                    FileTools.CreateFile(localPath, bytes);
                    uwr = Uwr;
                    uwr.GetAudioClip(localPath, callback, audioType);
                });
            }
        }
    }
}