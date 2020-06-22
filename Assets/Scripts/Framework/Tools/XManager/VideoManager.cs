using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.UI;
using System;

public class VideoManager : SingletonMono<VideoManager>
{
    private RenderTexture movie;
    private VideoPlayer videoPlayer;
    private void Awake()
    {
        videoPlayer = CreateVideoPlayer();
        movie = new RenderTexture(Screen.width, Screen.height, 24);
    }

    private VideoPlayer CreateVideoPlayer(bool playOnAwake = false)
    {
        VideoPlayer video = this.TryGetComponect<VideoPlayer>();
        video.playOnAwake = playOnAwake;
        return video;
    }

    public void PlayVideo(Image image, VideoClip clip, UnityAction cb = null, bool loop = false, int width = 1920, int height = 1080)
    {
        //在Image上播放视频
        if (videoPlayer.targetTexture == null) return;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = movie;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = loop;
        videoPlayer.Play();
        PlayEndCallback(() => { cb?.Invoke(); });
        int w = videoPlayer.targetTexture.width;
        int h = videoPlayer.targetTexture.height;
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        Texture2D t = new Texture2D(w, h, TextureFormat.ARGB32, false);
        RenderTexture.active = videoPlayer.targetTexture;
        t.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        t.Apply();
        image.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f)) as Sprite;
    }
    public void PlayVideo(Image image, string url, UnityAction cb = null, bool loop = false, int width = 1920, int height = 1080)
    {
        //在Image上播放视频
        if (videoPlayer.targetTexture == null) return;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = movie;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        videoPlayer.isLooping = loop;
        videoPlayer.Play();
        PlayEndCallback(() => { cb?.Invoke(); });
        int w = videoPlayer.targetTexture.width;
        int h = videoPlayer.targetTexture.height;
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        Texture2D t = new Texture2D(w, h, TextureFormat.ARGB32, false);
        RenderTexture.active = videoPlayer.targetTexture;
        t.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        t.Apply();
        image.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f)) as Sprite;
    }
    public void PlayVideo(RawImage rawImage, VideoClip clip, UnityAction cb = null, bool loop = false, int width = 1920, int height = 1080)
    {
        //在RawImage上播放视频
        if (videoPlayer.texture == null) return;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = loop;
        videoPlayer.Play();
        PlayEndCallback(() => { cb?.Invoke(); });
        rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        rawImage.texture = videoPlayer.texture;
    }
    public void PlayVideo(RawImage rawImage, string url, UnityAction cb = null, bool loop = false, int width = 1920, int height = 1080)
    {
        //在RawImage上播放视频
        if (videoPlayer.texture == null) return;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;
        videoPlayer.isLooping = loop;
        videoPlayer.Play();
        PlayEndCallback(() => { cb?.Invoke(); });
        rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        rawImage.texture = videoPlayer.texture;
    }
    /// <summary>播放结束回调</summary>
    private void PlayEndCallback(UnityAction cb)
    {
        if (videoPlayer != null && videoPlayer.isPrepared)
        {
            videoPlayer.loopPointReached += (videoPlayer) =>
            {
                if (videoPlayer != null)
                {
                    videoPlayer.frame = 1;
                }
                cb?.Invoke();
            };
        }
    }

    private long frameIndex = 0;
    private Action<Texture2D> callback = null;
    /// <summary>获取视频某帧图片</summary>
    public void GetVideoFrameTexture(VideoClip clip, long frameId, Action<Texture2D> action)
    {
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady += OnFrameReadyEvent;
        videoPlayer.Play();
        frameIndex = frameId;
        callback = action;
    }
    private void OnFrameReadyEvent(VideoPlayer source, long frameIdx)
    {
        if (frameIdx == frameIndex)
        {
            RenderTexture renderTexture = source.texture as RenderTexture;
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            videoPlayer.frameReady -= OnFrameReadyEvent;
            videoPlayer.sendFrameReadyEvents = false;
            videoPlayer.Stop();
            callback?.Invoke(texture);
        }
    }
}
