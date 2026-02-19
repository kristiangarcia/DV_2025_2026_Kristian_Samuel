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

        // Crear el Animator Controller
        var controller = AnimatorController.CreateAnimatorControllerAtPath(rutaController);

        // Añadir parámetros
        controller.AddParameter("Velocidad", AnimatorControllerParameterType.Float);
        controller.AddParameter("Atacando", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Morir", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Golpeado", AnimatorControllerParameterType.Trigger);

        var rootStateMachine = controller.layers[0].stateMachine;

        // ==== Buscar clips de animación ====
        AnimationClip clipIdle = BuscarClip(carpeta, "zombie idle");
        AnimationClip clipWalk = BuscarClip(carpeta, "walking");
        AnimationClip clipRun = BuscarClip(carpeta, "zombie running");
        AnimationClip clipAttack = BuscarClip(carpeta, "zombie attack");
        AnimationClip clipDeath = BuscarClip(carpeta, "zombie agonizing");
        AnimationClip clipHit = BuscarClip(carpeta, "zombie reaction hit");

        // Logs
        Debug.Log("[AnimatorZombie] Idle: " + (clipIdle != null ? clipIdle.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Walk: " + (clipWalk != null ? clipWalk.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Run: " + (clipRun != null ? clipRun.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Attack: " + (clipAttack != null ? clipAttack.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Death: " + (clipDeath != null ? clipDeath.name : "NO ENCONTRADO"));
        Debug.Log("[AnimatorZombie] Hit: " + (clipHit != null ? clipHit.name : "NO ENCONTRADO"));

        // ==== Crear estados ====
        var stateIdle = rootStateMachine.AddState("Idle", new Vector3(300, 0, 0));
        if (clipIdle != null) stateIdle.motion = clipIdle;
        rootStateMachine.defaultState = stateIdle;

        var stateWalk = rootStateMachine.AddState("Walking", new Vector3(300, 100, 0));
        if (clipWalk != null) stateWalk.motion = clipWalk;

        var stateRun = rootStateMachine.AddState("Running", new Vector3(300, 200, 0));
        if (clipRun != null) stateRun.motion = clipRun;

        var stateAttack = rootStateMachine.AddState("Attack", new Vector3(600, 100, 0));
        if (clipAttack != null) stateAttack.motion = clipAttack;

        var stateDeath = rootStateMachine.AddState("Death", new Vector3(600, 0, 0));
        if (clipDeath != null) stateDeath.motion = clipDeath;

        var stateHit = rootStateMachine.AddState("HitReaction", new Vector3(600, 200, 0));
        if (clipHit != null) stateHit.motion = clipHit;

        // ==== Crear transiciones ====

        // Idle → Walk (Velocidad > 0.1)
        var idleToWalk = stateIdle.AddTransition(stateWalk);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Velocidad");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.2f;

        // Walk → Idle (Velocidad < 0.1)
        var walkToIdle = stateWalk.AddTransition(stateIdle);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Velocidad");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.2f;

        // Walk → Run (Velocidad > 3)
        var walkToRun = stateWalk.AddTransition(stateRun);
        walkToRun.AddCondition(AnimatorConditionMode.Greater, 3f, "Velocidad");
        walkToRun.hasExitTime = false;
        walkToRun.duration = 0.2f;

        // Run → Walk (Velocidad < 3)
        var runToWalk = stateRun.AddTransition(stateWalk);
        runToWalk.AddCondition(AnimatorConditionMode.Less, 3f, "Velocidad");
        runToWalk.hasExitTime = false;
        runToWalk.duration = 0.2f;

        // Any State → Attack (Atacando = true)
        var anyToAttack = rootStateMachine.AddAnyStateTransition(stateAttack);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Atacando");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0.15f;

        // Attack → Walk (Atacando = false)
        var attackToWalk = stateAttack.AddTransition(stateWalk);
        attackToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "Atacando");
        attackToWalk.hasExitTime = true;
        attackToWalk.exitTime = 0.9f;
        attackToWalk.duration = 0.2f;

        // Any State → Death (Morir trigger)
        var anyToDeath = rootStateMachine.AddAnyStateTransition(stateDeath);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Morir");
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0.1f;

        // Any State → HitReaction (Golpeado trigger)
        var anyToHit = rootStateMachine.AddAnyStateTransition(stateHit);
        anyToHit.AddCondition(AnimatorConditionMode.If, 0, "Golpeado");
        anyToHit.hasExitTime = false;
        anyToHit.duration = 0.1f;

        // HitReaction → Walk (after exit time)
        var hitToWalk = stateHit.AddTransition(stateWalk);
        hitToWalk.hasExitTime = true;
        hitToWalk.exitTime = 0.8f;
        hitToWalk.duration = 0.2f;

        // Guardar
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ [AnimatorZombie] Controller creado en: " + rutaController);
        EditorUtility.DisplayDialog("DeadWave", "Animator Controller de zombies creado en:\n" + rutaController, "OK");
    }

    static AnimationClip BuscarClip(string carpeta, string nombreArchivo)
    {
        // Buscar FBX que contenga el nombre
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { carpeta });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object obj in subAssets)
            {
                if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    if (path.ToLower().Contains(nombreArchivo.ToLower()))
                    {
                        return clip;
                    }
                }
            }
        }

        // Segundo intento: buscar directamente por archivo
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
