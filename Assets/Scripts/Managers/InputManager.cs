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
        Debug.Log("Button Mode Pressed");
        currentMode = InputMode.Buttons;
        
        
    }
    public void SetTouchMode()
    {
        Debug.Log("Touch Mode Pressed");
        currentMode = InputMode.Touch;
        
    }
}

