using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main controller for interacting with the LED window
/// </summary>
public class LEDListController : ScrollListController
{
    // The current device-animal pair to retrieve console logs for
    private Device currDevice;
    private Animal currAnimal;

    new void Awake()
    {
        base.Awake();
        if (listCanvas == null)
        {
            Debug.LogWarning("LEDListCanvas not instantiated, looking...");
            listCanvas = transform.parent.gameObject;
        }

        if (itemTemplate == null)
        {
            Debug.LogWarning("LEDList ItemTemplate not instantiated, looking...");
            itemTemplate = transform.Find("LEDItemTemplate").gameObject;
        }
    }

    /// <summary>
    /// Open/close the list
    /// Sets the current device-animal pair
    /// </summary>
    /// <param name="show"></param>
    /// <param name="d"></param>
    /// <param name="a"></param>
    public void ToggleList(bool show, Device d, Animal a)
    {
        base.ToggleList(show);
        currDevice = d;
        currAnimal = a;
    }
}
