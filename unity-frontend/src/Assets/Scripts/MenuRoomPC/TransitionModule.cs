using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the location of where the player wants to teleport to from the map in the menu room
/// </summary>
public class TransitionModule : MonoBehaviour
{
    static public bool shouldTeleport = false;
    static public Vector2 coords;
}
