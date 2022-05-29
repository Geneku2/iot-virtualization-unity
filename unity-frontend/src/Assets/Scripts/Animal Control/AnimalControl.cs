using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AnimalControl : MonoBehaviour
{
    int TIME_CONTROL = 1;
    int DISTANCE_0R_DEGREE_CONTROL = 0;
    int HYBRID_VECTOR_CONTROL = 2;

    public Animator animator;

    private GameObject animalObject;
    /*
    private ForwardMovementControl forwardMotion;
    private TurnMovementControl turnMotion;
    */
    private RealtimeMovementControl moveControl;

    private bool test;
    private bool test2;

    public GameObject testObject;
    public float speed;

    private PlayMakerFSM fsm;
    public bool movementFinished;

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
        /*
        if (transform.Find("Movement Control") == null)
        {
            Debug.LogError("No Movement Control Script Attached!");
        }
        else
        {
            forwardMotion = transform.Find("Movement Control").GetComponent<ForwardMovementControl>();
            turnMotion = transform.Find("Movement Control").GetComponent<TurnMovementControl>();
        }
        */

        moveControl = gameObject.GetComponent<RealtimeMovementControl>();

        test = false;
        test2 = false;
        /*
        //elephantAnimator.SetTrigger("isHit");
        forwardMotion.initialize(gameObject, animator);
        turnMotion.initialize(gameObject, animator);
        */

        fsm = this.transform.root.GetComponent<PlayMakerFSM>();
        movementFinished = true;
    }

    
    void Update()
    {
        movementFinished = moveControl.movementFinished();
    }

    public void move(Vector3 destination, float v)
    {
        moveControl.move(destination);
    }

    /*
    public MovementControlScheme assembleMovementSchemeFromVector(Vector3 destination, float v)
    {
        MovementControlScheme scheme = new MovementControlScheme(HYBRID_VECTOR_CONTROL, HYBRID_VECTOR_CONTROL,
            0, 0, v, false, destination);

        return scheme;
    }*/

    public void stopMotion()
    {
        moveControl.stopMotion();
    }
}
