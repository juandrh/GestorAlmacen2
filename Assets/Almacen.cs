using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// clase auxiliar para alimentar de posiciones 2D al almacén
public class PosicionAlmacen
{
    public int x;
    public int z;

    public PosicionAlmacen(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public static PosicionAlmacen operator +(PosicionAlmacen a, PosicionAlmacen b)
     => new PosicionAlmacen(a.x + b.x, a.z + b.z);

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return x == ((PosicionAlmacen)obj).x && z == ((PosicionAlmacen)obj).z;
        }

    }
    public override int GetHashCode()
    {
        return 0;
    }

}

// Clase nodo que almacena las soluciones parciales del problema
public class Nodo
{

    public List<PosicionAlmacen> posicionesNodo; // posiciones robots en este nodo
    public float valor;
    public Nodo padre;
    public int jugador;
    public int nivel;



    public Nodo(List<PosicionAlmacen> posicionesNodo, int jugador, int nivel, float valor, Nodo p)
    {

        this.posicionesNodo = new List<PosicionAlmacen>(posicionesNodo);
        this.jugador = jugador;
        this.nivel = nivel;
        this.valor = valor;
        padre = p;

    }

    public bool Equals(Nodo obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {

            foreach (PosicionAlmacen p in posicionesNodo)
            {
                if (!p.Equals(obj.posicionesNodo))
                {
                    return false;
                }


            }
            return true;
        }

    }

    public override int GetHashCode()
    {
        return 0;
    }

}

// Clase principal que crea el almacén 
// Realiza la búsqueda con el algoritmo Minimax poda alfa-beta
// Mueve el robot "ganador" hacia el destino
public class Almacen : MonoBehaviour
{

    // Lista de los 8 vecinos de una posición dada
    public List<PosicionAlmacen> direcciones = new List<PosicionAlmacen>() {
                                            new PosicionAlmacen(1,0),
                                            new PosicionAlmacen(0,1),
                                            new PosicionAlmacen(-1,0),
                                            new PosicionAlmacen(0,-1),
                                            new PosicionAlmacen(-1,-1),
                                            new PosicionAlmacen(-1,1),
                                            new PosicionAlmacen(1,1),
                                            new PosicionAlmacen(1,-1)};
    public int ancho = 30;
    public int largo = 30;
    public int[,] plano;
    public int escala = 3;
    [SerializeField] private GameObject bloqueSuelo;
    [SerializeField] private GameObject bloquePared;
    [SerializeField] private GameObject bloqueES;
    [SerializeField] private GameObject bloqueRecarga;
    [SerializeField] private GameObject bloqueEstanteria;
    [SerializeField] private GameObject marcador0;
    [SerializeField] private GameObject marcador1;
    [SerializeField] private GameObject marcadorDestino;
    [SerializeField] private GameObject marcadorOrigen;
    [SerializeField] private GameObject marcadorRuta1;
    [SerializeField] private GameObject marcadorRuta2;
    [SerializeField] private GameObject robot1;
    [SerializeField] private GameObject robot2;
    private PosicionAlmacen destino;
    private List<PosicionAlmacen> posicionesIniciales = new List<PosicionAlmacen>();
    private Nodo nodoInicial;
    private List<Nodo> frontera = new List<Nodo>();
    private List<Nodo> visitados = new List<Nodo>();
    private List<Nodo> soluciones = new List<Nodo>();
    private List<Vector3> ruta = new List<Vector3>();
    private Nodo mejorSolucion;
    private float inf = 100000.0f;
    private bool jugador1Moviendo = false;
    private bool jugador2Moviendo = false;
    int indiceMarcadorActual = 0;
    private float velocidad = 30.0f;
    private float velocidadRotacion = 15.0f;



    // Start is called before the first frame update
    void Start()
    {
        GenerarMapa();
        DibujarMapa();
    }


