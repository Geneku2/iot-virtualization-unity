using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsMenuVR : MonoBehaviour
{
    [SerializeField]
    private LaserShooter laser;

    void Start()
    {
        if (laser == null)
        {
            Debug.LogWarning("LaserController not instantiated, looking...");
            laser = GameObject.Find("LaserPointer").GetComponent<LaserShooter>();
        }
    }

    private void Update()
    {
        ShootLaser();
    }

    private void ShootLaser()
    {
        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            laser.RenderLaser();
        }
        else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger))
        {
            laser.HitTarget();
        }
    }
}
