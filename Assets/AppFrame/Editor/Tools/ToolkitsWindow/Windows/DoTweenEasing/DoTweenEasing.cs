using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppFrame.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using UnityEditor;
using UnityEditor.UIElements;

namespace AppFrame.Editor
{
    public class DoTweenEasing : IToolkitEditor
    {

        private const int POINT_COUNT = 30;
        private const float TIME_DELTATIME = 0.02f;
        private readonly float _overshootOrAmplitude = DOTween.defaultEaseOvershootOrAmplitude; // 过冲或振幅
        private readonly float _period = DOTween.defaultEasePeriod; // 周期
        private float duration = 5f;
        private Ease ease = Ease.Linear;
        private Slider animationSlider;
        private EnumField easeEnumField;
        private CurveField easeCurveField;
        private FloatField durationField;
        private Button btnPlay;
        
        public void OnCreate(VisualElement root)
        {
            animationSlider = root.Q<Slider>("Animation");
            easeEnumField = root.Q<EnumField>("Easing");
            easeCurveField = root.Q<CurveField>("Curve");
            durationField = root.Q<FloatField>("Duration");
            btnPlay = root.Q<Button>("Play");
            btnPlay.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("arrow-right@2x").image);;
            
            easeEnumField.value = Ease.Linear;
            easeCurveField.value = EaseToCurve(Ease.Linear);
            easeEnumField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                isPlaying = false;
                animationSlider.value = 0;
                time = 0f;
                var ease = (Ease)System.Enum.Parse(typeof(Ease), evt.newValue);
                easeCurveField.value = EaseToCurve(ease);
                this.ease = ease;
                isPlaying = true;
            });
            easeCurveField.SetEnabled(false);
            easeCurveField.renderMode = CurveField.RenderMode.Mesh;
            animationSlider.SetEnabled(false);
            
            durationField.RegisterCallback<ChangeEvent<float>>((evt) =>
            {
                duration = evt.newValue;
            });
            
            btnPlay.RegisterCallback<MouseUpEvent>((ent) =>
            {
                isPlaying = false;
                animationSlider.value = 0;
                time = 0f;
                isPlaying = true;
            });
        }

        private float time = 0f;

        private bool isPlaying = false;
        public void OnUpdate()
        {
            if (isPlaying)
            {
                time += TIME_DELTATIME;
                if (time > duration)
                {
                    time = duration;
                    isPlaying = false;
                }
                var lerp = Evaluate(ease, time, duration, _overshootOrAmplitude, _period);
                animationSlider.value = lerp;
            }
        }

        public void OnDestroy()
        {
            
        }
        
        private AnimationCurve EaseToCurve(Ease ease)
        {
            // 创建一个新的AnimationCurve
            AnimationCurve curve = new AnimationCurve();
 
            for (int i = 0; i < POINT_COUNT; i++)
            {
                var t1 = i * 1f;
                var v1 = POINT_COUNT * Evaluate(ease, t1, POINT_COUNT, _overshootOrAmplitude, _period);
                curve.AddKey(t1, v1);

                var t2 = (i + 1) * 1f;
                var v2 = POINT_COUNT * Evaluate(ease, t2, POINT_COUNT, _overshootOrAmplitude, _period);
                curve.AddKey(t2, v2);
            }
 
            return curve;
        }
        
        private float Evaluate(Ease easeType, float time, float duration, float overshootOrAmplitude, float period)
        {
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(easeType, null, time, duration, overshootOrAmplitude, period);
        }
    }
}
