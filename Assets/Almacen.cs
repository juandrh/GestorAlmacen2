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
    public int[,] mapa;
    public int escala = 3;
    [SerializeField] private GameObject bloqueSuelo;
    [SerializeField] private GameObject bloquePared;
    [SerializeField] private GameObject bloqueES;
    [SerializeField] private GameObject bloqueRecarga;
    [SerializeField] private GameObject bloqueEstanteria;


    // Start is called before the first frame update
    void Start()
    {
        GenerarMapa();
        DibujarMapa();

    }


    void GenerarMapa()
    {

        mapa = new int[ancho, largo];

        //0 = suelo, 1 = pared, 2 = estanteria, 3- Zona Entrada, 4 = Zona salida, 5 = zona de recarga, 101..= paquete (futuras implementaciones)
        // Generar suelo al mapa        
        for (int z = 1; z < largo - 1; z++)
            for (int x = 1; x < ancho - 1; x++)
            {
                mapa[x, z] = 0;
            }
        // Generar paredes al mapa
        for (int z = 0; z < largo; z++)
        {
            mapa[0, z] = 1;
            mapa[ancho - 1, z] = 1;
        }
        for (int x = 0; x < ancho; x++)
        {
            mapa[x, 0] = 1;
            mapa[x, largo - 1] = 1;
        }
        // Generar estanterias
        int col = 25;
        for (int i = 1; i < 6; i++)
        {

            for (int j = 0; j < 5; j++)
            {
                for (int z = 4; z < 10; z++)
                {
                    mapa[col, z + j * 9] = 2;
                    mapa[col + 1, z + j * 9] = 2;
                }
            }
            col += 5;
        }

        // Generar zonas entrada, salida y recarga
        for (int z = 10; z < 20; z++)
            for (int x = 0; x < 5; x++)
            {
                mapa[x, z] = 3;
            }

        for (int z = 30; z < 40; z++)
            for (int x = 0; x < 5; x++)
            {
                mapa[x, z] = 4;
            }

        for (int x = 10; x < 20; x++)
            for (int z = 1; z < 5; z++)
            {
                mapa[x, z] = 5;
            }


    }


    void DibujarMapa()
    {
        for (int z = 0; z < largo; z++)
            for (int x = 0; x < ancho; x++)
            {
                Vector3 pos = new Vector3(x * escala, 0f, z * escala);
                if (mapa[x, z] == 0)
                {
                    GameObject suelo = Instantiate(bloqueSuelo, pos, Quaternion.identity);
                    suelo.transform.localScale = new Vector3(escala, escala * 0.1f, escala);

                }
                if (mapa[x, z] == 1)
                {
                    GameObject pared = Instantiate(bloquePared, pos, Quaternion.identity);
                    pared.transform.localScale = new Vector3(escala, escala * 4f, escala);
                }
                if (mapa[x, z] == 2)
                {
                    GameObject estanteria = Instantiate(bloqueEstanteria, pos, Quaternion.identity);
                    estanteria.transform.localScale = new Vector3(escala, escala * 2.0f, escala);
                }
                if (mapa[x, z] == 3 || mapa[x, z] == 4)
                {
                    GameObject es = Instantiate(bloqueES, pos, Quaternion.identity);
                    es.transform.localScale = new Vector3(escala, escala * 0.1f, escala);
                }
                if (mapa[x, z] == 5)
                {
                    GameObject recarga = Instantiate(bloqueRecarga, pos, Quaternion.identity);
                    recarga.transform.localScale = new Vector3(escala, escala * 0.1f, escala);
                }
            }
    }



    // Update is called once per frame
    void Update()
    {

    }
}
