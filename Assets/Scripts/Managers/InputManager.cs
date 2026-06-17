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
    public static InputManager instance;
    public InputMode currentMode = InputMode.Touch;
    [SerializeField] private GameplayUI gameplayUI;

    public void Awake()
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

    public void SetButtonMode()
    {
        currentMode = InputMode.Buttons;
        gameplayUI.UpdateButtons();
        
    }
    public void SetTouchMode()
    {
        currentMode = InputMode.Touch;
        gameplayUI.UpdateButtons();
    }
}

