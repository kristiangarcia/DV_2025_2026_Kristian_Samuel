using UnityEngine;
using UnityEngine.AI; // Necesario para la Inteligencia Artificial

public class ZombieKamikaze : MonoBehaviour
{
    public Transform objetivo;         // Aquí arrastraremos a tu Jugador
    public GameObject efectoExplosion; // Aquí pondremos el efecto visual (opcional)
    
    private NavMeshAgent agente;

    void Start()
    {
        // Cogemos el componente NavMeshAgent automáticamente
        agente = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Si el objetivo existe, le decimos al agente que vaya hacia él
        if (objetivo != null)
        {
            agente.SetDestination(objetivo.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Si entra en el trigger alguien con el Tag "Player"
        if (other.CompareTag("Player"))
        {
            Explotar();
        }
    }

    void Explotar()
    {
        // 1. Mostrar explosión (si tenemos una puesta)
        if (efectoExplosion != null)
        {
            Instantiate(efectoExplosion, transform.position, transform.rotation);
        }

        // 2. Destruir al zombie
        Destroy(gameObject);

        // ¡AQUÍ PODRÍAS RESTAR VIDA! 
        // Por ejemplo: collision.gameObject.SendMessage("RecibirDaño", 50);
        Debug.Log("¡BOOM! El zombie ha explotado contra el jugador.");
    }
}