using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace App.Core.Tools
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class EventColliderTrigger : MonoBehaviour
    {
        [Serializable]
        public class TriggerEvent : UnityEvent<GameObject>
        {
        }

        // Event delegates triggered.
        [Header("OnTrigger Event")] [FormerlySerializedAs("onEnter")] [Space] [SerializeField]
        private TriggerEvent m_OnEnter = new TriggerEvent();

        public TriggerEvent onTriggerEnter
        {
            get => m_OnEnter;
            set => m_OnEnter = value;
        }

        // Event delegates triggered.
        [FormerlySerializedAs("onStay")] [SerializeField]
        private TriggerEvent m_OnStay = new TriggerEvent();

        public TriggerEvent onTriggerStay
        {
            get => m_OnStay;
            set => m_OnStay = value;
        }

        // Event delegates triggered.
        [FormerlySerializedAs("onExit")] [SerializeField]
        private TriggerEvent m_OnExit = new TriggerEvent();

        public TriggerEvent onTriggerExit
        {
            get => m_OnExit;
            set => m_OnExit = value;
        }

        // 进入触发器
        private void OnTriggerEnter(Collider trigger)
        {
            m_OnEnter.Invoke(trigger.gameObject);
        }

        // 停留触发器
        private void OnTriggerStay(Collider trigger)
        {
            m_OnStay.Invoke(trigger.gameObject);
        }

        // 退出触发器
        private void OnTriggerExit(Collider trigger)
        {
            m_OnExit.Invoke(trigger.gameObject);
        }
    }
}
