using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalInfo : MonoBehaviour
{
    public string name;
    public string type;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void deployAnimal(GameObject animal)
    {
        PlayMakerFSM fsm = animal.GetComponent<PlayMakerFSM>();

        fsm.FsmVariables.GetFsmString("Type").Value = type;
        fsm.FsmVariables.GetFsmString("Name").Value = name;
    }
}
