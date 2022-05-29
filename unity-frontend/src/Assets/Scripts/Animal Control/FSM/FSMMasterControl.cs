using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayMaker;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.PlayerLoop;

public class FSMMasterControl : MonoBehaviour
{
    const string ELEPHANT = "Elephant";

    // animal types
    public static string[] animals = {"Elephant", "Lion", "Zebra"};
    // stores animal count
    public static Dictionary<string, int> animal_count;

    // world boundaries
    public static Vector2 min_bound = new Vector2(5f, 5f);
    public static Vector2 max_bound = new Vector2(995f, 995f);

    

    void Start()
    {

        // initializeFSM();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //public static List<Animal> initializeFSM(GameObject templates)
    //{

    //    List<Animal> animalList = new List<Animal>();

    //    foreach (string animal in animals)
    //    {
    //        GameObject template = templates.transform.Find(animal).gameObject;
    //        //GameObject parent = GameObject.Find(animal + "_list");
    //        int count = animal_count[animal];
    //        int id = 0;
    //        for (int i = 0; i < count - 1; i++)
    //        {
    //            GameObject instance = Instantiate(template);
    //            Vector3 new_position = new Vector3(Random.Range(min_bound[0], max_bound[0]), 0f, Random.Range(min_bound[1], max_bound[1]));
    //            new_position.y = Terrain.activeTerrain.SampleHeight(new_position);
    //            instance.transform.position = new_position;
    //            instance.GetComponent<Animal>().InitAnimal(id++);
    //            animalList.Add(instance.GetComponent<Animal>());
    //            //instance.transform.parent = parent.transform;
    //        }
    //    }
    //    return animalList;

    //}

    public static Vector3 getRandomNextLocation(GameObject animal, float minRadius,
        float maxRadius, float minX, float maxX, float minZ, float maxZ)
    {
        Vector3 cur = animal.transform.position;

        float x = cur.x;
        float z = cur.z;

        float  x_delta = Random.Range(0, maxRadius);

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

        if (Mathf.Abs(x - cur.x) >= minRadius)
        {
            float z_delta = Mathf.Sqrt(Mathf.Pow(maxRadius, 2) - Mathf.Pow((x - cur.x), 2));

            maxZ_circle = z + z_delta;
            minZ_circle = z - z_delta;
        } else
        {
            minZ_circle = Mathf.Sqrt(Mathf.Pow(minRadius, 2) - Mathf.Pow((x - cur.x), 2)) * dir + cur.z;
            maxZ_circle = Mathf.Sqrt(Mathf.Pow(maxRadius, 2) - Mathf.Pow((x - cur.x), 2)) * dir + cur.z;
        }


        if (minZ_circle < maxZ_circle)
        {
            float temp = maxZ_circle;
            maxZ_circle = minZ_circle;
            minZ_circle = temp;
        }

        z = Random.Range(minZ_circle, maxZ_circle);

        x = Mathf.Clamp(x, minX, maxX);
        z = Mathf.Clamp(z, minZ, maxZ);

        return new Vector3(x, cur.y, z);

    }
    public static Vector3 getNearestWater(GameObject animal, float minX, float maxX, float minZ, float maxZ)
    {
        // temporary
        return new Vector3(600f, 0f, 500f);
    }

    public static Vector3 getNearestFood(GameObject animal, float minX, float maxX, float minZ, float maxZ)
    {
        // temporary
        return new Vector3(200f, 0f, 440f);
    }

    public Transform getNearestAnimal(Vector3 cooord, string animal_name)
    {
        GameObject animal_list = GameObject.Find(animal_name + "_list");
        float min_distance = float.MaxValue;
        Transform min_child = null;
        foreach (Transform child in animal_list.GetComponentsInChildren<Transform>())
        {
            float dist = Vector3.Distance(child.position, cooord);
            if (dist < min_distance)
            {
                min_distance = dist;
                min_child = child;
            }
        }
        return min_child;
    }

    public static void animalMoveTo(GameObject animal, Vector3 d)
    {
        animal.GetComponent<RealtimeMovementControl>().move(d);
    }

    public static void animalMoveTowards(GameObject animal, GameObject target)
    {
        animal.GetComponent<RealtimeMovementControl>().move(target);
    }

    public static void hunt(GameObject predator, GameObject prey)
    {
        animalMoveTowards(predator, prey);
    }
}
