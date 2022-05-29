using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButtonVR : MonoBehaviour, ILaserTarget, ILaserColor
{
    Color ILaserColor.C => Color.green;

    void ILaserTarget.OnLaserHit(RaycastHit target, GameObject origin)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
