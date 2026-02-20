using UnityEngine;
using UnityEngine.AI;

public class ZombieNormal : MonoBehaviour
{
    [Header("Configuración")]
    public Transform objetivo;       // El jugador
    public float vida = 100f;
    public float vidaMaxima = 100f;
    public float dañoAtaque = 20f;   // Daño por cada golpe
    public float velocidadAtaque = 1f; // Segundos entre cada golpe
    public float distanciaAtaque = 2.5f; // Distancia para atacar

    [Header("Sonidos")]
    [Tooltip("Sonidos de muerte (Zombie_dead_1, Zombie_dead_2).")]
    public AudioClip[] sonidosMuerte;
    [Tooltip("Sonidos de golpe al jugador (Zombie_hit_1, 2, 3).")]
    public AudioClip[] sonidosGolpe;

    private NavMeshAgent agente;
    private Animator animator;
    private float tiempoUltimoAtaque;
    private bool estaMuerto = false;

    // Para el efecto de sangre en el material
    private Renderer[] renderers;
    private Color[] coloresOriginales;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Si no hay Animator, intentar buscarlo en hijos
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        vidaMaxima = vida;

        // Guardar colores originales para efecto de sangre
        renderers = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderers.Length];
        propBlock = new MaterialPropertyBlock();

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
            float distancia = Vector3.Distance(transform.position, objetivo.position);

            // Perseguir al jugador siempre
            agente.SetDestination(objetivo.position);

            // Velocidad actual para animaciones walk/run
            if (animator != null)
            {
                float velocidad = agente.velocity.magnitude;
                animator.SetFloat("Velocidad", velocidad);
            }
        }
    }

    // --- SISTEMA DE DAÑO AL JUGADOR ---
    void OnCollisionStay(Collision collision)
    {
        if (estaMuerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= tiempoUltimoAtaque + velocidadAtaque)
            {
                VidaJugador vidaScript = collision.gameObject.GetComponent<VidaJugador>();
                if (vidaScript != null)
                {
                    vidaScript.RecibirDaño(dañoAtaque);
                    tiempoUltimoAtaque = Time.time;
                    ReproducirSonidoAleatorio(sonidosGolpe);
                }
            }
        }
    }

    // --- SISTEMA PARA MORIR ---
    public void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;

        vida -= cantidad;

        // Efecto de sangre: teñir de rojo según vida restante
        ActualizarColorSangre();

        // Forzar animación de golpe directamente
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

            // Cuanto menos vida, más rojo
            // Interpolamos del color original hacia ROJO SANGRE
            Color colorSangre = new Color(0.6f, 0.05f, 0.02f, 1f);
            Color colorFinal = Color.Lerp(colorSangre, coloresOriginales[i], porcentajeVida);

            // Aplicar al material
            if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = colorFinal;
            else if (renderers[i].material.HasProperty("_BaseColor"))
                renderers[i].material.SetColor("_BaseColor", colorFinal);
        }
    }

    void ReproducirSonidoAleatorio(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
        {
            float vol = PlayerPrefs.GetFloat("VolumenEfectos", 80f) / 100f;
            AudioSource.PlayClipAtPoint(clip, transform.position, vol);
        }
    }

    void Morir()
    {
        estaMuerto = true;
        ReproducirSonidoAleatorio(sonidosMuerte);

        // Forzar animación de muerte directamente
        if (animator != null)
            animator.CrossFade("Death", 0.1f);

        // Detener movimiento
        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        // Desactivar collider para no bloquear
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Destruir después de que termine la animación de muerte
        Destroy(gameObject, 2.5f);
    }
}