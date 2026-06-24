using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    // ========== Reference ===========
    [Header ("UI Elements")]
    [SerializeField] private GameObject ButtonInputSystemImage;
    
    
    // ===== coin =====
    [SerializeField] private GameObject DoubleCoinAmountImage;
    
    private void Start()
    {
        DoubleCoinAmountImage.SetActive(false); // x2 is disabled when the game start
        // Check the choice made in the main menu as soon as the level loads
        if (InputManager.instance != null)
        {
            if (InputManager.instance.currentMode == InputMode.Buttons)
            {
                ButtonInputSystemImage.SetActive(true);  // Show buttons
            }
            else
            {
                ButtonInputSystemImage.SetActive(false); // Hide buttons for swipe mode
            }
        }
    }

    private void Update()
    {
        DoubleCoinBuff();
    }

    private void DoubleCoinBuff()
    {
        if (GameManager.gameManager.coinMultiplier > 1)
        {
            DoubleCoinAmountImage.SetActive(true); // set the image active true when the double x2 buff 
        }
        else
        {
            DoubleCoinAmountImage.SetActive(false);
        }
    }
    
}
