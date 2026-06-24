using UnityEngine;
using UnityEngine.UI;

public class InputButtonsBinder : MonoBehaviour
{
    [SerializeField] private Button touchButton;
    [SerializeField] private Button buttonModeButton;

    private void Start()
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