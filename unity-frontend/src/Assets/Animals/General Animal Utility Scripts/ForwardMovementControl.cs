using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ForwardMovementControl : MonoBehaviour
{
    int controlIndex;
    int TIME_CONTROL = 1;
    int DISTANCE_CONTROL = 0;
    int HYBRID_VECTOR_CONTROL = 2;

    float moveSpeed;
    float timeToMove;
    float cachedSpeed;
    const float maximumDeltaSpeed = 0.6f;
    const float MAXIMUM_SPEED = 2;
    const float MINIMUM_SPEED = 0.2f;

    const string IDLE = "ToIdle";

    float destinatedDistance;
    Vector3 vectorDestination;
    TurnMovementControl turnMotion;

    IEnumerator speedSmoothCoroutine;

    Vector3 posRef;
    float maximumFloatDifference = 0.05f;

    GameObject animal;
    Animator animalAnimator;

    bool finished;

    //Keep the terminal speed after the animation is finished
    bool keepTerminalSpeed;
    bool readyToMove;
    bool coroutinePaused;

    //ControlIndex: 0(Default) for distance, 1 for time
    public void initialize(GameObject animalGO, Animator animator)
    {
        finished = true;
        readyToMove = false;
        moveSpeed = 0;
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

        if (readyToMove == false)
        {
            Debug.LogError("MOVEMENT CONTROL NOT READY!");

            return;
        }

        finished = false;

        if (controlIndex == TIME_CONTROL)
        {
            StartCoroutine(moveByTime(0, timeToMove));
        }

        if (controlIndex == DISTANCE_CONTROL)
        {
            posRef = animal.transform.position;
            StartCoroutine(moveByDistance(posRef, destinatedDistance));
        }

        if (controlIndex == HYBRID_VECTOR_CONTROL)
        {
            posRef = animal.transform.position;
            StartCoroutine(moveByVector(posRef, vectorDestination, turnMotion));
        }

        if (cachedSpeed != moveSpeed)
        {
            speedSmoothCoroutine = smoothForwardSpeed(Time.time);

            StartCoroutine(speedSmoothCoroutine);
        }

        readyToMove = false;
    }

    public void resetAllCoroutines()
    {
        StopAllCoroutines();
    }

    public void stop()
    {
        cachedSpeed = 0;
        moveSpeed = 0;
        setAnimatorTrigger(IDLE);
        StartCoroutine(stopMovement());
    }

    IEnumerator stopMovement()
    {
        for (; ; )
        {
            if (!animalAnimator.GetCurrentAnimatorStateInfo(0).IsName("Motions"))
            {
                setAnimatorForwardValue(0);

                yield break;
            }

            yield return null;
        }
    }

    //DO NOT USE THIS EASILY
    public void smoothStop()
    {
        resetMovement();
        cachedSpeed = 0;
        speedSmoothCoroutine = smoothForwardSpeed(0);
        StartCoroutine(speedSmoothCoroutine);
    }

    public void initHybridVectorControl(Vector3 destination, float speed, bool kTS, TurnMovementControl tM)
    {
        controlIndex = HYBRID_VECTOR_CONTROL;

        moveSpeed = 0;
        cachedSpeed = speed;
        Mathf.Clamp(cachedSpeed, MINIMUM_SPEED, MAXIMUM_SPEED);

        vectorDestination = destination;
        turnMotion = tM;

        keepTerminalSpeed = kTS;
        setAnimatorForwardValue(0);
        readyToMove = true;
    }

    public void initMovementScheme(MovementControlScheme scheme)
    {
        if (scheme.forwardControlIndex == TIME_CONTROL)
        {
            setAnimatorForwardValue(0);
            initTimeControl(scheme.distanceOrTime, scheme.speed, scheme.keepTerminalSpeed);
            start();
            return;
        }

        if (scheme.forwardControlIndex == DISTANCE_CONTROL)
        {
            setAnimatorForwardValue(0);
            initDistanceControl(scheme.distanceOrTime, scheme.speed, scheme.keepTerminalSpeed);
            start();
            return;
        }

        if (scheme.forwardControlIndex == HYBRID_VECTOR_CONTROL)
        {
            setAnimatorForwardValue(0);
            initHybridVectorControl(scheme.vectorRef, scheme.speed, scheme.keepTerminalSpeed, scheme.getTurnMotionControl());
            start();
            return;
        }

        Debug.LogError("Invalid Control Scheme!");
    }

    public void initTimeControl(float time, float speed, bool kTS)
    {
        controlIndex = TIME_CONTROL;

        moveSpeed = 0;
        cachedSpeed = speed;
        Mathf.Clamp(cachedSpeed, MINIMUM_SPEED, MAXIMUM_SPEED);
        timeToMove = time;
        readyToMove = true;
        keepTerminalSpeed = kTS;

        setAnimatorForwardValue(0);
    }

    public void initDistanceControl(float distance, float speed, bool kTS)
    {
        controlIndex = DISTANCE_CONTROL;

        moveSpeed = 0;
        destinatedDistance = distance;
        cachedSpeed = speed;
        Mathf.Clamp(cachedSpeed, MINIMUM_SPEED, MAXIMUM_SPEED);
        readyToMove = true;
        keepTerminalSpeed = kTS;

        setAnimatorForwardValue(0);
    }

    public bool isFinished()
    {
        return finished;
    }

    public void pauseAllCoroutines()
    {
        coroutinePaused = true;
    }

    public void resumeAllCoroutines()
    {
        coroutinePaused = false;
    }

    public void resetMovement()
    {
        finished = true;
        readyToMove = false;
        if (speedSmoothCoroutine != null)
        {
            StopCoroutine(speedSmoothCoroutine);
        }
    }

    IEnumerator smoothForwardSpeed(float timeRef)
    {
        for (; ; )
        {
            if (coroutinePaused)
            {
                yield return null;
            }

            if (moveSpeed != cachedSpeed)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, cachedSpeed, (Time.time - timeRef) * maximumDeltaSpeed * Time.deltaTime);
                if (cachedSpeed != 0)
                {
                    moveSpeed = Mathf.Clamp(moveSpeed, MINIMUM_SPEED, MAXIMUM_SPEED);
                } else
                {
                    moveSpeed = Mathf.Clamp(moveSpeed, 0, MAXIMUM_SPEED);
                }
                setAnimatorForwardValue(moveSpeed);

                yield return null;
            }
            else
            {
                yield break;
            }

        }
    }

    IEnumerator smoothTerminalSpeed(Vector3 positionRef, float distance)
    {
        float distanceRemaining = distance - Vector3.Distance(animal.transform.position, positionRef);

        float maxSpeed = moveSpeed;


        for (; ; )
        {
            if (coroutinePaused)
            {
                yield return null;
            }

            if (finished)
            {
                print("TERMINATED!");
                resetMovement();
                stop();
                yield break;
            }

            moveSpeed = Mathf.Lerp(maxSpeed, MINIMUM_SPEED, (distanceRemaining - (distance - Vector3.Distance(animal.transform.position, positionRef))) / distanceRemaining);

            moveSpeed = Mathf.Clamp(moveSpeed, MINIMUM_SPEED, MAXIMUM_SPEED);

            setAnimatorForwardValue(moveSpeed);

            yield return null;
        }
    }

    IEnumerator smoothTerminalSpeedByVector(Vector3 positionRef, Vector3 destination)
    {
        float distanceRemaining = getXZDistance(animal.transform.position, destination);

        float maxSpeed = moveSpeed;

        for (; ; )
        {
            if (coroutinePaused)
            {
                yield return null;
            }

            if (finished)
            {
                resetMovement();
                stop();
                yield break;
            }
            moveSpeed = Mathf.Lerp(maxSpeed, MINIMUM_SPEED, (distanceRemaining - Vector3.Distance(animal.transform.position, destination)) / distanceRemaining);

            moveSpeed = Mathf.Clamp(moveSpeed, MINIMUM_SPEED, MAXIMUM_SPEED);

            setAnimatorForwardValue(moveSpeed);

            yield return null;
        }
    }

    IEnumerator moveByVector(Vector3 positionRef, Vector3 destination, TurnMovementControl tM)
    {
        float distanceRef = getXZDistance(animal.transform.position, destination);

        for(; ; )
        {
            if (coroutinePaused)
            {
                yield return null;
            }

            if (!keepTerminalSpeed
                && ((distanceRef - getXZDistance(animal.transform.position, positionRef) <= 10
                && getXZDistance(animal.transform.position, positionRef) / distanceRef >= 0.75f)))
            {
                if (speedSmoothCoroutine != null)
                {
                    StopCoroutine(speedSmoothCoroutine);
                }

                /*
                if (moveSpeed < MINIMUM_SPEED)
                {
                    moveSpeed = MINIMUM_SPEED;
                    setAnimatorForwardValue(MINIMUM_SPEED);
                }

                cachedSpeed = MINIMUM_SPEED;
                speedSmoothCoroutine = smoothForwardSpeed(Time.time);
                StartCoroutine(speedSmoothCoroutine);
                */

                speedSmoothCoroutine = smoothTerminalSpeedByVector(positionRef, destination);
                StartCoroutine(speedSmoothCoroutine);
                //REF SET TO TRUE
                keepTerminalSpeed = true;
            }

            if (getXZDistance(animal.transform.position, destination) <= 3f)
            {
                tM.endVectorTurn();
                resetMovement();
                stop();
                yield break;
            }

            yield return null;
        }

    }

    IEnumerator moveByTime(float timeRef, float time)
    {
        for (; ; )
        {
            if (coroutinePaused)
            {
                yield return null;
            }

            if (!keepTerminalSpeed && (time - timeRef) <= 2)
            {
                if (speedSmoothCoroutine != null)
                {
                    StopCoroutine(speedSmoothCoroutine); 
                }

                if (moveSpeed < MINIMUM_SPEED)
                {
                    moveSpeed = MINIMUM_SPEED;
                    setAnimatorForwardValue(MINIMUM_SPEED);
                }

                cachedSpeed = MINIMUM_SPEED;
                speedSmoothCoroutine = smoothForwardSpeed(Time.time);
                StartCoroutine(speedSmoothCoroutine);

                //REFERENCE SET TO TRUE
                keepTerminalSpeed = true;

                print("ALMOST COMPLETE");
            }

            if (approximately(timeRef, time, maximumFloatDifference))
            {
                resetMovement();
                stop();
                yield break;
            }

            timeRef += Time.deltaTime;

            yield return null;
        }

    }

    IEnumerator moveByDistance(Vector3 positionRef, float distance)
    {
        for (; ; )
        {
            if (coroutinePaused)
            {
                yield return null;
            }

            if (!keepTerminalSpeed
                && ((distance - Vector3.Distance(animal.transform.position, positionRef) <= 10
                && Vector3.Distance(animal.transform.position, positionRef) / distance >= 0.75f)))
            {
                if (speedSmoothCoroutine != null)
                {
                    StopCoroutine(speedSmoothCoroutine);
                }

                /*
                if (moveSpeed < MINIMUM_SPEED)
                {
                    moveSpeed = MINIMUM_SPEED;
                    setAnimatorForwardValue(MINIMUM_SPEED);
                }

                cachedSpeed = MINIMUM_SPEED;
                speedSmoothCoroutine = smoothForwardSpeed(Time.time);
                StartCoroutine(speedSmoothCoroutine);
                */

                speedSmoothCoroutine = smoothTerminalSpeed(positionRef, distance);
                StartCoroutine(speedSmoothCoroutine);
                //REF SET TO TRUE
                keepTerminalSpeed = true;
            }

            if (Vector3.Distance(animal.transform.position, positionRef) >= distance)
            {
                resetMovement();
                stop();
                yield break;
            }

            yield return null;
        }
    }

    private void setAnimatorForwardValue(float f)
    {
        animalAnimator.SetFloat("Forward", f);
    }

    private void setAnimatorTrigger(string s)
    {
        animalAnimator.SetTrigger(s);
    }

    private bool approximately(float x, float y, float t)
    {
        return (Mathf.Abs(x - y) <= t);
    }

    private float getXZDistance(Vector3 v1, Vector3 v2)
    {
        Vector2 v1_t = new Vector2(v1.x, v1.z);

        Vector2 v2_t = new Vector2(v2.x, v2.z);

        return Vector2.Distance(v1_t, v2_t);
    }

    public void loggins()
    {
        print("Current Control Scheme: " + controlIndex + " Has Motion Finished: " + finished
            + " Ready To Move: " + readyToMove + " Coroutine Pasued: " + coroutinePaused + " Keep Terminal Speed: " + keepTerminalSpeed);
        print("Current Forward Speed: " + moveSpeed);
    }
}
