using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherControl : MonoBehaviour
{
    // the prefab of Rain from the assets
    public GameObject Rain;
    // the x position to rain
    [SerializeField]
    float x;
    // the z position to rain
    [SerializeField]
    float z;
    // the period it rains (default to be 10 seconds)
    [SerializeField]
    float duration = 10f;
    // a bool variale to indicate whether it should rain or not
    [SerializeField]
    bool isRaining = false;

    void Start()
    {
        // check if the MakeRain Assets under OldEnvironment exists
        if (Rain == null) {
            Debug.Log("The GameObject Rain does not exist, please check.");
            return;
        }
        // no rain at default
        Rain.SetActive(false);
    }

    void Update()
    {
        // call the Rain() function when isRain is true
        if (isRaining)
        {
            Raining();
            isRaining = false;
        }
    }

    // the function to instantiate the Rain prefab at a selected position (x, 10, z)
    private void Raining() {
        // instantiate the Rain prefab at the selected position
        Vector3 position = new Vector3(x, 10f, z);
        GameObject rainObject = Instantiate(Rain, position, transform.rotation);
        rainObject.SetActive(true);

        // stop the rain after a period of duration 
        Destroy(rainObject, duration);
    }
}
