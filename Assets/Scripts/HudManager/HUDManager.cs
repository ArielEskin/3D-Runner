using System.Drawing;
using System.Collections;
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
   private int lastTierIndex = -1;
   
   // ==========reference=========
   private GameManager gameManager;
   [SerializeField] private DifficultyManager difficultyManager;
   
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
        if (difficultyManager.currentTierIndex == lastTierIndex)
            return;
        
        lastTierIndex = difficultyManager.currentTierIndex;
        
        MediumLevel.gameObject.SetActive(false);
        HardLevel.gameObject.SetActive(false);

        if (difficultyManager.currentTierIndex == 1)
        {
            StartCoroutine(ShowTextFor2Seconds(MediumLevel));
            MediumLevel.color = Color.red;
        }
        else if (difficultyManager.currentTierIndex >= 2)
        {
            StartCoroutine(ShowTextFor2Seconds(HardLevel));
            HardLevel.color = Color.red;
        }
    }

    private IEnumerator ShowTextFor2Seconds(TextMeshProUGUI text)
    {
        // wait 2 seconds for the medium level text
        MediumLevel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        MediumLevel.gameObject.SetActive(false);
        
        // wait 2 seconds for the medium level text
        HardLevel.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        HardLevel.gameObject.SetActive(false);
        
    }

    private void UpdateTime()
    {
        // managing the time text 
        timeText.text = $"Time: {gameManager.timeSurvived:F1}";
        timeText.color = Color.black;
        timeText.fontSize = 60;
    }

    private void UpdateDistance()
    {
        //managing the distance text 
        distanceText.text = $"{Mathf.RoundToInt(gameManager.distanceTravelled)}/meters";
        distanceText.color = Color.black;
        distanceText.fontSize = 60;
    }
}
