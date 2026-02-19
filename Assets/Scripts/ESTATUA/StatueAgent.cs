using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/** DAV - 2ºDAM
 * CLASE STATUEAGENT - "La Estatua Acechadora"
 *
 * Inspirada en los Weeping Angels (Doctor Who), Coil-Head (Lethal Company) y los Boos (Mario).
 *
 * REGLA DE ORO: La estatua solo puede moverse cuando el jugador NO la está mirando.
 *
 * Detección de visión: NO usamos OnBecameVisible() (poco fiable en ML-Agents).
 * En su lugar calculamos el PRODUCTO PUNTO (Dot Product) entre:
 *   - El vector forward de la cámara del jugador
 *   - El vector desde la cámara hacia la estatua
 * Si dot > umbral (~53° de campo visual) → la estatua es "vista".
 *
 * OBSERVACIONES (Space Size = 12):
 *   [0-2] Posición relativa del jugador   (Vector3 normalizado)
 *   [3]   ¿Está siendo vista?             (0 = no / 1 = sí)
 *   [4-6] Forward de la cámara del jugador (Vector3 normalizado)
 *         → Permite a la IA ANTICIPAR si la cámara se va a girar hacia ella.
 *   [7-9] Velocidad propia normalizada    (Vector3 / velocidadMax)
 *         → CRÍTICO: sin esto la red no distingue "voy directo" de "estoy orbitando".
 *   [10]  Distancia normalizada al jugador (float 0-1)
 *   [11]  Dot(dirMovimiento, dirObjetivo)  (float -1 a 1)
 *         → Feedback inmediato de alineación: 1=directo, 0=perpendicular, -1=huyendo.
 *
 * ACCIONES (Continuous Actions = 2):
 *   [0] moveX  [-1, 1]
 *   [1] moveZ  [-1, 1]
 */
public class StatueAgent : Agent
{
    // ─── REFERENCIAS ────────────────────────────────────────────────────────────
    [Header("Referencias")]
    [Tooltip("Transform del jugador objetivo (lo que la estatua quiere alcanzar).")]
    public Transform jugador;

    [Tooltip("Transform que actúa como cámara del jugador (hijo del objeto jugador).")]
    public Transform camaraJugador;

    [Tooltip("(Opcional) Script TargetPlayer para sincronizar el reinicio de episodio.")]
    public TargetPlayer targetPlayerCtrl;

    // ─── MOVIMIENTO ─────────────────────────────────────────────────────────────
    [Header("Parámetros de Movimiento")]
    [Tooltip("Velocidad máxima de desplazamiento de la estatua.")]
    public float velocidad = 4f;

    // ─── DETECCIÓN VISUAL ───────────────────────────────────────────────────────
    [Header("Parámetros de Detección Visual")]
    [Range(0f, 1f)]
    [Tooltip("Dot Product mínimo para ser considerada 'vista'.\n" +
             "0.50 ≈ 60° | 0.60 ≈ 53° | 0.70 ≈ 45° | 0.87 ≈ 30°")]
    public float umbralVision = 0.6f;

    [Tooltip("Distancia máxima a la que el jugador puede ver la estatua.")]
    public float distanciaMaxVision = 20f;

    // ─── ENTRENAMIENTO ──────────────────────────────────────────────────────────
    [Header("Parámetros de Entrenamiento")]
    [Tooltip("Radio del área de entrenamiento. Si la estatua sale: penalización y reset.")]
    public float distanciaMaxima = 12f;

    [Tooltip("Distancia a la que se considera que la estatua alcanzó al jugador (éxito).")]
    public float distanciaExito = 0.35f;

    [Tooltip("Radio aleatorio de spawn para estatua y jugador al inicio del episodio.")]
    public float radioSpawn = 5f;

    // ─── DEBUG ──────────────────────────────────────────────────────────────────
    [Header("Debug")]
    public bool mostrarGizmos = true;
    public bool activarLogs    = true;

    // ─── VARIABLES PRIVADAS ─────────────────────────────────────────────────────
    private bool   episodioTerminado = false;
    private int    episodioCount     = 0;
    private float  recompensaEpisodio = 0f;
    private float  recompensaTotal    = 0f;
    private float  distanciaAnterior;

    // Estado de la mecánica (usado también en Gizmos y debugStatus)
    private bool   siendoVista  = false;
    private string debugStatus  = "Inicializando...";

