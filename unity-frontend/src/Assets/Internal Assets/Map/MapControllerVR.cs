using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controller for interacting with the map for the VR version
/// </summary>
public class MapControllerVR : MapController, ILaserTarget, ILaserColor
{
    /// <summary>
    /// The color of the laser when the map is being targeted
    /// </summary>
    Color ILaserColor.C => Color.green;


    /// <summary>
    /// How far away from player to render map
    /// </summary>
    private const float renderDistance = 7;

    /// <summary>
    /// How far away player can move before closing map automatically
    /// </summary>
    private const float maxDist = 20;

    protected new void Start()
    {
        if (mapCanvas == null)
        {
            Debug.LogWarning("MapCanvas not instantiated, defaulting to parent object");
            mapCanvas = transform.parent.gameObject;
        }

        base.Start();
    }

    /// <summary>
    /// Open/Close map
    /// </summary>
    /// <param name="show"></param>
    public new void ToggleMap(bool show)
    {
        if (show)
        {
            mapCanvas.transform.position = player.transform.position + player.transform.forward * renderDistance + new Vector3(0f, 1f, 0f);
            mapCanvas.transform.rotation = player.transform.rotation;
        }
        mapCanvas.SetActive(show);
    }

    public new void ToggleMap()
    {
        ToggleMap(!mapCanvas.activeSelf);
    }

    /// <summary>
    /// Checks how far the player is from the map and closes it if the player is too far away
    /// Compares using the maxDist param
    /// </summary>
    public void CheckMapDistance()
    {
        float dist = Vector3.Distance(mapCanvas.transform.position, player.transform.position);
        if (dist > maxDist)
        {
            mapCanvas.SetActive(false);
        }
    }

    /// <summary>
    /// Teleport user to selected location on map
    /// </summary>
    /// <param name="target">Information about the hit location</param>
    /// <param name="origin">Not needed</param>
    void ILaserTarget.OnLaserHit(RaycastHit target, GameObject origin)
    {
        Teleport(target.point);
    }
}
