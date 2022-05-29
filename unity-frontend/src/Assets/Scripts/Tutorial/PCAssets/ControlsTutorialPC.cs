using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only used to detect inputs and advance tutorial states. Must be used in conjunction with ControllerPC
/// </summary>
public class ControlsTutorialPC : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private UIListMasterController ui;
    [SerializeField]
    private MapController map;
    [SerializeField]
    private PCDeviceAttachController deviceAttach;
    [SerializeField]
    private TutorialControllerPC tutorialController;

    void Awake()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not instantiated, looking...");
            player = GameObject.FindWithTag("Player");
        }

        if (ui == null)
        {
            Debug.LogWarning("UI not instantiated, looking...");
            ui = GameObject.Find("UIListCanvas").GetComponent<UIListMasterController>();
        }

        if (map == null)
        {
            Debug.LogWarning("MapController not instantiated, looking...");
            map = GameObject.Find("Map").GetComponent<MapController>();
        }

        if (deviceAttach == null)
        {
            Debug.LogWarning("PCDeviceAttachController not instantiated, looking...");
            deviceAttach = GameObject.Find("DeviceAttachUI").GetComponent<PCDeviceAttachController>();
        }

        if (tutorialController == null)
        {
            Debug.LogWarning("TutorialControllerPC not instantiated, looking...");
            tutorialController = GameObject.Find("TutorialCanvas").GetComponent<TutorialControllerPC>();
        }
    }

    void Update()
    {
        UI();
        Map();
        DetectStateSwitches();
    }

    private void DetectStateSwitches()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            tutorialController.NextState(TutorialControllerPC.State.Jump);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            tutorialController.NextState(TutorialControllerPC.State.Sprint);
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            tutorialController.NextState(TutorialControllerPC.State.UnlockCursor);
        }
        else if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D))
        {
            tutorialController.NextState(TutorialControllerPC.State.Movement);
        }
    }

    // Below are the specific actions that a player can do
    private void UI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tutorialController.NextState(TutorialControllerPC.State.OpenLists);
            tutorialController.NextState(TutorialControllerPC.State.DeviceConsole);
        }
    }

    private void Map()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            tutorialController.NextState(TutorialControllerPC.State.Map);
        }

        if (map.IsActive())
        {
            if (Input.GetMouseButtonDown(0))
            {
                tutorialController.NextState(TutorialControllerPC.State.MapTele);
            }
        }
    }
}
