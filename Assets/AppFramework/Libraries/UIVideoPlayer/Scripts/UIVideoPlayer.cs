using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Video.UI;

public class UIVideoPlayer : SingletonMono<UIVideoPlayer>
{
    public VideoPlayer VideoPlayer;
    private RawImage _videoDisplay;
    private RenderTexture movie;

    [Header("Options")] 
    
    [SerializeField] float _keyVolumeDelta = 0.05f;
    [SerializeField] float _jumpDeltaTime = 5f;
    [SerializeField] bool _autoHide = true;
    [SerializeField] float _userInactiveDuration = 1.5f;

    [Header("Keyboard Controls")] 
    
    [SerializeField] bool _enableKeyboardControls = true;
    [SerializeField] KeyCode KeyVolumeUp = KeyCode.UpArrow;
    [SerializeField] KeyCode KeyVolumeDown = KeyCode.DownArrow;
    [SerializeField] KeyCode KeyTogglePlayPause = KeyCode.Space;
    [SerializeField] KeyCode KeyToggleMute = KeyCode.M;
    [SerializeField] KeyCode KeyJumpForward = KeyCode.RightArrow;
    [SerializeField] KeyCode KeyJumpBack = KeyCode.LeftArrow;

    [Header("Optional Components")] 
    
    [SerializeField] OverlayManager _overlayManager = null;
    [SerializeField] RectTransform _timelineTip = null;

    [Header("UI Components")] 
    
    [SerializeField] RectTransform _canvasTransform = null;
    [SerializeField] Slider _sliderTime = null;
    [SerializeField] EventTrigger _videoTouch = null;
    [SerializeField] CanvasGroup _controlsGroup = null;

    [Header("UI Components (Optional)")] 
    
    [SerializeField] Text _textTimeDuration = null;
    [SerializeField] Slider _sliderVolume = null;
    [SerializeField] Button _buttonPlayPause = null;
    [SerializeField] Button _buttonVolume = null;
    [SerializeField] Button _buttonTimeBack = null;
    [SerializeField] Button _buttonTimeForward = null;
    [SerializeField] HorizontalSegmentsPrimitive _segmentsSeek = null;
    [SerializeField] HorizontalSegmentsPrimitive _segmentsBuffered = null;
    [SerializeField] HorizontalSegmentsPrimitive _segmentsProgress = null;


    private bool _wasPlayingBeforeTimelineDrag;
    private float _controlsFade = 1f;
    private Material _playPauseMaterial;
    private Material _volumeMaterial;
    private Material _subtitlesMaterial;
    private Material _optionsMaterial;
    private Material _audioSpectrumMaterial;
    private float[] _spectrumSamples = new float[128];
    private float[] _spectrumSamplesSmooth = new float[128];
    private float _maxValue = 1f;
    private float _audioVolume = 1f;

    private float _audioFade = 0f;
    private bool _isAudioFadingUpToPlay = true;
    private const float AudioFadeDuration = 0.25f;
    private float _audioFadeTime = 0f;

    private readonly LazyShaderProperty _propMorph = new LazyShaderProperty("_Morph");
    private readonly LazyShaderProperty _propMute = new LazyShaderProperty("_Mute");
    private readonly LazyShaderProperty _propVolume = new LazyShaderProperty("_Volume");
    private readonly LazyShaderProperty _propSpectrum = new LazyShaderProperty("_Spectrum");
    private readonly LazyShaderProperty _propSpectrumRange = new LazyShaderProperty("_SpectrumRange");

    public struct LazyShaderProperty
    {
        public LazyShaderProperty(string name)
        {
            _name = name;
            _id = 0;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Id
        {
            get
            {
                if (_id == 0)
                {
                    _id = Shader.PropertyToID(_name);
                }

                return _id;
            }
        }

        private string _name;
        private int _id;
    }

    private void Awake()
    {
        movie = new RenderTexture(1920, 1080, 24);
        _videoDisplay = _videoTouch.gameObject.GetComponent<RawImage>();
        _tipDisplay = _timelineTip.Find("HoverThumbnail").GetComponent<RawImage>();
    }

