using UnityEngine;

// Skybox apocalíptico para DeadWave
// Se aplica automáticamente al iniciar la escena
// Cielo oscuro con tonos rojos/naranjas de apocalipsis zombie

public class SkyboxApocaliptico : MonoBehaviour
{
    void Awake()
    {
        AplicarSkybox();
        ConfigurarIluminacion();
    }

    void AplicarSkybox()
    {
        // Crear material de skybox procedural
        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader == null)
        {
            Debug.LogWarning("[Skybox] Shader Skybox/Procedural no encontrado, usando color sólido");
            ConfigurarSkyboxSolido();
            return;
        }

        Material skyMat = new Material(skyShader);

        // Configurar el cielo procedural
        skyMat.SetFloat("_SunSize", 0.05f);         // Sol/luna
        skyMat.SetFloat("_SunSizeConvergence", 8f);  // Convergencia del sol
        skyMat.SetFloat("_AtmosphereThickness", 3.5f); // Atmósfera densa pero no tanto
        skyMat.SetFloat("_Exposure", 1.0f);            // Más brillo

        // Colores apocalípticos pero visibles
        skyMat.SetColor("_SkyTint", new Color(0.45f, 0.15f, 0.1f, 1f));      // Rojo menos oscuro
        skyMat.SetColor("_GroundColor", new Color(0.12f, 0.06f, 0.04f, 1f)); // Marrón oscuro

        RenderSettings.skybox = skyMat;

        // Forzar actualización del entorno
        DynamicGI.UpdateEnvironment();
    }

    void ConfigurarSkyboxSolido()
    {
        // Fallback: skybox de 6 caras con color sólido
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.02f, 0.02f, 1f);
        }
    }

    void ConfigurarIluminacion()
    {
        // Luz ambiental oscura pero visible
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.35f, 0.12f, 0.08f, 1f);      // Cielo: rojo cálido
        RenderSettings.ambientEquatorColor = new Color(0.25f, 0.1f, 0.06f, 1f);    // Horizonte: naranja
        RenderSettings.ambientGroundColor = new Color(0.1f, 0.05f, 0.03f, 1f);     // Suelo: oscuro

        // Niebla apocalíptica suave
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.15f, 0.06f, 0.04f, 1f);  // Niebla rojiza
        RenderSettings.fogDensity = 0.008f;                              // Más suave

        // Luz direccional (simular luna roja)
        Light[] luces = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light luz in luces)
        {
            if (luz.type == LightType.Directional)
            {
                luz.color = new Color(0.9f, 0.5f, 0.3f, 1f);  // Luz naranja cálida
                luz.intensity = 1.2f;                              // Más fuerte
                luz.transform.rotation = Quaternion.Euler(35f, -30f, 0f); // Ángulo bajo
                break;
            }
        }
    }
}
