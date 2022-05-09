using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PicoVRManager : SingletonMonoEvent<PicoVRManager>
{
    private GameObject locomotionSystem;
    private XRInteractionManager _interactionManager;
    private ActionBasedControllerManager leftHand;
    private ActionBasedControllerManager rithtHand;
    private Camera _mainCamera;
    public XRInteractionManager InteractionManager { get { return _interactionManager; } }
    public GameObject LeftController { get { return leftHand.baseControllerGameObject; } }
    public GameObject RightController { get { return rithtHand.baseControllerGameObject; } }
    public Camera MainCamera { get { return _mainCamera; } }
    private void Awake()
    {
        leftHand = this.FindComponent<ActionBasedControllerManager>("XR Origin/Camera Offset/LeftHand");
        rithtHand = this.FindComponent<ActionBasedControllerManager>("XR Origin/Camera Offset/RightHand");
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
        rithtHand.SetBaseController(enable);
    }
    public void SetTeleportController(bool enable)
    {
        leftHand.SetTeleportController(enable);
        rithtHand.SetTeleportController(enable);
    }
    public void SetTeleportEnable(bool enable)
    {
        locomotionSystem.GetComponent<TeleportationProvider>().enabled = enable;
        locomotionSystem.GetComponent<ActionBasedSnapTurnProvider>().enabled = enable;
        locomotionSystem.GetComponent<ActionBasedContinuousMoveProvider>().enabled = enable;
    }
}
