//Ctrl+F "NOT AVAILABLE" for the stuff I didn't finish yet

//using WebSocket4Net;
using OVRSimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using WebSocketSharp;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using UnitySocketIO;
using UnitySocketIO.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Client to handle sending and receiving information about devices attached to animals and their sensor outputs
/// TODO: combine with AnimalSimulationClient
/// </summary>
public class DeviceSimulationClient : MonoBehaviour
{
    public static DeviceSimulationClient one;

    [SerializeField]
    private AnimalController animalController;
    [SerializeField]
    private DeviceListController deviceListController;

    /// <summary>
    /// Only used to detect pauses.
    /// TODO: generalize for PC and VR versions once VR has a pause function
    /// </summary>
    [SerializeField]
    private PCSettingsGear settings;

    public string consoleLogFileName;

    private Thread connectionThread;

    /// <summary>
    /// Keeps track of whehter a pause signal was sent. Prevents multiple ones from being sent
    /// </summary>
    private bool pauseSent = false;

    private string ipAddress = "3.21.186.44";
    private const int movesimPort = 8888;
    private const int webBackendPort = 80; //either 443 or 80
    private const int kafkaPort = 9092;
    private const int streamingPort = 8766;

    /// <summary>
    /// Used as keys to get to each animals device information
    /// Needs to be sync'd with received server's data
    /// </summary>
    private const string ANIMAL_INFO = "animalhugeinfos";
    private const string DEVICES = "devices";

    [SerializeField]
    
    private SocketIOController io;
    public bool socketConnected = false; 

    private string AUTHTOKEN;
    private HttpClient httpClient;
    private string sessionId;
    private ConcurrentBag<string> bag;

    /// <summary>
    /// Adjust how long to wait before asking server for more sensor data
    /// </summary>
    private const float DELAY_FOR_DEVICE_INFO = 10.0f;
    private float elapsedTime = 0.0f;

    // Stores the OS that the user is on
    string OS = "Linux";

    // Start is called before the first frame update
    void Start()
    {
        if (one == null)
        {
            one = this;
        }

        // Create Console Log file
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
        consoleLogFileName = filePath + "/log_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".txt";
        Debug.Log("Console Log found at:" + consoleLogFileName);


        //LoginUser("test");
        //if (animalController == null)
        //{
        //    Debug.LogWarning("Animal controller not instantiated, looking...");
        //    animalController = GameObject.Find("Animals").GetComponent<AnimalController>();
        //}
        //if (deviceListController == null)
        //{
        //    Debug.LogWarning("Device list controller not instantiated, looking...");
        //    deviceListController = GameObject.Find("DeviceList").GetComponent<DeviceListController>();
        //}

    }

    private void Update()
    {
        // Time.deltaTime can only be called from main thread
        elapsedTime += Time.deltaTime;

        //Debug.Log("Doesn't = NULL: " + (one != null));
    }

    /// <summary>
    /// Creates the message that will be sent to the server. It consists of two parts:
    /// [<AnimalDeviceJson>, 0 or 1 for no pause/pause]
    /// </summary>
    /// <returns></returns>
    private string BuildClientMessage()
    {
        return "[" + BuildAnimalDeviceJson() + ", " + (settings.paused ? "1" : "0") + "]";
    }

    /// <summary>
    /// Builds the JSON that will be sent to the backend telling what sensors are on what animals.
    /// Each device can have a variety of sensors, so the format is a series of nested dictionaries.
    /// Example:
    ///     {
    ///         "[Animal ID 1]": {
    ///              "[Device ID 1]": [List of sensor types],
    ///              "[Device ID 2]": [List of sensor types]
    ///         },
    ///         "[Animal ID 2]": {
    ///              "[Device ID 1]": [List of sensor types],
    ///         }
    ///     }
    /// </summary>
    /// <returns></returns>
    private string BuildAnimalDeviceJson()
    {
        if (animalController.Animals == null)
        {
            return "{}";
        }

        var animalsWithDevices = animalController.Animals.Where(animal => animal.AttachedDevices.Count > 0);
        if (animalsWithDevices.Count() > 0)
        {
            var entries = animalsWithDevices.Select(animal =>
                string.Format("\"{0}\": {{ {1} }}", animal.ID, string.Join(",", animal.AttachedDevices.Select(d =>
                    string.Format("\"{0}\": [{1}]", d.ID, string.Join(",", d.SensorTypes))))));
            return "{" + string.Join(",", entries) + "}";
        }
        else
        {
            return "{}";
        }
    }

    public void UseIPAddress(string addr)
    {
        ipAddress = addr;
        if (ipAddress.StartsWith("http://"))
        {
            ipAddress = ipAddress.Substring("http://".Length);
        }
        else if (ipAddress.StartsWith("https://"))
        {
            ipAddress = ipAddress.Substring("https://".Length);
        }
        if (ipAddress.EndsWith("/"))
        {
            ipAddress = ipAddress.Substring(0, ipAddress.Length - 1);
        }
        Debug.Log("IP Address: " + ipAddress);
    }

