using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomControlMain : MonoBehaviour
{
    [SerializeField]
    public GameObject mapCanvas;

    private bool cursorLocked;

    private float timeRef;

    /// <summary>
    /// Control how long UI will linger after player moves away his or her sight and
    /// how far will the player starts to see the sensor readings panel
    /// </summary>
    public float uiLingerTime = 10;
    public float uiVisibleDistance = 30f;

    private GameObject selected;

    // Start is called before the first frame update
    void Start()
    {
        cursorLocked = true;

        if (mapCanvas == null)
        {
            Debug.LogWarning("MapCanvas not instantiated, looking...");
            mapCanvas = GameObject.Find("MapCanvas");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetComponent<FirstPersonAIO>().playerCanMove)
            return;

        /// In conflict with ControlPC.cs
        //if (mapCanvas.activeSelf)
        //{
        //    setCursorLockState(false);
        //}
        //else
        //{
        //    if (Input.GetKeyDown(KeyCode.Escape))
        //        cursorLocked = !cursorLocked;

        //    setCursorLockState(cursorLocked);
        //}

        if (cursorLocked)
        {
            //Detect collider in front of the player with tag "Animal"
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, uiVisibleDistance))
            {
                if (hit.collider.gameObject.tag.Equals("Animal"))
                {
                    
                    if (selected != null && selected != hit.collider.gameObject)
                    {
                        selected.transform.root.gameObject.GetComponent<SensorUI>().deactivate();
                    }

                    selected = hit.collider.gameObject;

                    selected.transform.root.gameObject.GetComponent<SensorUI>().activate();

                    timeRef = Time.time;
                }
            }
            else
            {
                //Fade out the UIs after uiLingerTime
                if (Time.time - timeRef >= uiLingerTime)
                {
                    if (selected != null)
                    {
                        selected.transform.gameObject.GetComponent<SensorUI>().deactivate();

                        selected = null;
                    }
                }
            }
        }
    }

    public void setCursorLockState(bool lockState)
    {
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }
}
