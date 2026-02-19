using UnityEngine;
using UnityEngine.InputSystem;

/** DAV - 2ºDAM
 * CLASE PLAYERFPS – Controlador de jugador en primera persona
 *
 * Usado en la escena de ejecución (6_EXEC_Estatua) para que el jugador
 * pueda moverse y mirar con el ratón, simulando el punto de vista real.
 *
 * La rotación de CamaraPoint es lo que la StatueAgent lee para calcular
 * si está siendo vista (via producto punto en calcularSiEsVista()).
 */
public class PlayerFPS : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform vacío hijo a altura 1.7 que actúa como los ojos del jugador.")]
    public Transform camaraPoint;

    [Header("Movimiento")]
    public float velocidad = 5f;

    [Header("Ratón")]
    public float sensibilidad = 2f;

    // Acumulador de rotación vertical (para clampear el giro arriba/abajo)
    private float rotX = 0f;

    // ════════════════════════════════════════════════════════════════════════════
    void Start()
    {
        // No bloqueamos aquí: en el Editor el Game View necesita foco primero.
        // El jugador hace clic para activar el mouse look.
    }

    // ════════════════════════════════════════════════════════════════════════════
    void Update()
    {
        var kb    = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // ── Clic para capturar el cursor (necesario en Unity Editor) ──────────
        if (mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        // ── Escape para liberar el cursor ─────────────────────────────────────
        if (kb.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // ── Movimiento WASD en el plano XZ (siempre activo) ───────────────────
        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        Vector3 dir = (transform.right * h + transform.forward * v).normalized;
        transform.position += dir * velocidad * Time.deltaTime;

        // ── Mouse look: solo cuando el cursor está capturado ──────────────────
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = mouse.delta.ReadValue();

        // Rotación horizontal: gira el jugador completo (Y)
        transform.Rotate(Vector3.up, delta.x * sensibilidad * 0.1f);

        // Rotación vertical: inclina solo la cámara (X), clampeada ±80°
        if (camaraPoint != null)
        {
            rotX -= delta.y * sensibilidad * 0.1f;
            rotX  = Mathf.Clamp(rotX, -80f, 80f);
            camaraPoint.localEulerAngles = new Vector3(rotX, 0f, 0f);
        }
    }
}
