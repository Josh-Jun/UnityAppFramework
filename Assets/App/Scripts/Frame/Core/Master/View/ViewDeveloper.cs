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
            view.transform.RectTransform().anchoredPosition = from;
            var tweener = view.transform.RectTransform().DOAnchorPos(to, duration).SetEase(Ease.Linear).Pause();
            Tweeners.Add(tweener);
            return view;
        }

        public static ViewBase Rotate(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.transform.localEulerAngles = from;
            var tweener = view.transform.DOLocalRotate(to, duration).SetEase(Ease.Linear).Pause();
            Tweeners.Add(tweener);
            return view;
        }

        public static ViewBase Scale(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            view.transform.localScale = from;
            var tweener = view.transform.DOScale(to, duration).SetEase(Ease.Linear).Pause();
            Tweeners.Add(tweener);
            return view;
        }

        public static ViewBase Fade(this ViewBase view, float from, float to, float duration)
        {
            var canvasGroup = view.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = from;
            var tweener = canvasGroup.DOFade(to, duration).SetEase(Ease.Linear).Pause();
            Tweeners.Add(tweener);
            return view;
        }

        public static void Play(this ViewBase view, bool isOpenView)
        {
            if(isOpenView) view.OpenView();
            TweenSequence = DOTween.Sequence();
            foreach (var tweener in Tweeners)
            {
                TweenSequence.Join(tweener);
            }
            TweenSequence.OnComplete(() =>
            {
                Tweeners.Clear();
                if(!isOpenView) view.CloseView();
            });
            TweenSequence.Play();
        }
    }
}