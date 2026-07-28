using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum DifficultyTier
{
    Easy,
    Medium,
    Hard
}

public class GameManager : MonoBehaviour
{ 
    // ==========references================================================================================================================================================
    
    [SerializeField] private PlayerMovement playerMovement;
    public static GameManager gameManager;
    [SerializeField] private DifficultyManager difficultyManager;
    public MainMenu mainMenu; 
    
    [Header("=========GameManager Settings=========")]
    [field: SerializeField] public float timeSurvived { get; private set; }
    [field: SerializeField] public  float distanceTravelled { get; private set; }
    [field: SerializeField] public int DifficultyUpLevel { get; private set; }
    
    [Header("=========Power-Up States=========")]
    public bool isInvincible = false;
    public bool isMagnetActive = false;
    public int coinMultiplier = 1;
    public float magnetRadius = 10f;
    // Timers
    private float invincibilityTimer = 0f;
    private float magnetTimer = 0f;
    private float multiplierTimer = 0f;
    
    // =========DifficultyManager=========
    [SerializeField] private float nextDifficultyDistance;
    public DifficultyTier currentTier = DifficultyTier.Easy;
    
    // =========PlayerManager=========
    public bool isDead;
    
    //=========Coins=========
    [field: SerializeField] public int Coins { get; private set; }
    
    //=========Button=========
    [SerializeField] private Button retryButton;
    [SerializeField] private Button backButton;
    private bool profileSavedThisRun;
    
    // ==========================================================================================================================================================

    private void Awake() // Sets the GameManager instance and resets the difficulty
    {
        gameManager = this;
        if (difficultyManager != null)
        {
            difficultyManager.ResetDifficulty();
        }

        if (ProfileManager.instance != null && ProfileManager.instance.activeProfile != null)
        {
            ProfileManager.instance.ApplyProfileToGameSession();
            Debug.Log("Selected theme for this run: " + ProfileManager.instance.activeProfile.selectedThemeID);
        }
    }
    
    void Start() // Hides the game over UI buttons at the start of a run
    {
        retryButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        
    }

    private void Update() // Tracks player survival time, distance traveled, active power-up durations, and checks for level-ups
    {
        TimeSurvived();
        DistancePlayed();
        LevelUp();
        HandlePowerUpTimers();
    }

    public void ActivatePowerUp(PowerUpData data) // Identifies which power-up was collected and triggers its specific effects and timers.
    {
        if (data.powerUpName == "Invincibility") 
        {
            isInvincible = true;
            invincibilityTimer = data.effectDuration; // Sets/Resets the clock
        }
        else if (data.powerUpName == "DoubleCoins") 
        {
            coinMultiplier = Mathf.RoundToInt(data.scoreMultiplierValue);
            multiplierTimer = data.effectDuration;
        }
        else if (data.powerUpName == "Magnet") 
        {
            isMagnetActive = true;
            magnetRadius = data.magnetRadius;
            magnetTimer = data.effectDuration;
        }
    }
    
    private void HandlePowerUpTimers() // Counts down the timers every frame
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0) isInvincible = false;
        }

        if (multiplierTimer > 0)
        {
            multiplierTimer -= Time.deltaTime;
            if (multiplierTimer <= 0) coinMultiplier = 1; // Back to normal
        }

        if (magnetTimer > 0)
        {
            magnetTimer -= Time.deltaTime;
            if (magnetTimer <= 0) isMagnetActive = false;
        }
    }
    public void AddCoin(int amount)
    {
        int permanentBonusPercent = 0;

        if (ProfileManager.instance != null &&
            ProfileManager.instance.activeProfile != null)
        {
            permanentBonusPercent =
                ProfileManager.instance.activeProfile.permanentCoinBonusPercent;
        }

        float permanentMultiplier = 1f + (permanentBonusPercent / 100f);

        int earnedCoins = Mathf.CeilToInt(
            amount * coinMultiplier * permanentMultiplier
        );

        Coins += earnedCoins;

        HUDManager.instance.UpdateCoinsText(Coins);

        // Record coin-goal progress at pickup time. This keeps the achievement
        // correct even if the player leaves a run before the normal run save.
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.RecordCoins(earnedCoins);
        }

        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.TrackCoinsCollected(earnedCoins, Coins);
        }
    }

    private void TimeSurvived() // Calculates the run's time survived based on time and movement speed
    {
        if (!isDead)
        {
            timeSurvived += Time.deltaTime;
        }
    }

    private void DistancePlayed() // Calculates the run's distance traveled based on time and movement speed
    {
        if (!isDead)
        {
            distanceTravelled += Time.deltaTime * playerMovement.moveSpeed;
        }
    }
    
    
    public void KillPlayer(string obstacleTag) // Stops the game loop, plays the specific death animation, triggers audio, and starts the game over sequences
    {
        if (isDead) return;
        
        isDead = true;
        SoundManager.instance.PlaySound3D("DeathSound", playerMovement.transform.position);
        Debug.Log("Player hit: " + obstacleTag);
        
        playerMovement.TriggerDeathAnimation(obstacleTag);

        SaveRunToActiveProfile();
        StartCoroutine(GameOverSequence());
        StartCoroutine(GameOverRetryButton());
    }

    private IEnumerator GameOverSequence() // Waits for the death animation to finish before showing the "Dead" UI text
    {
        yield return new WaitForSeconds(3f);
        HUDManager.instance.UpdateDeadText();
        Time.timeScale = 1f;
    }

    public void LevelUp() // Checks if the player has passed the current difficulty's distance threshold and triggers a tier upgrade if they have
    {
        if (difficultyManager.currentTierIndex < difficultyManager.difficultyTiers.Count - 1)
        {
            DifficultyData nextTierData = difficultyManager.difficultyTiers[difficultyManager.currentTierIndex + 1];

            if (distanceTravelled >= nextTierData.distanceToReach)
            {
                if (currentTier == DifficultyTier.Easy) currentTier = DifficultyTier.Medium;
                else if (currentTier == DifficultyTier.Medium) currentTier = DifficultyTier.Hard;

                Debug.Log(currentTier.ToString() + " reached!");
                
                difficultyManager.LevelUpDifficulty(); 
                playerMovement.moveSpeed = difficultyManager.currentDifficulty.movementSpeed; 
            }
        }
    }
    
    private IEnumerator GameOverRetryButton() // Delays the appearance of the retry/menu buttons and pauses the game time
    {
        yield return new WaitForSeconds(3f);
        retryButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RetryButton() // Restores the timescale and loads the game scene again
    {
        SaveRunToActiveProfile();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void BackButton() // Restores the timescale and loads the main menu scene
    {
        SaveRunToActiveProfile();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void SaveRunToActiveProfile()
    {
        if (profileSavedThisRun) return;
        if (ProfileManager.instance == null || ProfileManager.instance.activeProfile == null) return;

        PlayerProfileData profile = ProfileManager.instance.activeProfile;
        profile.totalCoins += Coins;
        profile.highestDistance = Mathf.Max(profile.highestDistance, distanceTravelled);
        profile.longestTimeSurvived = Mathf.Max(profile.longestTimeSurvived, timeSurvived);
        if (GoalManager.Instance != null)
        {
            GoalManager.Instance.RecordRun(
                Coins,
                distanceTravelled,
                timeSurvived
            );
        }
        profile.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        if (InputManager.instance != null)
        {
            profile.inputModeIndex = InputManager.instance.currentMode == InputMode.Buttons ? 0 : 1;
        }

        ProfileManager.instance.TriggerSaveSequence();
        profileSavedThisRun = true;
        Debug.Log("Saved run stats to profile: " + profile.profileName);
    }
    
}