    // Seguimiento de velocidad propia (necesario porque el agente no tiene Rigidbody)
    private Vector3 posicionPrevia;
    private Vector3 velocidadActual = Vector3.zero;

    // ════════════════════════════════════════════════════════════════════════════
    // 1. INICIALIZACIÓN – Se ejecuta UNA VEZ al pulsar Play
    // ════════════════════════════════════════════════════════════════════════════
    void Start()
    {
        // Acelerar el tiempo durante el entrenamiento (aprende hasta 20x más rápido).
        // En inference (modelo cargado) corre a velocidad normal.
        bool entrenando = Academy.Instance.IsCommunicatorOn;
        Time.timeScale = entrenando ? 20f : 1f;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 2. REINICIO DE EPISODIO – Cada vez que gana, pierde o se agota el tiempo
    // ════════════════════════════════════════════════════════════════════════════
    public override void OnEpisodeBegin()
    {
        episodioTerminado = false;
        episodioCount++;
        recompensaEpisodio = 0f;

        // Spawn de la estatua en posición aleatoria dentro del radio
        transform.position = PuntoAleatorio(radioSpawn);

        // Resetear tracker de velocidad
        posicionPrevia    = transform.position;
        velocidadActual   = Vector3.zero;

        if (Academy.Instance.IsCommunicatorOn)
        {
            // En training: spawn aleatorio del jugador (mínimo 2m de separación)
            int intentos = 0;
            do
            {
                jugador.position = PuntoAleatorio(radioSpawn);
                intentos++;
            } while (Vector3.Distance(transform.position, jugador.position) < 2f && intentos < 10);

            // Sincronizar el script del jugador (rota la cámara a un ángulo base nuevo)
            if (targetPlayerCtrl != null) targetPlayerCtrl.ReiniciarEpisodio();
        }

        distanciaAnterior = Vector3.Distance(transform.position, jugador.position);

        if (activarLogs)
            Debug.Log($"[Estatua · Ep {episodioCount}] INICIO | Dist inicial: {distanciaAnterior:F2}m");
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 3. OBSERVACIONES – Lo que la IA "siente" del mundo (7 valores)
    // ════════════════════════════════════════════════════════════════════════════
    public override void CollectObservations(VectorSensor sensor)
    {
        // OBS [0-2]: Posición relativa del jugador (normalizada por distanciaMaxima)
        // La IA sabe en qué dirección y cuán lejos está su objetivo.
        Vector3 posicionRelativa = (jugador.position - transform.position) / distanciaMaxima;
        sensor.AddObservation(posicionRelativa); // 3 observaciones

        // OBS [3]: ¿Está siendo vista? → La IA sabe si puede moverse o no en este momento.
        siendoVista = calcularSiEsVista();
        sensor.AddObservation(siendoVista ? 1f : 0f); // 1 observación

        // OBS [4-6]: Forward de la cámara del jugador (normalizado)
        // CLAVE: permite a la IA anticipar si la cámara se está girando HACIA ella.
        sensor.AddObservation(camaraJugador.forward); // 3 observaciones

        // OBS [7-9]: Velocidad propia normalizada por velocidad máxima
        // CRÍTICO: sin esta observación la red no distingue entre "voy directo al objetivo"
        // y "estoy orbitando a la misma distancia". Primer fix contra el orbiting.
        sensor.AddObservation(velocidadActual / velocidad); // 3 observaciones

        // OBS [10]: Distancia normalizada al jugador (0 = encima, 1 = en el límite del área)
        float distNorm = Vector3.Distance(transform.position, jugador.position) / distanciaMaxima;
        sensor.AddObservation(Mathf.Clamp01(distNorm)); // 1 observación

        // OBS [11]: Alineación movimiento-objetivo: 1=directo, 0=perpendicular, -1=huyendo
        // Feedback inmediato que refuerza la recompensa de alineación.
        Vector3 dirObjetivo = (jugador.position - transform.position).normalized;
        float dotAlineacion = velocidadActual.magnitude > 0.01f
            ? Vector3.Dot(velocidadActual.normalized, dirObjetivo)
            : 0f;
        sensor.AddObservation(dotAlineacion); // 1 observación

        // TOTAL: 12 observaciones → Space Size = 12 en BehaviorParameters
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 4. ACCIONES Y RECOMPENSAS – El corazón del aprendizaje
    // ════════════════════════════════════════════════════════════════════════════
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (episodioTerminado) return;

        // Releer el estado de visión (puede haber cambiado desde CollectObservations)
        siendoVista = calcularSiEsVista();

        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // Threshold de 0.1 para ignorar ruido de la red neuronal (valores casi-cero)
        bool intentaMoverse = new Vector2(moveX, moveZ).magnitude > 0.1f;

        // ★ ─── REGLA DE ORO ─── ★
        if (siendoVista)
        {
            if (intentaMoverse)
            {
                // CASTIGO: intenta moverse estando a la vista → viola la regla fundamental
                AplicarRecompensa(-0.1f);
                debugStatus = "★ ¡VISTA! Intenta moverse → PENALIZACIÓN";
            }
            else
            {
                // Correctamente quieta: comportamiento neutro (no premia ni penaliza)
                debugStatus = "Vista → Quieta (correcto)";
            }
            // En cualquier caso, NO se aplica movimiento físico
        }
        else
        {
            // LIBRE para moverse: aplica el movimiento
            Vector3 movimiento = new Vector3(moveX, 0f, moveZ) * velocidad * Time.deltaTime;
            transform.position += movimiento;
            debugStatus = "No vista → Moviéndose";

            // RECOMPENSA DE ALINEACIÓN: premia moverse DIRECTO al jugador.
            // Penaliza el movimiento perpendicular (orbiting) directamente.
            //   dot =  1.0 → va directo al objetivo   → +0.02
            //   dot =  0.0 → movimiento perpendicular  →  0.00  (orbiting sin beneficio)
            //   dot = -1.0 → se aleja                 → -0.02
            if (intentaMoverse)
            {
                Vector3 dirObjetivo   = (jugador.position - transform.position).normalized;
                Vector3 dirMovimiento = new Vector3(moveX, 0f, moveZ).normalized;
                float   alineacion    = Vector3.Dot(dirObjetivo, dirMovimiento);
                AplicarRecompensa(alineacion * 0.02f);
            }
        }

        // ─── Actualizar velocidad propia ──────────────────────────────────────
        // Debe calcularse DESPUÉS del movimiento para reflejar el desplazamiento real.
        velocidadActual = (transform.position - posicionPrevia) / Time.deltaTime;
        posicionPrevia  = transform.position;

        // ─── Métricas de progreso ──────────────────────────────────────────────
        float distanciaActual = Vector3.Distance(transform.position, jugador.position);

        if (!siendoVista)
        {
            // SIMÉTRICO: recompensa acercarse Y penaliza alejarse.
            // Reducido a 0.05 porque la recompensa de alineación ya cubre la dirección.
            float progreso = distanciaAnterior - distanciaActual;
            AplicarRecompensa(progreso * 0.05f);
        }

        // Siempre actualizamos la distancia anterior (el jugador también puede moverse)
        distanciaAnterior = distanciaActual;

        // Penalización por tiempo: crea urgencia para llegar rápido.
        // Con MaxStep=5000 → -5/5000 = -0.001/step → acumulado -5 si no toca nunca.
        // Guard: MaxStep=0 significa sin límite (modo EXEC), no dividir por cero.
        if (MaxStep > 0)
            AplicarRecompensa(-5f / MaxStep);

        // ─── Condiciones de fin de episodio ───────────────────────────────────
        if (distanciaActual <= distanciaExito)
            finalizaEpisodio("¡ALCANZÓ al jugador!", 5.0f, true);

        if (distanciaActual > distanciaMaxima)
            finalizaEpisodio("Salió del área de entrenamiento", -1.0f, false);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // 5. HEURÍSTICA – Control por teclado para probar la mecánica manualmente
    //    Usa el modo "Heuristic Only" en BehaviorParameters para activarlo.
    // ════════════════════════════════════════════════════════════════════════════
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var acc = actionsOut.ContinuousActions;
        acc[0] = Input.GetAxisRaw("Horizontal"); // A / D  o ← →
        acc[1] = Input.GetAxisRaw("Vertical");   // W / S  o ↑ ↓
    }

    // ════════════════════════════════════════════════════════════════════════════
    // DETECCIÓN DE VISIÓN – Cálculo vectorial mediante Producto Punto
    // ════════════════════════════════════════════════════════════════════════════
    private bool calcularSiEsVista()
    {
        // 1. Comprobación de distancia (optimización: no calcular si está muy lejos)
        float distancia = Vector3.Distance(camaraJugador.position, transform.position);
        if (distancia > distanciaMaxVision) return false;

        // 2. Vector unitario desde la cámara del jugador HACIA la estatua
        Vector3 dirCamaraAEstatua = (transform.position - camaraJugador.position).normalized;

        // 3. PRODUCTO PUNTO: mide el "alineamiento" entre dos vectores.
        //    Resultado entre -1 (opuestos) y +1 (perfectamente alineados).
        //    Si dot > umbralVision → la estatua está dentro del campo visual de la cámara.
        //    Ejemplo con umbral 0.6: visible si está dentro de ≈53° del centro de la cámara.
        float dot = Vector3.Dot(camaraJugador.forward, dirCamaraAEstatua);

        return dot > umbralVision;

        // ── VERSIÓN MEJORADA PARA LA ESCENA DE JUEGO (con raycast de obstáculos) ──
        // Descomenta este bloque y comenta el "return" anterior para usarla en el juego real.
        // Detecta si hay un muro u objeto entre el jugador y la estatua.
        //
        // if (dot <= umbralVision) return false; // Fuera del ángulo de visión
        //
        // Vector3 origen    = camaraJugador.position;
        // Vector3 direccion = (transform.position - origen);
        // if (Physics.Raycast(origen, direccion, out RaycastHit hit, distanciaMaxVision))
        //     return hit.collider.CompareTag("Estatua"); // Solo visible si el rayo llega a la estatua
        // return false;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // MÉTODOS AUXILIARES
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>Aplica una recompensa y la registra en el acumulador del episodio.</summary>
    private void AplicarRecompensa(float valor)
    {
        AddReward(valor);
        recompensaEpisodio += valor;
    }

    /// <summary>Cierra el episodio con bonus, log de color y EndEpisode().</summary>
    private void finalizaEpisodio(string razon, float bonus, bool exito)
    {
        if (episodioTerminado) return;
        episodioTerminado = true;

        AplicarRecompensa(bonus);
        recompensaTotal += recompensaEpisodio;

        if (activarLogs)
        {
            string color  = exito ? "green" : "red";
            string estado = exito ? "LOGRADO" : "REINICIO";
            float media   = episodioCount > 0 ? recompensaTotal / episodioCount : 0f;
            Debug.Log($"<color={color}><b>[Ep {episodioCount}] {estado}</b> {razon} | " +
                      $"Reward: {recompensaEpisodio:F3} | Media: {media:F3}</color>");
        }

        EndEpisode();
    }

    /// <summary>Devuelve una posición aleatoria en el plano XZ a altura 0.5f,
    /// centrada en el padre del agente para soportar múltiples instancias de entrenamiento.</summary>
    private Vector3 PuntoAleatorio(float radio)
    {
        Vector3 centro = transform.parent != null ? transform.parent.position : Vector3.zero;
        return new Vector3(
            centro.x + Random.Range(-radio, radio),
            0.5f,
            centro.z + Random.Range(-radio, radio)
        );
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GIZMOS – Visualización en el Editor para depuración
    // ════════════════════════════════════════════════════════════════════════════
    private void OnDrawGizmos()
    {
        if (!mostrarGizmos || jugador == null || camaraJugador == null) return;

        // Línea estatua → jugador: VERDE si no vista, ROJO si vista
        Gizmos.color = siendoVista ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position + Vector3.up, jugador.position + Vector3.up);

        // Rayo del forward de la cámara (naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
        Gizmos.DrawRay(camaraJugador.position, camaraJugador.forward * Mathf.Min(distanciaMaxVision, 10f));

        // Radio de éxito alrededor del jugador (azul)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(jugador.position, distanciaExito);

        // Límite del área de entrenamiento (amarillo semitransparente) — centrado en el padre
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Vector3 centroPadre = transform.parent != null ? transform.parent.position : Vector3.zero;
        Gizmos.DrawWireCube(centroPadre, new Vector3(distanciaMaxima * 2, 0.1f, distanciaMaxima * 2));

        // Label de debug sobre la estatua
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.2f, debugStatus);
#endif
    }
}
