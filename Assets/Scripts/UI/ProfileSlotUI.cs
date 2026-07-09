using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSlotUI : MonoBehaviour
{
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TMP_Text profileNameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button selectButton;

    private PlayerProfileData profileData;

    private void Awake()
    {
        AutoFindReferences();
    }

    public void Setup(PlayerProfileData profile, System.Action<PlayerProfileData> onSelected)
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

        if (selectButton == null) return;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelected?.Invoke(profileData));
    }

    private void AutoFindReferences()
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
    }

    private void LoadThumbnail(string path)
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

    private string FormatDate(string dateText)
    {
        if (string.IsNullOrEmpty(dateText)) return "Not played";
        return dateText.Length >= 10 ? dateText.Substring(0, 10) : dateText;
    }
}
