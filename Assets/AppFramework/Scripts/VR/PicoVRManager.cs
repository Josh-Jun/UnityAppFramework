using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PicoVRManager : MonoBehaviour
{
    private GameObject locomotionSystem;
    private XRInteractionManager interactionManager;
    private ActionBasedControllerManager leftController;
    private ActionBasedControllerManager rithtController;
    private void Awake()
    {
        leftController = this.FindComponent<ActionBasedControllerManager>("XR Origin/Camera Offset/LeftHand");
        rithtController = this.FindComponent<ActionBasedControllerManager>("XR Origin/Camera Offset/RightHand");
        interactionManager = this.FindComponent<XRInteractionManager>("XR Interaction Manager");
        locomotionSystem = this.FindGameObject("Locomotion System");
    }
    private void Start()
    {
        SetBaseController(false);
        SetTeleportController(false);

        SetTeleportEnable(false);
    }
    public void SetBaseController(bool enable)
    {
        leftController.SetBaseController(enable);
        rithtController.SetBaseController(enable);
    }
    public void SetTeleportController(bool enable)
    {
        leftController.SetTeleportController(enable);
        rithtController.SetTeleportController(enable);
    }
    public void SetTeleportEnable(bool enable)
    {
        locomotionSystem.GetComponent<TeleportationProvider>().enabled = enable;
        locomotionSystem.GetComponent<ActionBasedSnapTurnProvider>().enabled = enable;
        locomotionSystem.GetComponent<ActionBasedContinuousMoveProvider>().enabled = enable;
    }
}
