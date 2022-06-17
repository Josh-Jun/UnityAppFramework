using RenderHeads.Media.AVProVideo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AVProManager : SingletonMono<AVProManager>
{
    #region Private Variable
    private MediaPlayer _mediaPlayer;
    private class TrackedEvent : UnityEvent { }
    private class Entry
    {
        public MediaPlayerEvent.EventType eventID = MediaPlayerEvent.EventType.Started;
        public TrackedEvent callback = new TrackedEvent();
    }

    [SerializeField]
    private List<Entry> m_Delegates;
    private List<Entry> Entrys
    {
        get
        {
            if (m_Delegates == null)
                m_Delegates = new List<Entry>();
            return m_Delegates;
        }
        set { m_Delegates = value; }
    }
    private void Execute(MediaPlayerEvent.EventType id)
    {
        for (int i = 0, imax = Entrys.Count; i < imax; ++i)
        {
            var ent = Entrys[i];
            if (ent.eventID == id && ent.callback != null)
                ent.callback.Invoke();
        }
    }
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
    public void AddTrackedEvent(MediaPlayerEvent.EventType eventID, UnityAction trackedEvent)
    {
        Entry entry = new Entry { eventID = eventID };
        entry.callback.AddListener(trackedEvent);
        Entrys.Add(entry);
    }
    #endregion
    private void Awake()
    {
        _mediaPlayer = gameObject.TryGetComponent<MediaPlayer>();
        gameObject.TryGetComponent<AudioSource>();
        gameObject.TryGetComponent<AudioOutput>().Player = MediaPlayer;
        MediaPlayer.AutoOpen = false;
        MediaPlayer.AutoStart = false;
        MediaPlayer.Loop = false;
        MediaPlayer.Events.AddListener(OnMediaPlayerEvent);
    }
    public void InitManager()
    {
        transform.SetParent(App.app.transform);
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
    /// <summary>视频播放事件</summary>
    /// <param name="mediaPlayer">播放器</param>
    /// <param name="eventType">播放事件</param>
    /// <param name="error">错误</param>
    private void OnMediaPlayerEvent(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode error)
    {
        if (error == ErrorCode.None)
        {
            Execute(eventType);
            return;
        }
        Debug.Log(error);
    }
}
