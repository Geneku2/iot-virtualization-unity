using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using System.Net;
using System.Linq;

/// <summary>
/// Represents the animal and controls its behavior
/// </summary>
public abstract class Animal : MonoBehaviour, ILaserTarget, ILaserColor, IEquatable<Animal>
{
    // Override if needed
    Color ILaserColor.C => Color.green;

    const string ULTRASONIC_MAX = "4000.0";
    bool IEquatable<Animal>.Equals(Animal other)
    {
        return ID == other.ID;
    }

    public int ID { get; private set; }
    public string Name { get; private set; }

    public AnimalType Type { get; set; }

    public GameObject eyeLevel { get; set; }

    public AnimalState State { get; set; }

    public List<Device> AttachedDevices { get; private set; }

    public List<int> AttachedSensors;

    public Dictionary<int, string> SensorData;

    public abstract void Move(float distance, float time);

    public abstract void Eat();

    public abstract void Death();

    private float lastUpdateTime = 0f;

    private VitalSensorDemo demosensor;

    public static Dictionary<Device.Sensor, GameObject> TestEnvironmemtSensorData { get; set; }

    /// <summary>
    /// Required to be called if new animal is instantiated
    /// </summary>
    /// <param name="info"></param>
    public void InitAnimal(int id, AnimalType type)
    {
        ID = id;
        Type = type;
        Name = type.ToString() + "-" + id.ToString();

        eyeLevel = transform.Find("Eye Level").gameObject;
        AttachedDevices = new List<Device>();
        AttachedSensors = new List<int>();
        SensorData = new Dictionary<int, string>();

        if (TestEnvironmemtSensorData == null)
        {
            TestEnvironmemtSensorData = new Dictionary<Device.Sensor, GameObject>();
            TestEnvironmemtSensorData.Add(Device.Sensor.temperature, GameObject.Find("temperatureData"));
            TestEnvironmemtSensorData.Add(Device.Sensor.humidity, GameObject.Find("humidityData"));
            TestEnvironmemtSensorData.Add(Device.Sensor.airQuality, GameObject.Find("airQualityData"));
            
        }

        demosensor = GetComponent<VitalSensorDemo>();

        //Debug.Log(String.Format("New animal instantiated: ID: {0}, Name: {1}, Type: {2}, Initial State: {3}", ID, Name, Type, State));
    }

    /// <summary>
    /// Attach device to animal and update the device's attached animals list
    /// </summary>
    /// <param name="target">not used</param>
    /// <param name="origin">The origin of the raycast, assumed to be the player</param>
    void ILaserTarget.OnLaserHit(RaycastHit target, GameObject origin)
    {
        if (origin != null)
        {
            SelectedDeviceController controller = origin.GetComponentInChildren<SelectedDeviceController>();
            if (controller != null)
            {
                AttachDevice(controller.Device);
            }
            else
            {
                Debug.LogWarning("No SelectedDeviceController component in GameObject: " + origin);
            }
        }
        else
        {
            Debug.LogWarning("No GameObject passed in");
        }
    }

    /// <summary>
    /// Add a device to the list of attached devices
    /// </summary>
    /// <param name="d"></param>
    public void AttachDevice(Device d)
    {
        if (d == null)
        {
            Debug.LogWarning("Device is null");
            return;
        }

        foreach (int s_id in d.SensorTypes)
        {
            if (!AttachedSensors.Contains(s_id))
            {
                AttachedSensors.Add(s_id);
                SensorData.Add(s_id, "\"NaN\"");
            }
        }
        GetComponent<SensorUI>().generateSensorUI();
        AttachedDevices.Add(d);
        d.AttachAnimalToDevice(this);
        // Call add_device for socketio
        if (Client.simClientEnabled)
            DeviceSimulationClient.one.SocketIOAddDevice(d.ID, ID);
    }

    public void ClearDevices()
    {
        foreach (Device d in AttachedDevices)
        {
            d.DetachAnimalFromDevice(this);
            DeviceSimulationClient.one.SocketIORemoveDevice(d.ID, ID);
        }
        AttachedSensors = new List<int>();
        SensorData = new Dictionary<int, string>();
        AttachedDevices = new List<Device>();

    }

