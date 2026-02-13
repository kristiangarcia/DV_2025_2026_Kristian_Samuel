using UnityEngine;

public class SistemaDisparo : MonoBehaviour
{
    public float dañoArma = 35f;
    public float rango = 100f;
    public Camera camaraFPS;
    public LayerMask capasAfectadas; // Asegúrate de que esto incluye "Default"

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Disparar();
        }
    }

    void Disparar()
    {
        RaycastHit hit;

        // DIBUJA EL RAYO ROJO EN LA ESCENA (Visible en la pestaña Scene)
        Debug.DrawRay(camaraFPS.transform.position, camaraFPS.transform.forward * rango, Color.red, 2.0f);

        // Lanzamos el rayo
        if (Physics.Raycast(camaraFPS.transform.position, camaraFPS.transform.forward, out hit, rango, capasAfectadas))
        {
            // --- DIAGNÓSTICO CLAVE ---
            Debug.Log("1. HE GOLPEADO A: " + hit.transform.name);
            Debug.Log("2. CAPA DEL OBJETO: " + LayerMask.LayerToName(hit.transform.gameObject.layer));

            // Buscamos el script en el objeto golpeado O EN SUS PADRES
            ZombieKamikaze zombie = hit.transform.GetComponentInParent<ZombieKamikaze>();

            if (zombie != null)
            {
                zombie.RecibirDaño(dañoArma);
                Debug.Log(">>> ¡ÉXITO! Script encontrado. Vida restada.");
            }
            else
            {
                // Si sale esto, aquí está el problema
                Debug.LogError(">>> ERROR: He golpeado al objeto, pero NO encuentro el script 'ZombieKamikaze' en él ni en sus padres.");
            }
        }
        else
        {
            Debug.Log("--- TIRO FALLIDO: El Raycast no ha tocado nada ---");
        }
    }
}