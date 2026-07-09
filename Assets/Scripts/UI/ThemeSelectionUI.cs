using TMPro;
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
    [SerializeField] private ThemeOption[] themes;
    [SerializeField] private TMP_Text themeNameText;
    [SerializeField] private Image themePreviewImage;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button startRunButton;
    [SerializeField] private Button backButton;

    private int currentIndex;

    private void Start()
    {
        EnsureProfileManagerExists();
        AutoFindReferences();
        EnsureAtLeastOneTheme();

        if (leftButton != null) leftButton.onClick.AddListener(PreviousTheme);
        if (rightButton != null) rightButton.onClick.AddListener(NextTheme);
        if (startRunButton != null) startRunButton.onClick.AddListener(StartRun);
        if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        ShowTheme();
    }

    private void PreviousTheme()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = themes.Length - 1;
        ShowTheme();
    }

    private void NextTheme()
    {
        currentIndex++;
        if (currentIndex >= themes.Length) currentIndex = 0;
        ShowTheme();
    }

    private void ShowTheme()
    {
        if (themes == null || themes.Length == 0) return;

        if (themeNameText != null)
        {
            themeNameText.text = themes[currentIndex].displayName;
        }

        if (themePreviewImage != null)
        {
            themePreviewImage.sprite = themes[currentIndex].previewImage;
        }
    }

    private void StartRun()
    {
        if (themes == null || themes.Length == 0) return;

        if (ProfileManager.instance.activeProfile != null)
        {
            ProfileManager.instance.activeProfile.selectedThemeID = themes[currentIndex].themeID;
            ProfileManager.instance.SaveActiveProfileJSON();
        }

        SceneManager.LoadScene("Game");
    }

    private void AutoFindReferences()
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

    private void EnsureAtLeastOneTheme()
    {
        if (themes != null && themes.Length > 0)
        {
            for (int i = 0; i < themes.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(themes[i].themeID))
                {
                    themes[i].themeID = i == 0 ? "Default" : "Theme_" + (i + 1);
                }

                if (string.IsNullOrWhiteSpace(themes[i].displayName))
                {
                    themes[i].displayName = i == 0 ? "Default" : "Theme " + (i + 1);
                }
            }

            return;
        }

        themes = new[]
        {
            new ThemeOption
            {
                themeID = "Default",
                displayName = "Default",
                previewImage = themePreviewImage != null ? themePreviewImage.sprite : null
            }
        };
    }

    private void EnsureProfileManagerExists()
    {
        if (ProfileManager.instance != null) return;

        GameObject profileManagerObject = new GameObject("ProfileManager");
        ProfileManager profileManager = profileManagerObject.AddComponent<ProfileManager>();
        profileManager.activeProfile = new PlayerProfileData("profile_1", "Player 1");
        profileManager.SaveActiveProfileJSON();
    }
}
