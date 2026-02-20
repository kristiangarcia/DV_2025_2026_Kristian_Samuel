using UnityEngine;

/** DAV - 2ºDAM
 * Gestor de música para el Menú Principal.
 * Rota aleatoriamente entre las pistas asignadas sin repetir la actual.
 * Añadir al GameObject de la escena MenuPrincipal y asignar las 7 pistas.
 */
public class GestorMusicaMenu : MonoBehaviour
{
    [Header("Pistas de Música")]
    [Tooltip("Arrastra aquí las 7 pistas del pack (01 a 07).")]
    public AudioClip[] pistas;

    [Header("Configuración")]
    [Range(0f, 1f)]
    public float volumen = 0.6f;

    private AudioSource audioSource;
    private int ultimaPista = -1;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.volume = volumen;
        ReproducirSiguiente();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
            ReproducirSiguiente();
    }

    void ReproducirSiguiente()
    {
        if (pistas == null || pistas.Length == 0) return;

        int indice;
        do
        {
            indice = Random.Range(0, pistas.Length);
        } while (indice == ultimaPista && pistas.Length > 1);

        ultimaPista = indice;
        audioSource.clip = pistas[indice];
        audioSource.Play();
    }
}
