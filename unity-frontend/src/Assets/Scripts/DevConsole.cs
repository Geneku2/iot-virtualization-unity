using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DevConsole : MonoBehaviour
{
    public static DevConsole instance;

    [SerializeField] private UIMasterControl masterControl;
    [SerializeField] private InputField console;
    [SerializeField] private TextMeshProUGUI displayMessage;
    [SerializeField] private TextMeshProUGUI sensorData;
    [SerializeField] private TextMeshProUGUI simResults;
    private const KeyCode toggleKey = KeyCode.BackQuote;
    private const KeyCode activationKey = KeyCode.Return;

    private GameObject ceiling;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        console.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        instance = this;

        if (!console.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(toggleKey))
            {
                console.gameObject.SetActive(true);
                Focus();
            }
        }
        else
        {
            if (Input.GetKeyDown(toggleKey)){
                console.gameObject.SetActive(false);
            }
            else if (Input.GetKeyDown(activationKey))
            {
                string[] args = System.Text.RegularExpressions.Regex.Split(console.text, @"\s+");
                console.text = "";
                switch (args[0])
                {
                    case "attach":
                    case "a":
                        if (args.Length != 3)
                        {
                            displayMessage.text = "[Device ID] [Animal ID]";
                            break;
                        }
                        int deviceIndex = int.Parse(args[1]);
                        int animalIndex = int.Parse(args[2]);

                        if (deviceIndex < DeviceTransitionModule.circuits.Count && animalIndex < AnimalController.idToAnimal.Count)
                        {
                            Device d = DeviceListController.idToDevice[DeviceTransitionModule.circuits[deviceIndex].circuitId];
                            masterControl.AttachDevice(AnimalController.idToAnimal[animalIndex],d);
                            displayMessage.text = $"Command: Attached Device {deviceIndex} to Animal {animalIndex}";
                        }
                        else
                        {
                            displayMessage.text = "Out of Bound";
                        }
                        break;

                    case "delete":
                    case "d":
                        if (args.Length != 2)
                        {
                            displayMessage.text = "[Animal ID]";
                            break;
                        }
                        animalIndex = int.Parse(args[1]);

                        if (animalIndex < AnimalController.idToAnimal.Count)
                        {
                            masterControl.ClearDevices(AnimalController.idToAnimal[animalIndex]);
                            displayMessage.text = $"Command: Deleted all devices from Animal {animalIndex}";
                        }
                        else
                        {
                            displayMessage.text = "Out of Bound";
                        }
                        break;

                    case "ceiling":
                    case "c":
                        if (args.Length != 1)
                        {
                            displayMessage.text = "0 input required.";
                            break;
                        }
                        if (ceiling == null) {
                            ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
                            ceiling.name = "Ceiling";
                            Destroy(ceiling.GetComponent<MeshRenderer>());
                            ceiling.transform.localScale = new Vector3(100, 1, 100);
                            ceiling.transform.position = new Vector3(500, 250, 500);
                        }
                        // TODO: hold a reference to player instead of doing a search every frame
                        GameObject.FindGameObjectWithTag("Player").transform.position += (ceiling.transform.position.y + 10) * Vector3.up;
                        break;

                    case "ground":
                    case "g":
                        if (ceiling != null)
                        {
                            Destroy(ceiling);
                        }
                        break;

                    default:
                        displayMessage.text = "Command not found";
                        break;

                }
                Focus();
            }
        }

    }

    public static bool Active()
    {
        return instance.console.gameObject.activeSelf;
    }

    public static void UpdateData(string data)
    {
        instance.sensorData.text = JsonBeautify(data);
    }

    public static void UpdateResult(string result)
    {
        instance.simResults.text = JsonBeautify(result);
    }


    private void Focus()
    {
        EventSystem.current.SetSelectedGameObject(console.gameObject, null);
        console.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private static string JsonBeautify(string json)
    {
        int indent = 2;
        int level = 0;

        int i = 0;
        int n = json.Length;
        while (i < n)
        {
            switch (json[i])
            {
                case '{':
                case '[':
                    i++;
                    level++;
                    SwitchRow();
                    break;
                case '}':
                case ']':
                    level--;
                    SwitchRow();
                    i++;
                    break;
                case ',':
                    i++;
                    SwitchRow();
                    break;
                default:
                    i++;
                    break;
            }
        }

        return json;

        void SwitchRow()
        {
            json = json.Insert(i, '\n' + new String(' ', indent * level));
            i += indent * level + 1;
            n += indent * level + 1;
        }
    }
}
