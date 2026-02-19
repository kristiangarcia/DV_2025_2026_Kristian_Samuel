using UnityEngine;
using UnityEngine.UI;

// Panel de Opciones - DeadWave
// Controla los ajustes del juego: volumen, sensibilidad, resolución, pantalla completa
// Todos los valores se guardan con PlayerPrefs

public class ControlOpciones : MonoBehaviour
{
    private Slider sliderMusica;
    private Slider sliderEfectos;
    private Slider sliderSensibilidad;
    private Toggle togglePantallaCompleta;
    private Dropdown dropdownResolucion;
    private Text txtValorMusica;
    private Text txtValorEfectos;
    private Text txtValorSensibilidad;

    private MenuPrincipal menuPrincipal;
    private Resolution[] resoluciones;

    // Colores recibidos del menú principal
    private Color colorTexto;
    private Color colorRojo;
    private Color colorRojoOscuro;
    private Color colorBoton;

    public void Inicializar(Transform panelPadre, Color textoColor, Color rojoColor, Color rojoOscColor, Color botonColor, MenuPrincipal menu)
    {
        colorTexto = textoColor;
        colorRojo = rojoColor;
        colorRojoOscuro = rojoOscColor;
        colorBoton = botonColor;
        menuPrincipal = menu;

        float posY = 0.72f;
        float separacion = 0.14f;

        // Volumen Música
        sliderMusica = CrearOpcionSlider("Volumen Música", panelPadre, posY, 0f, 100f,
            PlayerPrefs.GetFloat("VolumenMusica", 80f));
        txtValorMusica = sliderMusica.transform.parent.Find("Valor").GetComponent<Text>();
        sliderMusica.onValueChanged.AddListener((v) => {
            PlayerPrefs.SetFloat("VolumenMusica", v);
            txtValorMusica.text = Mathf.RoundToInt(v).ToString() + "%";
        });

        // Volumen Efectos
        posY -= separacion;
        sliderEfectos = CrearOpcionSlider("Volumen Efectos", panelPadre, posY, 0f, 100f,
            PlayerPrefs.GetFloat("VolumenEfectos", 80f));
        txtValorEfectos = sliderEfectos.transform.parent.Find("Valor").GetComponent<Text>();
        sliderEfectos.onValueChanged.AddListener((v) => {
            PlayerPrefs.SetFloat("VolumenEfectos", v);
            txtValorEfectos.text = Mathf.RoundToInt(v).ToString() + "%";
        });

        // Sensibilidad del Ratón
        posY -= separacion;
        sliderSensibilidad = CrearOpcionSlider("Sensibilidad Ratón", panelPadre, posY, 0.1f, 10f,
            PlayerPrefs.GetFloat("Sensibilidad", 2f));
        txtValorSensibilidad = sliderSensibilidad.transform.parent.Find("Valor").GetComponent<Text>();
        sliderSensibilidad.onValueChanged.AddListener((v) => {
            PlayerPrefs.SetFloat("Sensibilidad", v);
            txtValorSensibilidad.text = v.ToString("F1");
        });

        // Pantalla Completa
        posY -= separacion;
        togglePantallaCompleta = CrearOpcionToggle("Pantalla Completa", panelPadre, posY,
            PlayerPrefs.GetInt("PantallaCompleta", 1) == 1);
        togglePantallaCompleta.onValueChanged.AddListener((v) => {
            Screen.fullScreen = v;
            PlayerPrefs.SetInt("PantallaCompleta", v ? 1 : 0);
        });

        // Resolución
        posY -= separacion;
        dropdownResolucion = CrearOpcionDropdown("Resolución", panelPadre, posY);

        // Botón Volver
        CrearBotonVolver(panelPadre);

        CargarResoluciones();
    }

