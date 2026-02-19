using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Pantalla de Pausa - DeadWave
// Se activa con ESC, pausa el juego y muestra opciones

public class PantallaPausa : MonoBehaviour
{
    public static PantallaPausa Instancia { get; private set; }
    private Canvas canvas;
    private bool pausado = false;

    void Awake()
    {
        Instancia = this;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (pausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        if (pausado) return;
        pausado = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CrearUI();
    }

    public void Reanudar()
    {
        if (!pausado) return;
        pausado = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (canvas != null) Destroy(canvas.gameObject);
    }

    void CrearUI()
    {
        // Canvas
        GameObject canvasObj = new GameObject("CanvasPausa");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // ===== FONDO OSCURO SEMITRANSPARENTE =====
        GameObject fondoObj = new GameObject("Fondo");
        fondoObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rtFondo = fondoObj.AddComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;
        Image imgFondo = fondoObj.AddComponent<Image>();
        imgFondo.color = new Color(0.02f, 0, 0, 0.85f);

        // ===== PANEL CENTRAL =====
        GameObject panelObj = new GameObject("PanelCentral");
        panelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rtPanel = panelObj.AddComponent<RectTransform>();
        rtPanel.anchorMin = new Vector2(0.5f, 0.5f);
        rtPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rtPanel.sizeDelta = new Vector2(500, 450);
        rtPanel.anchoredPosition = Vector2.zero;
        Image imgPanel = panelObj.AddComponent<Image>();
        imgPanel.color = new Color(0.05f, 0.02f, 0.02f, 0.9f);

        // Borde del panel
        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.5f, 0.05f, 0.05f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        // ===== SEPARADOR SUPERIOR =====
        CrearLinea(panelObj.transform, 0.88f);

        // ===== TÍTULO: PAUSA =====
        CrearTexto(panelObj.transform, "P A U S A", 64, FontStyle.Bold,
            new Color(0.8f, 0.05f, 0.05f, 1f), new Vector2(0, 130), true);

        // ===== SEPARADOR INFERIOR DEL TÍTULO =====
        CrearLinea(panelObj.transform, 0.62f);

        // ===== BOTÓN: REANUDAR =====
        CrearBoton(panelObj.transform, "REANUDAR", new Vector2(0, 30), () => Reanudar());

        // ===== BOTÓN: REINICIAR =====
        CrearBoton(panelObj.transform, "REINICIAR", new Vector2(0, -40), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        // ===== BOTÓN: MENÚ PRINCIPAL =====
        CrearBoton(panelObj.transform, "MENÚ PRINCIPAL", new Vector2(0, -110), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MenuPrincipal");
        });

        // ===== BOTÓN: SALIR =====
        CrearBoton(panelObj.transform, "SALIR DEL JUEGO", new Vector2(0, -180), () =>
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }

    void CrearTexto(Transform padre, string contenido, int tamaño, FontStyle estilo,
        Color color, Vector2 posicion, bool conSombra)
    {
        GameObject obj = new GameObject("Texto_" + contenido);
        obj.transform.SetParent(padre, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(450, tamaño + 20);
        rt.anchoredPosition = posicion;

        if (conSombra)
        {
            Shadow sombra = obj.AddComponent<Shadow>();
            sombra.effectColor = new Color(0.8f, 0, 0, 0.4f);
            sombra.effectDistance = new Vector2(3, -3);
        }

        Text texto = obj.AddComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = tamaño;
        texto.fontStyle = estilo;
        texto.color = color;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.text = contenido;
    }

    void CrearBoton(Transform padre, string texto, Vector2 posicion, UnityEngine.Events.UnityAction accion)
    {
        // Contenedor del botón
        GameObject btnObj = new GameObject("Boton_" + texto);
        btnObj.transform.SetParent(padre, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(350, 55);
        rt.anchoredPosition = posicion;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.03f, 0.03f, 0.8f);

        // Borde
        Outline outl = btnObj.AddComponent<Outline>();
        outl.effectColor = new Color(0.5f, 0.05f, 0.05f, 0.3f);
        outl.effectDistance = new Vector2(1, -1);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(accion);

        // Colores del botón
        ColorBlock colores = btn.colors;
        colores.normalColor = new Color(0.15f, 0.03f, 0.03f, 0.8f);
        colores.highlightedColor = new Color(0.4f, 0.05f, 0.05f, 0.9f);
        colores.pressedColor = new Color(0.6f, 0.05f, 0.05f, 1f);
        colores.selectedColor = colores.highlightedColor;
        btn.colors = colores;

        // Texto del botón
        GameObject txtObj = new GameObject("TextoBoton");
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform rtTxt = txtObj.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        Text txtComp = txtObj.AddComponent<Text>();
        txtComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtComp.fontSize = 24;
        txtComp.fontStyle = FontStyle.Bold;
        txtComp.color = new Color(0.8f, 0.7f, 0.65f, 1f);
        txtComp.alignment = TextAnchor.MiddleCenter;
        txtComp.text = texto;
    }

    void CrearLinea(Transform padre, float posY)
    {
        GameObject obj = new GameObject("Linea");
        obj.transform.SetParent(padre, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, posY);
        rt.anchorMax = new Vector2(0.9f, posY);
        rt.sizeDelta = new Vector2(0, 2);
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.5f, 0.05f, 0.05f, 0.4f);
    }

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }
}
