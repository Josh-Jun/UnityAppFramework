using System.Collections;
using System.Collections.Generic;
using AppFrame.Tools;
using DG.Tweening;
using UnityEngine;

namespace AppFrame.View
{
    public static class ViewDeveloper
    {
        public static ViewBase Move(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.IsPlayTween = true;
            view.transform.localPosition = from;
            view.TweenSequence.Join(view.transform.DOLocalMove(to, duration)).Pause();
            return view;
        }
        public static ViewBase Rotate(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.IsPlayTween = true;
            view.transform.localEulerAngles = from;
            view.TweenSequence.Join(view.transform.DOLocalRotate(to, duration)).Pause();
            return view;
        }
        public static ViewBase Scale(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.IsPlayTween = true;
            view.transform.localScale = from;
            view.TweenSequence.Join(view.transform.DOScale(to, duration)).Pause();
            return view;
        }
        public static ViewBase Fade(this ViewBase view, float from, float to, float duration)
        {
            view.IsPlayTween = true;
            CanvasGroup canvasGroup = view.TryGetComponent<CanvasGroup>();
            canvasGroup.alpha = from;
            view.TweenSequence.Join(canvasGroup.DOFade(to, duration)).Pause();
            return view;
        }
        public static ViewBase SetEase(this ViewBase view, Ease ease)
        {
            view.TweenSequence.SetEase(ease);
            return view;
        }
        public static ViewBase SetLoops(this ViewBase view, int loopCount, LoopType loopType)
        {
            view.TweenSequence.SetLoops(loopCount, loopType);
            return view;
        }
    }
}