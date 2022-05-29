using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller to drive the tutorial. Each state represents one step in the tutorial
/// </summary>
public class TutorialControllerVR : MonoBehaviour
{
    // Order of tutorial is defined as the order of the states listed
    public enum State
    {
        Movement,
        Teleport,
        LaserShooter,
        Map,
        MapTele,
        DeviceList,
        ListScroll,
        SelectDevice,
        AttachDevice,
        AnimalList,
        AnimalTele,
        Filter,
        OpenConsole,
        ConsoleScroll,
        BackButton,
        End
    }

    public State currentState = State.Movement;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private AnimalController animalController;
    [SerializeField]
    private AnimalListController animalListController;
    [SerializeField]
    private MapIconController mapIconController;

    private AnimalSimulationClient.AnimalInfo defaultAnimal = new AnimalSimulationClient.AnimalInfo { id = 1, name = "Susan", state = AnimalState.Move, type = AnimalType.Elephant };

    private GameObject tutorialCanvas;

    /// <summary>
    /// How far in front of player to spawn animal
    /// </summary>
    private const float spawnDistance = 40f;

    void Start()
    {
        tutorialCanvas = transform.Find("TutorialCanvas").gameObject;
    }

    void Update()
    {
        if (currentState == State.AttachDevice && animalController.Animals == null)
        {
            CreateDefaultAnimal();    
        }
        tutorialCanvas.GetComponentInChildren<Text>().text = tutorialText[currentState];
    }

    public void NextState(State current)
    {
        if (current == currentState && currentState != State.End)
        {
            currentState += 1;
        }
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
        {State.Movement, "Welcome to African Savannah. Move around with the left thumbstick and turn with the right." },
        {State.Teleport, "Use the right hand trigger (the button by your middle finger) to select a location and teleport to it" },
        {State.LaserShooter, "Use the right shoulder button (the one by your index finger) to shoot a laser" },
        {State.Map, "Press B (right controller, top button) to open the map" },
        {State.MapTele, "Select a map location with the laser to teleport to it" },
        {State.DeviceList, "Press Y (left controller, top button) to open the device list." },
        {State.ListScroll, "Use the left thumbstick to scroll through the list. Movement is disabled while the list is open" },
        {State.SelectDevice, "Press X (left controller, bottom button) to select a device for attachment" },
        {State.AttachDevice, "Target the animal with the laser to attach your current device. You should see a list of attached devices next to the animal. Note: movement is temporarily disabled." },
        {State.AnimalList, "Open the device list again and move the left thumbstick left and right to switch between the device list and animal list" },
        {State.AnimalTele, "Press X on an animal to teleport next to it" },
        {State.Filter, "Open one of the lists again and press the left shoulder button to filter by that device or animal, showing either all animals with that device attached or all devices attahced to an animal" },
        {State.OpenConsole, "Press the left shoulder button again to open up the console and LED list for a specific device-animal pair" },
        {State.ConsoleScroll, "Scroll through the console with the left thumbstick and the LED list with the right thumbstick" },
        {State.BackButton, "Press the right shoulder button to go back" },
        {State.End, "Congradulations, you are done with the tutorial! You can press Y to close the device list." },
    };
}
