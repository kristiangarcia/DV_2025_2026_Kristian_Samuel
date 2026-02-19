using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/** JMMolina - DAV - 2ºDAM
 * CLASE SIMPLEAGENT
 * Estamos entrenando el "cerebro" de un enemigo que nos persigue. 
 * Aprendiendo mediante "premios" y "castigos" (Reinforcement Learning).
 */
public class SimpleAgent : Agent
{
    [Header("Referencias")]
    public Transform target; //El objetivo que el agente debe alcanzar.

    [Header("Param. Movimiento")]
    public float moveSpeed=3f; //Qué tan rápido se moverá

    [Header("Param. Entrenamiento")]
    public int maxSteps=2500; //El límite de tiempo (pasos) antes de que demos por fracasado un EPISODIO.

    [Header("Debug")]
    public bool activarLogs=true;
    public int logCadaNSteps=500;

    //VARIABLES DE CONTROL Y ESTADÍSTICAS
    private int episodeCount=0;      // Cuántas partidas (EPISODIOS) hemos jugado para entrenar.
    private int stepCount=0;         // Cuántas decisiones se ha tomado en este EPISODIO.
    private float episodeReward=0f;  //Puntos acumulados en el EPISODIO actual.
    private float totalReward=0f;    //Puntos acumulados en toda la historia del ENTRENAMIENTO.
    private bool episodeEnded=false; //Bandera para evitar que siga actuando tras ganar/perder.
    private float distanciaAnterior; //Para saber si en el paso anterior estábamos más lejos o cerca.

    // 1.INICIALIZACIÓN: Se ejecta una vez, cuando le damos al Play
    void Start(){
        //Detecta si Unity está conectado al "entrenador" (Python).
        bool training=Academy.Instance.IsCommunicatorOn;

        //Cuando estamos en TRAINING, el circuito es: CollectObservations-->IA-->OnActionReceived
        //Cuando no está en TRAINING, el circuito es: CollectObservations-->Unity-->OnActionReceived

        // Si estamos entrenando, aceleramos el tiempo (20x) para que la IA aprenda rápido.
        // Si ya está entrenado (inference), corre a velocidad normal (1x).
        Time.timeScale=training ? 20f : 1f;
        //5x - 20x training estable
        //20x - 100x agresivo: la física y la IA falla, no da tiempo a procesarla. No simula bien
    }

    // 2.REINICIO DEL EPISODIO: Se ejecuta cada vez que el agente GANA, PIERDE o se ACABA EL TIEMPO.
    public override void OnEpisodeBegin(){
        episodeEnded=false;
        episodeCount++;
        stepCount=0;
        episodeReward=0f;

        //Devolvemos al agente al punto de partida (el centro).
        transform.position=new Vector3(0f, 0.5f, 0f);

        //Si estamos entrenando, movemos el objetivo a un lugar al azar.
        //Esto evita que la IA solo "memorice" un camino y la obliga a "entender" dónde está el target.
        if (Academy.Instance.IsCommunicatorOn){
            target.position=new Vector3(
                Random.Range(-5f, 5f),
                0.5f,
                Random.Range(-5f, 5f)
            );
        }

        // Calculamos la distancia inicial para empezar a medir el progreso.
        distanciaAnterior=Vector3.Distance(transform.position, target.position);
        if (activarLogs) Debug.Log($"[Episodio {episodeCount}] INICIO");
    }

    //3. OBSERVACIONES: ¿Qué ve el agente? (Los ojos de la IA)
    public override void CollectObservations(VectorSensor sensor){
        //Calculamos un vector que apunta desde el Agente hacia el Objetivo.
        Vector3 relativePos=target.position - transform.position;
        //Le pasamos esta información a la red neuronal, las 3 OBSERVACIONES que necesita (Vector3) 
        //Dividimos por 10 para "NORMALIZAR" (a la IA funciona mejor con rangos acotados entre -1 y 1), 
        // la IA así distingue mejor las diferencias en los pesos de las neuronas.
        sensor.AddObservation(relativePos/10f); //Usamos 10 porque es la máxima distancia posible que puede haber
    }

