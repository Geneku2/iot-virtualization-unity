using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttachedDeviceButton : MonoBehaviour
{
    public Device Device { get; set; }

    public GameObject DeviceNameTextNormal;
    public GameObject DeviceNameTextPressed;

    private Button button;

    private GameObject UIMasterControl;

    void Start()
    {
        UIMasterControl = GameObject.Find("UI Master Control");

        button = GetComponent<Button>();
        button.onClick.AddListener(ForwardToMasterControl);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ForwardToMasterControl()
    {
        UIMasterControl.GetComponent<UIMasterControl>().SelectedDevice = Device;
        UIMasterControl.GetComponent<UIMasterControl>().SetSelectedDeviceName(Device.Name);
        UIMasterControl.GetComponent<UIMasterControl>().ConsoleOutputListGameobject.
            GetComponent<ConsoleOutputList>().
            InitConsoleOutputList(UIMasterControl.GetComponent<UIMasterControl>().SelectedDevice,
                                   UIMasterControl.GetComponent<UIMasterControl>().SelectedAnimal);
    }

    public void SetDeviceName(Device d)
    {
        Device = d;

        if (gameObject.GetComponent<ButtonManager>())
        {
            gameObject.GetComponent<ButtonManager>().buttonText = d.Name;
        }

        if (gameObject.GetComponent<TextMeshProUGUI>())
        {
            gameObject.GetComponent<TextMeshProUGUI>().text = d.Name;
        }

        if (DeviceNameTextNormal != null)
        {
            DeviceNameTextNormal.GetComponent<TextMeshProUGUI>().text = d.Name;
        }

        if (DeviceNameTextPressed != null)
        {

            DeviceNameTextPressed.GetComponent<TextMeshProUGUI>().text = d.Name;
        }
    }
}
