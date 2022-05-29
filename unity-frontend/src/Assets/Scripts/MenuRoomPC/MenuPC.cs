using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds all of the callback functions for the PC Menu room buttons
/// </summary>
public class MenuPC : MonoBehaviour
{
    private GameObject player;

    private const int REST_SMALL = 0;
    private const int REST_LARGE = 1;
    private const int TELEPORT = 2;

    private PlayerMenuPCCustomControl playerControl;

    private void Start()
    {
        player = GameObject.Find("Player");
        playerControl = player.GetComponent<PlayerMenuPCCustomControl>();
    }

    public void restSmall()
    {
        playerControl.rest(true);
        playerControl.switchCamera(REST_SMALL);
    }

    public void restLarge()
    {
        playerControl.rest(true);
        playerControl.switchCamera(REST_LARGE);
    }
    public void StartGame()
    {
        playerControl.allowTeleport = true;
        playerControl.rest(true);
        playerControl.switchCamera(TELEPORT);
    }

    public void Tutorial()
    {
        SceneManager.LoadScene("TutorialPC");
    }

    public void QuitGame()
    {
        print("HI");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}