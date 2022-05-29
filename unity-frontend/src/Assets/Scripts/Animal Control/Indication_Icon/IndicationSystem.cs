using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicationSystem : MonoBehaviour
{
    //There can only exist ONE indication system at a time

    public static bool indicationSystemSwitch;

    public static GameObject hostAnimal;

    public GameObject destinationIndicatorGameObject;
    private static GameObject destinationIndicator;

    public GameObject canvas;
    public GameObject indicationIconTemplate;

    private static RealtimeMovementControl moveControl;

    public static List<GameObject> indicationHosts = new List<GameObject>();
    public static List<GameObject> indicationIcons = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        destinationIndicator = destinationIndicatorGameObject;

        if (hostAnimal != null)
            moveControl = hostAnimal.GetComponent<RealtimeMovementControl>();

        destinationIndicator.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (indicationSystemSwitch)
        {
            //Destination Indication
            if (!moveControl.destinationReached)
            {
                destinationIndicator.transform.position =
                    new Vector3(moveControl.destination.x, destinationIndicator.transform.position.y,
                        moveControl.destination.z);
                destinationIndicator.SetActive(hostAnimal.GetComponent<AnimalManualControl>().animalCameraActive);
            } else
            {
                destinationIndicator.SetActive(false);
            }

            if (hostAnimal.GetComponent<AnimalManualControl>().animalCameraActive)
            {
                //Collider Indication
                Collider[] hitColliders = Physics.OverlapSphere(hostAnimal.transform.position, 30);

                Camera animalCamera = hostAnimal.GetComponent<AnimalManualControl>().camera.GetComponent<Camera>();
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(animalCamera);

                foreach (var hitCollider in hitColliders)
                {
                    print("Examining " + hitCollider.gameObject);

                    if (hitCollider.tag == "Tree" || hitCollider.tag == "Animal")
                    {
                        if (hitCollider.gameObject == hostAnimal)
                            continue;
                    }
                    else
                    {
                        continue;
                    }
                    //Find if collider is already in list
                    //int idx = indicationHosts.FindIndex(gameobject => gameObject.name == hitCollider.gameObject.name);
                    int idx = -1;
                    foreach (GameObject go in indicationHosts)
                    {
                        idx++;
                        if (go == hitCollider.gameObject)
                            break;
                        if (idx == indicationHosts.Count - 1)
                            idx = -1;
                    }

                    print("Count: " + indicationHosts.Count);

                    //Check if the animal is within the viewing frustum
                    bool withinView = false;
                    Vector2 transformXZForwardAngle = new Vector2(hostAnimal.transform.forward.x, hostAnimal.transform.forward.z);
                    Vector2 transformToObjectXZAngle = new Vector2(hitCollider.gameObject.transform.position.x - hostAnimal.transform.position.x,
                                                                    hitCollider.gameObject.transform.position.z - hostAnimal.transform.position.z);
                    //print("Viewing angle: " + Vector2.Angle(transformXZForwardAngle, transformToObjectXZAngle));
                    //print("Forward v2d: " + transformXZForwardAngle);
                    //print("Relative v2d: " + transformToObjectXZAngle);
                    withinView = Vector2.Angle(transformXZForwardAngle, transformToObjectXZAngle) < ((animalCamera.fieldOfView + 60) / 2);

                    if (idx == -1)
                    {
                        if (withinView)
                        {
                            GameObject newIcon = Instantiate(indicationIconTemplate, canvas.transform);
                            newIcon.GetComponent<IndicationIcon>().SetHost(hitCollider.gameObject);
                            newIcon.GetComponent<IndicationIcon>().SetAnimal(hostAnimal);
                            newIcon.SetActive(true);
                            indicationHosts.Add(hitCollider.gameObject);
                            indicationIcons.Add(newIcon);
                        }
                    }
                    else
                    {
                        if (withinView)
                        {
                            indicationIcons[idx].SetActive(true);
                        }
                        else
                        {
                            indicationIcons[idx].SetActive(false);
                        }
                    }

                }
            }
        }
    }

    public static void SetHostAnimal(GameObject animal)
    {
        if (animal == hostAnimal)
            return;

        indicationHosts.Clear();
        indicationIcons.Clear();

        hostAnimal = animal;

        moveControl = hostAnimal.GetComponent<RealtimeMovementControl>();
    }

    public static void TurnOffIS()
    {
        destinationIndicator.SetActive(false);
        indicationSystemSwitch = false;
        indicationHosts.Clear();
        indicationIcons.Clear();
    }

    public static void TurnOnIS(GameObject animal)
    {
        indicationSystemSwitch = true;
        SetHostAnimal(animal);
    }
}
