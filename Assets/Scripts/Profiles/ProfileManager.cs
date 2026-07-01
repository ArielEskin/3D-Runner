using System.Collections;
using System.IO;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager instance;
    public PlayerProfileData activeProfile; 

    [Header("Screenshot Settings")]
    [Tooltip("Drag your Canvas here so the UI hides when the screenshot is taken!")]
    public Canvas mainGameplayUI; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ====================================================================================
    // SCREEN CAPTURE & SAVING SEQUENCE
    // ====================================================================================
    
    public void TriggerSaveSequence() // Call when the player dies, pauses, or clicks "Save & Quit"
    {
        if (activeProfile == null) return;
        StartCoroutine(CaptureScreenshotAndSave());
    }

    private IEnumerator CaptureScreenshotAndSave()
    {
        // Hide the UI for the thumbnail
        if (mainGameplayUI != null) mainGameplayUI.enabled = false;

        // Wait for Unity to finish rendering the camera to the screen
        yield return new WaitForEndOfFrame();

        // Create a blank texture to hold the pixel data
        int width = Screen.width;
        int height = Screen.height;
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read the pixels from the screen and apply them to the texture
        screenshotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshotTexture.Apply();

        // Turn the UI back on instantly so the player doesn't notice
        if (mainGameplayUI != null) mainGameplayUI.enabled = true;

        // Change the texture into a standard PNG format
        byte[] imageBytes = screenshotTexture.EncodeToPNG();
        Destroy(screenshotTexture);

        // Define the safe Android folder path and write the PNG to the hard drive
        string fileName = activeProfile.profileID + "_thumbnail.png";
        string imagePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(imagePath, imageBytes);
        
        Debug.Log("Screenshot saved at: " + imagePath);

        // Save that exact file path into our profile data UI can find it later
        activeProfile.screenshotPath = imagePath;

        SaveActiveProfileJSON(); 
    }

    // ====================================================================================
    // JSON SERIALIZATION (Backend Logic)
    // ====================================================================================
    private void SaveActiveProfileJSON()
    {
        string json = JsonUtility.ToJson(activeProfile, true); 
        string filePath = Path.Combine(Application.persistentDataPath, activeProfile.profileID + ".json"); 
        File.WriteAllText(filePath, json); 
        
        Debug.Log("Profile JSON Saved Successfully at: " + filePath);
    }

    public PlayerProfileData LoadProfile(string profileID)
    {
        string filePath = Path.Combine(Application.persistentDataPath, profileID + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath); 
            return JsonUtility.FromJson<PlayerProfileData>(json); 
        }
        
        Debug.LogWarning("Save file not found for: " + profileID);
        return null;
    }
    
    private void OnApplicationPause(bool isPaused) 
    {
        if (isPaused)
        {
            SaveActiveProfileJSON();
        }
    }
    
    // ====================================================================================
    // RESTORE STATE (Applying the JSON to the game)
    // ====================================================================================
    public void ApplyProfileToGameSession()
    {
        if (activeProfile == null) return;

        // Restore Audio n
        PlayerPrefs.SetFloat("MusicVolume", activeProfile.musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", activeProfile.sfxVolume);

        // Restore Controls
        if (InputManager.instance != null)
        {
            if (activeProfile.inputModeIndex == 0) InputManager.instance.SetButtonMode();
            else InputManager.instance.SetTouchMode();
        }

        // Inform the GameManager
        if (GameManager.gameManager != null)
        {
            // Set the exact seed so the track generates identically!
            Random.InitState(activeProfile.lastRunSeed);
            Debug.Log("Game state restored for profile: " + activeProfile.profileName);
        }
    }
    // ====================================================================================
    // DIRECTORY SCANNING (For  UI)
    // ====================================================================================
    public System.Collections.Generic.List<PlayerProfileData> GetAllSavedProfiles()
    {
        System.Collections.Generic.List<PlayerProfileData> allProfiles = new System.Collections.Generic.List<PlayerProfileData>();
        
        // Search the Android persistent path s for files ending in .json
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string path in filePaths)
        {
            // Read and deserialize every file we find
            string json = File.ReadAllText(path);
            PlayerProfileData profile = JsonUtility.FromJson<PlayerProfileData>(json);
            
            if (profile != null)
            {
                allProfiles.Add(profile);
            }
        }
        return allProfiles; // Hand the complete list over to UI script
    }
}