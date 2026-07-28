using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // ==========references================================================================================================================================================

    public static SoundManager instance;
    
    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2Dsource;

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

    public void PlaySound3D(AudioClip clip, Vector3 position) // Spawns a temporary audio source at a specific coordinate for spatial sound (like picking up a coin)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
    public void PlaySound3D(string soundName, Vector3 position)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), position);
    }

    public void PlaySound2D(string soundName) // Plays flat UI or system sounds directly into the player's ears
    {
        sfx2Dsource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));
    }
}