    void FixedUpdate()
    {
        //float terrain_height = Terrain.activeTerrain.SampleHeight(transform.position);
        //if (transform.position.y - terrain_height > 10f)
        //{
        //    Vector3 new_position = transform.position;
        //    new_position.y = terrain_height;
        //    transform.position = new_position;
        //}
        //Debug.Log(gameObject);


        if (Time.time - lastUpdateTime > Client.DataUpdateInterval) {
            lastUpdateTime = Time.time;

            // Simulation
            foreach (int s_id in AttachedSensors)
            {
                Device.Sensor s = Device.SensorFromID(s_id);
                switch (s)
                {
                    case Device.Sensor.gps:
                        SensorData[s_id] = string.Format("[{0}, {1}]", transform.position.x, transform.position.z);
                        break;
                    case Device.Sensor.ultrasonic:
                        RaycastHit hit;
                        Transform eyelevel = eyeLevel.transform;
                        if (Physics.Raycast(eyelevel.transform.position, eyelevel.transform.TransformDirection(Vector3.forward), out hit, Device.ULTRASONIC_MAX))
                        {
                            //Debug.Log("Hit: " + hit.collider.name + ", Dist:" + hit.distance);
                            Debug.DrawRay(eyelevel.transform.position, eyelevel.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                            SensorData[s_id] = hit.distance.ToString();
                        }
                        else
                        {
                            SensorData[s_id] = ULTRASONIC_MAX;
                        }
                        break;
                    case Device.Sensor.airQuality:
                    case Device.Sensor.temperature:
                    case Device.Sensor.humidity:
                        GameObject sensorDataObject;
                        TestEnvironmemtSensorData.TryGetValue(s, out sensorDataObject);
                        if (sensorDataObject == null)
                        {
                            Debug.LogWarning(s + " data missing.");
                            break;
                        }
                        float squareDistSum = 0f;
                        float sensorData = 0f;
                        foreach (EnvironmentData dp in sensorDataObject.transform.GetComponentsInChildren<EnvironmentData>())
                        {
                            squareDistSum += dp.SquareDist(transform.position);
                        }
                        foreach (EnvironmentData dp in sensorDataObject.transform.GetComponentsInChildren<EnvironmentData>())
                        {
                            sensorData += dp.data * dp.SquareDist(transform.position) / squareDistSum;
                        }
                        SensorData[s_id] = sensorData.ToString();
                        //Debug.Log("Sensor " + s.ToString() + ": " + sensorData + "\n");
                        break;
                    case Device.Sensor.pulseOxi:
                        //duplicate names here aren't really useful
                        GameObject sensorDataObject_1;
                        TestEnvironmemtSensorData.TryGetValue(Device.Sensor.temperature, out sensorDataObject_1);
                        if (sensorDataObject_1 == null)
                        {
                            Debug.LogWarning(Device.Sensor.temperature + " data missing.");
                            break;
                        }
                        float squareDistSum_1 = 0f;
                        float sensorData_1 = 0f;
                        foreach (EnvironmentData dp in sensorDataObject_1.transform.GetComponentsInChildren<EnvironmentData>())
                        {
                            squareDistSum_1 += dp.SquareDist(transform.position);
                        }
                        foreach (EnvironmentData dp in sensorDataObject_1.transform.GetComponentsInChildren<EnvironmentData>())
                        {
                            sensorData_1 += dp.data * dp.SquareDist(transform.position) / squareDistSum_1;
                        }
                        float ir = 0.98f * sensorData_1;
                        float red = 0.02f * sensorData_1;

                        //18, 22, 26, 35 are the temps

                        //i dont really get this math but okay
                        float ratio = (float)((100.0f * Math.Log(red) * Math.Log(red) / Math.Log(ir) / Math.Log(ir) - 1.0f)/1.7);
                        float heartRateCalc = (demosensor.heartRate - demosensor.baseHeartRate)/(demosensor.baseHeartRate*1.5f - demosensor.baseHeartRate);
                        int oxy = (int)(93.0f + ratio*(0.5 + 0.5*heartRateCalc));
                        oxy = (int)(Mathf.Clamp((float)(oxy), 93, 100));

                        SensorData[s_id] = "[" + demosensor.heartRate + ", " + oxy + "]";
                        break;
                    default:
                        //Debug.LogWarning("Underfined sensor data requested");
                        continue;
                }
            }

            // Build sensor data
            //TODO: Figure out why AttachedDevices becomes NULL here
            if (AttachedDevices == null)
                return;

            if (AttachedDevices.Count > 0)
            {
                foreach (Device d in AttachedDevices)
                {
                    string device_data = "";
                    List<string> sensorIDs = DeviceTransitionModule.circuitToSensorIDs[d.ID];
                    List<int> sensors = DeviceTransitionModule.circuitToSensorTypes[d.ID];
                    for (int s_i=0; s_i < sensors.Count;s_i++)
                    {
                        string s_id = sensorIDs[s_i];
                        int s_type = sensors[s_i];
                        Device.Sensor s = Device.SensorFromID(s_type);
                        if (s == Device.Sensor.underfined)
                            continue;

                        string sensor_reading = Device.SensorPrefix(s) + SensorData[s_type];

                        string data_log = $"{{ \"id\": {s_id}, \"type\": {s_type}, \"input\":{{ {sensor_reading} }} }}";

                        //Client.BuildSensorData(this, d.ID, s_id, sensor_reading);
                        if (device_data == "")
                        {
                            device_data += data_log;
                        }
                        else
                        {
                            device_data += "," + data_log;
                        }
                    }
                    device_data = "[" + device_data + "]";
                    Client.BuildDeviceData(ID, d.ID, device_data);
                }
            }
        }
    }
}
