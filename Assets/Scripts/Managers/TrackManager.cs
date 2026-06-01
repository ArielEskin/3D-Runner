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
    
    // NEW: How many track tiles should be totally empty at the start of the game
    public int startingSafeTiles = 1; 
    
    private float spawnZ = 0f;
    private Queue<GameObject> activeTiles;

    private Queue<GameObject> activeItemsOnTracks = new Queue<GameObject>();
    private Queue<int> itemsPerTileQueue = new Queue<int>();

    void Start()
    {
        activeTiles = new Queue<GameObject>();
        for (int i = 0; i < tilesOnScreen; i++)
        {
            // If 'i' is less than startingSafeTiles (e.g., 0 and 1), pass 'false' to disable obstacles
            SpawnTile(i >= startingSafeTiles); 
        }
    }

    void Update()
    {
        float oldestTileZ = spawnZ - (tilesOnScreen * tileLength);

        if (playerTransform.position.z > oldestTileZ + tileLength + despawnBuffer)
        {
            // Normal gameplay tiles spawned while running will always have obstacles (pass true)
            SpawnTile(true);
            RecycleTile();
        }
    }

    // Added the spawnObstacles parameter
    private void SpawnTile(bool spawnObstacles)
    {
        GameObject tile = pooler.GetTile();
        tile.transform.position = Vector3.forward * spawnZ;
        activeTiles.Enqueue(tile);
        
        // Pass the decision down to the item spawner
        SpawnItemsOnTile(spawnZ, spawnObstacles);

        spawnZ += tileLength;
    }

    // Added the spawnObstacles parameter here too
    private void SpawnItemsOnTile(float currentZ, bool spawnObstacles)
    {
        int itemsSpawnedOnThisTile = 0;

        // ===================================================================
        // NATURAL SCENERY SPAWNING (Always happens, even in the safe zone)
        // ===================================================================

        int leftSceneryCount = Random.Range(2, 6); 
        for (int i = 0; i < leftSceneryCount; i++)
        {
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

        // ===================================================================
        // GAMEPLAY ITEMS 
        // ===================================================================
        
        int maxObstacles = difficultyManager.currentDifficulty.maxObstaclesPerTrack;
        List<SpawnableItem> allowedItems = difficultyManager.currentDifficulty.allowedItems;
        
        // NEW: Check if this tile is allowed to have obstacles!
        if (spawnObstacles && maxObstacles > 0 && allowedItems.Count > 0) 
        {
            float availableLength = tileLength - 20f; 
            float zSpacing = availableLength / maxObstacles; 

            for (int i = 0; i < maxObstacles; i++)
            {
                int randomLane = Random.Range(-1, 2); 
                float sliceStart = 10f + (i * zSpacing);
                float randomZOffset = sliceStart + Random.Range(0f, zSpacing * 0.8f);

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

                Quaternion spawnRotation = (itemTag == "LowObstacle" || itemTag == "HighObstacle") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

                GameObject spawnedItem = ObjectPooler.Instance.SpawnFromPool(itemTag, new Vector3(randomLane, itemY, currentZ + randomZOffset), spawnRotation);
                
                if (spawnedItem != null)
                {
                    activeItemsOnTracks.Enqueue(spawnedItem);
                    itemsSpawnedOnThisTile++; 
                }
            }
        }
        
        // Always save the count to the recycling queue, even if it's just the scenery!
        itemsPerTileQueue.Enqueue(itemsSpawnedOnThisTile);
    }

    private void RecycleTile()
    {
        GameObject oldTile = activeTiles.Dequeue();
        pooler.ReturnTile(oldTile);

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