using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Clase para la creación de marcadores de la ruta
public class MarcadorRuta
{
    public PosicionAlmacen posicion;
    public float G;
    public float H;
    public float F;
    public GameObject marcador;
    public MarcadorRuta padre;

    public MarcadorRuta(PosicionAlmacen pos, float g, float h, float f, GameObject marcador, MarcadorRuta r)
    {
        posicion = pos;
        G = g;
        H = h;
        F = f;
        this.marcador = marcador;
        padre = r;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return posicion.Equals(((MarcadorRuta)obj).posicion);
        }

    }

    public override int GetHashCode()
    {
        return 0;
    }

}



// Clase principal que proporciona la inteligencia al robot
public class RobotIA : MonoBehaviour
{
    public Almacen almacen;
    public Material M_frontera;
    public Material M_visitados;
    private List<MarcadorRuta> frontera = new List<MarcadorRuta>();
    private List<MarcadorRuta> visitados = new List<MarcadorRuta>();
    private List<Vector3> posicionesRuta = new List<Vector3>();
    public GameObject origen;
    public GameObject destino;
    public GameObject pasoRuta;
    private MarcadorRuta nodoOrigen;
    private MarcadorRuta nodoDestino;
    private MarcadorRuta nodoUltimo;
    private bool terminadoCalculoRuta = false;
    private List<PosicionAlmacen> posiciones;
    private Vector3 posicionOrigen;
    private Vector3 posicionDestino;
    [SerializeField] private GameObject robot;
    private bool estaMoviendo = false;
    int indiceMarcadorActual = 0;
    private float velocidad = 30.0f;
    private float velocidadRotacion = 15.0f;


    // Inicio del Algortimo A*
    void EmpezarBuscarRuta()
    {
        terminadoCalculoRuta = false;
        posiciones = new List<PosicionAlmacen>();
        // Crea un listado de localizaciones en el almacén por donde puede pasar el robot
        for (int z = 1; z < almacen.largo - 1; z++)
            for (int x = 1; x < almacen.ancho - 1; x++)
            {
                if (almacen.mapa[x, z] == 0 || almacen.mapa[x, z] == 3 || almacen.mapa[x, z] == 4 || almacen.mapa[x, z] == 5)
                {
                    posiciones.Add(new PosicionAlmacen(x, z));
                }
            }

        // Los mezcla aleatoriamente para escoger uno 
        posiciones.Shuffle();
        posicionDestino = new Vector3(posiciones[0].x * almacen.escala, 0, posiciones[0].z * almacen.escala);
        nodoDestino = new MarcadorRuta(new PosicionAlmacen(posiciones[0].x, posiciones[0].z), 0, 0, 0, Instantiate(destino, posicionDestino, Quaternion.identity), null);
        // Limpiar listas Frontera y Visitados
        frontera.Clear();
        visitados.Clear();

        frontera.Add(nodoOrigen);
        nodoUltimo = nodoOrigen;

        // Bucle para recorrer los nodos del grafo siguiendo el algortimo A*
        while (!terminadoCalculoRuta)
        {
            BuscarSiguenteNodo(nodoUltimo);
        }


        CogerRutaEncontrada();
        posicionOrigen = posicionDestino;
        nodoOrigen = nodoDestino;
        Debug.Log("terminado");
        // Al terminar permitir el movimiento por la ruta
        estaMoviendo = true;

    }


