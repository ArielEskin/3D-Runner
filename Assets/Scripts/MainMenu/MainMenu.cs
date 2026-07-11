using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    
    // ==========references================================================================================================================================================

    public AudioMixer audioMixer;

    public Slider MusicSlider;
    public Slider sfxSlider;
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Slider loadingSlider;
    
    // ==========================================================================================================================================================

    public void Start() // Loads the saved audio preferences and starts the menu music
    {
        LoadVolume();
        MusicManager.instance.PlayMusic("MainMenu");
    }

    public void Play() // Swaps the UI to the loading screen and triggers the scene loading sequence
    {
        Debug.Log("Press On StartButton");
        // MusicManager.instance.PlayMusic("MainMenu");
        SceneManager.LoadScene("ProfileSelection");
    }
    
    IEnumerator LoadLevelASync(string levelToLoad) // A Coroutine that fakes a smooth loading bar while Unity loads the 3D environment in the background
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (fakeProgress < 1f || loadOperation.progress < 0.9f)
        {
            fakeProgress += Time.deltaTime / 3f;

            loadingSlider.value = Mathf.Clamp01(fakeProgress);

            yield return null;
        }

        loadingSlider.value = 1f;

        yield return new WaitForSeconds(0.5f);

        loadOperation.allowSceneActivation = true;
    }

    public void BackToMenu() // Loads the main menu scene from gameplay
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void UpdateMusicVolume(float volume) // Adjusts the live AudioMixer levels when the UI sliders are dragged.
    {
        Debug.Log("Music slider value: " + volume);
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume) // Adjusts the live AudioMixer levels when the UI sliders are dragged.
    {
        Debug.Log("SFX slider value: " + volume);
        audioMixer.SetFloat("SFXVolume", volume);

    }

    public void SaveVolume() // Writes the exact slider values using PlayerPrefs
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        
        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume() // Reads the exact slider values using PlayerPrefs
    {
        
        MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        
        Debug.Log("MusicSlider = " + MusicSlider);
        Debug.Log("SFXSlider = " + sfxSlider);
        
    }
}
