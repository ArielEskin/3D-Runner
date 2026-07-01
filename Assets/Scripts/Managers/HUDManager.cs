using System;
using System.Drawing;
using System.Collections;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

public class HUDManager : MonoBehaviour
{
    // ==========references================================================================================================================================================

   [Header("=========HUDManager Settings=========")]
   [SerializeField] private TextMeshProUGUI timeText;
   [SerializeField] private TextMeshProUGUI distanceText;
   [SerializeField] private TextMeshProUGUI MediumLevel;
   [SerializeField] private TextMeshProUGUI HardLevel;
   [SerializeField] private TextMeshProUGUI CoinsAmountText;
   [SerializeField] private TextMeshProUGUI DeadText;
   private int lastTierIndex = -1;

   private GameManager gameManager;
   [SerializeField] private DifficultyManager difficultyManager;
   public static HUDManager instance;
   
   // ==========================================================================================================================================================

   private void Awake() // Initializes the HUD singleton and caches the GameManager reference
   {
       instance = this;
   }

   void Start() // Initializes the HUD singleton and caches the GameManager reference
    {
        gameManager = GameManager.gameManager;
    }

    private void Update() // Refreshes all text elements every frame.
    {
        UpdateTime();
        UpdateDistance();
        UpdateLevelText();
        UpdateDeadText();
        
    }

    private void UpdateLevelText() // Detects a difficulty change and triggers the corresponding on-screen announcement
    {
        if (difficultyManager.currentTierIndex == lastTierIndex)
            return;
        
        lastTierIndex = difficultyManager.currentTierIndex;
        
        MediumLevel.gameObject.SetActive(false);
        HardLevel.gameObject.SetActive(false);

        if (difficultyManager.currentTierIndex == 1)
        {
            StartCoroutine(ShowTextFor2SecondsMediumLevel(MediumLevel));
            MediumLevel.color = Color.red;
        }
        else if (difficultyManager.currentTierIndex >= 2)
        {
            StartCoroutine(ShowTextFor2SecondsHardLevel(HardLevel));
            HardLevel.color = Color.red;
        }
    }

    private IEnumerator ShowTextFor2SecondsMediumLevel(TextMeshProUGUI MediumLevel) // Coroutines that flash the "Medium" text on screen temporarily
    {
        MediumLevel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        MediumLevel.gameObject.SetActive(false);
    }
    
    private IEnumerator ShowTextFor2SecondsHardLevel(TextMeshProUGUI HardLevel) // Coroutines that flash the "Hard" text on screen temporarily
    {
        HardLevel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        HardLevel.gameObject.SetActive(false);
    }

    private void UpdateTime() // Formats the raw backend numbers into clean strings for the UI text [Time]
    {
        timeText.text = $"{gameManager.timeSurvived:F1}";
    }

    private void UpdateDistance() // Formats the raw backend numbers into clean strings for the UI text [Distance]
    {
        distanceText.text = $"{Mathf.RoundToInt(gameManager.distanceTravelled)}";
    }

    public void UpdateCoinsText(int amount) // Formats the raw backend numbers into clean strings for the UI text [Coins]
    {
        CoinsAmountText.text = amount.ToString();
        CoinsAmountText.fontSize = 10;
    }

    public void UpdateDeadText() // Reveals the game-over text overlay when the player dies
    {
        if (gameManager.isDead)
        {
            DeadText.gameObject.SetActive(true);
        }
    }
}
