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
#if UNITY_EDITOR
        LeftController.SetActive(false);
        RightController.SetActive(false);
#endif
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
    public void SetBaseOrigin(GameObject go)
    {
        transform.Find("XR Origin").GetComponent<Unity.XR.CoreUtils.XROrigin>().Origin = go;
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
    public void AddTrackedEvent(TrackedEventType eventID, UnityAction<RaycastHit> trackedEvent)
    {
        Entry entry = Entrys.Find(s => s.eventID == eventID);
        if (entry == null)
        {
            entry = new Entry { eventID = eventID };
        }
        entry.callback.AddListener(trackedEvent);
        Entrys.Add(entry);
    }
    public void RemoveTrackedEvent(TrackedEventType eventID, UnityAction<RaycastHit> trackedEvent)
    {
        Entry entry = Entrys.Find(s => s.eventID == eventID);
        if (entry != null)
        {
            entry.callback.RemoveListener(trackedEvent);
        }
    }
    public void RemoveTrackedEvent(TrackedEventType eventID, bool isClear = false)
    {
        Entry entry = Entrys.Find(s => s.eventID == eventID);
        if (entry != null)
        {
            entry.callback.RemoveAllListeners();
            if (isClear)
            {
                Entrys.Remove(entry);
            }
        }
    }
    public void RemoveAllTrackedEvent()
    {
        Entrys.Clear();
    }
    
    [Serializable]
    public class TrackedEvent : UnityEvent<RaycastHit> { }
    [Serializable]
    public class Entry
    {
        public TrackedEventType eventID = TrackedEventType.LeftMenuDownEvent;
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
    private void Execute(TrackedEventType id, RaycastHit rayHit)
    {
        for (int i = 0, imax = Entrys.Count; i < imax; ++i)
        {
            var ent = Entrys[i];
            if (ent.eventID == id && ent.callback != null)
                ent.callback.Invoke(rayHit);
        }
    }
    private RaycastHit leftRaycastHit;
    private RaycastHit rightRaycastHit;
    
    private bool leftMenu, rightMenu, leftGrip, rightGrip, leftTrigger, rightTrigger, leftJoystick, rightJoystick, leftXA,rightXA,leftYB, rightYB;
    private bool leftMenuDown, rightMenuDown, leftGripDown, rightGripDown, leftTriggerDown, rightTriggerDown, leftJoystickDown, rightJoystickDown, leftXADown,rightXADown,leftYBDown, rightYBDown;
    private bool leftMenuing, rightMenuing, leftGriping, rightGriping, leftTriggering, rightTriggering, leftJoysticking, rightJoysticking, leftXAing, rightXAing, leftYBing, rightYBing;
    public void Update()
    {
        #region RaycastTarget
        if (leftXRRayInteractor != null)
        {
            leftXRRayInteractor.TryGetCurrent3DRaycastHit(out leftRaycastHit);
        }
        if (rightXRRayInteractor != null)
        {
            rightXRRayInteractor.TryGetCurrent3DRaycastHit(out rightRaycastHit);
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
                    leftTriggering = true;
                    Execute(TrackedEventType.LeftTriggerDownEvent, leftRaycastHit);
                }
                else
                {
                    leftTriggering = false;
                    Execute(TrackedEventType.LeftTriggerUpEvent, leftRaycastHit);
                }
            }
            if (leftTriggering)
            {
                Execute(TrackedEventType.LeftTriggeringEvent, leftRaycastHit);
            }

        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerDown))
        {
            if (rightTrigger != rightTriggerDown)
            {
                rightTrigger = rightTriggerDown;
                if (rightTriggerDown)
                {
                    rightTriggering = true;
                    Execute(TrackedEventType.RightTriggerDownEvent, rightRaycastHit);
                }
                else
                {
                    rightTriggering = false;
                    Execute(TrackedEventType.RightTriggerUpEvent, rightRaycastHit);
                }
            }
            if (rightTriggering)
            {
                Execute(TrackedEventType.RightTriggeringEvent, rightRaycastHit);
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
                    leftMenuing = true;
                    Execute(TrackedEventType.LeftMenuDownEvent, leftRaycastHit);
                }
                else
                {
                    leftMenuing = false;
                    Execute(TrackedEventType.LeftMenuUpEvent, leftRaycastHit);
                }
            }
            if (leftMenuing)
            {
                Execute(TrackedEventType.LeftMenuingEvent, leftRaycastHit);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.menuButton, out rightMenuDown))
        {
            if (rightMenu != rightMenuDown)
            {
                rightMenu = rightMenuDown;
                if (rightMenuDown)
                {
                    rightMenuing = true;
                    Execute(TrackedEventType.RightMenuDownEvent, rightRaycastHit);
                }
                else
                {
                    rightMenuing = false;
                    Execute(TrackedEventType.LeftMenuUpEvent, rightRaycastHit);
                }
            }
            if (rightMenuing)
            {
                Execute(TrackedEventType.LeftMenuingEvent, rightRaycastHit);
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
                    leftGriping = true;
                    Execute(TrackedEventType.LeftGripDownEvent, leftRaycastHit);
                }
                else
                {
                    leftGriping = false;
                    Execute(TrackedEventType.LeftGripUpEvent, leftRaycastHit);
                }
            }
            if (leftGriping)
            {
                Execute(TrackedEventType.LeftGripingEvent, leftRaycastHit);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.gripButton, out rightGripDown))
        {
            if (rightGrip != rightGripDown)
            {
                rightGrip = rightGripDown;
                if (rightGripDown)
                {
                    rightGriping = true;
                    Execute(TrackedEventType.RightGripDownEvent, rightRaycastHit);
                }
                else
                {
                    rightGriping = false;
                    Execute(TrackedEventType.RightGripUpEvent, rightRaycastHit);
                }
            }
            if (rightGriping)
            {
                Execute(TrackedEventType.RightGripingEvent, rightRaycastHit);
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
                    leftJoysticking = true;
                    Execute(TrackedEventType.LeftJoystickDownEvent, leftRaycastHit);
                }
                else
                {
                    leftJoysticking = false;
                    Execute(TrackedEventType.LeftJoystickUpEvent, leftRaycastHit);
                }
            }
            if (leftJoysticking)
            {
                Execute(TrackedEventType.LeftJoystickingEvent, leftRaycastHit);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out rightJoystickDown))
        {
            if (rightJoystick != rightJoystickDown)
            {
                rightJoystick = rightJoystickDown;
                if (rightJoystickDown)
                {
                    rightJoysticking = true;
                    Execute(TrackedEventType.RightJoystickDownEvent, rightRaycastHit);
                }
                else
                {
                    rightJoysticking = false;
                    Execute(TrackedEventType.RightJoystickUpEvent, rightRaycastHit);
                }
            }
            if (rightJoysticking)
            {
                Execute(TrackedEventType.RightJoystickingEvent, rightRaycastHit);
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
                    leftXAing = true;
                    Execute(TrackedEventType.LeftXADownEvent, leftRaycastHit);
                }
                else
                {
                    leftXAing = false;
                    Execute(TrackedEventType.LeftXAUpEvent, leftRaycastHit);
                }
            }
            if (leftXAing)
            {
                Execute(TrackedEventType.LeftXAingEvent, leftRaycastHit);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out rightXADown))
        {
            if (rightXA != rightXADown)
            {
                rightXA = rightXADown;
                if (rightXADown)
                {
                    rightXAing = true;
                    Execute(TrackedEventType.RightXADownEvent, rightRaycastHit);
                }
                else
                {
                    rightXAing = false;
                    Execute(TrackedEventType.RightXAUpEvent, rightRaycastHit);
                }
            }
            if (rightXAing)
            {
                Execute(TrackedEventType.RightXAingEvent, rightRaycastHit);
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
                    leftYBing = true;
                    Execute(TrackedEventType.LeftYBDownEvent, leftRaycastHit);
                }
                else
                {
                    leftYBing = false;
                    Execute(TrackedEventType.LeftYBUpEvent, leftRaycastHit);
                }
            }
            if (leftYBing)
            {
                Execute(TrackedEventType.LeftYBingEvent, leftRaycastHit);
            }
        }
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out rightYBDown))
        {
            if (rightYB != rightYBDown)
            {
                rightYB = rightYBDown;
                if (rightYBDown)
                {
                    rightYBing = true;
                    Execute(TrackedEventType.RightYBDownEvent, rightRaycastHit);
                }
                else
                {
                    rightYBing = false;
                    Execute(TrackedEventType.RightYBUpEvent, rightRaycastHit);
                }
            }
            if (rightYBing)
            {
                Execute(TrackedEventType.RightYBingEvent, rightRaycastHit);
            }
        }
        #endregion
    }
    #endregion
}
