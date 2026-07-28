using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class DailyRewardManager : MonoBehaviour
{
    // ==========references================================================================================================================================================
    public static DailyRewardManager Instance { get; private set; }

    [SerializeField] private int dailyRewardCoins = 100;
    [SerializeField] private float notificationDelayHours = 24f;
    public event Action RewardStateChanged;

    // ==========================================================================================================================================================
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

    public int DailyRewardCoins
    {
        get { return dailyRewardCoins; }
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
    }

    private void Start() // Requests notification permissions on Android and updates the initial reward state
    {
#if UNITY_ANDROID
        StartCoroutine(RequestAndroidNotificationPermission());
#endif
        

        RefreshRewardState();
    }

    private void OnApplicationFocus(bool hasFocus) // Refreshes the reward state when returning to the game from the background
    {
        if (hasFocus)
        {
            RefreshRewardState();
        }
    }

    private void OnApplicationPause(bool paused) // Schedules the push notification for the next reward when minimizing the game
    {
        if (paused)
        {
            ScheduleDailyRewardNotification();
        }
    }

    public bool IsRewardAvailable() // Checks if 24 hours have passed since the last claimed reward
    {
        PlayerProfileData profile = CurrentProfile;

        if (profile == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(profile.lastDailyRewardUtc))
        {
            return true;
        }

        DateTime lastClaimTime;

        bool dateWasRead = DateTime.TryParse(
            profile.lastDailyRewardUtc,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out lastClaimTime
        );

        if (!dateWasRead)
        {
            return true;
        }

        return DateTime.UtcNow >= lastClaimTime.ToUniversalTime().AddHours(24);
    }

    public void ClaimDailyReward() // Awards coins to the player and records the claim time in the active profile
    {
        if (!IsRewardAvailable())
        {
            Debug.Log("Daily reward is not available yet.");
            return;
        }

        PlayerProfileData profile = CurrentProfile;

        profile.totalCoins += dailyRewardCoins;
        profile.totalDailyRewardsCollected++;

        profile.lastDailyRewardUtc = DateTime.UtcNow.ToString(
            "O",
            CultureInfo.InvariantCulture
        );

        ProfileManager.instance.SaveActiveProfileJSON();

        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.CheckProgress();
        }

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.TrackDailyRewardClaimed(dailyRewardCoins);
        }

        ScheduleDailyRewardNotification();
        RefreshRewardState();

        Debug.Log("Daily reward claimed: " + dailyRewardCoins + " coins.");
    }

    public void RefreshRewardState() // Triggers an event to notify the UI to update its buttons and text
    {
        RewardStateChanged?.Invoke();
    }

    public void ScheduleDailyRewardNotification() // Cancels old notifications and sets a new one to fire in 24 hours
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        AndroidNotificationChannel channel = new AndroidNotificationChannel()
        {
            Id = "daily_rewards",
            Name = "Daily Rewards",
            Importance = Importance.Default,
            Description = "Notifications when a daily reward is ready."
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        AndroidNotification notification = new AndroidNotification()
        {
            Title = "Your reward is ready!",
            Text = "Come back to RailRush and collect your daily coins.",
            FireTime = DateTime.Now.AddHours(notificationDelayHours),
            IntentData = "daily_reward"
        };

        AndroidNotificationCenter.SendNotification(
            notification,
            "daily_rewards"
        );
#endif
        
    }

#if UNITY_ANDROID
    private IEnumerator RequestAndroidNotificationPermission() // Asks the user for permission to send push notifications on Android 13+
    {
        PermissionRequest request = new PermissionRequest();

        while (request.Status == PermissionStatus.RequestPending)
        {
            yield return null;
        }

        Debug.Log("Android notification permission: " + request.Status);
    }
#endif


}