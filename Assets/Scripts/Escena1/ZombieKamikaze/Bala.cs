using UnityEngine;

public class Bala : MonoBehaviour
{
    [Header("Configuración")]
    public float daño = 20f; // Este número lo cambiaremos en Unity según el arma

    void Start()
    {
        // Importante: Que la bala se destruya sola a los 3 segundos si no da a nada
        // para no llenar el juego de basura.
        Destroy(gameObject, 3f);
    }

    // Usamos OnTriggerEnter porque el Zombie tiene "Is Trigger" activado
    void OnTriggerEnter(Collider other)
    {
        // 1. Intentamos sacar el script del Zombie del objeto que hemos tocado
        ZombieKamikaze zombie = other.GetComponent<ZombieKamikaze>();

        if (zombie != null)
        {
            // 2. Si es un zombie, le hacemos daño
            zombie.RecibirDaño(daño);
            
            // 3. Destruimos la bala (para que no lo atraviese y mate al de atrás)
            Destroy(gameObject);
        }
        // 4. Si chocamos con una pared o suelo (algo que no sea el jugador ni otras balas)
        else if (!other.CompareTag("Player") && !other.CompareTag("Bala"))
        {
            Destroy(gameObject);
        }
    }
}