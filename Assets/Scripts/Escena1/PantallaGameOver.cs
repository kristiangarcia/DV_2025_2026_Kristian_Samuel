using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;

// Pantalla de Game Over - DeadWave
// Se muestra cuando el jugador muere (vida <= 0)
// Construye toda la UI por código con temática de terror

public class PantallaGameOver : MonoBehaviour
{
    private Canvas canvas;
    private Text tituloTexto;
    private Text subtituloTexto;
    private bool activa = false;

    // Colores temáticos horror (mismos que el menú principal)
    private Color colorRojo = new Color(0.7f, 0.05f, 0.05f, 1f);
    private Color colorTexto = new Color(0.85f, 0.82f, 0.78f, 1f);
    private Color colorBotonNormal = new Color(0.12f, 0.08f, 0.08f, 0.9f);
    private Color colorRojoOscuro = new Color(0.4f, 0.02f, 0.02f, 1f);
    private Color colorSombra = new Color(0, 0, 0, 0.8f);

    // Singleton para acceso fácil
    public static PantallaGameOver Instancia { get; private set; }

    void Awake()
    {
        Instancia = this;
    }

    /// <summary>
    /// Llamar cuando el jugador muera para mostrar la pantalla de Game Over
    /// </summary>
    public void MostrarGameOver()
    {
        if (activa) return;
        activa = true;

        // Pausar el juego
        Time.timeScale = 0f;

        // Desbloquear cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CrearEventSystem();
        CrearCanvas();
        CrearUI();

        StartCoroutine(EfectoFadeIn());
        StartCoroutine(EfectoParpadeoTitulo());
    }

