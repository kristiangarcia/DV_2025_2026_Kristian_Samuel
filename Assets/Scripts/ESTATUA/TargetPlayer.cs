using UnityEngine;

/** DAV - 2ºDAM
 * CLASE TARGETPLAYER – Jugador Dummy para Entrenamiento
 *
 * Simula el comportamiento de un jugador en la escena de entrenamiento.
 * Su función principal es mover la cámara de forma que cree VENTANAS DE OPORTUNIDAD
 * para que la StatueAgent pueda moverse sin ser vista.
 *
 * MODOS DISPONIBLES:
 *   · Oscilación (recomendado): Péndulo predecible → permite al agente aprender a
 *     anticipar cuándo la cámara se aleja de ella.
 *   · Aleatorio: Giros instantáneos a ángulos al azar → más impredecible,
 *     entrena un comportamiento más reactivo.
 *
 * PARA MÚLTIPLES INSTANCIAS DE ENTRENAMIENTO: Cada par StatueAgent + TargetPlayer
 * debe ser hijo de un GameObject padre independiente. Las referencias deben
 * apuntar SIEMPRE al TargetPlayer del mismo par.
 */
public class TargetPlayer : MonoBehaviour
{
    // ─── REFERENCIAS ────────────────────────────────────────────────────────────
    [Header("Referencias")]
    [Tooltip("Transform vacío hijo de este objeto que actúa como la 'cámara' del jugador.\n" +
             "Solo necesita posición y rotación, NO necesita componente Camera real.")]
    public Transform camara;

    // ─── MODO DE GIRO ────────────────────────────────────────────────────────────
    [Header("Modo de Giro")]
    [Tooltip("Oscilación: movimiento de péndulo. Predecible y bueno para aprendizaje inicial.\n" +
             "Aleatorio: giros instantáneos. Más difícil, para refinamiento avanzado.")]
    public bool usarOscilacion = true;

    // ─── CONFIGURACIÓN OSCILACIÓN ────────────────────────────────────────────────
    [Header("Oscilación (péndulo)")]
    [Tooltip("Velocidad del barrido. A 20x timeScale, 0.5f da ventanas cómodas al agente.")]
    public float velocidadOscilacion = 0.5f;

    [Tooltip("Amplitud del barrido en grados. 130° cubre buena parte de la arena.")]
    [Range(30f, 180f)]
    public float amplitudOscilacion = 130f;

    // ─── CONFIGURACIÓN ALEATORIA ─────────────────────────────────────────────────
    [Header("Rotación Aleatoria")]
    [Tooltip("Segundos (en tiempo de juego escalado) entre cada giro aleatorio.")]
    public float tiempoEntreCambios = 1.5f;

    // ─── VARIABLES PRIVADAS ─────────────────────────────────────────────────────
    // Offset aleatorio que se renueva en cada episodio para que la oscilación
    // no cubra siempre el mismo arco del espacio.
    private float offsetAngulo;
    private float timer;

    // ════════════════════════════════════════════════════════════════════════════
    void Start()
    {
        ReiniciarEpisodio();
    }

    // ════════════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (camara == null) return;

        if (usarOscilacion)
        {
            // Péndulo: función seno sobre el tiempo escalado.
            // El offset aleatorio hace que cada episodio empiece en una posición angular distinta,
            // obligando al agente a generalizar en lugar de memorizar un único patrón.
            float angulo = Mathf.Sin(Time.time * velocidadOscilacion) * amplitudOscilacion;
            camara.rotation = Quaternion.Euler(0f, angulo + offsetAngulo, 0f);
        }
        else
        {
            // Giros instantáneos aleatorios: simula un jugador nervioso mirando en todas direcciones
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                camara.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                timer = tiempoEntreCambios;
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Reinicia el estado del jugador dummy al comienzo de cada episodio.
    /// Llamado automáticamente desde StatueAgent.OnEpisodeBegin() si hay referencia.
    /// </summary>
    public void ReiniciarEpisodio()
    {
        // Nuevo ángulo base aleatorio → cada episodio cubre un arco diferente del espacio
        offsetAngulo = Random.Range(0f, 360f);
        timer        = tiempoEntreCambios;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GIZMOS – Visualización en el Editor
    // ════════════════════════════════════════════════════════════════════════════
    private void OnDrawGizmos()
    {
        if (camara == null) return;

        // Rayo del forward de la cámara (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(camara.position, camara.forward * 8f);

        // Representación del jugador (esfera pequeña)
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        // Posición de la cámara (punto naranja)
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(camara.position, 0.15f);
    }
}
