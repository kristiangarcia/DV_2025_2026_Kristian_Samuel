using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Pantalla de transición entre rondas - DeadWave
// Se muestra 5 segundos entre oleadas con temática de terror

public class PantallaRonda : MonoBehaviour
{
    private Canvas canvas;
    private Text textoRonda;
    private Text textoMensaje;
    private Image fondo;

    public static PantallaRonda Instancia { get; private set; }

    void Awake()
    {
        Instancia = this;
    }

    public void MostrarRonda(int ronda, System.Action alTerminar)
    {
        StartCoroutine(EfectoRonda(ronda, alTerminar));
    }

    IEnumerator EfectoRonda(int ronda, System.Action alTerminar)
    {
        CrearUI();

        // Mensaje según la ronda
        string[] mensajes = {
            "PREPÁRATE PARA LA OLEADA",
            "LAS SOMBRAS SE ACERCAN",
            "NO HAY ESCAPATORIA",
            "SOBREVIVE O MUERE",
            "LOS KAMIKAZES DESPIERTAN",
            "EL INFIERNO SE DESATA",
            "LA OSCURIDAD TE CONSUME",
            "NADIE SALDRÁ VIVO",
            "ÚLTIMA RESISTENCIA",
            "EL FIN SE ACERCA"
        };
        string mensaje = mensajes[Mathf.Min(ronda - 1, mensajes.Length - 1)];

        textoRonda.text = "";
        textoMensaje.text = "";

        // === FASE 1: Fade in fondo (0.5s) ===
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            Color c = fondo.color;
            c.a = Mathf.Lerp(0, 0.85f, t / 0.5f);
            fondo.color = c;
            yield return null;
        }

        // === FASE 2: Mostrar número de ronda con efecto (1.5s) ===
        textoRonda.text = "R O N D A   " + ronda;
        textoRonda.color = new Color(0.8f, 0.05f, 0.05f, 0f);

        t = 0;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            // Fade in + efecto de escala
            float alpha = Mathf.Clamp01(t / 0.8f);
            float glitch = Random.Range(0f, 1f) > 0.9f ? Random.Range(-0.05f, 0.05f) : 0f;
            textoRonda.color = new Color(0.8f + glitch, 0.05f, 0.05f, alpha);
            yield return null;
        }

        // === FASE 3: Mostrar mensaje (2s) ===
        textoMensaje.text = mensaje;
        textoMensaje.color = new Color(0.6f, 0.55f, 0.5f, 0f);

        t = 0;
        while (t < 2f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / 0.6f);
            textoMensaje.color = new Color(0.6f, 0.55f, 0.5f, alpha);

            // Parpadeo del título
            float intensidad = 0.7f + Mathf.Sin(Time.time * 4f) * 0.3f;
            textoRonda.color = new Color(0.8f, 0.05f, 0.05f, intensidad);
            yield return null;
        }

        // === FASE 4: Fade out (1s) ===
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / 1f);
            fondo.color = new Color(0.02f, 0, 0, 0.85f * alpha);
            textoRonda.color = new Color(0.8f, 0.05f, 0.05f, alpha);
            textoMensaje.color = new Color(0.6f, 0.55f, 0.5f, alpha);
            yield return null;
        }

        // Destruir UI y continuar
        Destroy(canvas.gameObject);
        alTerminar?.Invoke();
    }

    void CrearUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("CanvasRonda");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Fondo oscuro
        GameObject fondoObj = new GameObject("FondoRonda");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rtFondo = fondoObj.AddComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;
        fondo = fondoObj.AddComponent<Image>();
        fondo.color = new Color(0.02f, 0, 0, 0f);

        // Líneas decorativas de sangre
        for (int i = 0; i < 5; i++)
        {
            GameObject lineaObj = new GameObject("Linea_" + i);
            lineaObj.transform.SetParent(canvasObj.transform, false);
            RectTransform rt = lineaObj.AddComponent<RectTransform>();
            float posY = Random.Range(0.1f, 0.9f);
            rt.anchorMin = new Vector2(0, posY);
            rt.anchorMax = new Vector2(1, posY + 0.002f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = lineaObj.AddComponent<Image>();
            img.color = new Color(0.5f, 0, 0, Random.Range(0.05f, 0.12f));
        }

        // Separador superior
        CrearSeparador(canvasObj.transform, 0.62f);

        // Texto RONDA X
        GameObject rondaObj = new GameObject("TextoRonda");
        rondaObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rtRonda = rondaObj.AddComponent<RectTransform>();
        rtRonda.anchorMin = new Vector2(0.5f, 0.55f);
        rtRonda.anchorMax = new Vector2(0.5f, 0.55f);
        rtRonda.sizeDelta = new Vector2(800, 120);
        rtRonda.anchoredPosition = Vector2.zero;

        Shadow sombra = rondaObj.AddComponent<Shadow>();
        sombra.effectColor = new Color(0.8f, 0, 0, 0.5f);
        sombra.effectDistance = new Vector2(3, -3);

        textoRonda = rondaObj.AddComponent<Text>();
        textoRonda.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoRonda.fontSize = 90;
        textoRonda.fontStyle = FontStyle.Bold;
        textoRonda.color = new Color(0.8f, 0.05f, 0.05f, 0f);
        textoRonda.alignment = TextAnchor.MiddleCenter;

        // Separador inferior
        CrearSeparador(canvasObj.transform, 0.48f);

        // Texto mensaje
        GameObject msgObj = new GameObject("TextoMensaje");
        msgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rtMsg = msgObj.AddComponent<RectTransform>();
        rtMsg.anchorMin = new Vector2(0.5f, 0.42f);
        rtMsg.anchorMax = new Vector2(0.5f, 0.42f);
        rtMsg.sizeDelta = new Vector2(700, 40);
        rtMsg.anchoredPosition = Vector2.zero;

        textoMensaje = msgObj.AddComponent<Text>();
        textoMensaje.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoMensaje.fontSize = 22;
        textoMensaje.color = new Color(0.6f, 0.55f, 0.5f, 0f);
        textoMensaje.alignment = TextAnchor.MiddleCenter;
    }

    void CrearSeparador(Transform padre, float posY)
    {
        GameObject sepObj = new GameObject("Separador");
        sepObj.transform.SetParent(padre, false);
        RectTransform rt = sepObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, posY);
        rt.anchorMax = new Vector2(0.8f, posY);
        rt.sizeDelta = new Vector2(0, 2);
        rt.anchoredPosition = Vector2.zero;
        Image img = sepObj.AddComponent<Image>();
        img.color = new Color(0.5f, 0.05f, 0.05f, 0.4f);
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }
}
