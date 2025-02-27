using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using App.Core.Tools;

namespace App.Core.Master
{
    public static class ViewDeveloper
    {
        private static Sequence TweenSequence;
        private static readonly List<Tweener> Tweeners = new List<Tweener>();

        public static ViewBase Move(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.transform.localPosition = from;
            Tweeners.Add(view.transform.DOLocalMove(to, duration).SetEase(Ease.Linear).Pause());
            return view;
        }

        public static ViewBase Rotate(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.transform.localEulerAngles = from;
            Tweeners.Add(view.transform.DOLocalRotate(to, duration).SetEase(Ease.Linear).Pause());
            return view;
        }

        public static ViewBase Scale(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.transform.localScale = from;
            Tweeners.Add(view.transform.DOScale(to, duration).SetEase(Ease.Linear).Pause());
            return view;
        }

        public static ViewBase Fade(this ViewBase view, float from, float to, float duration)
        {
            var canvasGroup = view.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = from;
            Tweeners.Add(canvasGroup.DOFade(to, duration).SetEase(Ease.Linear).Pause());
            return view;
        }

        public static void Play(this ViewBase view, bool isOpenView)
        {
            if(isOpenView) view.SetViewActive(true);
            TweenSequence = DOTween.Sequence();
            foreach (var tweener in Tweeners)
            {
                TweenSequence.Join(tweener);
            }
            TweenSequence.OnComplete(() =>
            {
                Tweeners.Clear();
                if(!isOpenView) view.SetViewActive(false);
            });
            TweenSequence.Play();
        }
    }
}