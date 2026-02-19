using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GeneradorHordas : MonoBehaviour
{
    [Header("Prefabs de Zombies")]
    public GameObject prefabZombie;           // Zombie normal
    public GameObject prefabZombieKamikaze;   // Zombie kamikaze (desde ronda 5)
    public Transform jugador;

    [Header("Configuración de Rondas")]
    public int zombiesRondaInicial = 5;       // Zombies en la ronda 1
    public int zombiesExtraPorRonda = 3;      // Cuántos más por cada ronda
    public float tiempoEntreSpawns = 0.5f;

    [Header("Área de Búsqueda")]
    public float radioDelMapa = 50f;

    // Estado interno
    private int rondaActual = 0;
    private int zombiesSpawneados = 0;
    private int zombiesRestantes = 0;
    private bool rondaEnCurso = false;
    private bool esperandoEntreRondas = false;
    private float vidaExtraZombie = 0f;

    void Start()
    {
        // Añadir componentes de UI si no existen
        if (FindFirstObjectByType<PantallaRonda>() == null)
            gameObject.AddComponent<PantallaRonda>();
        if (FindFirstObjectByType<HUDRonda>() == null)
            gameObject.AddComponent<HUDRonda>();
        if (FindFirstObjectByType<PantallaPausa>() == null)
            gameObject.AddComponent<PantallaPausa>();

        // Iniciar la primera ronda tras 2 segundos
        StartCoroutine(IniciarPrimeraRonda());
    }

    IEnumerator IniciarPrimeraRonda()
    {
        yield return new WaitForSeconds(2f);
        IniciarRonda();
    }

    void IniciarRonda()
    {
        rondaActual++;
        int totalZombies = zombiesRondaInicial + (rondaActual - 1) * zombiesExtraPorRonda;
        zombiesSpawneados = 0;
        zombiesRestantes = totalZombies;
        rondaEnCurso = true;
        esperandoEntreRondas = false;

        // Incrementar vida extra de zombies (5 HP extra por ronda, empezando en ronda 2)
        vidaExtraZombie = (rondaActual - 1) * 5f;

        // Actualizar HUD con número de ronda
        if (HUDRonda.Instancia != null)
            HUDRonda.Instancia.ActualizarRonda(rondaActual);

        Debug.Log("[RONDA " + rondaActual + "] Empieza con " + totalZombies + " zombies. Vida extra: +" + vidaExtraZombie);

        StartCoroutine(SpawnearZombiesRonda(totalZombies));
    }

    IEnumerator SpawnearZombiesRonda(int total)
    {
        while (zombiesSpawneados < total)
        {
            Vector2 puntoPlano = Random.insideUnitCircle * radioDelMapa;
            Vector3 puntoAleatorio = transform.position + new Vector3(puntoPlano.x, 0, puntoPlano.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(puntoAleatorio, out hit, 10f, NavMesh.AllAreas))
            {
                // Decidir qué tipo de zombie spawnear
                bool esKamikaze = false;
                if (rondaActual >= 5 && prefabZombieKamikaze != null)
                {
                    // Probabilidad de kamikaze: 20% en ronda 5, +5% por ronda
                    float probKamikaze = 0.20f + (rondaActual - 5) * 0.05f;
                    probKamikaze = Mathf.Min(probKamikaze, 0.5f); // Máximo 50%
                    esKamikaze = Random.value < probKamikaze;
                }

                GameObject prefab = esKamikaze ? prefabZombieKamikaze : prefabZombie;
                Vector3 posicionSegura = hit.position + Vector3.up;
                GameObject nuevoZombie = Instantiate(prefab, posicionSegura, Quaternion.identity);

                // Forzar NavMesh
                NavMeshAgent agente = nuevoZombie.GetComponent<NavMeshAgent>();
                if (agente != null)
                    agente.Warp(hit.position);

                // Configurar objetivo y vida extra
                if (esKamikaze)
                {
                    ZombieKamikaze scriptK = nuevoZombie.GetComponent<ZombieKamikaze>();
                    if (scriptK != null)
                    {
                        scriptK.objetivo = jugador;
                        scriptK.vida += vidaExtraZombie;
                    }
                }
                else
                {
                    ZombieNormal scriptN = nuevoZombie.GetComponent<ZombieNormal>();
                    if (scriptN != null)
                    {
                        scriptN.objetivo = jugador;
                        scriptN.vida += vidaExtraZombie;
                    }
                }

                zombiesSpawneados++;
                yield return new WaitForSeconds(tiempoEntreSpawns);
            }
            else
            {
                yield return null;
            }
        }
    }

    void Update()
    {
        if (!rondaEnCurso || esperandoEntreRondas) return;

        // Comprobar si todos los zombies han muerto
        // Solo comprobar si ya hemos spawneado todos
        int totalRonda = zombiesRondaInicial + (rondaActual - 1) * zombiesExtraPorRonda;
        if (zombiesSpawneados >= totalRonda)
        {
            GameObject[] zombiesVivos = GameObject.FindGameObjectsWithTag("Zombie");

            if (zombiesVivos.Length == 0)
            {
                // ¡Ronda completada!
                rondaEnCurso = false;
                esperandoEntreRondas = true;
                MostrarPantallaRonda();
            }
        }
    }

    void MostrarPantallaRonda()
    {
        Debug.Log("[RONDA " + rondaActual + " COMPLETADA]");

        PantallaRonda pantalla = FindFirstObjectByType<PantallaRonda>();
        if (pantalla == null)
        {
            GameObject obj = new GameObject("PantallaRonda");
            pantalla = obj.AddComponent<PantallaRonda>();
        }

        // Mostrar pantalla de la SIGUIENTE ronda
        pantalla.MostrarRonda(rondaActual + 1, () =>
        {
            // Callback: cuando termine la animación, iniciar siguiente ronda
            IniciarRonda();
        });
    }
}