    // Start is called before the first frame update
    void Start()
    {
        VideoPlayer.sendFrameReadyEvents = true;
        VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
        VideoPlayer.targetTexture = movie;
        VideoPlayer.loopPointReached += OnVideoLoopOrPlayFinished;
        VideoPlayer.prepareCompleted += OnPrepareCompleted;
        VideoPlayer.started += OnStarted;
        VideoPlayer.seekCompleted += OnSeekCompleted;
        VideoPlayer.errorReceived += OnErrorReceived;
        
        SetupPlayPauseButton();
        SetupTimeBackForwardButtons();
        SetupVolumeButton();
        CreateTimelineDragEvents();
        CreateVideoTouchEvents();
        CreateVolumeSliderEvents();
        UpdateVolumeSlider();
    }

    private struct UserInteraction
    {
        public static float InactiveTime;
        private static Vector3 _previousMousePos;
        private static int _lastInputFrame;

        public static bool IsUserInputThisFrame()
        {
            if (Time.frameCount == _lastInputFrame)
            {
                return true;
            }
#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
            bool touchInput = (Input.touchSupported && Input.touchCount > 0);
            bool mouseInput = (Input.mousePresent && (Input.mousePosition != _previousMousePos ||
                                                      Input.mouseScrollDelta != Vector2.zero ||
                                                      Input.GetMouseButton(0)));

            if (touchInput || mouseInput)
            {
                _previousMousePos = Input.mousePosition;
                _lastInputFrame = Time.frameCount;
                return true;
            }

            return false;
#else
			return true;
#endif
        }
    }

    private void UpdateControlsVisibility()
    {
        if (UserInteraction.IsUserInputThisFrame() || !CanHideControls())
        {
            UserInteraction.InactiveTime = 0f;
            FadeUpControls();
        }
        else
        {
            UserInteraction.InactiveTime += Time.unscaledDeltaTime;
            if (UserInteraction.InactiveTime >= _userInactiveDuration)
            {
                FadeDownControls();
            }
            else
            {
                FadeUpControls();
            }
        }
    }

    private void FadeUpControls()
    {
        if (!_controlsGroup.gameObject.activeSelf)
        {
            _controlsGroup.gameObject.SetActive(true);
        }

        _controlsFade = Mathf.Min(1f, _controlsFade + Time.deltaTime * 8f);
        _controlsGroup.alpha = Mathf.Pow(_controlsFade, 5f);
    }

    private void FadeDownControls()
    {
        if (_controlsGroup.gameObject.activeSelf)
        {
            _controlsFade = Mathf.Max(0f, _controlsFade - Time.deltaTime * 3f);
            _controlsGroup.alpha = Mathf.Pow(_controlsFade, 5f);
            if (_controlsGroup.alpha <= 0f)
            {
                _controlsGroup.gameObject.SetActive(false);
            }
        }
    }

    private bool CanHideControls()
    {
        bool result = true;
        if (!_autoHide)
        {
            result = false;
        }
#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
        else if (Input.mousePresent)
        {
            // Check whether the mouse cursor is over the controls, in which case we can't hide the UI
            RectTransform rect = _controlsGroup.GetComponent<RectTransform>();
            Vector2 canvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out canvasPos);

            Rect rr = RectTransformUtility.PixelAdjustRect(rect, null);
            result = !rr.Contains(canvasPos);
        }
#endif
        return result;
    }

    private RawImage _tipDisplay;

