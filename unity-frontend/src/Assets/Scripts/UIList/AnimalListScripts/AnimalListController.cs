using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main controller for opening the list and interacting with animals in the list.
/// Should be attached to the animal list gameobject
/// </summary>
public class AnimalListController : ScrollListButtonController<Animal>
{
    /// <summary>
    /// Wherever the animal list is stored
    /// </summary>
    [SerializeField]
    private AnimalController animalController;

    private Vector3 teleportOffset = new Vector3(5f, 0f, 5f);

    public int elephantCount = -1;
    public int lionCount = -1;
    public int zebraCount = -1;

    new void Awake()
    {
        base.Awake();

        if (listCanvas == null)
        {
            Debug.LogWarning("AnimalListCanvas not instantiated, looking...");
            listCanvas = transform.parent.gameObject;
        }

        if (animalController == null)
        {
            Debug.LogWarning("AnimalController not instantiated, looking...");
            // animalController = GameObject.Find("Animals").GetComponent<AnimalController>();
        }
        else
        {
            Dictionary<AnimalType, int> animal_count = new Dictionary<AnimalType, int>();
            animal_count.Add(AnimalType.Elephant, elephantCount);
            animal_count.Add(AnimalType.Lion, lionCount);
            animal_count.Add(AnimalType.Zebra, zebraCount);
            animalController.setAnimalCount(animal_count);
        }

        if (itemTemplate == null)
        {
            Debug.LogWarning("AnimalItemTemplate not instantiated, looking...");
            itemTemplate = transform.Find("AnimalEntryTemplate").gameObject;
        }
    }

    /// <summary>
    /// Initialize entries list and populate UI list
    /// Add a button handler to set the current index to the button that was pressed
    /// </summary>
    /// <param name="animals"></param>
    public void InitAnimalList(List<Animal> animals)
    {
        entries = animals;
        foreach (Animal animal in animals) 
        {
            AddItem(animal.Name); 
        }
        AddButtonHandler(SetCurrentIndex);
    }

    /// <summary>
    /// Retrieve list of animals from AnimalController
    /// </summary>
    public void InitAnimalList()
    {
        if (animalController != null)
        {
            if (animalController.Animals != null)
            {
                InitAnimalList(animalController.Animals);
            }
            else
            {
                Debug.LogWarning("Animal list not instantiated, AnimalList cannot be initialized");
            }
        }
        else
        {
            Debug.LogWarning("AnimalController not instantiated, AnimalList cannot be initialized");
        }
    }

    /// <summary>
    /// Teleport the player to the currently selected animal
    /// </summary>
    /// <param name="player"></param>
    public void Teleport(GameObject player)
    {
        player.transform.position = GetCurrentEntry().transform.position + teleportOffset;
    }
}
