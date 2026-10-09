using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinPropellerX : MonoBehaviour
{
    public float spinSpeed = 1000;

    // Update is called once per frame
    void Update()
    {
        // spin the propeller around its local Z axis
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);
    }
}
