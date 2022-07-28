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
    private XRRayInteractor leftXRRayInteractor;
    private XRRayInteractor rightXRRayInteractor;
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
        leftXRRayInteractor = LeftController.GetComponent<XRRayInteractor>();
        rightXRRayInteractor = RightController.GetComponent<XRRayInteractor>();
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

    public void CameraScreenFade(bool isFade, float time, Action callback = null)
    {
        PXR_ScreenFade screenFade = MainCamera.GetComponent<PXR_ScreenFade>();
        screenFade.fadeTime = time;
        float startAlpha = isFade ? 0f : 1f;
        float endAlpha = !isFade ? 0f : 1f;
        screenFade.StartScreenFade(startAlpha, endAlpha, callback);
    }
    public void SetControllerVibration(float strength, int time, PXR_Input.Controller controller)
    {
        PXR_Input.SetControllerVibration(strength, time, controller);
    }

    #region HandControllerTrackedEvent
    public void AddTrackedEvent(TrackedEventType eventID, UnityAction<XRNode, GameObject> trackedEvent)
    {
        Entry entry = new Entry { eventID = eventID };
        entry.callback.AddListener(trackedEvent);
        Entrys.Add(entry);
    }
    [Serializable]
    public class TrackedEvent : UnityEvent<XRNode, GameObject> { }
    [Serializable]
    public class Entry
    {
        public TrackedEventType eventID = TrackedEventType.MenuDownEvent;
        public TrackedEvent callback = new TrackedEvent();
    }

    [SerializeField]
    private List<Entry> m_Delegates;
    public List<Entry> Entrys
    {
        get
        {
            if (m_Delegates == null)
                m_Delegates = new List<Entry>();
            return m_Delegates;
        }
        set { m_Delegates = value; }
    }
    private void Execute(TrackedEventType id, XRNode node, GameObject target)
    {
        for (int i = 0, imax = Entrys.Count; i < imax; ++i)
        {
            var ent = Entrys[i];
            if (ent.eventID == id && ent.callback != null)
                ent.callback.Invoke(node, target);
        }
    }
    private GameObject leftTarget;
    private GameObject rightTarget;
    
    private bool leftMenu, rightMenu, leftGrip, rightGrip, leftTrigger, rightTrigger, leftJoystick, rightJoystick, leftXA,rightXA,leftYB, rightYB;
    private bool leftMenuDown, rightMenuDown, leftGripDown, rightGripDown, leftTriggerDown, rightTriggerDown, leftJoystickDown, rightJoystickDown, leftXADown,rightXADown,leftYBDown, rightYBDown;
    public void Update()
    {
        #region RaycastTarget
        if (leftXRRayInteractor != null)
        {
            leftXRRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit leftRayHit);
            leftTarget = leftRayHit.collider ? leftRayHit.collider.gameObject : null;
        }
        if (rightXRRayInteractor != null)
        {
            rightXRRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit rightRayHit);
            rightTarget = rightRayHit.collider ? rightRayHit.collider.gameObject : null;
        }
        #endregion
        #region Trigger
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerDown))
        {
            if (leftTrigger != leftTriggerDown)
            {
                leftTrigger = leftTriggerDown;
                if (leftTriggerDown)
                {
                    Execute(TrackedEventType.TriggerDownEvent, XRNode.LeftHand, leftTarget);
                }
                else
                {
                    Execute(TrackedEventType.TriggerUpEvent, XRNode.LeftHand, leftTarget);
                }
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerDown))
        {
            if (rightTrigger != rightTriggerDown)
            {
                rightTrigger = rightTriggerDown;
                if (rightTriggerDown)
                {
                    Execute(TrackedEventType.TriggerDownEvent, XRNode.RightHand, rightTarget);
                }
                else
                {
                    Execute(TrackedEventType.TriggerUpEvent, XRNode.RightHand, rightTarget);
                }
            }
        }
        #endregion
        #region Menu
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.menuButton, out leftMenuDown))
        {
            if (leftMenu != leftMenuDown)
            {
                leftMenu = leftMenuDown;
                if (leftMenuDown)
                {
                    Execute(TrackedEventType.MenuDownEvent, XRNode.LeftHand, leftTarget);
                }
                else
                {
                    Execute(TrackedEventType.MenuUpEvent, XRNode.LeftHand, leftTarget);
                }
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.menuButton, out rightMenuDown))
        {
            if (rightMenu != rightMenuDown)
            {
                rightMenu = rightMenuDown;
                if (rightMenuDown)
                {
                    Execute(TrackedEventType.MenuDownEvent, XRNode.RightHand, rightTarget);
                }
                else
                {
                    Execute(TrackedEventType.MenuUpEvent, XRNode.RightHand, rightTarget);
                }
            }
        }
        #endregion
        #region Grip
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.gripButton, out leftGripDown))
        {
            if (leftGrip != leftGripDown)
            {
                leftGrip = leftGripDown;
                if (leftGripDown)
                {
                    Execute(TrackedEventType.GripDownEvent, XRNode.LeftHand, leftTarget);
                }
                else
                {
                    Execute(TrackedEventType.GripUpEvent, XRNode.LeftHand, leftTarget);
                }
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.gripButton, out rightGripDown))
        {
            if (rightGrip != rightGripDown)
            {
                rightGrip = rightGripDown;
                if (rightGripDown)
                {
                    Execute(TrackedEventType.GripDownEvent, XRNode.RightHand, rightTarget);
                }
                else
                {
                    Execute(TrackedEventType.GripUpEvent, XRNode.RightHand, rightTarget);
                }
            }
        }
        #endregion
        #region Joystick
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out leftJoystickDown))
        {
            if (leftJoystick != leftJoystickDown)
            {
                leftJoystick = leftJoystickDown;
                if (leftJoystickDown)
                {
                    Execute(TrackedEventType.JoystickDownEvent, XRNode.LeftHand, leftTarget);
                }
                else
                {
                    Execute(TrackedEventType.JoystickUpEvent, XRNode.LeftHand, leftTarget);
                }
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out rightJoystickDown))
        {
            if (rightJoystick != rightJoystickDown)
            {
                rightJoystick = rightJoystickDown;
                if (rightJoystickDown)
                {
                    Execute(TrackedEventType.JoystickDownEvent, XRNode.RightHand, rightTarget);
                }
                else
                {
                    Execute(TrackedEventType.JoystickUpEvent, XRNode.RightHand, rightTarget);
                }
            }
        }
        #endregion
        #region X/A
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primaryButton, out leftXADown))
        {
            if (leftXA != leftXADown)
            {
                leftXA = leftXADown;
                if (leftXADown)
                {
                    Execute(TrackedEventType.XADownEvent, XRNode.LeftHand, leftTarget);
                }
                else
                {
                    Execute(TrackedEventType.XAUpEvent, XRNode.LeftHand, leftTarget);
                }
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out rightXADown))
        {
            if (rightXA != rightXADown)
            {
                rightXA = rightXADown;
                if (rightXADown)
                {
                    Execute(TrackedEventType.XADownEvent, XRNode.RightHand, rightTarget);
                }
                else
                {
                    Execute(TrackedEventType.XAUpEvent, XRNode.RightHand, rightTarget);
                }
            }
        }
        #endregion
        #region Y/B
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.secondaryButton, out leftYBDown))
        {
            if (leftYB != leftYBDown)
            {
                leftYB = leftYBDown;
                if (leftYBDown)
                {
                    Execute(TrackedEventType.YBDownEvent, XRNode.LeftHand, leftTarget);
                }
                else
                {
                    Execute(TrackedEventType.YBUpEvent, XRNode.LeftHand, leftTarget);
                }
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out rightYBDown))
        {
            if (rightYB != rightYBDown)
            {
                rightYB = rightYBDown;
                if (rightYBDown)
                {
                    Execute(TrackedEventType.YBDownEvent, XRNode.RightHand, rightTarget);
                }
                else
                {
                    Execute(TrackedEventType.YBUpEvent, XRNode.RightHand, rightTarget);
                }
            }
        }
        #endregion
    }
    #endregion
}
