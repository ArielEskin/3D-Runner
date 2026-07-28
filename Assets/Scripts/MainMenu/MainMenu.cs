using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private TMP_Text coinsAmountText;
    
    // ==========================================================================================================================================================

    public void Start() // Loads the saved audio preferences and starts the menu music
    {
        EnsureProfileManagerExists();
        LoadVolume();
        LoadLastProfile();
        UpdateCoinText(); // load the coins amount the player have 
        MusicManager.instance.PlayMusic("MainMenu");
    }

    public void Play() // Swaps the UI to the loading screen and triggers the scene loading sequence
    {
        Debug.Log("Press On StartButton");
        // MusicManager.instance.PlayMusic("MainMenu");
        SceneManager.LoadScene("ProfileSelection");
    }

    public void UpdateCoinText()
    {
        if (coinsAmountText != null)
        {
            int totalCoins = 0;
            
            // If a player profile exists, use its saved coin total.
            if (ProfileManager.instance != null &&
                ProfileManager.instance.activeProfile != null)
            {
                totalCoins = ProfileManager.instance.activeProfile.totalCoins;
            }

            // Show the number on the Main Menu.
            coinsAmountText.text = totalCoins.ToString();
        }
    }

    private void EnsureProfileManagerExists()
    {
        if (ProfileManager.instance != null) return;

        GameObject profileManagerObject = new GameObject("ProfileManager");
        profileManagerObject.AddComponent<ProfileManager>();
    }

    private void LoadLastProfile()
    {
        string lastProfileID = PlayerPrefs.GetString("LastSelectedProfileID", "");
        if (string.IsNullOrEmpty(lastProfileID) || ProfileManager.instance == null) return;

        PlayerProfileData savedProfile = ProfileManager.instance.LoadProfile(lastProfileID);
        if (savedProfile != null)
        {
            ProfileManager.instance.activeProfile = savedProfile;
        }
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

    public void OpenAchievements() 
    {
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.OpenAchievements();
        }
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
        
        PlayerPrefs.Save();
    }

    public void LoadVolume() // Reads the exact slider values using PlayerPrefs
    {
        
        MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        
        Debug.Log("MusicSlider = " + MusicSlider);
        Debug.Log("SFXSlider = " + sfxSlider);
        
    }
}
