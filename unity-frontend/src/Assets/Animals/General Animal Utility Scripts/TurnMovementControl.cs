using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Threading;
using System.Xml.Serialization;
using TMPro.Examples;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class TurnMovementControl : MonoBehaviour
{
    int controlIndex;
    int VECTOR_CONTROL = 2;
    int TIME_CONTROL = 1;
    int DEGREE_CONTROL = 0;

    float turnMagnitude;
    float timeToTurn;

    //CHANGE NONE OF THESE! I TESTED FOR THE PERFECT PARAMETER FOR LIKE 5 HOURS!
    const float MAXIMUM_TURN = 1;
    float MAXIMUM_TURN_COMPENSATION;
    float MINIMUM_TURN_COMPENSATION;
    float MINIMUM_TURN_ADJUSTMENT = 0.001f;
    float MINIMUM_ANGLE_FOR_FULL_SPEED = 90;
    float MINIMUM_TIME_FOR_FULL_SPEED = 6;
    const float MAXIMUM_TURN_ANGLE = 180;
    const float MINIMUM_TURN = 0;
    const float MINIMUM_TURN_IN_MOTION = 0.1f;
    const float MINIMUM_DEGREE_TOLERANCE = 0.5f;

    const float LINEUP_SMOOTH_T = 0.2f;
    const float TURN_SMOOTH_T = 1f;
    const float LINEUP_DEG = 2f;
    const float TURN_MAX = 1.0f;
    
    float turnSpeed = 0f;

    float degreeToTurn;
    Vector3 vectorDestination;

    Vector3 posRef;
    float maximumFloatDifference = 0.01f;

    GameObject animal;
    Animator animalAnimator;

    bool finished;
    bool readyToTurn;
    bool coroutinePaused;
    bool vectorTurnEnd;
    public void initialize(GameObject animalGO, Animator animator)
    {
        finished = true;
        readyToTurn = false;
        turnMagnitude = 0;
        animal = animalGO;
        animalAnimator = animator;
        coroutinePaused = false;
    }

    public void start()
    {
        if (coroutinePaused)
        {
            Debug.LogError("COROUTINE PAUSED!");
            return;
        }

        if (!finished)
        {
            Debug.LogError("PREVIOUS MOVEMENT IN PROGRESS!");

            return;
        }

        if (readyToTurn == false)
        {
            Debug.LogError("TURN CONTROL NOT READY!");

            return;
        }

        finished = false;

        if (controlIndex == TIME_CONTROL)
        {
            if (timeToTurn == 0)
            {
                resetMovement();
                stop();
                return;
            }

            StartCoroutine(turnByTime(0, timeToTurn));
        }

        if (controlIndex == DEGREE_CONTROL)
        {
            if(degreeToTurn == 0)
            {
                resetMovement();
                stop();
                return;
            }

            posRef = animal.transform.forward;
            StartCoroutine(turnByDegree(posRef, degreeToTurn));
        }

        if (controlIndex == VECTOR_CONTROL)
        {
            if (vectorTurnEnd)
            {
                Debug.LogError("VECTOR TURN INITIALIZATION ERROR!");
            }
            StartCoroutine(turnByVectorReference(vectorDestination));
        }

    }

    public void initMovementScheme(MovementControlScheme scheme)
    {
        if (scheme.forwardControlIndex == TIME_CONTROL)
        {
            setAnimatorTurnValue(0);
            initTimeControl(scheme.turnAngleOrTime);
            start();
            return;
        }

        if (scheme.forwardControlIndex == DEGREE_CONTROL)
        {
            setAnimatorTurnValue(0);
            initDegreeControl(scheme.turnAngleOrTime);
            start();
            return;
        }

        if (scheme.forwardControlIndex == VECTOR_CONTROL)
        {
            scheme.setTurnMovementControl(this);
            setAnimatorTurnValue(0);
            initVectorControl(scheme.vectorRef);
            start();
            return;
        }

        Debug.LogError("Invalid Control Scheme!");
    }

    public void initTimeControl(float time)
    {
        controlIndex = TIME_CONTROL;

        MINIMUM_TURN_COMPENSATION = MINIMUM_TURN_IN_MOTION * time / MINIMUM_TIME_FOR_FULL_SPEED;
        MINIMUM_TURN_COMPENSATION = Mathf.Clamp(MINIMUM_TURN_COMPENSATION, MINIMUM_TURN, MINIMUM_TURN_IN_MOTION);
        MAXIMUM_TURN_COMPENSATION = MAXIMUM_TURN * time / MINIMUM_TIME_FOR_FULL_SPEED;
        MAXIMUM_TURN_COMPENSATION = Mathf.Clamp(MAXIMUM_TURN_COMPENSATION, MINIMUM_TURN_IN_MOTION, MAXIMUM_TURN);

        if (time < 0)
        {
            time = 0;
        }

        timeToTurn = time;
        readyToTurn = true;
    }

    public void initVectorControl(Vector3 destination)
    {
        controlIndex = VECTOR_CONTROL;

        float degreeRef = getXZAngle(animal.transform.forward, destination - animal.transform.position);
        degreeRef = Mathf.Clamp(degreeRef, -MAXIMUM_TURN_ANGLE, MAXIMUM_TURN_ANGLE);

        MINIMUM_TURN_COMPENSATION = MINIMUM_TURN_IN_MOTION * Mathf.Abs(degreeRef) / MINIMUM_ANGLE_FOR_FULL_SPEED;
        MINIMUM_TURN_COMPENSATION = Mathf.Clamp(MINIMUM_TURN_COMPENSATION, MINIMUM_TURN, MINIMUM_TURN_IN_MOTION);
        MAXIMUM_TURN_COMPENSATION = MAXIMUM_TURN * Mathf.Abs(degreeRef) / MINIMUM_ANGLE_FOR_FULL_SPEED;
        MAXIMUM_TURN_COMPENSATION = Mathf.Clamp(MAXIMUM_TURN_COMPENSATION, MINIMUM_TURN_IN_MOTION, MAXIMUM_TURN);

        vectorDestination = destination;

        vectorTurnEnd = false;
        readyToTurn = true;
    }

    public void initDegreeControl(float degree)
    {
        controlIndex = DEGREE_CONTROL;

        degree = Mathf.Clamp(degree, -MAXIMUM_TURN_ANGLE, MAXIMUM_TURN_ANGLE);
        degreeToTurn = degree;

        MINIMUM_TURN_COMPENSATION = MINIMUM_TURN_IN_MOTION * Mathf.Abs(degree) / MINIMUM_ANGLE_FOR_FULL_SPEED;
        MINIMUM_TURN_COMPENSATION = Mathf.Clamp(MINIMUM_TURN_COMPENSATION, MINIMUM_TURN, MINIMUM_TURN_IN_MOTION);
        MAXIMUM_TURN_COMPENSATION = MAXIMUM_TURN * Mathf.Abs(degree) / MINIMUM_ANGLE_FOR_FULL_SPEED;
        MAXIMUM_TURN_COMPENSATION = Mathf.Clamp(MAXIMUM_TURN_COMPENSATION, MINIMUM_TURN_IN_MOTION, MAXIMUM_TURN);

        readyToTurn = true;
    }

    IEnumerator turnByVectorReference(Vector3 destination)
    {
        bool linedUp = false;
        Vector3 destinationDireciton = destination - animal.transform.position;
        float degreeRef = getXZAngle(destinationDireciton, animal.transform.forward);

        float dir = degreeRef / Mathf.Abs(degreeRef);

        if (vectorTurnEnd)
        {
            Debug.LogError("VECTOR TURN INITIALIZATION ERROR!");
        }

        for(; ; )
        {
            if (coroutinePaused)
            {
                yield return null;
            }

            if (vectorTurnEnd)
            {
                resetMovement();
                stop();
                yield break;
            }

            destinationDireciton = destination - animal.transform.position;

            float degreeRemained = getXZAngle(destinationDireciton, animal.transform.forward);
            

            //print("Degree diff: " + degreeRemained);
            linedUp = Mathf.Abs(degreeRemained) < LINEUP_DEG;
            //print("Lined up: " + linedUp);
            if (linedUp)
                turnMagnitude = Mathf.SmoothDamp(turnMagnitude, 0f, ref turnSpeed, LINEUP_SMOOTH_T);
            else
                turnMagnitude = Mathf.SmoothDamp(turnMagnitude, degreeRemained > 0 ? TURN_MAX : -TURN_MAX, ref turnSpeed, TURN_SMOOTH_T);
            
            //print("Turn Magnitude: " + turnMagnitude);
            setAnimatorTurnValue(turnMagnitude);
            yield return null;
            //yield return new WaitForSeconds(1f);

            //if (linedUp)
            //{
            //    print("LINED UP AT degree difference: " + degreeRemained);

            //    if (Mathf.Abs(degreeRemained) >= 45)
            //    {
            //        linedUp = false;
            //        degreeRef = getXZAngle(animal.transform.forward, destination - animal.transform.position);
            //        dir = degreeRef / Mathf.Abs(degreeRef);
            //    }

            //    if (degreeRemained > maximumFloatDifference)
            //    {
            //        turnMagnitude = +MINIMUM_TURN_ADJUSTMENT;
            //    } else if (degreeRemained < -maximumFloatDifference)
            //    {
            //        turnMagnitude = -MINIMUM_TURN_ADJUSTMENT;
            //    } else
            //    {
            //        turnMagnitude = 0;
            //    }

            //    setAnimatorTurnValue(turnMagnitude);

            //    print("Turning magnitude: " + turnMagnitude);

            //} else
            //{
            //    linedUp = approximately(degreeRemained, 0, MINIMUM_DEGREE_TOLERANCE);
            //    print("Lined up set: " + linedUp);
            //    if (linedUp)
            //    {
            //        yield return null;
            //    }

            //    if (Mathf.Abs(degreeRef) < Mathf.Abs(degreeRemained))
            //    {
            //        degreeRef = degreeRemained;
            //        dir = degreeRef / Mathf.Abs(degreeRef);
            //    }

            //    turnMagnitude = Mathf.Sin(Mathf.PI * degreeRemained / degreeRef);

            //    if (degreeRemained / degreeRef <= 0.1f)
            //    {
            //        turnMagnitude = Mathf.Clamp(turnMagnitude, maximumFloatDifference, MAXIMUM_TURN_COMPENSATION);
            //    }
            //    else
            //    {
            //        turnMagnitude = Mathf.Clamp(turnMagnitude, MINIMUM_TURN_COMPENSATION, MAXIMUM_TURN_COMPENSATION);
            //    }

            //    turnMagnitude *= dir;
            //    print("Turning magnitude: " + turnMagnitude);
            //    setAnimatorTurnValue(turnMagnitude);
            //}

            //yield return null;
        }
    }

    public void endVectorTurn()
    {
        vectorTurnEnd = true;
    }

    IEnumerator turnByDegree(Vector3 positionRef, float degree)
    {
        turnMagnitude = 0;

        float dir = degree / Mathf.Abs(degree);

        degree = degree * dir;

        for(; ; )
        {
            if (coroutinePaused)
            {
                stop();

                yield return null;
            }

            float degreeTurned = Vector3.Angle(positionRef, animal.transform.forward);

            if(degreeTurned >= degree)
            {
                resetMovement();
                stop();
                yield break;
            }

            turnMagnitude = Mathf.Sin(Mathf.PI * degreeTurned / degree);

            if (degreeTurned/degree >= 0.9f)
            {
                turnMagnitude = Mathf.Clamp(turnMagnitude, maximumFloatDifference, MAXIMUM_TURN_COMPENSATION);
            } else
            {
                turnMagnitude = Mathf.Clamp(turnMagnitude, MINIMUM_TURN_COMPENSATION, MAXIMUM_TURN_COMPENSATION);
            }

            turnMagnitude *= dir;

            setAnimatorTurnValue(turnMagnitude);

            yield return null;
        }
    }

    IEnumerator turnByTime(float timeRef, float time)
    {

        for(; ; )
        {
            if (coroutinePaused)
            {
                stop();
                yield return null;
            }

            float timeElapsed = timeRef;

            if (timeElapsed > time)
            {
                resetMovement();
                stop();
                yield break;
            }

            turnMagnitude = Mathf.Sin(Mathf.PI * timeElapsed / time);

            if (timeElapsed / time >= 0.9f)
            {
                turnMagnitude = Mathf.Clamp(turnMagnitude, maximumFloatDifference, MAXIMUM_TURN_COMPENSATION);
            }
            else
            {
                turnMagnitude = Mathf.Clamp(turnMagnitude, MINIMUM_TURN_COMPENSATION, MAXIMUM_TURN_COMPENSATION);
            }

            setAnimatorTurnValue(turnMagnitude);

            timeRef += Time.deltaTime;

            yield return null;

        }
    }

    public void resetMovement()
    {
        finished = true;
        readyToTurn = false;
    }

    public void stop()
    {
        turnMagnitude = 0;
        setAnimatorTurnValue(0);
    }

    private float getXZAngle(Vector3 v1, Vector3 v2)
    {
        Vector2 v1_t = new Vector2(v1.x, v1.z);

        Vector2 v2_t = new Vector2(v2.x, v2.z);

        return Vector2.SignedAngle(v1_t, v2_t);
    }

    private bool approximately(float x, float y, float t)
    {
        return (Mathf.Abs(x - y) <= t);
    }

    private void setAnimatorTurnValue(float f)
    {
        animalAnimator.SetFloat("Turn", f);
    }

    public bool isFinished()
    {
        return finished;
    }
}
