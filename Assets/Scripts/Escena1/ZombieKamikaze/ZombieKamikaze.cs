using UnityEngine;
using UnityEngine.AI;

public class ZombieKamikaze : MonoBehaviour
{
    [Header("Configuración Zombie")]
    public Transform objetivo;
    public float vida = 100f;
    public float dañoExplosion = 50f;
    public GameObject efectoExplosion;
    
    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (objetivo != null)
        {
            agente.SetDestination(objetivo.position);
        }
    }

    // ---------------------------------------------------------
    // CAMBIO IMPORTANTE: USAMOS COLLISION ENTER (CHOQUE FÍSICO)
    // ---------------------------------------------------------
    void OnCollisionEnter(Collision collision)
    {
        // SI CHOCO CONTRA EL JUGADOR -> EXPLOTO
        if (collision.gameObject.CompareTag("Player"))
        {
            ExplotarAtaque(collision.gameObject);
        }
        // Nota: Las balas no usan colisión física, usan el script de disparo,
        // así que no necesitamos mirar "ProyectilBala" aquí si usas Raycast.
    }

    // Esta función la llama TU PISTOLA desde el script "SistemaDisparo"
    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;
        Debug.Log("Zombie herido. Vida: " + vida);

        if (vida <= 0)
        {
            Destroy(gameObject);
        }
    }

    void ExplotarAtaque(GameObject jugador)
    {
        if (efectoExplosion != null)
        {
            Instantiate(efectoExplosion, transform.position, transform.rotation);
        }

        VidaJugador vidaScript = jugador.GetComponent<VidaJugador>();
        if (vidaScript != null)
        {
            vidaScript.RecibirDaño(dañoExplosion);
        }

        Destroy(gameObject);
    }
}