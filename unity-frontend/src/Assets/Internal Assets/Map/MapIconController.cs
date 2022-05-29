using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Controls the location of icons on the map
/// </summary>
public class MapIconController : MonoBehaviour{
    private MapController mapController;

    /// <summary>
    /// OVR player controller
    /// </summary>
    [SerializeField]
    private GameObject player;

    /// <summary>
    /// Icon that represents the player on the map
    /// </summary>
    [SerializeField]
    private GameObject playerIcon;

    /// <summary>
    /// Holds all of the sprites for animal icons
    /// </summary>
    [SerializeField]
    private Transform animalIconTemplates;

    /// <summary>
    /// Grab list of animals instantiated
    /// </summary>
    [SerializeField]
    private AnimalController animalController;

    /// <summary>
    /// Maps an animal to its respective sprite
    /// </summary>
    private Dictionary<AnimalType, GameObject> animalToSprite;

    /// <summary>
    /// Instantiated animal icons
    /// Uses ID of animal to keep track
    /// </summary>
    private Dictionary<int, GameObject> animalsIcons;

    void Start()
    {
        mapController = GetComponent<MapController>();
        
        if (player == null)
        {
            Debug.LogWarning("Player not instantiated, looking...");
            player = GameObject.FindWithTag("Player");
        }

        if (playerIcon == null)
        {
            Debug.LogWarning("Player icon not instantiated, looking...");
            playerIcon = transform.parent.Find("MapUserIcon").gameObject;
        }

        if (animalIconTemplates == null)
        {
            Debug.LogWarning("AnimalIconTemplates not instantiated, looking...");
            animalIconTemplates = transform.parent.Find("AnimalIconTemplates");
        }

        if (animalController == null)
        {
            Debug.LogWarning("AnimalController not instantiated, looking...");
            // animalController = GameObject.Find("Animals").GetComponent<AnimalController>();
        }
    }

    void Update()
    {
        if (transform.parent.gameObject.activeSelf)
        {
            UpdatePlayerIcon();
        }
    }

    /// <summary>
    /// Initialize the mapping from AnimalType to sprite
    /// Assumes the templates are named the same as the type and stored in AnimalIconTemplates
    /// </summary>
    private void InitAnimalSprites()
    {
        animalToSprite = new Dictionary<AnimalType, GameObject>();
        foreach (AnimalType animal in Enum.GetValues(typeof(AnimalType)))
        {
            Transform animalSprite = animalIconTemplates.Find(animal.ToString());
            if (animalSprite == null)
            {
                Debug.Log("No associated icon template found for: " + animal.ToString());
            }
            else
            {
                Debug.Log("Found icon for: " + animal.ToString());
                animalToSprite.Add(animal, animalSprite.gameObject);
            }
        }
    }

    /// <summary>
    /// Initialize the set of icons to be drawn on the map
    /// </summary>
    public void CreateAnimalsIcons()
    {
        InitAnimalSprites();
        List<Animal> animals = animalController.Animals;
        animalsIcons = new Dictionary<int, GameObject>();
        foreach (Animal animal in animals)
        {
            animalsIcons[animal.ID] = Instantiate(animalToSprite[animal.Type], transform.parent);
        }
    }

    /// <summary>
    /// Move icons based on position of the animals
    /// </summary>
    public void UpdateAnimalIcons()
    {
        if (!transform.parent.gameObject.activeSelf || mapController == null)
        {
            return;
        }

        List<Animal> animals = animalController.Animals;
        foreach (Animal animal in animals)
        {
            Vector2 newAnimalIconPos = mapController.WorldToMapCoords(animal.transform.position);
            animalsIcons[animal.ID].transform.localPosition = new Vector3(newAnimalIconPos.x, newAnimalIconPos.y, 0);
        }
    }

    /// <summary>
    /// Move the player icon based on the player's current location
    /// </summary>
    private void UpdatePlayerIcon()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 playerRot = player.transform.rotation.eulerAngles;

        playerIcon.transform.localRotation = Quaternion.Euler(0, 0, -playerRot.y);

        Vector3 mapLocation = mapController.WorldToMapCoords(playerPos);
        playerIcon.transform.gameObject.transform.localPosition = mapLocation;
    }
}
