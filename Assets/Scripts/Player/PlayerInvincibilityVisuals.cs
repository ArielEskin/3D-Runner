using UnityEngine;

public class PlayerInvincibilityVisuals : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("How fast the colors cycle")]
    public float colorCycleSpeed = 5f; 

    // Arrays to hold all the different body parts and their specific original colors
    private Renderer[] allRenderers;
    private Color[] originalColors;
    
    private bool isFlashing = false; 

    private void Awake()
    {
        // Grab EVERY piece of the character (Body, Shirt, Pants, Sneakers)
        allRenderers = GetComponentsInChildren<Renderer>();
        
        // Setup our color array to match the amount of body parts we found
        originalColors = new Color[allRenderers.Length];

        // Save the original color of each specific part so the shirt stays the shirt color later
        for (int i = 0; i < allRenderers.Length; i++)
        {
            originalColors[i] = allRenderers[i].material.color;
        }
    }

    private void Update()
    {
        if (allRenderers.Length == 0) return;

        if (GameManager.gameManager.isInvincible)
        {
            isFlashing = true;
            
            float changingHue = Mathf.PingPong(Time.time * colorCycleSpeed, 1f);
            Color flashColor = Color.HSVToRGB(changingHue, 1f, 1f);

            // Apply the rainbow flash to every single body part
            foreach (Renderer rend in allRenderers)
            {
                rend.material.color = flashColor;
            }
        }
        else if (isFlashing)
        {
            // The power-up just ended. Loop through and assign each part its specific original color back!
            for (int i = 0; i < allRenderers.Length; i++)
            {
                allRenderers[i].material.color = originalColors[i];
            }
            isFlashing = false;
        }
    }
}