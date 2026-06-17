using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonControlsPanel;
    [SerializeField] private Image buttonModeImage;
    [SerializeField] private Image touchModeImage;
    
    
    // Update is called once per frame
    void Update()
    {
        if (InputManager.instance.currentMode == InputMode.Buttons)
        {
            buttonControlsPanel.SetActive(true);
            
            SetAlpha(buttonModeImage, 0.5f); // Active
            SetAlpha(touchModeImage, 1f);    // Inactive

        }
        else
        {
            buttonControlsPanel.SetActive(false);

            SetAlpha(buttonModeImage, 1f);   // Inactive
            SetAlpha(touchModeImage, 0.5f);  // Active
        }
        
    }
    
    private void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
    
}
