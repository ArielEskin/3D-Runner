using System;
using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    // ============ refernce ============
    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;
    
    
    
    public void Awake()
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

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        Debug.Log("PlayMusic called: " + trackName);
    
        AudioClip clip = musicLibrary.GetClipFromName(trackName);
    
        Debug.Log("Clip found: " + (clip != null ? clip.name : "NULL"));
    
        StartCoroutine(AnimateMusicCrossFade(clip, fadeDuration));
    }
    
    IEnumerator AnimateMusicCrossFade(AudioClip nextTrack, float fadeDuration = 0.5f)
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
