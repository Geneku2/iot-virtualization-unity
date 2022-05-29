using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Refactored from LionUserController
/// </summary>
public class Lion : Animal
{
    LionCharacter lion;

    void Start()
    {
        lion = GetComponent<LionCharacter>();
    }

    // TODO: understand how Move works
    public override void Move(float distance, float time)
    {
        float speed = distance / time;
        float anim_speed = FpsBasedSpeed(speed);
        lion.Move(anim_speed);
    }
    public override void Eat()
    {
        lion.Eat();
    }
 
    public override void Death()
    {
        lion.Death();
    }

    // TODO: figure out why this is needed
    // Taken from LionUserController
    private float FpsBasedSpeed(float speed)
    {
        float fps = 1f / Time.deltaTime;
        return speed / (float)(4.86461 / 30 * fps);
    }
}
