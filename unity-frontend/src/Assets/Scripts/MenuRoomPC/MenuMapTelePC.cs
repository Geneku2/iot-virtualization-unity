using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Attach to the map in the menu room to store the location of where the player should be teleported to when they load into the game
/// </summary>
public class MenuMapTelePC : MonoBehaviour
{
    // A plane is 10 units on each side, meaning the max (X,Z) is (5,5)
    private const float maxX = 5.0f;
    private const float maxZ = 5.0f;

    /// <summary>
    /// TP board params
    /// </summary>
    private static float tpMinX = -4.752f;
    private static float tpMinZ = -4.780f;
    private static float tpMaxX = 5.238f;
    private static float tpMaxZ = 5.171f;

    private GameObject player;

    private PlayerMenuPCCustomControl playerControl;
    private void Start()
    {
        player = GameObject.Find("Player");

        playerControl = player.GetComponent<PlayerMenuPCCustomControl>();
    }
    void Update()
    {
        //Previous codes for teleport. This was deleted and replaced by a better one
        /*
        if (!playerControl.isSitting() && Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Vector3 localHit = gameObject.transform.InverseTransformPoint(hit.point);

                    // Set transition data class's static variables [0, 1], then switch scenes
                    TransitionModule.shouldTeleport = true;
                    TransitionModule.coords = new Vector2((localHit.x + maxX) / (2 * maxX), (localHit.z + maxZ) / (2 * maxZ));
                    SceneManager.LoadScene("AfricanSavannahPC");
                }
            }
        }*/
    }

    /// <summary>
    /// Calculate the position on board. 
    /// Formula -- X coordinate: (X + MaxX/2) / (2*MaxX)  Z coordinate: (Z + MaxZ/2) / (2*MaxZ)
    /// </summary>
    /// <returns>The 2D vector representing player position on board.</returns>
    private static Vector3 positionRatioXZ(float x, float z)
    {
        return new Vector2((x + (tpMaxX - tpMinX) / 2) / ((tpMaxX - tpMinX) * 2),
                           (z + (tpMaxZ - tpMinZ) / 2) / ((tpMaxZ - tpMinZ) * 2));
    }

    /// <summary>
    /// Initialize teleport routine
    /// </summary>
    public static void teleport(float x, float z)
    {
        TransitionModule.shouldTeleport = true;
        TransitionModule.coords = positionRatioXZ(x, z);
        SceneManager.LoadScene("AfricanSavannahPC");
    }
}