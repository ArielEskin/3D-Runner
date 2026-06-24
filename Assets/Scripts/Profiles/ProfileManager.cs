using System.IO;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager instance;
    public PlayerProfileData activeProfile; 

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

    // ==================================================================================================================
    // JSON SERIALIZATION (SAVING)
    // ==================================================================================================================
    public void SaveActiveProfile()
    {
        if (activeProfile == null) return;
        
        string json = JsonUtility.ToJson(activeProfile, true); // Convert the C# object into a JSON string
        string filePath = Path.Combine(Application.persistentDataPath, activeProfile.profileID + ".json"); // Define the save path (safe folder on Android)
        File.WriteAllText(filePath, json); // Write the text file to the phone's hard drive
        
        Debug.Log("Profile Saved Successfully at: " + filePath);
    }

    // ==================================================================================================================
    // JSON (LOADING PROFILE)
    // ==================================================================================================================
    public PlayerProfileData LoadProfile(string profileID)
    {
        string filePath = Path.Combine(Application.persistentDataPath, profileID + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath); // Read the text from the file

            PlayerProfileData loadedData = JsonUtility.FromJson<PlayerProfileData>(json); // Convert the JSON text back into our C# class
            return loadedData;
        }
        else
        {
            Debug.LogWarning("Save file not found for: " + profileID);
            return null;
        }
    }
    
    
    
    private void OnApplicationPause(bool isPaused) // Save when the app is minimized
    {
        if (isPaused)
        {
            Debug.Log("Game minimized or closing! Triggering Auto-Save...");
            SaveActiveProfile();
        }
    }
}