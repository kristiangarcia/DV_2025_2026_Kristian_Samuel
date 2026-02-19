using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.Barracuda;

/** DAV - 2ºDAM
 * EDITOR SCRIPT: CrearEscenaEstatua
 *
 * Genera la escena de entrenamiento "6_TRN_Estatua.unity" de forma programática.
 * Construye un grid 3×3 de áreas de entrenamiento, cada una con:
 *   - Suelo (Plane 24×24u)
 *   - TargetPlayer con Visual (Capsule) + CamaraPoint + CamaraVisual (Sphere)
 *   - Estatua_Agent con Visual (Cube) + BoxCollider + BehaviorParameters
 *     + DecisionRequester + StatueAgent (todas las referencias cableadas)
 *
 * Uso: menú  DAV ▶ Crear Escena 6 – Estatua Acechadora
 */
public static class CrearEscenaEstatua
{
    // ─── CONSTANTES ──────────────────────────────────────────────────────────────
    const string SCENE_PATH = "Assets/Scenes/6_TRN_Estatua.unity";
    const string MAT_PARENT = "Assets/Scripts/ESTATUA";
    const string MAT_CHILD  = "Materials";
    const string MAT_FOLDER = MAT_PARENT + "/" + MAT_CHILD;

    const int   GRID = 3;    // 3×3 = 9 instancias
    const float SEP  = 28f;  // Separación en unidades entre áreas

    // ════════════════════════════════════════════════════════════════════════════
    [MenuItem("DAV/Crear Escena 6 \u2013 Estatua Acechadora")]
    public static void Ejecutar()
    {
        // Ofrece guardar la escena actual antes de reemplazarla
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // ── Materiales compartidos URP ────────────────────────────────────────
        if (!AssetDatabase.IsValidFolder(MAT_FOLDER))
            AssetDatabase.CreateFolder(MAT_PARENT, MAT_CHILD);

        Material matSuelo   = ObtenerMat("MatSuelo",   new Color(0.22f, 0.22f, 0.22f)); // gris oscuro
        Material matJugador = ObtenerMat("MatJugador",  new Color(0.20f, 0.60f, 1.00f)); // azul
        Material matEstatua = ObtenerMat("MatEstatua",  new Color(0.55f, 0.55f, 0.55f)); // gris piedra
        Material matCamara  = ObtenerMat("MatCamara",   new Color(1.00f, 0.85f, 0.00f)); // amarillo

        // ── Nueva escena vacía ────────────────────────────────────────────────
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Luz Direccional ───────────────────────────────────────────────────
        var luzGO = new GameObject("Directional Light");
        var luz   = luzGO.AddComponent<Light>();
        luz.type      = LightType.Directional;
        luz.intensity = 1f;
        luzGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── Grid 3×3 ─────────────────────────────────────────────────────────
        for (int f = 0; f < GRID; f++)
            for (int c = 0; c < GRID; c++)
            {
                int idx = f * GRID + c;
                CrearArea(
                    idx,
                    new Vector3((c - 1) * SEP, 0f, (f - 1) * SEP),
                    matSuelo, matJugador, matEstatua, matCamara,
                    principal: idx == 0);   // solo la 0 muestra gizmos/logs
            }

        // ── Guardar escena ────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(escena, SCENE_PATH);
        AssetDatabase.Refresh();
        Debug.Log($"<color=lime><b>✓ Escena creada en {SCENE_PATH}</b></color>");
    }

