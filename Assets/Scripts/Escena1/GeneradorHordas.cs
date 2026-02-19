using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GeneradorHordas : MonoBehaviour
{
    [Header("Configuración de la Horda")]
    public GameObject prefabZombie;       
    public Transform jugador;             
    public int maxZombiesEnMapa = 30;     
    public float tiempoEntreSpawns = 0.5f; 

    [Header("Área de Búsqueda")]
    public float radioDelMapa = 50f;      

    void Start()
    {
        StartCoroutine(GenerarZombies());
    }

    IEnumerator GenerarZombies()
    {
        while (true)
        {
            GameObject[] zombiesVivos = GameObject.FindGameObjectsWithTag("Zombie");

            if (zombiesVivos.Length < maxZombiesEnMapa)
            {
                // Buscamos un punto aleatorio dentro del círculo
                Vector2 puntoPlano = Random.insideUnitCircle * radioDelMapa;
                Vector3 puntoAleatorio = transform.position + new Vector3(puntoPlano.x, 0, puntoPlano.y);
                
                NavMeshHit hit;
                
                // Si encontramos suelo azul EXACTO...
                if (NavMesh.SamplePosition(puntoAleatorio, out hit, 10f, NavMesh.AllAreas))
                {
                    // 1. Creamos el zombie un poco más arriba para evitar choques con el suelo al nacer
                    Vector3 posicionSegura = hit.position + Vector3.up; 
                    GameObject nuevoZombie = Instantiate(prefabZombie, posicionSegura, Quaternion.identity);

                    // 2. FORZAMOS al NavMeshAgent a pegarse al suelo azul (Esto evita las estatuas y tirones)
                    NavMeshAgent agente = nuevoZombie.GetComponent<NavMeshAgent>();
                    if (agente != null)
                    {
                        agente.Warp(hit.position);
                    }

                    // 3. Le damos la orden de atacar
                    ZombieNormal scriptZombie = nuevoZombie.GetComponent<ZombieNormal>();
                    if (scriptZombie != null)
                    {
                        scriptZombie.objetivo = jugador;
                    }

                    yield return new WaitForSeconds(tiempoEntreSpawns);
                }
                else
                {
                    yield return null; 
                }
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}