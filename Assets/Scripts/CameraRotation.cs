using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    private float rotationSpeed = 200;
    [SerializeField] GameObject player;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float horizontalInput = Input.GetAxis("Mouse X");

        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);

        transform.position = player.transform.position;
    }
}