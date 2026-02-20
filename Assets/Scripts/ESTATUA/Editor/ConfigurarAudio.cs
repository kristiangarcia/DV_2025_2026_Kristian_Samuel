using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/** DAV - 2ºDAM
 * Menú: DAV → Configurar Audio
 * Configura automáticamente todo el audio del proyecto:
 *   - GestorMusicaMenu en la escena MenuPrincipal (7 pistas aleatorias)
 *   - GestorMusicaJuego en la escena Main (ZOMBIES GIALLORE en loop)
 *   - Sonidos de muerte y golpe en los prefabs ZombieNormal y ZombieKamikaze
 */
public static class ConfigurarAudio
{
    // ── Rutas de audio ───────────────────────────────────────────────────────
    const string BASE_MUSICA = "Assets/Music_Sounds/Apocalypse Survival Horror Music Pack/";
    const string BASE_ZSF    = "Assets/Music_Sounds/Gameplay/zombie_sounds/";

    static readonly string[] PISTAS_MENU = new[]
    {
        BASE_MUSICA + "01 When It Began.mp3",
        BASE_MUSICA + "02 Torn.mp3",
        BASE_MUSICA + "03 Survive.mp3",
        BASE_MUSICA + "04 Hide.mp3",
        BASE_MUSICA + "05 After It's All Gone.mp3",
        BASE_MUSICA + "06 Tranquility.mp3",
        BASE_MUSICA + "07 Parting Ways.mp3",
    };

    const string GAMEPLAY_MUSIC = "Assets/Music_Sounds/Gameplay/ZOMBIES GIALLORE.wav";

    static readonly string[] SONIDOS_MUERTE = new[]
    {
        BASE_ZSF + "Zombie_dead_1.wav",
        BASE_ZSF + "Zombie_dead_2.wav",
    };

    static readonly string[] SONIDOS_GOLPE = new[]
    {
        BASE_ZSF + "Zombie_hit_1.wav",
        BASE_ZSF + "Zombie_hit_2.wav",
        BASE_ZSF + "Zombie_hit_3.wav",
    };

    // ── Rutas de escenas y prefabs ───────────────────────────────────────────
    const string ESCENA_MENU = "Assets/Scenes/MenuPrincipal.unity";
    const string ESCENA_MAIN = "Assets/Scenes/Main.unity";
    const string PREFAB_NORMAL   = "Assets/Prefabs-Zombie2/Zombi.prefab";
    const string PREFAB_KAMIKAZE = "Assets/Prefabs-Zombie2/Zombie regular 1.prefab";

    // ════════════════════════════════════════════════════════════════════════
    [MenuItem("DAV/Configurar Audio")]
    static void Ejecutar()
    {
        // Guardar la escena actual para volver a ella al final
        string escenaOriginal = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int errores = 0;

        // ── 1. Prefabs de zombies (no requieren abrir escena) ────────────────
        if (!ConfigurarPrefabZombie(PREFAB_NORMAL,   "ZombieNormal"))   errores++;
        if (!ConfigurarPrefabZombie(PREFAB_KAMIKAZE, "ZombieKamikaze")) errores++;

        // ── 2. Escena MenuPrincipal ──────────────────────────────────────────
        if (!ConfigurarEscenaMenu()) errores++;

        // ── 3. Escena Main ───────────────────────────────────────────────────
        if (!ConfigurarEscenaMain()) errores++;

        // ── Volver a la escena original ─────────────────────────────────────
        if (!string.IsNullOrEmpty(escenaOriginal))
            EditorSceneManager.OpenScene(escenaOriginal);

        if (errores == 0)
            EditorUtility.DisplayDialog("Audio configurado",
                "✓ Todo el audio ha sido configurado correctamente.", "OK");
        else
            EditorUtility.DisplayDialog("Audio configurado con advertencias",
                $"Se completó con {errores} error(es). Revisa la consola.", "OK");
    }

