﻿using System;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Tools;
using DG.Tweening;
using UnityEngine;

namespace App.Core.Master
{
    public class ViewBase : EventBaseMono
    {
        public bool ViewActive => gameObject.activeSelf;
        public ViewMold Mold { get; set; }
        private static Sequence TweenSequence;
        private static readonly List<Tweener> Tweeners = new List<Tweener>();


        [Obsolete("此方法已弃用，请使用InitWindow方法", true)]
        protected virtual void Awake()
        {
            InitView();
            RegisterEvent();
        }

        [Obsolete("此方法已弃用，请使用RegisterEvent方法", true)]
        protected virtual void Start()
        {
        }

        /// <summary>初始化UI窗口</summary>
        protected virtual void InitView()
        {
        }

        /// <summary>注册消息事件,默认删除此事件</summary>
        protected virtual void RegisterEvent()
        {
        }

        /// <summary>打开窗口</summary>
        public void OpenView(object obj = null)
        {
            transform.SetAsLastSibling();
            if (gameObject.activeSelf)
            {
                Tweeners.Clear();
                return;
            }
            gameObject.SetActive(true);
            if (HasEvent($"Open{name}"))
                SendEventMsg($"Open{name}", obj);
            if (Tweeners.Count <= 0) return;
            TweenSequence = DOTween.Sequence();
            foreach (var tweener in Tweeners)
            {
                TweenSequence.Join(tweener);
            }
            TweenSequence.OnComplete(() =>
            {
                Tweeners.Clear();
            });
            TweenSequence.Play();
        }

        /// <summary>关闭窗口</summary>
        public void CloseView()
        {
            if (!gameObject.activeSelf)
            {
                Tweeners.Clear();
                return;
            }
            if (Tweeners.Count <= 0)
            {
                gameObject.SetActive(false);
                if (HasEvent($"Close{name}"))
                    SendEventMsg($"Close{name}");
            }
            else
            {
                TweenSequence = DOTween.Sequence();
                foreach (var tweener in Tweeners)
                {
                    TweenSequence.Join(tweener);
                }
                TweenSequence.OnComplete(() =>
                {
                    Tweeners.Clear();
                    gameObject.SetActive(false);
                    if (HasEvent($"Close{name}"))
                        SendEventMsg($"Close{name}");
                });
                TweenSequence.Play();
            }
        }
        
        public void AddTweener(Tweener tweener)
        {
            Tweeners.Add(tweener);
        }

        public void SetAsLastSibling()
        {
            transform.SetAsLastSibling();
        }

        public void SetAsFirstSibling()
        {
            transform.SetAsFirstSibling();
        }

        public void SetSiblingIndex(int index)
        {
            transform.SetSiblingIndex(index);
        }

        public void SetParent(Transform parent, bool worldPositionStays = false)
        {
            transform.SetParent(parent, worldPositionStays);
        }
    }
}