using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.Barracuda;

/** DAV - 2ºDAM
 * CLASE SPAWNADORESTATUA
 *
 * Gestiona la aparición de la Estatua Acechadora en la escena principal.
 * Se llama desde GeneradorHordas al inicio de cada ronda.
 *
 * - A partir de rondaInicio (por defecto 3), spawnea UNA estatua.
 * - La estatua persiste entre rondas (no se destruye al terminar la horda).
 * - Si la estatua ya existe no se crea otra.
 *
 * Añadir este componente al mismo GameObject que GeneradorHordas
 * y asignar los campos en el Inspector.
 */
public class SpawnadorEstatua : MonoBehaviour
{
    [Header("Prefab y Modelo")]
    [Tooltip("Prefab visual de la estatua (Cemetery Statue).")]
    public GameObject prefabEstatua;

    [Tooltip("Modelo .onnx entrenado con ML-Agents.")]
    public NNModel modeloEstatua;

    [Header("Referencias")]
    [Tooltip("Transform del jugador (mismo que usa GeneradorHordas).")]
    public Transform jugador;

    [Tooltip("Transform de la cámara del jugador. Si está vacío se busca por tag 'MainCamera'.")]
    public Transform camaraJugador;

    [Header("Configuración")]
    [Tooltip("Ronda a partir de la cual aparece la estatua.")]
    public int rondaInicio = 3;

    [Tooltip("Distancia desde el jugador a la que aparece la estatua.")]
    public float distanciaSpawn = 15f;

    // Referencia a la estatua viva (null si no existe)
    private GameObject estatuaActual;

    // ════════════════════════════════════════════════════════════════════════════
    /// <summary>
    /// Llamado por GeneradorHordas.IniciarRonda() con el número de ronda actual.
    /// Spawnea la estatua si toca y aún no existe.
    /// </summary>
    public void ComprobarSpawn(int rondaActual)
    {
        if (rondaActual < rondaInicio) return;
        if (estatuaActual != null) return;   // Ya vive una estatua

        if (prefabEstatua == null)
        {
            Debug.LogWarning("[Estatua] prefabEstatua no asignado en el Inspector.");
            return;
        }
        if (modeloEstatua == null)
        {
            Debug.LogWarning("[Estatua] modeloEstatua (.onnx) no asignado en el Inspector.");
            return;
        }

        // Resolver camaraJugador si no está asignada en el Inspector
        if (camaraJugador == null)
        {
            var camGO = GameObject.FindWithTag("MainCamera");
            if (camGO != null) camaraJugador = camGO.transform;
        }

        if (jugador == null)
        {
            Debug.LogWarning("[Estatua] jugador no asignado en el Inspector.");
            return;
        }
        if (camaraJugador == null)
        {
            Debug.LogWarning("[Estatua] camaraJugador no encontrada (asígnala o etiqueta la cámara como MainCamera).");
            return;
        }

        // Posición: detrás del jugador a distanciaSpawn, a la misma altura que el jugador
        Vector3 dir      = -jugador.forward;
        Vector3 spawnPos = jugador.position + dir * distanciaSpawn;
        // Raycast hacia abajo para apoyarla en el suelo real de la escena
        if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 20f))
            spawnPos.y = groundHit.point.y;
        else
            spawnPos.y = jugador.position.y;

        // Instanciar el prefab visual
        estatuaActual = Instantiate(prefabEstatua, spawnPos, Quaternion.identity);
        estatuaActual.name = "Estatua_Agent";

        // Añadir BoxCollider si el prefab no trae ninguno en el root
        if (estatuaActual.GetComponent<Collider>() == null)
        {
            var bc    = estatuaActual.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 0.9f, 0f);
            bc.size   = new Vector3(0.8f, 1.8f, 0.8f);
        }

        // ── StatueAgent PRIMERO (para que DecisionRequester no duplique Agent) ──
        var agente = estatuaActual.AddComponent<StatueAgent>();
        agente.jugador          = jugador;
        agente.camaraJugador    = camaraJugador;
        agente.targetPlayerCtrl = null;
        agente.MaxStep          = 0;      // Sin límite de episodio en juego real
        agente.distanciaMaxima  = 60f;    // Radio amplio para un mapa real
        agente.distanciaExito   = 1.0f;
        agente.mostrarGizmos    = false;
        agente.activarLogs      = false;

        // ── BehaviorParameters (auto-creado por Agent, solo configurar) ──
        var bp = estatuaActual.GetComponent<BehaviorParameters>();
        bp.BehaviorName = "StatueAgent";
        bp.BrainParameters.VectorObservationSize        = 12;
        bp.BrainParameters.NumStackedVectorObservations = 1;
        bp.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(2);
        bp.Model        = modeloEstatua;
        bp.BehaviorType = BehaviorType.InferenceOnly;

        // ── DecisionRequester DESPUÉS de StatueAgent ──
        var dr = estatuaActual.AddComponent<DecisionRequester>();
        dr.DecisionPeriod = 5;

        Debug.Log($"[Ronda {rondaActual}] Estatua Acechadora spawneada en {spawnPos}");
    }
}
