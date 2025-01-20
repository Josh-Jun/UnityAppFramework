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
            get { return m_OnEnter; }
            set { m_OnEnter = value; }
        }

        // Event delegates triggered.
        [FormerlySerializedAs("onStay")] [SerializeField]
        private TriggerEvent m_OnStay = new TriggerEvent();

        public TriggerEvent onTriggerStay
        {
            get { return m_OnStay; }
            set { m_OnStay = value; }
        }

        // Event delegates triggered.
        [FormerlySerializedAs("onExit")] [SerializeField]
        private TriggerEvent m_OnExit = new TriggerEvent();

        public TriggerEvent onTriggerExit
        {
            get { return m_OnExit; }
            set { m_OnExit = value; }
        }

        // 进入触发器
        void OnTriggerEnter(Collider collider)
        {
            m_OnEnter.Invoke(collider.gameObject);
        }

        // 停留触发器
        void OnTriggerStay(Collider collider)
        {
            m_OnStay.Invoke(collider.gameObject);
        }

        // 退出触发器
        void OnTriggerExit(Collider collider)
        {
            m_OnExit.Invoke(collider.gameObject);
        }
    }
}
