using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the main class used for defining all controls to the game for the PC version
/// The main purpose is to enable/disable certain controls based on the current game state and call the correct functions to execute the desired action
/// </summary>
public class ControlsPC : MonoBehaviour
{
    private const bool LOGIN_REQUIRED = true;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private UIListMasterController ui;
    [SerializeField]
    private GameObject settingsUI;
    [SerializeField]
    private MapFadeIn map;
    [SerializeField]
    private PCDeviceAttachController deviceAttach;
    [SerializeField]
    private Client client;

    //private UnityStandardAssets.Characters.FirstPerson.MouseLook m;

    void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

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
            map = GameObject.Find("Map").GetComponent<MapFadeIn>();
        }

        if (deviceAttach == null)
        {
            Debug.LogWarning("PCDeviceAttachController not instantiated, looking...");
            deviceAttach = GameObject.Find("DeviceAttachUI").GetComponent<PCDeviceAttachController>();
        }
    }

    void Update()
    {
        //UI();
        CheckCursorStatus();
        Map();
        UnlockCursor();
        DisableUI();
    }

    /// <summary>
    /// Set whether the cursor should be locked to the screen or not
    /// </summary>
    /// <param name="locked"></param>
    public void SetCursorLock(bool locked)
    {
        if (locked) {
            player.GetComponent<FirstPersonAIO>().crossHair.enabled = false;
        } else {
            player.GetComponent<FirstPersonAIO>().crossHair.enabled = true;
        }
        player.GetComponent<FirstPersonAIO>().ControllerPause();
    }

    private bool GetCursorLock()
    {
        return player.GetComponent<FirstPersonAIO>().controllerPauseState ;
    }

    /// <summary>
    /// Close all UI elements when the cursor is locked
    /// </summary>
    private void DisableUI()
    {
        if (GetCursorLock())
        {
            ui.ToggleUI(false);
            deviceAttach.ToggleDropDown(false);
        }
    }

    // Below are the specific actions that a player can do
    private void UI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ui.ToggleUI();
            SetCursorLock(!ui.IsActive());
        }
    }

    private void Map()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!map.mapOpened)
            {
                SetCursorLock(true);
                map.openMap();
                return;
            }
            else
            {
                SetCursorLock(false);
                map.closeMap();
                return;
            }
        }
    }

    private void UnlockCursor()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            SetCursorLock(!GetCursorLock());
        }
    }

    private void CheckCursorStatus()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (UIMasterControl.ADListSetAside || UIMasterControl.ADListEnabled || map.mapOpened || settingsUI.GetComponent<PCSettingsGear>().paused)
                return;

            if (GetCursorLock())
                SetCursorLock(false);
        }
    }
}