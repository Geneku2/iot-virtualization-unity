using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script that checks if the player selected a location from the map in the menu room and teleport them to that location if so
/// </summary>
public class TeleportFromMenu : MonoBehaviour
{
    [SerializeField]
    private MapController mapController;

    void Start()
    {
       //if (TransitionModule.shouldTeleport)
       // {
       //     TransitionModule.shouldTeleport = false;
       //     mapController.TeleportFromMenu(TransitionModule.coords);
       // } 
    }
}
