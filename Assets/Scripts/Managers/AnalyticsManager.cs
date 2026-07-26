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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject managerObj = new GameObject("AnalyticsManager");
            managerObj.AddComponent<AnalyticsManager>();
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
    }

    private void Start()
    {
        InitializeAnalytics();
    }

    private async void InitializeAnalytics()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            analyticsReady = true;
            TrackSessionStarted();
            Debug.Log("Unity Analytics is ready.");
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Unity Analytics could not start: " + exception.Message);
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
        string safeId = string.IsNullOrEmpty(milestone.id) ? milestone.name : milestone.id;

        Record(new CustomEvent("milestone_claimed")
        {
            { "milestone_id", safeId },
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
        AnalyticsService.Instance.Flush();
    }

}