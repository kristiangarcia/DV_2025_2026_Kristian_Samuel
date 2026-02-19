using UnityEngine;

/** DAV - 2ºDAM
 * CLASE GESTORPARTIDAESTATUA
 *
 * Detecta cuando la estatua atrapa al jugador (proximidad < umbral)
 * y muestra un overlay de pantalla roja con el mensaje "¡TE ATRAPÓ!".
 *
 * No modifica StatueAgent: funciona solo comprobando la distancia cada frame.
 * Colocar este script en cualquier GameObject de la escena EXEC.
 */
public class GestorPartidaEstatua : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public Transform estatua;

    [Header("Parámetros")]
    [Tooltip("Distancia a la que se considera que la estatua ha atrapado al jugador.")]
    public float distanciaCaptura = 1.2f;

    [Tooltip("Segundos que se muestra el overlay antes de que desaparezca.")]
    public float duracionOverlay = 2.5f;

    // ─── Estado ──────────────────────────────────────────────────────────────
    private bool  atrapado    = false;
    private float timerOverlay = 0f;

    // Textura de 1×1 píxel para el fondo rojo
    private Texture2D texRojo;

    // ════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        texRojo = new Texture2D(1, 1);
        texRojo.SetPixel(0, 0, new Color(0.8f, 0f, 0f, 0.45f));
        texRojo.Apply();
    }

    // ════════════════════════════════════════════════════════════════════════
    void Update()
    {
        if (jugador == null || estatua == null) return;

        float dist = Vector3.Distance(jugador.position, estatua.position);

        if (dist <= distanciaCaptura)
        {
            atrapado    = true;
            timerOverlay = duracionOverlay;
        }

        if (atrapado)
        {
            timerOverlay -= Time.deltaTime;
            if (timerOverlay <= 0f)
                atrapado = false;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    void OnGUI()
    {
        if (!atrapado) return;

        // Fondo rojo semitransparente
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texRojo);

        // Texto "¡TE ATRAPÓ!"
        var estilo = new GUIStyle(GUI.skin.label)
        {
            fontSize  = Mathf.RoundToInt(Screen.height * 0.12f),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "¡TE ATRAPÓ!", estilo);
    }
}
