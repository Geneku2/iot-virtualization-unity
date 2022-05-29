using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentData : MonoBehaviour
{
    // Start is called before the first frame update
    public float data;

    public float SquareDist(Vector3 pos)
    {
        return Mathf.Pow(pos.x - transform.position.x, 2) + Mathf.Pow(pos.z - transform.position.z, 2);
    }
}
