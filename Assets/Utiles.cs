using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase para mezclar aleatoriamente los elementos de una lista
// Se utiliza para elegir aleatoriamente los nodos de destino 
// Fuente: https://learn.unity.com/
public static class Utiles
{
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
