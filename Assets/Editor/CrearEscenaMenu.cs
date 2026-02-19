#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Script de Editor para crear la escena del Menú Principal de DeadWave
// Uso: Menú Unity -> Tools -> DeadWave -> Crear Escena Menu

public class CrearEscenaMenu : Editor
{
    [MenuItem("Tools/DeadWave/Crear Escena Menu")]
    static void Crear()
    {
        // 1. Crear nueva escena
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Crear cámara principal con fondo negro
        GameObject camaraObj = new GameObject("Main Camera");
        Camera cam = camaraObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f, 1f);
        cam.orthographic = false;
        cam.tag = "MainCamera";
        camaraObj.AddComponent<AudioListener>();

        // 3. Crear luz ambiental tenue (roja para ambiencia horror)
        GameObject luzObj = new GameObject("Luz Ambiental");
        Light luz = luzObj.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(0.3f, 0.05f, 0.05f, 1f);
        luz.intensity = 0.3f;
        luzObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 4. Crear GameObject con el MenuPrincipal
        GameObject menuObj = new GameObject("MenuPrincipal");
        menuObj.AddComponent<MenuPrincipal>();

        // 5. Guardar la escena
        string rutaEscena = "Assets/Scenes/MenuPrincipal.unity";
        EditorSceneManager.SaveScene(escena, rutaEscena);
        
        // 6. Configurar Build Settings
        ConfigurarBuildSettings(rutaEscena);

        AssetDatabase.Refresh();
        Debug.Log("<color=red>[DeadWave]</color> Escena MenuPrincipal creada exitosamente en: " + rutaEscena);
        
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(rutaEscena);

        EditorUtility.DisplayDialog("DeadWave", 
            "¡Escena del Menú Principal creada!\n\n" +
            "Ruta: " + rutaEscena + "\n\n" +
            "Build Settings configurado:\n" +
            "  Escena 0: MenuPrincipal\n" +
            "  Escena 1: Main\n\n" +
            "Pulsa Play para probarla.", "OK");
    }

    [MenuItem("Tools/DeadWave/Configurar Build Settings")]
    static void ConfigurarBuildDesdeMenu()
    {
        ConfigurarBuildSettings("Assets/Scenes/MenuPrincipal.unity");
        EditorUtility.DisplayDialog("DeadWave", 
            "Build Settings actualizado:\n" +
            "  Escena 0: MenuPrincipal\n" +
            "  Escena 1: Main", "OK");
    }

    static void ConfigurarBuildSettings(string rutaEscena)
    {
        string rutaMain = "Assets/Scenes/Main.unity";
        
        var listaEscenas = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        // Escena 0: MenuPrincipal
        var sceneMenu = AssetDatabase.LoadAssetAtPath<SceneAsset>(rutaEscena);
        if (sceneMenu != null)
        {
            listaEscenas.Add(new EditorBuildSettingsScene(rutaEscena, true));
            Debug.Log("<color=red>[DeadWave]</color> Escena 0: " + rutaEscena + " ✓");
        }
        
        // Escena 1: Main (juego principal)
        var sceneMain = AssetDatabase.LoadAssetAtPath<SceneAsset>(rutaMain);
        if (sceneMain != null)
        {
            listaEscenas.Add(new EditorBuildSettingsScene(rutaMain, true));
            Debug.Log("<color=red>[DeadWave]</color> Escena 1: " + rutaMain + " ✓");
        }
        else
        {
            Debug.LogWarning("<color=red>[DeadWave]</color> No se encontró: " + rutaMain);
        }
        
        // Aplicar al EditorBuildSettings
        EditorBuildSettings.scenes = listaEscenas.ToArray();
        Debug.Log("<color=red>[DeadWave]</color> Build Settings actualizado con " + listaEscenas.Count + " escenas.");
    }
}
#endif
