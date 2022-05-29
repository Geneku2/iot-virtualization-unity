using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enable player gameObject to do various things in Menu Room
/// </summary>
public class PlayerMenuPCCustomControl : MonoBehaviour
{
    /// <summary>
    /// Five menu buttons
    /// </summary>
    private GameObject startButton;
    //private GameObject tutorialButton;
    private GameObject quitButton;
    private GameObject restButtonSmall;
    private GameObject restButtonLarge;

    /// <summary>
    /// Cameras
    /// </summary>
    public GameObject head;
    private Camera mainCamera;
    private Camera restCameraSmall;
    private Camera restCameraLarge;
    private Camera teleportCamera;

    /// <summary>
    /// Camera indices
    /// </summary>
    private const int MAIN_CAMERA = -1;
    private const int REST_SMALL = 0;
    private const int REST_LARGE = 1;
    private const int TELEPORT = 2;

    /// <summary>
    /// TP board params
    /// </summary>
    private GameObject tpXAxis;
    private GameObject tpZAxis;
    private GameObject tpPoint;
    private GameObject coordX;
    private GameObject coordZ;
    private float tpOffsetX = 0;
    private float tpOffsetZ = 0.04f;
    private float tpMinX = -4.752f;
    private float tpMinZ = -4.780f;
    private float tpMaxX = 5.238f;
    private float tpMaxZ = 5.171f;

    /// <summary>
    /// Focused button
    /// </summary>
    private GameObject selectedButton;

    public bool allowTeleport;

    private bool isResting = false;
    private Vector3 prevLoc;

    void Start()
    {
        //Initialization
        if (head == null)
        {
            head = transform.Find("Main Camera").gameObject;
        }

        allowTeleport = false;

        mainCamera = this.transform.Find("Main Camera").gameObject.GetComponent<Camera>();
        restCameraLarge = GameObject.Find("Sit Pos 2").transform.Find("Camera").gameObject.GetComponent<Camera>();
        restCameraSmall = GameObject.Find("Sit Pos 1").transform.Find("Camera").gameObject.GetComponent<Camera>();
        teleportCamera = GameObject.Find("Teleport Pos").transform.Find("Camera").gameObject.GetComponent<Camera>();

        switchCamera(MAIN_CAMERA);

        restCameraLarge.gameObject.SetActive(false);
        restCameraSmall.gameObject.SetActive(false);
        teleportCamera.gameObject.SetActive(false);

        tpXAxis = GameObject.Find("X Axis");
        tpZAxis = GameObject.Find("Z Axis");
        tpPoint = GameObject.Find("TP Point");
        coordX = GameObject.Find("Coord X");
        coordZ = GameObject.Find("Coord Z");

        if (startButton == null)
        {
            startButton = GameObject.Find("Start Button");
        }

        /*if (tutorialButton == null)
        {
            tutorialButton = GameObject.Find("Tutorial Button");
        }*/

        if (quitButton == null)
        {
            quitButton = GameObject.Find("Quit Button");
        }

        if (restButtonLarge == null)
        {
            restButtonLarge = GameObject.Find("Rest Button Large");
        }

        if (restButtonSmall == null)
        {
            restButtonSmall = GameObject.Find("Rest Button Small");
        }
    }


    void Update()
    {
        //Rest has the highest priority
        //TODO: smoother camera transition
        if (isResting)
        {
            //Disable player movement while resting
            if (this.GetComponent<FirstPersonAIO>().playerCanMove)
            {
                this.GetComponent<FirstPersonAIO>().playerCanMove = false;
                this.GetComponent<FirstPersonAIO>().enableCameraMovement = false;
            }

            //Check for teleport status
            if (allowTeleport)
            {
                startButton.GetComponent<MenuRoomControlButton>().setColorOpacity(0);
                //Cancel button selection by TAB
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    setAllButtonsOpacity(1);
                    this.GetComponent<FirstPersonAIO>().playerCanMove = true;
                    this.GetComponent<FirstPersonAIO>().enableCameraMovement = true;
                    rest(false);

                    switchCamera(MAIN_CAMERA);
                    return;
                }

                Ray tpRay = teleportCamera.ScreenPointToRay(Input.mousePosition);

                //coordinate-view sync logistics
                if (Physics.Raycast(tpRay, out RaycastHit tpHit, 10f))
                {

                    if (tpHit.collider.name.Equals("Board"))
                    {
                        Vector3 localHit = tpHit.collider.gameObject.transform.InverseTransformPoint(tpHit.point);

                        localHit.x = Mathf.Clamp(localHit.x, tpMinX, tpMaxX);

                        localHit.z = Mathf.Clamp(localHit.z, tpMinZ, tpMaxZ);

                        tpPoint.transform.localPosition = localHit;
                        tpXAxis.transform.localPosition =
                            new Vector3(localHit.x + tpOffsetX, 0, tpXAxis.transform.localPosition.z);
                        tpZAxis.transform.localPosition =
                            new Vector3(tpZAxis.transform.localPosition.x, 0, localHit.z + tpOffsetZ);
                        //display relative coordinates
                        coordX.GetComponent<TextMesh>().text = localHit.x.ToString("F2");
                        coordZ.GetComponent<TextMesh>().text = localHit.z.ToString("F2");
                    }
                }

                //TELEPORT
                if (Input.GetMouseButtonDown(0))
                {
                    MenuMapTelePC.teleport(float.Parse(coordX.GetComponent<TextMesh>().text),
                                           float.Parse(coordZ.GetComponent<TextMesh>().text));
                }

                return;
            }

            //Press Tab to Exit Resting Status
            //TODO: Add on-screen notation for the key
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                this.GetComponent<FirstPersonAIO>().playerCanMove = true;
                this.GetComponent<FirstPersonAIO>().enableCameraMovement = true;
                setAllButtonsOpacity(1);
                rest(false);

                switchCamera(MAIN_CAMERA);
            }

