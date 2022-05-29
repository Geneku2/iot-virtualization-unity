using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndicationIcon : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject signGameobject;
    public GameObject animal;
    public GameObject host;

    private RealtimeMovementControl.ActionState actionstate;
    private Animator animator;

    private float scale = 1.2f;
    private bool isVisible;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetStateDiscover();
        isVisible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IndicationSystem.indicationSystemSwitch)
        {
            Destroy(gameObject);
            return;
        }

        float distance = Vector3.Distance(host.transform.position, animal.transform.position);
        SetPos(animal.GetComponent<AnimalManualControl>().camera.GetComponent<Camera>()
            .WorldToScreenPoint(host.transform.position));

        if (distance >= 10 && distance <= 20)
        {
            scale = 1.2f + (1.8f-1.2f)*((20 - distance) / 10);
        } else if (distance < 10)
        {
            scale = 1.8f;
        } else
        {
            scale = 1.2f;
        }

        GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);

        // Eventually we want:
        /*
        if (hostAnimal.GetComponent<RealtimeMovementControl>().actionState
            == RealtimeMovementControl.ActionState.TryAvoid &&
            actionstate != RealtimeMovementControl.ActionState.TryAvoid)
        {
            SetStateWarn();
        } else if (hostAnimal.GetComponent<RealtimeMovementControl>().actionState
            == RealtimeMovementControl.ActionState.Avoid &&
            actionstate != RealtimeMovementControl.ActionState.Avoid)
        {
            SetStateAvoid();
        } else
        {
            if (hostAnimal.GetComponent<RealtimeMovementControl>().actionState
                != RealtimeMovementControl.ActionState.Avoid &&
                hostAnimal.GetComponent<RealtimeMovementControl>().actionState
                != RealtimeMovementControl.ActionState.TryAvoid)
            {
                if (actionstate == RealtimeMovementControl.ActionState.TryAvoid ||
                    actionstate == RealtimeMovementControl.ActionState.Avoid)
                {
                    SetStateDiscover();
                }
            }
        }*/

        //For now:
        if (distance <= 30)
        {
            animator.ResetTrigger("onExit");
            isVisible = true;
            if (distance <= 15)
            {
                SetStateAvoid();
            } else
            {
                if (distance <= 20)
                {
                    SetStateWarn();
                } else
                {
                    SetStateDiscover();
                }
            }
        } else
        {
            if (isVisible)
            {
                animator.SetTrigger("onExit");
                animator.ResetTrigger("onAvoid");
                animator.ResetTrigger("onWarn");
                animator.ResetTrigger("onDiscover");
                isVisible = false;
            }

            if (distance > 40)
            {
                destroySelf();
            }
                
        }
    }

    private void destroySelf()
    {
        int idx = -1;
        foreach (GameObject go in IndicationSystem.indicationHosts)
        {
            idx++;
            if (go == host.gameObject)
                break;
            if (idx == IndicationSystem.indicationHosts.Count - 1)
                idx = -1;
        }
        IndicationSystem.indicationHosts.RemoveAt(idx);
        IndicationSystem.indicationIcons.RemoveAt(idx);
        Destroy(gameObject);
    }

    public void SetStateDiscover()
    {
        animator.ResetTrigger("onAvoid");
        animator.ResetTrigger("onWarn");
        animator.SetTrigger("onDiscover");
        signGameobject.GetComponent<Image>().material.color = Color.white;
    }

    public void SetStateWarn()
    {
        animator.ResetTrigger("onAvoid");
        animator.ResetTrigger("onDiscover");
        animator.SetTrigger("onWarn");
        signGameobject.GetComponent<Image>().material.color = Color.white;
    }

    public void SetStateAvoid()
    {
        animator.ResetTrigger("onWarn");
        animator.ResetTrigger("onDiscover");
        animator.SetTrigger("onAvoid");
        signGameobject.GetComponent<Image>().material.color = Color.white;
    }

    public void SetAnimal(GameObject a)
    {
        animal = a;
    }

    public void SetHost(GameObject h)
    {
        host = h;
    }

    public void SetPos(float posX, float posY)
    {
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
    }

    public void SetPos(Vector2 position)
    {
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(position.x, position.y);
    }
}
