using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to control the device and animal lists and the console output for the PC version
/// </summary>
public class UIListMasterController : MonoBehaviour
{
    [SerializeField]
    protected DeviceListController deviceList;

    [SerializeField]
    protected AnimalListController animalList;

    [SerializeField]
    protected ConsoleLogController consoleLog;

    [SerializeField]
    protected LEDListController ledList;

    protected Device currentDeviceFilter;

    protected Animal currentAnimalFilter;

    protected void Awake()
    {
        if (deviceList == null)
        {
            Debug.LogWarning("DeviceListController not instantiated, looking...");
            deviceList = GetComponentInChildren<DeviceListController>();
        }

        if (animalList == null)
        {
            Debug.LogWarning("AnimalListController not instantiated, looking...");
            animalList = GetComponentInChildren<AnimalListController>();
        }

        if (consoleLog == null)
        {
            Debug.LogWarning("ConsoleLogController not instantiated, looking...");
            consoleLog = GetComponentInChildren<ConsoleLogController>();
        }

        if (ledList == null)
        {
            Debug.LogWarning("LEDListController not instantiated, looking...");
            ledList = GetComponentInChildren<LEDListController>();
        }

        //Used to demo LED window, remove below when LEDs are retrieved from backend
        for (int i = 0; i < 30; i++)
        {
            string s = "LED #" + i;
            ledList.AddItem(s);
        }
    }

    protected void Start()
    {
        AddDeviceButtonhandlers();
        AddAnimalButtonHandlers();
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        CheckConsoleLEDWindow();

    }

    /// <summary>
    /// Whether the entire UI canvas is active
    /// </summary>
    /// <returns></returns>
    public bool IsActive()
    {
        return gameObject.activeSelf;
    }

    /// <summary>
    /// Whether the console and LED UIs are active
    /// </summary>
    /// <returns></returns>
    public bool IsConsoleLEDActive()
    {
        return consoleLog.IsActive() && ledList.IsActive();
    }

    /// <summary>
    /// Turn the entire UI on or off
    /// </summary>
    /// <param name="show">UI active state</param>
    public void ToggleUI(bool show)
    {
        gameObject.SetActive(show);

        if (show)
        {
            //FilterCurrentList();
            deviceList.ToggleList(true);
            animalList.ToggleList(true);
        }
    }

    /// <summary>
    /// Swap the current active state of the UI
    /// </summary>
    public void ToggleUI()
    {
        ToggleUI(!gameObject.activeSelf);
    }

    /// <summary>
    /// Add the FilterByDevices handler to the buttons in the device list
    /// Replace/remove when devices are retrieved from backend
    /// </summary>
    public void AddDeviceButtonhandlers()
    {
        deviceList.AddButtonHandler(FilterByDevices);
    }

    /// <summary>
    /// Add the FilterByAnimals handler to the buttons in the animal list
    /// </summary>
    public void AddAnimalButtonHandlers()
    {
        animalList.AddButtonHandler(FilterByAnimals);
    }

    /// <summary>
    /// Filter the animal list by a device
    /// If the animal list is already filtered by that device then unfilter it
    /// </summary>
    /// <param name="index">Index of entry in device list</param>
    public void FilterByDevices(int index)
    {
        Device d = deviceList.Entries[index];
        if (d == currentDeviceFilter)
        {
            animalList.ResetList();
            currentDeviceFilter = null;
        }
        else
        {
            animalList.FilterList(d.AttachedAnimals);
            currentDeviceFilter = d;
        }
    }

    /// <summary>
    /// Filter the device list by an animal
    /// If the device list is already filtered by that device then unfilter it
    /// </summary>
    /// <param name="index">Index of entry in animal list</param>
    public void FilterByAnimals(int index)
    {
        Animal a = animalList.Entries[index];
        if (a == currentAnimalFilter)
        {
            deviceList.ResetList();
            currentAnimalFilter = null;
        }
        else
        {
            deviceList.FilterList(a.AttachedDevices);
            currentAnimalFilter = a;
        }
    }

    /// <summary>
    /// Check if the Console log and LED list windows should be opened or closed and do so
    /// The windows should only be opened if the user selects one device and one animal from the lists
    /// </summary>
    private void CheckConsoleLEDWindow()
    {
        if (currentAnimalFilter != null && currentDeviceFilter != null)
        {
            consoleLog.ToggleList(true, currentDeviceFilter, currentAnimalFilter);
            ledList.ToggleList(true, currentDeviceFilter, currentAnimalFilter);
        }
        else
        {
            consoleLog.ToggleList(false, null, null);
            ledList.ToggleList(false, null, null);
        }
    }
}
