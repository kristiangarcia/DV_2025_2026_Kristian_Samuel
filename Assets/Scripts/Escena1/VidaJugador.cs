using UnityEngine;
using UnityEngine.UI; // Necesario para la UI
using UnityEngine.SceneManagement; // Necesario para reiniciar

public class VidaJugador : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    public float velocidadRegeneracion = 15f; // Cuánta vida recuperas por segundo
    public float tiempoParaEmpezarACurarse = 3f; // Segundos sin recibir daño para curarte

    [Header("Efectos Visuales")]
    public Image pantallaSangre; // Arrastra aquí tu imagen roja "PantallaSangre"
    public Slider barraDeVida;   // (Opcional) Si quieres mantener la barra también

    private float ultimoGolpe; // Para saber cuándo fue el último daño

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        // 1. REGENERACIÓN DE VIDA
        // Si ha pasado X tiempo desde el último golpe y no tenemos la vida a tope...
        if (Time.time > ultimoGolpe + tiempoParaEmpezarACurarse && vidaActual < vidaMaxima)
        {
            vidaActual += velocidadRegeneracion * Time.deltaTime;
            
            // Asegurarnos de no pasar de 100
            if (vidaActual > vidaMaxima) vidaActual = vidaMaxima;
        }

        // 2. EFECTO DE PANTALLA ROJA
        ActualizarEfectosVisuales();
    }

    public void RecibirDaño(float cantidad)
    {
        vidaActual -= cantidad;
        ultimoGolpe = Time.time; // Guardamos el momento exacto del golpe

        // Comprobar muerte
        if (vidaActual <= 0)
        {
            // Reiniciar nivel
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void ActualizarEfectosVisuales()
    {
        // --- Lógica de la Pantalla Roja ---
        if (pantallaSangre != null)
        {
            // Calculamos qué tan transparente debe ser.
            // Si vida es 100, alfa es 0 (invisible). Si vida es 0, alfa es 1 (rojo total).
            float opacidad = 1.0f - (vidaActual / vidaMaxima);
            
            // Limitamos para que no sea totalmente opaco (máximo 0.8 de rojo)
            // para que puedas seguir viendo el juego aunque estés muriendo.
            opacidad = Mathf.Clamp(opacidad, 0f, 0.8f);

            // Aplicamos el color
            Color colorSangre = pantallaSangre.color;
            colorSangre.a = opacidad;
            pantallaSangre.color = colorSangre;
        }

        // --- Lógica de la Barra (Opcional) ---
        if (barraDeVida != null)
        {
            barraDeVida.value = vidaActual / vidaMaxima;
        }
    }
}