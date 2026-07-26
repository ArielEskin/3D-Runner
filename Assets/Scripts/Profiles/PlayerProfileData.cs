using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfileData
{
    [Header("Profile Metadata")]
    public string profileID; // Unique identifier for the slot
    public string profileName; // Name the player types in for save file

    [Header("Progression & Stats")]
    public int totalCoins;
    public float highestDistance;
    public float longestTimeSurvived;

    [Header("Last Run Details")]
    public int lastRunSeed; // The seed needed to replay the same track
    public string screenshotPath; // The local file path to the saved thumbnail image
    public string lastPlayedDate;  // Last time played To show on the UI

    [Header("Player Preferences")]
    public string selectedThemeID; // The theme string to pass to the Scene Manager
    public int selectedSkinIndex; // The currently equipped skin
    public List<int> unlockedSkins; // The indices of all skins the player has bought
    public List<string> unlockedThemes; // List of themes the player has bought/unlocked
    public float musicVolume;         
    public float sfxVolume;
    public int inputModeIndex; // 0 for Buttons, 1 for Touch 
    
    [Header("Task 4 Progression")]
    // Lets old save files be safely upgraded exactly once.
    public bool task4ProgressMigrated;
    public int lifetimeCoinsCollected;
    public float lifetimeDistanceTravelled;
    public float lifetimeSecondsSurvived;
    public int permanentCoinBonusPercent;

    [Header("Task 4 Daily Reward")]
    public string lastDailyRewardUtc;

    [Header("Task 4 Milestones")]
    public List<MilestoneProgressData> milestones;
    
    public PlayerProfileData(string id, string name) // Constructor to create a fresh, default profile if one doesn't exist
    {
        profileID = id;
        profileName = name;
        lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        screenshotPath = "";
        
        totalCoins = 0;
        highestDistance = 0f;
        longestTimeSurvived = 0f;
        
        selectedThemeID = "Default";
        selectedSkinIndex = 0; // Default to first skin
        unlockedSkins = new List<int> { 0 }; // Player always owns the default skin
        lastRunSeed = Random.Range(1000, 99999); // Generate a random seed for the very first run
        unlockedThemes = new List<string> { "Default" };
        
        musicVolume = 0f;
        sfxVolume = 0f;
        inputModeIndex = 1; // Default to Touch
        
        task4ProgressMigrated = true;
        lifetimeCoinsCollected = 0;
        lifetimeDistanceTravelled = 0f;
        lifetimeSecondsSurvived = 0f;
        permanentCoinBonusPercent = 0;

        lastDailyRewardUtc = "";
        milestones = new List<MilestoneProgressData>();
    }
    
    [System.Serializable]
    public class MilestoneProgressData
    {
        public string milestoneId;
        public bool rewardClaimed;

        public MilestoneProgressData(string id)
        {
            milestoneId = id;
            rewardClaimed = false;
        }
    }
}
