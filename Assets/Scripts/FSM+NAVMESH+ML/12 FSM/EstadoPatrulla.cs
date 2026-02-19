using UnityEngine;

namespace DAM_IA_ML
{
    public class EstadoPatrulla : IEstados
    {
        public void Enter(Enemigo e) 
        { 
            Debug.Log("Entrando en Patrulla (NavMesh)"); 

            // =========================
            // AÑADIDO (NavMesh)
            // =========================
            if (e.Agente != null)
            {
                e.Agente.ResetPath();
                e.Agente.isStopped = false; // Permitimos movimiento
                e.Agente.speed = e.v;       // Restauramos velocidad base
               
            }

            // =========================
            // ELIMINADO (movimiento manual 2D)
            // =========================
            /*
            // Antes no había NavMesh, no hacía falta "activar" nada
            // El movimiento se gestionaba directamente en Update
            */
        }

        public void Update(Enemigo e)
        {
            if (e.puntosPatrulla == null || e.puntosPatrulla.Length == 0) return;

            // =========================
            // DESTINO ACTUAL
            // =========================
            Vector3 destino = e.puntosPatrulla[e.puntoDestino].position;
           

            // ======================================================
            // AÑADIDO (NavMesh)
            // Movimiento controlado por NavMeshAgent
            // ======================================================
            if (e.Agente != null)
            {
                e.Agente.SetDestination(destino);

                // Esperamos a que el agente termine de calcular la ruta
                if (!e.Agente.pathPending &&
                    e.Agente.remainingDistance <= e.Agente.stoppingDistance + 0.2f)
                {
                    // Avanzamos al siguiente punto de patrulla
                    e.puntoDestino = (e.puntoDestino + 1) % e.puntosPatrulla.Length;
                }
            }

            // ======================================================
            // ELIMINADO (movimiento manual SIN NavMesh)
            // ======================================================
            /*
            // Movimiento directo por transform (2D / sin NavMesh)
            float speed = e.v * Time.deltaTime;
            e.transform.position = Vector3.MoveTowards(
                e.transform.position,
                destino,
                speed
            );

            // Comprobación de llegada manual
            if (Vector3.Distance(e.transform.position, destino) < 0.1f)
            {
                e.puntoDestino = (e.puntoDestino + 1) % e.puntosPatrulla.Length;
            }
            */
        }

        public void Exit(Enemigo e) 
        { 
            Debug.Log("Saliendo de Patrulla");

            // =========================
            // OPCIONAL (NavMesh)
            // =========================
            /*
            // Podríamos detener el agente aquí si el diseño lo requiere
            if (e.Agente != null)
                e.Agente.isStopped = true;
            */
        }
    }
}