            return;
        }

        //Detect Focused buttons
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 15f))
        {
            if (hit.collider.gameObject == quitButton)
            {
                resetBorderAndSelectedButton(quitButton);

            }
            else if (hit.collider.gameObject == startButton)
            {
                resetBorderAndSelectedButton(startButton);

            }/*
            else if (hit.collider.gameObject == tutorialButton)
            {
                resetBorderAndSelectedButton(tutorialButton);

            }*/
            else if (hit.collider.gameObject == restButtonSmall)
            {
                resetBorderAndSelectedButton(restButtonSmall);

            }
            else if (hit.collider.gameObject == restButtonLarge)
            {
                resetBorderAndSelectedButton(restButtonLarge);
            }
        } else
        {
            resetBorderAndSelectedButton(null);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedButton == null)
            {
                return;
            }

            selectedButton.GetComponent<MenuRoomControlButton>().performFunction();
        }
    }
    /// <summary>
    /// Deselect border of current button and activate the new button
    /// </summary>
    /// <param name="newButton">The focused new button</param>
    private void resetBorderAndSelectedButton(GameObject newButton)
    {
        //This will deactivate current button if user moves his line of sight
        if (newButton == null)
        {
            if (selectedButton != null)
            {
                selectedButton.GetComponent<MenuRoomControlButton>().deactivateBorder();
                selectedButton = null;
            }

            return;
        }

        //Switch selected button
        if (selectedButton != null)
        {
            selectedButton.GetComponent<MenuRoomControlButton>().deactivateBorder();
        }

        selectedButton = newButton;
        selectedButton.GetComponent<MenuRoomControlButton>().activateBorder();
    }
    /// <summary>
    /// Set the opacity for all menu buttons.
    /// This is usually used for setting absolute opacity aka 0s and 1s.
    /// Opacity in between (as player approach/move away from them) is set within the menu button script.
    /// </summary>
    /// <param name="f">The percent opacity of the buttons</param>
    private void setAllButtonsOpacity(float f)
    {
        if (f == 0)
        {
            //Disable all buttons
            restButtonLarge.GetComponent<MenuRoomControlButton>().setColorOpacity(0);
            restButtonSmall.GetComponent<MenuRoomControlButton>().setColorOpacity(0);
            startButton.GetComponent<MenuRoomControlButton>().setColorOpacity(0);
            quitButton.GetComponent<MenuRoomControlButton>().setColorOpacity(0);
            //tutorialButton.GetComponent<MenuRoomControlButton>().setColorOpacity(0);
        }
        else
        {
            //Set three core buttons to being visible but leave the rest buttons untouched
            startButton.GetComponent<MenuRoomControlButton>().setColorOpacity(f);
            quitButton.GetComponent<MenuRoomControlButton>().setColorOpacity(f);
            //tutorialButton.GetComponent<MenuRoomControlButton>().setColorOpacity(f);
        }
    }

    /// <summary>
    /// Switch the camera view
    /// </summary>
    /// <param name="index">Index for the desire camera</param>
    public void switchCamera(int index)
    {
        if (index == MAIN_CAMERA)
        {
            mainCamera.gameObject.SetActive(true);
            restCameraSmall.gameObject.SetActive(false);
            restCameraLarge.gameObject.SetActive(false);
            teleportCamera.gameObject.SetActive(false);

            return;
        }

        //TODO: Smoother Camera Transition. Maybe add some animation
        switch (index)
        {
            case REST_SMALL:
                mainCamera.gameObject.SetActive(false);
                restCameraSmall.gameObject.SetActive(true);
                restCameraSmall.GetComponentInParent<MouseLooker>().resetValues();
                break;
            case REST_LARGE:
                mainCamera.gameObject.SetActive(false);
                restCameraLarge.gameObject.SetActive(true);
                restCameraLarge.GetComponentInParent<MouseLooker>().resetValues();
                break;
            case TELEPORT:
                mainCamera.gameObject.SetActive(false);
                teleportCamera.gameObject.SetActive(true);
                teleportCamera.GetComponentInParent<MouseLooker>().resetValues();
                break;
        }
    }
    /// <summary>
    /// Set rest status, reset allowteleport if rest is canceled
    /// </summary>
    /// <param name="b">Whether to rest or unrest</param>
    public void rest(Boolean b)
    {
        isResting = b;

        if (isResting)
        {
            setAllButtonsOpacity(0);
        } else
        {
            allowTeleport = false;
        }
    }

    /// <summary>
    /// Return if the player is resting
    /// </summary>
    public Boolean isSitting()
    {
        return isResting;
    }
}