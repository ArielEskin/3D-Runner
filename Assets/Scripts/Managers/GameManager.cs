using UnityEngine;
using System.Collections;

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

    private void Awake()
    {
        gameManager = this;
    }
    
    void Start()
    {
        
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
        if (currentTier == DifficultyTier.Easy && distanceTravelled >= 150f) // if currentTier = easy && ditance bigger then 150
        {
            currentTier = DifficultyTier.Medium; // change the currentTier easy to medium

            Debug.Log("Medium reached!");

            difficultyManager.LevelUpDifficulty(); //the scriptablescript changing the level to medium
            playerMovement.moveSpeed = difficultyManager.currentDifficulty.movementSpeed; // match the playerSpeed to player like in the new level
        }
        else if (currentTier == DifficultyTier.Medium && distanceTravelled >= 400f) // if currentTier = medium && ditance bigger then 400
        {
            currentTier = DifficultyTier.Hard;// change the currentTier medium to hard

            Debug.Log("Hard reached!");

            difficultyManager.LevelUpDifficulty();//the scriptablescript changing the level to hard
            playerMovement.moveSpeed = difficultyManager.currentDifficulty.movementSpeed; // match the playerSpeed to player like in the new level
        }
    }
    
}
