using UnityEngine;
using UnityEngine.UI;

public class InputButtonsBinder : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [SerializeField] private Button touchButton;
    [SerializeField] private Button buttonModeButton;
    
    // ==========================================================================================================================================================

    private void Start() // Links the physical UI buttons to the InputManager, ensuring they trigger the correct control scheme swap (Touch vs. Buttons) when clicked.
    {
        touchButton.onClick.RemoveAllListeners();
        buttonModeButton.onClick.RemoveAllListeners();

        touchButton.onClick.AddListener(() =>
        {
            InputManager.instance.SetTouchMode();
        });

        buttonModeButton.onClick.AddListener(() =>
        {
            InputManager.instance.SetButtonMode();
        });
    }
}