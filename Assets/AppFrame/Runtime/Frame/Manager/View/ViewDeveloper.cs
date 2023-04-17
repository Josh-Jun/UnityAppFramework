using System.Collections;
using System.Collections.Generic;
using AppFrame.Enum;
using AppFrame.Tools;
using DG.Tweening;
using UnityEngine;

namespace AppFrame.View
{
    public static class ViewDeveloper
    {
        public static ViewBase Move(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            ViewTweenData data = new ViewTweenData();
            data.mold = TweenMold.Move;
            data.from = from;
            data.to = to;
            data.duration = duration;
            view.ViewTweenDataList.Add(data);
            return view;
        }
        public static ViewBase Rotate(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            ViewTweenData data = new ViewTweenData();
            data.mold = TweenMold.Rotate;
            data.from = from;
            data.to = to;
            data.duration = duration;
            view.ViewTweenDataList.Add(data);
            return view;
        }
        public static ViewBase Scale(this ViewBase view, Vector3 from, Vector3 to, float duration)
        {
            ViewTweenData data = new ViewTweenData();
            data.mold = TweenMold.Scale;
            data.from = from;
            data.to = to;
            data.duration = duration;
            view.ViewTweenDataList.Add(data);
            return view;
        }
        public static ViewBase Fade(this ViewBase view, float from, float to, float duration)
        {
            ViewTweenData data = new ViewTweenData();
            data.mold = TweenMold.Fade;
            data.from = Vector3.one * from;
            data.to = Vector3.one * to;
            data.duration = duration;
            view.ViewTweenDataList.Add(data);
            return view;
        }
    }
}