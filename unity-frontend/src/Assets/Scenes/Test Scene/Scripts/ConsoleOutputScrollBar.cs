using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ConsoleOutputScrollBar : MonoBehaviour, IPointerClickHandler
{
    public GameObject consolelogScrollList;
    private GameObject UIMasterControl;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIMasterControl.GetComponent<UIMasterControl>().SelectedDevice == null)
        {
            UIMasterControl.GetComponent<UIMasterControl>().ConsoleOutputListGameobject
                .GetComponent<ConsoleOutputList>().HaltAutoScroll();
        }
        
    }

    public void haltAutoScroll()
    {
        UIMasterControl.GetComponent<UIMasterControl>().ConsoleOutputListGameobject
                .GetComponent<ConsoleOutputList>().HaltAutoScroll();
    }
    

    void Start()
    {
        UIMasterControl = GameObject.Find("UI Master Control");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
