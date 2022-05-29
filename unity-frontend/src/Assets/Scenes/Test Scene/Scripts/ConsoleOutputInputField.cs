using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleOutputInputField : MonoBehaviour
{
    // AVOID USING LINES MORE THAN 30 CHARACTERS TO PREVENT WRAPPING

    //SUBJECT TO FUTURE CHANGE
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText(string s)
    {
        this.GetComponent<TMP_InputField>().text = s;
    }
}
