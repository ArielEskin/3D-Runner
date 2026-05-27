using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ==========reference=========
    [SerializeField] private PlayerMovement playerMovement;
    public static GameManager gameManager;

    [Header("=========GameManager Settings=========")]
    [field: SerializeField] public float timeSurvived { get; private set; }
    [field: SerializeField] public  float distanceTravelled { get; private set; }
    
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

    public bool PlayerIsDead()
    {
        if (isDead)
        {
            Time.timeScale = 0f;
            return true;
        }
        return false;
    }
    
}
