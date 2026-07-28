using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileSelectionUI : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    private const int MaxProfiles = 4;

    [SerializeField] private Transform contentParent;
    [SerializeField] private ProfileSlotUI profileSlotPrefab;
    [SerializeField] private Button newProfileButton;
    [SerializeField] private string nextSceneName = "ThemeSelection";
    [SerializeField] private string backSceneName = "MainMenu";
    
    // ==========================================================================================================================================================

    private void Start() // Initializes the UI, locates references, and populates the profile list
    {
        EnsureProfileManagerExists();
        AutoFindContentParent();
        AutoFindNewProfileButton();
        WireBackButton();
        WireNewProfileButton();
        BuildProfileList();
    }

    private void AutoFindContentParent() // Finds the scroll view content container automatically if missing
    {
        if (contentParent != null) return;

        GameObject contentObject = GameObject.Find("Content");
        if (contentObject != null)
        {
            contentParent = contentObject.transform;
        }
    }

    private void BuildProfileList() // Clears old slots and instantiates new ones for every saved profile
    {
        if (contentParent == null)
        {
            Debug.LogError("ProfileSelectionUI cannot find the Scroll View Content object.");
            return;
        }

        if (profileSlotPrefab == null)
        {
            Debug.LogError("ProfileSelectionUI needs your ProfileSlot prefab assigned in the Inspector.");
            return;
        }

        foreach (Transform child in contentParent)
        {
            // Keep the manually placed New Profile button in the ScrollView.
            // Only the profile-slot objects are rebuilt from saved data.
            if (child.GetComponent<ProfileSlotUI>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        List<PlayerProfileData> profiles = ProfileManager.instance.GetAllSavedProfiles();

        if (profiles.Count == 0)
        {
            profiles.Add(CreateStarterProfile());
        }

        foreach (PlayerProfileData profile in profiles)
        {
            ProfileSlotUI slot = Instantiate(profileSlotPrefab, contentParent);
            slot.Setup(profile, SelectProfile, DeleteProfile);
        }

        UpdateNewProfileButtonState(profiles.Count);
    }

    private void SelectProfile(PlayerProfileData profile) // Loads the chosen profile as active and transitions to the next scene
    {
        PlayerProfileData loadedProfile = ProfileManager.instance.LoadProfile(profile.profileID);
        ProfileManager.instance.activeProfile = loadedProfile != null ? loadedProfile : profile;

        // Remember which profile Main Menu should load when the app is opened again.
        PlayerPrefs.SetString("LastSelectedProfileID", ProfileManager.instance.activeProfile.profileID);
        PlayerPrefs.Save();
        ProfileManager.instance.SaveActiveProfileJSON();

        Debug.Log("Profile selected: " + ProfileManager.instance.activeProfile.profileName);
        SceneManager.LoadScene(nextSceneName);
    }

    private PlayerProfileData CreateStarterProfile() // Generates a default profile if no saved profiles exist
    {
        PlayerProfileData starterProfile = new PlayerProfileData("profile_1", "Player 1");
        ProfileManager.instance.activeProfile = starterProfile;
        ProfileManager.instance.SaveActiveProfileJSON();
        return starterProfile;
    }

    public void CreateNewProfile() // Generates a new profile with a unique ID and refreshes the list
    {
        EnsureProfileManagerExists();

        List<PlayerProfileData> existingProfiles = ProfileManager.instance.GetAllSavedProfiles();

        if (existingProfiles.Count >= MaxProfiles)
        {
            Debug.Log("Profile limit reached. Maximum profiles: " + MaxProfiles);
            UpdateNewProfileButtonState(existingProfiles.Count);
            return;
        }

        PlayerProfileData previouslyActiveProfile = ProfileManager.instance.activeProfile;

        // A GUID makes the save-file name unique, so no existing profile can be overwritten.
        string profileID = "profile_" + System.Guid.NewGuid().ToString("N");
        string profileName = "Player " + (existingProfiles.Count + 1);
        PlayerProfileData newProfile = new PlayerProfileData(profileID, profileName);

        // Save the new profile, then restore the profile that was selected before.
        ProfileManager.instance.activeProfile = newProfile;
        ProfileManager.instance.SaveActiveProfileJSON();
        ProfileManager.instance.activeProfile = previouslyActiveProfile;

        BuildProfileList();
        Debug.Log("Created new profile: " + profileName);
    }

    private void DeleteProfile(PlayerProfileData profile) // Deletes the selected profile from disk and refreshes the list
    {
        if (profile == null || ProfileManager.instance == null)
        {
            return;
        }

        if (ProfileManager.instance.DeleteProfile(profile))
        {
            BuildProfileList();
        }
    }

    private void WireBackButton() // Automatically finds and connects the back button to load the main menu
    {
        GameObject backButtonObject = GameObject.Find("BackToMainMenu");
        if (backButtonObject == null) return;

        Button backButton = backButtonObject.GetComponent<Button>();
        if (backButton == null) return;

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() => SceneManager.LoadScene(backSceneName));
    }

    private void AutoFindNewProfileButton() // Locates the 'New Profile' button in the content container if unassigned
    {
        if (newProfileButton != null || contentParent == null) return;

        foreach (Button button in contentParent.GetComponentsInChildren<Button>(true))
        {
            string buttonName = button.gameObject.name.ToLowerInvariant();
            if (buttonName.Contains("new") || buttonName.Contains("add") || buttonName.Contains("create"))
            {
                newProfileButton = button;
                return;
            }
        }
    }

    private void WireNewProfileButton() // Connects the 'New Profile' button to the creation logic
    {
        if (newProfileButton == null) return;

        newProfileButton.onClick.RemoveListener(CreateNewProfile);
        newProfileButton.onClick.AddListener(CreateNewProfile);
    }

    private void UpdateNewProfileButtonState(int profileCount) // Disables the 'New Profile' button if the maximum profile limit is reached
    {
        if (newProfileButton == null)
        {
            return;
        }

        newProfileButton.interactable = profileCount < MaxProfiles;
    }

    private void EnsureProfileManagerExists() // Creates a temporary ProfileManager for testing directly in the scene
    {
        if (ProfileManager.instance != null) return;

        GameObject profileManagerObject = new GameObject("ProfileManager");
        profileManagerObject.AddComponent<ProfileManager>();
    }
}
