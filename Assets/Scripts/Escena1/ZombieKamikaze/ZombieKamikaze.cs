using UnityEngine;
using UnityEngine.AI;

public class ZombieKamikaze : MonoBehaviour
{
    [Header("Configuración Zombie")]
    public Transform objetivo;
    public float vida = 100f;
    public float vidaMaxima = 100f;
    public float dañoExplosion = 50f;
    public GameObject efectoExplosion;

    private NavMeshAgent agente;
    private Animator animator;
    private bool estaMuerto = false;

    // Para el efecto de sangre
    private Renderer[] renderers;
    private Color[] coloresOriginales;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        vidaMaxima = vida;

        // Guardar colores originales
        renderers = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                coloresOriginales[i] = renderers[i].material.color;
            else if (renderers[i].material.HasProperty("_BaseColor"))
                coloresOriginales[i] = renderers[i].material.GetColor("_BaseColor");
            else
                coloresOriginales[i] = Color.white;
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        if (objetivo != null && agente != null && agente.isOnNavMesh)
        {
            agente.SetDestination(objetivo.position);

            // Velocidad para animaciones
            if (animator != null)
            {
                float velocidad = agente.velocity.magnitude;
                animator.SetFloat("Velocidad", velocidad);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (estaMuerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            ExplotarAtaque(collision.gameObject);
        }
    }

    public void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;

        vida -= cantidad;

        // Efecto de sangre
        ActualizarColorSangre();

        // Forzar animación de golpe
        if (animator != null && vida > 0)
            animator.CrossFade("HitReaction", 0.1f);

        if (vida <= 0)
        {
            Morir();
        }
    }

    void ActualizarColorSangre()
    {
        float porcentajeVida = Mathf.Clamp01(vida / vidaMaxima);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Color colorSangre = new Color(0.6f, 0.05f, 0.02f, 1f);
            Color colorFinal = Color.Lerp(colorSangre, coloresOriginales[i], porcentajeVida);

            if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = colorFinal;
            else if (renderers[i].material.HasProperty("_BaseColor"))
                renderers[i].material.SetColor("_BaseColor", colorFinal);
        }
    }

    void Morir()
    {
        estaMuerto = true;

        if (animator != null)
            animator.CrossFade("Death", 0.1f);

        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 2.5f);
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