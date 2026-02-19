using UnityEngine;
using System;
using UnityEngine.AI;

/* NOTE  RESUMEN DEL AGENTE-ML
1. DETECCIÓN INTELIGENTE
    Confirmación de Visión (0.5s): El enemigo debe MANTENER CONTACTO VISUAL continuo durante 0.5segundos 
    para activar el modo HUIDA. Esto evita "tembleques" y que la IA reaccione por error al pasar tras objetos pequeños.
    Raycast de Visibilidad (VERDE/ROJO): Solo hay detección si no hay obstáculos físicos (etiqueta "Obstaculos") 
    entre el jugador y el agente dentro del rango de DETECCIÓN

2. MOVIMIENTO Y DISTANCIAS
    -FSM de 2 ESTADOS: Patrulla<-->Huída
     
3. OLVIDO 
    Tiempo de Olvido (5s): Tiempo que tarda el enemigo en volver a PATRULLA 
    si pierde de vista al jugador y este está lejos.

CONTRAS DEL MODELO ENTRENADO:
-El agente se ha vuelto un "VAGO MIEDOSO" que prefiere quedarse tieso recibiendo 
el castigo por ser visto antes que obtener PENALIZACIÓN por ir a ciegas, ya que, 
como su radar es de corto alcance y NO TIENE MEMORIA, si no tiene un muro a menos de 
4 metros se siente perdido, decide que es mejor malo conocido que bueno por conocer; 
- Modo "Cangrejo"
*/ 

namespace DAM_IA_ML
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemigo : MonoBehaviour
    {
        private FSM_Enemigo fsm;
        private NavMeshAgent agente;
        public NavMeshAgent Agente => agente;//Getter

        //2 ESTADOS
        private EstadoPatrulla ePATRULLA = new EstadoPatrulla();
        private EstadoHuir eHUIR = new EstadoHuir();

        // EVENTOS
        public Action OnJugadorDetectado;
        public Action OnJugadorPerdido;

        [Header("Configuración de Patrulla")]
        public Transform[] puntosPatrulla; 
        public int puntoDestino = 0;       
        public float v = 2f;

        [Header("Configuración ML / Alerta")]
        public Transform player;
        public float rangoDetec = 12f;   
        public float velocidadML = 8f;
        public float tiempoHastaIniciarPatrulla = 5f; 
        public float tiempoOffsetVision = 0.5f; //Evita parpadeos

        [Header("Estado Interno")]
        private float cronometroHastaInicPatrulla; //Irá decrementándose desde 5 hasta 0
        private float cronometroConfirmacionDeteccion; //Irá subiendo desde 0 hasta 0.5
        private bool modoAlertaActivo = false; // Una especie de "getter" para conocer el estado actual

        //IMPORTANTE: Accedemos al script Escurridizo para uso de funciones públicas: activar/desactivar ML, los gizmos, etc..
        private Escurridizo mlScript;


        private void Awake()
        {//Config agente y acciones
            agente = GetComponent<NavMeshAgent>();
            agente.speed = v;
            OnJugadorDetectado += IrAAlerta;
            OnJugadorPerdido += IrAPatrulla;
            //Acceso a script público para activar/desactivar elementos
            mlScript = GetComponent<Escurridizo>();
            if (mlScript != null) 
                mlScript.SetGizmos(false);
        }

        private void Start()
        {//Inicio FSM y "fichaje" del Player
            fsm = new FSM_Enemigo();
            fsm.ChangeState(this, ePATRULLA);
        }

        private void Update()
        {
            fsm.Update(this);//Miniactualizaciones según estado activo 
            if (player == null) return;

            float distancia = Vector3.Distance(transform.position, player.position);
            bool meVen = agenteVePlayer();
            Debug.Log("TE VEO? "+meVen);

            //LÓGICA DE DETECCIÓN CON FILTRO. "Me tienen fichado": Te estoy viendo y estas en rango
            if (meVen && distancia < rangoDetec){
                if (!modoAlertaActivo){
                    cronometroConfirmacionDeteccion += Time.deltaTime;
                    if (cronometroConfirmacionDeteccion >= tiempoOffsetVision){//0.5s
                        OnJugadorDetectado?.Invoke();//internamente pone modoAlertaActivo=true;
                    }
                }
                //Siempre que haya visión entre ambos, inicializa a 5s
                //y cuando NO haya visión irá decrementándose como se ve más abajo
                cronometroHastaInicPatrulla = tiempoHastaIniciarPatrulla; 
            }
            else{ //Si no me ven o no estoy en rango, el cronometro está a 0
                cronometroConfirmacionDeteccion = 0; 
            }

            //GESTIÓN DEL COMPORTAMIENTO EN HUIDA. 
            if (modoAlertaActivo){
                if (meVen){//2 VEO AL JUGADOR : Activamos GIZMOS, ML y movimiento
                    ToggleMLAgent(true);
                    agente.isStopped = true;
                    agente.velocity = Vector3.zero;
                }
                else{//3 NO VEO AL JUGADOR: Desactivamos GIZMOS y ML 
                    ToggleMLAgent(false);
                    agente.isStopped = true;
                    agente.velocity = Vector3.zero;

                    //Un AJUSTE EXTRA: Si el jugador está relativamente cerca, el enemigo olvida MUY despacio
                    if (distancia < rangoDetec * 0.8f) 
                        cronometroHastaInicPatrulla -= Time.deltaTime * 0.2f; 
                    else 
                        cronometroHastaInicPatrulla -= Time.deltaTime;

                    //Cuando el peligro ha pasado pasamos al estado PATRULLA
                    if (cronometroHastaInicPatrulla <= 0) OnJugadorPerdido?.Invoke();
                }
            }
        }

        private void IrAAlerta() {
            modoAlertaActivo = true;
            fsm.ChangeState(this, eHUIR);
            if (mlScript != null) mlScript.SetGizmos(true);
        }
        // Este código APAGA el ML y el cerebro de la IA para que el enemigo se calme y 
        // limpie todos sus rastros de alerta. Resetea la velocidad y los cronómetros para que el agente 
        // vuelva a caminar tranquilamente por sus puntos de patrulla.
        private void IrAPatrulla() {
            modoAlertaActivo = false;
            agente.isStopped = false;
            agente.speed = v;
            cronometroConfirmacionDeteccion = 0;
            ToggleMLAgent(false);
            fsm.ChangeState(this, ePATRULLA);
            if (mlScript != null) mlScript.SetGizmos(false);
        }

        private bool agenteVePlayer(){
            Vector3 posAgent = transform.position + Vector3.up * 1.5f;
            Vector3 posPlayer = player.position + Vector3.up * 1.5f;
            Vector3 direc = posPlayer - posAgent;

            //Raycast solo devuelve primer objeto con el que choca, por lo que
            //al chocar con collider de obstáculo, no ve al player
            if (Physics.Raycast(posAgent, direc, out RaycastHit hit, rangoDetec)) {
                if (hit.collider.CompareTag("Player")) return true;
            }
            return false;
        }

        private void ToggleMLAgent(bool active){
            if (mlScript != null && mlScript.enabled != active){
                mlScript.enabled = active;
                // Si desactivamos el ML, el NavMeshAgent debe recuperar el control
                if(!active) {
                    mlScript.EndEpisode(); 
                    agente.isStopped = true;
                }
            }
        }

    }
}