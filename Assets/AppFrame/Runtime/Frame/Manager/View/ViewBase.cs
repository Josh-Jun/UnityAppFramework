﻿using System;
using System.Collections;
using System.Collections.Generic;
using AppFrame.Enum;
using AppFrame.Tools;
using DG.Tweening;
using UnityEngine;

namespace AppFrame.View
{
    public class ViewTweenData
    {
        public TweenMold mold;
        public Vector3 from;
        public Vector3 to;
        public float duration;
    }
    public class ViewBase : EventBaseMono
    {
        public bool ViewActive { get { return gameObject.activeSelf; } }
        public ViewMold ViewMold { get; set; }
        public List<ViewTweenData> ViewTweenDataList { get; set; } = new List<ViewTweenData>();

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
            //显隐
            AddEventMsg<bool>(name, SetViewActive);
        }

        /// <summary>打开窗口</summary>
        protected virtual void OpenView()
        {
            SetAsLastSibling();
        }

        /// <summary>关闭窗口</summary>
        protected virtual void CloseView()
        {
            
        }

        private Sequence TweenSequence;
        /// <summary>设置窗体显/隐</summary>
        public void SetViewActive(bool isActive = true)
        {
            if (this == null) return;
            
            if (ViewTweenDataList.Count > 0)
            {
                TweenSequence = DOTween.Sequence();
                for (int i = 0; i < ViewTweenDataList.Count; i++)
                {
                    ViewTweenData data = ViewTweenDataList[i];
                    switch (data.mold)
                    {
                        case TweenMold.Move:
                            transform.localPosition = data.from;
                            TweenSequence.Join(transform.DOLocalMove(data.to, data.duration).SetEase(Ease.Linear));
                            break;
                        case TweenMold.Rotate:
                            transform.localEulerAngles = data.from;
                            TweenSequence.Join(transform.DOLocalRotate(data.to, data.duration).SetEase(Ease.Linear));
                            break;
                        case TweenMold.Scale:
                            transform.localScale = data.from;
                            TweenSequence.Join(transform.DOScale(data.to, data.duration).SetEase(Ease.Linear));
                            break;
                        case TweenMold.Fade:
                            CanvasGroup canvasGroup = this.TryGetComponent<CanvasGroup>();
                            canvasGroup.alpha = data.from.x;
                            TweenSequence.Join(canvasGroup.DOFade(data.to.x, data.duration).SetEase(Ease.Linear));
                            break;
                    }
                }
                
                if (isActive)
                {
                    SetActive(isActive);
                }
                else
                {
                    TweenSequence.OnComplete(() =>
                    {
                        ViewTweenDataList.Clear();
                        SetActive(isActive);
                    });
                }
            }
            else
            {
                SetActive(isActive);
            }
        }

        private void SetActive(bool isActive)
        {
            if (!isActive)
            {
                CloseView();
            }
            if (gameObject != null)
            {
                if (gameObject.activeSelf != isActive)
                {
                    gameObject.SetActive(isActive);
                }
            }
            if (isActive)
            {
                OpenView();
            }
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
    }
}