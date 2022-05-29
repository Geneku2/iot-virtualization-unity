using HutongGames.PlayMaker.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class RealtimeMovementControl : MonoBehaviour
{
    //Whether the control coming from this script is bypassed
    public bool controlBypassed;

    //Behavior States
    public enum MotionState { 
        Stop,
        Casual,
        Stroll,
        Follow,
        Follow_Strict,
        Agitated
    }

    public enum ActionState
    {
        Avoid,
        TryAvoid,
        Normal,
        Track
    }

    public enum GroupState
    {
        Single,
        Leader,
        Fauna,
        Fauna_Outlier
    }
    /*** Behavior Design
     * Casual:
     *      Equal chance of walk and stand, random target
     * Stroll:
     *      Casually setting a waypoint and proceed to reach there
     * Follow: 
     *      Casually follow a leader in a formation
     * Follow_Strict:
     *      Strictly follow a leader in a formation
     * Agitated:
     *      Running directly at target
     * ***/

    //Constants
    private const float MAX_DELTA_SPEED = 1;
    private const float MAX_DELTA_FORWARD_MAGNITUDE = 1;
    private const float MAX_DELTA_TURN_MAGNITUDE = 0.5f;

    private const float FORWARD_SLIGHT_ADJUSTMENT = 0.01f; // For vector sway
    private const float FORWARD_MINOR_ADJUSTMENT = 1;
    private const float FORWARD_ADJUSTMENT = 1.5f;
    private const float FORWARD_QUICK_ADJUSTMENT = 2;
    private const float FORWARD_STEP_ADJUSTMENT = 0.45f;

    private const float TURN_SLIGHT_ADJUSTMENT = 0.07f; // For vector sway
    private const float TURN_STEP_ADJUSTMENT = 0.3f;
    private const float TURN_MINOR_ADJUSTMENT = 0.3f;
    private const float TURN_ADJUSTMENT = 1f;

    private const float MAX_SPEED = 2;
    private const float MAX_FORWARD = 2;
    private const float MIN_FORWARD = 0;
    private const float MAX_TURN = 1;
    private const float MIN_TURN = -1;

    private const float MAX_VISUAL_DISTANCE = 30;
    private const float MIN_ALERT_DISTANCE_CASUAL = 10;
    private const float MIN_ALERT_DISTANCE_AGITATED = 30;
    private const float MIN_ALERT_DISTANCE_FOLLOW = 3;
    private const float MAX_ALERT_DISTANCE_FOLLOW = 6;

    private float THETA = 60;
    private float VIEW_SLOPE = 1;
    private float FOLLOW_SLOPE = 2;

    private float MAX_FLOAT_DIFFERENCE = 0.05f;
    //DESIGN TO BE IMPLEMENTED IN THE FUTURE:

    /*** Animal FOV preliminary Design
     * 
     *                 \         /                   T           
     *                  \   θ   /                    |
     *  -View Slope      \     /   + View Slope      |  - VISUAL DISTANCE    T
     *                    \   /                      |                       |   - ALERT DISTANCE
     *                     O O                       |     Action:Normal     |   Action: Avoid/TryAvoid (SUBJECT TO STATE)
     * ***/

    /*** Fauna formation preliminary design
     * 
     *                          LEADER
     *                   |                   |
     *                  /                     \
     *                 /   Animals             \
     * +Follow Slope  /                         \      -Follow Slope
     *               /                           \
     *              /                  Animals    \ Animals (Outliers)
     *             /    Animals                    \
     *            /                                 \
     *              
     * ***/


    //ESSENTIAL VALUES
    public float speed;
    public float forward;
    public float turn;
    public float forwardVector;
    public float turnVector;
    public GameObject destinationTest;

    //TESTING
    private Transform animalTransform;
    private float prevYRot;
    private float curYRot;

    //PARAMETERS
    public MotionState motionState;
    public ActionState actionState;
    private GroupState groupState;

    //PREMADE PARAMS
    private MotionState AGITATED;
    private MotionState CASUAL;
    private MotionState STROLLING;
    private MotionState FOLLOWING;
    private MotionState FOLLOWING_STRICT;
    private MotionState STOPPING;
    private ActionState AVOID;
    private ActionState TRY_AVOID;
    private ActionState NORMAL;
    private ActionState TRACKING;

    //PROGRESS INDICATOR FOR TRACKING
    private bool isAvoiding;
    private bool isTracking;
    public bool destinationReached;

    //Vector Tracking
    public Vector3 destination;
    private Vector3 orientation;
    //GO Tracking
    private GameObject destinationGameOject;
    public GameObject eyeLevel;
    public GameObject Leader;
    private Animator animalAnimator;

    //Manual Control
    private float turnProgress;
    private float turnVectorManual;
    private float forwardVectorManual;
    

    //PAP
    private bool onPAP = false;

    //REF VALUES
    float timeRef = 0;



    void Start()
    {
        if (transform.Find("Eye Level") == null)
        {
            Debug.LogError("Eye level for animal " + gameObject.name + " not found.");
        } else
        {
            eyeLevel = transform.Find("Eye Level").gameObject;

            animalAnimator = GetComponent<Animator>();
        }

        //TEST
        forward = 1.5f;

        initializeBehaviorStates();

        inItializeEssentialParams();

        initializePremadeParams();

        initializeTrackingProperties();

        timeRef = Time.time;

        animalTransform = this.GetComponentInParent<Transform>();
        prevYRot = animalTransform.rotation.y;
        curYRot = animalTransform.rotation.y;

        //if (destinationTest != null)
        //{
        //    initializeTracking(destinationTest.transform.position);
        //}

        //move(destinationTest.transform.position);
    }

    void initializeBehaviorStates()
    {
        motionState = MotionState.Casual;
        actionState = ActionState.Normal;
        groupState = GroupState.Single;
    }

    void inItializeEssentialParams()
    {
        forward = 0;
        turn = 0;

        forwardVector = 0;
        turnVector = 0;

        speed = 0;
    }

    //Premade variable assignments
    void initializePremadeParams()
    {
        STOPPING = MotionState.Stop;
        AGITATED = MotionState.Agitated;
        CASUAL = MotionState.Casual;
        STROLLING = MotionState.Stroll;
        FOLLOWING = MotionState.Follow;
        FOLLOWING_STRICT = MotionState.Follow_Strict;

        AVOID = ActionState.Avoid;
        TRY_AVOID = ActionState.TryAvoid;
        NORMAL = ActionState.Normal;
        TRACKING = ActionState.Track;
    }
    
    void initializeTrackingProperties()
    {
        isAvoiding = false;
        isTracking = false;
        destinationReached = false;
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (GetComponent<AnimalManualControl>().manualControlOn)
        {
            controlBypassed = true;
            return;
        } else
        {
            controlBypassed = false;
        }

        detectAndHandleObstacle();

        updateTurn();

        updateForward();

    }

    void log()
    {
        print("Action State " + actionState + " " + " Name: " + this.gameObject.name + " AT " + Time.time);
        print("Motion State " + motionState + " " + " Name: " + this.gameObject.name + " AT " + Time.time);
    }
    //Random positive/negative sign
    int randomSign()
    {
        int i = Random.Range(0, 2);

        return (i == 0) ? -1 : 1;
    }
    //Random 50% chance
    bool randomTryAvoid()
    {
        int i = Random.Range(0, 2);

        return (i == 0) ? true : false;
    }

    //NORMAL UPDATES
    void updateTurn()
    {
        //uses transform rotation to calculate turning instead of what was used earlier
        curYRot = animalTransform.rotation.eulerAngles.y;

        //calculate "derivative" via math
        float difference = curYRot - prevYRot;
        //Debug.Log(difference);

        //arbitrary "turn threshold" where not to display turn animation
        if(Math.Abs(difference) < 0.05)
        {
            turn = 0.95f*turn;
            if (Math.Abs(turn) < 0.1)
            {
                turn = 0;
            }
        } else if (curYRot > prevYRot) //this part makes sense because basically using numbers to see if turning
        {
            turn += 0.04f;
        } else if (curYRot < prevYRot)
        {
            turn -= 0.04f;
        }
        //clamps it, with help of forward so animal doesn't spaz out when standing still
        turn = Mathf.Clamp(turn, -(forward + 0.1f)/2, (forward + 0.1f)/2);
        setAnimatorTurnValue(turn);
        prevYRot = curYRot;
    }

    void updateForward()
    {
        if (onPAP && !animalIs(STOPPING))
            setMotionState(AGITATED);

        if (animalDoes(TRACKING))
        {
            //REACH DESTINATION
            if (destinationReached)
            {
                setActionState(NORMAL);
                setMotionState(STOPPING);
                //print("Destination Reached " + this.gameObject.name);

                if (destinationGameOject != null)
                    destinationGameOject = null;
            }
            else
            {
                forwardVector = 1f;
            }
        }

        if (animalIs(AGITATED))
            forwardVector = 1.8f;

        if (animalIs(STOPPING))
            forwardVector = 0;
        else
        {
            if (animalDoes(AVOID))
            {
                forwardVector = 0f;
            }
            else if (animalDoes(TRY_AVOID))
            {
                forwardVector = 0.5f;
            }
            else if (animalDoes(NORMAL))
            {
                forwardVector = 1.5f;
            }
        }

        if (animalIs(STOPPING))
        {
            if (forward != forwardVector)
            {
                float delta = 0;

                if (Math.Abs(forwardVector - forward) < FORWARD_STEP_ADJUSTMENT / 2)
                {
                    delta = (Math.Abs(forwardVector - forward));

                    delta = Mathf.Clamp(delta, FORWARD_SLIGHT_ADJUSTMENT, FORWARD_STEP_ADJUSTMENT);

                    //sign
                    delta *= (forwardVector - forward) / Math.Abs((forwardVector - forward));
                }
                else
                {
                    delta = (forwardVector - forward) / Math.Abs((forwardVector - forward)) * FORWARD_STEP_ADJUSTMENT;
                }

                forward += delta * Time.deltaTime;
            }

            if (approximately(forward, forwardVector, MAX_FLOAT_DIFFERENCE/10))
                forward = forwardVector;

            setAnimatorForwardValue(forwardVector);

            return;
        }
        

        if (forward != forwardVector)
        {
            float delta = 0;

            if (Math.Abs(forwardVector - forward) < FORWARD_STEP_ADJUSTMENT)
            {
                delta = (Math.Abs(forwardVector - forward));

                delta = Mathf.Clamp(delta, FORWARD_SLIGHT_ADJUSTMENT, FORWARD_STEP_ADJUSTMENT);

                //sign
                delta *= (forwardVector - forward) / Math.Abs((forwardVector - forward));
            } else
            {
                delta = (forwardVector - forward) / Math.Abs((forwardVector - forward)) * FORWARD_STEP_ADJUSTMENT;
            }

            forward += delta * Time.deltaTime;
        }

        forward = Mathf.Clamp(forward, 0, 2);

        setAnimatorForwardValue(forward);
    }

    void detectAndHandleObstacle()
    {
        if (Physics.Raycast(eyeLevel.transform.position,
                            eyeLevel.transform.forward, out RaycastHit hit,
                            MAX_VISUAL_DISTANCE)){


            if (isObstacle(hit))
            {
                // print(this.GetComponent<Animal>().Name + " DETECTED OBSTACLE " + hit.collider.tag);

                if (animalIs(FOLLOWING) || animalIs(FOLLOWING_STRICT))
                {
                    if (hit.distance <= MIN_ALERT_DISTANCE_FOLLOW)
                    {
                        setActionState(TRY_AVOID);
                    }

                    if (hit.distance <= MAX_ALERT_DISTANCE_FOLLOW)
                    {
                        setActionState(AVOID);
                    }

                }
                else if (animalIs(CASUAL) || animalIs(STROLLING))
                {

                    if (hit.distance <= MIN_ALERT_DISTANCE_CASUAL)
                    {
                        setActionState(AVOID);
                    }

                }
                else if (animalIs(AGITATED))
                {
                    if (hit.distance <= MIN_ALERT_DISTANCE_AGITATED)
                    {
                        setActionState(AVOID);
                    }
                }
            }

        } else
        {
            if (animalDoes(AVOID))
            {
                Invoke("resumeNormalActivities", 3);
            }
        }
    }

    private void resumeNormalActivities()
    {
        if (isTracking)
        {
            setActionState(TRACKING);
            turnVector = 0;
        }
        else
        {
            setActionState(NORMAL);
        }
    }

    bool isObstacle(RaycastHit hit)
    {
        return hit.collider.tag.Equals("Tree") ||
            hit.collider.tag.Equals("Animal");
    }

    //START TRACKING
    public void initializeTracking(Vector3 d)
    {
        destination = d;
        isTracking = true;
        destinationReached = false;

        setActionState(TRACKING);
        setMotionState(CASUAL);
    }

    //Helper methods
    void setActionState(ActionState state)
    {
        actionState = state;
    }

    void setMotionState(MotionState state)
    {
        motionState = state;
    }

    bool animalIs(MotionState state)
    {
        return state == motionState;
    }

    bool animalDoes(ActionState state)
    {
        return state == actionState;
    }

    private void setAnimatorForwardValue(float f)
    {
        animalAnimator.SetFloat("Forward", f);
    }

    private void setAnimatorTurnValue(float t)
    {
        animalAnimator.SetFloat("Turn", t);
    }

    private bool approximately(float x, float y, float t)
    {
        return (Mathf.Abs(x - y) <= t);
    }
    private float getXZAngle(Vector3 v1, Vector3 v2)
    {
        Vector2 v1_t = new Vector2(v1.x, v1.z);

        Vector2 v2_t = new Vector2(v2.x, v2.z);

        return -Vector2.SignedAngle(v1_t, v2_t);
    }

    public void move(Vector3 d)
    {

        if (isTracking)
        {
            if (animalIs(STOPPING))
            {
                initializeTracking(d);
            }
            setDestination(d);
            return;
        }
        else
        {
            initializeTracking(d);
            setDestination(d);
        }
    }

    public void move(GameObject d)
    {

        if (isTracking)
        {
            if (animalIs(STOPPING))
            {
                initializeTracking(d.transform.position);
            }
            setDestination(d);
            return;
        } else
        {
            initializeTracking(d.transform.position);
            setDestination(d);
        }
    }

    //THIS METHOD ISN'T EVEN CALLED, like EVER, BRUH - HAO QI 22/02/05
    public void move(float f, float t)
    {
        if (forward > f)
        {
            forward -= 0.6f * Time.deltaTime;
            if (forward < f)
                forward = f;
        } else if (forward < f)
        {
            forward += 0.5f * Time.deltaTime;
            if (forward > f)
                forward = f;
        }
        /* //IDK WHATS GOING ON HERE -HAO QI 22/02/05
        if (turnVectorManual != t)
        {
            turnProgress = 0;
            turnVectorManual = t;
        }

        if (turn != turnVectorManual)
        {
            turn = Mathf.Lerp(turn, turnVectorManual, turnProgress);
            if (turnProgress >= 1)
            {
                turnProgress = 1;
            } else
            {
                turnProgress += 0.25f * Time.deltaTime;
            }
        }
        */
        setAnimatorForwardValue(forward);
        setAnimatorTurnValue(turn); //set to "t" temporarily bc i was testing -HQ 22/02/05

    }

    public bool movementFinished()
    {
        return animalIs(STOPPING) && destinationReached;
    }

    public void stopMotion()
    {
        setMotionState(STOPPING);
        setActionState(NORMAL);

        isTracking = false;
        destinationReached = true;

        if (onPAP)
            setPAPOff();
    }
    //Only Use After tracking is initialized!
    public void setDestination(Vector3 d)
    {
        if (isTracking)
            destination = d;
    }
    //Only Use After tracking is initialized!
    public void setDestination(GameObject d)
    {
        if (isTracking)
            destinationGameOject = d;
    }

    public void setPAPOn()
    {
        onPAP = true;
    }

    public void setPAPOff()
    {
        onPAP = false;
    }

    public void AddPredatorToListForPrey(GameObject predator, GameObject prey)
    {
        prey.GetComponent<AvoidPredator>().AssignPredator(predator);
    }

    public void RemovePredatorToListForPrey(GameObject predator, GameObject prey)
    {
        prey.GetComponent<AvoidPredator>().RemovePredator(predator);
    }
}
