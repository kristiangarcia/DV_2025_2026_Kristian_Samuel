using UnityEngine;

// Sistema de Partículas de Sangre - DeadWave
// Crea un efecto de salpicadura de sangre al impactar un zombie
// Todo generado por código, no necesita prefabs ni configuración manual

public class EfectoSangre : MonoBehaviour
{
    /// <summary>
    /// Crea una explosión de partículas de sangre en el punto de impacto
    /// </summary>
    public static void Crear(Vector3 posicion, Vector3 direccionImpacto)
    {
        GameObject obj = new GameObject("EfectoSangre");
        obj.transform.position = posicion;

        // Orientar el efecto en la dirección opuesta al disparo
        if (direccionImpacto != Vector3.zero)
            obj.transform.rotation = Quaternion.LookRotation(direccionImpacto);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        // Detener para configurar
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // ============ MÓDULO PRINCIPAL ============
        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.6f, 0.0f, 0.0f, 1f),  // Rojo oscuro
            new Color(0.3f, 0.0f, 0.0f, 1f)    // Rojo muy oscuro
        );
        main.gravityModifier = 1.5f;
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;

        // ============ EMISIÓN: Ráfaga instantánea ============
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 15, 25)
        });

        // ============ FORMA: Cono para dispersión realista ============
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.1f;

        // ============ COLOR OVER LIFETIME: Fade out ============
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.7f, 0.0f, 0.0f), 0f),
                new GradientColorKey(new Color(0.3f, 0.0f, 0.0f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // ============ SIZE OVER LIFETIME: Se encogen ============
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.3f, 1.5f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ============ RENDERER: Material con shader de partículas ============
        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.5f, 0.0f, 0.0f, 1f);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        // ============ SUB-EFECTO: Gotas que caen ============
        GameObject gotasObj = new GameObject("Gotas");
        gotasObj.transform.SetParent(obj.transform, false);
        ParticleSystem gotasPS = gotasObj.AddComponent<ParticleSystem>();
        gotasPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var gotasMain = gotasPS.main;
        gotasMain.duration = 0.1f;
        gotasMain.loop = false;
        gotasMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        gotasMain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        gotasMain.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        gotasMain.startColor = new Color(0.4f, 0.0f, 0.0f, 0.9f);
        gotasMain.gravityModifier = 2.5f;
        gotasMain.maxParticles = 20;
        gotasMain.simulationSpace = ParticleSystemSimulationSpace.World;
        gotasMain.playOnAwake = false;

        var gotasEmission = gotasPS.emission;
        gotasEmission.enabled = true;
        gotasEmission.rateOverTime = 0;
        gotasEmission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 8, 15)
        });

        var gotasShape = gotasPS.shape;
        gotasShape.enabled = true;
        gotasShape.shapeType = ParticleSystemShapeType.Hemisphere;
        gotasShape.radius = 0.2f;

        var gotasRenderer = gotasObj.GetComponent<ParticleSystemRenderer>();
        gotasRenderer.material = renderer.material;

        // ============ ARRANCAR Y AUTO-DESTRUIR ============
        ps.Play();
        gotasPS.Play();

        // Destruir después de que terminen todas las partículas
        Destroy(obj, 2f);
    }
}
