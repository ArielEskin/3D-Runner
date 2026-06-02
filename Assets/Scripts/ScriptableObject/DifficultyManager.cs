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
    
    public void ResetDifficulty() 
    {
        currentTierIndex = 0;

        if (difficultyTiers.Count > 0)
        {
            currentDifficulty = difficultyTiers[0];
        }
    }
    
    public void LevelUpDifficulty() // Call this when the player reaches a certain distance/score
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