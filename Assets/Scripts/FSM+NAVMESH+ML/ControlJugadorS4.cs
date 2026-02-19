using UnityEngine;
using UnityEngine.InputSystem;

// Control Jugador 3D
// Compatible con Enemigos FSM + NavMesh
// JMMolina 2025 - Adaptado correctamente a 3D
// Se ha tomado ControlJugador.cs de la Sesión1

namespace DAM_IA_ML
{
    //NOTE: Nuevo! Una forma de "añadir" componentes a un GO desde el código
    [RequireComponent(typeof(CharacterController))]
    public class ControlJugadorS4 : MonoBehaviour
    {
        private PlayerControls controls;//puente entre InputSystem y este script 
        private Vector2 entradaMov;
        private Vector2 entradaMirar;

        [Header("Ajustes de Movimiento")]
        public float velocMov = 5f;
        public float fuerzaSalto = 5f;
        private float velocVertical;

        [Header("Efecto de Caminar (Head Bob)")]
        public float frecuenciaBob = 5f;
        public float amplitudBob = 0.05f;
        private float temporizadorBob;
        private float cameraYdefecto;

        private CharacterController controller;

        [Header("Cámaras")]
        public Camera fpsCamera;
        public Camera tpsCamera;
        private bool esFPS = true;

        [Header("Rotación")]
        public float sensibMouse = 0.2f;
        private float pitchCamara = 0f;


        //NOTE: NUEVO!!
        // =========================
        // AÑADIDO (Knockback suave)
        // =========================
        public float fuerzaKnockback = 1.0f;
        public float duracionKnockback = 0.15f;
        //NOTE: NUEVO!!

        // =========================
        // UNITY
        // =========================
        private void Awake()
        {
            // Componentes
            controller = GetComponent<CharacterController>();
            controls = new PlayerControls();
            //DECLARACIÓN DE SUSCRIPCIONES (+): Cuando se ejecuta una acción, usa los valores asociados a esa acción
            //para lo que nos interese
            controls.Player.Move.performed += ctx =>
                entradaMov = ctx.ReadValue<Vector2>();
            controls.Player.Move.canceled += ctx =>
                entradaMov = Vector2.zero;
            controls.Player.Look.performed += ctx =>
                entradaMirar = ctx.ReadValue<Vector2>();
            controls.Player.Look.canceled += ctx =>
                entradaMirar = Vector2.zero;

            //ACCIONES GENERALES: Cambia Vista y Salir/Escape    
            controls.Player.Jump.performed += ctx => Salto();
            controls.Player.CambiaVista.performed += ctx => CambiaVista();
            controls.Player.Exit.performed += ctx => SalirJuego();
            cameraYdefecto = fpsCamera.transform.localPosition.y;
            LockCursor();
        }

        //Eventos necesarios para que el motor habilite/deshabilite los controles
        //Tanto al lanzar la escena como al salirnos.
        //Útiles para menús y menús emergentes.
        private void OnEnable() => controls.Player.Enable();
        private void OnDisable() => controls.Player.Disable();

        private void Update()
        {
            ManejarMovimiento();
            ManejarRotacion();
            if (esFPS) ManejarHeadBob();
        }

        private void ManejarMovimiento()
        {
            // Movimiento horizontal (X/Z)
            //Importante: Para que avance, la cámaray FPS DEBE ser hija del Player
            // x: transform.right es eje X local al GO, es decir, controlado por las teclas A/D
            // y: transform.forward es el eje Z local al GO, es decir, controlado por las teclas W/S

            Vector3 move =
                transform.right * entradaMov.x +
                transform.forward * entradaMov.y;

            controller.Move(move * velocMov * Time.deltaTime);

            //No usamos RayCast, la variable isGrounded controla el "toque" con el suelo

            if (controller.isGrounded && velocVertical < 0)
                velocVertical = -1f;

            //velocVertical la cambiamos al saltar
            velocVertical += Physics.gravity.y * Time.deltaTime;
            controller.Move(Vector3.up * velocVertical * Time.deltaTime);
        }
        //NOTE: NUEVO

        private void ManejarRotacion()//Giramos con respecto a Vector3.up (Eje Y)
        {
            transform.Rotate(Vector3.up * entradaMirar.x * sensibMouse);
            //Debug.Log(transform);
            if (esFPS)
            {
                pitchCamara -= entradaMirar.y * sensibMouse;
                pitchCamara = Mathf.Clamp(pitchCamara, -80f, 80f);
                fpsCamera.transform.localRotation =
                    Quaternion.Euler(pitchCamara, 0f, 0f);
            }
            else
            {
                tpsCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
            }
        }

        private void ManejarHeadBob()
        {
            if (!controller.isGrounded) return;

            if (entradaMov.magnitude > 0.1f)
            {
                temporizadorBob += Time.deltaTime * frecuenciaBob;
                float newY = cameraYdefecto +
                             Mathf.Sin(temporizadorBob) * amplitudBob;

                fpsCamera.transform.localPosition =
                    new Vector3(
                        fpsCamera.transform.localPosition.x,
                        newY,
                        fpsCamera.transform.localPosition.z
                    );
            }
            else
            {
                fpsCamera.transform.localPosition =
                    new Vector3(
                        fpsCamera.transform.localPosition.x,
                        Mathf.Lerp(
                            fpsCamera.transform.localPosition.y,
                            cameraYdefecto,
                            Time.deltaTime * 10f
                        ),
                        fpsCamera.transform.localPosition.z
                    );
            }
        }

        private void Salto()
        {
            if (controller.isGrounded)
                velocVertical = fuerzaSalto;
        }

        private void CambiaVista()
        {
            esFPS = !esFPS;
            fpsCamera.gameObject.SetActive(esFPS);
            tpsCamera.gameObject.SetActive(!esFPS);
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SalirJuego()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Enemigos"))
            {
                Debug.Log("<color=red>¡Cuidado! Has chocado con un enemigo!!.</color>");
                // =========================
                // AÑADIDO (Knockback mínimo)
                // =========================
                Vector3 dir = hit.transform.root.position - transform.position;//Calcula dirección golpe
                dir.y = 0f; //evita mover en vertical
                StartCoroutine(
                    KnockbackSuave(
                        hit.transform.root,
                        dir.normalized
                    )
                );
            }
        }

        //Función GENÉRICA KNOCKBACK
        private System.Collections.IEnumerator KnockbackSuave(Transform enemigo,Vector3 direccion)
        {
            Vector3 inicio = enemigo.position;
            Vector3 fin = inicio + direccion * fuerzaKnockback;

            float t = 0f; //Interpolación temporal
            while (t < duracionKnockback)
            {
                t += Time.deltaTime;
                enemigo.position = Vector3.Lerp(inicio, fin, t / duracionKnockback);
                yield return null;
            }
        }
    }


}
