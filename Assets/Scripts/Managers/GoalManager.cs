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
        public CanvasGroup canvasGroup;
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

    private void Awake() // Enforces the singleton pattern and persists the manager across scene loads
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

    private void Start() // Binds the achievements UI and refreshes its visual state on startup
    {
        BindAchievementsUI();
        RefreshAchievementsUI();
    }

    private void OnDestroy() // Cleans up scene load subscriptions to prevent memory leaks
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Rebinds the UI automatically whenever the Main Menu scene is loaded
    {
        if (scene.name == "MainMenu")
        {
            BindAchievementsUI();
            RefreshAchievementsUI();
        }
    }

    public IReadOnlyList<MilestoneData> GetMilestones() // Returns the full list of defined milestones
    {
        return milestones;
    }

    public float GetProgress(MilestoneData milestone) // Calculates the current progress value based on the specified milestone metric
    {
        PlayerProfileData profile = CurrentProfile;

        if (profile == null || milestone == null)
        {
            return 0f;
        }

        switch (milestone.metric)
        {
            case MilestoneMetric.LifetimeCoins:
                return Mathf.Max(
                    profile.lifetimeCoinsCollected,
                    profile.totalCoins
                );

            case MilestoneMetric.LifetimeDistance:
                return profile.lifetimeDistanceTravelled;

            case MilestoneMetric.LifetimeSeconds:
                return profile.lifetimeSecondsSurvived;

            case MilestoneMetric.TotalDailyRewards:
                return profile.totalDailyRewardsCollected;
            
            case MilestoneMetric.HighestSingleRunDistance:
                return profile.highestDistance;
            
            case MilestoneMetric.HighestSingleRunCoins:
                return profile.highestCoinsInOneRun;
            
            case MilestoneMetric.SkinsBought:
                // Minus 1 because the player starts with 1 default skin unlocked
                return profile.unlockedSkinIndices.Count - 1; 
            
            case MilestoneMetric.ThemesBought:
                // Minus 1 because the player starts with 1 default theme unlocked
                return profile.unlockedThemes.Count - 1; 
            
            default:
                return 0f;
        }
    }

    public bool IsComplete(MilestoneData milestone) // Checks if the progress has reached or exceeded the milestone's target value
    {
        return GetProgress(milestone) >= milestone.targetValue;
    }

    public bool CanClaim(MilestoneData milestone) // Determines if the milestone is complete and the reward has not been claimed yet
    {
        PlayerProfileData.MilestoneProgressData record =
            GetRecord(milestone);

        return IsComplete(milestone) &&
               record != null &&
               !record.rewardClaimed;
    }

    public bool HasClaimed(MilestoneData milestone) // Checks if the milestone reward was already collected
    {
        PlayerProfileData.MilestoneProgressData record =
            GetRecord(milestone);

        return record != null && record.rewardClaimed;
    }

    public void RecordRun(
        int coinsCollectedThisRun,
        float distanceThisRun,
        float secondsThisRun) // Adds the run's distance and time to the lifetime stats and saves the profile
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

    public void RecordCoins(int collectedCoins) // Adds collected coins to the lifetime total and saves the profile
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

    public void CheckProgress() // Fires the progress changed event to notify UI listeners to update
    {
        ProgressChanged?.Invoke();
        RefreshAchievementsUI();
    }

    public void ClaimReward(MilestoneData milestone) // Awards the milestone bonus to the player and marks it as claimed in the save file
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
    public void OpenAchievements() // Makes the achievements panel visible and refreshes UI data
    {
        BindAchievementsUI();

        if (achievementsPanel != null)
        {
            achievementsPanel.SetActive(true);
            RefreshAchievementsUI();
        }
    }

    public void CloseAchievements() // Hides the achievements panel from the screen
    {
        if (achievementsPanel != null)
        {
            achievementsPanel.SetActive(false);
        }
    }

    private void BindAchievementsUI() // Locates and links all the progress texts and claim buttons inside the achievements panel
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
                progressText = FindProgressText(milestone),
                canvasGroup = claimButton.transform.parent != null 
                    ? claimButton.transform.parent.GetComponent<CanvasGroup>() 
                    : null
            };

            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(
                () => ClaimReward(milestone)
            );

            goalCards.Add(card);
        }
    }

    private void RefreshAchievementsUI() // Updates the text labels, button states, and opacity for every milestone card
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
                card.claimButton.gameObject.SetActive(!claimed);
            }

            if (card.claimButtonText != null)
            {
                card.claimButtonText.text = claimed
                    ? "CLAIMED"
                    : canClaim ? "CLAIM" : "LOCKED";
            }
            
            if (card.canvasGroup != null)
            {
                card.canvasGroup.alpha = claimed ? 0.6f : 1f;
            }
        }
    }

    private TMP_Text FindProgressText(MilestoneData milestone) // Searches the UI hierarchy for the specific text object matching the milestone's target string
    {
        if (achievementsPanel == null || milestone == null)
        {
            return null;
        }
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

    private string FormatProgress(MilestoneData milestone, float progress) // Formats the numeric progress and reward details into a readable string
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

    private string GetUnit(MilestoneData milestone) // Determines the correct text unit (coins, meters ect...) for the metric
    {
        switch (milestone.metric)
        {
            case MilestoneMetric.LifetimeCoins:
                return "coins";

            case MilestoneMetric.LifetimeDistance:
                return "meters";
            
            case MilestoneMetric.TotalDailyRewards:
                return "daily rewards";
            
            case MilestoneMetric.HighestSingleRunDistance:
                return "meters";
            
            case MilestoneMetric.HighestSingleRunCoins:
                return "coins";
            
            case MilestoneMetric.SkinsBought:
                return "skins"; 
            
            case MilestoneMetric.ThemesBought:
                return "themes"; 

            default:
                return "seconds";
        }
    }

    private int CompareButtonsFromTopToBottom(Button first, Button second) // Sorts UI buttons vertically by comparing their screen positions
    {
        RectTransform firstTransform =
            first.GetComponent<RectTransform>();
        RectTransform secondTransform =
            second.GetComponent<RectTransform>();

        float firstY = firstTransform == null
            ? 0f
            : firstTransform.position.y;
        float secondY = secondTransform == null
            ? 0f
            : secondTransform.position.y;

        return secondY.CompareTo(firstY);
    }

    private GameObject FindSceneObject(string objectName) // Locates a hidden UI object in the active scene by its name
    {
        foreach (Transform item in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (item.name == objectName && item.gameObject.scene == SceneManager.GetActiveScene())
            {
                return item.gameObject;
            }
        }

        return null;
    }

    private PlayerProfileData.MilestoneProgressData GetRecord(MilestoneData milestone) // Retrieves or creates the save record for a specific milestone
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
