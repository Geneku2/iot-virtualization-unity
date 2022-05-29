using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOG_TEST : MonoBehaviour
{
    public bool humanControl;
    public GameObject animal;
    public GameObject target;
    public Vector3 destination;

    private RealtimeMovementControl moveControl;

    private bool flag = true;

    // Start is called before the first frame update
    void Start()
    {
        moveControl = animal.GetComponent<RealtimeMovementControl>();
        humanControl = animal.GetComponent<AnimalManualControl>().manualControlOn;

        IndicationSystem.indicationSystemSwitch = false;
        IndicationSystem.SetHostAnimal(animal);
    }

    // Update is called once per frame
    void Update()
    {
        if (flag)
        {
            moveControl.move(target);
            flag = false;
        }

        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            print("IS SWITCH TO " + !IndicationSystem.indicationSystemSwitch);
            if (IndicationSystem.indicationSystemSwitch)
                IndicationSystem.TurnOffIS();
            else
                IndicationSystem.TurnOnIS(animal);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            print("MC SWITCH TO "  + !animal.GetComponent<AnimalManualControl>().manualControlOn);
            if (animal.GetComponent<AnimalManualControl>().manualControlOn)
                animal.GetComponent<AnimalManualControl>().SwitchOffManualControl();
            else
                animal.GetComponent<AnimalManualControl>().SwitchToManualControl();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            print("Aniaml Camera SWITCH TO " + !animal.GetComponent<AnimalManualControl>().animalCameraActive);
            if (animal.GetComponent<AnimalManualControl>().animalCameraActive)
                animal.GetComponent<AnimalManualControl>().SwitchOffAnimalCamera();
            else
                animal.GetComponent<AnimalManualControl>().SwitchToAnimalCamera();
        }

        if (humanControl)
            return;
    }
}
