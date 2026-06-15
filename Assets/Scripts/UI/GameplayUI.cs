using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonControlsPanel;
    
    // Update is called once per frame
    void Update()
    {
        if (InputManager.instance.currentMode == InputMode.Buttons)
        {
            buttonControlsPanel.SetActive(true);
        }
        else
        {
            buttonControlsPanel.SetActive(false);
        }
    }
}
