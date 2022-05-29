using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementControlScheme
{
    // Start is called before the first frame update
    public int forwardControlIndex;
    public int turnContorlIndex;
    public float distanceOrTime;
    public float turnAngleOrTime;
    public float speed;
    public bool keepTerminalSpeed;
    public Vector3 vectorRef;

    private TurnMovementControl tM;

    public MovementControlScheme(int forwardIndex, int turnIndex, float dOT, float aOT, float v, bool kTS, Vector3 vector)
    {
        forwardControlIndex = forwardIndex;
        turnContorlIndex = turnIndex;
        distanceOrTime = dOT;
        turnAngleOrTime = aOT;
        speed = v;
        keepTerminalSpeed = kTS;
        vectorRef = vector;
    }

    public TurnMovementControl getTurnMotionControl()
    {
        return tM;
    }

    public void setTurnMovementControl(TurnMovementControl t)
    {
        tM = t;
    }
}
