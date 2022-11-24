using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implementación para el movimiento de la cámara
public class CameraMov : MonoBehaviour
{

    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    private float horizontalInput;
    private float verticalInput;
    private float fordwardInput;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        fordwardInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Mouse Y");
        verticalInput = Input.GetAxis("Mouse X");
        transform.Translate(Vector3.forward * Time.deltaTime * speed * fordwardInput);

        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.left * Time.deltaTime * rotationSpeed * horizontalInput * 200);
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * verticalInput * 200);
        }

    }
}
