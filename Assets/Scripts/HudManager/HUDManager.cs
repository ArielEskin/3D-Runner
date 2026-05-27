using System.Drawing;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

public class NewMonoBehaviourScript : MonoBehaviour
{
   [Header("=========HUDManager Settings=========")]
   [SerializeField] private TextMeshProUGUI timeText;
   [SerializeField] private TextMeshProUGUI distanceText;
   [SerializeField] private TextMeshProUGUI MediumLevel;
   [SerializeField] private TextMeshProUGUI HardLevel;
   
   // ==========reference=========
   private GameManager gameManager;
   private DifficultyManager difficultyManager;
   
    void Start()
    {
        gameManager = GameManager.gameManager;
        
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateTime();
        UpdateDistance();
        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        if (difficultyManager.currentTierIndex > 0 && difficultyManager.currentTierIndex < 2)
        {
            MediumLevel.gameObject.SetActive(true);
            MediumLevel.text = $"Medium Level!";
            timeText.color = Color.red;
            timeText.fontSize = 80;
            
        }

        if (difficultyManager.currentTierIndex > 1)
        {
            HardLevel.gameObject.SetActive(true);
            HardLevel.text = $"Hard Level!";
            timeText.color = Color.red;
            timeText.fontSize = 80;
        }
    }

    private void UpdateTime()
    {
        timeText.text = $"Time: {gameManager.timeSurvived:F1}";
        timeText.color = Color.black;
        timeText.fontSize = 80;
    }

    private void UpdateDistance()
    {
        distanceText.text = $"{Mathf.RoundToInt(gameManager.distanceTravelled)}/meters";
        distanceText.color = Color.black;
        distanceText.fontSize = 80;
    }
}
