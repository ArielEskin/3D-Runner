using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InputMode
{
    Buttons,
    Touch
}
public class InputManager : MonoBehaviour
{
    // ==========references================================================================================================================================================

    public static InputManager instance;
    public InputMode currentMode = InputMode.Touch;
    
    // ==========================================================================================================================================================

    public void Awake() // Initializes the global Singleton for input state tracking.
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetButtonMode() // Changes the active control scheme to buttons and tells the UI to update its visual layout
    {
        Debug.Log("Button Mode Pressed");
        currentMode = InputMode.Buttons;
        if (ProfileManager.instance != null && ProfileManager.instance.activeProfile != null)
        {
            ProfileManager.instance.activeProfile.inputModeIndex = 0;
            ProfileManager.instance.SaveActiveProfileJSON();
        }
    }
    
    public void SetTouchMode() // Changes the active control scheme to swipe and tells the UI to update its visual layout
    {
        Debug.Log("Touch Mode Pressed");
        currentMode = InputMode.Touch;
        if (ProfileManager.instance != null && ProfileManager.instance.activeProfile != null)
        {
            ProfileManager.instance.activeProfile.inputModeIndex = 1;
            ProfileManager.instance.SaveActiveProfileJSON();
        }
    }
}

