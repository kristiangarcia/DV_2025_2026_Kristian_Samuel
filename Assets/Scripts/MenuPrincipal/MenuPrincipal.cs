using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;

// Menú Principal - DeadWave
// Construye toda la UI del menú por código con temática de terror

public class MenuPrincipal : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panelPrincipal;
    private GameObject panelOpciones;
    private ControlOpciones controlOpciones;

    // Colores temáticos horror
    private Color colorFondo = new Color(0.02f, 0.02f, 0.05f, 0.95f);
    private Color colorRojo = new Color(0.7f, 0.05f, 0.05f, 1f);
    private Color colorRojoOscuro = new Color(0.4f, 0.02f, 0.02f, 1f);
    private Color colorTexto = new Color(0.85f, 0.82f, 0.78f, 1f);
    private Color colorBotonNormal = new Color(0.12f, 0.08f, 0.08f, 0.9f);
    private Color colorBotonHover = new Color(0.25f, 0.05f, 0.05f, 1f);
    private Color colorBotonPresionado = new Color(0.5f, 0.02f, 0.02f, 1f);
    private Color colorSombra = new Color(0, 0, 0, 0.8f);

    private Text tituloTexto;
    private Text subtituloTexto;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CrearEventSystem();
        CrearCanvas();
        CrearFondo();
        CrearPanelPrincipal();
        CrearPanelOpciones();

        panelOpciones.SetActive(false);

        StartCoroutine(EfectoParpadeoTitulo());
        StartCoroutine(EfectoSubtitulo());
    }

    // ===================== EVENT SYSTEM =====================
    void CrearEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
        }
    }

    // ===================== CANVAS =====================
    void CrearCanvas()
    {
        GameObject canvasObj = new GameObject("CanvasMenu");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    // ===================== FONDO OSCURO =====================
    void CrearFondo()
    {
        // Fondo principal negro
        GameObject fondoObj = CrearPanel("Fondo", canvas.transform, Vector2.zero, new Vector2(0, 0), new Vector2(1, 1));
        Image fondoImg = fondoObj.GetComponent<Image>();
        fondoImg.color = new Color(0.01f, 0.01f, 0.02f, 1f);

        // Viñeta/gradiente oscuro desde los bordes
        GameObject vinetaObj = CrearPanel("Vineta", canvas.transform, Vector2.zero, new Vector2(0, 0), new Vector2(1, 1));
        Image vinetaImg = vinetaObj.GetComponent<Image>();
        vinetaImg.color = new Color(0.05f, 0, 0, 0.3f);

        // Líneas decorativas horizontales (efecto de escaneo)
        for (int i = 0; i < 5; i++)
        {
            GameObject lineaObj = new GameObject("LineaHorror_" + i);
            lineaObj.transform.SetParent(canvas.transform, false);
            RectTransform rt = lineaObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.1f + i * 0.18f);
            rt.anchorMax = new Vector2(1, 0.1f + i * 0.18f + 0.001f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image lineaImg = lineaObj.AddComponent<Image>();
            lineaImg.color = new Color(0.3f, 0, 0, 0.08f);
        }
    }

    // ===================== PANEL PRINCIPAL =====================
    void CrearPanelPrincipal()
    {
        panelPrincipal = CrearPanel("PanelPrincipal", canvas.transform, Vector2.zero, new Vector2(0, 0), new Vector2(1, 1));
        panelPrincipal.GetComponent<Image>().color = Color.clear;

        // ---- TÍTULO "DEADWAVE" ----
        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(panelPrincipal.transform, false);
        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0.5f, 0.75f);
        rtTitulo.anchorMax = new Vector2(0.5f, 0.75f);
        rtTitulo.sizeDelta = new Vector2(900, 150);
        rtTitulo.anchoredPosition = Vector2.zero;

        // Sombra del título
        Shadow sombra = tituloObj.AddComponent<Shadow>();
        sombra.effectColor = new Color(0.6f, 0, 0, 0.7f);
        sombra.effectDistance = new Vector2(3, -3);

        Outline outline = tituloObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0, 0, 0.5f);
        outline.effectDistance = new Vector2(2, -2);

        tituloTexto = tituloObj.AddComponent<Text>();
        tituloTexto.text = "DEADWAVE";
        tituloTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tituloTexto.fontSize = 110;
        tituloTexto.fontStyle = FontStyle.Bold;
        tituloTexto.color = colorRojo;
        tituloTexto.alignment = TextAnchor.MiddleCenter;

        // ---- SUBTÍTULO ----
        GameObject subObj = new GameObject("Subtitulo");
        subObj.transform.SetParent(panelPrincipal.transform, false);
        RectTransform rtSub = subObj.AddComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 0.65f);
        rtSub.anchorMax = new Vector2(0.5f, 0.65f);
        rtSub.sizeDelta = new Vector2(600, 40);
        rtSub.anchoredPosition = Vector2.zero;

        subtituloTexto = subObj.AddComponent<Text>();
        subtituloTexto.text = "S U R V I V E   T H E   W A V E S";
        subtituloTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtituloTexto.fontSize = 22;
        subtituloTexto.color = new Color(0.6f, 0.55f, 0.5f, 0f);
        subtituloTexto.alignment = TextAnchor.MiddleCenter;

        // ---- SEPARADOR ----
        CrearSeparador(panelPrincipal.transform, 0.60f);

        // ---- BOTONES ----
        float posY = 0.48f;
        float separacion = 0.1f;

        CrearBotonMenu("Empezar Juego", panelPrincipal.transform, posY, () => EmpezarJuego());
        CrearBotonMenu("Opciones", panelPrincipal.transform, posY - separacion, () => MostrarOpciones());
        CrearBotonMenu("Salir", panelPrincipal.transform, posY - separacion * 2, () => SalirJuego());

        // ---- CRÉDITO INFERIOR ----
        GameObject creditoObj = new GameObject("Credito");
        creditoObj.transform.SetParent(panelPrincipal.transform, false);
        RectTransform rtCred = creditoObj.AddComponent<RectTransform>();
        rtCred.anchorMin = new Vector2(0.5f, 0.05f);
        rtCred.anchorMax = new Vector2(0.5f, 0.05f);
        rtCred.sizeDelta = new Vector2(400, 30);
        rtCred.anchoredPosition = Vector2.zero;

        Text credTexto = creditoObj.AddComponent<Text>();
        credTexto.text = "© 2025 DeadWave - All Rights Reserved";
        credTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        credTexto.fontSize = 14;
        credTexto.color = new Color(0.3f, 0.28f, 0.25f, 0.6f);
        credTexto.alignment = TextAnchor.MiddleCenter;
    }

    // ===================== PANEL DE OPCIONES =====================
    void CrearPanelOpciones()
    {
        // Fondo oscuro que cubre toda la pantalla
        panelOpciones = CrearPanel("PanelOpciones", canvas.transform, Vector2.zero, new Vector2(0, 0), new Vector2(1, 1));
        panelOpciones.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);

        // Panel central de opciones
        GameObject panelCentral = CrearPanel("PanelCentralOpciones", panelOpciones.transform, 
            Vector2.zero, new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.9f));
        Image panelImg = panelCentral.GetComponent<Image>();
        panelImg.color = new Color(0.06f, 0.04f, 0.04f, 0.95f);

        Outline outlinePanel = panelCentral.AddComponent<Outline>();
        outlinePanel.effectColor = colorRojoOscuro;
        outlinePanel.effectDistance = new Vector2(2, -2);

        // Título Opciones
        GameObject tituloOpcion = new GameObject("TituloOpciones");
        tituloOpcion.transform.SetParent(panelCentral.transform, false);
        RectTransform rtTit = tituloOpcion.AddComponent<RectTransform>();
        rtTit.anchorMin = new Vector2(0.5f, 0.88f);
        rtTit.anchorMax = new Vector2(0.5f, 0.88f);
        rtTit.sizeDelta = new Vector2(400, 60);
        rtTit.anchoredPosition = Vector2.zero;

        Text txtTitOpc = tituloOpcion.AddComponent<Text>();
        txtTitOpc.text = "OPCIONES";
        txtTitOpc.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtTitOpc.fontSize = 48;
        txtTitOpc.fontStyle = FontStyle.Bold;
        txtTitOpc.color = colorRojo;
        txtTitOpc.alignment = TextAnchor.MiddleCenter;

        Shadow sombraTit = tituloOpcion.AddComponent<Shadow>();
        sombraTit.effectColor = new Color(0.5f, 0, 0, 0.5f);
        sombraTit.effectDistance = new Vector2(2, -2);

        CrearSeparador(panelCentral.transform, 0.83f);

        // Añadir componente ControlOpciones
        controlOpciones = panelOpciones.AddComponent<ControlOpciones>();
        controlOpciones.Inicializar(panelCentral.transform, colorTexto, colorRojo, colorRojoOscuro, colorBotonNormal, this);
    }

    // ===================== FUNCIONES DE BOTONES =====================
    public void EmpezarJuego()
    {
        SceneManager.LoadScene("Main");
    }

    public void MostrarOpciones()
    {
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(true);
    }

    public void OcultarOpciones()
    {
        panelOpciones.SetActive(false);
        panelPrincipal.SetActive(true);
    }

    void SalirJuego()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ===================== EFECTOS VISUALES =====================
    IEnumerator EfectoParpadeoTitulo()
    {
        while (true)
        {
            // Parpadeo sutil del título
            float intensidad = 0.7f + Mathf.Sin(Time.time * 1.5f) * 0.3f;
            float glitch = Random.Range(0f, 1f) > 0.97f ? Random.Range(0.3f, 0.6f) : 0f;
            
            Color c = colorRojo;
            c.a = intensidad - glitch;
            if (tituloTexto != null)
                tituloTexto.color = c;

            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator EfectoSubtitulo()
    {
        // Fade in gradual del subtítulo
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 0.5f;
            if (subtituloTexto != null)
            {
                Color c = subtituloTexto.color;
                c.a = Mathf.Lerp(0, 0.7f, t);
                subtituloTexto.color = c;
            }
            yield return null;
        }
    }

    // ===================== UTILIDADES DE UI =====================
    GameObject CrearPanel(string nombre, Transform padre, Vector2 pos, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        obj.AddComponent<Image>();
        return obj;
    }

    void CrearBotonMenu(string texto, Transform padre, float posY, UnityEngine.Events.UnityAction accion)
    {
        // Container del botón
        GameObject botonObj = new GameObject("Boton_" + texto);
        botonObj.transform.SetParent(padre, false);
        RectTransform rt = botonObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, posY);
        rt.anchorMax = new Vector2(0.5f, posY);
        rt.sizeDelta = new Vector2(380, 60);
        rt.anchoredPosition = Vector2.zero;

        // Imagen de fondo del botón
        Image imgBoton = botonObj.AddComponent<Image>();
        imgBoton.color = colorBotonNormal;

        // Outline rojo sutil
        Outline outlineBtn = botonObj.AddComponent<Outline>();
        outlineBtn.effectColor = new Color(0.4f, 0.02f, 0.02f, 0.6f);
        outlineBtn.effectDistance = new Vector2(1, -1);

        // Sombra
        Shadow sombraBtn = botonObj.AddComponent<Shadow>();
        sombraBtn.effectColor = colorSombra;
        sombraBtn.effectDistance = new Vector2(3, -3);

        // Componente Button
        Button boton = botonObj.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = Color.white;
        colores.highlightedColor = new Color(1.8f, 0.6f, 0.6f, 1f);
        colores.pressedColor = new Color(2.5f, 0.3f, 0.3f, 1f);
        colores.selectedColor = new Color(1.5f, 0.5f, 0.5f, 1f);
        colores.fadeDuration = 0.15f;
        boton.colors = colores;
        boton.targetGraphic = imgBoton;
        boton.onClick.AddListener(accion);

        // Texto del botón
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        RectTransform rtTxt = textoObj.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        Text txt = textoObj.AddComponent<Text>();
        txt.text = texto.ToUpper();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 28;
        txt.fontStyle = FontStyle.Bold;
        txt.color = colorTexto;
        txt.alignment = TextAnchor.MiddleCenter;
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
}
