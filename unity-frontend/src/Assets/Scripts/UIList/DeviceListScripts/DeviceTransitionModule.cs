using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporarily holds the retrieved devices in between scene transitions
/// </summary>
public class DeviceTransitionModule : MonoBehaviour
{
    public static List<Circuit> circuits;

    // Key: circuit ID
    // Value: List of sensors in the circuit by their type
    public static Dictionary<string, List<int>> circuitToSensorTypes;
    public static Dictionary<string, List<string>> circuitToSensorIDs;
}
