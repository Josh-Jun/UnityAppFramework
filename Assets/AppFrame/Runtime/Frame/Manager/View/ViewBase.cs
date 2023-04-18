using System;
using System.Collections;
using System.Collections.Generic;
using AppFrame.Enum;
using AppFrame.Tools;
using DG.Tweening;
using UnityEngine;

namespace AppFrame.View
{
    public class ViewBase : EventBaseMono
    {
        public bool ViewActive { get { return gameObject.activeSelf; } }
        public ViewMold ViewMold { get; set; }

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

        /// <summary>设置窗体显/隐</summary>
        public void SetViewActive(bool isActive = true)
        {
            if (this == null) return;

            if (!isActive) CloseView();

            if (gameObject != null)
            {
                if (gameObject.activeSelf != isActive)
                {
                    gameObject.SetActive(isActive);
                }
            }

            if (isActive) OpenView();
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