using UnityEngine;
using UnityEngine.UI;

// HUD de Ronda - DeadWave
// Muestra el número de ronda en números romanos estilo CoD Zombies
// Se actualiza automáticamente leyendo la ronda de GeneradorHordas

public class HUDRonda : MonoBehaviour
{
    public static HUDRonda Instancia { get; private set; }

    private Text textoRonda;
    private Text textoSombra;
    private int rondaMostrada = 0;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Buscar y ocultar CUALQUIER texto antiguo que contenga "Ronda"
        // Busca en TMPro (TextMeshProUGUI)
        var todosLosTMP = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tmp in todosLosTMP)
        {
            if (tmp.text != null && tmp.text.Contains("onda"))
            {
                tmp.gameObject.SetActive(false);
            }
        }
        // Busca en Text normal (Unity UI)
        var todosLosText = FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (var txt in todosLosText)
        {
            if (txt.text != null && txt.text.Contains("onda"))
            {
                txt.gameObject.SetActive(false);
            }
        }

        CrearHUD();
    }

    public void ActualizarRonda(int ronda)
    {
        if (ronda == rondaMostrada) return;
        rondaMostrada = ronda;

        string romano = ConvertirARomano(ronda);

        if (textoRonda != null) textoRonda.text = romano;
        if (textoSombra != null) textoSombra.text = romano;
    }

    void CrearHUD()
    {
        // Canvas para el HUD en overlay
        GameObject canvasObj = new GameObject("CanvasHUDRonda");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Contenedor arriba-derecha
        GameObject contenedor = new GameObject("ContenedorRonda");
        contenedor.transform.SetParent(canvasObj.transform, false);
        RectTransform rtCont = contenedor.AddComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(1f, 1f);
        rtCont.anchorMax = new Vector2(1f, 1f);
        rtCont.pivot = new Vector2(1f, 1f);
        rtCont.anchoredPosition = new Vector2(-30, -15);
        rtCont.sizeDelta = new Vector2(200, 100);

        // === CAPA 1: Sombra roja de sangre (efecto profundidad) ===
        GameObject sombraObj = new GameObject("SombraRonda");
        sombraObj.transform.SetParent(contenedor.transform, false);
        RectTransform rtSombra = sombraObj.AddComponent<RectTransform>();
        rtSombra.anchorMin = Vector2.zero;
        rtSombra.anchorMax = Vector2.one;
        rtSombra.offsetMin = new Vector2(3, -3);
        rtSombra.offsetMax = new Vector2(3, -3);

        textoSombra = sombraObj.AddComponent<Text>();
        textoSombra.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoSombra.fontSize = 72;
        textoSombra.fontStyle = FontStyle.Bold;
        textoSombra.color = new Color(0.3f, 0.0f, 0.0f, 0.6f); // Sombra roja oscura
        textoSombra.alignment = TextAnchor.MiddleRight;
        textoSombra.text = "I";

        // === CAPA 2: Texto principal (rojo sangre) ===
        GameObject textoObj = new GameObject("TextoRonda");
        textoObj.transform.SetParent(contenedor.transform, false);
        RectTransform rtTexto = textoObj.AddComponent<RectTransform>();
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        textoRonda = textoObj.AddComponent<Text>();
        textoRonda.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoRonda.fontSize = 72;
        textoRonda.fontStyle = FontStyle.Bold;
        textoRonda.color = new Color(0.7f, 0.05f, 0.02f, 0.95f); // Rojo sangre
        textoRonda.alignment = TextAnchor.MiddleRight;
        textoRonda.text = "I";

        // Efecto de relieve sangriento
        Shadow sombra1 = textoObj.AddComponent<Shadow>();
        sombra1.effectColor = new Color(0.9f, 0.1f, 0.0f, 0.3f);
        sombra1.effectDistance = new Vector2(1, -1);

        Outline contorno = textoObj.AddComponent<Outline>();
        contorno.effectColor = new Color(0.2f, 0.0f, 0.0f, 0.5f);
        contorno.effectDistance = new Vector2(2, -2);
    }

    string ConvertirARomano(int numero)
    {
        if (numero <= 0) return "I";
        if (numero > 3999) return numero.ToString();

        string[] miles = { "", "M", "MM", "MMM" };
        string[] cientos = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
        string[] decenas = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
        string[] unidades = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

        return miles[numero / 1000] +
               cientos[(numero % 1000) / 100] +
               decenas[(numero % 100) / 10] +
               unidades[numero % 10];
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }
}
