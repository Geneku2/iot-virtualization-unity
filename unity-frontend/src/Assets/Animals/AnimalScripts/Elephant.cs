using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Refactored from ElephantUserController
/// </summary>
public class Elephant : Animal
{
    ElephantCharacter elephant;

    void Start()
    {
        elephant = GetComponent<ElephantCharacter>();
    }

    // TODO: understand how Move works
    public override void Move(float distance, float time)
    {
        /*
        float speed = distance / time;
        float anim_speed = FpsBasedSpeed(speed);
        elephant.Move(anim_speed);
        */
    }

    public override void Eat()
    {
        elephant.Eat();
    }

    public override void Death()
    {
        elephant.Death();
    }

    // TODO: figure out why this is needed
    // Taken from ElephantUserController
    private float FpsBasedSpeed(float speed)
    {
        float fps = 1f / Time.deltaTime;
        return speed / (float)(4.76837 / 30 * fps);
    }
}
