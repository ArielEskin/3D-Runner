using UnityEngine;

public class PlayerInvincibilityVisuals : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [Header("Visual Settings")]
    public float colorCycleSpeed = 5f; 
    private Renderer[] allRenderers; // hold all the different body parts and their original colors
    private Color[] originalColors;
    private bool isFlashing = false; 
    
    // ==========================================================================================================================================================
    
    private void Awake() // Finds every 3D mesh on the character model and saves their original default colors
    {
        allRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[allRenderers.Length];
        
        for (int i = 0; i < allRenderers.Length; i++)
        {
            originalColors[i] = allRenderers[i].material.color;
        }
    }

    private void Update() // Flashes the materials through the rainbow if invincible, and snaps them back to their original colors the moment the buff ends
    {
        if (allRenderers.Length == 0) return;

        if (GameManager.gameManager.isInvincible)
        {
            isFlashing = true;
            
            float changingHue = Mathf.PingPong(Time.time * colorCycleSpeed, 1f);
            Color flashColor = Color.HSVToRGB(changingHue, 1f, 1f);
            
            foreach (Renderer rend in allRenderers)
            {
                rend.material.color = flashColor;
            }
        }
        else if (isFlashing)
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                allRenderers[i].material.color = originalColors[i];
            }
            isFlashing = false;
        }
    }
}