using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for navigating the device dropdown list and selecting a device to attach to an animal
/// </summary>
public class PCDeviceAttachController : MonoBehaviour
{
    /// <summary>
    /// The gameobject of the scroll list itself
    /// </summary>
    [SerializeField]
    private GameObject dropdownScrollList;

    /// <summary>
    /// The controller for the device ScrollList
    /// </summary>
    private PCDeviceAttachListController listController;

    /// <summary>
    /// The text field that displays the currently selected device
    /// </summary>
    [SerializeField]
    private Text currentDeviceText;

    /// <summary>
    /// The image component of the dropdown arrow
    /// </summary>
    [SerializeField]
    private Image dropdownImage;

    /// <summary>
    /// The image component of the selected device object
    /// </summary>
    [SerializeField]
    private Image devicePanelImage;

    /// <summary>
    /// Keep track of the original color for both testing if it is active and restoring the original color
    /// </summary>
    private Color originalDevicePanelColor;
    private Color originalDropDownColor;

    /// <summary>
    /// Whether a device should be attached to the animal
    /// </summary>
    private bool shouldAttach = false;

    void Awake()
    {
        if (dropdownScrollList == null)
        {
            dropdownScrollList = transform.parent.Find("DeviceAttachListUI").gameObject;
        }

        listController = dropdownScrollList.GetComponentInChildren<PCDeviceAttachListController>();

        if (currentDeviceText == null)
        {
            currentDeviceText = transform.Find("DeviceText").GetComponent<Text>();
        }

        if (dropdownImage == null)
        {
            dropdownImage = transform.Find("Dropdown").GetComponent<Image>();
        }

        if (devicePanelImage == null)
        {
            devicePanelImage = transform.Find("DevicePanel").GetComponent<Image>();
        }

        originalDevicePanelColor = devicePanelImage.color;
        originalDropDownColor = dropdownImage.color;
    }

    /// <summary>
    /// Prep for attachment
    /// </summary>
    public void SelectCurrentDevice()
    {           
        devicePanelImage.color = Color.green;
        shouldAttach = true;
    }

    /// <summary>
    /// Turn off attachment
    /// </summary>
    public void UnselectCurrentDevice()
    {

        devicePanelImage.color = originalDevicePanelColor;
        shouldAttach = false;
    }

    /// <summary>
    /// Attach the currently selected device to the current animal gameobject and to its DeviceCanvas
    /// Then add the animal to the device's list of attached animals
    /// Finally turn off attachment
    /// </summary>
    /// <param name="animal"></param>
    public void AttachDevice(GameObject animal)
    {
        if (!shouldAttach)
        {
            Debug.Log("No device selected for attachment");
            return;
        }

        Animal a = animal.GetComponent<Animal>();
        AnimalDeviceCanvasController canvasController = animal.GetComponentInChildren<AnimalDeviceCanvasController>();

        Device d = listController.GetCurrentEntry();

        a.AttachDevice(d);
        ///canvasController.AttachDevice(d);
        ///
        //TODO: ADD Attach device script
        ToggleDevicePanel();
        Debug.Log(d.ID + " attached to:" + animal.name);
    }

    /// <summary>
    /// Turn the dropdown on/off
    /// </summary>
    public void ToggleDropDown()
    {
        if (dropdownImage.color == originalDropDownColor)
        {
            ToggleDropDown(true);
        }
        else
        {
            ToggleDropDown(false);
        }
    }

    public void ToggleDropDown(bool show)
    {
        if (show)
        {
            dropdownImage.color = Color.grey;
            dropdownScrollList.SetActive(true);
        } 
        else
        {
            dropdownImage.color = originalDropDownColor;
            dropdownScrollList.SetActive(false);
        }
    }

    /// <summary>
    /// Select/unselect device for attachment
    /// </summary>
    public void ToggleDevicePanel()
    {
        // When selected, clicks become attempts to first attach a device in the world
        if (devicePanelImage.color == originalDevicePanelColor)
        {
             SelectCurrentDevice();
        }
        else
        {
            UnselectCurrentDevice();
        }
    }

    /// <summary>
    /// Used when user clicks on a new device for attachment and selects device for attachment as well
    /// </summary>
    /// <param name="deviceName"></param>
    public void DeviceSelected(Text deviceName)
    {
        currentDeviceText.text = deviceName.text;
        ToggleDropDown();
        SelectCurrentDevice();
    }
}