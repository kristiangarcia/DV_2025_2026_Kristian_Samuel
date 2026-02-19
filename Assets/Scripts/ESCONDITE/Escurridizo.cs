using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/** JMMolina - DAV - 2ºDAM
 * CLASE ESCURRIDIZO
 * Estamos entrenando el "cerebro" de un agente que se oculta al vernos tras una cobertura 
 * Aprendiendo mediante "premios" y "castigos" (Reinforcement Learning).
 */

    public class Escurridizo : Agent
    {
        [Header("Referencias")]
        public Transform player;//Jugador
        private NavMeshAgent navAgent;//Referencia al agente
        public GeneradorCoberturas gc;//Generador de coberturas

        [Header("Configuración de Rangos")]
        public float distanciaMaximaSoportada = 15f;//Si se aleja más, penaliza 
        public int pasosNecesariosOculto = 150; //Pasos que DEBERÍA de estar oculto tras la cobertura

        [Header("Debug")]
        public bool mostrarGizmos = true; // Variable para activar/desactivar visualmente

        //Variables para controlar la mecánica de OCULTACIÓN
        private int pasosQuieto = 0;//Contador de pasos quieto
        private bool isVisible;//Visible o no
        private string debugStatus = "";

        public override void Initialize()
        {
            if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
            //Configuramos el NavMeshAgent para que sea ágil
            navAgent.speed = 8f;
            navAgent.acceleration = 60f;
            navAgent.angularSpeed = 600f;
            navAgent.stoppingDistance = 0.1f;
        }

        public override void OnEpisodeBegin()
        {
            pasosQuieto = 0;
            
            // Resetear entorno si existe el generador
            if (gc != null) gc.ResetCovers();

            // Reposicionar Player y Agente aleatoriamente, si estamos en TRN
            if (Academy.Instance.IsCommunicatorOn){
                teletransporta(player, 2f);
                teletransporta(transform, 7f);
            }
            //Si hay agente sobre un NavMesh válido
            if (navAgent.isOnNavMesh) navAgent.ResetPath();
        }
        public override void CollectObservations(VectorSensor sensor)
        {
            // 7 OBSERVACIONES DIRECTAS, el resto con SENSOR3D (se añaden automáticamente)

            //1.Dirección y distancia al jugador (Normalizado)
            Vector3 dirToPlayer = (player.position - transform.position);
            sensor.AddObservation(dirToPlayer.normalized);//3 obs, en un array unitario donde la suma es siempre 1
            sensor.AddObservation(dirToPlayer.magnitude/distanciaMaximaSoportada);//1 obs
            //2.Estado de visibilidad actual
            sensor.AddObservation(isVisible ? 1f : 0f);//1 obs
            //3. Velocidad actual del agente
            sensor.AddObservation(navAgent.velocity.magnitude/navAgent.speed);// 1 obs
            //4. Proximidad a obstáculos (Normalizado a 4 metros)
            sensor.AddObservation(dameDistanciaAObstaculo()/4f);//4f es una distancia de referencia

        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (!navAgent.isOnNavMesh) return;

            // 1) MOVEMOS AGENTE
            //Interpretamos las acciones continuas como dirección X y Z
            Vector3 moveDir = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);

            //Usamos Move para un control más directo que SetDestination,
            //así es más orgánico el movimiento
            navAgent.Move(moveDir*navAgent.speed * Time.fixedDeltaTime);

            //2)ACTUALIZAMOS ESTADO DE VARIABLES PARA REWARDS
            isVisible = meEstaViendoPlayer();
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            float distObstaculo = dameDistanciaAObstaculo();
            bool estaPegado = distObstaculo < 1.5f;
            bool estaQuieto = navAgent.velocity.magnitude < 0.5f;

            //3) SISTEMA DE RECOMPENSAS
            if (!isVisible && estaPegado){// El agente está haciendo lo que queremos
                if (estaQuieto){
                    pasosQuieto++;
                    AddReward(0.010f); //Recompensa MEDIA por mantener la posición
                    debugStatus = "OBJETIVO: OCULTO Y QUIETO";
                }
                else{
                    AddReward(0.005f);//Recompensa LEVE por estar oculto aunque se mueva
                    debugStatus = "MOVIÉNDOSE HACIA COBERTURA";
                }
            }
            else
            {
                pasosQuieto = 0; //Si es visto o se aleja del muro, el contador vuelve a cero
                if (isVisible){
                    AddReward(-0.01f); //Penalización MEDIA por estar expuesto
                    debugStatus = "¡VISIBLE!";
                }
                else{
                    AddReward(-0.002f); //Penalización MUY LEVE por no estar pegado a un muro
                    debugStatus = "BUSCANDO MURO";
                }
            }

            //Castigo por tiempo (le obliga a encontrar cobertura rápido)
            AddReward(-1f / MaxStep);

            // 4) CONDICIONES DE FINALIZACIÓN ---
            //A. Éxito: Aguantó suficiente tiempo oculto
            if (pasosQuieto >= pasosNecesariosOculto){
                finalizaEpisodio("ÉXITO: Agente ocultado correctamente", 5.0f, true);
            }

            //B. Fallo: Se alejó demasiado del área de juego
            if (distToPlayer > distanciaMaximaSoportada){
                finalizaEpisodio("FALLO: Demasiado lejos del jugador", -1.0f, false);
            }
        }

        private bool meEstaViendoPlayer(){
            //Rayo desde jugador al agente
            Vector3 origin = player.position + Vector3.up * 1.5f;
            Vector3 target = transform.position + Vector3.up * 1.0f;
            Vector3 direction = target - origin;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, direction.magnitude + 0.1f)){
                // Si choca con algo etiquetado como "Obstaculos", no es visible
                if (hit.collider.CompareTag("Obstaculos")) return false;
            }
            return true;
        }

        private float dameDistanciaAObstaculo(){
            Collider[] colliders = Physics.OverlapSphere(transform.position, 4.0f);
            float minimaDistancia = 4.0f;
            foreach (var col in colliders){//Podría chocar con más de un objeto
                if (col.CompareTag("Obstaculos")){//ClosestPoint es el punto más cercano del colider
                    float dist = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
                    if (dist < minimaDistancia) minimaDistancia = dist;
                }
            }
            return minimaDistancia;
        }

        private void finalizaEpisodio(string reason, float bonus, bool success)
        {
            AddReward(bonus);
            string color = success ? "green" : "red";
            Debug.Log($"<color={color}><b>[{(success ? "LOGRADO" : "REINICIO")}]</b> {reason}</color>");
            EndEpisode();
        }

        //Hacemos un Warp(teletransporte) a posiciones aleatorias dentro de un radio
        private void teletransporta(Transform t, float radius){
            Vector3 randomPoint = Vector3.zero + Random.insideUnitSphere * radius;
            randomPoint.y = 0.5f;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius + 5f, NavMesh.AllAreas)){
                if (t == transform) navAgent.Warp(hit.position);
                else t.position = hit.position;
            }
        }

        // Dibujado de debug en el editor
        private void OnDrawGizmos(){
            if (!mostrarGizmos || player == null) return;
            
            //Línea entre Agente--Player
            Gizmos.color = isVisible ? Color.red : Color.green;
            Gizmos.DrawLine(player.position + Vector3.up * 1.5f, transform.position + Vector3.up * 1f);

//Etiquetas sobre el agente
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, debugStatus);
#endif
        }

        //Hay que dejar la cabecera para que pueda usarlo en Escena
        public override void Heuristic(in ActionBuffers actionsOut)
        {
        }

        /// <summary>
        /// Activa o desactiva la visualización de los Gizmos de depuración.
        /// </summary>
        /// <param name="state">True para mostrar, False para ocultar.</param>
        public void SetGizmos(bool state){
            mostrarGizmos = state;
        }
    }