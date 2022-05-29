using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.IO;
using DigitalRuby.RainMaker;
using UnityEngine.UI;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using OVRSimpleJSON;
using System.IO;
using WebSocketSharp;

/// <summary>
/// Client for interacting with the server
/// TODO: investigate multithreading to prevent blocks while waiting for server response
/// </summary>
public class Client : MonoBehaviour
{
    /// <summary>
    /// Information about animals provided by server
    /// </summary>
    [Serializable]
    public struct AnimalInfo
    {
        public int id;
        public string name;
        public AnimalType type;
        public AnimalState state;
        public Position pos;
    }

    /// <summary>
    /// Location of animal in world
    /// </summary>
    [Serializable]
    public struct Position
    {
        public float x;
        public float z;
    }

    /// <summary>
    /// Data received from server
    /// </summary>
    [Serializable]
    public struct DataIn
    {
        public AnimalInfo[] animals;
        public RainLocation[] rainLocations;
    }

    /// <summary>
    /// Data sent out to server
    /// </summary>
    [Serializable]
    public struct DataOut
    {
        // TODO: figure out what data needs to be outputted
    }

    private const int port = 8080;
    private const string ip = "127.0.0.1";
    private UdpClient client;
    private IPEndPoint endPoint;
    private WebSocket socket; 

    private const string start = "START";
    private const string next = "NEXT";

    private static string[] displayedProperties = new string[] { "message", "brightness" };

    [SerializeField]
    private DeviceListController deviceListController;
    [SerializeField]
    private PCDeviceAttachListController deviceAttachListController;
    [SerializeField]
    private AnimalController animalController;
    [SerializeField]
    private AnimalListController animalListController;
    [SerializeField]
    private MapClicker mapClicker;
    [SerializeField]
    private RainController rainController;
    [SerializeField]
    private Terrain terrain;

    private Vector3 terrainSize;

    // world boundaries
    public static Vector2 min_bound = new Vector2(50f, 50f);
    public static Vector2 max_bound = new Vector2(950f, 950f);

    private float lastUpdateTime;
    private float OutputUpdateInterval = 5f;
    public static float DataUpdateInterval = 5f;

    public static bool simClientEnabled = false;

    private static Dictionary<int, string> SensorDataLog;

    private static UIMasterControl UIMasterControl;

    

    void Start()
    {
        if (animalController == null)
        {
            Debug.LogWarning("Animal controller not instantiated, looking...");
            animalController = GameObject.Find("Animals").GetComponent<AnimalController>();
        }

        if (animalListController == null)
        {
            Debug.LogWarning("Animal list controller not instantiated, looking...");
            animalListController = GameObject.Find("AnimalList").GetComponent<AnimalListController>();
        }

        if (rainController == null)
        {
            Debug.LogWarning("Rain controller not instantiated, looking...");
            rainController = GameObject.Find("Rain").GetComponent<RainController>();
        }

        if (mapClicker == null)
        {
            Debug.LogWarning("MapIconController not instantiated, looking...");
            mapClicker = GameObject.Find("Map").GetComponent<MapClicker>();
        }

        if (terrain == null)
        {
            Debug.LogWarning("Terrain not instantiated, looking...");
            terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        }

        if (!GameObject.Find("UI Master Control"))
        {
            Debug.LogError("UI MASTER CONTROL NOT FOUND");
        } else
        {
            UIMasterControl = GameObject.Find("UI Master Control").GetComponent<UIMasterControl>();
        }

        simClientEnabled = (DeviceSimulationClient.one != null);
        terrainSize = terrain.terrainData.size;
        SensorDataLog = new Dictionary<int, string>();
        
        AnimalSimulationClient.AnimalInfo[] newAnimals = null;
        animalController.CreateAnimals(newAnimals);
        animalListController.InitAnimalList();
        mapClicker.CreateAnimalsIcons();
        UIMasterControl.InitializeADCanvas();
        UIMasterControl.InitializeAnimalsAndDevices();

        //solely used for writing sensor data so it is decodable, not any impact (START)
        string filePath = Directory.GetCurrentDirectory() + "/SimResults";
        try
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

        }
        catch (IOException e)
        {
            Console.WriteLine("File Path cannot be created. Exception: " + e.Message);
        }
        string sensorDecode = filePath + "/DecodeKeys_Sensor.txt";
        File.Delete(sensorDecode);
        StreamWriter writer = new StreamWriter(sensorDecode, true);
        writer.WriteLine("{\"27\": \"GPS\",");
        writer.WriteLine(" \"3\": \"Ultrasonic\",");
        writer.WriteLine(" \"9\": \"AirQuality\",");
        writer.WriteLine(" \"24\": \"Temperature\",");
        writer.WriteLine(" \"25\": \"Humidity\",");
        writer.WriteLine(" \"31\": \"Vitals\"}");
        writer.Close();