    // Función principal del búsqueda de nodos del Algoritmo A*
    void BuscarSiguenteNodo(MarcadorRuta marcador)
    {
        // Si llega al destino terminar la búsqueda
        if (marcador.Equals(nodoDestino))
        {
            terminadoCalculoRuta = true;
            Debug.Log("Encontrado punto de destino");
            return;
        }

        // Recorrer cada vecino del nodo actual asignando los valores de la función de evaluación 
        foreach (PosicionAlmacen pos in almacen.direcciones)
        {
            PosicionAlmacen vecino = pos + marcador.posicion;
            // No buscar en paredes o estanterias
            if (almacen.mapa[vecino.x, vecino.z] == 1 || almacen.mapa[vecino.x, vecino.z] == 2) continue;
            // No buscar fuera del almacén
            if (vecino.x < 1 || vecino.x > almacen.ancho || vecino.z < 1 || vecino.z > almacen.largo) continue;
            // No buscar si ya fue visitado anteriormente
            if (estaVisitado(vecino)) continue;

            float G = Vector2.Distance(marcador.posicion.ToVector(), vecino.ToVector()) + marcador.G;
            float H = Vector2.Distance(vecino.ToVector(), nodoDestino.posicion.ToVector());
            float F = G + H;

            // Crear marcador en el nodo actual para su visualización
            GameObject puntoRuta = Instantiate(pasoRuta, new Vector3(vecino.x * almacen.escala, 0, vecino.z * almacen.escala), Quaternion.identity);

            // actualiza los valores de la función de evaluación  y lo añade a la frontera si no estaba ya.
            if (!actualizaMarcadores(vecino, G, H, F, marcador))
                frontera.Add(new MarcadorRuta(vecino, G, H, F, puntoRuta, marcador));

        }

        // Escoger nodo con menor valor de f
        frontera = frontera.OrderBy(p => p.F).ToList<MarcadorRuta>();
        MarcadorRuta mRuta = (MarcadorRuta)frontera.ElementAt(0);
        // Añadirlo a nodos visitados y quitarlo de la frontera
        visitados.Add(mRuta);
        frontera.RemoveAt(0);
        nodoUltimo = mRuta;
    }


    // Actualiza los datos de los marcadores
    bool actualizaMarcadores(PosicionAlmacen pos, float g, float h, float f, MarcadorRuta marcador)
    {
        foreach (MarcadorRuta m in frontera)
        {
            if (m.posicion.Equals(pos))
            {
                m.G = g;
                m.H = h;
                m.F = f;
                m.padre = marcador;
                return true;
            }
        }
        return false;
    }

    // Consulta si el nodo se ha visitado
    bool estaVisitado(PosicionAlmacen pos)
    {
        foreach (MarcadorRuta m in visitados)
        {
            if (m.posicion.Equals(pos)) return true;
        }
        return false;
    }

    // Almacena y muestra la ruta más corta calculada
    void CogerRutaEncontrada()
    {

        MarcadorRuta comienzo = nodoUltimo;
        posicionesRuta.Clear();
        while (!nodoOrigen.Equals(comienzo) && comienzo != null)
        {
            Vector3 posicionActual = new Vector3(comienzo.posicion.x * almacen.escala, 0, comienzo.posicion.z * almacen.escala);
            posicionesRuta.Add(posicionActual);
            Instantiate(destino, posicionActual, Quaternion.identity);
            comienzo = comienzo.padre;

        }
        Instantiate(destino, new Vector3(nodoOrigen.posicion.x * almacen.escala, 0, nodoOrigen.posicion.z * almacen.escala), Quaternion.identity);
        posicionesRuta.Reverse();

    }

    // Start is called before the first frame update
    void Start()
    {

        posicionOrigen = robot.transform.position;

        nodoOrigen = new MarcadorRuta(new PosicionAlmacen(13, 4), 0, 0, 0, Instantiate(origen, posicionOrigen, Quaternion.identity), null);

    }

    // Update is called once per frame
    void Update()
    {
        // Comenzar el proceso pulsando 'C'
        if (Input.GetKeyDown(KeyCode.C)) EmpezarBuscarRuta();

        // Dirigirse al destino tras el cálculo de la ruta
        if (estaMoviendo)
        {

            // Si llega al punto de la ruta continuar con el siguiente
            if (Vector3.Distance(robot.transform.position, posicionesRuta[indiceMarcadorActual]) < 4)
                indiceMarcadorActual++;

            // Si llega al final de la ruta empezar de nuevo el proceso global
            if (indiceMarcadorActual >= posicionesRuta.Count())
            {
                indiceMarcadorActual = 0;
                estaMoviendo = false;
                EmpezarBuscarRuta();
            }

            // Girar en dirección del siguiente nodo y avanzar
            Quaternion mirarPuntoRuta = Quaternion.LookRotation(posicionesRuta[indiceMarcadorActual] - robot.transform.position);
            robot.transform.rotation = Quaternion.Slerp(robot.transform.rotation, mirarPuntoRuta, velocidadRotacion * Time.deltaTime);
            robot.transform.Translate(0, 0, velocidad * Time.deltaTime);

        }
    }



}
