using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only used in the tutorial to advance states
/// </summary>
public class UIMasterControllerTutorialVR : UIMasterControllerVR
{
    [SerializeField]
    private TutorialControllerVR tutorial;

    public new void Forward()
    {
        if (!IsOnlyOneListActive())
        {
            tutorial.NextState(TutorialControllerVR.State.OpenConsole);
        }
        else
        {
            tutorial.NextState(TutorialControllerVR.State.Filter);
        }
    }

    public void SpecialAction()
    {
        if (currentList == deviceList)
        {
            tutorial.NextState(TutorialControllerVR.State.SelectDevice);
        }
        else
        {
            tutorial.NextState(TutorialControllerVR.State.AnimalTele);
        }
    }
}
