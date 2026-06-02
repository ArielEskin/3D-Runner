using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // tells Unity to display this in the Inspector.
public class SpawnableItem // A helper class that groups together everything we need to know about a specific item before we spawn it. 
{
    public string poolTag; // "LowObstacle", "HighObstacle", "Coin"
    [Range(0f, 1f)] 
    public float spawnProbability; // Creates a slider in the Inspector from 0 to 1 - Represents the chance of the item spawning (0.5 = 50% chance, 1.0 = 100% chance)
    public float spawnHeight = 1f; // The Y-axis position where this specific item should spawn so it doesn't float or sink
}

// A blueprint for creating Difficulty Level assets (like "DifficultyEasy" or "DifficultyHard").
// [CreateAssetMenu] adds a button in Unity's right-click menu (Create -> EndlessRunner -> Difficulty Level)
[CreateAssetMenu(fileName = "New Difficulty", menuName = "EndlessRunner/Difficulty Level")]
public class DifficultyData : ScriptableObject
{
    [Header("Game Settings")]
    public float movementSpeed; // How fast the player runs forward when this difficulty is active
    public float distanceToReach; // How many meters the player must survive on this difficulty before leveling up to the next one
    
    [Header("Spawning Settings")]
    public float spawnRate = 1f; // How frequently a track tile tries to spawn things (mot currently used)
    public int maxObstaclesPerTrack = 3; // The absolute maximum number of obstacles/coins that can be placed on a single 50-unit track tile
    
    [Header("Allowed Items & Probabilities")]
    public List<SpawnableItem> allowedItems; // The list of items that are legally allowed to spawn in this difficulty tier. If an item isn't in this list, it will NEVER spawn!
}