    void GenerarMapa()
    {
        plano = new int[ancho, largo];
        //0 = suelo, 1 = pared, 2 = estanteria, 3- Zona Entrada, 4 = Zona salida, 5 = zona de recarga, 100 = destino , 101..= posiciones iniciales robots
        // Generar suelo al mapa        
        for (int z = 1; z < largo - 1; z++)
            for (int x = 1; x < ancho - 1; x++)
            {
                plano[x, z] = 0;
            }
        // Generar paredes al mapa
        for (int z = 0; z < largo; z++)
        {
            plano[0, z] = 1;
            plano[ancho - 1, z] = 1;
        }
        for (int x = 0; x < ancho; x++)
        {
            plano[x, 0] = 1;
            plano[x, largo - 1] = 1;
        }
        // Generar estanterias
        int col = 25;
        for (int i = 1; i < 6; i++)
        {

            for (int j = 0; j < 5; j++)
            {
                for (int z = 4; z < 10; z++)
                {
                    plano[col, z + j * 9] = 2;
                    plano[col + 1, z + j * 9] = 2;
                }
            }
            col += 5;
        }

        // Generar zonas entrada, salida y recarga
        for (int z = 10; z < 20; z++)
            for (int x = 0; x < 5; x++)
            {
                plano[x, z] = 3;
            }

        for (int z = 30; z < 40; z++)
            for (int x = 0; x < 5; x++)
            {
                plano[x, z] = 4;
            }

        for (int x = 10; x < 20; x++)
            for (int z = 1; z < 5; z++)
            {
                plano[x, z] = 5;
            }
    }


    void DibujarMapa()
    {
        for (int z = 0; z < largo; z++)
            for (int x = 0; x < ancho; x++)
            {
                Vector3 pos = new Vector3(x * escala, 0f, z * escala);
                if (plano[x, z] == 0)
                {
                    GameObject suelo = Instantiate(bloqueSuelo, pos, Quaternion.identity);
                    suelo.transform.localScale = new Vector3(escala, escala * 0.1f, escala);

                }
                if (plano[x, z] == 1)
                {
                    GameObject pared = Instantiate(bloquePared, pos, Quaternion.identity);
                    pared.transform.localScale = new Vector3(escala, escala * 4f, escala);
                }
                if (plano[x, z] == 2)
                {
                    GameObject estanteria = Instantiate(bloqueEstanteria, pos, Quaternion.identity);
                    estanteria.transform.localScale = new Vector3(escala, escala * 2.0f, escala);
                }
                if (plano[x, z] == 3 || plano[x, z] == 4)
                {
                    GameObject es = Instantiate(bloqueES, pos, Quaternion.identity);
                    es.transform.localScale = new Vector3(escala, escala * 0.1f, escala);
                }
                if (plano[x, z] == 5)
                {
                    GameObject recarga = Instantiate(bloqueRecarga, pos, Quaternion.identity);
                    recarga.transform.localScale = new Vector3(escala, escala * 0.1f, escala);
                }
            }
    }


    // Update is called once per frame
    void Update()
    {
        // Comenzar la búsqueda pulsando 'C'
        if (Input.GetKeyDown(KeyCode.C))
        {
            ejecutaAlgoritmo();
        }

        // Mover robot adecuado cuando se encuentra la solución
        if (jugador1Moviendo)
        {
            // Si llega al punto de la ruta continuar con el siguiente
            if (Vector3.Distance(robot1.transform.position, ruta[indiceMarcadorActual]) < 3)
                indiceMarcadorActual++;
            // Si llega al final de la ruta empezar de nuevo el proceso global
            if (indiceMarcadorActual >= ruta.Count)
            {
                indiceMarcadorActual = 0;
                jugador1Moviendo = false;
            }
            // Girar en dirección del siguiente nodo y avanzar
            Quaternion mirarPuntoRuta = Quaternion.LookRotation(ruta[indiceMarcadorActual] - robot1.transform.position);
            robot1.transform.rotation = Quaternion.Slerp(robot1.transform.rotation, mirarPuntoRuta, velocidadRotacion * Time.deltaTime);
            robot1.transform.Translate(0, 0, velocidad * Time.deltaTime);
        }
        if (jugador2Moviendo)
        {
            // Si llega al punto de la ruta continuar con el siguiente
            if (Vector3.Distance(robot2.transform.position, ruta[indiceMarcadorActual]) < 3)
                indiceMarcadorActual++;
            // Si llega al final de la ruta empezar de nuevo el proceso global
            if (indiceMarcadorActual >= ruta.Count)            {
                indiceMarcadorActual = 0;
                jugador2Moviendo = false;
            }
            // Girar en dirección del siguiente nodo y avanzar
            Quaternion mirarPuntoRuta = Quaternion.LookRotation(ruta[indiceMarcadorActual] - robot2.transform.position);
            robot2.transform.rotation = Quaternion.Slerp(robot2.transform.rotation, mirarPuntoRuta, velocidadRotacion * Time.deltaTime);
            robot2.transform.Translate(0, 0, velocidad * Time.deltaTime);
        }
    }


