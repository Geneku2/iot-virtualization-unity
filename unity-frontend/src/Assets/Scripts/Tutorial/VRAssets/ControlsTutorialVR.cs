using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls class for the tutorial only. Used to advance state
/// Must be used with ControlsVR
/// Should mirror the controls in ControlsVR, but only advances the tutorial state
/// </summary>
public class ControlsTutorialVR : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private UIMasterControllerTutorialVR ui;
    [SerializeField]
    private MapControllerTutorialVR map;
    [SerializeField]
    private LaserShooter laser;
    [SerializeField]
    private TutorialControllerVR tutorial;

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
            ui = GameObject.Find("UICanvas").GetComponent<UIMasterControllerTutorialVR>();
        }

        if (map == null)
        {
            Debug.LogWarning("MapController not instantiated, looking...");
            map = GameObject.Find("Map").GetComponent<MapControllerTutorialVR>();
        }

        if (laser == null)
        {
            Debug.LogWarning("LaserController not instantiated, looking...");
            laser = GameObject.Find("LaserPointer").GetComponent<LaserShooter>();
        }

        if (tutorial == null)
        {
            Debug.LogWarning("TutorialController not instantiated, looking...");
            tutorial = GameObject.Find("Tutorial").GetComponent<TutorialControllerVR>();
        }

    }

    void Update()
    {
        UI();
        ToggleTeleport(true);
        ShootLaser();
        Map();

        DetectMovement();
        DetectTeleport();
    }

    private void ToggleTeleport(bool on)
    {
        player.transform.Find("Teleportation").gameObject.SetActive(on);
    }

    private bool leftThumbstickMovement = false;
    private bool rightThumbstickMovement = false;
    /// <summary>
    /// Detects if the player has used the left and right thumbsticks, if so advance to the next tutorial step
    /// </summary>
    private void DetectMovement()
    {
        if (tutorial.currentState == TutorialControllerVR.State.Movement)
        {
            if (!leftThumbstickMovement)
            {
                leftThumbstickMovement = OVRInput.Get(OVRInput.RawButton.LThumbstickUp) ||
                    OVRInput.Get(OVRInput.RawButton.LThumbstickDown) ||
                    OVRInput.Get(OVRInput.RawButton.LThumbstickLeft) ||
                    OVRInput.Get(OVRInput.RawButton.LThumbstickRight);
            }

            if (!rightThumbstickMovement)
            {
                rightThumbstickMovement = OVRInput.Get(OVRInput.RawButton.RThumbstickLeft) ||
                    OVRInput.Get(OVRInput.RawButton.RThumbstickRight);
            }

            if (leftThumbstickMovement && rightThumbstickMovement)
            {
                tutorial.NextState(TutorialControllerVR.State.Movement);
            }
        }
    }

    private void DetectTeleport()
    {
        if (OVRInput.GetUp(OVRInput.RawButton.RHandTrigger))
        {
            tutorial.NextState(TutorialControllerVR.State.Teleport);
        }
    }

    // Below are the specific actions that a player can do
    private bool consoleScrolled = false;
    private bool ledScrolled = false;
    private void UI()
    {
        if (!map.IsActive() && OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            tutorial.NextState(TutorialControllerVR.State.DeviceList);
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
                    consoleScrolled = true;
                }
                
                if (rightUp || rightDown)
                {
                    ledScrolled = true;
                }

                if (consoleScrolled && ledScrolled)
                {
                    tutorial.NextState(TutorialControllerVR.State.ConsoleScroll);
                }
            }
            else
            {
                if (ui.IsOnlyOneListActive())
                {
                    bool left = OVRInput.Get(OVRInput.RawButton.LThumbstickLeft);
                    bool right = OVRInput.Get(OVRInput.RawButton.LThumbstickRight);
                    if (left || right)
                    {
                        tutorial.NextState(TutorialControllerVR.State.AnimalList);
                    }
                }

                if (leftUp || leftDown)
                {
                    tutorial.NextState(TutorialControllerVR.State.ListScroll);
                }

                if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {
                    ui.Forward();
                }

                if (OVRInput.GetDown(OVRInput.RawButton.X))
                {
                    ui.SpecialAction();
                }
            }

            if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                tutorial.NextState(TutorialControllerVR.State.BackButton);
            }
        }
    }

    private void Map()
    {
        if (!ui.IsActive() && OVRInput.GetDown(OVRInput.RawButton.B))
        {
            tutorial.NextState(TutorialControllerVR.State.Map);
        }
    }

    private void ShootLaser()
    {
        if (!ui.IsActive())
        {
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
            {
                tutorial.NextState(TutorialControllerVR.State.LaserShooter);
            }
        }
    }
}
