using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main controller for opening the list and interacting with the devices in the list. 
/// Should be attached to the device list gameobject
/// </summary>
public class DeviceListController : ScrollListButtonController<Device>
{
    /// <summary>
    /// Used in VR only to display what device the user currently has selected
    /// </summary>
    [SerializeField]
    private SelectedDeviceController selectedDeviceController;

    public static Dictionary<string, Device> idToDevice;

    new void Awake()
    {
        base.Awake();

        if (listCanvas == null)
        {
            Debug.LogWarning("DeviceListController: DeviceListCanvas not instantiated, looking...");
            listCanvas = transform.parent.gameObject;
        }

        if (itemTemplate == null)
        {
            Debug.LogWarning("DeviceListController: DeviceList ItemTemplate not instantiated, looking...");
            itemTemplate = transform.Find("DeviceEntryTemplate").gameObject;
        }

        if (selectedDeviceController == null)
        {
            Debug.LogWarning("DeviceListController: SelectedDeviceController not instantiated, looking...");
            GameObject selectedDeviceCanvas = GameObject.Find("SelectedDeviceCanvas");
            if (selectedDeviceCanvas)
            {
                selectedDeviceController = selectedDeviceCanvas.GetComponent<SelectedDeviceController>();
            }
            else
            {
                Debug.LogWarning("DeviceListController: SelectedDeviceCanvas not found");
            }
        }

        idToDevice = new Dictionary<string, Device>();

        if (DeviceTransitionModule.circuits == null)
        {
            InitSampleDevices(10);
        }
        InitDeviceList();
    }

    /// <summary>
    /// Retrieves the circuits from the DeviceTransitionModule 
    /// Initialize the entries list and populate the UI list
    /// Add a button handler to set the current index to the button that was pressed
    /// </summary>
    public void InitDeviceList()
    {
        InitDeviceList(ParseCircuitInformation(DeviceTransitionModule.circuits));
    }

    /// <summary>
    /// Initialize the entires list and populate the UI list
    /// Add a button handler to set the current index to the button that was pressed
    /// </summary>
    /// <param name="devices"></param>
    public void InitDeviceList(List<Device> devices)
    {
        entries = devices;
        foreach (Device device in entries)
        {
            AddItem(device.Name);
        }
        AddButtonHandler(SetCurrentIndex);
    }

    /// <summary>
    /// Given an array of circuits, parse it into an array of devices
    /// </summary>
    /// <param name="circuits"></param>
    public List<Device> ParseCircuitInformation(List<Circuit> circuits)
    {
        List<Device> devices = new List<Device>();
        foreach (Circuit c in circuits)
        {
            Device newDevice = new Device(c.circuitName, c.circuitId, DeviceTransitionModule.circuitToSensorTypes[c.circuitId]);
            idToDevice.Add(c.circuitId, newDevice);
            devices.Add(newDevice);
        }
        return devices;
    }

    /// <summary>
    /// Set the currently selected device as the device for attachment
    /// </summary>
    public void SelectPrimaryDevice()
    {
        selectedDeviceController.SetDevice(GetCurrentEntry());
    }

    private void InitSampleDevices(int n)
    {

        Debug.Log("IDK WHAT IS GOING ON");
        List<Circuit> circuits = new List<Circuit>();
        Dictionary<string, List<int>> circuitToSensorTypes = new Dictionary<string, List<int>>();
        Dictionary<string, List<string>> circuitToSensorIDs = new Dictionary<string, List<string>>();
        for (int i = 0; i < n; i++)
        {
            Circuit c = new Circuit { circuitId = i.ToString(), circuitName = "Device " + i };
            circuits.Add(c);
            circuitToSensorIDs.Add(i.ToString(), new List<string> { "id1", "id2", "id3", "id4", "id5" });
            circuitToSensorTypes.Add(i.ToString(), new List<int> { 3, 4, 9, 24, 25 });
        }
        DeviceTransitionModule.circuits = circuits;
        DeviceTransitionModule.circuitToSensorIDs = circuitToSensorIDs;
        DeviceTransitionModule.circuitToSensorTypes = circuitToSensorTypes;
    }
}
