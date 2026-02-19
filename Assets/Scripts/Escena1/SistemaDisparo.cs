using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SistemaDisparo : MonoBehaviour
{
    public float dañoArma = 100f;
    public float rango = 100f;
    public float radioDisparo = 0.5f; // Grosor del rayo (SphereCast)
    public Camera camaraFPS;

    private HashSet<Collider> collidersIgnorados;

    void Start()
    {
        collidersIgnorados = new HashSet<Collider>();
        Transform raiz = transform.root;
        foreach (Collider col in raiz.GetComponentsInChildren<Collider>(true))
            collidersIgnorados.Add(col);

        // Buscar la cámara FPS desde ControlJugador
        if (camaraFPS == null)
        {
            ControlJugador control = FindFirstObjectByType<ControlJugador>();
            if (control != null)
            {
                if (control.fpsCamera != null)
                    camaraFPS = control.fpsCamera;

                foreach (Collider c in control.transform.root.GetComponentsInChildren<Collider>(true))
                    collidersIgnorados.Add(c);
            }
        }

        if (dañoArma < 100f) dañoArma = 100f;
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Disparar();
    }

    void Disparar()
    {
        if (camaraFPS == null) return;

        Vector3 origen = camaraFPS.transform.position;
        Vector3 direccion = camaraFPS.transform.forward;

        Debug.DrawRay(origen, direccion * rango, Color.red, 1.0f);

        // SphereCastAll: rayo GORDO que detecta con más facilidad
        RaycastHit[] hits = Physics.SphereCastAll(origen, radioDisparo, direccion, rango, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (collidersIgnorados.Contains(hit.collider))
                continue;

            if (hit.distance < 1.0f)
                continue;

            // Buscar zombie por componente (padre e hijos)
            ZombieKamikaze zombieK = hit.transform.GetComponentInParent<ZombieKamikaze>();
            if (zombieK == null) zombieK = hit.transform.GetComponentInChildren<ZombieKamikaze>();
            if (zombieK != null)
            {
                zombieK.RecibirDaño(dañoArma);
                return;
            }

            ZombieNormal zombieN = hit.transform.GetComponentInParent<ZombieNormal>();
            if (zombieN == null) zombieN = hit.transform.GetComponentInChildren<ZombieNormal>();
            if (zombieN != null)
            {
                zombieN.RecibirDaño(dañoArma);
                return;
            }

            // Buscar zombie por tag como último recurso
            if (hit.transform.root.CompareTag("Zombie"))
            {
                // Intentar destruir directamente
                ZombieNormal zn = hit.transform.root.GetComponent<ZombieNormal>();
                ZombieKamikaze zk = hit.transform.root.GetComponent<ZombieKamikaze>();
                if (zn != null) { zn.RecibirDaño(dañoArma); return; }
                if (zk != null) { zk.RecibirDaño(dañoArma); return; }
                // Si todo falla, destruir el objeto
                Destroy(hit.transform.root.gameObject);
                return;
            }
        }
    }
}