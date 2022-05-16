using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AVProManager : SingletonMono<AVProManager>
{
    #region Private Variable
    private MediaPlayer _mediaPlayer;
    #endregion
    #region Public Variable
    public MediaPlayer MediaPlayer { get { return _mediaPlayer; } private set { } }

    /// <summary> 是否播放 </summary>
    public bool IsPlaying
    {
        get
        {
            if (_mediaPlayer && _mediaPlayer.Control != null)
            {
                return _mediaPlayer.Control.IsPlaying();
            }
            return false;
        }
    }
    /// <summary> 是否播放完成 </summary>
    public bool IsFinished
    {
        get
        {
            if (_mediaPlayer && _mediaPlayer.Control != null)
            {
                return _mediaPlayer.Control.IsFinished();
            }
            return false;
        }
    }
    /// <summary> 是否可请求 </summary>
    public bool IsSeeking
    {
        get
        {
            if (_mediaPlayer && _mediaPlayer.Control != null)
            {
                return _mediaPlayer.Control.IsSeeking();
            }
            return false;
        }
    }
    /// <summary> 是否可播放 </summary>
    public bool CanPlay
    {
        get
        {
            if (_mediaPlayer && _mediaPlayer.Control != null)
            {
                return _mediaPlayer.Control.CanPlay();
            }
            return false;
        }
    }
    #endregion
    private void Awake()
    {
        _mediaPlayer = gameObject.TryGetComponent<MediaPlayer>();
        gameObject.TryGetComponent<AudioSource>();
        gameObject.TryGetComponent<AudioOutput>().Player = MediaPlayer;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary> 打开视频 </summary>
    public bool OpenMedia(MediaPath path, bool autoPlay = true)
    {
        return _mediaPlayer.OpenMedia(path.PathType, path.Path, autoPlay);
    }
    public bool OpenMedia(MediaPathType pathType, string path, bool autoPlay = true)
    {
        return _mediaPlayer.OpenMedia(pathType, path, autoPlay);
    }
    public bool OpenMedia(MediaReference mediaReference, bool autoPlay = true)
    {
        return _mediaPlayer.OpenMedia(mediaReference, autoPlay);
    }
    public bool OpenMedia(bool autoPlay = true)
    {
        return _mediaPlayer.OpenMedia(autoPlay);
    }

    /// <summary> 开始播放 </summary>
    public void Play()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            _mediaPlayer.Play();
        }
    }
    /// <summary> 暂停播放 </summary>
    public void Pause()
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            _mediaPlayer.Pause();
        }
    }
    /// <summary> 设置快进快退 </summary>
    public void SeekRelative(float deltaTime)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            TimeRange timelineRange = GetTimelineRange();
            double time = _mediaPlayer.Control.GetCurrentTime() + deltaTime;
            time = System.Math.Max(time, timelineRange.startTime);
            time = System.Math.Min(time, timelineRange.startTime + timelineRange.duration);
            _mediaPlayer.Control.Seek(time);
        }
    }
    private TimeRange GetTimelineRange()
    {
        if (_mediaPlayer.Info != null)
        {
            return Helper.GetTimelineRange(_mediaPlayer.Info.GetDuration(), _mediaPlayer.Control.GetSeekableTimes());
        }
        return new TimeRange();
    }
    /// <summary> 设置静音 </summary>
    public void MuteAudio(bool mute)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            // Change mute
            _mediaPlayer.Control.MuteAudio(mute);
        }
    }
    /// <summary> 改变音量 </summary>
    public void ChangeAudioVolume(float _audioVolume)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            _mediaPlayer.AudioVolume = _audioVolume;
        }
    }
    /// <summary> 设置是否循环 </summary>
    public void SetLooping(bool islooping)
    {
        if (_mediaPlayer && _mediaPlayer.Control != null)
        {
            _mediaPlayer.Control.SetLooping(islooping);
        }
    }
}
