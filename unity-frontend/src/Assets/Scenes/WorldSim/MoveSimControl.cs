using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using Oculus.Platform.Models;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;

public class MoveSimControl : MonoBehaviour
{
    /*
     This is a helper class to create an accumulated weighted array used for randomness in decision making of FSM
     Reference:
     https://gamedev.stackexchange.com/questions/162976/how-do-i-create-a-weighted-collection-and-then-pick-a-random-element-from-it
    */
    public class WeightedRandomBehavior
    {

        private class Entry
        {
            public double accumulatedWeight;
            public double itemWeight;
            public string item { get; set; }
        }

        private List<Entry> entries = new List<Entry>();
        private double totalWeight;
        private System.Random rand = new System.Random();

        public void AddEntry(string item, double weight)
        {
            totalWeight += weight;
            entries.Add(new Entry { item = item, itemWeight = weight, accumulatedWeight = totalWeight });
        }

        public void ModifyValue(string state, double newWeight)
        {
            //update curr itemWeight
            int index = entries.FindIndex(x => x.item.Contains(state));
            if (index < 0)
            {
                Debug.Log("Invalid Index");
                return;
            }
            Entry temp;
            temp = entries[index];
            //weightDiff range from [-temp.itemWeight, newWeight - temp.itemWeight]
            double weightDiff = newWeight - temp.itemWeight;
            //Debug.Log(string.Format("weightDiff:{0}", weightDiff));
            temp.itemWeight += weightDiff;
            //calculate new accumulatedWeight with curr itemWeight 
            totalWeight += weightDiff;
            //Debug.Log(string.Format("new itemWeight:{0}", temp.itemWeight));
            //Debug.Log(string.Format("new total sum:{0}", accumulatedWeight));
            //update accumulatedWeight of all states following this state by adding weightDiff   
            for (int i = index; i < entries.Count; i++)
            {
                entries[i].accumulatedWeight += weightDiff;
                //Debug.Log(string.Format("item{0} has new accWeight:{1}", entries[i].item, entries[i].accumulatedWeight));
            }

        }

        public void getCurrContent()
        {
            foreach (Entry e in entries)
            {
                //Debug.Log(string.Format("item: {0:0}, value:{1:0}, accValue:{2:0}", e.item, e.itemWeight, e.accumulatedWeight));
            }
        }
        public string GetRandom()
        {
            double r = rand.NextDouble() * totalWeight;

            foreach (Entry entry in entries)
            {
                if (entry.accumulatedWeight >= r && entry.itemWeight != 0)
                {
                    return entry.item;
                }
            }
            return default(string); //should only happen when there are no entries
        }
    }

    //set default animal attributes 
    private static Vector2 radius_bound = new Vector2(50.0f, 80.0f);
    private const float idleTime = 5f;
    private const float walkTime = 20f;
    private const float thirst_max = 100f;
    private const float hunger_max = 100f;
    private const float thirst_min = 0f;
    private const float hunger_min = 0f;
    private const float thirst_dec = 0.004f;
    private const float hunger_dec = 0.002f;
    private const float thirst_inc = 0.2f;
    private const float hunger_inc = 0.1f;
    private const float eat_dist = 10.0f;
    private const float drink_dist = 20.0f;

    public float thirst;
    public float hunger;
    public float thirst_var;
    public float hunger_var;
    public bool eatReady;
    public bool drinkReady;
    public Vector3 destination;
    public float walk_speed = 0.8f;
    public float run_speed = 1.2f;
    // changes for world clock
    public float timer = 0.0f;
    public int clock = 0;    // 60s is a day, value in range (0, 59)
    public int seconds;         // Timer in seconds
    public bool daytime;        // if seconds % 60 is <= 5
    //public bool sleepReady;

    private bool actionUpdate = true;
    private AnimalControl animalControl;
    private PlayMakerFSM fsm;
    private Animator animator;

    private float deltaThirst;
    private float decisionRef;
    private string nextState;
    private float idleTimeElapsed;
    private float walkTimeElapsed;
    WeightedRandomBehavior weightedProb = new WeightedRandomBehavior();

    //TEST
    public GameObject prey;