    // ════════════════════════════════════════════════════════════════════════
    // PREFABS DE ZOMBIES
    // ════════════════════════════════════════════════════════════════════════
    static bool ConfigurarPrefabZombie(string rutaPrefab, string tipoScript)
    {
        var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(rutaPrefab);
        if (prefabGO == null)
        {
            Debug.LogWarning($"[Audio] No encontrado prefab: {rutaPrefab}");
            return false;
        }

        // Buscar el componente por nombre de tipo (evita referencia directa de Assembly)
        Component comp = null;
        foreach (var c in prefabGO.GetComponentsInChildren<Component>(true))
        {
            if (c != null && c.GetType().Name == tipoScript) { comp = c; break; }
        }

        if (comp == null)
        {
            Debug.LogWarning($"[Audio] Componente {tipoScript} no encontrado en {rutaPrefab}");
            return false;
        }

        var so = new SerializedObject(comp);

        AsignarArrayDeClips(so, "sonidosMuerte", SONIDOS_MUERTE);
        AsignarArrayDeClips(so, "sonidosGolpe",  SONIDOS_GOLPE);

        so.ApplyModifiedProperties();
        PrefabUtility.SavePrefabAsset(prefabGO);
        Debug.Log($"[Audio] ✓ {tipoScript} configurado en {rutaPrefab}");
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    // ESCENA MENÚ PRINCIPAL
    // ════════════════════════════════════════════════════════════════════════
    static bool ConfigurarEscenaMenu()
    {
        var escena = EditorSceneManager.OpenScene(ESCENA_MENU, OpenSceneMode.Single);
        if (!escena.IsValid()) { Debug.LogWarning("[Audio] No se pudo abrir " + ESCENA_MENU); return false; }

        // Eliminar GestorMusicaMenu previo si existe
        foreach (var go in escena.GetRootGameObjects())
        {
            if (go.GetComponent<GestorMusicaMenu>() != null)
                Object.DestroyImmediate(go);
        }

        // Crear nuevo GameObject con el componente
        var gestorGO = new GameObject("GestorMusicaMenu");
        var gestor   = gestorGO.AddComponent<GestorMusicaMenu>();

        // Asignar pistas via SerializedObject
        var so   = new SerializedObject(gestor);
        var prop = so.FindProperty("pistas");
        prop.arraySize = PISTAS_MENU.Length;
        for (int i = 0; i < PISTAS_MENU.Length; i++)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(PISTAS_MENU[i]);
            if (clip == null) Debug.LogWarning($"[Audio] No encontrado: {PISTAS_MENU[i]}");
            prop.GetArrayElementAtIndex(i).objectReferenceValue = clip;
        }
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("[Audio] ✓ GestorMusicaMenu configurado en MenuPrincipal");
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    // ESCENA MAIN (GAMEPLAY)
    // ════════════════════════════════════════════════════════════════════════
    static bool ConfigurarEscenaMain()
    {
        var escena = EditorSceneManager.OpenScene(ESCENA_MAIN, OpenSceneMode.Single);
        if (!escena.IsValid()) { Debug.LogWarning("[Audio] No se pudo abrir " + ESCENA_MAIN); return false; }

        // Eliminar GestorMusicaJuego previo si existe
        foreach (var go in escena.GetRootGameObjects())
        {
            if (go.GetComponent<GestorMusicaJuego>() != null)
                Object.DestroyImmediate(go);
        }

        // Crear nuevo GameObject con el componente
        var gestorGO = new GameObject("GestorMusicaJuego");
        var gestor   = gestorGO.AddComponent<GestorMusicaJuego>();

        // Asignar clip via SerializedObject
        var so   = new SerializedObject(gestor);
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(GAMEPLAY_MUSIC);
        if (clip == null) Debug.LogWarning("[Audio] No encontrado: " + GAMEPLAY_MUSIC);
        so.FindProperty("musicaGameplay").objectReferenceValue = clip;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("[Audio] ✓ GestorMusicaJuego configurado en Main");
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════
    // HELPER
    // ════════════════════════════════════════════════════════════════════════
    static void AsignarArrayDeClips(SerializedObject so, string campo, string[] rutas)
    {
        var prop = so.FindProperty(campo);
        if (prop == null) { Debug.LogWarning($"[Audio] Campo '{campo}' no encontrado."); return; }

        prop.arraySize = rutas.Length;
        for (int i = 0; i < rutas.Length; i++)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(rutas[i]);
            if (clip == null) Debug.LogWarning($"[Audio] No encontrado: {rutas[i]}");
            prop.GetArrayElementAtIndex(i).objectReferenceValue = clip;
        }
    }
}
