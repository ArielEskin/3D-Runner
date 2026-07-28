using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    public static AnalyticsManager Instance { get; private set; }
    
    [SerializeField] private GameObject analyticsConsentPanel;
    
    private bool analyticsReady;
    private const string ConsentKey = "AnalyticsConsent";
    
    // ==========================================================================================================================================================
    
    private void Awake() // Enforces the singleton pattern and persists the manager across scene loads
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start() // Checks for saved analytics consent and prompts the player if no choice is saved
    {
        if (PlayerPrefs.HasKey(ConsentKey))
        {
            bool playerAccepted =
                PlayerPrefs.GetInt(ConsentKey) == 1;

            InitializeAnalytics(playerAccepted);
        }
        else if (analyticsConsentPanel != null)
        {
            analyticsConsentPanel.SetActive(true);
        }
    }

    public void AcceptAnalytics() // Closes the consent panel and starts data collection
    {
        if (analyticsConsentPanel != null)
        {
            analyticsConsentPanel.SetActive(false);
        }

        InitializeAnalytics(true);
    }

    public void DeclineAnalytics() // Closes the consent panel and disables data collection
    {
        if (analyticsConsentPanel != null)
        {
            analyticsConsentPanel.SetActive(false);
        }

        InitializeAnalytics(false);
    }

    private async void InitializeAnalytics(bool playerAccepted) // Initializes the Unity Services SDK and starts tracking if permitted
    {
        PlayerPrefs.SetInt(ConsentKey, playerAccepted ? 1 : 0);
        PlayerPrefs.Save();

        try
        {
            await UnityServices.InitializeAsync();

            if (!playerAccepted)
            {
                analyticsReady = false;
                return;
            }

            AnalyticsService.Instance.StartDataCollection();

            analyticsReady = true;

            TrackSessionStarted();

            Debug.Log("Unity Analytics is ready.");
        }
        catch (System.Exception exception)
        {
            Debug.LogError(
                "Unity Analytics could not start: " +
                exception.Message
            );
        }
    }

    public void TrackSessionStarted() // Sends an event recording the start of a new gameplay session
    {
        Record(new CustomEvent("session_started")
        {
            { "app_version", Application.version },
            { "platform", Application.platform.ToString() }
        });
    }

    public void TrackCoinsCollected(int amount, int runCoinTotal) // Sends an event recording coins picked up during gameplay
    {
        Record(new CustomEvent("coins_collected")
        {
            { "amount", amount },
            { "run_coin_total", runCoinTotal }
        });
    }

    public void TrackRunEnded(
        float distance,
        float seconds,
        int coins,
        string deathCause) // Sends a detailed event summarizing the entire run when the player dies
    {
        Record(new CustomEvent("run_ended")
        {
            { "distance", distance },
            { "seconds_survived", seconds },
            { "coins_collected", coins },
            { "death_cause", deathCause }
        });
    }

    public void TrackDailyRewardClaimed(int rewardAmount) // Sends an event recording that the daily reward was collected
    {
        Record(new CustomEvent("daily_reward_claimed")
        {
            { "reward_amount", rewardAmount }
        });
    }

    public void TrackMilestoneClaimed(MilestoneData milestone) // Sends an event recording that a milestone was completed and claimed
    {
        Record(new CustomEvent("milestone_claimed")
        {
            { "milestone_id", milestone.id },
            { "reward_coins", milestone.rewardCoins },
            {
                "permanent_coin_bonus_percent",
                milestone.permanentCoinBonusPercent
            }
        });
    }

    private void Record(CustomEvent customEvent) // A helper function that safely fires the event to Unity Analytics
    {
        if (!analyticsReady)
        {
            return;
        }

        AnalyticsService.Instance.RecordEvent(customEvent);
    }
}