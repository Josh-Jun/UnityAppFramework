using System.Collections.Generic;
using AppFrame.Tools;
using DG.Tweening;
using UnityEngine;

namespace AppFrame.View
{
    public static class ViewDeveloper
    {
        private static Sequence TweenSequence;
        private static List<Tweener> Tweeners = new List<Tweener>();

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
            CanvasGroup canvasGroup = view.TryGetComponent<CanvasGroup>();
            canvasGroup.alpha = from;
            Tweeners.Add(canvasGroup.DOFade(to, duration).SetEase(Ease.Linear).Pause());
            return view;
        }

        public static void Play(this ViewBase view, bool isOpenView)
        {
            if(isOpenView) view.SetViewActive(isOpenView);
            TweenSequence = DOTween.Sequence();
            Tweener tweener;
            for (int i = 0; i < Tweeners.Count; i++)
            {
                tweener = Tweeners[i];
                TweenSequence.Join(tweener);
            }
            TweenSequence.OnComplete(() =>
            {
                Tweeners.Clear();
                if(!isOpenView) view.SetViewActive(isOpenView);
            });
            TweenSequence.Play();
        }
    }
}