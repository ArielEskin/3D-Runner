using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableItem
{
    public string poolTag; // "obstacle", "Coin"
    [Range(0f, 1f)] 
    public float spawnProbability; // 0.5 = 50% chance, 1.0 = 100% chance
}

[CreateAssetMenu(fileName = "New Difficulty", menuName = "EndlessRunner/Difficulty Level")]
public class DifficultyData : ScriptableObject
{
    [Header("Game Settings")]
    public float movementSpeed = 10f;
    
    [Header("Spawning Settings")]
    public float spawnRate = 1f; // How frequently a track tile tries to spawn things
    public int maxObstaclesPerTrack = 3;
    
    [Header("Allowed Items & Probabilities")]
    public List<SpawnableItem> allowedItems;
}