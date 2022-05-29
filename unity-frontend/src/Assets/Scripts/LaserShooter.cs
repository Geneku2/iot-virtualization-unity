using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Any object that interacts with the laser must implement the onLaserHit to receive laser hit events
/// </summary>
interface ILaserTarget
{
    /// <summary>
    /// What the object should do when it is hit by a raycast
    /// </summary>
    /// <param name="target">Information about the hit</param>
    /// <param name="origin">The origin of the laser</param>
    void OnLaserHit(RaycastHit target, GameObject origin);
}

/// <summary>
/// Implement this interface to change the laser color when object is targeted
/// </summary>
interface ILaserColor
{
    Color C
    {
        get;
    }
}

/// <summary>
/// Responsible for managing the laser that the player uses to interact with the world.
/// </summary>
public class LaserShooter : MonoBehaviour
{

    /// <summary>
    /// How far out to shoot the laser
    /// </summary>
    [SerializeField]
    private float range = 100f;

    /// <summary>
    /// Renders the laser into the world
    /// </summary>
    private LineRenderer laser;

    /// <summary>
    /// Keeps track of what the currently target of the laser
    /// </summary>
    private RaycastHit target;

    /// <summary>
    /// Default laser color
    /// </summary>
    private Color defaultColor = Color.white;

    /// <summary>
    /// Whether the laser is being fired
    /// </summary>
    private bool laserActive;

    void Start()
    {
        laser = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        // Manage state of laser
        if (laserActive)
        {
            laser.enabled = true;
            laserActive = false;
        }
        else
        {
            laser.enabled = false;
        }
    }

    /// <summary>
    /// Draws the laser as long as this function is called.
    /// </summary>
    public void RenderLaser()
    {
        laserActive = true;
        Ray laserRay = new Ray(transform.position, transform.forward);
        laser.SetPosition(0, transform.position);
        bool hit = Physics.Raycast(laserRay, out target, range);

        GameObject selected = null;
        if (hit)
        {
            laser.SetPosition(1, target.point);
            selected = target.collider.gameObject;
        }
        else
        {
            laser.SetPosition(1, transform.position + (transform.forward * range));
        }
        ChangeLaserColor(selected);
    }

    /// <summary>
    /// Let every script with ILaserTarget on gameobject know it has been hit
    /// TODO: It is possible this is not needed if there is a way to handle events in Unity VR
    /// </summary>
    /// <param name="origin">optional parameter: The origin gameobject of the laser</param>
    public void HitTarget(GameObject origin = null)
    {
        if (target.collider == null)
        {
            Debug.Log("No collider for target object");
            return;
        }
        GameObject selected = target.collider.gameObject;
        ILaserTarget[] targetObjects = selected.GetComponentsInChildren<ILaserTarget>();
        if (targetObjects != null && targetObjects.Length > 0)
        {
            foreach (ILaserTarget targetObject in targetObjects)
            {
                targetObject.OnLaserHit(target, origin);
            }
        }
        else
        {
            Debug.Log(selected.name + " does not have any ILaserTarget interfaces");
        }
    }

    /// <summary>
    /// Changes the color of the laser based on the game object that was hit
    /// Only uses first one found on game object
    /// </summary>
    /// <param name="selected">The gameobject the laser collided with</param>
    private void ChangeLaserColor(GameObject selected)
    {
        if (selected == null)
        {
            laser.material.color = defaultColor;
            return;
        }
        ILaserColor color = selected.GetComponentInChildren<ILaserColor>();
        if (color != null)
        {
            laser.material.color = color.C;
        }
        else
        {
            laser.material.color = defaultColor;
        }
    }
}
