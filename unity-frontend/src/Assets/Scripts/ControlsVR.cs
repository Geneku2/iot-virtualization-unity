using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the main class used for defining all controls to the game. 
/// The main purpose is to enable/disable certain controls based on the current game state and call the correct functions to execute the desired action
/// </summary>
public class ControlsVR : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private UIMasterControllerVR ui;
    [SerializeField]
    private MapControllerVR map;
    [SerializeField]
    private LaserShooter laser;

    /// <summary>
    /// Global cooldown to prevent one input from firing multiple times in the same frame
    /// Useful if one button has multiple purposes so only one is activated
    /// </summary>
    private bool buttonPressed = false;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not instantiated, looking...");
            player = GameObject.FindWithTag("Player");
        }

        if (ui == null)
        {
            Debug.LogWarning("UIMasterController not instantiated, looking...");
            ui = GameObject.Find("UICanvas").GetComponent<UIMasterControllerVR>();
        }

        if (map == null)
        {
            Debug.LogWarning("MapController not instantiated, looking...");
            map = GameObject.Find("Map").GetComponent<MapControllerVR>();
        }

        if (laser == null)
        {
            Debug.LogWarning("LaserController not instantiated, looking...");
            laser = GameObject.Find("LaserPointer").GetComponent<LaserShooter>();
        }

    }

    void Update()
    {
        UI();
        Map();
        ShootLaser();

        // Only reset buttonPressed if no buttons are being held down from a previous input
        if (!OVRInput.Get(OVRInput.Button.Any))
        {
            buttonPressed = false;
        }
    }

    // Below are the specific actions that a player can do

    private void UI()
    {
        if (!map.IsActive() && OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            ui.ToggleUI(player);
        }

        if (ui.IsActive())
        {
            bool leftUp = OVRInput.Get(OVRInput.RawButton.LThumbstickUp);
            bool leftDown = OVRInput.Get(OVRInput.RawButton.LThumbstickDown);
            bool rightUp = OVRInput.Get(OVRInput.RawButton.RThumbstickUp);
            bool rightDown = OVRInput.Get(OVRInput.RawButton.RThumbstickDown);
            if (ui.IsConsoleLEDActive())
            {
                if (leftUp || leftDown)
                {
                    ui.ScrollConsoleLog(leftUp);
                }

                if (rightUp || rightDown)
                {
                    ui.ScrollLEDList(rightUp);
                }

            }
            else
            {
                // Allow switching if only one list is active
                if (ui.IsOnlyOneListActive())
                {
                    bool left = OVRInput.Get(OVRInput.RawButton.LThumbstickLeft);
                    bool right = OVRInput.Get(OVRInput.RawButton.LThumbstickRight);
                    if (left || right)
                    {
                        ui.SwitchList();
                    }
                }

                if (leftUp || leftDown)
                {
                    ui.ScrollCurrentList(leftUp);
                }

                if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {
                    ui.Forward();
                }

                if (OVRInput.GetDown(OVRInput.RawButton.X))
                {
                    ui.SpecialAction(player);
                }
            }

            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                ui.Back();
                buttonPressed = true;
            }

            ui.CheckUIDistance(player);
        }

        // Disable movement if UI is open
        player.GetComponent<OVRPlayerController>().EnableRotation = !ui.IsActive();
        player.GetComponent<OVRPlayerController>().EnableLinearMovement = !ui.IsActive();
    }

    private void Map()
    {
        if (!ui.IsActive() && OVRInput.GetDown(OVRInput.RawButton.B))
        {
            map.ToggleMap();
        }

        if (map.IsActive())
        {
            map.CheckMapDistance();
        }
    }

    private void ShootLaser()
    {
        if (!buttonPressed)
        {
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
            {
                laser.RenderLaser();
            }
            else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger))
            {
                laser.HitTarget(origin: player);
            }
        }
    }
}