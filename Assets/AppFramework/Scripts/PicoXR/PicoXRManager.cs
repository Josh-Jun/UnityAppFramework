using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.PXR;
using UnityEngine.XR;

public class PicoXRManager : SingletonMonoEvent<PicoXRManager>
{
    private GameObject locomotionSystem;
    private XRInteractionManager _interactionManager;
    private ActionBasedControllerManager leftHand;
    private ActionBasedControllerManager rightHand;
    private Camera _mainCamera;
    public XRInteractionManager InteractionManager { get { return _interactionManager; } }
    public GameObject LeftController { get { return leftHand.baseControllerGameObject; } }
    public GameObject RightController { get { return rightHand.baseControllerGameObject; } }
    public Camera MainCamera { get { return _mainCamera; } }
    private void Awake()
    {
        leftHand = this.FindComponent<ActionBasedControllerManager>("XR Origin/Camera Offset/LeftHand");
        rightHand = this.FindComponent<ActionBasedControllerManager>("XR Origin/Camera Offset/RightHand");
        _interactionManager = this.FindComponent<XRInteractionManager>("XR Interaction Manager");
        locomotionSystem = this.FindGameObject("Locomotion System");
        _mainCamera = this.FindComponent<Camera>("XR Origin/Camera Offset/Main Camera");
    }
    private void Start()
    {
        SetBaseController(false);
        SetTeleportController(false);

        //SetTeleportEnable(false);
    }
    public void SetBaseController(bool enable)
    {
        leftHand.SetBaseController(enable);
        rightHand.SetBaseController(enable);
    }
    public void SetTeleportController(bool enable)
    {
        leftHand.SetTeleportController(enable);
        rightHand.SetTeleportController(enable);
    }
    public void SetTeleportEnable(bool enable)
    {
        locomotionSystem.GetComponent<TeleportationProvider>().enabled = enable;
        locomotionSystem.GetComponent<ActionBasedSnapTurnProvider>().enabled = enable;
        locomotionSystem.GetComponent<ActionBasedContinuousMoveProvider>().enabled = enable;
    }
    public void SetControllerVibration(float strength, int time, PXR_Input.Controller controller)
    {
        PXR_Input.SetControllerVibration(strength, time, controller);
    }

    #region HandControllerTrackedEvent
    [Serializable]
    public class TrackedEvent : UnityEvent<XRNode> { }
    [Serializable]
    public class Entry
    {
        public TrackedEventType eventID = TrackedEventType.MenuDownEvent;
        public TrackedEvent callback = new TrackedEvent();
    }

    [SerializeField]
    private List<Entry> m_Delegates;
    public List<Entry> Triggers
    {
        get
        {
            if (m_Delegates == null)
                m_Delegates = new List<Entry>();
            return m_Delegates;
        }
        set { m_Delegates = value; }
    }
    private void Execute(TrackedEventType id, XRNode node)
    {
        for (int i = 0, imax = Triggers.Count; i < imax; ++i)
        {
            var ent = Triggers[i];
            if (ent.eventID == id && ent.callback != null)
                ent.callback.Invoke(node);
        }
    }
    public void Update()
    {
        #region Trigger
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerDown))
        {
            if (leftTriggerDown)
            {
                Execute(TrackedEventType.TriggerDownEvent, XRNode.LeftHand);
            }
            else
            {
                Execute(TrackedEventType.TriggerUpEvent, XRNode.LeftHand);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerDown))
        {
            if (rightTriggerDown)
            {
                Execute(TrackedEventType.TriggerDownEvent, XRNode.RightHand);
            }
            else
            {
                Execute(TrackedEventType.TriggerUpEvent, XRNode.RightHand);
            }
        }
        #endregion
        #region Menu
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.menuButton, out bool leftMenuDown))
        {
            if (leftMenuDown)
            {
                Execute(TrackedEventType.MenuDownEvent, XRNode.LeftHand);
            }
            else
            {
                Execute(TrackedEventType.MenuUpEvent, XRNode.LeftHand);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.menuButton, out bool rightMenuDown))
        {
            if (rightMenuDown)
            {
                Execute(TrackedEventType.MenuDownEvent, XRNode.RightHand);
            }
            else
            {
                Execute(TrackedEventType.MenuUpEvent, XRNode.RightHand);
            }
        }
        #endregion
        #region Grip
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.gripButton, out bool leftGripDown))
        {
            if (leftGripDown)
            {
                Execute(TrackedEventType.GripDownEvent, XRNode.LeftHand);
            }
            else
            {
                Execute(TrackedEventType.GripUpEvent, XRNode.LeftHand);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.gripButton, out bool rightGripDown))
        {
            if (rightGripDown)
            {
                Execute(TrackedEventType.GripDownEvent, XRNode.RightHand);
            }
            else
            {
                Execute(TrackedEventType.GripUpEvent, XRNode.RightHand);
            }
        }
        #endregion
        #region Joystick
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool leftJoystickDown))
        {
            if (leftJoystickDown)
            {
                Execute(TrackedEventType.JoystickDownEvent, XRNode.LeftHand);
            }
            else
            {
                Execute(TrackedEventType.JoystickUpEvent, XRNode.LeftHand);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool rightJoystickDown))
        {
            if (rightJoystickDown)
            {
                Execute(TrackedEventType.JoystickDownEvent, XRNode.RightHand);
            }
            else
            {
                Execute(TrackedEventType.JoystickUpEvent, XRNode.RightHand);
            }
        }
        #endregion
        #region X/A
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primaryButton, out bool leftXADown))
        {
            if (leftXADown)
            {
                Execute(TrackedEventType.XADownEvent, XRNode.LeftHand);
            }
            else
            {
                Execute(TrackedEventType.XAUpEvent, XRNode.LeftHand);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out bool rightXADown))
        {
            if (rightXADown)
            {
                Execute(TrackedEventType.XADownEvent, XRNode.RightHand);
            }
            else
            {
                Execute(TrackedEventType.XAUpEvent, XRNode.RightHand);
            }
        }
        #endregion
        #region Y/B
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.secondaryButton, out bool leftYBDown))
        {
            if (leftYBDown)
            {
                Execute(TrackedEventType.YBDownEvent, XRNode.LeftHand);
            }
            else
            {
                Execute(TrackedEventType.YBDownEvent, XRNode.LeftHand);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out bool rightYBDown))
        {
            if (rightYBDown)
            {
                Execute(TrackedEventType.YBDownEvent, XRNode.RightHand);
            }
            else
            {
                Execute(TrackedEventType.YBDownEvent, XRNode.RightHand);
            }
        }
        #endregion
    }
    #endregion
}
