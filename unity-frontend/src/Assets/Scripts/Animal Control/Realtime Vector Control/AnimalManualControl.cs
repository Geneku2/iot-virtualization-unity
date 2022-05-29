using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManualControl : MonoBehaviour
{
    public bool manualControlOn;
    public bool animalCameraActive;
    private RealtimeMovementControl moveControl;

    public GameObject camera;

    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        moveControl = GetComponent<RealtimeMovementControl>();

        if (camera == null)
            Debug.LogError("Animal camera not initialized!");

        if (player == null)
            player = GameObject.Find("Player");

        manualControlOn = false;
        camera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (manualControlOn)
        {
            float forward, turn;

            if (Input.GetAxis("Vertical") > 0)
            {
                forward = 1.35f;
            } else
            {
                forward = 0;

                if (Input.GetAxis("Vertical") < 0)
                {
                    transform.position += (-1) * transform.forward * 3 * Time.deltaTime;
                }
            }

            if (Input.GetAxis("Horizontal") > 0)
            {
                turn = 1;
            } else if (Input.GetAxis("Horizontal") < 0)
            {
                turn = -1;
            } else
            {
                turn = 0;
            }

            moveControl.move(forward, turn);
        }
    }

    public void SwitchToManualControl()
    {
        manualControlOn = true;
        animalCameraActive = true;

        player.GetComponent<FirstPersonAIO>().playerCanMove = false;
        player.GetComponent<FirstPersonAIO>().playerCamera.gameObject.SetActive(false);
        camera.SetActive(true);

        IndicationSystem.TurnOnIS(gameObject);
    }

    public void SwitchOffManualControl()
    {
        manualControlOn = false;
        moveControl.forward = 0;
        moveControl.turn = 0;
    }

    public void SwitchToAnimalCamera()
    {
        animalCameraActive = true;

        player.GetComponent<FirstPersonAIO>().playerCanMove = false;
        player.GetComponent<FirstPersonAIO>().playerCamera.gameObject.SetActive(false);
        camera.SetActive(true);

        IndicationSystem.TurnOnIS(gameObject);
    }

    public void SwitchOffAnimalCamera()
    {
        animalCameraActive = false;

        camera.SetActive(false);
        player.GetComponent<FirstPersonAIO>().playerCanMove = true;
        player.GetComponent<FirstPersonAIO>().playerCamera.gameObject.SetActive(true);

        IndicationSystem.TurnOffIS();
    }
}