    void Awake()
    {
        animalControl = GetComponent<AnimalControl>();
        fsm = GetComponent<PlayMakerFSM>();
        animator = GetComponent<Animator>();
        actionUpdate = false;
    }
    // // Start is called before the first frame update
    // void Start()
    // {
    //     animalControl = GetComponent<AnimalControl>();
    //     fsm = GetComponent<PlayMakerFSM>();
    //     animator = GetComponent<Animator>();
    //     actionUpdate = false;

    // }
    public void Initialize()
    {
        weightedProb.AddEntry("Drink", 0.0);
        weightedProb.AddEntry("Eat", 0.0);
        weightedProb.AddEntry("Walk", 50.0);
        weightedProb.AddEntry("Idle", 50.0);
        weightedProb.getCurrContent();
        thirst = Random.Range(20f, 65f);
        hunger = Random.Range(20f, 65f);
        // For some variation between the animals
        thirst_var = Random.Range(-0.001f, 0.001f);
        hunger_var = Random.Range(-0.0005f, 0.0005f);
        //eatReady = false;
        //drinkReady = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (thirst > thirst_max || hunger > hunger_max)
        //    fsm.SendEvent("Die");

    }


    // setTimer 
    // public void setTimer() 
    // {
    //     timer += Time.deltaTime;
    //     seconds = (int)(timer % 60);
    //     clock = seconds % 60;
    //     if (clock < 1) 
    //     {
    //         daytime = false;
    //     }
    //     else {
    //         daytime = true;
    //     }
    // }


    public void MakeDecision()
    {
        //TODO: setTimer
        //setTimer();
        //make decisions based on hunger/thrist or randomly choose to walk/stay idle
        if (thirst < 30)
        {
            nextState = "Drink";
        }
        else if (hunger < 30)
        {
            nextState = "Eat";
        }
        // else if (!daytime)
        // {
        //     nextState = "Sleep";
        // }
        else
        {
            weightedProb.ModifyValue("Drink", Math.Max(0, 80.0 - thirst));
            weightedProb.ModifyValue("Eat", Math.Max(0, 80.0 - hunger));
            weightedProb.getCurrContent();
            nextState = weightedProb.GetRandom();
        }

        // Override assignment for testing
        //nextState = "Walk";

        fsm.SendEvent(nextState);
    }

    public void IdleUpdate()
    {
        if (!actionUpdate)
        {
            actionUpdate = true;
            // First action
            idleTimeElapsed = 0f;
        }
        else
        {
            idleTimeElapsed += Time.deltaTime;
            if (idleTimeElapsed > idleTime)
                ActionComplete();
        }

    }

    public void WalkUpdate()
    {
        float param = 1f;

        if (!actionUpdate)
        {
            actionUpdate = true;
            destination = GetRandomNextLocation();
            walkTimeElapsed = 0f;
            //return;

            // Prey
            // if (gameObject.name.Contains("Zebra") || gameObject.name.Contains("Elephant"))
            // {
            //     float dist_min = float.MaxValue;
            //     Vector3 location = transform.position;
            //     List<int> predatorList = AnimalController.animalListByType[AnimalType.Lion];
            //     foreach (int id in predatorList)
            //     {
            //         Vector3 i = AnimalController.idToAnimal[id].transform.position;
            //         float dist = Vector3.Distance(transform.position, i);
            //         if (dist < dist_min)
            //         {
            //             dist_min = dist;
            //             location = i;
            //         }
            //     }

            //     if (dist_min < 20f)
            //         param = 1.5f;
            //     else if (dist_min < 50f)
            //         param = 1.3f;
            //     else if (dist_min < 100f)
            //         param = 1.1f;

            //     destination += param * Vector3.Normalize(destination - location);
            // }

            animalControl.move(destination, walk_speed);
        }
        else
        {
            thirst -= (thirst_dec + thirst_var);
            hunger -= (hunger_dec + hunger_var);

            walkTimeElapsed += Time.deltaTime;
            if (walkTimeElapsed > walkTime) {
                ActionComplete();
            }
            if (animalControl.movementFinished)
            {
                ActionComplete();
            }
        }

    }

