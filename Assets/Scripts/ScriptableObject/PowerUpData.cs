using UnityEngine;

[CreateAssetMenu(fileName = "New PowerUp Data", menuName = "EndlessRunner/PowerUp Data")]
public class PowerUpData : ScriptableObject
{
    [Header("General Settings")]
    public string powerUpName; // e.g., "Magnet", "Invincibility", "x2 Score"
    public string poolTag;     // To link with your ObjectPooler
    
    [Header("Effect Parameters")]
    public float effectDuration = 5f; // How long the power-up lasts in seconds
    
    // Optional: Specific variables for different power-ups
    public float scoreMultiplierValue = 2f; // Only used if this is a Multiplier power-up
    public float magnetRadius = 10f;        // Only used if this is a Magnet power-up
}