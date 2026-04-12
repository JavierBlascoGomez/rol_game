using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
    [Header("Configuración de Sonido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundClip;

    private void OnEnable()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.PlayOneShot(soundClip);
        }
        else
        {
            Debug.LogWarning("Falta el AudioSource o el AudioClip en " + gameObject.name);
        }
    }
}