    public void EatUpdate()
    {
        //TEMPORARY SCRIPTS FOR PREDATOR-AND-PREY
        //EXCLUDE PREDATOR FROM CALLING ACTION_COMPLETE IN SCRIPT
        //ENDING STATES FOR PREDATORS ARE IN FSM
        if (hunger > hunger_max)
        {
            ActionComplete();
        }
        //TEMPORARY SCRIPTS FOR PREDATOR-AND-PREY
        //ALLOW PREDATOR TO REGENERATE HUNGER EVEN IN EAT UPDATE ONLY
        if (gameObject.name.Contains("Lion"))
        {
            hunger += hunger_inc;
            return;
        }

        if (eatReady) {
            hunger += hunger_inc;
        }
        else if (!actionUpdate)
        {
            actionUpdate = true;
            destination = GetEatLocation();
            animalControl.move(destination, run_speed);
        }
        else if (animalControl.movementFinished)
        {
            eatReady = true;
            animator.SetTrigger("Eat");
            // if (Vector3.Distance(transform.position, GetEatLocation()) < eat_dist)
            // {
            //     hunger += hunger_inc;
            // }
            // else
            // {
            //     animalControl.stopMotion();
            //     actionUpdate = false;
            // }
        }

    }

    public void DrinkUpdate()
    {
        if (thirst > thirst_max)
        {
            ActionComplete();
        }
        else if (drinkReady)
        {
            thirst += thirst_inc;
        }
        else if (!actionUpdate)
        {
            actionUpdate = true;
            destination = GetDrinkLocation();
            animalControl.move(destination, run_speed);
        }
        else if (animalControl.movementFinished)
        {
            drinkReady = true;
            animator.SetTrigger("Eat");
        } else
        {
            actionUpdate = false;
            fsm.SendEvent("FINISHED");
            
        }
        
    }


    // public void SleepUpdate()
    // {
    //     if (daytime)
    //         ActionComplete();
    //     else if (!actionUpdate)
    //     {
    //         actionUpdate = true;
    //         destination = GetShelterLocation();
    //         animalControl.move(destination, run_speed);
    //     }
    //     else if (animalControl.movementFinished)
    //     {
    //         //sleepReady = true;
    //         // action embedded in unity?
    //         Debug.log("shelter arrived. sleep for 1s");
    //         ActionComplete();
    //     }
    // }

    public void ActionEnd()
    {
        // Clamp thirst hunger
        thirst = Mathf.Clamp(thirst, thirst_min, thirst_max);
        hunger = Mathf.Clamp(hunger, hunger_min, hunger_max);
        // Reset Value
        if (!actionUpdate)
        {
            //animalControl.stopMotion();
            fsm.SendEvent("FINISHED");
        }
    }

    private Vector3 GetRandomNextLocation()
    {
        Vector3 cur = gameObject.transform.position;

        float x = cur.x;
        float z = cur.z;

        float x_delta = Random.Range(5f, radius_bound[1] - 5f);

        float dir = Random.Range(0, 2);
        if (dir == 0)
        {
            dir = -1;
        }

        x += dir * x_delta;

        // (x - cur.x)^2 + (z - (cur.z))^2 = minR^2
        // (x - cur.x)^2 + (z - (cur.z))^2 = maxR^2

        float minZ_circle, maxZ_circle;

        dir = Random.Range(0, 2);
        if (dir == 0)
        {
            dir = -1;
        }

        if (Mathf.Abs(x - cur.x) >= radius_bound[0])
        {
            float z_delta = Mathf.Sqrt(Mathf.Pow(radius_bound[1], 2) - Mathf.Pow((x - cur.x), 2));

            maxZ_circle = z + z_delta;
            minZ_circle = z - z_delta;
        }
        else
        {
            minZ_circle = Mathf.Sqrt(Mathf.Pow(radius_bound[0], 2) - Mathf.Pow((x - cur.x), 2)) * dir + cur.z;
            maxZ_circle = Mathf.Sqrt(Mathf.Pow(radius_bound[1], 2) - Mathf.Pow((x - cur.x), 2)) * dir + cur.z;
        }


        if (minZ_circle < maxZ_circle)
        {
            float temp = maxZ_circle;
            maxZ_circle = minZ_circle;
            minZ_circle = temp;
        }

        z = Random.Range(minZ_circle, maxZ_circle);

        x = Mathf.Clamp(x, Client.min_bound.x, Client.max_bound.x);
        z = Mathf.Clamp(z, Client.min_bound.y, Client.max_bound.y);

        // Vector3 final_dest = new Vector3(x, cur.y, z);
        // float test_distance = Vector3.Distance(cur, final_dest);
        // print("distance to destination " + test_distance);
        //if (x < 100 || x > 900 || z < 100 || z > 900)
        //{
        //}
        return new Vector3(x, cur.y, z);

    }