    /// <summary>
    /// Given the server's response, extract each animal's sensor data and add it to the appropriate device's logs
    /// Example format (note that unneeded info was removed such as animal type):
    /// {
    ///     "animalhugeinfos": [
    ///         {
    ///             "id": 0,
    ///             "devices": null
    ///         },
    ///         {
    ///             "id": 1,
    ///             "devices": {
    ///                 [Device ID]: {
    ///                     "temperature": 52,
    ///                     "humidity": 7
    ///                 },
    ///                 [Device ID 2]: {
    ///                     "pressure": 24
    ///                 }
    ///             }
    ///        }
    ///    ]
    /// }
    ///
    /// </summary>
    /// <param name="serverMessage"></param>
    private void UpdateDeviceLogs(string serverMessage)
    {
        Debug.Log("UpdateDeviceLogs - IF YOU SEE THIS, LOCATE WHERE IT IS!!!" + serverMessage);

        try
        {
            var jsonMsg = JSON.Parse(serverMessage);
            

            /*should theoretically write parsed data to log
            StreamWriter writer = new StreamWriter(consoleLogFileName, true);
            writer.WriteLine(jsonMsg + "\n");
            writer.Close();
            /**/

            foreach (var animal in jsonMsg[ANIMAL_INFO])
            {
                var animalInfo = animal.Value;
                int animalId = animalInfo["id"];
                var deviceInfo = animalInfo[DEVICES];
                if (deviceInfo != null)
                {
                    foreach (string deviceId in deviceInfo.Keys)
                    {
                        int index = int.Parse(deviceId);
                        //deviceListController.Entries[index].AddConsoleLogs(animalId, deviceInfo[deviceId]);
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("Could not parse server message for device info. Exception message: " + e.Message + ". Server message: " + serverMessage);
        }
    }

    public async Task<string> LoginUser(string tempCode)
    {

        httpClient = new HttpClient();

        string content = $"{{ \"tempCode\": \"{tempCode}\"}}";

        StringContent stringContent = new StringContent(
            content,
            UnicodeEncoding.UTF8,
            "application/json");

        string path_login = $"http://{ipAddress}:{webBackendPort}/api/user/login-with-temp-code";

        try
        {
            HttpResponseMessage response3 = await httpClient.PostAsync(path_login, stringContent);

            string res = "";
            if (response3.IsSuccessStatusCode)
            {
                try
                {
                    res = await response3.Content.ReadAsStringAsync();
                    JObject parsed = JObject.Parse(res);
                    res = parsed["token"].ToString();

                    httpClient.DefaultRequestHeaders.Authorization
                        = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", res);

                    AUTHTOKEN = res;
                    Debug.Log("Authtoken: " + AUTHTOKEN);
                    sessionId = KeyGenerator.GetUniqueKey(30);
                    bag = new ConcurrentBag<string>();

                    // Connect to socketio
                    SocketIOInit();
                    SocketIOConnect();
                    return "ack";
                }
                catch (Exception e)
                {
                    Debug.LogError("Could not read Content of HTTP Response to LoginUser: " + e.InnerException);
                    throw e;
                }

            }
            else
            {
                throw new Exception("Did not receive successful server status code at " + path_login);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not reach Login Path " + path_login + ": \t" + e.InnerException);
            throw e;
        }

    }

    // <summary>
    // Init new thread for networking
    // </summary>
    //public async Task<string> SendToServer(string sensorValue)
    //{
    //    string return_value = "";
    //    try
    //    {

    //        var connectionThread = new Thread(() =>
    //        {
    //            Thread.CurrentThread.IsBackground = true;
    //            return_value = ConnectToServer(sensorValue);

    //        });

    //        connectionThread.Start();
    //        connectionThread.Join();
    //    }
    //    catch (Exception e)
    //    {
    //        UnityEngine.Debug.Log("DeviceSimulationClient connect exception: " + e.Message);
    //    }

    //    return return_value;

    //}

    public async Task<string> GetUserDevices()
    {

        string path = $"http://{ipAddress}:{webBackendPort}/api/user/circuits";

        HttpResponseMessage response = await httpClient.GetAsync(path);
        string res = "";
        if (response.IsSuccessStatusCode)
        {
            res = await response.Content.ReadAsStringAsync();
        }
        return res;
    }

    public async Task<string> getCircuitDesign(string device_id)
    {

        string path = $"http://{ipAddress}:{webBackendPort}/api/circuit/get/{device_id}";

        HttpResponseMessage response = await httpClient.GetAsync(path);
        string res = "";
        if (response.IsSuccessStatusCode)
        {
            res = await response.Content.ReadAsStringAsync();
        }
        return res;
    }

    private void SocketIOInit()
    {
        Debug.Log("SocketIOInit() Called");
        io.settings.url = ipAddress;
        io.settings.port = webBackendPort;
        io.headers = new Dictionary<string, string>();
        io.headers.Add("Authorization", $"Bearer {AUTHTOKEN}");
        io.Init();
        io.On("connect", (SocketIOEvent e) => {
            Debug.Log("SocketIO: Connection established.");
            if (AUTHTOKEN == "")
                Debug.LogError("Empty token");
            io.Emit("register_user", $"{{ \"token\": \"{AUTHTOKEN}\"}}");
            socketConnected = true;
            //AUTHTOKEN = "";
        });

        /**/
        io.On("disconnect", (SocketIOEvent e) => {
            Debug.Log("SocketIO: Disconnected.");
            socketConnected = false;
        });

        io.On("echo_test", (SocketIOEvent e) =>
        {
            Debug.Log("SocketIO: Echo received: " + e.data);
        });

        io.On("sim_result", (SocketIOEvent e) =>
        {
            Debug.Log("SocketIO: Simulation Results: " + e.data);
            Client.BuildConsoleOutput(e.data, true, consoleLogFileName);
        });
    }

    // Connect socketio
    public void SocketIOConnect()
    {
        if (socketConnected)
        {
            Debug.LogError("SocketIO: Connection already established.");
            return;
        }
        io.Connect();
    }

    // Disconnect socketio.
    public void SocketIODisconnect()
    {
        if (!socketConnected)
        {
            Debug.LogError("SocketIO: Connection already disconnected.");
            return;
        }
        io.Close();
    }

    // Emit 'echo_test'.
    public void SocketIOEcho(string data)
    {
        if (!SocketIOCheckConnection())
            return;
        io.Emit("echo_test", $"\"{data}\"");
    }

    // Emit 'add_device'.
    public void SocketIOAddDevice(string device_id, int animal_id)
    {
        if (!SocketIOCheckConnection())
            return;
        string content = $"{{ \"id\": \"{device_id}_{animal_id}\", \"timestamp\": {Time.time} }}";
        Debug.Log(content);
        //NOT AVAILABLE
        io.Emit("add_device", content);
    }

    // Emit 'remove_device'.
    public void SocketIORemoveDevice(string device_id, int animal_id)
    {
        if (!SocketIOCheckConnection())
            return;
        string content = $"{{ \"id\": \"{device_id}_{animal_id}\"}}";
        io.Emit("remove_device", content);
    }

    // Emit 'get_sim_result'. Send sensor data and get simulation results.
    public void SocketIOGetSimResults(string dataout)
    {
        if (!SocketIOCheckConnection())
            return;
        Debug.Log("Getting Sim Results via SocketIO");
        //sensor reading but not circuit data
        io.Emit("get_sim_result", dataout);
    }

    // Check SocketIO connection status. Return boolean.
    public bool SocketIOCheckConnection()
    {
        if (!socketConnected)
            Debug.LogError("SocketIO: Socket not connected");
        return socketConnected;
    }

    public class KeyGenerator
    {
        internal static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }
    }

    public string sendDataToWebSocket(WebSocket ws, string sensor_data)
    {
        ws.Send(sensor_data);

        string s = null;

        while (s == null)
        {
            bag.TryTake(out s);
            Thread.Sleep(500); 
        }

        return s; 
        

    }

    /// <summary>
    /// Attempt to connect to movesim server
    /// </summary>
    public WebSocket ConnectToServerSocket()
    {

        string content = $"{{ \"session_id\": \"{sessionId}\", \"auth\": \"{AUTHTOKEN}\"}}";
        Debug.Log("Sending request: " + content);
        

        //string path_login = $"http://localhost:8766/sim_results?access_token={AUTHTOKEN}";
        //string path_login = $"http://localhost:8766/sim_results";

        //Use this for dev 
        //var ws = new WebSocket("ws://localhost:8766/sim_results");

        //Use this for prod 
        var ws = new WebSocket($"ws://{ipAddress}:5001/sim_results");


        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Response: " + e.Data);
            bag.Add(e.Data);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log(e.Message); 
        };

        ws.Connect();

        ws.Send(content);

        return ws;

    }

    /// <summary>
    /// Attempt to connect to movesim server
    /// </summary>
    public async Task<string> ConnectToServer(string sensor_data)
    {

        //sensor_data = "";
        string content = $"{{ \"session_id\": \"{sessionId}\", \"auth\": \"{AUTHTOKEN}\", \"sensor_data\": {sensor_data}}}";
        Debug.Log("Sending request: " + content);
        StringContent stringContent = new StringContent(
            content,
            UnicodeEncoding.UTF8,
            "application/json");


        string path_login = $"http://{ipAddress}:{webBackendPort}/streaming/sim_results";

        HttpResponseMessage response3 = await httpClient.PostAsync(path_login, stringContent);

        Debug.Log(response3);

        string res = "";
        if (response3.IsSuccessStatusCode)
        {
            res = await response3.Content.ReadAsStringAsync();

        }

        Debug.Log("Response status code is : " + response3.StatusCode);

        Debug.Log("Connect response: " + res);
        return res;
    }


    private bool ShouldAskForData()
    {
        if (elapsedTime >= DELAY_FOR_DEVICE_INFO)
        {
            elapsedTime = 0.0f;
            return true;
        }
        else
        {
            return false;
        }
    }


}