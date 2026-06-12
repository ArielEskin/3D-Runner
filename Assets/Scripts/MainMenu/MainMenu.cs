using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider MusicSlider;
    public Slider SFXSlider;
    
    public void Start()
    {
        LoadVolume();
        MusicManager.instance.PlayMusic("MainMenu");
    }

    public void Play()
    {
        Debug.Log("Press On StartButton");
        
        MusicManager.instance.PlayMusic("GameMusic");
        
        SceneManager.LoadScene("Game");
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
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
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
