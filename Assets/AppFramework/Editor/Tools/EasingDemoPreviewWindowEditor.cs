using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DG.Tweening;
public class EasingDemoPreviewWindowEditor : EditorWindow
{
    [MenuItem("Tools/My ToolsWindow/Dotween Easing Demo", false, 5)]
    public static void ShowWindow()
    {
        GetWindowWithRect<EasingDemoPreviewWindowEditor>(new Rect(0, 0, 400, 480), false, "Dotween Easing Demo");
    }
    const int POINT_COUNT = 30;
    const float TIME_DELTATIME = 0.02f;
    const int POINT_LENGTH = 10;
    Ease _ease = Ease.Linear;
    private int _easeEnumLength = 0;
    Vector2 _rectStartPos = new Vector2(50, 130);
    private Vector3 _ballTmpPos = new Vector3(85, 165, -10);
    private float _ballMoveTimer;
    private float _ballMoveDuration = 5;
    private bool _isBallMove = false;
    private Vector2 _ballTargetPos;
    private Vector3 _ballStartPos;
    private Texture _ballTex;
    private Texture _bgTex;
    private float _overshootOrAmplitude = DOTween.defaultEaseOvershootOrAmplitude; // 过冲或振幅
    private float _period = DOTween.defaultEasePeriod; // 周期
    private void OnEnable()
    {
        var easeArray = System.Enum.GetValues(typeof(Ease));
        _easeEnumLength = easeArray.Length - 2;
        _ballTex = GetBallTexture();
        _bgTex = GetBGTex();
        EditorApplication.update += Update;
    }
    private void Update()
    {
        if (_isBallMove)
        {
            _ballMoveTimer += TIME_DELTATIME;
            if (_ballMoveTimer > _ballMoveDuration)
            {
                _ballMoveTimer = _ballMoveDuration;
                _isBallMove = false;
            }
            var lerp = Evaluate(_ease, _ballMoveTimer, _ballMoveDuration, _overshootOrAmplitude, _period);
            _ballTmpPos.x = _ballStartPos.x + (_ballTargetPos.x - _ballStartPos.x) * lerp;
            _ballTmpPos.y = _ballStartPos.y + (_ballTargetPos.y - _ballStartPos.y) * lerp;
            Repaint();
        }
    }
    private void OnGUI()
    {
        HandleEvent();
        DrawBgGridUI();
        DrawCurveUI();
        DrawballUI();
    }
    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }
    private void OnDestroy()
    {
        DestroyImmediate(_ballTex);
        DestroyImmediate(_bgTex);
    }
    void HandleEvent()
    {
        if (Event.current.type == EventType.KeyDown)
        {
            int e = (int)_ease;
            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    e--;
                    if (e < 0)
                    {
                        e = _easeEnumLength - 1;
                    }
                    _ease = (Ease)e;
                    break;
                case KeyCode.DownArrow:
                    e++;
                    if (e > _easeEnumLength - 1)
                    {
                        e = 0;
                    }
                    _ease = (Ease)e;
                    break;
            }
            _isBallMove = false;
            Repaint();
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (Event.current.mousePosition.y < _rectStartPos.y)
            {
                return;
            }
            _isBallMove = true;
            _ballStartPos = _ballTmpPos;
            _ballTargetPos = Event.current.mousePosition;
            _ballMoveTimer = 0;
        }
    }
    void DrawBgGridUI()
    {
        GUI.DrawTexture(new Rect(_rectStartPos.x, _rectStartPos.y, POINT_COUNT * POINT_LENGTH, POINT_COUNT * POINT_LENGTH), _bgTex);
    }
    void DrawCurveUI()
    {
        for (int i = 0; i < POINT_COUNT; i++)
        {
            var time = i * 1f;
            var value = POINT_COUNT * Evaluate(_ease, time, POINT_COUNT, _overshootOrAmplitude, _period);
            var p1 = new Vector2(_rectStartPos.x + time * POINT_LENGTH, _rectStartPos.y + POINT_COUNT * POINT_LENGTH - value * POINT_LENGTH);
            if (i >= POINT_COUNT)
            {
                continue;
            }
            var time2 = (i + 1) * 1f;
            var value2 = POINT_COUNT * Evaluate(_ease, time2, POINT_COUNT, _overshootOrAmplitude, _period);
            var p2 = new Vector2(_rectStartPos.x + time2 * POINT_LENGTH, _rectStartPos.y + POINT_COUNT * POINT_LENGTH - value2 * POINT_LENGTH);

            Handles.color = Color.red;
            Handles.DrawLine(p1, p2);
        }
        Handles.color = Color.white;
    }
    void DrawballUI()
    {
        _ease = (Ease)EditorGUILayout.EnumPopup("Easing:", _ease);
        _ballMoveDuration = EditorGUILayout.FloatField("Duration:", _ballMoveDuration);
        _overshootOrAmplitude = EditorGUILayout.FloatField("OvershootOrAmplitude:", _overshootOrAmplitude);
        _period = EditorGUILayout.FloatField("Period:", _period);
        var rect = new Rect(_ballTmpPos.x - _ballTex.width / 2, _ballTmpPos.y - _ballTex.height / 2, _ballTex.width, _ballTex.height);
        GUI.DrawTexture(rect, _ballTex);
    }
    Texture GetBallTexture()
    {
        var r = 32;
        var tex = new Texture2D(r * 2, r * 2, TextureFormat.ARGB32, false);
        var center = new Vector2(r, r);
        Color c1 = Color.white;
        for (int y = 0; y < tex.height; ++y)
        {
            for (int x = 0; x < tex.width; ++x)
            {
                var dis = Vector2.Distance(new Vector2(x, y), center);
                c1.a = 1 - Evaluate(Ease.InExpo, dis, r, _overshootOrAmplitude, _period);
                tex.SetPixel(x, y, c1);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.hideFlags = HideFlags.HideAndDontSave;
        return tex;
    }
    Texture GetBGTex()
    {
        var tex = new Texture2D(64, 64);
        Color c1 = new Color(0.5f, 0.5f, 0.5f, 0.82f);
        for (int y = 0; y < tex.height; ++y)
        {
            for (int x = 0; x < tex.width; ++x)
            {
                tex.SetPixel(x, y, c1);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.hideFlags = HideFlags.HideAndDontSave;
        return tex;
    }
    float Evaluate(Ease easeType, float time, float duration, float overshootOrAmplitude, float period)
    {
        return DG.Tweening.Core.Easing.EaseManager.Evaluate(easeType, null, time, duration, overshootOrAmplitude, period);
    }

}