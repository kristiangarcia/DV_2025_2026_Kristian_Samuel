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

    [Header("Sonidos")]
    [Tooltip("Sonidos de muerte (Zombie_dead_1, Zombie_dead_2).")]
    public AudioClip[] sonidosMuerte;
    [Tooltip("Sonidos de golpe al jugador (Zombie_hit_1, 2, 3).")]
    public AudioClip[] sonidosGolpe;

    private NavMeshAgent agente;
    private Animator animator;
    private bool estaMuerto = false;
    private float tiempoUltimoDestino = 0f;
    private const float INTERVALO_DESTINO = 0.25f;

    // Para el efecto de fuego
    private Renderer[] renderers;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        vidaMaxima = vida;

        // Pintar el zombie de ROJO FUEGO desde el principio
        renderers = GetComponentsInChildren<Renderer>();
        PintarDeFuego();
    }

    void PintarDeFuego()
    {
        Color colorFuego = new Color(0.85f, 0.15f, 0.02f, 1f); // Rojo fuego intenso

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            // Crear copia del material para no afectar otros zombies
            Material mat = rend.material;

            if (mat.HasProperty("_Color"))
                mat.color = colorFuego;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", colorFuego);

            // Activar emisión (brillo) para que parezca que echa fuego
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", new Color(1f, 0.3f, 0.05f, 1f) * 1.5f);
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        if (objetivo != null && agente != null && agente.isOnNavMesh)
        {
            if (Time.time >= tiempoUltimoDestino + INTERVALO_DESTINO)
            {
                agente.SetDestination(objetivo.position);
                tiempoUltimoDestino = Time.time;
            }

            if (animator != null)
            {
                float velocidad = (agente.hasPath && !agente.pathPending)
                    ? agente.velocity.magnitude
                    : 0f;
                animator.SetFloat("Velocidad", velocidad);
            }
        }
        else if (animator != null)
        {
            animator.SetFloat("Velocidad", 0f);
        }

        // Efecto de parpadeo de fuego en tiempo real
        ParpadeoFuego();
    }

    void ParpadeoFuego()
    {
        // Hace que el brillo varíe como llamas
        float intensidad = 1.2f + Mathf.Sin(Time.time * 6f) * 0.5f + Mathf.Sin(Time.time * 9.3f) * 0.3f;
        Color emision = new Color(1f, 0.3f, 0.05f, 1f) * intensidad;

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;
            if (rend.material.HasProperty("_EmissionColor"))
                rend.material.SetColor("_EmissionColor", emision);
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

        // Al recibir daño: más brillante (flash)
        foreach (Renderer rend in renderers)
        {
            if (rend != null && rend.material.HasProperty("_EmissionColor"))
                rend.material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0.1f, 1f) * 3f);
        }

        if (animator != null && vida > 0)
            animator.CrossFade("HitReaction", 0.1f);

        if (vida <= 0)
        {
            Morir();
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

        // Explotar al morir también
        CrearExplosion();

        if (animator != null)
            animator.CrossFade("Death", 0.1f);

        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 0.5f);
    }

    void ExplotarAtaque(GameObject jugador)
    {
        // Crear efecto de explosión
        CrearExplosion();

        VidaJugador vidaScript = jugador.GetComponent<VidaJugador>();
        if (vidaScript != null)
        {
            vidaScript.RecibirDaño(dañoExplosion);
            ReproducirSonidoAleatorio(sonidosGolpe);
        }

        Destroy(gameObject);
    }

    void CrearExplosion()
    {
        Vector3 pos = transform.position + Vector3.up * 0.5f;

        // ========== SISTEMA 1: Bola de fuego ==========
        GameObject fuego = new GameObject("ExplosionFuego");
        fuego.transform.position = pos;

        ParticleSystem psFuego = fuego.AddComponent<ParticleSystem>();
        psFuego.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var mainFuego = psFuego.main;
        mainFuego.duration = 0.5f;
        mainFuego.loop = false;
        mainFuego.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        mainFuego.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        mainFuego.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        mainFuego.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.6f, 0f, 1f),   // Naranja
            new Color(1f, 0.2f, 0f, 1f)     // Rojo fuego
        );
        mainFuego.gravityModifier = -0.3f; // Las llamas suben
        mainFuego.maxParticles = 50;
        mainFuego.simulationSpace = ParticleSystemSimulationSpace.World;
        mainFuego.playOnAwake = false;

        var emFuego = psFuego.emission;
        emFuego.enabled = true;
        emFuego.rateOverTime = 0;
        emFuego.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30, 50)
        });

        var shapeFuego = psFuego.shape;
        shapeFuego.enabled = true;
        shapeFuego.shapeType = ParticleSystemShapeType.Sphere;
        shapeFuego.radius = 0.3f;

        var colorFuego = psFuego.colorOverLifetime;
        colorFuego.enabled = true;
        Gradient gradFuego = new Gradient();
        gradFuego.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.8f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 0.4f),
                new GradientColorKey(new Color(0.3f, 0.05f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorFuego.color = gradFuego;

        var sizeFuego = psFuego.sizeOverLifetime;
        sizeFuego.enabled = true;
        AnimationCurve curveFuego = new AnimationCurve();
        curveFuego.AddKey(0f, 0.5f);
        curveFuego.AddKey(0.2f, 1.5f);
        curveFuego.AddKey(1f, 0f);
        sizeFuego.size = new ParticleSystem.MinMaxCurve(1f, curveFuego);

        var rendFuego = fuego.GetComponent<ParticleSystemRenderer>();
        rendFuego.material = new Material(Shader.Find("Particles/Standard Unlit"));
        rendFuego.material.color = new Color(1f, 0.5f, 0f, 1f);
        rendFuego.renderMode = ParticleSystemRenderMode.Billboard;

        // ========== SISTEMA 2: Chispas y escombros ==========
        GameObject chispas = new GameObject("ExplosionChispas");
        chispas.transform.SetParent(fuego.transform, false);

        ParticleSystem psChispas = chispas.AddComponent<ParticleSystem>();
        psChispas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var mainChispas = psChispas.main;
        mainChispas.duration = 0.2f;
        mainChispas.loop = false;
        mainChispas.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        mainChispas.startSpeed = new ParticleSystem.MinMaxCurve(5f, 12f);
        mainChispas.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        mainChispas.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.9f, 0.3f, 1f),  // Amarillo brillante
            new Color(1f, 0.5f, 0f, 1f)      // Naranja
        );
        mainChispas.gravityModifier = 2f;
        mainChispas.maxParticles = 30;
        mainChispas.simulationSpace = ParticleSystemSimulationSpace.World;
        mainChispas.playOnAwake = false;

        var emChispas = psChispas.emission;
        emChispas.enabled = true;
        emChispas.rateOverTime = 0;
        emChispas.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15, 25)
        });

        var shapeChispas = psChispas.shape;
        shapeChispas.enabled = true;
        shapeChispas.shapeType = ParticleSystemShapeType.Sphere;
        shapeChispas.radius = 0.1f;

        var rendChispas = chispas.GetComponent<ParticleSystemRenderer>();
        rendChispas.material = rendFuego.material;

        // ========== SISTEMA 3: Onda expansiva (anillo) ==========
        GameObject onda = new GameObject("OndaExpansiva");
        onda.transform.SetParent(fuego.transform, false);

        ParticleSystem psOnda = onda.AddComponent<ParticleSystem>();
        psOnda.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var mainOnda = psOnda.main;
        mainOnda.duration = 0.1f;
        mainOnda.loop = false;
        mainOnda.startLifetime = 0.4f;
        mainOnda.startSpeed = 0f;
        mainOnda.startSize = 0.5f;
        mainOnda.startColor = new Color(1f, 0.6f, 0.1f, 0.6f);
        mainOnda.maxParticles = 1;
        mainOnda.playOnAwake = false;

        var emOnda = psOnda.emission;
        emOnda.enabled = true;
        emOnda.rateOverTime = 0;
        emOnda.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 1)
        });

        var sizeOnda = psOnda.sizeOverLifetime;
        sizeOnda.enabled = true;
        AnimationCurve curveOnda = new AnimationCurve();
        curveOnda.AddKey(0f, 1f);
        curveOnda.AddKey(1f, 8f);
        sizeOnda.size = new ParticleSystem.MinMaxCurve(1f, curveOnda);

        var colorOnda = psOnda.colorOverLifetime;
        colorOnda.enabled = true;
        Gradient gradOnda = new Gradient();
        gradOnda.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0f),
                new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.7f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOnda.color = gradOnda;

        var rendOnda = onda.GetComponent<ParticleSystemRenderer>();
        rendOnda.material = rendFuego.material;

        // ========== ARRANCAR TODO ==========
        psFuego.Play();
        psChispas.Play();
        psOnda.Play();

        Destroy(fuego, 3f);
    }
}