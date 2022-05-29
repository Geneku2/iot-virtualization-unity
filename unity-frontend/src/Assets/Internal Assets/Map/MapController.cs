using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller for interacting with the map for the PC version
/// </summary>
public class MapController : MonoBehaviour
{
    [SerializeField]
    protected GameObject player;

    /// <summary>
    /// Parent map object
    /// </summary>
    [SerializeField]
    protected GameObject mapCanvas;
    [SerializeField]
    protected GameObject TeleportUI;
    [SerializeField]
    protected GameObject TeleportConfrim;
    // private TeleportConfirmBtn teleportConfirmBtn;


    /// <summary>
    /// The terrain the player is on
    /// </summary>
    [SerializeField]
    private Terrain terrain;

    /// <summary>
    /// Total size of the terrain. Y is max height - min height
    /// </summary>
    private Vector3 terrainSize;

    /// <summary>
    /// Size of the map object
    /// </summary>
    private Vector3 mapSize;

    private readonly Vector3 PLAYER_HEIGHT_OFFSET = new Vector3(0, 1.0f, 0);

    /// <summary>
    /// If overriding, call start at the end of the new start method
    /// </summary>
    protected void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not instantiated, looking...");
            player = GameObject.FindWithTag("Player");
        }

        if (mapCanvas == null)
        {
            Debug.LogWarning("MapCanvas not instantiated, looking...");
            mapCanvas = GameObject.Find("MapCanvas");
        }

        if (terrain == null)
        {
            Debug.LogWarning("Terrain not instantiated, looking...");
            terrain = GameObject.Find("Terrain").GetComponent<Terrain>();
        }

        terrainSize = terrain.terrainData.size;
        mapSize = GetComponent<Transform>().localScale;
        Debug.Log("Map size: " + mapSize);

        // Assume map is active for start to run
        mapCanvas.SetActive(false);
        //newly added
        TeleportUI.SetActive(false);
        // teleportConfirmBtn = TeleportConfrim.GetComponent<TeleportConfirmBtn>();

        // Teleport from menu
        if (TransitionModule.shouldTeleport)
        {
            TransitionModule.shouldTeleport = false;
            TeleportFromMenu(TransitionModule.coords);
        }
    }

    public bool IsActive()
    {
        return mapCanvas.activeSelf;
    }

    /// <summary>
    /// Open/Close map
    /// </summary>
    /// <param name="show"></param>
    public void ToggleMap(bool show)
    {
        mapCanvas.SetActive(show);
    }

    /// <summary>
    /// Invert the current state of the map
    /// </summary>
    public void ToggleMap()
    {
        ToggleMap(!mapCanvas.activeSelf);
    }

    /// <summary>
    /// Check if the player has clicked on a map location and if so, teleport the player to where they clicked
    /// </summary>
    public void CheckMapTeleport()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                // TeleportUI.SetActive(true);
                // teleportConfirmBtn.GetPositionInfo(hit.point, PLAYER_HEIGHT_OFFSET);

                Teleport(hit.point);
            }
        }
    }

    /// <summary>
    /// Teleport the player to the selected map location
    /// </summary>
    /// <param name="hitPoint">The selected map location in world coordinates</param>
    public void Teleport(Vector3 hitPoint)
    {
        Debug.Log("Initial teleport coords: " + hitPoint);
        Vector3 teleportLocation = MapToWorldCoords(hitPoint);
        Debug.Log("Teleport location: " + teleportLocation);

        Transform playerTrans = player.GetComponent<Transform>();
        playerTrans.position = teleportLocation;
        playerTrans.position = playerTrans.position + PLAYER_HEIGHT_OFFSET;
    }

    /// <summary>
    /// Converts a point on the map to the corresponding terrain point.
    /// Assumes the point is located in world coordinates (i.e. use the world point that the raycast hit the map)
    /// </summary>
    /// <param name="point">Map location in world coordinates</param>
    /// <returns>A vector where x in [0, terrainSize.x], y = terrain height at the (x, z) location, z in [0, terrainSize.z]</returns>
    public Vector3 MapToWorldCoords(Vector3 point)
    {
        Debug.Log("Initial map coords: " + point);

        Vector3 unscaledMapLocation = this.transform.InverseTransformPoint(point);
        Debug.Log(unscaledMapLocation);
        Debug.Log(this.transform.localScale);
        Vector3 scale = this.transform.localScale;
        // A quad has unit legnth one and is centered on (0,0) so an offset of (0.5, 0.5) is needed
        float mapX = (unscaledMapLocation.x + 0.5f) * scale.x;
        float mapY = (unscaledMapLocation.y + 0.5f) * scale.y;
        Debug.Log(string.Format("Selected map location ({0}, {1})", mapX, mapY));
        float percentX = Mathf.Clamp(mapX / mapSize.x, 0, 1);
        // Y in map coordinates is Z in terrain coordinates
        float percentZ = Mathf.Clamp(mapY / mapSize.y, 0, 1);

        // Calculate location in word coordinates
        float terrainX = terrainSize.x * percentX;
        float terrainZ = terrainSize.z * percentZ;

        Vector3 worldLocation = new Vector3(terrainX, 0, terrainZ);
        worldLocation.y = terrain.SampleHeight(worldLocation);
        Debug.Log("World location: " + worldLocation);
        return worldLocation;
    }

    /// <summary>
    /// Converts a point in world space (i.e. the terrain) to the corresponding point on the map.
    /// </summary>
    /// <param name="x">The x world coord</param>
    /// <param name="z">The z world coord</param>
    /// <returns>A vector where x in [0, mapSize.x], y in [0, mapSize.y]</returns>
    public Vector2 WorldToMapCoords(float x, float z)
    {
        //Debug.Log(string.Format("Initial world coords: ({0}, {1})", x, z));
        float percentX = x / terrainSize.x;
        // Y axis on map is Z axis on terrain
        float percentY = z / terrainSize.z;
        //Debug.Log("Percents: " + percentX + " " + percentY);

        // Local position is local to the center of the map
        float mapX = mapSize.x * percentX - mapSize.x / 2;
        float mapY = mapSize.y * percentY - mapSize.y / 2;
        Vector2 mapLocation = new Vector2(mapX, mapY);
        //Debug.Log("Map Location: " + mapLocation);
        return mapLocation;
    }

    /// <summary>
    /// Converts a point in world space (i.e. the terrain) to the corresponding point on the map.
    /// </summary>
    /// <param name="point">World space coordinates</param>
    /// <returns>A vector where x in [0, mapSize.x], y in [0, mapSize.y]</returns>
    public Vector2 WorldToMapCoords(Vector3 point)
    {
        return WorldToMapCoords(point.x, point.z);
    }

    /// <summary>
    /// Teleport the player to the selected map location, using menu room ([0, 1], [0, 1]) translation to map coordinates
    /// </summary>
    /// <param name="menuCoords">Represents the percentage along each side of the terrain</param>
    public void TeleportFromMenu(Vector3 menuCoords)
    {
        Debug.Log("MenuCoords: " + menuCoords);
        float percentX = Mathf.Clamp(menuCoords.x, 0, 1);
        // Y in map coordinates is Z in terrain coordinates
        float percentZ = Mathf.Clamp(menuCoords.y, 0, 1);

        // Calculate location in word coordinates
        float terrainX = terrainSize.x * percentX;
        float terrainZ = terrainSize.z * percentZ;

        Vector3 worldLocation = new Vector3(terrainX, 0, terrainZ);
        worldLocation.y = terrain.SampleHeight(worldLocation);
        Debug.Log("World location: " + worldLocation);

        Transform playerTrans = player.GetComponent<Transform>();
        playerTrans.position = worldLocation + PLAYER_HEIGHT_OFFSET;
    }
}
