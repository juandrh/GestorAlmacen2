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
    public float alfa;
    public float beta;
    public float valor;
    public GameObject marcador;
    public Nodo padre;
    public int jugador;

    

    public Nodo(List<PosicionAlmacen> posicionesNodo, int j,float a, float b, float v,  Nodo p)
    {
      
        this.posicionesNodo = new List<PosicionAlmacen>(posicionesNodo);
        jugador = j;
        alfa = a;
        beta = b;
        valor = v;        
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
                if(!p.Equals(obj.posicionesNodo)){
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
   [SerializeField]  private GameObject robot1 ;
   [SerializeField]  private GameObject robot2 ;
    private PosicionAlmacen destino; 
    private List<PosicionAlmacen> posicionesIniciales = new List<PosicionAlmacen>();  
    private List<Nodo> frontera = new List<Nodo>(); 
    private List<Nodo> visitados = new List<Nodo>(); 
    private int jugador;

    private int contador = 0;

   // private  int ganador;
    //private int numRobots= 2;
    //private bool terminadoCalculo = false;

  

    // Start is called before the first frame update
    void Start()
    {
        GenerarMapa();
        DibujarMapa();
        
        posicionesIniciales.Add(new PosicionAlmacen ((int)(robot1.transform.position.x/3),(int)(robot1.transform.position.z/3)));
        posicionesIniciales.Add(new PosicionAlmacen ((int)robot2.transform.position.x/3,(int)robot2.transform.position.z/3));
        escogerDestino();
        // Añadir primer nodo al grafo
        frontera.Add(new Nodo(posicionesIniciales,0,0.0f,0.0f,0.0f,null));
        
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
               Negamax_alfa_beta(frontera[0], 0.0f, 0.0f) ;
            }
           //Debug.Log(robot1.transform.position.x);

           
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
        plano[posiciones[0].x,posiciones[0].z]= 100;
        destino = new PosicionAlmacen(posiciones[0].x,posiciones[0].z);
        Instantiate(marcadorDestino, new Vector3(destino.x*escala,0,destino.z*escala), Quaternion.identity);
    }

    // Algoritmo Poda alfa-beta con negamax
  
    public float Negamax_alfa_beta(Nodo nodo, float alfa, float beta)
    {
        
            if (contador ==1000) return 0.0f;;
         int jugadorActual;
         if(nodo.jugador==posicionesIniciales.Count-1)
         { jugadorActual = 0;
         } else{
            jugadorActual = nodo.jugador+1;
         }
         
         //if (plano[x, z] == 0 || plano[x, z] == 3 || plano[x, z] == 4 || plano[x, z] == 5)
        
        if (esTerminal(nodo) || esFrontera(nodo)){
            return 0.0f;  // ------------------------------------------cambiar -----------------
        }
        List<Nodo> hijos = new List<Nodo>(); 

        List<PosicionAlmacen> posicionesHijo = new List<PosicionAlmacen>(nodo.posicionesNodo);
        foreach (PosicionAlmacen pos in direcciones)
        {
            
            PosicionAlmacen hijo = pos + nodo.posicionesNodo[jugadorActual];
            // No buscar en paredes o estanterias
            if (plano[hijo.x, hijo.z] == 1 || plano[hijo.x, hijo.z] == 2) continue;
            // No buscar fuera del almacén
            if (hijo.x < 1 || hijo.x > ancho || hijo.z < 1 || hijo.z > largo)
             continue;
            // No buscar si ya esta en frontera
            if (estaVisitado(new Nodo(posicionesHijo,0,0.0f,0.0f,0.0f,null))) continue;

            posicionesHijo[jugadorActual]= hijo;
            
            hijos.Add(new Nodo(posicionesHijo,jugadorActual,alfa, beta,0.0f,nodo));
            frontera.Add(new Nodo(posicionesHijo,jugadorActual,alfa, beta,0.0f,nodo));
            Instantiate(marcador, new Vector3(hijo.x*escala,0,hijo.z*escala), Quaternion.identity);
        }
        foreach (Nodo n in hijos)
        {
            
            float puntuacion = - Negamax_alfa_beta(n, -beta,-alfa);
            if(puntuacion>alfa){
                alfa = puntuacion;            
            }
            if(alfa >= beta){
               return alfa; 
            }
        }
        return alfa;
    }  
    
    float FuncionEvaluacion(Nodo nodo)
    {
      
       float minimo = 1000000.0f;
       int indiceNodoMasCerca;
       foreach (PosicionAlmacen pos in nodo.posicionesNodo)      
      {
        float temp = (pos.x-destino.x)*(pos.x-destino.x) + (pos.z-destino.z)*(pos.z-destino.z);
        
        if(temp<minimo){
            minimo = temp;
            indiceNodoMasCerca = nodo.posicionesNodo.IndexOf(pos);
        }
      }
       
       
        
      //  rehacer en funcion de algoritmo
        
      
      if (minimo==0){
        return 0;
      } else  return 1/minimo;
     
    }

        


    bool esTerminal(Nodo nodo)
    {
          
        float minimo = 1000000.0f;
       int indiceNodoMasCerca;
       foreach (PosicionAlmacen pos in nodo.posicionesNodo)      
      {
        float temp = (pos.x-destino.x)*(pos.x-destino.x) + (pos.z-destino.z)*(pos.z-destino.z);
        
        if(temp<minimo){
            minimo = temp;
            indiceNodoMasCerca = nodo.posicionesNodo.IndexOf(pos);
        }
      }
       
        
      //  rehacer en funcion del algoritmo
        
      
      if (minimo==0){
        return true;
      } else  return false;
     
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
