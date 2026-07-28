using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinShopUI : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    private static readonly string[] SkinNames = { "Claire", "Ty", "Zoe" };
    private static readonly int[] SkinIndices = { 1, 2, 3 };
    private static readonly int[] Prices = { 100, 250, 500 };

    private readonly List<Button> skinButtons = new List<Button>();
    private readonly List<TMP_Text> statusTexts = new List<TMP_Text>();
    
    // ==========================================================================================================================================================
    
    private void Start() // Initializes buttons and assigns skin icons
    {
        FindShopButtons();

        for (int i = 0; i < skinButtons.Count; i++)
        {
            int capturedIndex = i;
            skinButtons[i].onClick.AddListener(
                () => HandleSkinPressed(capturedIndex)
            );

            Texture2D iconTexture = Resources.Load<Texture2D>(
                "SkinIcons/" + SkinNames[i] + "Icon"
            );
            if (iconTexture != null)
            {
                Sprite icon = Sprite.Create(
                    iconTexture,
                    new Rect(0f, 0f, iconTexture.width, iconTexture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                Image image = skinButtons[i].GetComponent<Image>();
                image.sprite = icon;
                image.preserveAspect = true;
            }
        }

        Refresh();
    }

    private void FindShopButtons() // Locates and sorts the skin shop buttons horizontally
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Button button = transform.GetChild(i).GetComponent<Button>();
            if (button == null)
            {
                continue;
            }

            skinButtons.Add(button);
        }

        skinButtons.Sort(
            (left, right) =>
                left.transform.position.x.CompareTo(right.transform.position.x)
        );

        int count = Mathf.Min(SkinNames.Length, skinButtons.Count);
        if (skinButtons.Count > count)
        {
            skinButtons.RemoveRange(count, skinButtons.Count - count);
        }

        for (int i = 0; i < skinButtons.Count; i++)
        {
            statusTexts.Add(CreateStatusText(skinButtons[i], i));
        }
    }

    private TMP_Text CreateStatusText(Button button, int index) // Spawns or finds the text label showing price or equip status
    {
        TMP_Text existing = button.GetComponentInChildren<TMP_Text>(true);
        if (existing != null)
        {
            return existing;
        }

        GameObject textObject = new GameObject(
            SkinNames[index] + " Status",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );
        textObject.transform.SetParent(button.transform, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -12f);
        rect.sizeDelta = new Vector2(280f, 65f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = 34f;
        text.color = Color.white;
        text.raycastTarget = false;
        text.fontStyle = FontStyles.Bold;
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color32(45, 20, 12, 255);
        return text;
    }

    private void HandleSkinPressed(int shopIndex) // Attempts to purchase or equip the selected skin
    {
        PlayerProfileData profile = GetProfile();
        if (profile == null)
        {
            statusTexts[shopIndex].text = "Select a profile";
            return;
        }

        EnsureSkinData(profile);
        int skinIndex = SkinIndices[shopIndex];
        bool unlocked = profile.unlockedSkinIndices.Contains(skinIndex);

        if (!unlocked)
        {
            if (profile.totalCoins < Prices[shopIndex])
            {
                statusTexts[shopIndex].text = "Not enough coins";
                return;
            }

            profile.totalCoins -= Prices[shopIndex];
            profile.unlockedSkinIndices.Add(skinIndex);

            if (GoalManager.Instance != null)
            {
                GoalManager.Instance.CheckProgress();
            }
        }
        else
        {
            profile.selectedSkinIndex =
                profile.selectedSkinIndex == skinIndex ? 0 : skinIndex;
            PlayerPrefs.SetInt("SelectedSkin", profile.selectedSkinIndex);
            PlayerPrefs.Save();
        }

        ProfileManager.instance.SaveActiveProfileJSON();

        MainMenu mainMenu = FindFirstObjectByType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.UpdateCoinText();
        }

        Refresh();
    }

    private void Refresh() // Updates the visual text of all buttons to show price, Equipped, or Unequipped
    {
        PlayerProfileData profile = GetProfile();

        for (int i = 0; i < statusTexts.Count; i++)
        {
            if (profile == null)
            {
                statusTexts[i].text = Prices[i] + " coins";
                continue;
            }

            EnsureSkinData(profile);
            int skinIndex = SkinIndices[i];
            if (!profile.unlockedSkinIndices.Contains(skinIndex))
            {
                statusTexts[i].text = Prices[i] + " coins";
            }
            else if (profile.selectedSkinIndex == skinIndex)
            {
                statusTexts[i].text = "Equipped";
            }
            else
            {
                statusTexts[i].text = "Unequipped";
            }
        }
    }

    private PlayerProfileData GetProfile() // Safely retrieves the currently active player profile
    {
        return ProfileManager.instance != null
            ? ProfileManager.instance.activeProfile
            : null;
    }

    private void EnsureSkinData(PlayerProfileData profile) // Validates the profile data to ensure the default skin is unlocked
    {
        if (profile.unlockedSkinIndices == null)
        {
            profile.unlockedSkinIndices = new List<int> { 0 };
        }
    }
}
