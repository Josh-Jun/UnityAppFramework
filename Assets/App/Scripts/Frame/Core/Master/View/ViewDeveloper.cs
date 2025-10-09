using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using App.Core.Tools;

namespace App.Core.Master
{
    public static class ViewDeveloper
    {
        public static ViewBase Move(this ViewBase view, Vector3 to, float duration)
        {
            var tweener = view.transform.RectTransform().DOAnchorPos(to, duration).SetEase(Ease.Linear).Pause();
            view.AddTweener(tweener);
            return view;
        }

        public static ViewBase Rotate(this ViewBase view, Vector3 to, float duration)
        {
            var tweener = view.transform.DOLocalRotate(to, duration).SetEase(Ease.Linear).Pause();
            view.AddTweener(tweener);
            return view;
        }

        public static ViewBase Scale(this ViewBase view, Vector3 to, float duration)
        {
            var tweener = view.transform.DOScale(to, duration).SetEase(Ease.Linear).Pause();
            view.AddTweener(tweener);
            return view;
        }

        public static ViewBase Fade(this ViewBase view, float to, float duration)
        {
            var canvasGroup = view.GetOrAddComponent<CanvasGroup>();
            var tweener = canvasGroup.DOFade(to, duration).SetEase(Ease.Linear).Pause();
            view.AddTweener(tweener);
            return view;
        }
    }
}