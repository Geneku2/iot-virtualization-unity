using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AvoidPredator : MonoBehaviour
{
    public List<GameObject> predators;
    private RealtimeMovementControl realtimeMovementControl;
    private bool predator_around = false;
    private Vector3 dest;
    public int alert_range = 40;
    public bool onAvoid = false;

    void Start()
    {
        if (predators == null)
        {
            Debug.Log("Predators List is NULL!!!");
            return;
        }
        realtimeMovementControl = GetComponent<RealtimeMovementControl>();

        if (realtimeMovementControl == null)
        {
            Debug.Log("RealtimeMovementControl is NULL!!!");
            return;
        }

        //dest = new Vector3(0, 0, 200);
        realtimeMovementControl.move(dest);
    }

    void Update()
    {

        if (onAvoid)
        {
            // begin avoiding
            //realtimeMovementControl.move(dest);
            List<Vector3> escape_direcs = CheckPredator();
            if (predator_around)
            {
                Escape(escape_direcs);
                predator_around = false;
            }
        }
        else
        {
            // do nothing
        }
    }

    void Escape(List<Vector3> direcs)
    {
        Vector3 escape_direc = new Vector3(0, 0, 0);
        foreach (Vector3 direc in direcs)
        {
            escape_direc += direc;
        }
        dest = -200 * escape_direc.normalized;
        realtimeMovementControl.move(dest + transform.position);
    }

    List<Vector3> CheckPredator()
    {
        List<Vector3> run_direcs = new List<Vector3>();
        foreach (GameObject predator in predators)
        {
            Vector3 prey_loc = transform.position;
            Vector3 pred_loc = predator.transform.position;
            double distance = Math.Sqrt(Math.Pow((prey_loc.x - pred_loc.x), 2) + Math.Pow((prey_loc.z - pred_loc.z), 2));
            if (distance <= alert_range)
            {
                float coefficient = (float) Math.Pow(1 / distance, 2);
                Vector3 run_direc = new Vector3((pred_loc.x - prey_loc.x), 0,
                    (pred_loc.z - prey_loc.z));
             
                run_direcs.Add(coefficient * run_direc.normalized);
                predator_around = true;
            }
        }
        return run_direcs ;
    }

    public void StartAvoidPredator()
    {
        onAvoid = true;
    }

    public void AssignPredator(GameObject predator)
    {
        if (predators == null)
        {
            predators = new List<GameObject>();

            predators.Add(predator);
        } else
        {
            predators.Add(predator);
        }
    }

    public void RemovePredator(GameObject predator)
    {
        int index = -1;

        for(int i = 0; i < predators.Count; i++)
        {
            if (predator == predators[i])
            {
                index = i;
                break;
            }
        }

        if (index != -1)
        {
            predators.RemoveAt(index);
        }
    }

    public void OnExitAvoidPredator()
    {
        onAvoid = false;
        predators.Clear();
    }

    public bool isHunted()
    {
        return predators.Count > 0;
    }
}
