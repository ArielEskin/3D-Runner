using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyManager", menuName = "EndlessRunner/Difficulty Manager")]
public class DifficultyManager : ScriptableObject
{
    [Header("State")]
    public DifficultyData currentDifficulty;
    [Header("Progression Setup")]
    public List<DifficultyData> difficultyTiers; //  0=Easy, 1=Medium, 2=Hard
    public int currentTierIndex = 0;
    
    public void ResetDifficulty() // Forces the game back to the Easy tier when starting a new run
    {
        currentTierIndex = 0;

        if (difficultyTiers.Count > 0)
        {
            currentDifficulty = difficultyTiers[0];
        }
    }
    
    public void LevelUpDifficulty() // Advances the active difficulty profile to the next tier in the list when certain distance is met
    {
        Debug.Log("Trying To Level Up");
        if (currentTierIndex < difficultyTiers.Count - 1)
        {
            currentTierIndex++;
            currentDifficulty = difficultyTiers[currentTierIndex];
            Debug.Log("Difficulty increased to: " + currentDifficulty.name);
        }
        else
        {
            Debug.Log("No more difficulty tiers");
        }
    }
}