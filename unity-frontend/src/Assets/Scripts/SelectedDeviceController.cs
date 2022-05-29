using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps track of what device the user selected for attaching to animals
/// </summary>
public class SelectedDeviceController : MonoBehaviour
{
    /// <summary>
    /// The selected device
    /// </summary>
    public Device Device { get; private set; }

    /// <summary>
    /// The text field that notifies the user of what device they have currently selected
    /// </summary>
    private Text deviceDisplay;

    private void Start()
    {
        deviceDisplay = GetComponentInChildren<Text>();
    }

    public void SetDevice(Device d)
    {
        Device = d;
        deviceDisplay.text = d.Name;
    }

}
