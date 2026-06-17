using System.Collections.Generic;
using UnityEngine;

// Manages the endless spawning, despawning, and recycling of track tiles and all items (scenery, obstacles, coins).
public class TrackManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform mainCamera;
    public TrackPooler pooler;
    public DifficultyManager difficultyManager; 

    [Header("Track Settings")]
    public float tileLength = 50f;
    public int tilesOnScreen = 5;
    public float despawnBuffer = 10f; // Extra distance player must travel past a tile before it despawns (prevents visual popping)
    public int startingSafeTiles = 1; // How many tiles at Z=0 spawn completely empty of obstacles so the player can prepare
    
    [Header("Floating Origin")]
    public float resetThreshold = 1000f; // When the player passes this Z-coordinate, the whole world snaps back to 0
    private float spawnZ = 0f; // Tracks the Z-coordinate where the NEXT track tile needs to be placed
    
    private Queue<GameObject> activeTiles; // Tracks the actual floor pieces
    private Queue<GameObject> activeItemsOnTracks = new Queue<GameObject>(); // Tracks the trees, coins, and obstacles
    private Queue<int> itemsPerTileQueue = new Queue<int>(); // Remembers exactly how many items were spawned on a specific tile so we know how many to delete later

    void Start()
    {
        activeTiles = new Queue<GameObject>();
        for (int i = 0; i < tilesOnScreen; i++) // Fill the screen with track tiles when the game boots up
        {
            SpawnTile(i >= startingSafeTiles);  // Prevents Obstacles from spawning in front of the player when starting
        }
    }

    void Update()
    {
        float oldestTileZ = spawnZ - (tilesOnScreen * tileLength); // Calculate the exact Z of the oldest tile

        if (playerTransform.position.z > oldestTileZ + tileLength + despawnBuffer)  // Check if the player has passed the oldest tile + the despawn buffer
        {
            SpawnTile(true);
            RecycleTile();
        }
        if (playerTransform.position.z > resetThreshold) // Check if we have traveled too far ---> snap back to z=0
        {
            ResetOrigin();
        }
    }
    
    private void ResetOrigin()
    {
        float offset = playerTransform.position.z; // Find exactly how far we need to move everything back

        playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, 0f); // Snap Player back to Z=0

        if (mainCamera != null) // Snap Camera back by the exact same offset from player
        {
            mainCamera.position = new Vector3(mainCamera.position.x, mainCamera.position.y, mainCamera.position.z - offset);
        }

        spawnZ -= offset; // Reset the Track Spawner's target Z

        foreach (GameObject tile in activeTiles) // Snap all active track tiles back
        {
            tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z - offset);
        }
        
        foreach (GameObject item in activeItemsOnTracks) // Snap all active scenery, coins, and obstacles back
        {
            item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y, item.transform.position.z - offset);
        }

    }

    private void SpawnTile(bool spawnObstacles) // Grabs a new floor tile from the pool and places it at the front of the track
    {
        GameObject tile = pooler.GetTile();
        tile.transform.position = Vector3.forward * spawnZ;
        activeTiles.Enqueue(tile);
        
        SpawnItemsOnTile(spawnZ, spawnObstacles); // Trigger the item spawner for this specific tile's location

        spawnZ += tileLength; // Move Forward the spawning point for the next time this is called
    }

    private void SpawnItemsOnTile(float currentZ, bool spawnObstacles)
    {
        int itemsSpawnedOnThisTile = 0;

        // ====================================================================================
        // NATURAL SCENERY SPAWNING
        // ====================================================================================
        
        int leftSceneryCount = Random.Range(2, 6); 
        for (int i = 0; i < leftSceneryCount; i++) 
        {
            string sceneryTag = Random.Range(0, 100) < 20 ? "Tree" : "Plant"; 
            float randomX = Random.Range(-8f, -3.5f); 
            float randomZ = currentZ + Random.Range(0f, tileLength);
            float sceneryY = -0.6f; 

            GameObject scenery = ObjectPooler.Instance.SpawnFromPool(sceneryTag, new Vector3(randomX, sceneryY, randomZ), Quaternion.identity);
            if (scenery != null)
            {
                activeItemsOnTracks.Enqueue(scenery);
                itemsSpawnedOnThisTile++;
            }
        }
        
        int rightSceneryCount = Random.Range(2, 6);
        for (int i = 0; i < rightSceneryCount; i++) 
        {
            string sceneryTag = Random.Range(0, 100) < 20 ? "Tree" : "Plant";
            float randomX = Random.Range(3.5f, 8f); 
            float randomZ = currentZ + Random.Range(0f, tileLength);
            float sceneryY = -0.5f;

            GameObject scenery = ObjectPooler.Instance.SpawnFromPool(sceneryTag, new Vector3(randomX, sceneryY, randomZ), Quaternion.identity);
            if (scenery != null)
            {
                activeItemsOnTracks.Enqueue(scenery);
                itemsSpawnedOnThisTile++;
            }
        } 

        // ====================================================================================
        // OBSTACLES
        // ====================================================================================

        List<Vector2> lowObstaclePositions = new List<Vector2>(); 
        List<Vector2> highObstaclePositions = new List<Vector2>();

        int maxObstacles = difficultyManager.currentDifficulty.maxObstaclesPerTrack; 
        List<SpawnableItem> allowedItems = difficultyManager.currentDifficulty.allowedItems;
        
        if (spawnObstacles && maxObstacles > 0 && allowedItems.Count > 0)  
        {
            int rowsToSpawn = Mathf.Clamp(maxObstacles, 1, 4);
            float availableLength = tileLength - 20f; 
            float zSpacing = availableLength / rowsToSpawn;

            for (int i = 0; i < rowsToSpawn; i++)
            {
                float sliceStart = 10f + (i * zSpacing); 
                float exactZ = sliceStart + (zSpacing / 2f); 

                int itemsInThisRow = 1;
                if (maxObstacles > 4 && Random.Range(0, 100) < 60)
                {
                    itemsInThisRow = 2;
                }

                List<int> availableLanes = new List<int> { -1, 0, 1 };

                for (int j = 0; j < itemsInThisRow; j++)
                {
                    int laneIndex = Random.Range(0, availableLanes.Count);
                    int randomLane = availableLanes[laneIndex];
                    availableLanes.RemoveAt(laneIndex); 

                    float totalWeight = 0f; 
                    foreach (SpawnableItem item in allowedItems)
                    {
                        totalWeight += item.spawnProbability;
                    }

                    float randomVal = Random.Range(0f, totalWeight); 
                    SpawnableItem chosenItem = allowedItems[0];

                    foreach (SpawnableItem item in allowedItems) 
                    {
                        if (randomVal <= item.spawnProbability)
                        {
                            chosenItem = item;
                            break;
                        }
                        randomVal -= item.spawnProbability;
                    }

                    string itemTag = chosenItem.poolTag; 
                    float itemY = chosenItem.spawnHeight; 
                    
                    if (itemTag == "LowObstacle") 
                    {
                        lowObstaclePositions.Add(new Vector2(randomLane, exactZ));
                    }
                    else if (itemTag == "HighObstacle") 
                    {
                        highObstaclePositions.Add(new Vector2(randomLane, exactZ));
                    }

                    Quaternion spawnRotation = (itemTag == "LowObstacle" || itemTag == "HighObstacle") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

                    GameObject spawnedItem = ObjectPooler.Instance.SpawnFromPool(itemTag, new Vector3(randomLane, itemY, currentZ + exactZ), spawnRotation);
                    
                    if (spawnedItem != null)
                    {
                        activeItemsOnTracks.Enqueue(spawnedItem);
                        itemsSpawnedOnThisTile++; 
                    }
                }
            }
        }
        
        // ====================================================================================
        // COIN STREAKS 
        // ====================================================================================
        
        if (spawnObstacles && Random.Range(0, 100) < difficultyManager.currentDifficulty.coinStreakChance)
        {
            int streakLength = Random.Range(difficultyManager.currentDifficulty.minCoinsInStreak, difficultyManager.currentDifficulty.maxCoinsInStreak + 1);
            int coinLane = Random.Range(-1, 2);
            float coinSpacing = difficultyManager.currentDifficulty.coinSpacing;
            
            float maxAvailableLength = tileLength - 20f; 
            float totalStreakLength = streakLength * coinSpacing;
            
            if (totalStreakLength < maxAvailableLength)
            {
                float randomStartZ = 10f + Random.Range(0f, maxAvailableLength - totalStreakLength);
                
                for (int i = 0; i < streakLength; i++)
                {
                    float coinZ = randomStartZ + (i * coinSpacing);
                    
                    float coinY = 1.5f; // Default height for running into it
                    bool cancelCoin = false;

                    //  Is this coin touching a Low Obstacle? Raise it
                    foreach (Vector2 obs in lowObstaclePositions)
                    {
                        // If it's in the same lane and within 3 units of distance
                        if (obs.x == coinLane && Mathf.Abs(obs.y - coinZ) < 3f)
                        {
                            coinY = 2.8f; // Pushed up perfectly into the player's jump arc
                            break;
                        }
                    }

                    //  Is this coin touching a High Obstacle? Delete it
                    foreach (Vector2 obs in highObstaclePositions)
                    {
                        if (obs.x == coinLane && Mathf.Abs(obs.y - coinZ) < 3f)
                        {
                            cancelCoin = true; // Player can't go through a wall, so don't trick them!
                            break;
                        }
                    }

                    // If it overlaps a tall wall, skip this specific coin and move to the next one
                    if (cancelCoin) continue;

                    GameObject spawnedCoin = ObjectPooler.Instance.SpawnFromPool("Coin", new Vector3(coinLane, coinY, currentZ + coinZ), Quaternion.identity);
                    
                    if (spawnedCoin != null)
                    {
                        activeItemsOnTracks.Enqueue(spawnedCoin);
                        itemsSpawnedOnThisTile++;
                    }
                }
            }
        }
        // ====================================================================================
        // POWER-UPS 
        // ====================================================================================
        
        if (spawnObstacles && difficultyManager.currentDifficulty.allowedPowerUps.Count > 0)
        {
            // Roll the dice to see if a powerup should spawn on this tile
            if (Random.Range(0, 100) < difficultyManager.currentDifficulty.powerUpSpawnChancePerTrack)
            {
                int randomLane = Random.Range(-1, 2); 
                float randomZOffset = Random.Range(10, tileLength - 10);
                
                // Pick a random powerup from the Allowed PowerUps list
                int randomIndex = Random.Range(0, difficultyManager.currentDifficulty.allowedPowerUps.Count);
                SpawnablePowerUp chosenPU = difficultyManager.currentDifficulty.allowedPowerUps[randomIndex];

                // Spawn it!
                GameObject spawnedPU = ObjectPooler.Instance.SpawnFromPool(chosenPU.powerUpConfig.poolTag, new Vector3(randomLane, chosenPU.spawnHeight, currentZ + randomZOffset), Quaternion.identity);
                
                if (spawnedPU != null)
                {
                    activeItemsOnTracks.Enqueue(spawnedPU);
                    itemsSpawnedOnThisTile++;
                }
            }
        }

        itemsPerTileQueue.Enqueue(itemsSpawnedOnThisTile); 
    }

    private void RecycleTile() // Removes the oldest tile and its objects behind the camera and returns them to their respective pools.
    {
        GameObject oldTile = activeTiles.Dequeue(); // Clean up the floor tile
        pooler.ReturnTile(oldTile);

        int itemsToRecycle = itemsPerTileQueue.Dequeue(); // Find out exactly how many items (scenery + gameplay) were placed on this tile
        
        for(int i = 0; i < itemsToRecycle; i++)  // Dequeue that exact amount from the item queue and return them to the object pool
        {
            if (activeItemsOnTracks.Count > 0)
            {
                GameObject oldItem = activeItemsOnTracks.Dequeue();
                ObjectPooler.Instance.ReturnToPool(oldItem);
            }
        }
    }
}