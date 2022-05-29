using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller to drive the tutorial. Each state represents one step in the tutorial
/// </summary>
public class TutorialControllerPC : MonoBehaviour
{
    // Order of tutorial is defined as the order of the states listed
    public enum State
    {
        Movement = 1,
        Jump = 2,
        Sprint = 3,
        Map = 4,
        MapTele = 5,
        UnlockCursor = 6,
        DeviceAttachDropdown = 7,
        DeviceAttachSelect = 8,
        ToggleAttachment = 9,
        AttachDevice = 10,
        OpenLists = 11,
        DeviceFilter = 12,
        AnimalFilter = 13,
        DeviceConsole = 14,
        End = 15
    }

    public State currentState;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private AnimalController animalController;
    [SerializeField]
    private AnimalListController animalListController;
    [SerializeField]
    private MapIconController mapIconController;
    [SerializeField]
    private UIListMasterController uiController;

    private AnimalSimulationClient.AnimalInfo defaultAnimal = new AnimalSimulationClient.AnimalInfo { id = 1, name = "Susan", state = AnimalState.Move, type = AnimalType.Elephant };

    private Text tutorialPrompts;

    /// <summary>
    /// How far in front of player to spawn animal
    /// </summary>
    private const float spawnDistance = 40f;

    void Start()
    {
        tutorialPrompts = GetComponentInChildren<Text>();
        currentState = State.Movement;
    }

    void Update()
    {
        if (currentState == State.AttachDevice && animalController.Animals == null)
        {
            CreateDefaultAnimal();
            uiController.AddAnimalButtonHandlers();
        }
        tutorialPrompts.text = tutorialText[currentState];
    }

    /// <summary>
    /// Advance to the next state only if the state passed in is equal to the current state.
    /// </summary>
    /// <param name="current">Used so that the state advances only when it is supposed to</param>
    public void NextState(State current)
    {
        if (currentState != State.End && current == currentState)
        {
            currentState += 1;
        }
    }

    /// <summary>
    /// Used by event triggers
    /// </summary>
    /// <param name="current"></param>
    public void NextState(int current)
    {
        NextState((State)current);
    }

    public void CreateDefaultAnimal()
    {
        Vector3 animalPos = player.transform.position + player.transform.forward * spawnDistance;
        defaultAnimal.pos = new AnimalSimulationClient.Position { x = animalPos.x, z = animalPos.z };
        AnimalSimulationClient.AnimalInfo[] newAnimals = new AnimalSimulationClient.AnimalInfo[1] { defaultAnimal };
        animalController.CreateAnimals(newAnimals);
        // Set the current position of the created animal
        animalController.DoAction(newAnimals);
        animalListController.InitAnimalList();
        mapIconController.CreateAnimalsIcons();
    }

    // TODO: Separate the tutorial text into a file that is read in
    private Dictionary<State, string> tutorialText = new Dictionary<State, string>()
    {
        {State.Movement, "Welcome to African Savannah. Move around with WASD." },
        {State.Jump, "Jump with the spacebar." },
        {State.Sprint, "Hold left shift to sprint." },
        {State.Map, "Press M to open the map." },
        {State.MapTele, "Click on a location on the map to teleport to it." },
        {State.UnlockCursor, "Press left ctrl to unlock the cursor which allows you to interact with the menus and attach devices to animals." },
        {State.DeviceAttachDropdown, "The upper right is where you select a device for attachment. Please click the dropdown arrow to open a list of available devices." },
        {State.DeviceAttachSelect, "Now select a device for attachment." },
        {State.ToggleAttachment, "When it turns green, that menas the device is ready to be attachment. Click on the device in order to toggle the attachment state." },
        {State.AttachDevice, "Click on an animal while the device attachment is green in order to attach a device to the animal."  },
        {State.OpenLists, "Now press tab to open up the device and animal lists. You can use the mouse wheel or scrollbar to scroll through each list." },
        {State.DeviceFilter, "Clicking on a device will filter all animals by that device, showing only the animals with that device attached. Clicking the device again will unfilter it." },
        {State.AnimalFilter, "Clicking on an animal will do the same filtering but by animals, meaning it shows all the devices attached to that animal. Clicking the animal again will unfilter it." },
        {State.DeviceConsole, "Once a device-animal pair has been selected, the console log and LED list open up, displaying information sent from the selected device about the selected animal. Press tab again to close the lists." },
        {State.End, "Congradulations, you are done with the tutorial!" },
    };
}
