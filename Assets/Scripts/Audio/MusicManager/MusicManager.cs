using System;
using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // ==========references================================================================================================================================================

    public static MusicManager instance;
    
    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;
    
    // ==========================================================================================================================================================
    
    public void Awake() // Initializes the audio Singletons that persist across all menus and gameplay
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f) // Requests a specific music track and triggers the fading sequence
    {
        Debug.Log("PlayMusic called: " + trackName);
    
        AudioClip clip = musicLibrary.GetClipFromName(trackName);
    
        Debug.Log("Clip found: " + (clip != null ? clip.name : "NULL"));
    
        StartCoroutine(AnimateMusicCrossFade(clip, fadeDuration));
    }
    
    IEnumerator AnimateMusicCrossFade(AudioClip nextTrack, float fadeDuration = 0.5f) // A Coroutine that smoothly lowers the volume of the old track and raises the volume of the new one
    {
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1, 0f, percent);
            yield return null;
        }
        
        Debug.Log("Starting coroutine");
        
        musicSource.clip = nextTrack;
        musicSource.Play();
        
        Debug.Log("Is Playing: " + musicSource.isPlaying);
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
    }
    
}
