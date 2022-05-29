using System.Collections.Generic;
using UnityEngine;

public class MockList : MonoBehaviour
{
    public List<Animal> animals;

    public List<Device> devices;

    public UIMasterControl UIMasterControl;

    void Start()
    {
        devices = new List<Device>();

        for (int i = 0; i < animals.Count; i++)
        {
            animals[i].InitAnimal(i, animals[i].Type);
            for (int j = 0; j <= i; j++)
            {
                Device d = new Device("Device " + j.ToString(), i.ToString(), new List<int>());
                animals[i].AttachDevice(d);
            }

        }

        int deviceNum = animals.Count;

        for (int i = 0; i < deviceNum; i++)
        {
            Device d = new Device("Device " + i.ToString(), i.ToString(), new List<int>());

            devices.Add(d);
        }

        UIMasterControl = GameObject.Find("UI Master Control").GetComponent<UIMasterControl>();
        UIMasterControl.InitializeADCanvas();
        UIMasterControl.InitializeAnimalsAndDevicesTEST();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string getLogForDevice(Device d, Animal a)
    {
        foreach(Device device in devices)
        {
            if (device.ID == d.ID)
            {
                return $"Device Name: {d.Name} \nDevice ID: {d.ID} \nOn Animal: {a.Name} \nat {Time.time}";
            }
        }

        return "Device not found";
    }
}
