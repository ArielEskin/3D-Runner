using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("Optional Consent UI")]
    [SerializeField] private GameObject analyticsConsentPanel;

    private bool analyticsReady;

    private const string ConsentKey = "AnalyticsConsent";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
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

    public void AcceptAnalytics()
    {
        if (analyticsConsentPanel != null)
        {
            analyticsConsentPanel.SetActive(false);
        }

        InitializeAnalytics(true);
    }

    public void DeclineAnalytics()
    {
        if (analyticsConsentPanel != null)
        {
            analyticsConsentPanel.SetActive(false);
        }

        InitializeAnalytics(false);
    }

    private async void InitializeAnalytics(bool playerAccepted)
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

    public void TrackSessionStarted()
    {
        Record(new CustomEvent("session_started")
        {
            { "app_version", Application.version },
            { "platform", Application.platform.ToString() }
        });
    }

    public void TrackCoinsCollected(int amount, int runCoinTotal)
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
        string deathCause)
    {
        Record(new CustomEvent("run_ended")
        {
            { "distance", distance },
            { "seconds_survived", seconds },
            { "coins_collected", coins },
            { "death_cause", deathCause }
        });
    }

    public void TrackDailyRewardClaimed(int rewardAmount)
    {
        Record(new CustomEvent("daily_reward_claimed")
        {
            { "reward_amount", rewardAmount }
        });
    }

    public void TrackMilestoneClaimed(MilestoneData milestone)
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

    private void Record(CustomEvent customEvent)
    {
        if (!analyticsReady)
        {
            return;
        }

        AnalyticsService.Instance.RecordEvent(customEvent);
    }
}