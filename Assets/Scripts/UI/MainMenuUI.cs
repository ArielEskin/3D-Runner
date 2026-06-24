using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    // =========== reference =============
    [SerializeField] private Image buttonModeImage;
    [SerializeField] private Image touchModeImage;
    
    private void OnEnable()
    {
        UpdateButtons(); // IMPORTANT: refresh after scene load
    }
    
    private void Update()
    {
        UpdateButtons();
    }
    
    public void UpdateButtons()
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
    
    private void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