    // Inicio del proceso
    void ejecutaAlgoritmo()
    {        
        Debug.Log("Empezando!");
        posicionesIniciales.Clear();
        frontera.Clear();
        visitados.Clear();
        soluciones.Clear();
        ruta.Clear();
        mejorSolucion = null;
        posicionesIniciales.Add(new PosicionAlmacen((int)(robot1.transform.position.x / 3), (int)(robot1.transform.position.z / 3)));
        posicionesIniciales.Add(new PosicionAlmacen((int)(robot2.transform.position.x / 3), (int)(robot2.transform.position.z / 3)));
        escogerDestino();
        Instantiate(marcadorDestino, new Vector3(destino.x * escala, 0, destino.z * escala), Quaternion.identity);
        nodoInicial = new Nodo(posicionesIniciales, 0, 1, -inf, null);
        // Comienza el algortimo con una profundidad de búsqueda de 140 niveles
        minimax_alfa_beta(0, nodoInicial, 140, -inf, inf);
        Debug.Log("--> Finalizada busqueda!");       

        if (soluciones.Count > 0)  // para el caso en que haya encontrado soluciones terminales
        {
            int minimo = 10000;  // Se establece un valor muy alto para luego encontrar el mínimo
            foreach (Nodo n in soluciones)
            {
                if (n.nivel < minimo)
                {
                    minimo = n.nivel;
                    mejorSolucion = n;
                }
            }

            Debug.Log("--> Robot ganador ->" + mejorSolucion.padre.jugador);
            // calcula la ruta ganadora y mueve el robot correspondiente
            CogerRutaEncontrada(mejorSolucion.padre.jugador);
            if (mejorSolucion.padre.jugador == 0)
            {
                jugador1Moviendo = true;
            }
            else if (mejorSolucion.padre.jugador == 1)
            {
                jugador2Moviendo = true;
            }
        }
        else   
        {
            Debug.Log("No hay ganador");
        }
    }

    // Escoger una localización de destino al azar.
    void escogerDestino()
    {
        List<PosicionAlmacen> posiciones = new List<PosicionAlmacen>();   

        for (int z = 1; z < largo - 1; z++)
            for (int x = 1; x < ancho - 1; x++)
            {
                // evitar paredes al buscar destino
                if (plano[x, z] == 0 || plano[x, z] == 3 || plano[x, z] == 4 || plano[x, z] == 5)
                {
                    posiciones.Add(new PosicionAlmacen(x, z));
                }
            }

        // Los mezcla aleatoriamente para escoger uno 
        posiciones.Shuffle();
        // Asigna el valor de destino a una posicion de almacén
        plano[posiciones[0].x, posiciones[0].z] = 100;
        destino = new PosicionAlmacen(posiciones[0].x, posiciones[0].z);
    }

