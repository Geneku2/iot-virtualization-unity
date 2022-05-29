//currently not in use because no tutorial button

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Assign functions and borders to each menu room button. 
/// </summary>
public class MenuRoomControlButton : MonoBehaviour
{
    private Function function;

    private GameObject border;

    private enum Function
    {
        start = 0,
        tutorial,
        quit,
        rest_small,
        rest_large
    }

    //At least 15f because raycast detection in PlayerMenuPCCustomControl is 15f
    //If not, player would be able to select invisible buttons
    private const float minDistanceVisibleForRest = 15f;
    private const float minDistanceToSeeSolidColor = 5f;

    private const string START = "Start Button";
    private const string QUIT = "Quit Button";
    private const string TUTORIAL = "Tutorial Button";
    private const string REST_SMALL = "Rest Button Small";
    private const string REST_LARGE = "Rest Button Large";
    private const string BORDER = "border";

    private GameObject player;

    //For Rest Buttons ONLY
    private Color textColor;
    private Color borderColor;

    private MenuPC mp;

    void Start()
    {
        //Find player
        player = GameObject.Find("Player");

        if (player == null)
        {
            Debug.LogWarning("NO PLAYER GAMEOBJECT IS FOUND.");
        }

        //Master script for menu functions
        if (GameObject.Find("MenuScript") == null)
        {
            Debug.LogWarning("MENU SCRIPT NOT FOUND.");
        }else
        {
            mp = GameObject.Find("MenuScript").GetComponent<MenuPC>();
        }

        //Assign Functions by GameObject name
        switch (this.name)
        {
            case START:
                function = Function.start;
                break;

            case TUTORIAL:
                function = Function.tutorial;
                break;

            case QUIT:
                function = Function.quit;
                break;

            case REST_LARGE:
                function = Function.rest_large;
                break;

            case REST_SMALL:
                function = Function.rest_small;
                break;
            default:
                Debug.LogWarning("BUTTON OBJECT NAME ILLEGAL");
                break;
        }

        //Locate border for each button
        if (transform.Find(BORDER) == null)
        {
            Debug.LogWarning("BUTTON BORDER NOT FOUND");
        } else
        {
            border = transform.Find(BORDER).gameObject;
            borderColor = border.GetComponent<TextMesh>().color;
            deactivateBorder();
        }

        //Get Color to use later for transparency settings
        textColor = this.GetComponent<TextMesh>().color;

    }

    // Update is called once per frame
    void Update()
    {
        //Check if the button is a rest button
        if (!player.GetComponent<PlayerMenuPCCustomControl>().isSitting() &&
                (function == Function.rest_small || function == Function.rest_large))
        {
            float distance = Vector3.Distance(player.transform.position,
                transform.TransformPoint(Vector3.zero));

            //Set opacity proportional to the distance between player and the button
            if (distance > minDistanceVisibleForRest)
            {
                setColorOpacity(0);
            }
            else
            {
                setColorOpacity(1 - (distance - minDistanceToSeeSolidColor) / (minDistanceVisibleForRest - minDistanceToSeeSolidColor));
            }
        }

    }
    /// <summary>
    /// Sets the text color of this button
    /// </summary>
    public void setColorOpacity(float f)
    {
        textColor.a = f;
        borderColor.a = f;

        this.GetComponent<TextMesh>().color = textColor;
        border.GetComponent<TextMesh>().color = borderColor;
    }
    /// <summary>
    /// Execute the function assigned to this button
    /// </summary>
    public void performFunction()
    {
        switch (function)
        {
            case Function.start:
                mp.StartGame();
                break;
            case Function.quit:
                mp.QuitGame();
                break;
            case Function.tutorial:
                mp.Tutorial();
                break;
            case Function.rest_large:
                mp.restLarge();
                break;
            case Function.rest_small:
                mp.restSmall();
                break;
        }

    }
    /// <summary>
    /// Activate border
    /// </summary>
    public void activateBorder()
    {
        if (!border.activeSelf)
        {
            border.SetActive(true);
        }
    }
    /// <summary>
    /// Deactivate the dotted line border of the button
    /// </summary>
    public void deactivateBorder()
    {
        if (border.activeSelf)
        {
            border.SetActive(false);
        }
    }
}