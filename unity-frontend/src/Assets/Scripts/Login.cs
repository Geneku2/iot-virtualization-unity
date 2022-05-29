using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Proyecto26;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// Script to control the log in functionality
/// </summary>
public class Login : MonoBehaviour
{
    public enum Platform
    {
        PC,
        VR
    }

    /// <summary>
    /// References the objects on the log in page
    /// </summary>
    public InputField ipAddress;
    public InputField password;
    public GameObject errorText;
    public GameObject loginButton;
    public GameObject logoutButton;

    private bool loading = false;
    private bool toggle = false;

    /// <summary>
    /// Change this depending on whether it is the PC or VR login page
    /// </summary>
    public Platform platform;

    private const string BASE_PATH_LOCAL = "http://localhost:3000";
    private const string BASE_PATH_PROD = "TO BE FILLED";

    /// <summary>
    /// Change this to switch between local and production testing when in the Unity Editor
    /// </summary>
    private const bool USE_LOCAL_PATH = true;

    private void Awake()
    {
        // https://forum.unity.com/threads/input-field-upper-case.290624/
        //password.onValidateInput += delegate (string input, int charIndex, char addedChar) { return char.ToUpper(addedChar); };
    }

    void Update()
    {

        if (DeviceSimulationClient.one.socketConnected)
        {
            if (loading)
            {
                loading = false;
                DontDestroyOnLoad(DeviceSimulationClient.one);
                SceneManager.LoadSceneAsync("MenuRoomPC");
                //SceneManager.LoadScene("MenuRoom" + platform);
                UpdateMessage("Welcome to African Savannah", Color.yellow);
            }
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

    public async void LoginUser()
    {
        string tempCode = password.text.ToUpper();
        if (tempCode == "")
        {
            UpdateMessage("Code Empty", Color.red);
            return;
        }
        if (tempCode.Length != 6)
        {
            UpdateMessage("Incorrect Length", Color.red);
            return;
        }
        foreach (char c in tempCode)
        {
            if (!char.IsDigit(c) && !char.IsUpper(c))
            {
                UpdateMessage("Invalid Code", Color.red);
                return;
            }
        }

        // Change to input field
        GameObject defaultIP = GameObject.Find("Default IP");
        if (defaultIP != null)
            DeviceSimulationClient.one.UseIPAddress(defaultIP.GetComponent<Text>().text);
        if (ipAddress != null)
        {
            if (ipAddress.text != "")
                DeviceSimulationClient.one.UseIPAddress(ipAddress.text);
        }

        // Change ipAddress of DeviceSimulationClient to localhost when toggled
        GameObject LocalhostToggle = GameObject.Find("LocalhostToggle");
        if (LocalhostToggle != null && LocalhostToggle.GetComponent<Toggle>().isOn)
        {
            DeviceSimulationClient.one.GetComponent<DeviceSimulationClient>().UseIPAddress("localhost");
        }


        UpdateMessage("Logging in ...", Color.green);
        string res;

        Debug.Log("Tempcode: " + tempCode);
        // Task.Run(async () =>
        // {
        try
        {
            res = await DeviceSimulationClient.one.LoginUser(tempCode);
            if (res != "ack") return;

            try
            {
                res = await DeviceSimulationClient.one.GetUserDevices();
                Debug.Log("Found Devices: " + res);
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
                    res = await DeviceSimulationClient.one.getCircuitDesign(c.circuitId);
                    string circuit = res;
                    Debug.Log("Found Circuit: " + circuit);
                    List<int> sensors = new List<int>();
                    List<string> sensorIDs = new List<string>();
                    ParseSensors(circuit, sensors, sensorIDs);
                    circuitToSensorTypes.Add(c.circuitId, sensors);
                    circuitToSensorIDs.Add(c.circuitId, sensorIDs);
                }
                DeviceTransitionModule.circuits = circuits;
                DeviceTransitionModule.circuitToSensorTypes = circuitToSensorTypes;
                DeviceTransitionModule.circuitToSensorIDs = circuitToSensorIDs;
                loading = true;
            }
            catch (Exception e)
            {
                Debug.LogError("Could not find users circuits with Device Simulation Client: " + e.InnerException);
                UpdateMessage("Could not get your circuits", Color.red);
                loading = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Could not login user with Device Simulation Client: " + e.InnerException);
            UpdateMessage("Could not log in", Color.red);
            loading = false;
        }
        //   });
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
    }

    /// <summary>
    /// Parses the first digit in a string
    /// Taken from: https://stackoverflow.com/questions/19347758/c-sharp-extract-first-integer-from-a-string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private int GetFirstDigit(string input)
    {
        var digits = input.SkipWhile(c => !Char.IsDigit(c))
            .TakeWhile(Char.IsDigit)
            .ToArray();

        return int.Parse(new string(digits));
    }

    /// <summary>
    /// Send logout request only if user has a login token
    /// For the moment, logout is not needed for the backend
    /// </summary>
    public void Logout()
    {
        DeviceSimulationClient.one.SocketIODisconnect();
        UpdateMessage("Logged out.", Color.green);
    }

    /// <summary>
    /// Build the full URI path
    /// Will always use the prod base path when not in the unity editor
    /// Otherwise, use depends on the USE_LOCAL_PATH variable
    /// </summary>
    /// <param name="path">Must have the leading '/'</param>
    /// <returns></returns>
    private static string BuildUri(string path)
    {
#if UNITY_EDITOR
        if (USE_LOCAL_PATH)
        {
            return BASE_PATH_LOCAL + path;
        }
        else
        {
            return BASE_PATH_PROD + path;
        }
#else
            return BASE_PATH_PROD + path;
#endif
    }

    /// <summary>
    /// Set the message and show it to the user
    /// </summary>
    /// <param name="err"></param>
    private void UpdateMessage(string msg, Color c)
    {
        errorText.GetComponent<Text>().text = msg;
        errorText.GetComponent<Text>().color = c;
        errorText.SetActive(true);
    }

    public void OfflineMode()
    {
        UpdateMessage("Starting offline mode ...", Color.black);
        SceneManager.LoadSceneAsync("MenuRoomPC");
    }
    private void OnApplicationQuit()
    {
        Debug.Log("Quit");
        //Logout();
    }
}
