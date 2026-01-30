using UnityEngine;

public class CicloDiaNoche : MonoBehaviour
{
    [Header("Ajustes de Velocidad")]
    public float velocidadCiclo = 1.0f; // Cuanto más alto, más rápido pasa el día

    void Update(){
        // Rotamos la luz en el eje X constantemente
        transform.Rotate(Vector3.right * velocidadCiclo * Time.deltaTime);
    }
}
