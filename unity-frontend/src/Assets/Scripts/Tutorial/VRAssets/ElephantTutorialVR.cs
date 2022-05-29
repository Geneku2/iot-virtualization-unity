using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used only in the tutorial to advance state
/// </summary>
public class ElephantTutorialVR : Elephant, ILaserTarget
{
    [SerializeField]
    private TutorialControllerVR tutorial;

    /// <summary>
    /// Attach device to animal and update the device's attached animals list
    /// </summary>
    /// <param name="target">not used</param>
    /// <param name="origin">The origin of the raycast, assumed to be the player</param>
    void ILaserTarget.OnLaserHit(RaycastHit target, GameObject origin)
    {
        if (origin != null)
        {
            SelectedDeviceController controller = origin.GetComponentInChildren<SelectedDeviceController>();
            if (controller != null)
            {
                Device d = controller.Device;
                if (d != null)
                {
                    tutorial.NextState(TutorialControllerVR.State.AttachDevice);
                    AttachDevice(d);     
                }
                else
                {
                    Debug.LogWarning("No device selected in GameObject: " + origin);
                }
            }
            else
            {
                Debug.LogWarning("No SelectedDeviceController component in GameObject: " + origin);
            }
        }
        else
        {
            Debug.LogWarning("No GameObject passed in");
        }
    }
}
