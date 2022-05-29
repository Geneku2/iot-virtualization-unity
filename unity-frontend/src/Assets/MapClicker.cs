using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapClicker : MonoBehaviour, IPointerDownHandler
{
    // The gameObjects/prefabs
    [SerializeField] private RectTransform MapPanel;
    [SerializeField] private Terrain terrain;
    [SerializeField] private GameObject playerIcon;
    [SerializeField] private Transform animalIconTemplates;
    [SerializeField] private AnimalController animalController;
    [SerializeField] private Camera camera;
    private GameObject Player;
    private RectTransform rectTransform;
    private Dictionary<AnimalType, GameObject> animalToSprite;
    private Dictionary<int, GameObject> animalsIcons;

    
    // The width of the map in canvas space
    private float mapWidth;
    // The height of the map in canvas space
    private float mapHeight;
    // The width of the map (X)
    private float terrainWidth;
    // The length of the map (Z)
    private float terrainLength;
    // An height offset to apply when teleporting the player so they don't spawn in the ground
    private float playerHeightOffset = 3f;
    // Boolean indicating if the map is displayed right now
    private bool isMapEnabled = true;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        rectTransform = GetComponent<RectTransform>();

        var rect = MapPanel.rect;
        mapWidth = rect.width;
        mapHeight = rect.height;

        // check if the map exists
        if (MapPanel == null)
            Debug.LogError("Map Canvas is Missing");

        // get terrain size
        var terrainData = terrain.terrainData;
        terrainWidth = terrainData.size.x;
        terrainLength = terrainData.size.z;

        // hide the map UI at default
        MapPanel.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Teleports the player to a location when the map is clicked
    /// </summary>
    /// <param name="eventData">Mouse information</param>
    public void OnPointerDown(PointerEventData eventData)
    {

        Vector2 mapPos = ScreenToMap(eventData.position);
        Vector3 teleportLoc = MapToWorld(mapPos);
        

        // Teleports the player to the location they clicked on the map, plus a little offset so they don't spawn in the ground
        Player.transform.position = teleportLoc + playerHeightOffset * Vector3.up;
    }

    void Update()
    {
        // mousePos is the position of player's mouse on the screen
        // variables below may be useful for future functionalities and developments on the Map UI
        Vector2 mousePos;
        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;
        
        UpdatePlayerIcon();
        // the map will show up if player presses "M" key in the game
        // the map will go away if player presses "M" key when the map has already shown up
    }
    
    /// <summary>
    /// Takes in a screen position and converts it to be relative to the map panel's position
    /// With the origin (0,0) being the middle of the map
    /// </summary>
    /// <param name="screenPos">A position relative to the screen, with the origin at the bottom left</param>
    /// <returns>A position relative to the map panel, with the origin at the middle of the map</returns>
    public Vector2 ScreenToMap(Vector2 screenPos)
    {
        Vector2 mapPoint = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, camera, out mapPoint);
        return mapPoint;
    }

    /// <summary>
    /// Converts a map position to a position on the screen
    /// </summary>
    /// <param name="mapPos">A position on the map, with the origin at the middle of the map</param>
    /// <returns>A position on the screen, with the origin in the bottom left</returns>
    public Vector2 MapToScreen(Vector2 mapPos)
    {
        Vector3 mapPosInWorldSpace = rectTransform.TransformPoint(mapPos);
        return RectTransformUtility.WorldToScreenPoint(camera, mapPosInWorldSpace);
    }
    
    /// <summary>
    /// Takes in a map position and converts it into a world position
    /// Think of this as looking "down" at the game world
    /// </summary>
    /// <param name="mapPos">A position on the map, with the origin in the middle</param>
    /// <returns>A position in the world</returns>
    public Vector3 MapToWorld(Vector2 mapPos) { 
        //print("mapPos: " + mapPos);
        Vector2 relative_to_top_left = mapPos + new Vector2(mapWidth / 2, -mapHeight / 2);
        float worldX = Mathf.Clamp(terrainWidth * (relative_to_top_left.x / mapWidth),0,terrainWidth);
        float worldZ = Mathf.Clamp(terrainLength + terrainLength * (relative_to_top_left.y / mapHeight),0,terrainLength);
        float worldY = terrain.SampleHeight(new Vector3(worldX,0,worldZ));
        return new Vector3(worldX, worldY,
            worldZ);
    }

    /// <summary>
    /// Converts a world position to a 2d location on the map
    /// </summary>
    /// <param name="position">A world position</param>
    /// <returns>A 2d location on the map, equivalent to projecting the entire world onto a 2d surface looking down</returns>
    public Vector2 WorldToMap(Vector3 position)
    {
        return ScreenToMap(new Vector2(position.x * mapWidth / terrainWidth - mapWidth / 2, position.z * mapHeight / terrainLength - mapHeight / 2));
    }


    public void openMap()
    {
        MapPanel.gameObject.SetActive(true);
        isMapEnabled = true;
    }

    public void closeMap()
    {
        MapPanel.gameObject.SetActive(false);
        isMapEnabled = false;
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
                Debug.LogWarning("No associated icon template found for: " + animal);
            }
            else
            {
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
            animalsIcons[animal.ID] = Instantiate(animalToSprite[animal.Type], MapPanel);
        }
    }

    /// <summary>
    /// Move icons based on position of the animals
    /// </summary>
    public void UpdateAnimalIcons()
    {
        if (!isMapEnabled)
        {
            return;
        }

        List<Animal> animals = animalController.Animals;

        foreach (Animal animal in animals)
        {
            Vector2 newAnimalIconPos = MapToScreen(WorldToMap(animal.transform.position));
            animalsIcons[animal.ID].transform.localPosition = new Vector3(newAnimalIconPos.x, newAnimalIconPos.y, 0.1f);
        }
    }

    /// <summary>
    /// Move the player icon based on the player's current location
    /// </summary>
    private void UpdatePlayerIcon()
    {
        Vector3 playerPos = Player.transform.position;
        Vector3 playerRot = Player.transform.rotation.eulerAngles;

        playerIcon.transform.localRotation = Quaternion.Euler(0, 0, -playerRot.y);

        Vector3 mapLocation = MapToScreen(WorldToMap(playerPos));
        playerIcon.transform.localPosition = mapLocation;
    }
}
