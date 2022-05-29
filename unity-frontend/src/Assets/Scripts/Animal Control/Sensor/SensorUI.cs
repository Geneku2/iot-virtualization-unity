using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

// extra added stuff
using System.IO;

using TMPro;

public class SensorUI : MonoBehaviour
{
    public bool activated;

    public List<int> sensorId;

    public float deviceReadingsUpdateInterval = 2;
    private float timeRef;

    private Animal animal;
    private List<GameObject> sensorMesh;

    private GameObject sensorUIPanel;
    private GameObject player;

    private float minimumSensorSeparationDistance = 0.25f;

    private bool addingSensor;


    //added by to test with log output, testing and derivates from main project
    public GameObject animalShown;
    public GameObject ConsoleOutputList;
    public GameObject TextArea;
    public TMP_InputField theText;

    private string animalconsoleLogFileName;

    //private enum Sensor
    //{
    //    ultrasonic = 3,
    //    temperature = 24,
    //    humidity = 25,
    //    air_quality = 9
    //}

    //added to accomodate for path to add to log -hq
    void Awake(){
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
        animalconsoleLogFileName = filePath + "/log_" + DateTime.Now.ToString("yyMMdd_HHmmss") + "_READABLE.txt";
    }

    // Start is called before the first frame update
    void Start()
    {
        animal = GetComponent<Animal>();
        sensorMesh = new List<GameObject>();

        activated = false;

        sensorUIPanel = this.transform.Find("DeviceSensorUI").gameObject;

        player = GameObject.Find("Player");

        if (sensorUIPanel == null)
        {
            Debug.LogError("Device Sensor Panel Not Found");
        }

        if (player == null)
        {
            Debug.LogError("Player Not Found");
        }

        if (!this.gameObject.tag.Equals("Animal"))
        {
            Debug.LogError("Animal Tag not assigned! Remember to assign tag to not only this gO" +
                " but also the one that contains collider for animal model!");
        }

        generateSensorUI();

        timeRef = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            sensorUIPanel.transform.rotation = player.transform.rotation;
        }
    }

    void FixedUpdate()
    {
        //Debug.Log(animal.name + ", " + ()); //sensorUIPanel.transform.Find("Sensor").gameObject == null
        if (sensorId.Count != 0)
        {
            if (Time.time - timeRef >= deviceReadingsUpdateInterval)
            {
                updateSensorReadings();

                timeRef = Time.time;

                return;
            }

        } else
        {
            timeRef = Time.time;
        } 
    }

    public void activate()
    {
        if (!activated)
        {
            activated = true;
            StartCoroutine(fadeIn());
        }
    }

    //Set all UI components to be opaque/disappear
    void resetUIComponents()
    {
        activated = false;

        GameObject stem = sensorUIPanel.transform.Find("Stem").gameObject;
        GameObject branch = sensorUIPanel.transform.Find("Branch").gameObject;
        GameObject leaf = sensorUIPanel.transform.Find("Leaf").gameObject;
        GameObject title = sensorUIPanel.transform.Find("Title").gameObject;
        GameObject animalName = sensorUIPanel.transform.Find("Animal Name").gameObject;

        stem.transform.localScale = new Vector3(0, stem.transform.localScale.y, stem.transform.localScale.z);
        branch.transform.localScale = new Vector3(0, branch.transform.localScale.y, branch.transform.localScale.z);
        leaf.transform.localScale = new Vector3(0, leaf.transform.localScale.y, leaf.transform.localScale.z);

        Color title_color = title.GetComponent<TextMesh>().color;
        title_color.a = 0;

        title.GetComponent<TextMesh>().color = title_color;
        animalName.GetComponent<TextMesh>().color = title_color;

        foreach (GameObject sensor in sensorMesh)
        {
            Color c = sensor.GetComponent<TextMesh>().color;
            c.a = 0;

            sensor.GetComponent<TextMesh>().color = c;
        }

        StopAllCoroutines();
    }

    //Fade in effect
    IEnumerator fadeIn()
    {
        GameObject stem = sensorUIPanel.transform.Find("Stem").gameObject;
        GameObject branch = sensorUIPanel.transform.Find("Branch").gameObject;
        GameObject leaf = sensorUIPanel.transform.Find("Leaf").gameObject;
        GameObject title = sensorUIPanel.transform.Find("Title").gameObject;
        GameObject animalName = sensorUIPanel.transform.Find("Animal Name").gameObject;

        float stem_x_final = 0.2444346f;
        float branch_x_final = 0.1183408f;
        float leaf_x_final = 0.1762319f;

        float start = Time.time;
        float duration = 1.5f;
        duration *= (1 - stem.transform.localScale.x / stem_x_final);
        float x_scale_offset = stem.transform.localScale.x / stem_x_final;
        float fullOpacityDuration = 2;
        fullOpacityDuration *= (1 - title.GetComponent<TextMesh>().color.a);
        float opacity_offset = title.GetComponent<TextMesh>().color.a;

        for (; ; )
        {

            if (!activated)
            {
                yield break;
            }

            if ((Time.time - start) / fullOpacityDuration >= 1)
            {
                yield break;
            }

            //Update scale of the beams
            //Will be modified to trig function instead of linear for extra smoothness
            float x_scale = (Time.time - start) / duration + x_scale_offset;

            x_scale = Mathf.Clamp(x_scale, 0, 1);

            Vector3 stem_update = new Vector3(stem_x_final * (x_scale), stem.transform.localScale.y, stem.transform.localScale.z);
            Vector3 branch_update = new Vector3(branch_x_final * (x_scale), branch.transform.localScale.y, branch.transform.localScale.z);
            Vector3 leaf_update = new Vector3(leaf_x_final * (x_scale), leaf.transform.localScale.y, leaf.transform.localScale.z);

            //print(x_scale + x_scale_offset);
            stem.transform.localScale = stem_update;
            branch.transform.localScale = branch_update;
            leaf.transform.localScale = leaf_update;

            float opacity = (Time.time - start) / fullOpacityDuration + opacity_offset;
            opacity = Mathf.Clamp(opacity, 0, 1);

            Color title_color = title.GetComponent<TextMesh>().color;
            title_color.a = opacity;

            //title_color.a = opacity + opacity_offset;
            title.GetComponent<TextMesh>().color = title_color;
            animalName.GetComponent<TextMesh>().color = title_color;

            if (opacity <= 1)
            {
                foreach (GameObject sensor in sensorMesh)
                {
                    Color c = sensor.GetComponent<TextMesh>().color;
                    c.a = opacity;

                    sensor.GetComponent<TextMesh>().color = c;
                }
            }

            yield return null;
        }
    }

    //Fade out effect
    IEnumerator fadeOut()
    {
        GameObject stem = sensorUIPanel.transform.Find("Stem").gameObject;
        GameObject branch = sensorUIPanel.transform.Find("Branch").gameObject;
        GameObject leaf = sensorUIPanel.transform.Find("Leaf").gameObject;
        GameObject title = sensorUIPanel.transform.Find("Title").gameObject;
        GameObject animalName = sensorUIPanel.transform.Find("Animal Name").gameObject;

        float stem_x_final = 0.2444346f;
        float branch_x_final = 0.1183408f;
        float leaf_x_final = 0.1762319f;

        float start = Time.time;
        float duration = 1.5f;
        float stem_delta_x = stem_x_final / duration;
        float branch_delta_x = branch_x_final / duration;
        float leaf_delta_x = leaf_x_final / duration;
        float zeroOpacityDuration = 2;
        float delta_opacity = 1 / zeroOpacityDuration;

        for(; ; )
        {

            if (activated)
            {
                yield break;
            }

            if (stem.transform.localScale.x == 0 && title.GetComponent<TextMesh>().color.a == 0)
            {
                resetUIComponents();
                yield break;
            }

            //Update scale of the beams
            //Will be modified to trig function instead of linear for extra smoothness

            Vector3 stem_update = new Vector3(stem.transform.localScale.x - stem_delta_x * Time.deltaTime, stem.transform.localScale.y, stem.transform.localScale.z);
            Vector3 branch_update = new Vector3(branch.transform.localScale.x - branch_delta_x * Time.deltaTime, branch.transform.localScale.y, branch.transform.localScale.z);;
            Vector3 leaf_update = new Vector3(leaf.transform.localScale.x - leaf_delta_x * Time.deltaTime, leaf.transform.localScale.y, leaf.transform.localScale.z);

            //print(x_scale + x_scale_offset);
            if (stem.transform.localScale.x <= 0)
            {
                stem.transform.localScale = new Vector3(0, stem.transform.localScale.y, stem.transform.localScale.z);
            } else
            {
                stem.transform.localScale = stem_update;
            }

            if (branch.transform.localScale.x < 0)
            {
                branch.transform.localScale = new Vector3(0, branch.transform.localScale.y, branch.transform.localScale.z);
            }
            else
            {
                branch.transform.localScale = branch_update;
            }

            if (leaf.transform.localScale.x < 0)
            {
                leaf.transform.localScale = new Vector3(0, leaf.transform.localScale.y, leaf.transform.localScale.z);
            }
            else
            {
                leaf.transform.localScale = leaf_update;
            }

            float opacity = title.GetComponent<TextMesh>().color.a - delta_opacity * Time.deltaTime;

            opacity = Mathf.Clamp(opacity, 0, 1);

            Color title_color = title.GetComponent<TextMesh>().color;
            title_color.a = opacity;

            //title_color.a = opacity + opacity_offset;
            title.GetComponent<TextMesh>().color = title_color;
            animalName.GetComponent<TextMesh>().color = title_color;

            if (opacity <= 1)
            {
                foreach (GameObject sensor in sensorMesh)
                {
                    Color c = sensor.GetComponent<TextMesh>().color;
                    c.a = opacity;

                    sensor.GetComponent<TextMesh>().color = c;
                }
            }

            yield return null;
        }
    }

    public void deactivate()
    {
        if (activated)
        {
            activated = false;
            StartCoroutine(fadeOut());
        }
    }

    //Build the list for UI
    void generateSensorUITest()
    {
        //Testing only
        //foreach(int s in sensorId)
        //{
        //    sensors.Add(s, "Test");
        //}

        buildUI();

        resetUIComponents();
    }

    public void generateSensorUI()
    {

        buildUI();

        resetUIComponents();
    }

    void buildUI()
    {
        addingSensor = true;

        //using find? come back to

        sensorUIPanel.transform.Find("Animal Name").GetComponent<TextMesh>().text = animal.Name;

        GameObject sensorTemplate = sensorUIPanel.transform.Find("Sensor").gameObject;
        sensorTemplate.SetActive(false);

        Vector3 initPos = sensorTemplate.transform.localPosition;

        int count = 0;

        foreach(GameObject sensor in sensorMesh)
        {
            Destroy(sensor);
        }
        sensorMesh.Clear();

        foreach (KeyValuePair<int, string> entry in animal.SensorData)
        {
            if (Device.SensorFromID(entry.Key) == Device.Sensor.underfined)
                continue;

            Vector3 pos = initPos + new Vector3(0, -minimumSensorSeparationDistance * count, 0);
            string displayString = "", sensorName = "";
            Device.Sensor s = Device.SensorFromID(entry.Key);
            
            sensorName = s.ToString() + "Sensor";
            displayString = Device.SensorPrefix(s) + "\t\t";

            displayString += string.Format("{0:F2}", entry.Value);

            GameObject sensor = Instantiate(sensorTemplate, sensorUIPanel.transform);
            sensor.name = sensorName;
            sensor.GetComponent<TextMesh>().text = displayString;
            sensor.transform.localPosition = pos;

            sensor.SetActive(true);

            //Remember to clear this first
            sensorMesh.Add(sensor);

            count++;
        }
        addingSensor = false;
    }

    //TODO: Update sensorId if new device is attached
    //      Modify the dictionary to reflect real time sensor readings

    //Attach sensor need to modify sensors dic too
    void updateSensorReadings()
    {
        if (addingSensor)
            return;

        int count = 0;
        
        //printing to in game log
        //string processing
        char[] preprocess = animalShown.GetComponent<TextMeshProUGUI>().text.ToCharArray();
        for(int j = 0; j < preprocess.Length; j++){
            if(preprocess[j] == '-'){
                preprocess[j] = '_';
            }
        }
        string postprocess = new string(preprocess);

        //preinitializes text and text area
        if(ConsoleOutputList.transform.Find("Console Output Template(Clone)") != null){
            TextArea = ConsoleOutputList.transform.Find("Console Output Template(Clone)").gameObject;
            theText = TextArea.GetComponent<TMP_InputField>();
            if(postprocess == animal.name){
                theText.text = "=============== LOG NEXT ==================";
            }
        }

        bool nameWritten = false;
        //Logs Start for text
        StreamWriter writer = new StreamWriter(animalconsoleLogFileName, true);
        for (int i = 0; i < animal.SensorData.Count; i++)
        {   
            //sensors[id] = getSensorData(Device.SensorFromID(id));
            KeyValuePair<int, string> entry = animal.SensorData.ElementAt(i);

            if (Device.SensorFromID(entry.Key) == Device.Sensor.underfined){
                continue;
            }

            int id = entry.Key;
            string data = entry.Value;
            GameObject sensorGO = sensorMesh[count];
            string displayString = Device.SensorPrefix(Device.SensorFromID(id)) + "\t\t";
            displayString += data;
            if(!nameWritten){
                writer.WriteLine("=============== " + animal.Name + " LOG NEXT ==================\n");
                writer.WriteLine("Timestamp: " + Time.time + "\n");
                nameWritten = true;
            }
            if(displayString != ""){
                writer.WriteLine(displayString + "\n");
            }
            
            //This entire section is not working, it is supposed to print to log, but i think there are other ways to to that
            //updated 4/12/2022
            
            
            if(ConsoleOutputList.transform.Find("Console Output Template(Clone)") != null && postprocess == animal.name){
                theText.text = theText.text + "\n" + displayString;
            }

            sensorGO.GetComponent<TextMesh>().text = displayString;
            count++;
        }

        writer.Close();
    }

    //Minor modification to Zhili's Animal code
    //float getSensorData(Device.Sensor s)
    //{
    //    if (s == Device.Sensor.underfined)
    //    {
    //        Debug.LogWarning("Underfined sensor data requested");
    //        return 0;
    //    }

    //    if (s == Device.Sensor.ultrasonic)
    //    {
    //        RaycastHit hit;
    //        Transform eyelevel = transform.Find("eyelevel");
    //        if (Physics.Raycast(eyelevel.transform.position, eyelevel.transform.TransformDirection(Vector3.forward), out hit, Device.ULTRASONIC_MAX))
    //        {
    //            //Debug.Log("Hit: " + hit.collider.name + ", Dist:" + hit.distance);
    //            Debug.DrawRay(eyelevel.transform.position, eyelevel.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
    //            return hit.distance;
    //        }

    //        return 0;
    //    }
    //    else
    //    {
    //        GameObject sensorDataObject = GameObject.Find(s.ToString() + "Data");
    //        float squareDistSum = 0f;
    //        float sensorData = 0f;
    //        foreach (EnvironmentData dp in sensorDataObject.transform.GetComponentsInChildren<EnvironmentData>())
    //        {
    //            squareDistSum += dp.SquareDist(transform.position);
    //        }
    //        foreach (EnvironmentData dp in sensorDataObject.transform.GetComponentsInChildren<EnvironmentData>())
    //        {
    //            sensorData += dp.data * dp.SquareDist(transform.position) / squareDistSum;
    //        }
    //        return sensorData;
    //    }
    //}
}
