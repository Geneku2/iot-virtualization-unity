using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeviceButton : MonoBehaviour
{
    // Start is called before the first frame update

    public Device Device {get; set;}

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

    void Update()
    {

    }

    public void ForwardToMasterControl()
    {
        UIMasterControl.GetComponent<UIMasterControl>().SelectedDevice = Device;
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
