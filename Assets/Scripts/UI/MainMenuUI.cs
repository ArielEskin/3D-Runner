using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [SerializeField] private Image buttonModeImage;
    [SerializeField] private Image touchModeImage;
    
    // ==========================================================================================================================================================
    
    private void OnEnable() // Ensures the correct UI layout is showing based on the input mode
    {
        UpdateButtons();
    }
    
    private void Update() // Ensures the correct UI layout is showing based on the input mode
    {
        UpdateButtons();
    }
    
    public void UpdateButtons() // Turns the physical on-screen UI buttons on/off and dims the mode-selection icons
    {
        
        if (InputManager.instance == null) 
        {
            return; 
        }
        if (InputManager.instance.currentMode == InputMode.Buttons)
        {
            SetAlpha(buttonModeImage, 1f);
            SetAlpha(touchModeImage, 0.5f);
            
        }
        else
        {
            SetAlpha(buttonModeImage, 0.5f);
            SetAlpha(touchModeImage, 1f);
            
        }
    }
    
    private void SetAlpha(Image image, float alpha) // A helper method to quickly change an image's transparency
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
