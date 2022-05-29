using HutongGames.PlayMaker.Actions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleOutputList : MonoBehaviour
{
    public GameObject consoleOutputTemplate;
    private GameObject UIMasterControl;
    GameObject consoleOutputPlaceHolder;
    ScrollRect scrollRect;

    float timeRef;
    float timeRefTest;

    private const float timeToAutoScroll = 10;

    public bool autoScroll;

    private string path;

    //test
    private MockList mockOutput;

    private void Awake()
    {
        path = Application.dataPath+"/SimLog.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("=============== LOG START ==================\n");
        writer.Close();
    }
    void Start()
    {
        UIMasterControl = GameObject.Find("UI Master Control");

        consoleOutputPlaceHolder = transform.gameObject;
        consoleOutputTemplate.SetActive(false);

        scrollRect = this.GetComponentInParent<ScrollRect>();

        timeRef = Time.time;
        timeRefTest = Time.time;

        autoScroll = true;
    }

    void Update()
    {
        if (!UIMasterControl.GetComponent<UIMasterControl>().IsShowingConsoleOutput())
            return;

        //TEST
        //if (Time.time- timeRefTest >= 2)
        //{
        //    string log = mockOutput.getLogForDevice(UIMasterControl.GetComponent<UIMasterControl>().SelectedDevice,
        //                        UIMasterControl.GetComponent<UIMasterControl>().SelectedAnimal);
        //    UIMasterControl.GetComponent<UIMasterControl>().SelectedDevice.
        //        AddConsoleLogs(UIMasterControl.GetComponent<UIMasterControl>().SelectedAnimal.ID, log);
        //    UIMasterControl.GetComponent<UIMasterControl>().
        //        AddConsoleOutput(log);
            
        //    timeRefTest = Time.time;
        //}

        //TEST
        //if (Time.time - timeRefTest >= 2)
        //{
        //    addConsoleOutput(localTest());
        //    timeRefTest = Time.time;
        //}

        if (autoScroll)
        {
            ScrollToBottom();
        }
        else
        {
            if (Time.time - timeRef > timeToAutoScroll)
                autoScroll = true;

            if (scrollRect.normalizedPosition == new Vector2(0, 0))
                autoScroll = true;
        }

    }

    public void ScrollToBottom()
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    public void HaltAutoScroll()
    {
        autoScroll = false;
        timeRef = Time.time;
    }

    public void addConsoleOutput(string output)
    {
        GameObject newOutput = Instantiate(consoleOutputTemplate, transform);
        newOutput.SetActive(true);
        newOutput.transform.SetParent(transform);
        newOutput.GetComponent<ConsoleOutputInputField>().UpdateText(output);
    }

    //PROD
    public void InitConsoleOutputList(Device d, Animal a)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        if (!d.AnimalToConsoleLogs.ContainsKey(a.ID))
        {
            d.AnimalToConsoleLogs.Add(a.ID, new List<string>());
        }
        List<string> logs = d.AnimalToConsoleLogs[a.ID];
        
        if (logs.Count != 0)
        {
            print("Loading log");
            foreach (string s in logs)
            {
                addConsoleOutput(s);
            }
        } else
        {
            print("Adding log");
            addConsoleOutput("=============== LOG START ==================");
        }
    }

    public void UpdateConsoleLog(string log)
    {
        addConsoleOutput(log);
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(log + "\n");
        writer.Close();
    }

    public string localTest()
    {
        return "This Program Has Been Running for " + Time.time + "s. ";
    }
}