        //writing sensor decode (END)
    }

    void Update()
    {
        mapClicker.UpdateAnimalIcons();
        simClientEnabled = (DeviceSimulationClient.one != null);
        if (simClientEnabled)
        {
            if (Time.time - lastUpdateTime > OutputUpdateInterval)
            {
                lastUpdateTime = Time.time;
                string dataout = BuildDataOutput();
                if (dataout != "")
                {
                    //res = deviceSimulationClient.GetComponent<DeviceSimulationClient>().sendDataToWebSocket(socket, dataout);
                    // Get sim results from socketio
                    DeviceSimulationClient.one.SocketIOGetSimResults(dataout);
                    StreamWriter writer = new StreamWriter(DeviceSimulationClient.one.consoleLogFileName, true);
                    writer.WriteLine(dataout + "\n\n");
                    writer.Close();

                    // using dataout as res for in following 2 lines
                    //res = dataout;
                    //BuildConsoleOutput(dataout, true);
                    
                }
            }
        }
    }

    public static void BuildDeviceData(int animal_id, string device_id, string data)
    {
        //Debug.Log("This works");

        string data_log = $"{{\"deviceId\":\"{device_id}_{animal_id}\",\"timestamp\": {Time.time},\"sensors\":{data}}}";

        if (SensorDataLog.ContainsKey(animal_id))
        {
            SensorDataLog[animal_id] += ", " + data_log;
        }
        else
        {
            SensorDataLog.Add(animal_id, data_log);
        }

    }

    private string BuildDataOutput()
    {
        if (SensorDataLog.Count == 0)
        {
            return "";
        }
        string data = string.Join(",", SensorDataLog.Values);
        SensorDataLog = new Dictionary<int, string>();
        string dataout = "[" + data + "]";
        //could not find what this is -hq (16-3-2022)
        //DevConsole.UpdateData(dataout);
        return dataout;
    }

    public static void BuildConsoleOutput(string res, bool write_to_file = false, string consoleLogFileName = "")
    {
        //could not find what this is -hq (16-3-2022)
        Debug.Log("BuildConsoleOutput - IF YOU SEE THIS, LOCATE WHERE IT IS!!!" + res);
        //DevConsole.UpdateResult(res);

        try
        {
            var jsonMsg = JSON.Parse(res);
            Debug.Log("BuildConsoleOutput - IF YOU SEE THIS, LOCATE WHERE IT IS!!!" + jsonMsg);
            string status = jsonMsg["status"];
            switch (status)
            {
                case "ERROR":
                    Debug.LogWarning("Simulation error: " + jsonMsg["message"]);
                    return;
                case "OK":
                    foreach (JSONNode device in (JSONArray) jsonMsg["result"])
                    {
                        string[] idInfo = device["id"].Value.Split('_');
                        string deviceID = idInfo[0];
                        int animalID = int.Parse(idInfo[1]);
                        if (!DeviceListController.idToDevice.ContainsKey(deviceID))
                        {
                            Debug.LogWarning("Device " + deviceID + " not found when outputting to console");
                            continue;
                        }

                        foreach (JSONNode component in (JSONArray) device["result"])
                        {
                            JSONNode sensor = component[1];
                            Debug.Log(sensor.Value);
                            if (sensor.Value != "")
                            {
                                continue;
                            }
                            int type = sensor["type"].AsInt;
                            string name = sensor["name"].Value;
                            JSONNode info = sensor["info"];
                            foreach (string propertyName in displayedProperties)
                            {
                                var property = info[propertyName];
                                if (property != null)
                                {
                                    string consoleEntry = $"[{animalID}-{name}-{propertyName}]: {property.Value}";
                                    //Debug.Log("IMPORTANT DECODED SENSOR INFO" + consoleEntry);
                                    DeviceListController.idToDevice[deviceID].AddConsoleLogs(animalID, consoleEntry);
                                    UIMasterControl.AddConsoleOutput(consoleEntry);
                                }
                            }

                        }
                    }
                    break;
                default:
                    Debug.LogError("Unknown status: " + jsonMsg["message"]);
                    return;
            }
            
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not parse server message for device info. Exception message: " + e.Message + ". Server message: " + res);
        }
        // Write to file
        if (write_to_file)
        {
            try
            {
                //Debug.Log("Writing to file" + consoleLogFileName);
                File.AppendAllText(consoleLogFileName, res + "\n");
            }
            catch(IOException e)
            {
                Debug.LogError($"Cannot write to console log file. Exception: {e.Message} . Filename: {consoleLogFileName}");
            }
        }
        
    }
}