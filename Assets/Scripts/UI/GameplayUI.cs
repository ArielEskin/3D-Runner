using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [Header ("UI Elements")]
    [SerializeField] private GameObject ButtonInputSystemImage;
    [SerializeField] private GameObject DoubleCoinAmountImage;
    
    // ==========references================================================================================================================================================
    
    private void Start() // Ensures the correct UI layout is showing based on the input mode
    {
        DoubleCoinAmountImage.SetActive(false);
        if (InputManager.instance != null)
        {
            if (InputManager.instance.currentMode == InputMode.Buttons)
            {
                ButtonInputSystemImage.SetActive(true);
            }
            else
            {
                ButtonInputSystemImage.SetActive(false);
            }
        }
    }

    private void Update() // Ensures the correct UI layout is showing based on the input mode
    {
        DoubleCoinBuff();
    }

    private void DoubleCoinBuff() // Displays or hides the "x2" icon next to the coin counter based on the active GameManager state
    {
        if (GameManager.gameManager.coinMultiplier > 1) { DoubleCoinAmountImage.SetActive(true); }
        else { DoubleCoinAmountImage.SetActive(false); }
    }
}
