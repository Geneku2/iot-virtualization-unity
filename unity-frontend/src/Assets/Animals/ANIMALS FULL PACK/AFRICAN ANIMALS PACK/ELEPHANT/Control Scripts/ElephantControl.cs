using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using HutongGames.PlayMaker;

public class ElephantControl : MonoBehaviour
{
    int TIME_CONTROL = 1;
    int DISTANCE_0R_DEGREE_CONTROL = 0;
    int HYBRID_VECTOR_CONTROL = 2;

    public Animator animator;

    private GameObject animalObject;

    private ForwardMovementControl forwardMotion;
    private TurnMovementControl turnMotion;

    private bool test;
    private bool test2;

    public GameObject testObject;
    public float speed;

    private PlayMakerFSM fsm;
    public bool movementFinished;
    private bool enterWater;

    void Awake()
    {
        if (this.GetComponent<Animator>() == null)
        {
            Debug.LogError("No Animator Attached!");
        }
        else
        {
            animator = this.GetComponent<Animator>();
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
        //elephantAnimator.SetTrigger("isHit");
        forwardMotion.initialize(gameObject, animator);
        turnMotion.initialize(gameObject, animator);

        fsm = this.transform.root.GetComponent<PlayMakerFSM>();
        movementFinished = true;
        enterWater = false;
    }

    
    void Update()
    {
        if (testObject == null) {
            if (fsm.ActiveStateName == "Drink Update" || fsm.ActiveStateName == "Eat Update")
            {
                Debug.Log("Drinkingggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg");
                animator.SetTrigger("Eat");
            }
            movementFinished = forwardMotion.isFinished() && turnMotion.isFinished();

            if (movementFinished)
            {
                FsmBool boolVariable = fsm.FsmVariables.GetFsmBool("DestinationReached");
                if (!fsm.FsmVariables.GetFsmBool("DestinationReached").Value)
                {
                    boolVariable.Value = true;
                }
            }

        } else
        {
            if (movementFinished)
            {
                move(testObject.transform.position, speed);

                movementFinished = false;
            }
        }

    }

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.tag == "WaterResource")
    //    {
    //        enterWater = true;
    //    }
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.tag == "WaterResource")
    //    {
    //        enterWater = false;
    //    }
    //}

    public void move(Vector3 destination, float v)
    {
        MovementControlScheme scheme = assembleMovementSchemeFromVector(destination, v);

        turnMotion.initMovementScheme(scheme);
        forwardMotion.initMovementScheme(scheme);
    }

    public MovementControlScheme assembleMovementSchemeFromVector(Vector3 destination, float v)
    {
        MovementControlScheme scheme = new MovementControlScheme(HYBRID_VECTOR_CONTROL, HYBRID_VECTOR_CONTROL,
            0, 0, v, false, destination);

        return scheme;
    }

    
}
