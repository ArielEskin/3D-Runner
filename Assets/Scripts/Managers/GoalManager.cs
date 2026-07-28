using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance { get; private set; }

    [SerializeField] private List<MilestoneData> milestones =
        new List<MilestoneData>();

    public event Action ProgressChanged;

    private class GoalCardUI
    {
        public MilestoneData milestone;
        public TMP_Text progressText;
        public Button claimButton;
        public TMP_Text claimButtonText;
    }

    private GameObject achievementsPanel;
    private readonly List<GoalCardUI> goalCards =
        new List<GoalCardUI>();

    private PlayerProfileData CurrentProfile
    {
        get
        {
            if (ProfileManager.instance == null)
            {
                return null;
            }

            return ProfileManager.instance.activeProfile;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        BindAchievementsUI();
        RefreshAchievementsUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            BindAchievementsUI();
            RefreshAchievementsUI();
        }
    }

    public IReadOnlyList<MilestoneData> GetMilestones()
    {
        return milestones;
    }

    public float GetProgress(MilestoneData milestone)
    {
        PlayerProfileData profile = CurrentProfile;

        if (profile == null || milestone == null)
        {
            return 0f;
        }

        switch (milestone.metric)
        {
            case MilestoneMetric.LifetimeCoins:
                // Old profiles stored their earned coins in totalCoins before
                // Task 4 existed. Use the larger value so old progress shows
                // correctly and new live coin pickups still keep counting.
                return Mathf.Max(
                    profile.lifetimeCoinsCollected,
                    profile.totalCoins
                );

            case MilestoneMetric.LifetimeDistance:
                return profile.lifetimeDistanceTravelled;

            case MilestoneMetric.LifetimeSeconds:
                return profile.lifetimeSecondsSurvived;

            default:
                return 0f;
        }
    }

    public bool IsComplete(MilestoneData milestone)
    {
        return GetProgress(milestone) >= milestone.targetValue;
    }

    public bool CanClaim(MilestoneData milestone)
    {
        PlayerProfileData.MilestoneProgressData record =
            GetRecord(milestone);

        return IsComplete(milestone) &&
               record != null &&
               !record.rewardClaimed;
    }

    public bool HasClaimed(MilestoneData milestone)
    {
        PlayerProfileData.MilestoneProgressData record =
            GetRecord(milestone);

        return record != null && record.rewardClaimed;
    }

    public void RecordRun(
        int coinsCollectedThisRun,
        float distanceThisRun,
        float secondsThisRun)
    {
        PlayerProfileData profile = CurrentProfile;

        if (profile == null)
        {
            return;
        }

        // Coins are recorded when picked up in GameManager.AddCoin().
        // Do not add them again here, or each run would count them twice.
        profile.lifetimeDistanceTravelled += distanceThisRun;
        profile.lifetimeSecondsSurvived += secondsThisRun;

        ProfileManager.instance.SaveActiveProfileJSON();
        ProgressChanged?.Invoke();
        RefreshAchievementsUI();
    }

    public void RecordCoins(int collectedCoins)
    {
        PlayerProfileData profile = CurrentProfile;

        if (profile == null || collectedCoins <= 0)
        {
            return;
        }

        // Seed an old profile from its previously saved coin balance before
        // adding newly collected coins.
        profile.lifetimeCoinsCollected = Mathf.Max(
            profile.lifetimeCoinsCollected,
            profile.totalCoins
        );
        profile.lifetimeCoinsCollected += collectedCoins;

        ProfileManager.instance.SaveActiveProfileJSON();
        ProgressChanged?.Invoke();
        RefreshAchievementsUI();
    }

    public void ClaimReward(MilestoneData milestone)
    {
        if (!CanClaim(milestone))
        {
            Debug.Log("Milestone reward cannot be claimed yet.");
            return;
        }

        PlayerProfileData profile = CurrentProfile;

        PlayerProfileData.MilestoneProgressData record =
            GetRecord(milestone);

        profile.totalCoins += milestone.rewardCoins;
        profile.permanentCoinBonusPercent +=
            milestone.permanentCoinBonusPercent;

        record.rewardClaimed = true;

        ProfileManager.instance.SaveActiveProfileJSON();
        ProgressChanged?.Invoke();
        RefreshAchievementsUI();

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.TrackMilestoneClaimed(milestone);
        }

        MainMenu mainMenu = FindFirstObjectByType<MainMenu>();
        if (mainMenu != null)
        {
            mainMenu.UpdateCoinText();
        }

        Debug.Log("Claimed milestone reward: " + milestone.title);
    }

    // Connect your Achievements menu button to this method in Button On Click().
    public void OpenAchievements()
    {
        BindAchievementsUI();

        if (achievementsPanel != null)
        {
            achievementsPanel.SetActive(true);
            RefreshAchievementsUI();
        }
    }

    public void CloseAchievements()
    {
        if (achievementsPanel != null)
        {
            achievementsPanel.SetActive(false);
        }
    }

    private void BindAchievementsUI()
    {
        achievementsPanel = FindSceneObject("Achievements");
        goalCards.Clear();

        if (achievementsPanel == null)
        {
            return;
        }

        List<Button> claimButtons = new List<Button>();
        Button backButton = null;

        foreach (Button button in
                 achievementsPanel.GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name == "ClaimButton")
            {
                claimButtons.Add(button);
            }
            else if (button.gameObject.name == "Back")
            {
                backButton = button;
            }
        }

        // Your buttons are laid out top-to-bottom: coins, distance, survivor.
        claimButtons.Sort(CompareButtonsFromTopToBottom);

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(CloseAchievements);
            backButton.onClick.AddListener(CloseAchievements);
        }

        for (int index = 0;
             index < milestones.Count && index < claimButtons.Count;
             index++)
        {
            MilestoneData milestone = milestones[index];
            Button claimButton = claimButtons[index];

            GoalCardUI card = new GoalCardUI
            {
                milestone = milestone,
                claimButton = claimButton,
                claimButtonText =
                    claimButton.GetComponentInChildren<TMP_Text>(true),
                progressText = FindProgressText(milestone)
            };

            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(
                () => ClaimReward(milestone)
            );

            goalCards.Add(card);
        }
    }

    private void RefreshAchievementsUI()
    {
        foreach (GoalCardUI card in goalCards)
        {
            if (card.milestone == null)
            {
                continue;
            }

            float progress = GetProgress(card.milestone);
            bool claimed = HasClaimed(card.milestone);
            bool canClaim = CanClaim(card.milestone);

            if (card.progressText != null)
            {
                card.progressText.text =
                    FormatProgress(card.milestone, progress);
            }

            if (card.claimButton != null)
            {
                card.claimButton.interactable = canClaim;
            }

            if (card.claimButtonText != null)
            {
                card.claimButtonText.text = claimed
                    ? "CLAIMED"
                    : canClaim ? "CLAIM" : "LOCKED";
            }
        }
    }

    private TMP_Text FindProgressText(MilestoneData milestone)
    {
        if (achievementsPanel == null || milestone == null)
        {
            return null;
        }

        // Include the unit as well as the number. For example, "/ 500 coins"
        // must not accidentally match the Long Runner text "/ 5000 meters".
        string targetText = "/ " + milestone.targetValue.ToString("0") +
                            " " + GetUnit(milestone);

        foreach (TMP_Text text in
                 achievementsPanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.text.Contains(targetText))
            {
                return text;
            }
        }

        return null;
    }

    private string FormatProgress(MilestoneData milestone, float progress)
    {
        string unit = GetUnit(milestone);

        string reward = "Reward: " + milestone.rewardCoins + " coins";
        if (milestone.permanentCoinBonusPercent > 0)
        {
            reward += ", permanent +" +
                      milestone.permanentCoinBonusPercent + "% coins";
        }

        return Mathf.FloorToInt(progress) + " / " +
               milestone.targetValue.ToString("0") + " " + unit +
               "\n" + reward;
    }

    private string GetUnit(MilestoneData milestone)
    {
        switch (milestone.metric)
        {
            case MilestoneMetric.LifetimeCoins:
                return "coins";

            case MilestoneMetric.LifetimeDistance:
                return "meters";

            default:
                return "seconds";
        }
    }

    private int CompareButtonsFromTopToBottom(Button first, Button second)
    {
        RectTransform firstTransform =
            first.GetComponent<RectTransform>();
        RectTransform secondTransform =
            second.GetComponent<RectTransform>();

        float firstY = firstTransform == null
            ? 0f
            : firstTransform.anchoredPosition.y;
        float secondY = secondTransform == null
            ? 0f
            : secondTransform.anchoredPosition.y;

        return secondY.CompareTo(firstY);
    }

    private GameObject FindSceneObject(string objectName)
    {
        foreach (Transform item in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (item.name == objectName && item.gameObject.scene.IsValid())
            {
                return item.gameObject;
            }
        }

        return null;
    }

    private PlayerProfileData.MilestoneProgressData GetRecord(
        MilestoneData milestone)
    {
        PlayerProfileData profile = CurrentProfile;

        if (profile == null || milestone == null)
        {
            return null;
        }

        if (profile.milestones == null)
        {
            profile.milestones =
                new List<PlayerProfileData.MilestoneProgressData>();
        }

        foreach (PlayerProfileData.MilestoneProgressData record
                 in profile.milestones)
        {
            if (record.milestoneId == milestone.id)
            {
                return record;
            }
        }

        PlayerProfileData.MilestoneProgressData newRecord =
            new PlayerProfileData.MilestoneProgressData(milestone.id);

        profile.milestones.Add(newRecord);

        return newRecord;
    }
}
