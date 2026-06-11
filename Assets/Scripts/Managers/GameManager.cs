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
    // ==========reference=========
    [SerializeField] private PlayerMovement playerMovement;
    public static GameManager gameManager;
    [SerializeField] private DifficultyManager difficultyManager;
    

    [Header("=========GameManager Settings=========")]
    [field: SerializeField] public float timeSurvived { get; private set; }
    [field: SerializeField] public  float distanceTravelled { get; private set; }
    
    [field: SerializeField] public int DifficultyUpLevel { get; private set; }
    
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

    private void Awake()
    {
        gameManager = this;
    }
    
    void Start()
    {
        retryButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        
    }

    // Update is called once per frame
    private void Update()
    {
        TimeSurvived();
        DistancePlayed();
        LevelUp();
        
        // when is falling will stop the game need to stop the counting
    }
    
    public void AddCoin(int amount)
    {
        Coins += amount;
        HUDManager.instance.UpdateCoinsText(Coins);
        
    }

    private void TimeSurvived()
    {
        if (!isDead)
        {
            timeSurvived += Time.deltaTime; // count +1 after every 1 second
        }
    }

    private void DistancePlayed()
    {
        if (!isDead)
        {
            distanceTravelled += Time.deltaTime * playerMovement.moveSpeed; // The playerspeed is 5f so the distance will be 5 units/meter
        }
    }
    
    
    public void KillPlayer(string obstacleTag)
    {
        if (isDead) return; // Prevent this from triggering twice if you hit two hitboxes at once
        
        isDead = true;
        Debug.Log("Player hit: " + obstacleTag);

        // Tell the player to play the specific death animation
        playerMovement.TriggerDeathAnimation(obstacleTag);

        // Start the timer to wait for the animation to finish
        StartCoroutine(GameOverSequence());
        StartCoroutine(GameOverRetryButton());
    }

    private IEnumerator GameOverSequence()
    {
        // Wait for 3 seconds so death animations finishes
        yield return new WaitForSeconds(3f);
        HUDManager.instance.UpdateDeadText();
        Time.timeScale = 0f;
    }
    
    //================== Difficulty Changer methods ==================
    public void LevelUp()
    {
        // Check if there is another difficulty tier available to upgrade to
        if (difficultyManager.currentTierIndex < difficultyManager.difficultyTiers.Count - 1)
        {
            // Get the data for the NEXT tier
            DifficultyData nextTierData = difficultyManager.difficultyTiers[difficultyManager.currentTierIndex + 1];

            // Check if distance travelled meets the requirement in the Scriptable Object
            if (distanceTravelled >= nextTierData.distanceToReach)
            {
                // Update our Enum (Easy -> Medium -> Hard)
                if (currentTier == DifficultyTier.Easy) currentTier = DifficultyTier.Medium;
                else if (currentTier == DifficultyTier.Medium) currentTier = DifficultyTier.Hard;

                Debug.Log(currentTier.ToString() + " reached!");

                // Tell the DifficultyManager to step up its index
                difficultyManager.LevelUpDifficulty(); 
                
                // Apply the new movement speed from the Scriptable Object
                playerMovement.moveSpeed = difficultyManager.currentDifficulty.movementSpeed; 
            }
        }
    }
    
    // =====================Buttons====================
    private IEnumerator GameOverRetryButton()
    {
        yield return new WaitForSeconds(3f); // wait 3 seconds
        retryButton.gameObject.SetActive(true); // active my retry button
        backButton.gameObject.SetActive(true); // active my back button
        Time.timeScale = 0f;
    }

    public void RetryButton()
    {
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
        
    }
    
    
}
