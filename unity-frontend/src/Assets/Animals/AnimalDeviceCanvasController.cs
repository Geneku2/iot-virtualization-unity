using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the canvas that displays the devices attached to an animal
/// TODO: Possibly making this a scroll list since too many devices will mean the text goes into the ground, but then there is the issue of how to display that many devices to the user
/// </summary>
public class AnimalDeviceCanvasController : MonoBehaviour, ILaserTarget
{
    [SerializeField]
    private GameObject player;

    /// <summary>
    /// The animal script
    /// </summary>
    private Animal animal;

    /// <summary>
    /// Text object that will display the list of devices on an animal
    /// </summary>
    private Text deviceDisplay;

    /// <summary>
    /// The string that will be displayed to the user
    /// </summary>
    private string displayString = "";

    /// <summary>
    /// Max distance to render the device list on animals
    /// </summary>
    private const float renderDist = 626.0f;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not instantiated, looking...");
            player = GameObject.FindWithTag("Player");
        }
        // Assumes canvas is attached as child to the animal GameObject
        animal = gameObject.transform.parent.gameObject.GetComponent<Animal>();
        deviceDisplay = GetComponent<Text>();

        displayString += animal.Name + "\n";
        deviceDisplay.text = displayString;
    }

    void Update()
    {
        RenderList(); 
    }

    /// <summary>
    /// Renders the device list if the player is close enough
    /// </summary>
    private void RenderList()
    {
        float dist = Vector3.Distance(player.transform.position, animal.transform.position);
        if (dist <= renderDist)
        {
            deviceDisplay.enabled = true;
        } 
        else
        {
            deviceDisplay.enabled = false;
        }
    }

    /// <summary>
    /// Update the text of attached devices
    /// </summary>
    /// <param name="target">No needed</param>
    /// <param name="origin">Any object with a SelectedDeviceController</param>
    void ILaserTarget.OnLaserHit(RaycastHit target, GameObject origin)
    {
        if (origin != null)
        {
            SelectedDeviceController controller = origin.GetComponentInChildren<SelectedDeviceController>();
            if (controller != null)
            {
                AttachDevice(controller.Device);
            }
            else
            {
                Debug.LogWarning("No SelectedDeviceController component in GameObject: " + origin);
            }
        }
        else
        {
            Debug.LogWarning("No GameObject passed in");
        }
    }

    /// <summary>
    /// Add a device to the text of devices
    /// </summary>
    /// <param name="d"></param>
    public void AttachDevice(Device d)
    {
        if (d == null)
        {
            Debug.LogWarning("Device is null");
            return;
        }
        
        displayString += d.Name + "\n";
        deviceDisplay.text = displayString;
    }
}
