using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for interacting with the console window
/// </summary>
public class ConsoleLogController : ScrollListController
{
    /// <summary>
    /// Maximum amount of entries to store
    /// </summary>
    [SerializeField]
    private const int maxItems = 100;

    // The current device-animal pair to retrieve console logs for
    private Device currDevice;
    private Animal currAnimal;

    // ToggleList is called multiple times due to checking by UIListMasterController
    // This variable to used to run anything the first time the window is opened
    private bool firstActivate = true;

    private float prevTime;
    private float sensorUpdateInterval = 5f;

    new void Awake()
    {
        base.Awake();
        if (listCanvas == null)
        {
            Debug.LogWarning("ConsoleWindow not instantiated, looking...");
            listCanvas = transform.parent.gameObject;
        }

        if (itemTemplate == null)
        {
            Debug.LogWarning("ConsoleLog ItemTemplate not instantiated, looking...");
            itemTemplate = transform.Find("ConsoleLogItemTemplate").gameObject;
        }
        prevTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - prevTime > sensorUpdateInterval)
        {
            prevTime = Time.time;
            if (currDevice != null && currAnimal != null)
            {
                AddConsoleLogs(currDevice, currAnimal.ID);
                scrollBarPos = 1f;
            }
        }
    }

    /// <summary>
    /// Enable and disables the console window and sets the title of the console window
    /// </summary>
    /// <param name="show">Whether to enable the window</param>
    /// <param name="device">The device associated with this window</param>
    /// <param name="animal">The animal associated with this window</param>
    public void ToggleList(bool show, Device device, Animal animal)
    {
        listCanvas.SetActive(show);

        if (show && firstActivate)
        {
            currDevice = device;
            currAnimal = animal;

            GameObject title = listCanvas.transform.Find("ConsoleLogDeviceName").gameObject;
            title.GetComponent<Text>().text = device.Name + " - " + animal.Name;
            
            firstActivate = false;
        }
        else if (show)
        {
            // do nothing
        }
        else
        {
            ClearAll();
            firstActivate = true;
        }
    }

    /// <summary>
    /// Add a new log entry to the list and remove old ones if max is exceeded
    /// </summary>
    /// <param name="newLogString">Log text</param>
    public override void AddItem(string newLogString)
    {
        if (items.Count >= maxItems)
        {
            GameObject temp = items[0];
            Destroy(temp.gameObject);
            items.Remove(temp);
        }
        base.AddItem(newLogString);
    }

    /// <summary>
    /// Retrieve the sensor information from a device by animal ID
    /// </summary>
    /// <param name="device"></param>
    /// <param name="animalId"></param>
    public void AddConsoleLogs(Device device, int animalId)
    {
        if (device.AnimalToConsoleLogs.ContainsKey(animalId))
        {
            foreach (string log in device.AnimalToConsoleLogs[animalId])
            {
                AddItem(log);
            }
        }
    }

    /// <summary>
    /// Remove all console log gameobjects and clear the internal list
    /// </summary>
    public void ClearAll()
    {
        foreach(GameObject item in items)
        {
            Destroy(item.gameObject);
        }
        items.Clear();
    }
}
