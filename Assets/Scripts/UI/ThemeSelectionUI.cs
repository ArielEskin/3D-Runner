using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class ThemeOption
{
    public string themeID;
    public string displayName;
    public Sprite previewImage;
}

public class ThemeSelectionUI : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    [SerializeField] private ThemeOption[] themes;
    [SerializeField] private TMP_Text themeNameText;
    [SerializeField] private Image themePreviewImage;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button startRunButton;
    [SerializeField] private Button backButton;

    private int currentIndex;
    private TMP_Text lockedMessageText;
    
    // ==========================================================================================================================================================

    private void Start() // Initializes buttons and displays the currently selected theme
    {
        EnsureProfileManagerExists();
        AutoFindReferences();
        EnsureAtLeastOneTheme();
        CreateLockedMessage();

        if (leftButton != null) leftButton.onClick.AddListener(PreviousTheme);
        if (rightButton != null) rightButton.onClick.AddListener(NextTheme);
        if (startRunButton != null) startRunButton.onClick.AddListener(StartRun);
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        ShowTheme();
    }

    private void PreviousTheme() // Cycles the selection backward to the previous theme in the list
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = themes.Length - 1;
        ShowTheme();
    }

    private void NextTheme() // Cycles the selection forward to the next theme in the list
    {
        currentIndex++;
        if (currentIndex >= themes.Length) currentIndex = 0;
        ShowTheme();
    }

    private void ShowTheme() // Updates the UI preview image and text to match the currently viewed theme
    {
        if (themes == null || themes.Length == 0) return;

        if (themeNameText != null)
        {
            themeNameText.text = themes[currentIndex].displayName;
        }

        if (themePreviewImage != null)
        {
            themePreviewImage.sprite = themes[currentIndex].previewImage;
            themePreviewImage.color = IsCurrentThemeUnlocked()
                ? Color.white
                : new Color(0.45f, 0.45f, 0.45f, 1f);
        }

        if (lockedMessageText != null)
        {
            lockedMessageText.color =
                new Color32(255, 220, 145, 255);
            lockedMessageText.text = IsCurrentThemeUnlocked()
                ? string.Empty
                : "LOCKED - Buy this theme in the shop";
        }
    }

    private void StartRun() // Starts the game using the selected theme, but blocks if it is locked
    {
        if (themes == null || themes.Length == 0) return;

        if (!IsCurrentThemeUnlocked())
        {
            if (lockedMessageText != null)
            {
                lockedMessageText.text =
                    "You need to buy this theme first!";
                lockedMessageText.color =
                    new Color32(255, 185, 95, 255);
            }
            return;
        }

        if (ProfileManager.instance.activeProfile != null)
        {
            ProfileManager.instance.activeProfile.selectedThemeID = themes[currentIndex].themeID;
            ProfileManager.instance.SaveActiveProfileJSON();
        }

        SceneManager.LoadScene("LoadingScene");
        MusicManager.instance.PlayMusic("GameMusic");
    }

    private void AutoFindReferences() // Automatically finds UI elements if they were not assigned in the Inspector
    {
        if (themeNameText == null)
        {
            GameObject themeName = GameObject.Find("ThemeNameText");
            if (themeName != null) themeNameText = themeName.GetComponent<TMP_Text>();
        }

        if (themePreviewImage == null)
        {
            GameObject preview = GameObject.Find("ThemePreviewImage");
            if (preview != null) themePreviewImage = preview.GetComponent<Image>();
        }

        if (leftButton == null)
        {
            GameObject left = GameObject.Find("LeftButton");
            if (left != null) leftButton = left.GetComponent<Button>();
        }

        if (rightButton == null)
        {
            GameObject right = GameObject.Find("RightButton");
            if (right != null) rightButton = right.GetComponent<Button>();
        }

        if (startRunButton == null)
        {
            GameObject start = GameObject.Find("StartRunButton");
            if (start != null) startRunButton = start.GetComponent<Button>();
        }

        if (backButton == null)
        {
            GameObject back = GameObject.Find("BackButton");
            if (back != null) backButton = back.GetComponent<Button>();
        }
    }

    private void EnsureAtLeastOneTheme() // Populates the theme list with a default option if it is completely empty
    {
        if (themes != null && themes.Length > 0)
        {
            for (int i = 0; i < themes.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(themes[i].themeID))
                {
                    themes[i].themeID = i == 0 ? "Earth" : "Theme_" + (i + 1);
                }

                if (string.IsNullOrWhiteSpace(themes[i].displayName))
                {
                    themes[i].displayName = i == 0 ? "Earth" : "Theme " + (i + 1);
                }
            }

            return;
        }

        themes = new[]
        {
            new ThemeOption
            {
                themeID = "Earth",
                displayName = "Earth",
                previewImage = themePreviewImage != null ? themePreviewImage.sprite : null
            }
        };
    }

    private void EnsureProfileManagerExists() // Creates a temporary ProfileManager for testing directly in the scene
    {
        if (ProfileManager.instance != null) return;

        GameObject profileManagerObject = new GameObject("ProfileManager");
        ProfileManager profileManager = profileManagerObject.AddComponent<ProfileManager>();
        profileManager.activeProfile = new PlayerProfileData("profile_1", "Player 1");
        profileManager.SaveActiveProfileJSON();
    }

    private bool IsCurrentThemeUnlocked() // Checks if the currently viewed theme is owned by the player
    {
        PlayerProfileData profile = ProfileManager.instance != null
            ? ProfileManager.instance.activeProfile
            : null;
        if (profile == null || themes == null || themes.Length == 0)
        {
            return currentIndex == 0;
        }

        EnsureThemeData(profile);
        return profile.unlockedThemes.Contains(themes[currentIndex].themeID);
    }

    private void EnsureThemeData(PlayerProfileData profile) // Validates the profile data to ensure default themes exist
    {
        if (profile.unlockedThemes == null)
        {
            profile.unlockedThemes = new List<string>();
        }

        profile.unlockedThemes.Remove("Default");
        if (!profile.unlockedThemes.Contains("Earth"))
        {
            profile.unlockedThemes.Add("Earth");
        }
    }

    private void CreateLockedMessage() // Spawns the warning message text that appears when viewing locked themes
    {
        if (lockedMessageText != null || themeNameText == null)
        {
            return;
        }

        GameObject messageObject = new GameObject(
            "LockedThemeMessage",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        messageObject.transform.SetParent(themeNameText.transform.parent, false);

        RectTransform sourceRect =
            themeNameText.GetComponent<RectTransform>();
        RectTransform messageRect =
            messageObject.GetComponent<RectTransform>();
        messageRect.anchorMin = sourceRect.anchorMin;
        messageRect.anchorMax = sourceRect.anchorMax;
        messageRect.pivot = sourceRect.pivot;
        messageRect.anchoredPosition =
            sourceRect.anchoredPosition + new Vector2(0f, -95f);
        messageRect.sizeDelta = new Vector2(620f, 75f);

        lockedMessageText =
            messageObject.GetComponent<TextMeshProUGUI>();
        lockedMessageText.alignment = TextAlignmentOptions.Center;
        lockedMessageText.enableAutoSizing = true;
        lockedMessageText.fontSizeMin = 20f;
        lockedMessageText.fontSizeMax = 36f;
        lockedMessageText.fontStyle = FontStyles.Bold;
        lockedMessageText.color = new Color32(255, 220, 145, 255);
        lockedMessageText.outlineWidth = 0.2f;
        lockedMessageText.outlineColor =
            new Color32(55, 20, 10, 255);
        lockedMessageText.raycastTarget = false;
    }
}
