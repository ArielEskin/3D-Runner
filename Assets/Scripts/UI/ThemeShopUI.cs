using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemeShopUI : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    private static readonly string[] ThemeIDs =
    {
        "Earth",
        "Hell",
        "Winter"
    };

    private static readonly string[] DisplayNames =
    {
        "EARTH",
        "HELL",
        "SNOW"
    };

    private static readonly int[] Prices = { 0, 750, 500 };

    private readonly List<Button> themeButtons = new List<Button>();
    private readonly List<TMP_Text> statusTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> lockTexts = new List<TMP_Text>();

    // ==========================================================================================================================================================
    
    private void Start() // Initializes buttons and assigns theme icons
    {
        FindThemeButtons();

        for (int i = 0; i < themeButtons.Count; i++)
        {
            int capturedIndex = i;
            themeButtons[i].onClick.AddListener(
                () => HandleThemePressed(capturedIndex)
            );
            ApplyThemeIcon(themeButtons[i], ThemeIDs[i]);
        }

        Refresh();
    }

    private void FindThemeButtons() // Locates and sorts the shop buttons horizontally
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Button button = transform.GetChild(i).GetComponent<Button>();
            if (button != null)
            {
                themeButtons.Add(button);
            }
        }

        themeButtons.Sort(
            (left, right) =>
                left.transform.position.x.CompareTo(right.transform.position.x)
        );

        int count = Mathf.Min(ThemeIDs.Length, themeButtons.Count);
        if (themeButtons.Count > count)
        {
            themeButtons.RemoveRange(count, themeButtons.Count - count);
        }

        for (int i = 0; i < themeButtons.Count; i++)
        {
            statusTexts.Add(CreateStatusText(themeButtons[i], i));
            lockTexts.Add(CreateLockText(themeButtons[i]));
        }
    }

    private void ApplyThemeIcon(Button button, string themeID) // Loads and applies the specific theme preview image to a button
    {
        Texture2D texture = Resources.Load<Texture2D>(
            "ThemeIcons/" + themeID + "Icon"
        );
        if (texture == null)
        {
            return;
        }

        Sprite icon = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
        Image image = button.GetComponent<Image>();
        image.sprite = icon;
        image.preserveAspect = true;
    }

    private TMP_Text CreateStatusText(Button button, int index) // Spawns the text label showing price or ownership status
    {
        GameObject textObject = new GameObject(
            DisplayNames[index] + " Status",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(button.transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -12f);
        rect.sizeDelta = new Vector2(280f, 72f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 17f;
        text.fontSizeMax = 31f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color32(45, 20, 12, 255);
        text.raycastTarget = false;
        return text;
    }

    private TMP_Text CreateLockText(Button button) // Spawns the "LOCKED" overlay text for unowned themes
    {
        GameObject textObject = new GameObject(
            "Lock Label",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(button.transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(20f, 20f);
        rect.offsetMax = new Vector2(-20f, -20f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = "LOCKED";
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 20f;
        text.fontSizeMax = 38f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color32(255, 225, 180, 255);
        text.outlineWidth = 0.3f;
        text.outlineColor = new Color32(50, 15, 5, 255);
        text.raycastTarget = false;
        return text;
    }

    private void HandleThemePressed(int index) // Attempts to purchase the theme if the player has enough coins
    {
        PlayerProfileData profile = GetProfile();
        if (profile == null)
        {
            statusTexts[index].text = "Select a profile";
            return;
        }

        EnsureThemeData(profile);
        string themeID = ThemeIDs[index];
        if (profile.unlockedThemes.Contains(themeID))
        {
            statusTexts[index].text = "Owned";
            return;
        }

        if (profile.totalCoins < Prices[index])
        {
            statusTexts[index].text = "Not enough coins";
            return;
        }

        profile.totalCoins -= Prices[index];
        profile.unlockedThemes.Add(themeID);
        ProfileManager.instance.SaveActiveProfileJSON();

        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.CheckProgress();
        }

        MainMenu mainMenu = FindFirstObjectByType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.UpdateCoinText();
        }

        Refresh();
    }

    private void Refresh() // Updates the visual state of all buttons (grayed out vs full color) based on ownership
    {
        PlayerProfileData profile = GetProfile();
        if (profile != null)
        {
            EnsureThemeData(profile);
        }

        for (int i = 0; i < themeButtons.Count; i++)
        {
            bool owned = profile != null &&
                         profile.unlockedThemes.Contains(ThemeIDs[i]);
            Image image = themeButtons[i].GetComponent<Image>();
            image.color = owned
                ? Color.white
                : new Color(0.48f, 0.48f, 0.48f, 1f);
            lockTexts[i].gameObject.SetActive(!owned);

            if (owned)
            {
                statusTexts[i].text =
                    DisplayNames[i] + "\nOwned";
            }
            else
            {
                statusTexts[i].text =
                    DisplayNames[i] + "\n" + Prices[i] + " coins";
            }
        }
    }

    private PlayerProfileData GetProfile() // Safely retrieves the currently active player profile
    {
        return ProfileManager.instance != null
            ? ProfileManager.instance.activeProfile
            : null;
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

        if (string.IsNullOrWhiteSpace(profile.selectedThemeID) ||
            profile.selectedThemeID == "Default")
        {
            profile.selectedThemeID = "Earth";
        }
    }
}
