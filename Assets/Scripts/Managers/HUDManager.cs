using System;
using System.Drawing;
using System.Collections;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

public class HUDManager : MonoBehaviour
{
   [Header("=========HUDManager Settings=========")]
   [SerializeField] private TextMeshProUGUI timeText;
   [SerializeField] private TextMeshProUGUI distanceText;
   [SerializeField] private TextMeshProUGUI MediumLevel;
   [SerializeField] private TextMeshProUGUI HardLevel;
   [SerializeField] private TextMeshProUGUI CoinsAmountText;
   [SerializeField] private TextMeshProUGUI DeadText;
   private int lastTierIndex = -1;
   
   // ==========reference=========
   private GameManager gameManager;
   [SerializeField] private DifficultyManager difficultyManager;
   public static HUDManager instance;

   private void Awake()
   {
       instance = this;
   }

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
        UpdateDeadText();
        
    }

    private void UpdateLevelText()
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

    private IEnumerator ShowTextFor2SecondsMediumLevel(TextMeshProUGUI MediumLevel)
    {
        // wait 2 seconds for the medium level text
        MediumLevel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        MediumLevel.gameObject.SetActive(false);
        
    }
    
    private IEnumerator ShowTextFor2SecondsHardLevel(TextMeshProUGUI HardLevel)
    {
        // wait 2 seconds for the medium level text
        HardLevel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        HardLevel.gameObject.SetActive(false);
        
    }

    private void UpdateTime()
    {
        // managing the time text 
        timeText.text = $"Time:{gameManager.timeSurvived:F1}";
        timeText.color = Color.black;
        timeText.fontSize = 60;
    }

    private void UpdateDistance()
    {
        //managing the distance text 
        distanceText.text = $"{Mathf.RoundToInt(gameManager.distanceTravelled)}/M";
        distanceText.color = Color.black;
        distanceText.fontSize = 60;
    }
    
    //=============COINS===============
    public void UpdateCoinsText(int amount)
    {
        CoinsAmountText.text = amount.ToString();
        CoinsAmountText.color = Color.black;
        CoinsAmountText.fontSize = 40;
    }
    
    //=============Death===============
    public void UpdateDeadText()
    {
        if (gameManager.isDead)
        {
            DeadText.gameObject.SetActive(true);
        }
    }
}
