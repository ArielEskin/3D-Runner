using TMPro;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
   [Header("=========HUDManager Settings=========")]
   [SerializeField] private TextMeshProUGUI timeText;
   [SerializeField] private TextMeshProUGUI distanceText;
   
   private GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.gameManager;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateTime();
        UpdateDistance();
    }

    private void UpdateTime()
    {
        timeText.text = $"TIME\n{gameManager.timeSurvived:F1}";
    }

    private void UpdateDistance()
    {
        distanceText.text = $"DISTANCE\n{Mathf.RoundToInt(gameManager.distanceTravelled)} meters";
    }
}
