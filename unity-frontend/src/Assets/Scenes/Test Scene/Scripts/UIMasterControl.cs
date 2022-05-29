using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMasterControl : MonoBehaviour
{
    public GameObject ADCanvas;

    public GameObject AnimalButtonTemplate;
    public GameObject DeviceButtonTemplate;
    public GameObject AttachedDeviceButtonTemplate;
    public GameObject ConsoleOutputListGameobject;
    public GameObject SelectedAnimalName;
    public GameObject SelectedDeviceName;

    //AD Cabvas
    private GameObject ADAnimalList;
    public GameObject ADAnimalListHolder;
    private GameObject ADDeviceList;
    public GameObject ADDeviceListHolder;
    public GameObject ADAttachedDeviceListHolder;
    private Animator ADCanvasAnimator;
    private Animator ADListAnimator;
    private HorizontalSelector ADHorizontalSelector;
    public static bool ADListEnabled = false;
    public static bool ADListSetAside = false;

    private GameObject deviceAttachIcon;
    private Animator DAAnimator;
    private bool deviceAttachAnimalSelected;
    private bool attachingDevice;
    private bool onIconExit;

    private bool showingConsoleOutput;

    [SerializeField]
    private AnimalController animalController;

    public Device SelectedDevice { get; set; }
    public Animal SelectedAnimal { get; set; }


    private List<string> animalWithAttachedDevices = new List<string>();

    //TEST
    private MockList mockoutput;

    // Start is called before the first frame update
    void Start()
    {
        if (ADCanvas == null)
            Debug.LogError("AD Canvas not assigned.");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (attachingDevice)
            return;

        if (deviceAttachIcon.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Tab)){
                HideDeviceAttachIcon();
                return;
            }
            deviceAttachIcon.transform.position = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag.Equals("Animal") && !onIconExit)
                {
                    if (!deviceAttachAnimalSelected)
                    {
                        DAAnimator.SetTrigger("DAOnHover");
                        deviceAttachAnimalSelected = true;
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        AttachDevice(hit.collider.gameObject);

                        attachingDevice = true;
                        return;
                    }

                } else
                {
                    if (deviceAttachAnimalSelected)
                    {
                        DAAnimator.SetTrigger("DAOnIdle");
                        deviceAttachAnimalSelected = false;
                    }
                }
            } else
            {
                if (deviceAttachAnimalSelected)
                {
                    DAAnimator.SetTrigger("DAOnIdle");
                    deviceAttachAnimalSelected = false;
                }

            }

        }

        if (showingConsoleOutput)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CloseDeviceConsoleOutput();
                return;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab) && ADListSetAside)
        {
            ResetADList();

            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !ADListSetAside)
        {
            ADListEnabled = !ADListEnabled;

            if (ADListEnabled)
                ShowADList();
            else
                DisableADList();
        }

        
    }

    //PROD
    public void InitializeExternalVariables()
    {
        if (animalController == null)
        {
            Debug.LogError("ANIMAL CONTROLLER NOT FOUND");
        }
    }

    public void InitializeADCanvas()
    {

        if (GameObject.Find("Animal List") == null ||
            GameObject.Find("Device List") == null ||
            GameObject.Find("Animal_Device List") == null ||
            GameObject.Find("AD Horizontal Selector") == null ||
            ADCanvas.GetComponent<Animator>() == null)
            Debug.LogError("Animal List, Device List, AD Animators, or other requirements are not present.");
        else
        {
            ADAnimalList = GameObject.Find("Animal List");
            ADDeviceList = GameObject.Find("Device List");
            ADListAnimator = GameObject.Find("Animal_Device List").GetComponent<Animator>();
            ADCanvasAnimator = ADCanvas.GetComponent<Animator>();
            ADHorizontalSelector = GameObject.Find("AD Horizontal Selector").GetComponent<HorizontalSelector>();
        }

        if(GameObject.Find("Device Attach Icon") == null){
            Debug.LogError("Device Attach Icon not present");
        } else
        {
            deviceAttachIcon = GameObject.Find("Device Attach Icon");
            DAAnimator = deviceAttachIcon.GetComponent<Animator>();
            deviceAttachIcon.SetActive(false);
            deviceAttachAnimalSelected = false;
        }

        ADAnimalList.GetComponent<CanvasGroup>().alpha = 0;
        ADDeviceList.GetComponent<CanvasGroup>().alpha = 0;

        attachingDevice = false;
        onIconExit = false;
    }

    //PROD
    public void InitializeAnimalsAndDevices()
    {
        PopulateDeviceList(ParseCircuitInformation(DeviceTransitionModule.circuits));
        PopulateAnimalList(animalController.Animals);
    }

    public void InitializeAnimalsAndDevicesTEST()
    {
        PopulateDeviceList(mockoutput.devices);
        PopulateAnimalList(mockoutput.animals);
    }

    public List<Device> ParseCircuitInformation(List<Circuit> circuits)
    {
        List<Device> devices = new List<Device>();
        foreach (Circuit c in circuits)
        {
            Device newDevice = new Device(c.circuitName, c.circuitId, DeviceTransitionModule.circuitToSensorTypes[c.circuitId]);
            devices.Add(newDevice);
        }
        return devices;
    }

    public void ShowADList()
    {
        ADCanvasAnimator.Play("AD_Canvas_Fade_In");

        showList();
    }

    public void DisableADList()
    {
        ADCanvasAnimator.Play("AD_Canvas_Fade_Out");
    }


    //AD Canvas
    public void SwitchList()
    {
        if (ADListSetAside)
            return;

        //Index == 0 is device, Index == 1 is animal
        if (ADHorizontalSelector.index == 0)
        {
            ADListAnimator.Play("AD_Switch");
        } 
        else if (ADHorizontalSelector.index == 1)
        {
            ADListAnimator.Play("DA_Switch");
        }
    }

    public bool IsShowingConsoleOutput()
    {
        return showingConsoleOutput;
    }

    public void ShowDeviceConsoleOutput()
    {
        ADCanvasAnimator.Play("ATTACHED_DEVICE_PANEL_FADE_OUT");
        ADCanvasAnimator.Play("CONSOLE_OUTPUT_WINDOW_FADE_IN");
        
        showingConsoleOutput = true;
    }

    public void CloseDeviceConsoleOutput()
    {
        ADCanvasAnimator.Play("CONSOLE_OUTPUT_WINDOW_FADE_OUT");
        ADCanvasAnimator.Play("ATTACHED_DEVICE_PANEL_FADE_IN");

        showingConsoleOutput = false;
    }

    public void AddConsoleOutput(string s)
    {
        ConsoleOutputListGameobject.GetComponent<ConsoleOutputList>().UpdateConsoleLog(s);
    }

    private void showList()
    { 
        //Index == 0 is device, Index == 1 is animal
        if (ADHorizontalSelector.index == 0)
        {
            ADListAnimator.Play("D_FADE_IN");
        }
        else if (ADHorizontalSelector.index == 1)
        {
            ADListAnimator.Play("A_FADE_IN");
        }
    }

    public void SetAsideADList()
    {
        if (ADListSetAside)
        {
            ADCanvasAnimator.Play("ATTACHED_DEVICE_PANEL_FADE_IN");
            return;
        }


        ADCanvasAnimator.Play("AD_ASIDE");
        ADCanvasAnimator.Play("ATTACHED_DEVICE_PANEL_FADE_IN");
        ADListSetAside = true;
    }

    public void ResetADList()
    {
        ADCanvasAnimator.Play("AD_RECOVER");
        ADCanvasAnimator.Play("ATTACHED_DEVICE_PANEL_FADE_OUT");
        ADListSetAside = false;
    }

    public void ShowDeviceAttachIcon()
    {
        deviceAttachIcon.SetActive(true);
    }

    public void HideDeviceAttachIcon()
    {
        DAAnimator.SetTrigger("DAOnIdle");

        onIconExit = true;

        Invoke("ResetDAIcon", 1);
    }

    public void ResetDAIcon()
    {
        deviceAttachAnimalSelected = false;

        if (attachingDevice)
            attachingDevice = false;

        onIconExit = false;

        deviceAttachIcon.SetActive(false);
    }

    public void AttachDevice(GameObject animal)
    {
        Animal a = animal.GetComponent<Animal>();
        a.AttachDevice(SelectedDevice);

        if (animalWithAttachedDevices.Count == 0)
        {
            addAnimalButton(a, ADAnimalListHolder);
            animalWithAttachedDevices.Add(a.name);
        } else
        {
            bool found = false;
            foreach (string s in animalWithAttachedDevices)
            {
                if (s.Equals(a.name))
                {
                    found = true;
                }
            }

            if (!found)
            {
                addAnimalButton(a, ADAnimalListHolder);
                animalWithAttachedDevices.Add(a.name);
            }
        }

        DAAnimator.SetTrigger("DAOnAttach");
        Invoke("HideDeviceAttachIcon", 2);
    }

    public void AttachDevice(Animal animal, Device device)
    {
        animal.AttachDevice(device);
    }

    public void ClearDevices(Animal animal)
    {
        animal.ClearDevices();
        foreach (AnimalButton animalButton in ADAnimalListHolder.GetComponentsInChildren<AnimalButton>())
        {
            if (animalButton.Animal == animal)
            {
                Destroy(animalButton.gameObject);
            }
        }
    }

    #region CORE UI FUNCTIONS

    //PROD
    public void PopulateAnimalList(List<Animal> Animals)
    {
        foreach(Transform animal in ADAnimalListHolder.transform)
        {
            Destroy(animal.gameObject);
        }

        //foreach(Animal a in Animals)
        //{
        //    addAnimalButton(a, ADAnimalListHolder);
        //}
    }

    public void SetSelectedAnimalName(string s)
    {
        SelectedAnimalName.GetComponent<TextMeshProUGUI>().text = s;
    }

    public void SetSelectedDeviceName(string s)
    {
        SelectedDeviceName.GetComponent<ButtonManager>().buttonText = s;
    }

    private void addAnimalButton(Animal a, GameObject parent)
    {
        GameObject newOutput = Instantiate(AnimalButtonTemplate, parent.transform);
        newOutput.GetComponent<AnimalButton>().SetAnimalName(a);
        newOutput.SetActive(true);
    }

    public void PopulateDeviceList(List<Device> Devices)
    {
        foreach (Transform animal in ADDeviceListHolder.transform)
        {
            Destroy(animal.gameObject);
        }

        foreach (Device d in Devices)
        {
            addDeviceButton(d, ADDeviceListHolder);
        }
    }

    public void PopulateAttachedDeviceList()
    {
        foreach (Transform device in ADAttachedDeviceListHolder.transform)
        {
            Destroy(device.gameObject);
        }

        List<Device> attachedDevices = SelectedAnimal.AttachedDevices;
        foreach (Device d in attachedDevices)
        {
            addAttachedDeviceButtom(d, ADAttachedDeviceListHolder);
        }
    }

    private void addAttachedDeviceButtom(Device d, GameObject parent)
    {
        GameObject newOutput = Instantiate(AttachedDeviceButtonTemplate, parent.transform);
        newOutput.GetComponent<AttachedDeviceButton>().SetDeviceName(d);
        newOutput.SetActive(true);
    }

    private void addDeviceButton(Device d, GameObject parent)
    {
        GameObject newOutput = Instantiate(DeviceButtonTemplate, parent.transform);
        newOutput.GetComponent<DeviceButton>().SetDeviceName(d);
        newOutput.SetActive(true);
    }

    #endregion
}
