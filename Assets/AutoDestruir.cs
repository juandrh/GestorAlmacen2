using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Clase que permite la autodestrucción programada de los marcadores
public class AutoDestruir : MonoBehaviour
{
    [SerializeField] private GameObject marcador;


    // Start is called before the first frame update
    void Start()
    {
        Destroy(marcador, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
