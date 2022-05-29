using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialButtonVR : MonoBehaviour, ILaserTarget, ILaserColor
{
    Color ILaserColor.C => Color.green;

    void ILaserTarget.OnLaserHit(RaycastHit target, GameObject origin)
    {
        SceneManager.LoadScene("TutorialVR");
    }
}