    //4. ACCIONES Y RECOMPENSAS: El corazón del aprendizaje. la IA me devuelve
    public override void OnActionReceived(ActionBuffers actions){
        if (episodeEnded) return;

        stepCount++;

        //La IA nos envía dos números (array ContinuousActions) porque así se lo hemos dicho en Unity. 
        //Los limitamos (Clamp) para que no haga movimientos erráticos fuera de rango.
        //Estamos simulando pequeños PASITOS
        float moveX=Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ=Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // Aplicamos el movimiento físico en el mundo de Unity.
        Vector3 move=new Vector3(moveX, 0f, moveZ) * moveSpeed * Time.deltaTime;
        transform.position+= move;

        float distanciaActual=Vector3.Distance(transform.position, target.position);

        //SISTEMA DE RECOMPENSAS

        // A. DEAD ZONE: Si está muy cerca, le damos un premio diminuto para que se quede ahí y no vibre.
        if (distanciaActual < 0.3f){
            AddReward(0.001f);//Le decimos a la IA lo bueno o malo que es la recompensa
            episodeReward += 0.001f;//Lo anotamos nosotros para llevar un conteo
            return;
        }

        // B. PREMIO POR PROGRESO: Si la distancia actual es menor que la anterior, ¡está avanzando!
        //Esto es como decirle "caliente, caliente" mientras se acerca.
        float progress=distanciaAnterior - distanciaActual;
        float progressReward=progress * 0.5f; //Lo limitamos para que no sea tan grande
        AddReward(progressReward);
        episodeReward += progressReward;

        // C. PENALIZACIÓN POR MOVIMIENTO: Le restamos un poquito de puntos por moverse.
        // Esto le enseña a ser eficiente y no dar vueltas innecesarias (ahorro de energía).
        // Dicho en castellano: "Penaliza menos si alcanza objetivo en menos número de pasos"
        float penalizaMov=-move.magnitude * 0.001f;
        AddReward(penalizaMov);//Es negativa (penalización)
        episodeReward += penalizaMov;

        distanciaAnterior=distanciaActual;

        //D. TIMEOUT (Fracaso): Si llega al límite de pasos (maxSteps) sin tocar el target.
        if (stepCount >= maxSteps){
            episodeEnded=true;
            AddReward(-1.0f); // Gran castigo por no lograrlo a tiempo.
            episodeReward += -1.0f;
            LogFinEpisodio("FALLO");
            EndEpisode(); //Reinicia el ciclo. FIN!!!!!!!!
        }

        // Registro de logs cada N pasos para monitorear el progreso en consola.
        if (activarLogs && stepCount % logCadaNSteps == 0){
            float meanReward=episodeCount > 0 ? totalReward / episodeCount : 0f;
            Debug.Log($"[Ep {episodeCount}] Step {stepCount} | Reward: {episodeReward:F3}");
        }
    }

    //5. ÉXITO: Cuando el agente toca físicamente el objetivo.
    private void OnCollisionEnter(Collision collision){
        if (episodeEnded) return;

        if (collision.gameObject.CompareTag("Target")){
            episodeEnded=true;
            AddReward(2.0f); // ¡EL GORDO DE NAVIDAD! por completar la misión.
            episodeReward += 2.0f;

            LogFinEpisodio("ÉXITO");
            EndEpisode(); //Reinicia el ciclo, para seguir practivando. FIN!!!!!!!!
        }
    }

    //6. HEURÍSTICA: Nos permite controle al agente con el teclado.
    // Muy útil para probar si el agente tiene la fuerza/velocidad necesaria antes de entrenar la IA.
    public override void Heuristic(in ActionBuffers actionsOut){
        //Con solo estar esta cabecera nos deja "mover" el objeto en la escena.
        //Si no está no podemos mover el target y no podemos ver el resultado de lo que ocurre
        //cuando estamos probando con un ejemplo "real".

        //Podríamos también añadir el InputSystem y moverlo mediante un gamepad o teclas
    }

    //MÉTODO AUXILIAR: Para escribir en la consola los resultados finales con colores.
    private void LogFinEpisodio(string result){
        totalReward += episodeReward;
        float meanReward=totalReward/episodeCount;

        if (!activarLogs) return;
        string color=result == "ÉXITO" ? "green" : "red";
        Debug.Log($"<color={color}>[Episodio {episodeCount}] END ({result}) | Steps: {stepCount} | Reward: {episodeReward:F3}</color>");
    }
}