    void CargarResoluciones()
    {
        resoluciones = Screen.resolutions;
        dropdownResolucion.ClearOptions();

        var opciones = new System.Collections.Generic.List<string>();
        int indiceActual = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            // Evitar duplicados
            if (!opciones.Contains(opcion))
            {
                opciones.Add(opcion);
            }
            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                indiceActual = opciones.Count - 1;
            }
        }

        dropdownResolucion.AddOptions(opciones);
        dropdownResolucion.value = indiceActual;
        dropdownResolucion.RefreshShownValue();

        dropdownResolucion.onValueChanged.AddListener((indice) => {
            // Buscar la resolución correspondiente
            string seleccion = dropdownResolucion.options[indice].text;
            string[] partes = seleccion.Split('x');
            int ancho = int.Parse(partes[0].Trim());
            int alto = int.Parse(partes[1].Trim());
            Screen.SetResolution(ancho, alto, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolucionAncho", ancho);
            PlayerPrefs.SetInt("ResolucionAlto", alto);
        });
    }

    // ===================== CREADORES DE CONTROLES =====================

    Slider CrearOpcionSlider(string etiqueta, Transform padre, float posY, float min, float max, float valorInicial)
    {
        // Container
        GameObject container = new GameObject("Opcion_" + etiqueta);
        container.transform.SetParent(padre, false);
        RectTransform rtCont = container.AddComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.1f, posY - 0.04f);
        rtCont.anchorMax = new Vector2(0.9f, posY + 0.04f);
        rtCont.offsetMin = Vector2.zero;
        rtCont.offsetMax = Vector2.zero;

        // Etiqueta
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        RectTransform rtLabel = labelObj.AddComponent<RectTransform>();
        rtLabel.anchorMin = new Vector2(0, 0);
        rtLabel.anchorMax = new Vector2(0.35f, 1);
        rtLabel.offsetMin = Vector2.zero;
        rtLabel.offsetMax = Vector2.zero;

        Text txtLabel = labelObj.AddComponent<Text>();
        txtLabel.text = etiqueta;
        txtLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtLabel.fontSize = 22;
        txtLabel.color = colorTexto;
        txtLabel.alignment = TextAnchor.MiddleLeft;

        // Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(container.transform, false);
        RectTransform rtSlider = sliderObj.AddComponent<RectTransform>();
        rtSlider.anchorMin = new Vector2(0.38f, 0.2f);
        rtSlider.anchorMax = new Vector2(0.82f, 0.8f);
        rtSlider.offsetMin = Vector2.zero;
        rtSlider.offsetMax = Vector2.zero;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = valorInicial;

        // Background del slider
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform rtBg = bgObj.AddComponent<RectTransform>();
        rtBg.anchorMin = new Vector2(0, 0.35f);
        rtBg.anchorMax = new Vector2(1, 0.65f);
        rtBg.offsetMin = Vector2.zero;
        rtBg.offsetMax = Vector2.zero;
        Image imgBg = bgObj.AddComponent<Image>();
        imgBg.color = new Color(0.15f, 0.1f, 0.1f, 1f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform rtFillArea = fillArea.AddComponent<RectTransform>();
        rtFillArea.anchorMin = new Vector2(0, 0.35f);
        rtFillArea.anchorMax = new Vector2(1, 0.65f);
        rtFillArea.offsetMin = new Vector2(5, 0);
        rtFillArea.offsetMax = new Vector2(-5, 0);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform rtFill = fillObj.AddComponent<RectTransform>();
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.offsetMin = Vector2.zero;
        rtFill.offsetMax = Vector2.zero;
        Image imgFill = fillObj.AddComponent<Image>();
        imgFill.color = colorRojo;
        slider.fillRect = rtFill;

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform rtHandleArea = handleArea.AddComponent<RectTransform>();
        rtHandleArea.anchorMin = new Vector2(0, 0);
        rtHandleArea.anchorMax = new Vector2(1, 1);
        rtHandleArea.offsetMin = new Vector2(10, 0);
        rtHandleArea.offsetMax = new Vector2(-10, 0);

        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleArea.transform, false);
        RectTransform rtHandle = handleObj.AddComponent<RectTransform>();
        rtHandle.sizeDelta = new Vector2(20, 0);
        Image imgHandle = handleObj.AddComponent<Image>();
        imgHandle.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        slider.handleRect = rtHandle;
        slider.targetGraphic = imgHandle;

        // Valor numérico
        GameObject valorObj = new GameObject("Valor");
        valorObj.transform.SetParent(container.transform, false);
        RectTransform rtValor = valorObj.AddComponent<RectTransform>();
        rtValor.anchorMin = new Vector2(0.85f, 0);
        rtValor.anchorMax = new Vector2(1f, 1);
        rtValor.offsetMin = Vector2.zero;
        rtValor.offsetMax = Vector2.zero;

        Text txtValor = valorObj.AddComponent<Text>();
        if (max <= 10f)
            txtValor.text = valorInicial.ToString("F1");
        else
            txtValor.text = Mathf.RoundToInt(valorInicial).ToString() + "%";
        txtValor.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtValor.fontSize = 20;
        txtValor.color = colorRojo;
        txtValor.alignment = TextAnchor.MiddleCenter;

        return slider;
    }

    Toggle CrearOpcionToggle(string etiqueta, Transform padre, float posY, bool valorInicial)
    {
        // Container
        GameObject container = new GameObject("Opcion_" + etiqueta);
        container.transform.SetParent(padre, false);
        RectTransform rtCont = container.AddComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.1f, posY - 0.04f);
        rtCont.anchorMax = new Vector2(0.9f, posY + 0.04f);
        rtCont.offsetMin = Vector2.zero;
        rtCont.offsetMax = Vector2.zero;

        // Etiqueta
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        RectTransform rtLabel = labelObj.AddComponent<RectTransform>();
        rtLabel.anchorMin = new Vector2(0, 0);
        rtLabel.anchorMax = new Vector2(0.35f, 1);
        rtLabel.offsetMin = Vector2.zero;
        rtLabel.offsetMax = Vector2.zero;

        Text txtLabel = labelObj.AddComponent<Text>();
        txtLabel.text = etiqueta;
        txtLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtLabel.fontSize = 22;
        txtLabel.color = colorTexto;
        txtLabel.alignment = TextAnchor.MiddleLeft;

        // Toggle
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(container.transform, false);
        RectTransform rtToggle = toggleObj.AddComponent<RectTransform>();
        rtToggle.anchorMin = new Vector2(0.38f, 0.15f);
        rtToggle.anchorMax = new Vector2(0.45f, 0.85f);
        rtToggle.offsetMin = Vector2.zero;
        rtToggle.offsetMax = Vector2.zero;

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = valorInicial;

        // Fondo del toggle
        Image imgToggleBg = toggleObj.AddComponent<Image>();
        imgToggleBg.color = new Color(0.15f, 0.1f, 0.1f, 1f);

        // Checkmark
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(toggleObj.transform, false);
        RectTransform rtCheck = checkObj.AddComponent<RectTransform>();
        rtCheck.anchorMin = new Vector2(0.15f, 0.15f);
        rtCheck.anchorMax = new Vector2(0.85f, 0.85f);
        rtCheck.offsetMin = Vector2.zero;
        rtCheck.offsetMax = Vector2.zero;

        Image imgCheck = checkObj.AddComponent<Image>();
        imgCheck.color = colorRojo;
        toggle.graphic = imgCheck;
        toggle.targetGraphic = imgToggleBg;

        return toggle;
    }

    Dropdown CrearOpcionDropdown(string etiqueta, Transform padre, float posY)
    {
        // Container
        GameObject container = new GameObject("Opcion_" + etiqueta);
        container.transform.SetParent(padre, false);
        RectTransform rtCont = container.AddComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.1f, posY - 0.04f);
        rtCont.anchorMax = new Vector2(0.9f, posY + 0.04f);
        rtCont.offsetMin = Vector2.zero;
        rtCont.offsetMax = Vector2.zero;

        // Etiqueta
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        RectTransform rtLabel = labelObj.AddComponent<RectTransform>();
        rtLabel.anchorMin = new Vector2(0, 0);
        rtLabel.anchorMax = new Vector2(0.35f, 1);
        rtLabel.offsetMin = Vector2.zero;
        rtLabel.offsetMax = Vector2.zero;

        Text txtLabel = labelObj.AddComponent<Text>();
        txtLabel.text = etiqueta;
        txtLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtLabel.fontSize = 22;
        txtLabel.color = colorTexto;
        txtLabel.alignment = TextAnchor.MiddleLeft;

        // Dropdown
        GameObject dropObj = new GameObject("Dropdown");
        dropObj.transform.SetParent(container.transform, false);
        RectTransform rtDrop = dropObj.AddComponent<RectTransform>();
        rtDrop.anchorMin = new Vector2(0.38f, 0.1f);
        rtDrop.anchorMax = new Vector2(0.82f, 0.9f);
        rtDrop.offsetMin = Vector2.zero;
        rtDrop.offsetMax = Vector2.zero;

        Image imgDrop = dropObj.AddComponent<Image>();
        imgDrop.color = new Color(0.15f, 0.1f, 0.1f, 1f);

        Dropdown dropdown = dropObj.AddComponent<Dropdown>();
        dropdown.targetGraphic = imgDrop;

        // Caption Text (texto que muestra la opción seleccionada)
        GameObject captionObj = new GameObject("CaptionText");
        captionObj.transform.SetParent(dropObj.transform, false);
        RectTransform rtCaption = captionObj.AddComponent<RectTransform>();
        rtCaption.anchorMin = new Vector2(0.05f, 0);
        rtCaption.anchorMax = new Vector2(0.85f, 1);
        rtCaption.offsetMin = Vector2.zero;
        rtCaption.offsetMax = Vector2.zero;

        Text txtCaption = captionObj.AddComponent<Text>();
        txtCaption.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtCaption.fontSize = 18;
        txtCaption.color = colorTexto;
        txtCaption.alignment = TextAnchor.MiddleLeft;
        dropdown.captionText = txtCaption;

        // Flecha
        GameObject arrowObj = new GameObject("Arrow");
        arrowObj.transform.SetParent(dropObj.transform, false);
        RectTransform rtArrow = arrowObj.AddComponent<RectTransform>();
        rtArrow.anchorMin = new Vector2(0.88f, 0.2f);
        rtArrow.anchorMax = new Vector2(0.96f, 0.8f);
        rtArrow.offsetMin = Vector2.zero;
        rtArrow.offsetMax = Vector2.zero;
        Image imgArrow = arrowObj.AddComponent<Image>();
        imgArrow.color = colorRojo;

        // Template (desplegable)
        GameObject templateObj = new GameObject("Template");
        templateObj.transform.SetParent(dropObj.transform, false);
        RectTransform rtTemplate = templateObj.AddComponent<RectTransform>();
        rtTemplate.anchorMin = new Vector2(0, 0);
        rtTemplate.anchorMax = new Vector2(1, 0);
        rtTemplate.pivot = new Vector2(0.5f, 1f);
        rtTemplate.sizeDelta = new Vector2(0, 200);

        Image imgTemplate = templateObj.AddComponent<Image>();
        imgTemplate.color = new Color(0.1f, 0.06f, 0.06f, 0.98f);

        ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(templateObj.transform, false);
        RectTransform rtViewport = viewportObj.AddComponent<RectTransform>();
        rtViewport.anchorMin = Vector2.zero;
        rtViewport.anchorMax = Vector2.one;
        rtViewport.offsetMin = Vector2.zero;
        rtViewport.offsetMax = Vector2.zero;
        viewportObj.AddComponent<Image>().color = Color.white;
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        scrollRect.viewport = rtViewport;

        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform rtContent = contentObj.AddComponent<RectTransform>();
        rtContent.anchorMin = new Vector2(0, 1);
        rtContent.anchorMax = new Vector2(1, 1);
        rtContent.pivot = new Vector2(0.5f, 1);
        rtContent.sizeDelta = new Vector2(0, 28);
        scrollRect.content = rtContent;

        // Item template
        GameObject itemObj = new GameObject("Item");
        itemObj.transform.SetParent(contentObj.transform, false);
        RectTransform rtItem = itemObj.AddComponent<RectTransform>();
        rtItem.anchorMin = new Vector2(0, 0.5f);
        rtItem.anchorMax = new Vector2(1, 0.5f);
        rtItem.sizeDelta = new Vector2(0, 28);

        Toggle itemToggle = itemObj.AddComponent<Toggle>();

        // Item background
        Image imgItem = itemObj.AddComponent<Image>();
        imgItem.color = new Color(0.12f, 0.08f, 0.08f, 1f);
        itemToggle.targetGraphic = imgItem;

        // Item checkmark (highlight)
        GameObject itemCheckObj = new GameObject("Item Checkmark");
        itemCheckObj.transform.SetParent(itemObj.transform, false);
        RectTransform rtItemCheck = itemCheckObj.AddComponent<RectTransform>();
        rtItemCheck.anchorMin = Vector2.zero;
        rtItemCheck.anchorMax = Vector2.one;
        rtItemCheck.offsetMin = Vector2.zero;
        rtItemCheck.offsetMax = Vector2.zero;
        Image imgItemCheck = itemCheckObj.AddComponent<Image>();
        imgItemCheck.color = new Color(0.4f, 0.05f, 0.05f, 0.5f);
        itemToggle.graphic = imgItemCheck;

        // Item label
        GameObject itemLabelObj = new GameObject("Item Label");
        itemLabelObj.transform.SetParent(itemObj.transform, false);
        RectTransform rtItemLabel = itemLabelObj.AddComponent<RectTransform>();
        rtItemLabel.anchorMin = new Vector2(0.05f, 0);
        rtItemLabel.anchorMax = Vector2.one;
        rtItemLabel.offsetMin = Vector2.zero;
        rtItemLabel.offsetMax = Vector2.zero;

        Text txtItemLabel = itemLabelObj.AddComponent<Text>();
        txtItemLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txtItemLabel.fontSize = 18;
        txtItemLabel.color = colorTexto;
        txtItemLabel.alignment = TextAnchor.MiddleLeft;

        dropdown.itemText = txtItemLabel;
        dropdown.template = rtTemplate;
        templateObj.SetActive(false);

        return dropdown;
    }

    void CrearBotonVolver(Transform padre)
    {
        GameObject botonObj = new GameObject("BotonVolver");
        botonObj.transform.SetParent(padre, false);
        RectTransform rt = botonObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.06f);
        rt.anchorMax = new Vector2(0.5f, 0.06f);
        rt.sizeDelta = new Vector2(250, 50);
        rt.anchoredPosition = Vector2.zero;

        Image imgBoton = botonObj.AddComponent<Image>();
        imgBoton.color = colorBoton;

        Outline outlineBtn = botonObj.AddComponent<Outline>();
        outlineBtn.effectColor = colorRojoOscuro;
        outlineBtn.effectDistance = new Vector2(1, -1);

        Button boton = botonObj.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = Color.white;
        colores.highlightedColor = new Color(1.8f, 0.6f, 0.6f, 1f);
        colores.pressedColor = new Color(2.5f, 0.3f, 0.3f, 1f);
        colores.fadeDuration = 0.15f;
        boton.colors = colores;
        boton.targetGraphic = imgBoton;
        boton.onClick.AddListener(() => {
            PlayerPrefs.Save();
            menuPrincipal.OcultarOpciones();
        });

        // Texto
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        RectTransform rtTxt = textoObj.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        Text txt = textoObj.AddComponent<Text>();
        txt.text = "VOLVER";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.fontStyle = FontStyle.Bold;
        txt.color = colorTexto;
        txt.alignment = TextAnchor.MiddleCenter;
    }
}
