using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class ShopDailyRewardUI : MonoBehaviour
{
    // ==========references================================================================================================================================================
    
    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button closeShopButton;

    [Header("Daily Reward")]
    [SerializeField] private Button rewardCoinButton;
    [SerializeField] private TMP_Text rewardAmountText;
    [SerializeField] private TMP_Text rewardStatusText;

    private bool openedFromDailyRewardNotification;
    
    // ==========================================================================================================================================================
    
    private void Start() // Initializes buttons and subscribes to reward state events
    {
        openShopButton.onClick.AddListener(OpenShop);
        closeShopButton.onClick.AddListener(CloseShop);
        rewardCoinButton.onClick.AddListener(ClaimDailyReward);

        if (DailyRewardManager.Instance != null)
        {
            DailyRewardManager.Instance.RewardStateChanged += Refresh;
        }

#if UNITY_ANDROID
        AndroidNotificationIntentData notificationIntent =
            AndroidNotificationCenter.GetLastNotificationIntent();

        openedFromDailyRewardNotification =
            notificationIntent != null &&
            notificationIntent.Notification.IntentData == "daily_reward";
#endif

        Refresh();
    }

    private void OnDestroy() // Cleans up event subscriptions to prevent memory leaks
    {
        if (DailyRewardManager.Instance != null)
        {
            DailyRewardManager.Instance.RewardStateChanged -= Refresh;
        }
    }

    public void OpenShop() // Makes the shop panel visible and refreshes UI data
    {
        shopPanel.SetActive(true);
        Refresh();
    }

    public void CloseShop() // Hides the shop panel from the screen
    {
        shopPanel.SetActive(false);
    }

    private void ClaimDailyReward() // Requests the DailyRewardManager to claim the current reward
    {
        if (DailyRewardManager.Instance == null)
        {
            return;
        }

        DailyRewardManager.Instance.ClaimDailyReward();
        Refresh();
    }

    public void Refresh() // Updates the visual state of the reward button and texts based on availability
    {
        if (DailyRewardManager.Instance == null)
        {
            return;
        }

        bool rewardIsReady =
            DailyRewardManager.Instance.IsRewardAvailable();

        rewardAmountText.text =
            DailyRewardManager.Instance.DailyRewardCoins +
            " FREE COINS";

        rewardCoinButton.interactable = rewardIsReady;

        if (rewardIsReady)
        {
            rewardStatusText.text = "Tap the coin to claim!";

            // Only show Shop automatically when its notification opened the app.
            if (openedFromDailyRewardNotification)
            {
                shopPanel.SetActive(true);
            }
        }
        else
        {
            rewardStatusText.text =
                "CLAIMED\nCome back in 24 hours";
        }
    }
}
