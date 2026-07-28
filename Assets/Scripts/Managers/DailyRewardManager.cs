using System;
using System.Collections;
using System.Globalization;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    [SerializeField] private int dailyRewardCoins = 100;

    [Tooltip("Use 24 for the final version. Use 0.02 while testing (about 1 minute).")]
    [SerializeField] private float notificationDelayHours = 24f;

    public event Action RewardStateChanged;

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
#if UNITY_ANDROID
        StartCoroutine(RequestAndroidNotificationPermission());
#endif
        

        RefreshRewardState();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            RefreshRewardState();
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            ScheduleDailyRewardNotification();
        }
    }

    public bool IsRewardAvailable()
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

    public void ClaimDailyReward()
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

    public void RefreshRewardState()
    {
        RewardStateChanged?.Invoke();
    }

    public void ScheduleDailyRewardNotification()
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
    private IEnumerator RequestAndroidNotificationPermission()
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