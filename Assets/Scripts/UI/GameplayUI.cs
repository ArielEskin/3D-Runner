using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private GameObject ButtonInputSystemImage;

    
    
    private void Start()
    {
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
    
}
