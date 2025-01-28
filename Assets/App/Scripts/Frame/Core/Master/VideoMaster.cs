using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using App.Core.Tools;

namespace App.Core.Master
{
    public class VideoMaster : SingletonMono<VideoMaster>
    {
        private RenderTexture movie;
        private VideoPlayer VideoPlayer { get; set; }

        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
            VideoPlayer = this.TryGetComponent<VideoPlayer>();
            VideoPlayer.sendFrameReadyEvents = true;
        }

        /// <summary>在RawImage上播放视频，URL</summary>
        public void PlayVideo(RawImage rawImage, string url, Action cb = null, int width = 0, int height = 0)
        {
            SetRenderTexture(width, height);
            VideoPlayer.source = VideoSource.Url;
            VideoPlayer.url = url;
            rawImage.texture = movie;
            VideoPlayer.Play();
            VideoPlayer.loopPointReached += (VideoPlayer vp) => { cb?.Invoke(); };
        }

        /// <summary>在RawImage上播放视频，URL</summary>
        public void PlayVideo(RawImage rawImage, VideoClip clip, Action cb = null, int width = 0, int height = 0)
        {
            SetRenderTexture(width, height);
            VideoPlayer.source = VideoSource.VideoClip;
            VideoPlayer.clip = clip;
            rawImage.texture = movie;
            VideoPlayer.Play();
            VideoPlayer.loopPointReached += (VideoPlayer vp) => { cb?.Invoke(); };
        }

        /// <summary>设置RenderTexture</summary>
        private void SetRenderTexture(int width, int height, int depth = 24)
        {
            VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            VideoPlayer.targetTexture = movie;
            if (width == 0 || height == 0)
            {
                width = Screen.width;
                height = Screen.height;
            }

            movie = new RenderTexture(width, height, depth);
        }

        private long frameIndex = 0;
        private Action<Texture2D> callback = null;

        /// <summary>获取视频某帧图片</summary>
        public void GetVideoFrameTexture(VideoClip clip, long frameId, Action<Texture2D> action)
        {
            frameIndex = frameId;
            callback = action;
            VideoPlayer.renderMode = VideoRenderMode.APIOnly;
            VideoPlayer.source = VideoSource.VideoClip;
            VideoPlayer.clip = clip;
            VideoPlayer.waitForFirstFrame = true;
            VideoPlayer.sendFrameReadyEvents = true;
            VideoPlayer.frameReady += OnFrameReadyEvent;
            VideoPlayer.Play();
        }

        private void OnFrameReadyEvent(VideoPlayer source, long frameIdx)
        {
            if (frameIdx == frameIndex)
            {
                var renderTexture = source.texture as RenderTexture;
                if (renderTexture != null)
                {
                    var texture = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                    texture.Apply();
                    RenderTexture.active = null;
                    VideoPlayer.frameReady -= OnFrameReadyEvent;
                    VideoPlayer.sendFrameReadyEvents = false;
                    VideoPlayer.Stop();
                    callback?.Invoke(texture);
                }
            }
        }
    }
}
