using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the device attach list for the PC version
/// </summary>
public class PCDeviceAttachListController : DeviceListController 
{
    /// <summary>
    /// Used to sync the two device lists. If this is not necessary, then this can be removed
    /// and then just call InitDeviceList() below
    /// </summary>
    [SerializeField]
    private DeviceListController deviceListController;

    protected new void Awake()
    {
        base.Awake();

        if (deviceListController == null)
        {
            Debug.LogWarning("DeviceListController not instantiated, looking...");
            deviceListController = GameObject.Find("DeviceList").GetComponent<DeviceListController>();
        }

        if (listCanvas == null)
        {
            Debug.LogWarning("DeviceListCanvas not instantiated, looking...");
            listCanvas = transform.parent.gameObject;
        }

        if (itemTemplate == null)
        {
            Debug.LogWarning("DeviceAttachListEntryTemplate ItemTemplate not instantiated, looking...");
            itemTemplate = transform.Find("Device Attach List Entry Template").gameObject;
        }
    }

    protected new void Start()
    {
        base.Start();

        InitDeviceList(deviceListController.Entries);
    } 
 }