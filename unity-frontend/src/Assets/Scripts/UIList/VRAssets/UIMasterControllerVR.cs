using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to control all of the UI elements in the VR version of the game
/// The VR version requires more control over the UI elements currently such as repositioning the UI windows
/// </summary>
public class UIMasterControllerVR : UIListMasterController
{
    // Keeps track of which list is the main list the player is interacting with
    protected ScrollListController currentList;
    protected ScrollListController otherList;

    // Reduces sensitivity of switching lists
    protected const float switchCooldown = 0.1f;
    protected float currCooldown = 0f;

    /// <summary>
    /// How far in front of player to render list
    /// </summary>
    protected const float renderDistance = 7f;

    /// <summary>
    /// How far player moves away before list disappears
    /// </summary>
    protected const float maxDist = 20;

    protected new void Awake()
    {
        base.Awake();

        // Default device list as the first list
        currentList = deviceList;
        otherList = animalList;
        CenterCurrentList();
    }

    protected new void Start()
    {
        // Assume the UI is active initially so all start functions can be ran then turn itself off
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Whether only one of the two lists are active
    /// </summary>
    /// <returns></returns>
    public bool IsOnlyOneListActive()
    {
        return !deviceList.IsActive() || !animalList.IsActive();
    }

    public void ScrollCurrentList(bool up)
    {
        currentList.ScrollList(up);
    }

    public void ScrollConsoleLog(bool up)
    {
        consoleLog.ScrollList(up);
    }

    public void ScrollLEDList(bool up)
    {
        ledList.ScrollList(up);
    }

    /// <summary>
    /// Turn the entire UI on or off
    /// Also filters the current list if both lists are active
    /// </summary>
    /// <param name="show">UI active state</param>
    /// <param name="player">Needed if turning on UI to calculate where to put it in front of player</param>
    public void ToggleUI(bool show, GameObject player = null)
    {
        gameObject.SetActive(show);

        if (show)
        {
            // move device list in front of player with correct rotation
            gameObject.transform.position = player.transform.position + player.transform.forward * renderDistance;
            gameObject.transform.rotation = player.transform.rotation;

            FilterCurrentList();
            currentList.ToggleList(true);
        }
        else
        {
            // Turn off current list so when it is reenabled, the current entry is highlighted
            currentList.ToggleList(false);
        }
    }

    /// <summary>
    /// Swap the current active state of the UI
    /// </summary>
    /// <param name="player">Needed if turning on UI</param>
    public void ToggleUI(GameObject player = null)
    {
        ToggleUI(!gameObject.activeSelf, player);
    }

    /// <summary>
    /// Switch the currently active list
    /// </summary>
    public void SwitchList()
    {
        if (currCooldown < switchCooldown)
        {
            currCooldown += Time.deltaTime;
        }
        else
        {
            currCooldown = 0;
            currentList.ToggleList(false);

            SwapLists();

            currentList.ToggleList(true);
            CenterCurrentList();
        }
    }

    /// <summary>
    /// Select one entry from a list. 
    /// If only one list is active, open up the other list and filter it by the selected entry
    /// If both lists are active, open up the console and LED for that specific combination of entries
    /// </summary>
    public void Forward()
    {
        if (!IsOnlyOneListActive())
        {
            Device d = deviceList.GetCurrentEntry();
            Animal a = animalList.GetCurrentEntry();
            if (d != default && a != default)
            {
                currentDeviceFilter = d;
                currentAnimalFilter = a;
                SetFullUIPosition();
            }
        }
        else
        {
            SwapLists();
            FilterCurrentList();
            currentList.ToggleList(true);
            SetDoubleListPosition();
        }
    }

    /// <summary>
    /// The back button
    /// If console and LED are open, close them
    /// If both lists are open, close current list and reset its filtering
    /// Otherwise close the UI
    /// </summary>
    public void Back()
    {
        if (IsConsoleLEDActive())
        {
            currentDeviceFilter = null;
            currentAnimalFilter = null;
            SetDoubleListPosition();
        }
        else if (!IsOnlyOneListActive())
        {
            currentList.ToggleList(false);
            if (currentList == deviceList)
            {
                deviceList.ResetList();
            }
            else
            {
                animalList.ResetList();
            }
            SwapLists();
            currentList.ToggleList(true);
            CenterCurrentList();
        }
        else
        {
            ToggleUI(false);
        }

    }

    /// <summary>
    /// Call a list's special action if it has one
    /// </summary>
    /// <param name="player"></param>
    public void SpecialAction(GameObject player)
    {
        if (currentList == deviceList)
        {
            deviceList.SelectPrimaryDevice();
        }
        else
        {
            animalList.Teleport(player);
        }
        ToggleUI(false);
    }

    /// <summary>
    /// Checks how far the player is from the UI and closes it if the player is too far away
    /// Compares using the maxDist param
    /// </summary>
    /// <param name="player">The OVR camera</param>
    public void CheckUIDistance(GameObject player)
    {
        float dist = Vector3.Distance(gameObject.transform.position, player.transform.position);
        if (dist > maxDist)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Filter's the current list by the other one only if both are active
    /// </summary>
    protected void FilterCurrentList()
    {
        if (currentList == deviceList && animalList.IsActive())
        {
            deviceList.FilterList(animalList.GetCurrentEntry().AttachedDevices);
        }
        else if (currentList == animalList && deviceList.IsActive())
        {
            animalList.FilterList(deviceList.GetCurrentEntry().AttachedAnimals);
        }
    }

    /// <summary>
    /// Set local position of current list to the canvas center
    /// </summary>
    protected void CenterCurrentList()
    {
        Vector3 oldPos = currentList.listCanvas.transform.localPosition;
        currentList.listCanvas.gameObject.transform.localPosition = new Vector3(0, oldPos.y, oldPos.z);
    }

    /// <summary>
    /// Set local position of current list to the center, and the other list to the left of current
    /// </summary>
    protected void SetDoubleListPosition()
    {
        float offset = deviceList.listCanvas.GetComponent<RectTransform>().rect.width / 2 +
            animalList.listCanvas.GetComponent<RectTransform>().rect.width / 2;
        Vector3 oldPos = otherList.listCanvas.transform.localPosition;
        otherList.listCanvas.transform.localPosition = new Vector3(-offset, oldPos.y, oldPos.z);
        CenterCurrentList();
    }

    /// <summary>
    /// Set local position of console log to the center, the LED list to the right and the 
    /// other two lists to the left
    /// Assumes the position of the two main lists were set with SetDoubleUIPosition
    /// </summary>
    protected void SetFullUIPosition()
    {
        float consoleOffset = consoleLog.listCanvas.GetComponent<RectTransform>().rect.width / 2;

        // Calculate how much to move the lists
        float width = currentList.listCanvas.GetComponent<RectTransform>().rect.width;
        float listOffset = consoleOffset + width / 2;

        Vector3 oldPos = currentList.listCanvas.transform.localPosition;
        currentList.listCanvas.transform.localPosition = new Vector3(oldPos.x - listOffset, oldPos.y, oldPos.z);

        oldPos = otherList.listCanvas.transform.localPosition;
        otherList.listCanvas.transform.localPosition = new Vector3(oldPos.x - listOffset, oldPos.y, oldPos.z);

        // Center console log
        oldPos = consoleLog.listCanvas.gameObject.transform.localPosition;
        consoleLog.listCanvas.gameObject.transform.localPosition = new Vector3(0, oldPos.y, oldPos.z);

        // Place LED list to the left
        oldPos = ledList.listCanvas.gameObject.transform.localPosition;
        width = ledList.listCanvas.GetComponent<RectTransform>().rect.width;
        ledList.listCanvas.gameObject.transform.localPosition = new Vector3(consoleOffset + width / 2, oldPos.y, oldPos.z);
    }

    /// <summary>
    /// Swap the current list
    /// </summary>
    protected void SwapLists()
    {
        ScrollListController temp = currentList;
        currentList = otherList;
        otherList = temp;
    }
}
