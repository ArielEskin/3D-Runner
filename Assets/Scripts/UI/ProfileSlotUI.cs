using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSlotUI : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TMP_Text profileNameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;

    private PlayerProfileData profileData;
    
    // ==========================================================================================================================================================

    private void Awake() // Automatically locates internal UI references
    {
        AutoFindReferences();
    }

    public void Setup(
        PlayerProfileData profile,
        System.Action<PlayerProfileData> onSelected,
        System.Action<PlayerProfileData> onDeleted) // Populates the slot with profile data and wires up the buttons
    {
        profileData = profile;

        if (profileNameText != null)
        {
            profileNameText.text = profile.profileName;
        }

        if (statsText != null)
        {
            statsText.text = "Coins: " + profile.totalCoins +
                             "\nBest: " + Mathf.RoundToInt(profile.highestDistance) + "m" +
                             "\nLast: " + FormatDate(profile.lastPlayedDate);
        }

        LoadThumbnail(profile.screenshotPath);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(
                () => onSelected?.Invoke(profileData)
            );
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(
                () => onDeleted?.Invoke(profileData)
            );
        }
    }

    private void AutoFindReferences() // Automatically finds UI elements if they were not assigned in the Inspector
    {
        if (thumbnailImage == null)
        {
            Transform thumbnail = transform.Find("ThumbnailImage");
            if (thumbnail != null) thumbnailImage = thumbnail.GetComponent<Image>();
        }

        if (profileNameText == null)
        {
            Transform profileName = transform.Find("ProfileNameText");
            if (profileName != null) profileNameText = profileName.GetComponent<TMP_Text>();
        }

        if (statsText == null)
        {
            Transform stats = transform.Find("StatsText");
            if (stats != null) statsText = stats.GetComponent<TMP_Text>();
        }

        if (selectButton == null)
        {
            Transform select = transform.Find("SelectButton");
            if (select != null) selectButton = select.GetComponent<Button>();
        }

        if (deleteButton == null)
        {
            Transform delete = transform.Find("DeleteButton");
            if (delete != null) deleteButton = delete.GetComponent<Button>();
        }
    }

    private void LoadThumbnail(string path) // Loads the saved screenshot from disk and displays it
    {
        if (thumbnailImage == null) return;
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);

        thumbnailImage.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    private string FormatDate(string dateText) // Formats the last played timestamp into a short, readable date string
    {
        if (string.IsNullOrEmpty(dateText)) return "Not played";
        return dateText.Length >= 10 ? dateText.Substring(0, 10) : dateText;
    }
}
