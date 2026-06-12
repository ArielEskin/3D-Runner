using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider MusicSlider;
    public Slider SFXSlider;
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Slider loadingSlider;
    
    public void Start()
    {
        LoadVolume();
        MusicManager.instance.PlayMusic("MainMenu");
    }

    public void Play()
    {
        Debug.Log("Press On StartButton");
        MusicManager.instance.PlayMusic("GameMusic");
        
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);
        
        StartCoroutine(LoadLevelASync("Game"));
    }
    
    IEnumerator LoadLevelASync(string levelToLoad)
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

    public void Quit()
    {
        Debug.Log("Press On QuitButton");
        Application.Quit();
        Debug.Log("Game Closed");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void UpdateMusicVolume(float volume)
    {
        Debug.Log("Music slider value: " + volume);
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
        Debug.Log("SFX slider value: " + volume);
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        
        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        Debug.Log("MusicSlider = " + MusicSlider);
        Debug.Log("SFXSlider = " + SFXSlider);
        
        MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}
