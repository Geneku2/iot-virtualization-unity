using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OVRSimpleJSON;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using WebSocketSharp;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using UnitySocketIO;
using UnitySocketIO.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Adapted from older script.
/// Instantiates animals and controls animal actions
/// </summary>
public class AnimalController : MonoBehaviour
{
    // animal types
    public static string[] AnimalTypes = { "Elephant", "Lion", "Zebra" };
    // stores animal count
    public static Dictionary<AnimalType, int> animal_count;

    /// <summary>
    /// List of created animals
    /// </summary>
    public List<Animal> Animals { get; private set; }

    /// <summary>
    /// Game object that holds each animal template named by their AnimalType
    /// </summary>
    [SerializeField]
    private GameObject animalTemplates;

    /// <summary>
    /// Maps an animal type to its proper template game object
    /// </summary>
    private Dictionary<AnimalType, GameObject> animalTypeToTemplate;

    public static Dictionary<AnimalType, List<int>> animalListByType;

    [SerializeField]
    private GameObject foodLocations;

    public static List<Vector3> foodLocationList;

    /// <summary>
    /// Map animal ID to animal
    /// </summary>
    public static Dictionary<int, Animal> idToAnimal;

    private Dictionary<int, AnimalSimulationClient.Position> prevPos;

    private float previousMoveTime;

    private const float minDistance = 0.01f;
    private const float radianToDegree = 180 / Mathf.PI;

    private void Start()
    {
        if (animalTemplates == null)
        {
            Debug.LogWarning("AnimalTemplates not instantiated, looking...");
            animalTemplates = GameObject.Find("AnimalTemplates");
        }

        if (foodLocations == null)
        {
            Debug.LogWarning("FoodLocations not instantiated, looking...");
            foodLocations = GameObject.Find("FoodLocations");
        }

    }

    //ADDED BY HAO QI TO MAKE CHANGING ANIMAL NUMBERS EASIER, SEE AnimalListController.cs
    public void setAnimalCount(Dictionary<AnimalType, int> set){
        animal_count = set;
    }

    /// <summary>
    /// Finds the animal templates for each animal type
    /// </summary>
    private void InitAnimalTemplates()
    {
        animalTypeToTemplate = new Dictionary<AnimalType, GameObject>();
        foreach (AnimalType animal in Enum.GetValues(typeof(AnimalType)))
        {
            Transform animalTransform = animalTemplates.transform.Find(animal.ToString());
            if (animalTransform == null)
            {
                Debug.LogWarning("No associated animal template found for: " + animal.ToString());
            }
            else
            {
                //Debug.Log("Found animal template for: " + animal.ToString());
                animalTypeToTemplate.Add(animal, animalTransform.gameObject);
            }
        }

        if (animal_count == null ||
         (animal_count[AnimalType.Elephant] < 0 || animal_count[AnimalType.Lion] < 0 || animal_count[AnimalType.Zebra] < 0))
        {
            animal_count = new Dictionary<AnimalType, int>();
            animal_count.Add(AnimalType.Elephant, 1);
            animal_count.Add(AnimalType.Lion, 1);
            animal_count.Add(AnimalType.Zebra, 1);
        }
    }

    /// <summary>
    /// Finds the animal templates for each animal type
    /// </summary>
    private void InitSupplyLocations()
    {
        foodLocationList = new List<Vector3>();
        foreach (Transform i in foodLocations.transform)
            foodLocationList.Add(i.position);
    }

    /// <summary>
    /// Instantiate new animals from the information provided
    /// Note: each animal ID must be unique from all instantiated so far
    /// </summary>
    /// <param name="newAnimals"></param>
    public void CreateAnimals(AnimalSimulationClient.AnimalInfo[] newAnimals)
    {
        InitAnimalTemplates();
        InitSupplyLocations();
        Animals = new List<Animal>();
        idToAnimal = new Dictionary<int, Animal>();
        animalListByType = new Dictionary<AnimalType, List<int>>();
        int id = 0;

        //DECODING ANIMAL TO NUMBER JSON
        string filePath = Directory.GetCurrentDirectory() + "/SimResults";
        try
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

        } catch (IOException e) {
            Console.WriteLine("File Path cannot be created. Exception: " + e.Message);
        }
        filePath = filePath + "/DecodeKeys_Animal.txt";
        File.Delete(filePath);

        
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine("{");

        int totalAnimals = 0;
        foreach (AnimalType type in Enum.GetValues(typeof(AnimalType))){
            totalAnimals += animal_count[type];
        }