    // Update is called once per frame
    void Update()
    {
        if (VideoPlayer == null) return;

        UpdateControlsVisibility();
        UpdateAudioFading();

#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
        if (_timelineTip != null)
        {
            if (_isHoveringOverTimeline)
            {
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, Input.mousePosition, null,
                    out canvasPos);

                _segmentsSeek.gameObject.SetActive(true);
                _timelineTip.gameObject.SetActive(true);
                Vector3 mousePos = _canvasTransform.TransformPoint(canvasPos);

                _timelineTip.position = new Vector2(mousePos.x, _timelineTip.position.y);

                if (UserInteraction.IsUserInputThisFrame())
                {
                    // Work out position on the timeline
                    Bounds bounds =
                        RectTransformUtility.CalculateRelativeRectTransformBounds(this._sliderTime
                            .GetComponent<RectTransform>());
                    float x = Mathf.Clamp01((canvasPos.x - bounds.min.x) / bounds.size.x);

                    double time = (double)x * VideoPlayer.length;
                    // Update time text
                    Text hoverText = _timelineTip.GetComponentInChildren<Text>();
                    if (hoverText != null)
                    {
                        hoverText.text = string.Format("{0:00}:{1:00}", (int)(time / 60), (int)(time % 60));
                    }

                    // Update seek segment when hovering over timeline
                    if (_segmentsSeek != null)
                    {
                        float[] ranges = new float[2];
                        if (VideoPlayer.length > 0.0)
                        {
                            double t = (VideoPlayer.time / VideoPlayer.length);
                            ranges[1] = x;
                            ranges[0] = (float)t;
                        }

                        _segmentsSeek.Segments = ranges;
                    }
                }
            }
            else
            {
                _timelineTip.gameObject.SetActive(false);
                _segmentsSeek.gameObject.SetActive(false);
            }
        }
#endif

        // Updated stalled display
        if (_overlayManager)
        {
            _overlayManager.Reset();
            if (isReady)
            {
                _overlayManager.TriggerStalled();
            }
        }

        // Update keyboard input
        if (_enableKeyboardControls)
        {
#if (!ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER)
            // Keyboard toggle play/pause
            if (Input.GetKeyDown(KeyTogglePlayPause))
            {
                TogglePlayPause();
            }

            // Keyboard seek 5 seconds
            if (Input.GetKeyDown(KeyJumpBack))
            {
                SeekRelative(-_jumpDeltaTime);
            }
            else if (Input.GetKeyDown(KeyJumpForward))
            {
                SeekRelative(_jumpDeltaTime);
            }

            // Keyboard control volume
            if (Input.GetKeyDown(KeyVolumeUp))
            {
                ChangeAudioVolume(_keyVolumeDelta);
            }
            else if (Input.GetKeyDown(KeyVolumeDown))
            {
                ChangeAudioVolume(-_keyVolumeDelta);
            }

            // Keyboard toggle mute
            if (Input.GetKeyDown(KeyToggleMute))
            {
                ToggleMute();
            }
#endif
        }

        // Animation play/pause button
        if (_playPauseMaterial != null)
        {
            float t = _playPauseMaterial.GetFloat(_propMorph.Id);
            float d = 1f;
            if (VideoPlayer.isPlaying)
            {
                d = -1f;
            }

            t += d * Time.deltaTime * 6f;
            t = Mathf.Clamp01(t);
            _playPauseMaterial.SetFloat(_propMorph.Id, t);
        }

        // Animation volume/mute button
        if (_volumeMaterial != null)
        {
            float t = _volumeMaterial.GetFloat(_propMute.Id);
            float d = 1f;
            if (!VideoPlayer.GetDirectAudioMute(0))
            {
                d = -1f;
            }

            t += d * Time.deltaTime * 6f;
            t = Mathf.Clamp01(t);
            _volumeMaterial.SetFloat(_propMute.Id, t);
            _volumeMaterial.SetFloat(_propVolume.Id, _audioVolume);
        }
        // Update volume slider
        UpdateVolumeSlider();

        // Update time/duration text display
        if (_textTimeDuration)
        {
            double t1 = VideoPlayer.time;
            double d1 = VideoPlayer.length;
            _textTimeDuration.text = string.Format("{0:00}:{1:00} / {2:00}:{3:00}", (int)(t1 / 60), (int)(t1 % 60),
                (int)(d1 / 60), (int)(d1 % 60));
        }

