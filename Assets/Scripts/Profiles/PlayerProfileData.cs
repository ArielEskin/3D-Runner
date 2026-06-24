using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfileData
{
    [Header("Profile Metadata")]
    public string profileID;          // Unique identifier for the slot
    public string profileName;        // Name the player types in

    [Header("Progression & Stats")]
    public int totalCoins;
    public float highestDistance;
    public float longestTimeSurvived;

    [Header("Last Run Details")]
    public int lastRunSeed;           // The exact pseudorandom seed needed to replay the same track
    public string screenshotPath;     // The local file path to the saved thumbnail image
    public string lastPlayedDate;     // To show on the UI

    [Header("Player Preferences")]
    // Moving your MainMenu PlayerPrefs into the JSON save file
    public string selectedThemeID;    // The theme string to pass to the Scene Manager
    public List<string> unlockedThemes; // List of themes the player has bought/unlocked
    public float musicVolume;         
    public float sfxVolume;
    public int inputModeIndex;        // 0 for Buttons, 1 for Touch 
    
    // Constructor to create a fresh, default profile if one doesn't exist
    public PlayerProfileData(string id, string name)
    {
        profileID = id;
        profileName = name;
        lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        screenshotPath = "";
        
        totalCoins = 0;
        highestDistance = 0f;
        longestTimeSurvived = 0f;
        
        selectedThemeID = "Default";
        lastRunSeed = Random.Range(1000, 99999); // Generate a random seed for the very first run
        unlockedThemes = new List<string> { "Default" };
        
        musicVolume = 0f;
        sfxVolume = 0f;
        inputModeIndex = 1; // Default to Touch
    }
}