        foreach (AnimalType type in Enum.GetValues(typeof(AnimalType)))
        {
            List<int> animalListInType = new List<int>();
            GameObject template = animalTypeToTemplate[type];
            //GameObject parent = GameObject.Find(animal + "_list");
            int count = animal_count[type];
            
            for (int i = 0; i < count; i++)
            {
                Vector3 new_position = new Vector3(UnityEngine.Random.Range(Client.min_bound[0], Client.max_bound[0]), 0f, UnityEngine.Random.Range(Client.min_bound[1], Client.max_bound[1]));
                new_position.y = Terrain.activeTerrain.SampleHeight(new_position);
                GameObject instance = Instantiate(template);
                instance.name = template.name + "_" + id;
                instance.transform.position = new_position;
                Animal animalComponent = instance.GetComponent<Animal>();
                animalComponent.InitAnimal(id, type);
                Animals.Add(animalComponent);
                idToAnimal.Add(id, animalComponent);
                animalListInType.Add(id);
                
                
                if(id < totalAnimals - 1){
                    writer.WriteLine("\"" + id + "\": \"" + type + "\",\n");
                } else {
                    writer.WriteLine("\"" + id + "\": \"" + type + "\"");
                }
                
                id++;
            }

            animalListByType.Add(type, animalListInType);
        }
        writer.WriteLine("}");
        writer.Close();
        // old
        //foreach (AnimalSimulationClient.AnimalInfo newAnimal in newAnimals)
        //{
        //    GameObject newAnimalObj = Instantiate(animalTypeToTemplate[newAnimal.type], transform);
        //    Animal animalComponent = newAnimalObj.GetComponent<Animal>();
        //    animalComponent.InitAnimal(newAnimal);
        //    Animals.Add(animalComponent);
        //    idToAnimal.Add(animalComponent.ID, animalComponent);
        //}
    }

    //TODO: understand why the previous information needs to be stored and used
    //TODO: figure out why minDistance is needed
    //TODO: figure out how the yRotation works
    //TODO: figure out if it's possible to move this to the animal class
    public void DoAction(AnimalSimulationClient.AnimalInfo[] newInfo)
    {
        // If this is missing, unity crashes due to animation speed issues?
        // TODO: figure out how to better do this
        if (prevPos == null)
        {
            prevPos = new Dictionary<int, AnimalSimulationClient.Position>();
            foreach (AnimalSimulationClient.AnimalInfo info in newInfo)
            {
                Animal animal = idToAnimal[info.id];
                animal.transform.position = new Vector3(info.pos.x, animal.transform.position.y, info.pos.z);
                prevPos[info.id] = info.pos;
                previousMoveTime = Time.time;
                animal.State = info.state;
            }
        }

        bool moved = false;
        foreach (AnimalSimulationClient.AnimalInfo info in newInfo)
        {
            Animal animal = idToAnimal[info.id];
            AnimalSimulationClient.Position previousPos = prevPos[info.id];
            if (animal.State == AnimalState.Move)
            {
                float distance = CalcDistance(previousPos, info.pos);
                if (distance > minDistance)
                {
                    if (animal.transform.position.y < -100)
                    {
                        animal.transform.position = new Vector3(previousPos.x, 100, previousPos.z);
                        Debug.LogError("under terrain" + previousPos.x);
                        Debug.LogError("under terrain" + previousPos.z);
                    }
                    animal.transform.position = new Vector3(previousPos.x, animal.transform.position.y, previousPos.z);

                    /*
                    float yRotation;
                    // TODO: figure out what this is
                    if (Mathf.Abs(info.pos.z - previousPos.z) < 0.001f)
                    {
                        yRotation = (info.pos.x - previousPos.x) > 0 ? 90 : -90;
                    }
                    else
                    {
                        yRotation = Mathf.Atan2((info.pos.x - previousPos.x), (info.pos.z - previousPos.z)) * radianToDegree;
                    }

                    if (Mathf.Abs(transform.eulerAngles.y - yRotation) > 1)
                    {
                        animal.transform.eulerAngles = new Vector3(0f, yRotation, 0f);
                    }*/

                    animal.Move(distance, Time.time - previousMoveTime);
                    moved = true;
                }
            }
            else if (animal.State == AnimalState.Eat)
            {
                animal.Eat();
            }
            else if (animal.State == AnimalState.Death)
            {
                animal.Death();
            }

            animal.State = info.state;
        }

        if (moved)
        {
            foreach (AnimalSimulationClient.AnimalInfo info in newInfo)
            {
                prevPos[info.id] = info.pos;
            }
            previousMoveTime = Time.time;
        }
    }

    protected float CalcDistance(AnimalSimulationClient.Position first, AnimalSimulationClient.Position second)
    {
        return Mathf.Sqrt(Mathf.Pow(first.x - second.x, 2) + Mathf.Pow(first.z - second.z, 2));
    }
}
