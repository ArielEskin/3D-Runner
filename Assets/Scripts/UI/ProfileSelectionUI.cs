using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private ProfileSlotUI profileSlotPrefab;
    [SerializeField] private string nextSceneName = "ThemeSelection";
    [SerializeField] private string backSceneName = "MainMenu";

    private void Start()
    {
        EnsureProfileManagerExists();
        AutoFindContentParent();
        WireBackButton();
        BuildProfileList();
    }

    private void AutoFindContentParent()
    {
        if (contentParent != null) return;

        GameObject contentObject = GameObject.Find("Content");
        if (contentObject != null)
        {
            contentParent = contentObject.transform;
        }
    }

    private void BuildProfileList()
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
            Destroy(child.gameObject);
        }

        List<PlayerProfileData> profiles = ProfileManager.instance.GetAllSavedProfiles();

        if (profiles.Count == 0)
        {
            profiles.Add(CreateStarterProfile());
        }

        foreach (PlayerProfileData profile in profiles)
        {
            ProfileSlotUI slot = Instantiate(profileSlotPrefab, contentParent);
            slot.Setup(profile, SelectProfile);
        }
    }

    private void SelectProfile(PlayerProfileData profile)
    {
        PlayerProfileData loadedProfile = ProfileManager.instance.LoadProfile(profile.profileID);
        ProfileManager.instance.activeProfile = loadedProfile != null ? loadedProfile : profile;
        ProfileManager.instance.SaveActiveProfileJSON();

        Debug.Log("Profile selected: " + ProfileManager.instance.activeProfile.profileName);
        SceneManager.LoadScene(nextSceneName);
    }

    private PlayerProfileData CreateStarterProfile()
    {
        PlayerProfileData starterProfile = new PlayerProfileData("profile_1", "Player 1");
        ProfileManager.instance.activeProfile = starterProfile;
        ProfileManager.instance.SaveActiveProfileJSON();
        return starterProfile;
    }

    private void WireBackButton()
    {
        GameObject backButtonObject = GameObject.Find("BackToMainMenu");
        if (backButtonObject == null) return;

        Button backButton = backButtonObject.GetComponent<Button>();
        if (backButton == null) return;

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() => SceneManager.LoadScene(backSceneName));
    }

    private void EnsureProfileManagerExists()
    {
        if (ProfileManager.instance != null) return;

        GameObject profileManagerObject = new GameObject("ProfileManager");
        profileManagerObject.AddComponent<ProfileManager>();
    }
}
