/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月28 14:1
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace App.Core.Tools
{
    public class RectTransformListener : UIBehaviour
    {
        [Serializable]
        public class TransformChangeEvent : UnityEvent { }
        
        [Header("OnTransformChange Event")] 
        [Space]
        [SerializeField]
        private TransformChangeEvent m_OnTransformChange = new TransformChangeEvent();

        public TransformChangeEvent onTransformChange
        {
            get => m_OnTransformChange;
            set => m_OnTransformChange = value;
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if(!gameObject.activeInHierarchy) return;
            m_OnTransformChange?.Invoke();
        }

        [Header("OnTransformParentChange Event")]
        [Space]
        [SerializeField]
        private TransformChangeEvent m_OnTransformParentChange = new TransformChangeEvent();

        public TransformChangeEvent onTransformParentChange
        {
            get => m_OnTransformParentChange;
            set => m_OnTransformParentChange = value;
        }
        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            if(!gameObject.activeInHierarchy) return;
            m_OnTransformParentChange?.Invoke();
        }

        [Header("OnBeforeTransformParentChange Event")]
        [Space]
        [SerializeField]
        private TransformChangeEvent m_OnBeforeTransformParentChange = new TransformChangeEvent();

        public TransformChangeEvent onBeforeTransformParentChange
        {
            get => m_OnBeforeTransformParentChange;
            set => m_OnBeforeTransformParentChange = value;
        }
        protected override void OnBeforeTransformParentChanged()
        {
            base.OnBeforeTransformParentChanged();
            if(!gameObject.activeInHierarchy) return;
            m_OnBeforeTransformParentChange?.Invoke();
        }
    }
}