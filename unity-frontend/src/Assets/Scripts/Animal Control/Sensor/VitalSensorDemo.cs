using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitalSensorDemo : MonoBehaviour
{
    //NOTE - TYPE IS IN AnimalTemplates
    //if anybody reads this, i feel like it would be a really good idea to make generated animals prefabs instead of loaded inside the scene

    private const float ELEPHANT_DEFAULT = 30;
    private const float LION_DEFAULT = 60;
    private const float ZEBRA_DEFAULT = 125;

    private const float MAX_HR_INCREASE = 1.5f;
    private const float CASUAL_HR_INCREASE = 1.2f;

    // animal types
    public static string[] AnimalTypes = { "Elephant", "Lion", "Zebra" };
    public static float[] animalHeartRates = { ELEPHANT_DEFAULT, LION_DEFAULT, ZEBRA_DEFAULT };

    //used internals
    public float heartRate;
    public float baseHeartRate;
    private string myAnimalType;
    private Vector3 previous;

    //sibling objects
    Animator myAnimator;
    RealtimeMovementControl myMovementControl;
    


    // Start is called before the first frame update
    void Start()
    {
        previous = gameObject.transform.position;
        myAnimator = GetComponent<Animator>();
        myMovementControl = GetComponent<RealtimeMovementControl>();

        if(myAnimator.avatar.name == "ElephantAvatar"){
            myAnimalType = AnimalTypes[0];
            baseHeartRate = animalHeartRates[0];
        } else if(myAnimator.avatar.name == "SK_LionAvatar"){
            myAnimalType = AnimalTypes[1];
            baseHeartRate = animalHeartRates[1];
        } else if(myAnimator.avatar.name == "ZebraAvatar"){
            myAnimalType = AnimalTypes[2];
            baseHeartRate = animalHeartRates[2];
        }

        heartRate = baseHeartRate;
    }

    // Update is called once per frame
    private float timePassed = 0f;
    void FixedUpdate()
    {
        timePassed += Time.deltaTime;

        //const update every second
        if(timePassed > 5f)
        {
            if(myMovementControl.motionState == RealtimeMovementControl.MotionState.Agitated){ //RUNNING
                heartRate += baseHeartRate * 0.2f;
                heartRate = Mathf.Clamp(heartRate, baseHeartRate, MAX_HR_INCREASE * baseHeartRate);
                //Debug.Log("Added Agitated");
            } else if (myMovementControl.motionState == RealtimeMovementControl.MotionState.Stop || previous == gameObject.transform.position){ //RESTING PRESUMED
                heartRate -= baseHeartRate * 0.05f;
                heartRate = Mathf.Clamp(heartRate, baseHeartRate, MAX_HR_INCREASE * baseHeartRate);
                //Debug.Log("Added Stop");
            } else { //ELSE
                heartRate += baseHeartRate * 0.035f;
                heartRate = Mathf.Clamp(heartRate, baseHeartRate, CASUAL_HR_INCREASE * baseHeartRate);
                //Debug.Log("Added Walk");
            }
            timePassed = 0f;
        }
    }
}