    void CrearEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem_GameOver");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<InputSystemUIInputModule>();
        }
    }

    void CrearCanvas()
    {
        GameObject canvasObj = new GameObject("CanvasGameOver");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // Por encima de todo

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    void CrearUI()
    {
        // ---- FONDO OSCURO con fade ----
        GameObject fondoObj = CrearPanel("FondoGameOver", canvas.transform, Vector2.zero, Vector2.zero, Vector2.one);
        Image fondoImg = fondoObj.GetComponent<Image>();
        fondoImg.color = new Color(0.02f, 0, 0, 0f); // Empieza transparente

        // Líneas decorativas de sangre
        for (int i = 0; i < 7; i++)
        {
            GameObject lineaObj = new GameObject("LineaSangre_" + i);
            lineaObj.transform.SetParent(canvas.transform, false);
            RectTransform rt = lineaObj.AddComponent<RectTransform>();
            float posY = Random.Range(0.05f, 0.95f);
            rt.anchorMin = new Vector2(0, posY);
            rt.anchorMax = new Vector2(1, posY + 0.002f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image lineaImg = lineaObj.AddComponent<Image>();
            lineaImg.color = new Color(0.5f, 0, 0, Random.Range(0.05f, 0.15f));
        }

        // ---- TÍTULO "HAS MUERTO" ----
        GameObject tituloObj = new GameObject("TituloGameOver");
        tituloObj.transform.SetParent(canvas.transform, false);
        RectTransform rtTitulo = tituloObj.AddComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0.5f, 0.65f);
        rtTitulo.anchorMax = new Vector2(0.5f, 0.65f);
        rtTitulo.sizeDelta = new Vector2(900, 150);
        rtTitulo.anchoredPosition = Vector2.zero;

        Shadow sombra = tituloObj.AddComponent<Shadow>();
        sombra.effectColor = new Color(0.8f, 0, 0, 0.6f);
        sombra.effectDistance = new Vector2(4, -4);

        Outline outline = tituloObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.5f, 0, 0, 0.4f);
        outline.effectDistance = new Vector2(3, -3);

        tituloTexto = tituloObj.AddComponent<Text>();
        tituloTexto.text = "HAS MUERTO";
        tituloTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tituloTexto.fontSize = 100;
        tituloTexto.fontStyle = FontStyle.Bold;
        tituloTexto.color = new Color(0.7f, 0.05f, 0.05f, 0f); // Empieza transparente
        tituloTexto.alignment = TextAnchor.MiddleCenter;

        // ---- SUBTÍTULO ----
        GameObject subObj = new GameObject("SubtituloGameOver");
        subObj.transform.SetParent(canvas.transform, false);
        RectTransform rtSub = subObj.AddComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 0.55f);
        rtSub.anchorMax = new Vector2(0.5f, 0.55f);
        rtSub.sizeDelta = new Vector2(600, 40);
        rtSub.anchoredPosition = Vector2.zero;

        subtituloTexto = subObj.AddComponent<Text>();
        subtituloTexto.text = "L A S   O L E A D A S   T E   H A N   C O N S U M I D O";
        subtituloTexto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtituloTexto.fontSize = 20;
        subtituloTexto.color = new Color(0.5f, 0.45f, 0.4f, 0f);
        subtituloTexto.alignment = TextAnchor.MiddleCenter;

        // ---- SEPARADOR ----
        CrearSeparador(canvas.transform, 0.50f);

        // ---- BOTONES ----
        CrearBoton("Volver a Empezar", canvas.transform, 0.40f, () => VolverAEmpezar());
        CrearBoton("Menú Principal", canvas.transform, 0.30f, () => IrAlMenu());
        CrearBoton("Salir", canvas.transform, 0.20f, () => SalirJuego());
    }

    // ===================== FUNCIONES DE BOTONES =====================
    void VolverAEmpezar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    void SalirJuego()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ===================== EFECTOS =====================
    IEnumerator EfectoFadeIn()
    {
        float t = 0;
        Image fondo = canvas.transform.Find("FondoGameOver").GetComponent<Image>();

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 0.8f; // unscaledDeltaTime porque el juego está pausado

            // Fade del fondo
            Color cf = fondo.color;
            cf.a = Mathf.Lerp(0, 0.92f, t);
            fondo.color = cf;

            // Fade del título
            if (tituloTexto != null)
            {
                Color ct = tituloTexto.color;
                ct.a = Mathf.Lerp(0, 1f, t);
                tituloTexto.color = ct;
            }

            // Fade del subtítulo (más lento)
            if (subtituloTexto != null && t > 0.3f)
            {
                Color cs = subtituloTexto.color;
                cs.a = Mathf.Lerp(0, 0.7f, (t - 0.3f) / 0.7f);
                subtituloTexto.color = cs;
            }

            yield return null;
        }
    }

    IEnumerator EfectoParpadeoTitulo()
    {
        // Esperar al fade in
        yield return new WaitForSecondsRealtime(1.5f);

        while (true)
        {
            float intensidad = 0.7f + Mathf.Sin(Time.unscaledTime * 2f) * 0.3f;
            float glitch = Random.Range(0f, 1f) > 0.95f ? Random.Range(0.2f, 0.5f) : 0f;

            if (tituloTexto != null)
            {
                Color c = colorRojo;
                c.a = intensidad - glitch;
                tituloTexto.color = c;
            }

            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    // ===================== UTILIDADES =====================
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

    void CrearBoton(string texto, Transform padre, float posY, UnityEngine.Events.UnityAction accion)
    {
        GameObject botonObj = new GameObject("Boton_" + texto);
        botonObj.transform.SetParent(padre, false);
        RectTransform rt = botonObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, posY);
        rt.anchorMax = new Vector2(0.5f, posY);
        rt.sizeDelta = new Vector2(380, 55);
        rt.anchoredPosition = Vector2.zero;

        Image imgBoton = botonObj.AddComponent<Image>();
        imgBoton.color = colorBotonNormal;

        Outline outlineBtn = botonObj.AddComponent<Outline>();
        outlineBtn.effectColor = new Color(0.4f, 0.02f, 0.02f, 0.6f);
        outlineBtn.effectDistance = new Vector2(1, -1);

        Shadow sombraBtn = botonObj.AddComponent<Shadow>();
        sombraBtn.effectColor = colorSombra;
        sombraBtn.effectDistance = new Vector2(3, -3);

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
        txt.fontSize = 26;
        txt.fontStyle = FontStyle.Bold;
        txt.color = colorTexto;
        txt.alignment = TextAnchor.MiddleCenter;
    }

    void CrearSeparador(Transform padre, float posY)
    {
        GameObject sepObj = new GameObject("Separador");
        sepObj.transform.SetParent(padre, false);
        RectTransform rt = sepObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.25f, posY);
        rt.anchorMax = new Vector2(0.75f, posY);
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
