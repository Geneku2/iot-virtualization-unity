using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Refactored from ZebraUserController
/// </summary>
public class Zebra : Animal
{
    private ZebraCharacter zebra;

    void Start()
    {
        zebra = GetComponent<ZebraCharacter>();
    }

    // TODO: understand how Move works
    public override void Move(float distance, float time)
    {
        float speed = distance / time;
        float anim_speed = FpsBasedSpeed(speed);
        zebra.Move(anim_speed);
    }

    public override void Eat()
    {
        zebra.EatStart();
    }

    public override void Death()
    {
        zebra.Death();
    }

    // TODO: figure out why this is needed
    // Taken from ZebraUserController
    private float FpsBasedSpeed(float speed)
    {
        float fps = 1f / Time.deltaTime;
        return speed / (float)(4.936472 / 30 * fps);
    }
}
