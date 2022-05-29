using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoginLinkToggle : MonoBehaviour
{
    public GameObject ipAddressFeild;
    public GameObject self;

    private const string iotsys = "iotsys5";
    private const string iotprod = "iot-prod";

    // Start is called before the first frame update
    void Start()
    {
        ipAddressFeild = GameObject.Find("Default IP");
        self = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (ipAddressFeild != null && self.GetComponent<Toggle>().isOn)
        {
            ipAddressFeild.GetComponent<Text>().text = iotprod + ".cs.illinois.edu";
        } else {
            ipAddressFeild.GetComponent<Text>().text = iotsys + ".cs.illinois.edu";
        }
    }
}
