using UnityEngine;
using System.Collections.Generic;

public class GeneradorCoberturas : MonoBehaviour
{
    public GameObject[] coverPrefabs;
    public int minCovers = 6;
    public int maxCovers = 12;
    public Vector2 areaSize = new Vector2(18f, 18f);
    public float separacionMinima = 3.5f;

    private List<GameObject> coberturas = new List<GameObject>();

    //Se generan x coberturas, si choca

    public void ResetCovers()
    {
        foreach (var c in coberturas) if (c != null) Destroy(c);
        coberturas.Clear();

        int target = Random.Range(minCovers, maxCovers + 1);//Número aleatorio en rango
        //Prueba hasta 60 intentos de poner coberturas lejos de jugador
        for (int i = 0; i < 60 && coberturas.Count < target; i++){
            Vector3 pos = new Vector3(Random.Range(-areaSize.x/2, areaSize.x/2), 0, Random.Range(-areaSize.y/2, areaSize.y/2));
            if (posicSegura(pos)){
                GameObject go = Instantiate(coverPrefabs[Random.Range(0, coverPrefabs.Length)], pos, Quaternion.Euler(0, Random.Range(0, 360), 0));
                go.tag = "Obstaculos";//Para asegurarnos, aunque el prefab ya tiene ese tag
                coberturas.Add(go);
            }
        }
    }

    //Genera cobertura que NO choque con jugador
    private bool posicSegura(Vector3 pos){
        foreach (var c in coberturas)
            if (Vector3.Distance(pos, c.transform.position) < separacionMinima) return false;
        return true; 
    }
}