using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float rotationSpeed = 50.0f;

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
    }
}
