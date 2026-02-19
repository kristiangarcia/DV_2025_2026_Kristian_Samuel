using UnityEngine;
using UnityEngine.AI;

public class ZombieNormal : MonoBehaviour
{
    [Header("Configuración")]
    public Transform objetivo;       // El jugador
    public float vida = 100f;
    public float dañoAtaque = 20f;   // Daño por cada golpe/mordisco
    public float velocidadAtaque = 1f; // Segundos entre cada golpe

    private NavMeshAgent agente;
    private float tiempoUltimoAtaque;
    private float tiempoSiguienteCalculo = 0f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Solo perseguimos al jugador si el objetivo existe y SI ESTAMOS PISANDO EL NAVMESH
        if (objetivo != null && agente != null && agente.isOnNavMesh)
        {
            agente.SetDestination(objetivo.position);
        }
    }

    // --- SISTEMA DE DAÑO AL JUGADOR ---
    // Usamos OnCollisionStay para que te siga haciendo daño si se queda pegado a ti
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Comprueba si ha pasado suficiente tiempo desde el último golpe
            if (Time.time >= tiempoUltimoAtaque + velocidadAtaque)
            {
                VidaJugador vidaScript = collision.gameObject.GetComponent<VidaJugador>();
                if (vidaScript != null)
                {
                    vidaScript.RecibirDaño(dañoAtaque);
                    tiempoUltimoAtaque = Time.time; // Reiniciamos el temporizador
                    Debug.Log("¡Un zombie te ha golpeado!");
                }
            }
        }
    }

    // --- SISTEMA PARA MORIR (Lo llama tu arma) ---
    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            // Aquí podrías sumar puntos al jugador si tuvieras un script de Puntuación
            Destroy(gameObject);
        }
    }
}