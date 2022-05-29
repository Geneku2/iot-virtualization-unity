using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Timeline;

public class AnimalControlTest : MonoBehaviour
{
    public string[] commands;

    private ElephantCharacter eC;

    //To be implemented
    private ZebraCharacter zC;
    private LionCharacter lC;

    //Commands that requrie params
    //Format:   Turnings: turn left/right DEGREE(float) FORWARD_SPEED(float)
    //          Move: move DISTANCE(float) TIME(float)
    //e.g.      turn left 90 1.5 -- Turn left for 90 degrees while moving forward with a speed of 1.5/sec
    //          move 5 5         -- Move forward 5 meters in 5 seconds
    private const string TURN_LEFT = "turn left ";
    private const string TURN_RIGHT = "turn right ";
    private const string MOVE = "move ";
    private const string WALK = "walk ";
    private const string RUN = "run ";
    private const string EAT = "eat ";
    //Commands that only requrie time as params
    //Format: EAT/SIT TIME(float)
    //e.g.      eat 2           -- Eat for 2 seconds
    private const string sit = "sit ";
    private const string death = "death ";
    private const string eat = "eat ";

    //Utility indices
    private AnimalState animalState;
    private int commandIndex;
    private float timeRef;

    enum AnimalIndex
    {
        elephant,
        zebra,
        lion
    }

    enum AnimalAction
    {
        move,
        turn_left,
        turn_right,
        eat,
        death,
        sit
    }

    enum AnimalState
    {
        DoingAction,
        Transition,
        Finished
    }

    // Start is called before the first frame update
    void Start()
    {
        //Temporarily only for elephants
        eC = this.GetComponent<ElephantCharacter>();

        commandIndex = 0;

        animalState = AnimalState.Transition;

        timeRef = 0;
    }

    // Update is called once per frame
    void Update()
    {
        eC.Test();

        return;

        if (animalState == AnimalState.Transition)
        {
            if (commandIndex == commands.Length || commands[commandIndex].Equals(""))
            {
                eC.Move(0);
                animalState = AnimalState.Finished;
                return;
            }
            print("Executing command: " + commands[commandIndex]);
            interpretAction(commands[commandIndex]);
            commandIndex++;
        } else if (animalState == AnimalState.DoingAction)
        {
            return;
        } else if (animalState == AnimalState.Finished)
        {
            return;
        }

    }

    private void interpretAction(string s)
    {
        if (s.Contains(MOVE))
        {
            string parameters = s.Substring(MOVE.Length);
            string[] sParams = parameters.Split(' ');

            move(float.Parse(sParams[0]), float.Parse(sParams[1]), 1);
        }

        if (s.Contains(RUN))
        {
            string parameters = s.Substring(RUN.Length);
            string[] sParams = parameters.Split(' ');

            move(float.Parse(sParams[0]), float.Parse(sParams[1]), 1);
        }

        if (s.Contains(WALK))
        {
            string parameters = s.Substring(WALK.Length);
            string[] sParams = parameters.Split(' ');

            move(float.Parse(sParams[0]), float.Parse(sParams[1]), 1);
        }

        if (s.Contains(TURN_LEFT))
        {
            string parameters = s.Substring(TURN_LEFT.Length);

            string[] sParams = parameters.Split(' ');

            turnWhileMoving(-1, float.Parse(sParams[0]), float.Parse(sParams[1]));
        }

        if (s.Contains(TURN_RIGHT))
        {
            string parameters = s.Substring(TURN_RIGHT.Length);

            string[] sParams = parameters.Split(' ');

            turnWhileMoving(1, float.Parse(sParams[0]), float.Parse(sParams[1]));
        }

        if (s.Contains(EAT))
        {
            string parameters = s.Substring(EAT.Length);

            eatFor(float.Parse(parameters));
        }
    }

    private void eatFor(float t)
    {
        animalState = AnimalState.DoingAction;
        StartCoroutine(EatForTime(t));
    }
    
    //TODO: Find a better way to do this
    //Sometimes this script causes animation to not properly display, or skip animation to the next state
    IEnumerator EatForTime(float t)
    {
        float refTime = 0.05f;
        float timeElapsed = -refTime;

        for(; ; )
        {
            timeElapsed += refTime;

            if (timeElapsed >= t)
            {
                if (eC.elephantAnimator.GetCurrentAnimatorStateInfo(0).IsName("Eat"))
                {
                    yield return null;
                }
                if (!eC.elephantAnimator.GetCurrentAnimatorStateInfo(0).IsName("Eat"))
                {
                    eC.elephantAnimator.ResetTrigger("Eat");
                    animalState = AnimalState.Transition;
                    yield break;
                }
                
                yield return null;
            }

            eC.Eat();
            //Update every refTime sec
            yield return new WaitForSeconds(refTime);
        }
    }

    private void turnWhileMoving(float turnDirection, float degree, float speed)
    {
        //Avoid using degrees more than 360
        animalState = AnimalState.DoingAction;
        degree %= 360;
        eC.TurnAndMove(turnDirection, degree, speed);
        StartCoroutine(Turning());
    }

    IEnumerator Turning()
    {
        for (; ; )
        {

            if (eC.turnComplete)
            {
                print("Stopped Turning");
                animalState = AnimalState.Transition;
                yield break;
            }

            yield return new WaitForSeconds(0.01f);
        }
    }

    //direction: +1 or -1 representing forward/backward -- currently unimplemented
    private void move(float distance, float time, float direction)
    {
        if (distance == 0)
        {
            eC.Move(0);
            animalState = AnimalState.DoingAction;
            StartCoroutine(stopWalking(time));
            return;
        }

        animalState = AnimalState.DoingAction;
        eC.Move(distance / time * direction);
        StartCoroutine(stopWalking(time));
    }

    IEnumerator loggins()
    {
        for (; ; )
        {
            GameObject testObject = GameObject.Find("TestObject");
            print("GameObject transform rot = " + testObject.gameObject.transform.rotation);
            print("GameObject transform pos = " + testObject.gameObject.transform.position);

            //Update every 0.2 second
            yield return 0.02f;
        }
    }

    IEnumerator stopWalking(float timeMax)
    {
        yield return new WaitForSeconds(timeMax);
        StopCoroutine(loggins());
        animalState = AnimalState.Transition;
    }

}