    // ════════════════════════════════════════════════════════════════════════════
    // CONSTRUCCIÓN DE UN ÁREA DE ENTRENAMIENTO
    // ════════════════════════════════════════════════════════════════════════════
    static void CrearArea(int idx, Vector3 pos,
        Material matSuelo, Material matJugador, Material matEstatua, Material matCamara,
        bool principal)
    {
        // ── Raíz del área ─────────────────────────────────────────────────────
        var raiz = new GameObject($"TrainingArea_{idx:D2}");
        raiz.transform.position = pos;

        // ── Suelo ─────────────────────────────────────────────────────────────
        // Unity Plane = 10×10u por defecto → escala 2.4 → 24×24u
        var suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Suelo";
        suelo.transform.SetParent(raiz.transform, false);
        suelo.transform.localScale = new Vector3(2.4f, 1f, 2.4f);
        suelo.GetComponent<Renderer>().sharedMaterial = matSuelo;

        // ── TargetPlayer ──────────────────────────────────────────────────────
        var tpGO = new GameObject("TargetPlayer");
        tpGO.transform.SetParent(raiz.transform, false);
        tpGO.transform.localPosition = new Vector3(3f, 0f, 0f);

        // Visual del jugador: cápsula azul
        var visJug = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visJug.name = "Visual";
        visJug.transform.SetParent(tpGO.transform, false);
        visJug.transform.localPosition = new Vector3(0f, 1f, 0f);
        visJug.GetComponent<Renderer>().sharedMaterial = matJugador;
        Object.DestroyImmediate(visJug.GetComponent<Collider>());

        // CamaraPoint: Transform vacío a la altura de los ojos
        var camPt = new GameObject("CamaraPoint");
        camPt.transform.SetParent(tpGO.transform, false);
        camPt.transform.localPosition = new Vector3(0f, 1.7f, 0f);

        // CamaraVisual: esfera pequeña amarilla (ayuda visual, sin colisión)
        var camVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        camVis.name = "CamaraVisual";
        camVis.transform.SetParent(camPt.transform, false);
        camVis.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        camVis.GetComponent<Renderer>().sharedMaterial = matCamara;
        Object.DestroyImmediate(camVis.GetComponent<Collider>());

        // Componente TargetPlayer — apunta al CamaraPoint
        var tpComp   = tpGO.AddComponent<TargetPlayer>();
        tpComp.camara = camPt.transform;

        // ── Estatua_Agent ─────────────────────────────────────────────────────
        var estGO = new GameObject("Estatua_Agent");
        estGO.transform.SetParent(raiz.transform, false);
        estGO.transform.localPosition = new Vector3(-3f, 0f, 0f);

        // Visual de la estatua: cubo con proporciones de personaje
        var visEst = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visEst.name = "Visual";
        visEst.transform.SetParent(estGO.transform, false);
        visEst.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        visEst.transform.localScale    = new Vector3(0.8f, 1.8f, 0.8f);
        visEst.GetComponent<Renderer>().sharedMaterial = matEstatua;
        Object.DestroyImmediate(visEst.GetComponent<Collider>());

        // BoxCollider en el raíz del agente (coincide con el visual)
        var bc    = estGO.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0.9f, 0f);
        bc.size   = new Vector3(0.8f, 1.8f, 0.8f);

        // ── StatueAgent PRIMERO para que DecisionRequester no añada un Agent base extra
        // (DecisionRequester tiene [RequireComponent(typeof(Agent))]; si StatueAgent ya
        //  existe, Unity lo reconoce como Agent válido y no duplica el componente)
        var agente = estGO.AddComponent<StatueAgent>();
        agente.jugador          = tpGO.transform;
        agente.camaraJugador    = camPt.transform;
        agente.targetPlayerCtrl = tpComp;
        agente.MaxStep          = 5000;
        agente.mostrarGizmos    = principal;
        agente.activarLogs      = principal;
        EditorUtility.SetDirty(agente);

        // ── BehaviorParameters (ML-Agents 2.0.2) ──────────────────────────────
        // StatueAgent hereda de Agent, que tiene [RequireComponent(typeof(BehaviorParameters))].
        // Unity ya añadió uno automáticamente al hacer AddComponent<StatueAgent>().
        // Usamos GetComponent para configurar ese mismo y no crear un duplicado.
        // Space Size = 7: posición relativa (3) + siendoVista (1) + forward cámara (3)
        // Continuous Actions = 2: moveX, moveZ
        var bp = estGO.GetComponent<BehaviorParameters>();
        bp.BehaviorName = "StatueAgent";
        bp.BrainParameters.VectorObservationSize        = 12;
        bp.BrainParameters.NumStackedVectorObservations = 1;
        bp.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(2);
        EditorUtility.SetDirty(bp);

