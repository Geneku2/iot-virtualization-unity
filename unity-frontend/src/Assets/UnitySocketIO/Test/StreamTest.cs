using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static Device;
using System.IO;

public class StreamTest : MonoBehaviour
{
    // Login
    public InputField ipAddr;
    public InputField password;
    public GameObject errorText;
    public GameObject loginButton;
    public GameObject logoutButton;
    public Text consoleText;

    private string msg = "";
    private string msg_displayed = "";
    private Color msg_color;
    private bool device_added = false;

    private List<Device> devices;
    private Dictionary<string, Device> idToDevice;
    private Dictionary<int, string> SensorDataLog;
    private Dictionary<Sensor, string> mockSensorData;

    private readonly int animalId = 1;
    private string consoleLogFilename;

    void Awake()
    {
        // https://forum.unity.com/threads/input-field-upper-case.290624/
        password.onValidateInput += delegate (string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };
    }

    void Start()
    {
        devices = new List<Device>();
        idToDevice = new Dictionary<string, Device>();
        SensorDataLog = new Dictionary<int, string>();
        mockSensorData = new Dictionary<Sensor, string>();
        mockSensorData.Add(Sensor.gps, "[99.0, 99.0]");
        mockSensorData.Add(Sensor.ultrasonic, "99.0");
        mockSensorData.Add(Sensor.temperature, "99.0");
        mockSensorData.Add(Sensor.humidity, "99.0");
        mockSensorData.Add(Sensor.airQuality, "99.0");
        mockSensorData.Add(Sensor.underfined, "NaN");

        // Create Console Log file
        string filePath = Directory.GetCurrentDirectory() + "/_SimResults";
        try
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

        }
        catch (IOException e)
        {
            Debug.LogError("File Path cannot be created. Exception: " + e.Message);
        }
        consoleLogFilename = filePath + "/log_" + System.DateTime.Now.ToString("yyMMdd_HHmmss") + ".txt";
        Debug.Log("Console Log found at:" + consoleLogFilename);

    }

    void Update()
    {
        if (msg != msg_displayed)
        {
            msg_displayed = msg;
            UpdateMessage(msg_displayed, msg_color);
        }

        if (DeviceSimulationClient.one.socketConnected)
        {
            loginButton.SetActive(false);
            logoutButton.SetActive(true);
        }
        else
        {
            loginButton.SetActive(true);
            logoutButton.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            LoginUser();
            //LoginButton();
        }

    }

    public void LoginUser()
    {
        string tempCode = password.text;
        msg_color = Color.red;
        if (tempCode == "")
        {
            msg = "Code Empty";
            return;
        }
        if (tempCode.Length != 6)
        {
            msg = "Incorrect Length";
            return;
        }
        foreach (char c in tempCode)
        {
            if (!char.IsDigit(c) && !char.IsUpper(c))
            {
                msg = "Invalid Code";
                return;
            }
        }

        msg_color = Color.green;
        // Change to input field
        GameObject defaultIP = GameObject.Find("Default IP");
        if (defaultIP != null)
            DeviceSimulationClient.one.UseIPAddress(defaultIP.GetComponent<Text>().text);
        if (ipAddr != null)
        {
            if (ipAddr.text != "")
                DeviceSimulationClient.one.UseIPAddress(ipAddr.text);
            Debug.Log("Using IP addr " + ipAddr.text);
        }

        // Change ipAddess of DeviceSimulationClient to localhost when toggled
        GameObject LocalhostToggle = GameObject.Find("LocalhostToggle");
        if (LocalhostToggle != null && LocalhostToggle.GetComponent<Toggle>().isOn)
        {
            DeviceSimulationClient.one.GetComponent<DeviceSimulationClient>().UseIPAddress("localhost");
        }


        msg = "Logging in ...";
        string res;

        Debug.Log("Tempcode: " + tempCode);
        Task.Run(async () =>
        {
            res = await DeviceSimulationClient.one.LoginUser(tempCode);
            if (res != "ack") return;

            //deviceSimulationClient.SocketIOConnect();

            msg = "Loading User Info ...";
            res = await DeviceSimulationClient.one.GetUserDevices();
            //Debug.Log("Found Devices: " + res);
            JArray arr = JArray.Parse(res);
            List<Circuit> circuits = new List<Circuit>();
            Dictionary<string, List<int>> circuitToSensorTypes = new Dictionary<string, List<int>>();
            Dictionary<string, List<string>> circuitToSensorIDs = new Dictionary<string, List<string>>();
            foreach (JToken circuitString in arr)
            {

                Circuit c = new Circuit
                {
                    circuitId = circuitString["_id"].ToString(),
                    circuitName = circuitString["circuitName"].ToString()
                };
                circuits.Add(c);
                msg = "Loading Circuit: " + c.circuitName;
                res = await DeviceSimulationClient.one.getCircuitDesign(c.circuitId);
                string circuit = res;
                Debug.Log("Found Circuit: " + circuit);
                List<int> sensors = new List<int>();
                List<string> sensorIDs = new List<string>();
                ParseSensors(circuit, sensors, sensorIDs);
                circuitToSensorTypes.Add(c.circuitId, sensors);
                circuitToSensorIDs.Add(c.circuitId, sensorIDs);
            }
            Debug.Log(circuits.Count);
            DeviceTransitionModule.circuits = circuits;
            DeviceTransitionModule.circuitToSensorTypes = circuitToSensorTypes;
            DeviceTransitionModule.circuitToSensorIDs = circuitToSensorIDs;
            msg = "Logged in.";
        });
    }

    /// <summary>
    /// Given a circuit, retrieve all of the sensor types as a list of integers
    /// Example circuit: https://github.com/mccaesar/iot-virtualization/blob/refactor/new-backend/web-backend/sample-data/sample-circuit.json
    /// </summary>
    /// <param name="circuit"></param>
    /// <returns></returns>
    private void ParseSensors(string circuit, List<int> sensorTypes, List<string> sensorIDs)
    {
        JObject c_obj = JObject.Parse(circuit);
        // Check empty circuits
        if (!c_obj.ContainsKey("circuitObject"))
            return;

        JToken sensors = c_obj["circuitObject"]["circuits"];
        foreach (JToken sensor in sensors.Values())
        {
            int type = int.Parse(sensor["type"].ToString());
            sensorTypes.Add(type);
            Debug.Log("Sensor type: " + type);
            string id = sensor["id"].ToString();
            sensorIDs.Add(id);
            Debug.Log("Sensor id: " + id);
        }

        //circuit = circuit.ToLower();

        //List<int> sensors = new List<int>();
        //for (int index = 0; index < circuit.Length; index++)
        //{
        //    index = circuit.IndexOf("type", index);
        //    if (index == -1)
        //    {
        //        return sensors;
        //    }
        //    sensors.Add(GetFirstDigit(circuit.Substring(index)));
        //}

        //return sensors;
    }

    /// <summary>
    /// Send logout request only if user has a login token
    /// For the moment, logout is not needed for the backend
    /// </summary>
    public void Logout()
    {
        device_added = false;
        devices = new List<Device>();
        idToDevice = new Dictionary<string, Device>();
        SensorDataLog = new Dictionary<int, string>();
        DeviceSimulationClient.one.SocketIODisconnect();
        msg = "Logged out.";
        msg_color = Color.green;
    }

    // Update message and color
    private void UpdateMessage(string msg, Color c)
    {
        errorText.GetComponent<Text>().text = msg;
        errorText.GetComponent<Text>().color = c;
        errorText.SetActive(true);
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quit");
        //Logout();
    }

    public void AddAllDevices()
    {
        if (!CheckLogin())
            return;

        msg_color = Color.yellow;
        if (device_added)
        {
            msg = "Devices already added.";
            return;
        }
        // Add devices
        consoleText.text = "";
        foreach (Circuit c in DeviceTransitionModule.circuits)
        {
            Debug.Log(c);
            Device newDevice = new Device(c.circuitName, c.circuitId, DeviceTransitionModule.circuitToSensorTypes[c.circuitId]);
            idToDevice.Add(c.circuitId, newDevice);
            devices.Add(newDevice);
            DeviceSimulationClient.one.SocketIOAddDevice(c.circuitId, animalId);
            consoleText.text += c.ToString();
        }
        WriteToFile(consoleText.text);
        msg = "Devices added. Check console for details.";
        device_added = true;
    }

    public void GetSimResults()
    {
        if (!CheckLogin())
            return;

        msg_color = Color.yellow;
        foreach (Device d in devices)
        {
            string device_data = "";
            List<string> sensorIDs = DeviceTransitionModule.circuitToSensorIDs[d.ID];
            List<int> sensors = DeviceTransitionModule.circuitToSensorTypes[d.ID];
            for (int s_i = 0; s_i < sensors.Count; s_i++)
            {
                string s_id = sensorIDs[s_i];
                int s_type = sensors[s_i];
                Sensor s = SensorFromID(s_type);
                msg = s + " data simulated.";

                if (s == Sensor.underfined)
                    continue;
                string sensorData = mockSensorData[s];
                string sensor_reading = SensorPrefix(s) + sensorData;

                string data_log = $"{{\"timestamp\": {Time.time}, \"{s_id}\": {{ \"id\": \"{s_id}\", \"type\": {s_type}, \"input\":{{ {sensor_reading} }} }} }}";
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

            string sensorDataLog = $"\"{d.ID}_{animalId}\": {device_data} ";
            if (SensorDataLog.ContainsKey(animalId))
            {
                SensorDataLog[animalId] += ", " + sensorDataLog;
            }
            else
            {
                SensorDataLog.Add(animalId, sensorDataLog);
            }
        }

        // Build data output
        string output = string.Join(",", SensorDataLog.Values);
        SensorDataLog = new Dictionary<int, string>();
        string dataout = "{" + output + "}";

        DeviceSimulationClient.one.SocketIOGetSimResults(dataout);
        consoleText.text = dataout;
        WriteToFile(dataout);
        Debug.Log(dataout);
        msg = "Sensor data sent. Check console for details.";
    }

    private bool CheckLogin()
    {
        bool connected = DeviceSimulationClient.one.socketConnected;
        if (!connected)
        {
            msg = "Need to log in first.";
            msg_color = Color.red;
        }
        return connected;
    }

    private void WriteToFile(string res)
    {
        try
        {
            File.AppendAllText(consoleLogFilename, res + "\n");
        }
        catch (IOException e)
        {
            Debug.LogError($"Cannot write to console log file. Exception: {e.Message} . Filename: {consoleLogFilename}");
        }
    }
}
