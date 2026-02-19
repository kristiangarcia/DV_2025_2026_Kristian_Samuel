using UnityEngine;
using Unity.MLAgents; // Necesario para acceder al componente Agent

namespace DAM_IA_ML
{
    public class EstadoHuir : IEstados
    {
        public void Enter(Enemigo e)
        {
            Debug.Log("<color=#00FFFF><b>[FSM]</b> Entrando en Estado Alerta: ML-Agents activado</color>");

            //1. Preparamos el NavMeshAgent para el control de la IA
            if (e.Agente != null)
            {
                e.Agente.ResetPath();     // Limpiamos cualquier ruta de patrulla previa
                e.Agente.isStopped = false; // Nos aseguramos de que pueda moverse
                e.Agente.velocity = Vector3.zero;
            }

            //2. Activamos el componente de ML-Agents
            // Asumimos que el script se llama 'AgenteEntrenamientoFSM'
            var mlAgent = e.GetComponent<Escurridizo>();
            if (mlAgent != null){
                // Importante: Desactivamos el modo entrenamiento para evitar teletransportes
                mlAgent.enabled = true;
            }
            else{
                Debug.LogWarning("No se encontró el componente AgenteEntrenamientoFSM en el enemigo.");
            }
        }

        public void Update(Enemigo e)
        {
            // NOTA: No añadimos lógica de movimiento aquí.
            // El componente 'Decision Requester' en el Inspector pedirá decisiones al .onnx,
            // y el script 'Escurridizo' las ejecutará automáticamente.
            
            // Aquí solo podrías añadir una transición de salida, por ejemplo:
            // if (e.JugadorFueraDeRango()) { e.CambiarEstado(new EstadoPatrulla()); }
        }

        public void Exit(Enemigo e)
        {
            Debug.Log("<color=#FFA500><b>[FSM]</b> Saliendo de Estado Alerta: ML-Agents desactivado</color>");

            //3. Desactivamos el script de ML-Agents para que deje de controlar el NavMesh
            var mlAgent = e.GetComponent<Escurridizo>();
            if (mlAgent != null)
            {
                mlAgent.enabled = false;
            }

            //4. Limpiamos el estado del NavMesh para el siguiente estado (ej. Patrulla)
            if (e.Agente != null)
            {
                e.Agente.ResetPath();
                e.Agente.velocity = Vector3.zero;
            }
        }
    }
}