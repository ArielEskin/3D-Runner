using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ==========reference=========
    [SerializeField] private PlayerMovement playerMovement;
    public static GameManager gameManager;
    [SerializeField] private DifficultyManager difficultyManager;
    [SerializeField] private DifficultyManager difficultyTierManager;

    [Header("=========GameManager Settings=========")]
    [field: SerializeField] public float timeSurvived { get; private set; }
    [field: SerializeField] public  float distanceTravelled { get; private set; }
    
    [field: SerializeField] public int DifficultyUpLevel { get; private set; }
    
    // =========DifficultyManager=========
    [SerializeField] private float nextDifficultyDistance = 300f;
    
    // =========PlayerManager=========
    public bool isDead { get; private set; } = false;
    
    void Start()
    {
        gameManager = this;
    }

    // Update is called once per frame
    private void Update()
    {
        TimeSurvived();
        DistancePlayed();
        LevelUp();
        
        // when is falling will stop the game need to stop the counting
    }

    private void TimeSurvived()
    {
        if (!PlayerIsDead())
        {
            timeSurvived += Time.deltaTime; // count +1 after every 1 second
        }
    }

    private void DistancePlayed()
    {
        if (!PlayerIsDead())
        {
            distanceTravelled += Time.deltaTime * playerMovement.moveSpeed; // The playerspeed is 5f so the distance will be 5 units/meter
        }
    }

    public bool PlayerIsDead() // if the player falls that's mean he dead and the game is pause.
    {
        if (isDead)
        {
            Time.timeScale = 0f;
            return true;
        }
        return false;
    }
    
    //================== Difficulty Changer methods ==================
    public void LevelUp()
    {
        if (distanceTravelled >= nextDifficultyDistance) // if the travelled distance is bigger the nextDifficultyDistance is (20)
        {
            Debug.Log("Level up!");
            difficultyManager.LevelUpDifficulty(); // change the difficulty 
            
            playerMovement.moveSpeed = difficultyManager.currentDifficulty.movementSpeed; // Changing the speed of the player each level 

            nextDifficultyDistance += 300; // next level up will be more 20 distance travelled 
        }
    }
    
}