        // ── DecisionRequester ─────────────────────────────────────────────────
        // Period = 5: el agente decide cada 5 pasos de física (~10 veces por segundo a 50fps)
        // Se añade DESPUÉS de StatueAgent para que RequireComponent encuentre el agente existente
        var dr = estGO.AddComponent<DecisionRequester>();
        dr.DecisionPeriod = 5;
        EditorUtility.SetDirty(dr);
    }

    // ════════════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Carga el material URP desde el disco si existe; si no, lo crea y lo guarda como asset.
    /// Shader: "Universal Render Pipeline/Lit" — Color property: _BaseColor
    /// </summary>
    static Material ObtenerMat(string nombre, Color color)
    {
        string ruta = $"{MAT_FOLDER}/{nombre}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat != null) return mat;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, ruta);
        return mat;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // ESCENA DE EJECUCIÓN (EXEC)
    // ════════════════════════════════════════════════════════════════════════════

    const string EXEC_SCENE_PATH  = "Assets/Scenes/6_EXEC_Estatua.unity";
    const string MODEL_PATH       = "Assets/ML-Agents/Models/StatueAgent.onnx";
    const string PLAYER_PREFAB    = "Assets/Infima Games/Low Poly Shooter Pack - Free Sample/Prefabs/a/Personaje.prefab";

    [MenuItem("DAV/Crear Escena 6 EXEC \u2013 Estatua Acechadora")]
    public static void EjecutarEXEC()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // Verificar que el modelo existe antes de empezar
        var modelo = AssetDatabase.LoadAssetAtPath<NNModel>(MODEL_PATH);
        if (modelo == null)
        {
            Debug.LogError($"Modelo no encontrado en {MODEL_PATH}. " +
                           "Cópialo primero y espera a que Unity lo importe.");
            return;
        }

        // Materiales compartidos (mismos que TRN)
        Material matSuelo   = ObtenerMat("MatSuelo",   new Color(0.22f, 0.22f, 0.22f));
        Material matJugador = ObtenerMat("MatJugador",  new Color(0.20f, 0.60f, 1.00f));
        Material matEstatua = ObtenerMat("MatEstatua",  new Color(0.55f, 0.55f, 0.55f));

        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Luz
        var luzGO = new GameObject("Directional Light");
        var luz   = luzGO.AddComponent<Light>();
        luz.type      = LightType.Directional;
        luz.intensity = 1f;
        luzGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Única área de juego
        CrearAreaEXEC(matSuelo, matJugador, matEstatua, modelo);

        EditorSceneManager.SaveScene(escena, EXEC_SCENE_PATH);
        AssetDatabase.Refresh();
        Debug.Log($"<color=lime><b>✓ Escena EXEC creada en {EXEC_SCENE_PATH}</b></color>");
    }

    // ────────────────────────────────────────────────────────────────────────────
    static void CrearAreaEXEC(Material matSuelo, Material matJugador,
                               Material matEstatua, NNModel modelo)
    {
        var raiz = new GameObject("TrainingArea_00");

        // ── Suelo ─────────────────────────────────────────────────────────────
        var suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Suelo";
        suelo.transform.SetParent(raiz.transform, false);
        suelo.transform.localScale = new Vector3(2.4f, 1f, 2.4f);
        suelo.GetComponent<Renderer>().sharedMaterial = matSuelo;

        // ── Jugador: prefab completo del shooter pack ─────────────────────────
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB);
        if (playerPrefab == null)
        {
            Debug.LogError($"Prefab no encontrado en {PLAYER_PREFAB}");
            return;
        }
        var jugGO = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        jugGO.name = "Jugador";
        jugGO.transform.SetParent(raiz.transform, false);
        jugGO.transform.localPosition = new Vector3(3f, 0f, 0f);

        // La cámara principal está dentro del esqueleto: head → SOCKET_Camera → Camera
        // Buscamos la Camera con tag MainCamera dentro del prefab
        Camera camComp = null;
        foreach (var c in jugGO.GetComponentsInChildren<Camera>(true))
        {
            if (c.CompareTag("MainCamera")) { camComp = c; break; }
        }
        // Fallback: primera cámara que encuentre
        if (camComp == null) camComp = jugGO.GetComponentInChildren<Camera>(true);

        Transform camaraJugadorTransform = camComp != null ? camComp.transform : jugGO.transform;

        // ── Estatua_Agent ─────────────────────────────────────────────────────
        var estGO = new GameObject("Estatua_Agent");
        estGO.transform.SetParent(raiz.transform, false);
        estGO.transform.localPosition = new Vector3(-3f, 0f, 0f);

        var visEst = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visEst.name = "Visual";
        visEst.transform.SetParent(estGO.transform, false);
        visEst.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        visEst.transform.localScale    = new Vector3(0.8f, 1.8f, 0.8f);
        visEst.GetComponent<Renderer>().sharedMaterial = matEstatua;
        Object.DestroyImmediate(visEst.GetComponent<Collider>());

        var bc    = estGO.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0.9f, 0f);
        bc.size   = new Vector3(0.8f, 1.8f, 0.8f);

        // StatueAgent PRIMERO
        var agente = estGO.AddComponent<StatueAgent>();
        agente.jugador          = jugGO.transform;
        agente.camaraJugador    = camaraJugadorTransform;
        agente.targetPlayerCtrl = null;
        agente.MaxStep          = 0;
        agente.mostrarGizmos    = true;
        agente.activarLogs      = true;
        EditorUtility.SetDirty(agente);

        // BehaviorParameters: usar el auto-creado por Agent y asignar modelo
        var bp = estGO.GetComponent<BehaviorParameters>();
        bp.BehaviorName = "StatueAgent";
        bp.BrainParameters.VectorObservationSize        = 12;
        bp.BrainParameters.NumStackedVectorObservations = 1;
        bp.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(2);
        bp.Model        = modelo;
        bp.BehaviorType = BehaviorType.InferenceOnly;
        EditorUtility.SetDirty(bp);

        // DecisionRequester DESPUÉS de StatueAgent
        var dr = estGO.AddComponent<DecisionRequester>();
        dr.DecisionPeriod = 5;
        EditorUtility.SetDirty(dr);

        // ── Gestor de partida: overlay "¡TE ATRAPÓ!" ──────────────────────────
        var gestorGO = new GameObject("GestorPartida");
        gestorGO.transform.SetParent(raiz.transform, false);
        var gestor = gestorGO.AddComponent<GestorPartidaEstatua>();
        gestor.jugador = jugGO.transform;
        gestor.estatua = estGO.transform;
        EditorUtility.SetDirty(gestor);
    }
}