    private Vector3 GetEatLocation()
    {
        float dist_min = float.MaxValue;
        Vector3 location = transform.position;
        // Predator
        if (gameObject.name.Contains("Lion"))
        {
            List<int> preyList = AnimalController.animalListByType[AnimalType.Zebra];
            foreach (int id in preyList)
            {
                Vector3 i = AnimalController.idToAnimal[id].transform.position;
                float dist = Vector3.Distance(transform.position, i);
                if (dist < dist_min)
                {
                    dist_min = dist;
                    location = i;
                }
            }
        }
        else
        {
            foreach (Vector3 i in AnimalController.foodLocationList)
            {
                float dist = Vector3.Distance(transform.position, i);
                if (dist < dist_min)
                {
                    dist_min = dist;
                    location = i;
                }
            }
        }
        
        return location;
    }
    
    private Vector3 GetDrinkLocation()
    {
        float dist_min = float.MaxValue;
        GameObject nearest = null;
        foreach (GameObject water in GameObject.FindGameObjectsWithTag("Water"))
        {
            float dist = Vector3.Distance(transform.position, water.transform.position);
            if (dist < dist_min)
            {
                dist_min = dist;
                nearest = water;
            }
        }
        if (nearest == null)
        {
            Debug.LogError("Water not found");
            return Vector3.zero;
        }
        Vector3 destination = nearest.GetComponent<MeshCollider>().ClosestPointOnBounds(transform.position);
        // Move into the water a bit
        destination += Vector3.Normalize(destination - transform.position) * drink_dist;
        // Set destination to terrain height
        destination += Vector3.up * (Terrain.activeTerrain.SampleHeight(destination) - destination.y);
        return destination;
    }
    //Hardcoded Shelter locations for testing
    
    private List<Vector3> TestShelters = new List<Vector3>() 
    {
        new Vector3(800.0f, 100.0f, 400.0f),
        new Vector3(650.0f, 100.0f, 410.0f)
    };
    

    // private Vector3 GetShelterLocation()
    // {
    //     List<Vector3> Shelters = TestShelters; //GameObject.FindGameObjectsWithTag("Shelter") when Shelter tagged objects are added
    //     float dist_min = float.MaxValue;
    //     Vector3 closest_shelter = Vector3.zero;
       
    //     if (Shelters.Count == 0) 
    //     {
    //         Debug.LogError("No Shelters found");
    //         return Vector3.zero; 
    //     }

    //     foreach (Vector3 shelter in Shelters)
    //     {
    //         float dist = Vector3.Distance(transform.position, shelter);
    //         if (dist < dist_min) 
    //         {
    //             dist_min = dist;
    //             closest_shelter = shelter;
    //         }
    //     }
    
    //     return closest_shelter;
    // }
    //A simple modifcation to getEatLocation function for predators
    //return gameobject instead
    public GameObject getPreyGameObject()
    {
        float dist_min = float.MaxValue;

        GameObject prey = null;

        List<int> preyList = AnimalController.animalListByType[AnimalType.Zebra];
        foreach (int id in preyList)
        {
            Vector3 i = AnimalController.idToAnimal[id].transform.position;
            float dist = Vector3.Distance(transform.position, i);
            if (dist < dist_min)
            {
                dist_min = dist;
                prey = AnimalController.idToAnimal[id].transform.gameObject;
            }
        }

        return prey;
    }

    public void ActionComplete()
    {
        animalControl.stopMotion();
        // Set flag
        actionUpdate = false;
        // Send FSM Event
        fsm.SendEvent("FINISHED");
    }

}
