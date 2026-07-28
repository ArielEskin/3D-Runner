using System;
using UnityEngine;

public class ScreenController : MonoBehaviour
{

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait; // prevent landscape mode on the mobile
        
        DontDestroyOnLoad(gameObject);
    }
}
