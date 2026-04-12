using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> playlist; 
    [SerializeField] private bool playOnStart = true;

    private int lastSongIndex = -1;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (playOnStart && playlist.Count > 0)
        {
            StartCoroutine(PlayPlaylist());
        }
    }

    IEnumerator PlayPlaylist()
    {
        while (true)
        {
            PlayRandomSong();
            yield return new WaitWhile(() => audioSource.isPlaying);
            yield return new WaitForSeconds(0.5f); 
        }
    }

    void PlayRandomSong()
    {
        if (playlist.Count == 0) return;

        int randomIndex;
        
        do {
            randomIndex = Random.Range(0, playlist.Count);
        } while (randomIndex == lastSongIndex && playlist.Count > 1);

        lastSongIndex = randomIndex;
        
        audioSource.clip = playlist[randomIndex];
        audioSource.Play();
        
        Debug.Log("Reproduciendo ahora: " + audioSource.clip.name);
    }
}
