#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

// Editor Script: Crea automáticamente el Animator Controller para los zombies
// Ejecutar desde el menú: DeadWave > Crear Animator Zombies

public class CrearAnimatorZombies : MonoBehaviour
{
    [MenuItem("DeadWave/Crear Animator Zombies")]
    static void Crear()
    {
        string carpeta = "Assets/Not So Scary Zombie Pack";
        string rutaController = carpeta + "/ZombieAnimator.controller";

        // Borrar el anterior si existe
        if (File.Exists(rutaController))
            AssetDatabase.DeleteAsset(rutaController);

        // Crear el Animator Controller
        var controller = AnimatorController.CreateAnimatorControllerAtPath(rutaController);

        // Solo UN parámetro: Velocidad
        controller.AddParameter("Velocidad", AnimatorControllerParameterType.Float);

        var rootStateMachine = controller.layers[0].stateMachine;

        // ==== Buscar clips de animación ====
        AnimationClip clipIdle = BuscarClip(carpeta, "zombie idle");
        AnimationClip clipWalk = BuscarClip(carpeta, "walking");
        AnimationClip clipRun = BuscarClip(carpeta, "zombie running");
        AnimationClip clipDeath = BuscarClip(carpeta, "zombie agonizing");
        AnimationClip clipHit = BuscarClip(carpeta, "zombie reaction hit");

        Debug.Log("[AnimatorZombie] Idle: " + (clipIdle != null ? clipIdle.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Walk: " + (clipWalk != null ? clipWalk.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Run: " + (clipRun != null ? clipRun.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Death: " + (clipDeath != null ? clipDeath.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Hit: " + (clipHit != null ? clipHit.name : "NO ENCONTRADO"));

        // ==== 3 estados simples: Idle, Walking, Running ====
        var stateIdle = rootStateMachine.AddState("Idle", new Vector3(300, 0, 0));
        if (clipIdle != null) stateIdle.motion = clipIdle;
        rootStateMachine.defaultState = stateIdle;

        var stateWalk = rootStateMachine.AddState("Walking", new Vector3(300, 100, 0));
        if (clipWalk != null) stateWalk.motion = clipWalk;

        var stateRun = rootStateMachine.AddState("Running", new Vector3(300, 200, 0));
        if (clipRun != null) stateRun.motion = clipRun;

        // Estados para CrossFade (sin transiciones automáticas, se llaman desde código)
        var stateDeath = rootStateMachine.AddState("Death", new Vector3(600, 0, 0));
        if (clipDeath != null) stateDeath.motion = clipDeath;

        var stateHit = rootStateMachine.AddState("HitReaction", new Vector3(600, 200, 0));
        if (clipHit != null) stateHit.motion = clipHit;

        // ==== Transiciones SOLO para movimiento (Velocidad) ====

        // Idle → Walk (Velocidad > 0.1)
        var idleToWalk = stateIdle.AddTransition(stateWalk);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Velocidad");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.15f;

        // Walk → Idle (Velocidad < 0.1)
        var walkToIdle = stateWalk.AddTransition(stateIdle);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Velocidad");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.15f;

        // Walk → Run (Velocidad > 3)
        var walkToRun = stateWalk.AddTransition(stateRun);
        walkToRun.AddCondition(AnimatorConditionMode.Greater, 3f, "Velocidad");
        walkToRun.hasExitTime = false;
        walkToRun.duration = 0.15f;

        // Run → Walk (Velocidad < 3)
        var runToWalk = stateRun.AddTransition(stateWalk);
        runToWalk.AddCondition(AnimatorConditionMode.Less, 3f, "Velocidad");
        runToWalk.hasExitTime = false;
        runToWalk.duration = 0.15f;

        // HitReaction → Walking (después de que acabe la animación)
        var hitToWalk = stateHit.AddTransition(stateWalk);
        hitToWalk.hasExitTime = true;
        hitToWalk.exitTime = 0.85f;
        hitToWalk.duration = 0.15f;
        hitToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Velocidad");

        var hitToIdle = stateHit.AddTransition(stateIdle);
        hitToIdle.hasExitTime = true;
        hitToIdle.exitTime = 0.85f;
        hitToIdle.duration = 0.15f;

        // NO HAY transiciones AnyState - todo se controla desde código con CrossFade

        // Guardar
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ [AnimatorZombie] Controller LIMPIO creado en: " + rutaController);
        EditorUtility.DisplayDialog("DeadWave", "Animator Controller LIMPIO creado\n(sin Attack, sin AnyState)\n\n" + rutaController, "OK");
    }

    static AnimationClip BuscarClip(string carpeta, string nombreArchivo)
    {
        string[] archivos = Directory.GetFiles(carpeta, "*.fbx");
        foreach (string archivo in archivos)
        {
            if (archivo.ToLower().Contains(nombreArchivo.ToLower()))
            {
                string assetPath = archivo.Replace("\\", "/");
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (Object obj in subAssets)
                {
                    if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    {
                        return clip;
                    }
                }
            }
        }
        return null;
    }
}
#endif
