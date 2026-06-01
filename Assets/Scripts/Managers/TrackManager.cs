using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform mainCamera; // NEW: Drag your Main Camera here in the Inspector!
    public TrackPooler pooler;
    public DifficultyManager difficultyManager; 

    [Header("Track Settings")]
    public float tileLength = 50f;
    public int tilesOnScreen = 5;
    public float despawnBuffer = 10f; 
    public int startingSafeTiles = 2; 
    
    [Header("Floating Origin")]
    public float resetThreshold = 1000f; // NEW: When the player hits Z=1000, snap the world back
    
    private float spawnZ = 0f;
    private Queue<GameObject> activeTiles;

    private Queue<GameObject> activeItemsOnTracks = new Queue<GameObject>();
    private Queue<int> itemsPerTileQueue = new Queue<int>();

    void Start()
    {
        activeTiles = new Queue<GameObject>();
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile(i >= startingSafeTiles); 
        }
    }

    void Update()
    {
        float oldestTileZ = spawnZ - (tilesOnScreen * tileLength);

        if (playerTransform.position.z > oldestTileZ + tileLength + despawnBuffer)
        {
            SpawnTile(true);
            RecycleTile();
        }

        // NEW: Check if we have traveled too far
        if (playerTransform.position.z > resetThreshold)
        {
            ResetOrigin();
        }
    }
    
    private void ResetOrigin()
    {
        // Find exactly how far we need to move everything back
        float offset = playerTransform.position.z;

        // Snap Player back to Z=0
        playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, 0f);

        // Snap Camera back by the exact same offset
        if (mainCamera != null)
        {
            mainCamera.position = new Vector3(mainCamera.position.x, mainCamera.position.y, mainCamera.position.z - offset);
        }

        // 3. Reset the Track Spawner's target Z
        spawnZ -= offset;

        // 4. Snap all active track tiles back
        foreach (GameObject tile in activeTiles)
        {
            tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z - offset);
        }

        // 5. Snap all active scenery, coins, and obstacles back
        foreach (GameObject item in activeItemsOnTracks)
        {
            item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y, item.transform.position.z - offset);
        }

    }

    private void SpawnTile(bool spawnObstacles)
    {
        GameObject tile = pooler.GetTile();
        tile.transform.position = Vector3.forward * spawnZ;
        activeTiles.Enqueue(tile);
        
        SpawnItemsOnTile(spawnZ, spawnObstacles);

        spawnZ += tileLength;
    }

    private void SpawnItemsOnTile(float currentZ, bool spawnObstacles)
    {
        int itemsSpawnedOnThisTile = 0;

        // NATURAL SCENERY SPAWNING
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

        // GAMEPLAY ITEMS
        int maxObstacles = difficultyManager.currentDifficulty.maxObstaclesPerTrack;
        List<SpawnableItem> allowedItems = difficultyManager.currentDifficulty.allowedItems;
        
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