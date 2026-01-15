using UnityEngine;
using UnityEngine.InputSystem;

//Control Jugador 
//JMMolina 2025
//DAVJ - 2ºDAM


public class ControlJugador : MonoBehaviour{
    private PlayerControls controls;//puente entre InputSystem y este script 
    private Vector2 entradaMov;
    private Vector2 entradaMirar;

    [Header("Ajustes de Movimiento")]
    public float velocMov = 5f;
    public float sensibMouse = 0.2f;
    public float fuerzaSalto = 5f;

    [Header("Efecto de Caminar (Head Bob)")]
    public float frecuenciaBob = 5f;
    public float amplitudBob = 0.05f;
    private float temporizadorBob = 0f;
    private float cameraYdefecto;

    private CharacterController controller;
    private float velocVertical;
    private float pitchCamara = 0f;

    [Header("Cámaras")]
    public Camera fpsCamera;
    public Camera tpsCamera;
    private bool esFPS = true;

    private void Awake(){
        controls = new PlayerControls();
        controller = GetComponent<CharacterController>();

        //DECLARACIÓN DE SUSCRIPCIONES (+): Cuando se ejecuta una acción, usa los valores asociados a esa acción
        //para lo que nos interese
        //Cuando el InputSystem ejecute alguna acción Move, entonces cambia datos para que nos podamos mover
        controls.Player.Move.performed += AlMoverse;
        //El resto de eventos se reescriben con menos líneas
        // ctx es el valor que cambia de la función lambda, no se declara
        controls.Player.Move.canceled += ctx => entradaMov = Vector2.zero;
        controls.Player.Look.performed += ctx => entradaMirar = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => entradaMirar = Vector2.zero;
        controls.Player.Jump.performed += ctx => Salto();
        
        //ACCIONES GENERALES: Cambia Vista y Salir/Escape
        controls.Player.CambiaVista.performed += ctx => CambiaVista();
        controls.Player.Exit.performed += ctx => SalirJuego();
        cameraYdefecto = fpsCamera.transform.localPosition.y;
        LockCursor(); //Ancla ratón al centro y esconde puntero
    }

    //Si hay cambios en el vector de movimiento, se los pasa al evento Move y lo activa (performed)
    private void AlMoverse(InputAction.CallbackContext context){
        entradaMov = context.ReadValue<Vector2>();
    }

    //Eventos necesarios para que el motor habilite/deshabilite los controles
    //Tanto al lanzar la escena como al salirnos.
    //Útiles para menús y menús emergentes.
    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    //Bucle principal
    void Update(){
        ManejarMovimiento();
        ManejarRotacion();
        if (esFPS) ManejarHeadBob();
    }

    void ManejarMovimiento(){
        //Importante: Para que avance, la cámaray FPS DEBE ser hija del Player
        // x: transform.right es eje X local al GO, es decir, controlado por las teclas A/D
        // y: transform.forzard es el eje Z local al GO, es decir, controlado por las teclas W/S
        Vector3 move = transform.right * entradaMov.x + transform.forward * entradaMov.y;
        //Debug.Log(move);
        controller.Move(move * velocMov * Time.deltaTime);//Aquí se realiza el movimiento sobre el eje X/Z

        if (controller.isGrounded && velocVertical < 0)
            velocVertical = -1f; //Reseteamos velocidad a un valor estable. Evita que la gravedad se acumule al estar parado
        
        velocVertical += Physics.gravity.y * Time.deltaTime;
        controller.Move(Vector3.up * velocVertical * Time.deltaTime); //Aquí se realiza el movimiento sobre el eje Y
    }

    void ManejarRotacion(){//Giramos con respecto a Vector3.up (Eje Y)
        transform.Rotate(Vector3.up * entradaMirar.x * sensibMouse);
        //Debug.Log(transform);
        if (esFPS){
            pitchCamara -= entradaMirar.y * sensibMouse;
            pitchCamara = Mathf.Clamp(pitchCamara, -80f, 80f);//Limita rotación de "cabeza"
            fpsCamera.transform.localRotation = Quaternion.Euler(pitchCamara, 0f, 0f);//Rota cámara con respecto a jugador (su padre). Usamos localRotation y no Rotation
        }
        else{
            tpsCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
        }
    }

    void ManejarHeadBob(){
        if (!controller.isGrounded) return; //Saltando no hace HeadBob

        if (entradaMov.magnitude > 0.1f){ //Comprueba si estamos usando joystick o teclas.
        //Al usar joystick, podría mandar pequeñas señales por desgaste, así evitamos que se esté movimiento debido a este error
            temporizadorBob += Time.deltaTime * frecuenciaBob; //Controlamos lo rápido que anda
            float newY = cameraYdefecto + Mathf.Sin(temporizadorBob) * amplitudBob; //Onda sinusoidal (SENO)
            fpsCamera.transform.localPosition = new Vector3(fpsCamera.transform.localPosition.x, newY, fpsCamera.transform.localPosition.z);//Solo cambio Y
        }
        else{ //Hace un Lerp para si no nos movemos no haya brusquedad
            fpsCamera.transform.localPosition = new Vector3(fpsCamera.transform.localPosition.x, Mathf.Lerp(fpsCamera.transform.localPosition.y, cameraYdefecto, Time.deltaTime * 10f), fpsCamera.transform.localPosition.z);
        }
    }

    void SalirJuego(){
        //Usamos pragmas (directivas)
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Si estamos en el editor de Unity, deja de reproducir
        #else
            Application.Quit();// Si es el juego real (.exe), cierra la aplicación
        #endif
    }

    void LockCursor(){
        Cursor.lockState = CursorLockMode.Locked;//Tendría sentido desbloquearlo?? Ejemplos?d
        Cursor.visible = false;
    }

    void Salto() { if (controller.isGrounded) velocVertical = fuerzaSalto; } //La propipedad .isGrounded evita usar RayCast

    void CambiaVista(){ //Acrivo/Desactivo cámaras
        esFPS = !esFPS;
        fpsCamera.gameObject.SetActive(esFPS);
        tpsCamera.gameObject.SetActive(!esFPS);
    }


    // 1. COLISIONES SÓLIDAS (Rocas y Árboles)
    // Se usa OnControllerColliderHit porque el CharacterController no detecta OnCollisionEnter
    private void OnControllerColliderHit(ControllerColliderHit hit){
        // Usamos LayerMask para identificar la capa por nombre
        if (hit.gameObject.layer == LayerMask.NameToLayer("Rocks")){
            Debug.Log("<color=red>¡Cuidado! Has chocado con una roca sólida.");
        }
        
        if (hit.gameObject.layer == LayerMask.NameToLayer("Trees")){
            Debug.Log("<color=green>Te has chocado contra un árbol del bosque.");
        }

        if (hit.gameObject.CompareTag("BlueRock"))
        {
            Debug.Log("<color=lightblue>¡Cuidado! Has chocado con una ROCA AZUL (Tag detectado).</color>");
        }
    }

    // 2. COLISIÓN TIPO TRIGGER (Mushrooms)
    // Para que esto funcione, los champiñones deben tener la casilla "Is Trigger" marcada
    private void OnTriggerEnter(Collider other){
        if (other.gameObject.layer == LayerMask.NameToLayer("Mushrooms")){
            Debug.Log("<color=yellow>Has pasado por encima de un champiñón (Trigger detectado).");
            // Aquí podrías añadir lógica extra, como destruir el objeto o sumar puntos
            // Destroy(other.gameObject); 
        }
    }


}