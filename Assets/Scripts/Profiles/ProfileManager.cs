using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    // ==========references================================================================================================================================================

    public int selectedSkinIndex;
    public List<int> unlockedSkins;
    
    public static ProfileManager instance;
    public PlayerProfileData activeProfile; 

    [Header("Screenshot Settings")]
    [Tooltip("Drag your Canvas here so the UI hides when the screenshot is taken!")]
    public Canvas mainGameplayUI; 
    
    // ==========================================================================================================================================================

    private void Awake() //Initializes the Singleton pattern
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

    // ========================================================================================================================================================================
    // SCREEN CAPTURE & SAVING SEQUENCE
    // ========================================================================================================================================================================
    
    public void TriggerSaveSequence() //Initiates the Coroutine to safely capture a screenshot before saving.
    {
        if (activeProfile == null) return;
        StartCoroutine(CaptureScreenshotAndSave());
    }

    private IEnumerator CaptureScreenshotAndSave() // Hides the UI, saves the image as a PNG, and then triggers the JSON save
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

    // ========================================================================================================================================================================
    // JSON SERIALIZATION (Backend Logic)
    // ========================================================================================================================================================================
    public void SaveActiveProfileJSON() // Converts the active profile object into a JSON string and writes it to the device's data path
    {
        if (activeProfile == null) return;

        string json = JsonUtility.ToJson(activeProfile, true); 
        string filePath = Path.Combine(Application.persistentDataPath, activeProfile.profileID + ".json"); 
        File.WriteAllText(filePath, json); 
        
        Debug.Log("Profile JSON Saved Successfully at: " + filePath);
    }

    public PlayerProfileData LoadProfile(string profileID) // Reads a specific JSON file from the device and deserializes it back into a usable C# object
    {
        string filePath = Path.Combine(Application.persistentDataPath, profileID + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PlayerProfileData loadedProfile =
                JsonUtility.FromJson<PlayerProfileData>(json);

            bool profileChanged = UpgradeSkinProgress(loadedProfile);
            profileChanged |= UpgradeThemeProgress(loadedProfile);

            // Profiles made before Task 4 have no lifetime milestone data.
            // Use their best saved records as a fair starting point, once only.
            if (UpgradeTask4Progress(loadedProfile) || profileChanged)
            {
                File.WriteAllText(
                    filePath,
                    JsonUtility.ToJson(loadedProfile, true)
                );

                Debug.Log(
                    "Old profile upgraded for Task 4 milestones: " +
                    loadedProfile.profileName
                );
            }

            return loadedProfile;
        }
        
        Debug.LogWarning("Save file not found for: " + profileID);
        return null;
    }

    public bool DeleteProfile(PlayerProfileData profile)
    {
        if (profile == null || string.IsNullOrEmpty(profile.profileID))
        {
            return false;
        }

        string saveFolder = Path.GetFullPath(Application.persistentDataPath);
        string savePath = Path.GetFullPath(
            Path.Combine(saveFolder, profile.profileID + ".json")
        );

        // Safety check: only profile files inside this game's save folder may be removed.
        if (!savePath.StartsWith(saveFolder + Path.DirectorySeparatorChar))
        {
            Debug.LogError("Blocked an unsafe profile delete path.");
            return false;
        }

        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Profile save was not found: " + profile.profileName);
            return false;
        }

        DeleteThumbnailIfSafe(profile.screenshotPath, saveFolder);
        File.Delete(savePath);

        if (activeProfile != null &&
            activeProfile.profileID == profile.profileID)
        {
            activeProfile = null;

            if (PlayerPrefs.GetString("LastSelectedProfileID", "") ==
                profile.profileID)
            {
                PlayerPrefs.DeleteKey("LastSelectedProfileID");
                PlayerPrefs.Save();
            }
        }

        Debug.Log("Deleted profile: " + profile.profileName);
        return true;
    }

    private void DeleteThumbnailIfSafe(string thumbnailPath, string saveFolder)
    {
        if (string.IsNullOrEmpty(thumbnailPath))
        {
            return;
        }

        string fullThumbnailPath = Path.GetFullPath(thumbnailPath);
        if (fullThumbnailPath.StartsWith(saveFolder + Path.DirectorySeparatorChar) &&
            File.Exists(fullThumbnailPath))
        {
            File.Delete(fullThumbnailPath);
        }
    }

    private bool UpgradeTask4Progress(PlayerProfileData profile)
    {
        if (profile == null || profile.task4ProgressMigrated)
        {
            return false;
        }

        // An old profile did not record every historical run separately.
        // Its existing saved totals are the best trustworthy baseline.
        profile.lifetimeCoinsCollected = Mathf.Max(
            profile.lifetimeCoinsCollected,
            profile.totalCoins
        );
        profile.lifetimeDistanceTravelled = Mathf.Max(
            profile.lifetimeDistanceTravelled,
            profile.highestDistance
        );
        profile.lifetimeSecondsSurvived = Mathf.Max(
            profile.lifetimeSecondsSurvived,
            profile.longestTimeSurvived
        );

        if (profile.milestones == null)
        {
            profile.milestones =
                new System.Collections.Generic.List<
                    PlayerProfileData.MilestoneProgressData
                >();
        }

        profile.task4ProgressMigrated = true;
        return true;
    }

    private bool UpgradeSkinProgress(PlayerProfileData profile)
    {
        if (profile == null || profile.unlockedSkinIndices != null)
        {
            return false;
        }

        profile.unlockedSkinIndices = new System.Collections.Generic.List<int>
        {
            0
        };
        profile.selectedSkinIndex = 0;
        return true;
    }

    private bool UpgradeThemeProgress(PlayerProfileData profile)
    {
        if (profile == null)
        {
            return false;
        }

        bool changed = false;
        if (profile.unlockedThemes == null)
        {
            profile.unlockedThemes =
                new System.Collections.Generic.List<string>();
            changed = true;
        }

        if (profile.unlockedThemes.Remove("Default"))
        {
            changed = true;
        }

        if (!profile.unlockedThemes.Contains("Earth"))
        {
            profile.unlockedThemes.Add("Earth");
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(profile.selectedThemeID) ||
            profile.selectedThemeID == "Default")
        {
            profile.selectedThemeID = "Earth";
            changed = true;
        }

        if (!profile.unlockedThemes.Contains(profile.selectedThemeID))
        {
            profile.selectedThemeID = "Earth";
            changed = true;
        }

        return changed;
    }
    
    private void OnApplicationPause(bool isPaused)  // Automatically triggers a JSON data save when the mobile app is pushed to the background or closed.
    {
        if (isPaused) { SaveActiveProfileJSON(); }
    }

    private void OnApplicationQuit()
    {
        SaveActiveProfileJSON();
    }
    
    // ========================================================================================================================================================================
    // RESTORE STATE (Applying the JSON to the game)
    // ========================================================================================================================================================================
    public void ApplyProfileToGameSession() // Pushes the loaded JSON preferences (audio, controls, and seed) directly into the active game systems
    {
        if (activeProfile == null) return;

        // Restore Audio
        // PlayerPrefs.SetFloat("MusicVolume", activeProfile.musicVolume);
        // PlayerPrefs.SetFloat("SFXVolume", activeProfile.sfxVolume);

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
    // ========================================================================================================================================================================
    // DIRECTORY SCANNING (For  UI)
    // ========================================================================================================================================================================
    public System.Collections.Generic.List<PlayerProfileData> GetAllSavedProfiles() //Scans the Android storage for all JSON files and returns them as a list for the UI profile selector
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
