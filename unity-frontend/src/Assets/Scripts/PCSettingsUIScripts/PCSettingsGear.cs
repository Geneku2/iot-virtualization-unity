using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCSettingsGear : MonoBehaviour
{
    [SerializeField]
    private GameObject settingsUI;

    public bool paused = false;

    void Start()
    {
        settingsUI = GameObject.Find("SettingsUI");
        settingsUI.SetActive(false);
    }

    public void ToggleSettings()
    {
        if (!paused)
        {
            // Display the UI and pause the game
            paused = true;
            settingsUI.SetActive(true);
            Time.timeScale = 0.0f;
        }
        else
        {
            // Disappear the UI and continue the game
            paused = false;
            settingsUI.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }
}
