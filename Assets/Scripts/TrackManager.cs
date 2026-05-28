using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public TrackPooler pooler;
    public DifficultyManager difficultyManager; 

    [Header("Track Settings")]
    public float tileLength = 50f;
    public int tilesOnScreen = 5;
    public float despawnBuffer = 10f; 
    
    private float spawnZ = 0f;
    private Queue<GameObject> activeTiles;

    private Queue<GameObject> activeItemsOnTracks = new Queue<GameObject>();
    private Queue<int> itemsPerTileQueue = new Queue<int>();

    void Start()
    {
        activeTiles = new Queue<GameObject>();
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile(); 
        }
    }

    void Update()
    {
        float oldestTileZ = spawnZ - (tilesOnScreen * tileLength);

        // Uses the despawn buffer so tiles don't vanish exactly at the player's heels
        if (playerTransform.position.z > oldestTileZ + tileLength + despawnBuffer)
        {
            SpawnTile();
            RecycleTile();
        }
    }

    private void SpawnTile()
    {
        GameObject tile = pooler.GetTile();
        tile.transform.position = Vector3.forward * spawnZ;
        activeTiles.Enqueue(tile);
        
        // Call this unconditionally so every tile gets obstacles
        SpawnItemsOnTile(spawnZ);

        spawnZ += tileLength;
    }

    private void SpawnItemsOnTile(float currentZ)
    {
        int itemsSpawnedOnThisTile = 0;

        // NATURAL SCENERY SPAWNING ====================================================

        int leftSceneryCount = Random.Range(2, 6); 
        for (int i = 0; i < leftSceneryCount; i++)
        {
            //  20% chance for a Tree, 80% chance for a Plant
            string sceneryTag = Random.Range(0, 100) < 20 ? "Tree" : "Bush";
   
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
        // GAMEPLAY ITEMS (OBSTACLES/COIN) ===================================================
        int maxObstacles = difficultyManager.currentDifficulty.maxObstaclesPerTrack;
        
        // Prevent division by zero if easy mode ever has 0 obstacles
        if (maxObstacles > 0) 
        {
            // We only spawn between Z=10 and Z=(tileLength - 10) to avoid the very edges
            float availableLength = tileLength - 20f; 
            float zSpacing = availableLength / maxObstacles; // How much Z-space each item gets

            for (int i = 0; i < maxObstacles; i++)
            {
                int randomLane = Random.Range(-1, 2); 
                
                // Calculate the start of this specific item's "slice"
                float sliceStart = 10f + (i * zSpacing);
                
                // Pick a random spot strictly inside its own slice (using 0.8f leaves a small gap between slices)
                float randomZOffset = sliceStart + Random.Range(0f, zSpacing * 0.8f);
                
                // 3-WAY SPLIT: 40% Coin, 30% LowObstacle, 30% HighObstacle
                int itemRoll = Random.Range(0, 100);
                string itemTag = "Coin"; 
                if (itemRoll >= 40 && itemRoll < 70) itemTag = "LowObstacle";
                else if (itemRoll >= 70) itemTag = "HighObstacle";

                // SPECIFIC HEIGHTS FOR EVERY ITEM
                float itemY = 1.0f; // Default fallback just in case
                if (itemTag == "Coin") 
                {
                    itemY = 1.5f;
                }
                else if (itemTag == "LowObstacle") 
                {
                    itemY = 0.8f;
                }
                else if (itemTag == "HighObstacle") 
                {
                    itemY = 1.5f;
                }

                Quaternion spawnRotation = (itemTag == "LowObstacle" || itemTag == "HighObstacle") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

                GameObject spawnedItem = ObjectPooler.Instance.SpawnFromPool(itemTag, new Vector3(randomLane, itemY, currentZ + randomZOffset), spawnRotation);
                
                if (spawnedItem != null)
                {
                    activeItemsOnTracks.Enqueue(spawnedItem);
                    itemsSpawnedOnThisTile++; 
                }
            }
        }
        itemsPerTileQueue.Enqueue(itemsSpawnedOnThisTile);
    }

    private void RecycleTile()
    {
        GameObject oldTile = activeTiles.Dequeue();
        pooler.ReturnTile(oldTile);

        // Dequeue the exact number of items that were on this specific tile
        int itemsToRecycle = itemsPerTileQueue.Dequeue();
        
        for(int i = 0; i < itemsToRecycle; i++) 
        {
            if (activeItemsOnTracks.Count > 0)
            {
                GameObject oldItem = activeItemsOnTracks.Dequeue();
                ObjectPooler.Instance.ReturnToPool(oldItem);
            }
        }
    }
}