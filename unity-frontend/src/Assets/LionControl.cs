using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LionControl : MonoBehaviour
{
    int TIME_CONTROL = 1;
    int DISTANCE_0R_DEGREE_CONTROL = 0;
    int HYBRID_VECTOR_CONTROL = 2;

    public Animator elephantAnimator;

    private GameObject animalObject;

    private ForwardMovementControl forwardMotion;
    private TurnMovementControl turnMotion;

    private bool test;
    private bool test2;

    public GameObject testObject;
    public float speed;

    void Start()
    {
        if (this.GetComponent<Animator>() == null)
        {
            Debug.LogError("No Animator Attached!");
        }
        else
        {
            elephantAnimator = this.GetComponent<Animator>();
        }

        if (transform.Find("Movement Control") == null)
        {
            Debug.LogError("No Movement Control Script Attached!");
        }
        else
        {
            forwardMotion = transform.Find("Movement Control").GetComponent<ForwardMovementControl>();
            turnMotion = transform.Find("Movement Control").GetComponent<TurnMovementControl>();
        }

        test = true;
        test2 = true;

        forwardMotion.initialize(gameObject, elephantAnimator);
        turnMotion.initialize(gameObject, elephantAnimator);
    }


    void Update()
    {
        if (test)
        {
            /*
            turnMotion.initVectorControl(testObject.transform.position);
            forwardMotion.initHybridVectorControl(testObject.transform.position, speed, false, turnMotion);
            turnMotion.start(); 
            forwardMotion.start();*/
            MovementControlScheme scheme =
                new MovementControlScheme(HYBRID_VECTOR_CONTROL, HYBRID_VECTOR_CONTROL, 0, 0, speed, false, testObject.transform.position);

            move(scheme);

            test = false;
        }
        else
        {
            return;
        }
    }

    public void move(MovementControlScheme scheme)
    {
        if (scheme.speed == 0)
        {
            return;
        }

        turnMotion.initMovementScheme(scheme);
        forwardMotion.initMovementScheme(scheme);
    }

}

