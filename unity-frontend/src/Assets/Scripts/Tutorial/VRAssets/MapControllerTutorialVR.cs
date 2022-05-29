using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Map controller for tutorial only, used to advance state
/// </summary>
public class MapControllerTutorialVR : MapControllerVR, ILaserTarget
{
    [SerializeField]
    private TutorialControllerVR tutorial;

    /// <summary>
    /// Teleport user to selected location on map
    /// </summary>
    /// <param name="target">Information about the hit location</param>
    /// <param name="origin">Not needed</param>
    void ILaserTarget.OnLaserHit(RaycastHit target, GameObject origin)
    {
        tutorial.NextState(TutorialControllerVR.State.MapTele);
        Teleport(target.point);
    }
}
