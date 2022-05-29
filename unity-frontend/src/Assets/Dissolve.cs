using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple class that just sets the dissolve level on the Dissolve material to whatever the variable DissolveLevel is
///
/// Note that the Mask component on the Map makes the materials properties uneditable from within the editor, but it still works in game
/// as long as we don't store a reference to the material directly (the Mask component manually deletes and replaces our material at startup, invalidating our reference)
/// </summary>
public class Dissolve : MonoBehaviour
{
    /// <summary>
    /// The amount of "dissolve" of the shader
    /// Where 0 is none at all, and 1 is completely invisible
    /// </summary>
    [Range(0, 1)] public float DissolveLevel = 1;

    private Image i;
    
    private void Start()
    {
        i = GetComponent<Image>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {

        i.materialForRendering.SetFloat("_Level",DissolveLevel);
    }
    
}
