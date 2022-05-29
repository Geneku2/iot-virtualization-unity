using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Points the compass needle north (X = 0, Y = 0, Z = 1)
/// Note that this doesn't TRULY point north, it just sets the needle to be opposite of the camera's rotation
/// This would be fine, but our compass is rotated about 15 degrees angled towards the player in the UI (Y=295 instead of 270) which means the compass needle is ever so slightly off
/// </summary>
public class CompassPointNorth : MonoBehaviour
{
    [SerializeField] private GameObject compassNeedle;
    private Camera camera;
    
    // A counter that increases by 1 each frame until it gets to 100, then it resets back to 0
    private int wiggleCounter = 0;

    private void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Add a little offset to create a "wiggle" effect
        float wiggle = Mathf.Sin((float) (wiggleCounter / 100.0 * 2 * Mathf.PI));
        compassNeedle.transform.localEulerAngles = new Vector3(0, -camera.transform.eulerAngles.y + wiggle, 0);
        wiggleCounter = (wiggleCounter + 1) % 100;


    }
}
