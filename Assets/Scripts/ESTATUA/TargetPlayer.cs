using UnityEngine;

/** DAV - 2ºDAM
 * CLASE TARGETPLAYER – Jugador Dummy para Entrenamiento
 *
 * Simula un jugador real en la escena de entrenamiento:
 *   1. DEAMBULA aleatoriamente por el área (camina hacia waypoints aleatorios).
 *   2. GIRA la cámara a ángulos aleatorios con transición suave,
 *      incluyendo pitch (arriba/abajo) para imitar movimiento humano real.
 *
 * Diseñado para que las observaciones del StatueAgent en entrenamiento
 * sean lo más similares posible a las de la escena EXEC con el jugador real.
 *
 * NOTA: la referencia 'camara' debe apuntar al Transform de la MainCamera
 * dentro del prefab Personaje (igual que en EXEC). TargetPlayer setea
 * su rotación en espacio mundo directamente, anulando cualquier control
 * de input del prefab (que debe estar desactivado en entrenamiento).
 */
public class TargetPlayer : MonoBehaviour
{
    // ─── REFERENCIAS ────────────────────────────────────────────────────────────
    [Header("Referencias")]
    [Tooltip("Transform de la cámara del jugador (MainCamera dentro del prefab Personaje).")]
    public Transform camara;

    // ─── MOVIMIENTO ─────────────────────────────────────────────────────────────
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento. A 20x timeScale, 2 m/s da movimiento ágil.")]
    public float velocidadMovimiento = 2f;

    [Tooltip("Radio de deambulación respecto al centro del área (padre). " +
             "Debe ser menor que distanciaMaxima del StatueAgent.")]
    public float radioDeambulacion = 7f;

    // ─── CÁMARA ─────────────────────────────────────────────────────────────────
    [Header("Cámara")]
    [Tooltip("Velocidad de giro de la cámara en grados/segundo.")]
    public float velocidadGiro = 100f;

    [Tooltip("Tiempo mínimo que la cámara mantiene una dirección antes de girar.")]
    public float tiempoMinMirada = 0.4f;

    [Tooltip("Tiempo máximo que la cámara mantiene una dirección antes de girar.")]
    public float tiempoMaxMirada = 2.5f;

    [Tooltip("Rango de pitch (mirar arriba/abajo) en grados. Imita movimiento humano real.")]
    [Range(0f, 60f)]
    public float rangoPitch = 25f;

    // ─── VARIABLES PRIVADAS ─────────────────────────────────────────────────────
    private Vector3  destino;
    private float    timerCamara;
    private float    yawObjetivo;
    private float    yawActual;
    private float    pitchObjetivo;
    private float    pitchActual;

    // ════════════════════════════════════════════════════════════════════════════
    void Start()
    {
        ReiniciarEpisodio();
    }

    // ════════════════════════════════════════════════════════════════════════════
    void Update()
    {
        ActualizarMovimiento();
        ActualizarCamara();
    }

    // ─── MOVIMIENTO ALEATORIO ───────────────────────────────────────────────────
    private void ActualizarMovimiento()
    {
        // Llegar al waypoint → elegir uno nuevo
        Vector3 dirPlana = new Vector3(destino.x - transform.position.x, 0f, destino.z - transform.position.z);
        if (dirPlana.magnitude < 0.4f)
            ElegirNuevoDestino();

        // Mover hacia el waypoint en el plano XZ
        dirPlana = dirPlana.normalized;
        transform.position += dirPlana * velocidadMovimiento * Time.deltaTime;
    }

    // ─── ROTACIÓN DE CÁMARA ─────────────────────────────────────────────────────
    private void ActualizarCamara()
    {
        if (camara == null) return;

        timerCamara -= Time.deltaTime;
        if (timerCamara <= 0f)
            ElegirNuevoAnguloCamara();

        // Interpolar suavemente hacia el ángulo objetivo
        yawActual   = Mathf.MoveTowardsAngle(yawActual,   yawObjetivo,   velocidadGiro * Time.deltaTime);
        pitchActual = Mathf.MoveTowards     (pitchActual, pitchObjetivo, velocidadGiro * 0.5f * Time.deltaTime);

        // Aplicar en espacio mundo (anula la jerarquía del esqueleto del prefab)
        camara.rotation = Quaternion.Euler(pitchActual, yawActual, 0f);
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────────
    private void ElegirNuevoDestino()
    {
        Vector3 centro = transform.parent != null ? transform.parent.position : Vector3.zero;
        destino = new Vector3(
            centro.x + Random.Range(-radioDeambulacion, radioDeambulacion),
            transform.position.y,
            centro.z + Random.Range(-radioDeambulacion, radioDeambulacion)
        );
    }

    private void ElegirNuevoAnguloCamara()
    {
        yawObjetivo   = Random.Range(0f, 360f);
        pitchObjetivo = Random.Range(-rangoPitch, rangoPitch);
        timerCamara   = Random.Range(tiempoMinMirada, tiempoMaxMirada);
    }

    // ════════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Reinicia posición de destino y ángulo de cámara al comienzo de cada episodio.
    /// Llamado desde StatueAgent.OnEpisodeBegin().
    /// </summary>
    public void ReiniciarEpisodio()
    {
        ElegirNuevoDestino();
        ElegirNuevoAnguloCamara();
        // Snap inmediato al nuevo ángulo (sin transición entre episodios)
        yawActual   = yawObjetivo;
        pitchActual = pitchObjetivo;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GIZMOS
    // ════════════════════════════════════════════════════════════════════════════
    private void OnDrawGizmos()
    {
        // Waypoint destino (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(destino, 0.2f);
        Gizmos.DrawLine(transform.position + Vector3.up, destino + Vector3.up);

        if (camara == null) return;

        // Forward de la cámara (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(camara.position, camara.forward * 8f);
    }
}