        // Update time slider position
        if (_sliderTime&&!isReady)
        {
            double t = (VideoPlayer.time / VideoPlayer.length);
            _sliderTime.value = Mathf.Clamp01((float)t);
        }
    }

    private bool isReady = true;

    // 在查找操作完成后调用
    public void OnSeekCompleted(VideoPlayer player)
    {
    }

    // 开始播放后调用
    public void OnStarted(VideoPlayer player)
    {
        isReady = false;
        _audioVolume = VideoPlayer.GetDirectAudioVolume(0);
    }

    // 准备完成事件调用
    public void OnPrepareCompleted(VideoPlayer player)
    {
        _videoDisplay.texture = player.targetTexture;
        _tipDisplay.texture = player.targetTexture;
    }

    // 播放完成或者循环完成事件
    public void OnVideoLoopOrPlayFinished(VideoPlayer player)
    {
    }

    // 播放错误事件
    public void OnErrorReceived(VideoPlayer player, string error)
    {
        isReady = true;
    }

    void UpdateAudioFading()
    {
        // Increment fade timer
        if (_audioFadeTime < AudioFadeDuration)
        {
            _audioFadeTime = Mathf.Clamp(_audioFadeTime + Time.deltaTime, 0f, AudioFadeDuration);
        }

        // Trigger pause when audio faded down
        if (_audioFadeTime >= AudioFadeDuration)
        {
            if (!_isAudioFadingUpToPlay)
            {
                Pause(skipFeedback:true);
            }
        }

        // Apply audio fade value
        if (VideoPlayer != null && VideoPlayer.isPlaying)
        {
            _audioFade = Mathf.Clamp01(_audioFadeTime / AudioFadeDuration);
            if (!_isAudioFadingUpToPlay)
            {
                _audioFade = (1f - _audioFade);
            }
            ApplyAudioVolume();
        }
    }
    public void ChangeAudioVolume(float delta)
    {
        if (VideoPlayer != null)
        {
            // Change volume
            _audioVolume = Mathf.Clamp01(_audioVolume + delta);

            // Update the UI
            UpdateVolumeSlider();

            // Trigger the overlays
            if (_overlayManager)
            {
                _overlayManager.TriggerFeedback(delta > 0f
                    ? OverlayManager.Feedback.VolumeUp
                    : OverlayManager.Feedback.VolumeDown);
            }
        }
    }

    private Material DuplicateMaterialOnImage(Graphic image)
    {
        // Assign a copy of the material so we aren't modifying the material asset file
        image.material = new Material(image.material);
        return image.material;
    }

    private void SetupPlayPauseButton()
    {
        if (_buttonPlayPause)
        {
            _buttonPlayPause.onClick.AddListener(OnPlayPauseButtonPressed);
            _playPauseMaterial = DuplicateMaterialOnImage(_buttonPlayPause.GetComponent<Image>());
        }
    }

    private void SetupTimeBackForwardButtons()
    {
        if (_buttonTimeBack)
        {
            _buttonTimeBack.onClick.AddListener(OnPlayTimeBackButtonPressed);
        }

        if (_buttonTimeForward)
        {
            _buttonTimeForward.onClick.AddListener(OnPlayTimeForwardButtonPressed);
        }
    }

    private void SetupVolumeButton()
    {
        if (_buttonVolume)
        {
            _buttonVolume.onClick.AddListener(OnVolumeButtonPressed);
            _volumeMaterial = DuplicateMaterialOnImage(_buttonVolume.GetComponent<Image>());
        }
    }

    private void CreateVideoTouchEvents()
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { OnVideoPointerUp(); });
        _videoTouch.triggers.Add(entry);
    }

    private void OnVideoPointerUp()
    {
        bool controlsMostlyVisible = (_controlsGroup.alpha >= 0.5f && _controlsGroup.gameObject.activeSelf);
        if (controlsMostlyVisible)
        {
            TogglePlayPause();
        }
    }

    public void TogglePlayPause()
    {
        if (VideoPlayer != null)
        {
            if (VideoPlayer.isPlaying)
            {
                if (_overlayManager)
                {
                    _overlayManager.TriggerFeedback(OverlayManager.Feedback.Pause);
                }

                Pause();
            }
            else
            {
                if (_overlayManager)
                {
                    _overlayManager.TriggerFeedback(OverlayManager.Feedback.Play);
                }

                Play();
            }
        }
    }

    private void Play()
    {
        if (VideoPlayer != null)
        {
            if (_overlayManager)
            {
                _overlayManager.TriggerFeedback(OverlayManager.Feedback.Play);
            }

            VideoPlayer.Play();
        }
    }

    private void Pause(bool skipFeedback = false)
    {
        if (VideoPlayer != null)
        {
            if (!skipFeedback)
            {
                if (_overlayManager)
                {
                    _overlayManager.TriggerFeedback(OverlayManager.Feedback.Pause);
                }
            }

            VideoPlayer.Pause();
        }
    }

    private void UpdateVolumeSlider()
    {
        if (_sliderVolume)
        {
            if (VideoPlayer)
            {
                _sliderVolume.value = _audioVolume;
            }
        }
    }

    private void CreateTimelineDragEvents()
    {
        EventTrigger trigger = _sliderTime.gameObject.GetComponent<EventTrigger>();
        if (trigger != null)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { OnTimeSliderBeginDrag(); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) => { OnTimeSliderDrag(); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback.AddListener((data) => { OnTimeSliderEndDrag(); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { OnTimelineBeginHover((PointerEventData)data); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((data) => { OnTimelineEndHover((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }
    }

    private bool _isHoveringOverTimeline;

    private void OnTimeSliderBeginDrag()
    {
        if (VideoPlayer != null)
        {
            _wasPlayingBeforeTimelineDrag = VideoPlayer.isPlaying;
            if (_wasPlayingBeforeTimelineDrag)
            {
                VideoPlayer.Pause();
            }

            OnTimeSliderDrag();
        }
    }

    private void OnTimeSliderDrag()
    {
        if (VideoPlayer != null)
        {
            VideoPlayer.time = double.Parse((_sliderTime.value * VideoPlayer.length).ToString("0."));
            _isHoveringOverTimeline = true;
        }
    }

    private void OnTimeSliderEndDrag()
    {
        if (VideoPlayer != null)
        {
            if (_wasPlayingBeforeTimelineDrag)
            {
                VideoPlayer.Play();
                _wasPlayingBeforeTimelineDrag = false;
            }
        }
    }

    private void OnTimelineBeginHover(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            _isHoveringOverTimeline = true;
            _sliderTime.transform.localScale = new Vector3(1f, 2.5f, 1f);
        }
    }

    private void OnTimelineEndHover(PointerEventData eventData)
    {
        _isHoveringOverTimeline = false;
        _sliderTime.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void CreateVolumeSliderEvents()
    {
        if (_sliderVolume != null)
        {
            EventTrigger trigger = _sliderVolume.gameObject.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((data) => { OnVolumeSliderDrag(); });
                trigger.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback.AddListener((data) => { OnVolumeSliderDrag(); });
                trigger.triggers.Add(entry);
            }
        }
    }

    private void OnVolumeSliderDrag()
    {
        if (VideoPlayer != null)
        {
            _audioVolume = _sliderVolume.value;
            ApplyAudioVolume();
        }
    }

    private void ApplyAudioVolume()
    {
        if (VideoPlayer != null)
        {
            VideoPlayer.SetDirectAudioVolume(0, (_audioVolume * _audioFade));
        }
    }

    private void OnPlayPauseButtonPressed()
    {
        TogglePlayPause();
    }

    private void OnPlayTimeBackButtonPressed()
    {
        SeekRelative(-_jumpDeltaTime);
    }

    private void OnPlayTimeForwardButtonPressed()
    {
        SeekRelative(_jumpDeltaTime);
    }

    private void OnVolumeButtonPressed()
    {
        ToggleMute();
    }

    public void SeekRelative(float deltaTime)
    {
        if (VideoPlayer != null)
        {
            double time = VideoPlayer.time + deltaTime;
            time = System.Math.Max(time, 0);
            time = System.Math.Min(time, VideoPlayer.length);
            VideoPlayer.time = time;

            if (_overlayManager)
            {
                _overlayManager.TriggerFeedback(deltaTime > 0f
                    ? OverlayManager.Feedback.SeekForward
                    : OverlayManager.Feedback.SeekBack);
            }
        }
    }

    public void ToggleMute()
    {
        if (VideoPlayer != null)
        {
            if (VideoPlayer.GetDirectAudioMute(0))
            {
                VideoPlayer.SetDirectAudioMute(0, false);
            }
            else
            {
                VideoPlayer.SetDirectAudioMute(0, true);
            }
        }
    }
}