    // Algoritmo Poda alfa-beta con negamax
    public float minimax_alfa_beta(int jugador, Nodo nodo, int profundidad, float alfa, float beta)
    {
        // Limita el algoritmo a una profundidad de búsqueda dada para evitar consumir  todos los recursos del ordenador
        if (profundidad < 1)
        {
            return nodo.valor;
        }
        else profundidad--;


        // Recuerda las posiciones ya vistas para no caer en ciclos infinitos
        visitados.Add(nodo);

        // Al llegar a un nodo temrinal devolver el valor de evaluación 
        // Se trata del inverso del número de niveles que ha descendido (longitud del camino)
        // Porque se busca el camino más corto
        if (esTerminal(nodo.posicionesNodo))
        {
            soluciones.Add(nodo);
            if (nodo.nivel == 0)
            {
                return 1;
            }
            else return 1 / nodo.nivel;   // El valor de una solución es el 
        }

        // Crear hijos  -------------
        List<Nodo> hijos = new List<Nodo>();

        foreach (PosicionAlmacen pos in direcciones)
        {
            List<PosicionAlmacen> posicionesHijo = new List<PosicionAlmacen>(nodo.posicionesNodo); // copia posiciones del nodo padre
            PosicionAlmacen posHijo = pos + nodo.posicionesNodo[jugador];   //crea nueva posición para elrobot actual
            // No añadir hijo si son paredes o estanterias
            if (plano[posHijo.x, posHijo.z] == 1 || plano[posHijo.x, posHijo.z] == 2) continue;
            // No añadir hijo si es fuera del almacén
            if (posHijo.x < 1 || posHijo.x > ancho - 1 || posHijo.z < 1 || posHijo.z > largo - 1)
                continue;
            // No añadir hijo si ya esta ocupada la posición por otro robot
            bool saltarHijo = false;
            foreach (PosicionAlmacen posPadre in nodo.posicionesNodo)
            {
                if ((posHijo.x == posPadre.x) && (posHijo.z == posPadre.z))
                {
                    saltarHijo = true;
                    break;
                }
            }
            if (saltarHijo) continue;

            // modificar posicion del robot (jugador) actual
            posicionesHijo[jugador] = posHijo;
            // No añadir hijo si ya está visitado  
            if (estaVisitado(posicionesHijo, jugador)) continue;

            if (jugador == 0)
            {
                hijos.Add(new Nodo(posicionesHijo, 1, nodo.nivel + 1, inf, nodo));
                // añade marcador en pantalla
                Instantiate(marcador0, new Vector3(posHijo.x * escala - 2, 0, posHijo.z * escala - 2), Quaternion.identity);
            }
            else
            {
                hijos.Add(new Nodo(posicionesHijo, 0, nodo.nivel + 1, -inf, nodo));
                Instantiate(marcador1, new Vector3(posHijo.x * escala + 2, 0, posHijo.z * escala + 2), Quaternion.identity);
            }


        }
        // Fin crear hijos -----------------

        // Si se llega a un callejón sin salida marcar como peor solución

        if (hijos.Count == 0)
        {
            if (jugador == 0)
            {
                return -inf;
            }
            else
            {
                return inf;
            }

        }
        if (jugador == 0)
        {
                      
            float mejorAlfa = -inf;

            for (int i = 0; i < hijos.Count; i++)
            {
                float puntuacion = minimax_alfa_beta(1, hijos[i], profundidad, alfa, beta);
                hijos[i].valor = puntuacion;
                if (puntuacion > mejorAlfa)
                {
                    mejorAlfa = puntuacion;
                }
                if (mejorAlfa > alfa)
                {
                    alfa = mejorAlfa;
                }
                if (beta <= alfa)
                {
                    break;  // podar rama 
                }

            }
            return mejorAlfa;

        }
        else
        {
            float mejorBeta = inf;

            for (int i = 0; i < hijos.Count; i++)
            {
                float puntuacion = minimax_alfa_beta(0, hijos[i], profundidad, alfa, beta);
                hijos[i].valor = puntuacion;
                if (puntuacion < mejorBeta)
                {
                    mejorBeta = puntuacion;
                }
                if (mejorBeta < beta)
                {
                    beta = mejorBeta;
                }
                if (beta <= alfa)
                {
                    break; // podar rama 
                }

            }
            return mejorBeta;
        }
    }

    // Calcula si es terminal, es decir, si ha llegado al destino
    bool esTerminal(List<PosicionAlmacen> listaPosiciones)
    {
        float minimo = 1000000.0f;
        int indiceNodoMasCerca = 0;
        PosicionAlmacen posMinima = new PosicionAlmacen(0, 0);
        foreach (PosicionAlmacen pos in listaPosiciones)
        {
            float temp = (float)((pos.x - destino.x) * (pos.x - destino.x) + (pos.z - destino.z) * (pos.z - destino.z));

            if (temp < minimo)
            {
                minimo = temp;
                posMinima = pos;
                indiceNodoMasCerca = listaPosiciones.IndexOf(pos);
            }
        }
        if (minimo < 7.1f)  // distancia al destino
        {
            Debug.Log("LLegado a destino.");
            Instantiate(marcadorOrigen, new Vector3(posMinima.x * escala, 0, posMinima.z * escala), Quaternion.identity);
            return true;
        }
        else
        {
            return false;
        }
    }

    bool estaVisitado(List<PosicionAlmacen> listaPosiciones, int jugador)
    {
        foreach (Nodo n in visitados)
        {
            if ((n.posicionesNodo[jugador].x == listaPosiciones[jugador].x) && (n.posicionesNodo[jugador].z == listaPosiciones[jugador].z))
            {
                return true;
            }
        }
        return false;
    }


    // Reconstruir la ruta para marcarla en pantalla y mover el robot.
    // se recosntruye desde el último nodo hasta el comienzo a través de los padres de cada nodo
    void CogerRutaEncontrada(int jugador)
    {
        Nodo comienzo = mejorSolucion;
        ruta.Clear();
        while (!nodoInicial.Equals(comienzo) && comienzo != null)
        {
            Vector3 posicionActual = new Vector3(comienzo.posicionesNodo[jugador].x * escala, 0, comienzo.posicionesNodo[jugador].z * escala);
            ruta.Add(posicionActual);
            if (jugador == 0)
            {
                Instantiate(marcadorRuta1, posicionActual, Quaternion.identity);
            }
            else
            {
                Instantiate(marcadorRuta2, posicionActual, Quaternion.identity);
            }
            comienzo = comienzo.padre;
        }
        // Una vez encontrada invertir la ruta para que el robot la siga
        ruta.Reverse();
    }

}
