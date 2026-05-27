using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyManager", menuName = "EndlessRunner/Difficulty Manager")]
public class DifficultyManager : ScriptableObject
{
    [Header("State")]
    public DifficultyData currentDifficulty;
    
    [Header("Progression Setup")]
    public List<DifficultyData> difficultyTiers; //  0=Easy, 1=Medium, 2=Hard
    private int currentTierIndex = 0;

    
    public void LevelUpDifficulty() // Call this when the player reaches a certain distance/score
    {
        if (currentTierIndex < difficultyTiers.Count - 1)
        {
            currentTierIndex++;
            currentDifficulty = difficultyTiers[currentTierIndex];
            Debug.Log("Difficulty increased to: " + currentDifficulty.name);
        }
    }
}