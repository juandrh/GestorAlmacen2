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

    public Vector2 ToVector()
    {
        return new Vector2(x, z);
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
    public List<float> valores = new List<float>();
    public GameObject marcador;
    public Nodo padre;
    public int jugador;



    public Nodo(List<PosicionAlmacen> posicionesNodo, int j, List<float> valores, Nodo p)
    {

        this.posicionesNodo = new List<PosicionAlmacen>(posicionesNodo);
        jugador = j;
        this.valores = valores;
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

// Clase principal que crea el almacén y marca cada cuadrante según sea
// suelo, pared, Zona Entrada, Zona salida y zona de recarga
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

    [SerializeField] private GameObject marcador;
    [SerializeField] private GameObject marcadorDestino;
    [SerializeField] private GameObject robot1;
    [SerializeField] private GameObject robot2;
    private PosicionAlmacen destino;
    private List<PosicionAlmacen> posicionesIniciales = new List<PosicionAlmacen>();
    private List<Nodo> frontera = new List<Nodo>();
    private List<Nodo> visitados = new List<Nodo>();
    private List<float> valoresIniciales = new List<float>();
    private int jugador;

    //private int contador = 0;

    // private  int ganador;
    //private int numRobots= 2;
    //private bool terminadoCalculo = false;



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
        // Comenzar el proceso pulsando 'C'
        if (Input.GetKeyDown(KeyCode.C))
        {
           ejecutaAlgoritmo();
        }
            }

    void ejecutaAlgoritmo()
    {
        
        valoresIniciales.Clear();
        posicionesIniciales.Clear();
        frontera.Clear();

        
        valoresIniciales.Add(0.0f);
        valoresIniciales.Add(0.0f);
        posicionesIniciales.Add(new PosicionAlmacen((int)(robot1.transform.position.x / 3), (int)(robot1.transform.position.z / 3)));
        posicionesIniciales.Add(new PosicionAlmacen((int)(robot2.transform.position.x / 3), (int)(robot2.transform.position.z / 3)));
        escogerDestino();
        Instantiate(marcadorDestino, new Vector3(destino.x * escala, 0, destino.z * escala), Quaternion.identity);
        // Añadir primer nodo al grafo       
        frontera.Add(new Nodo(posicionesIniciales, 0, valoresIniciales, null));        
        Nodo nodo= Negamax_alfa_beta(frontera[0]);

        

    }


    void escogerDestino()
    {
        List<PosicionAlmacen> posiciones = new List<PosicionAlmacen>();
        // 

        for (int z = 1; z < largo - 1; z++)
            for (int x = 1; x < ancho - 1; x++)
            {
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

    public Nodo Negamax_alfa_beta(Nodo nodo)
    {

        
        if (esTerminal(nodo) || esFrontera(nodo))
        {
            return new Nodo(nodo.posicionesNodo,nodo.jugador,FuncionEvaluacion(nodo.posicionesNodo),nodo.padre);  
        }
        
        
        
        // Actualizar jugador
        //if (contador == 10) return nodo; 
        int jugadorActual;
        if (nodo.jugador == posicionesIniciales.Count-1)
        {
            jugadorActual = 0;
        }
        else
        {
            jugadorActual = nodo.jugador + 1;
        }


        
        List<Nodo> hijos = new List<Nodo>();
        List<PosicionAlmacen> posicionesHijo = new List<PosicionAlmacen>(nodo.posicionesNodo);
        foreach (PosicionAlmacen pos in direcciones)
        {

            PosicionAlmacen posHijo = pos + nodo.posicionesNodo[jugadorActual];
            // No añadir hijo si son paredes o estanterias
            if (plano[posHijo.x, posHijo.z] == 1 || plano[posHijo.x, posHijo.z] == 2) continue;
            // No añadir hijo si es fuera del almacén
            if (posHijo.x < 1 || posHijo.x > ancho || posHijo.z < 1 || posHijo.z > largo)
                continue;
            // No añadir hijo si ya esta ocupada la posición por otro robot
            bool saltarNodo =false;
           foreach (PosicionAlmacen posAl in nodo.posicionesNodo)
           {
               if ((posHijo.x == posAl.x) && (posHijo.z == posAl.z)){
                saltarNodo  = true;
                break;
               }

           }
            if(saltarNodo) continue;

            // No añadir hijo si ya es frontera o está visitado
            if (esFrontera(new Nodo(posicionesHijo, 0, nodo.valores, null))) continue;
            if (estaVisitado(new Nodo(posicionesHijo, 0, nodo.valores, null))) continue;


            // Modificar la posicion del robot
            posicionesHijo[jugadorActual] = posHijo;            
            Nodo nuevoNodo = new Nodo(posicionesHijo, jugadorActual,FuncionEvaluacion(posicionesHijo), nodo);
            hijos.Add(nuevoNodo);
            frontera.Add(nuevoNodo);
            Instantiate(marcador, new Vector3(posHijo.x * escala, 0, posHijo.z * escala), Quaternion.identity);
           
        }
        
        int mejorHijo=0; 
        float valorHijo=100000.0f;
        for (int i=0; i< hijos.Count; i++)
        {
            if (hijos[i].valores[jugadorActual]<valorHijo)
            {
                valorHijo = hijos[i].valores[jugadorActual];
                mejorHijo = i;
            }    

        }


        return Negamax_alfa_beta(hijos[mejorHijo]);
    }

    List<float> FuncionEvaluacion(List<PosicionAlmacen> p)
    {
        List<float> val = new List<float>();
        //float minimo = 1000000.0f;
        //int indiceNodoMasCerca;
        for (int i = 0; i < p.Count; i++)
        // foreach (PosicionAlmacen pos in nodo.posicionesNodo)      
        {
            val.Add((p[i].x - destino.x) * (p[i].x - destino.x) + (p[i].z - destino.z) * (p[i].z - destino.z));

            /* if(temp<minimo){
                minimo = temp;
                indiceNodoMasCerca = nodo.posicionesNodo.IndexOf(pos);
            } */
        }
        return val;


        //  rehacer en funcion de algoritmo


        /* if (minimo==0){
          return 0;
        } else  return 1/minimo; */

    }

    


    bool esTerminal(Nodo nodo)
    {

        float minimo = 1000000.0f;
        int indiceNodoMasCerca;
        foreach (PosicionAlmacen pos in nodo.posicionesNodo)
        {
            float temp = (pos.x - destino.x) * (pos.x - destino.x) + (pos.z - destino.z) * (pos.z - destino.z);

            if (temp < minimo)
            {
                minimo = temp;
                indiceNodoMasCerca = nodo.posicionesNodo.IndexOf(pos);
            }
        }


        //  rehacer en funcion del algoritmo


        if (minimo == 0)
        {
            return true;
        }
        else return false;

    }




    bool esFrontera(Nodo nodo)
    {
        foreach (Nodo n in frontera)
        {
            if (nodo.Equals(n)) return true;
        }
        return false;
    }

    bool estaVisitado(Nodo nodo)
    {
        foreach (Nodo n in visitados)
        {
            if (nodo.Equals(n)) return true;
        }
        return false;
    }




}
