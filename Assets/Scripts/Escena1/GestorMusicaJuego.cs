using UnityEngine;

/** DAV - 2ºDAM
 * Gestor de música para la escena de juego (Main).
 * Reproduce la canción de gameplay en bucle.
 * Añadir a un GameObject vacío en la escena Main y asignar la canción.
 */
public class GestorMusicaJuego : MonoBehaviour
{
    [Header("Música de Gameplay")]
    [Tooltip("Arrastra aquí 'ZOMBIES GIALLORE.wav'.")]
    public AudioClip musicaGameplay;

    [Range(0f, 1f)]
    public float volumen = 0.5f;

    void Start()
    {
        if (musicaGameplay == null)
        {
            Debug.LogWarning("[MusicaJuego] No hay AudioClip asignado.");
            return;
        }

        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.clip = musicaGameplay;
        src.loop = true;
        src.volume = volumen;
        src.Play();
    }
}
