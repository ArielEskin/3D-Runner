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

    private void SpawnItemsOnTile(float currentZ, bool spawnObstacles) // Handles the randomized placement of scenery and gameplay obstacles.
    {
        int itemsSpawnedOnThisTile = 0;
        
        // ====================================================================================
        // NATURAL SCENERY SPAWNING
        // ====================================================================================
        
        int leftSceneryCount = Random.Range(2, 6); 
        for (int i = 0; i < leftSceneryCount; i++) // Spawning the Left Side Environment
        {
            string sceneryTag = Random.Range(0, 100) < 20 ? "Tree" : "Plant"; 
            
            float randomX = Random.Range(-8f, -3.5f); // Randomize position outside the playable track (for natural environment feel)
            float randomZ = currentZ + Random.Range(0f, tileLength);
            float sceneryY = -0.6f; 

            // Pull the scenery from the ObjectPooler instead of using Instantiate
            GameObject scenery = ObjectPooler.Instance.SpawnFromPool(sceneryTag, new Vector3(randomX, sceneryY, randomZ), Quaternion.identity);
            if (scenery != null)
            {
                activeItemsOnTracks.Enqueue(scenery);
                itemsSpawnedOnThisTile++;
            }
        }
        
        int rightSceneryCount = Random.Range(2, 6);
        for (int i = 0; i < rightSceneryCount; i++) // Spawning the Left Side Environment
        {
            string sceneryTag = Random.Range(0, 100) < 20 ? "Tree" : "Plant";
            
            float randomX = Random.Range(3.5f, 8f); // Randomize position outside the playable track (for natural environment feel)
            float randomZ = currentZ + Random.Range(0f, tileLength);
            float sceneryY = -0.5f;

            // Pull the scenery from the ObjectPooler instead of using Instantiate
            GameObject scenery = ObjectPooler.Instance.SpawnFromPool(sceneryTag, new Vector3(randomX, sceneryY, randomZ), Quaternion.identity);
            if (scenery != null)
            {
                activeItemsOnTracks.Enqueue(scenery);
                itemsSpawnedOnThisTile++;
            }
        } 
        
        // ====================================================================================
        // GAMEPLAY OBJECTS
        // ====================================================================================
        
        int maxObstacles = difficultyManager.currentDifficulty.maxObstaclesPerTrack; // Pull the current difficulty rules from the Scriptable Object
        List<SpawnableItem> allowedItems = difficultyManager.currentDifficulty.allowedItems;
        
        if (spawnObstacles && maxObstacles > 0 && allowedItems.Count > 0)  // Enters if tile allows at least 1 object spawn, and the difficulty has objects set up
        {
            // Chop the track into equal segments so obstacles don't spawn inside each other
            float availableLength = tileLength - 20f; // Leave a 10 unit buffer at the start and end of the tile
            float zSpacing = availableLength / maxObstacles;

            for (int i = 0; i < maxObstacles; i++)
            {
                int randomLane = Random.Range(-1, 2); 
                float sliceStart = 10f + (i * zSpacing); // Calculate where this specific item's "slice" begins, and pick a random spot within it
                float randomZOffset = sliceStart + Random.Range(0f, zSpacing * 0.8f);

                float totalWeight = 0f; // Calculate the total probability weight of all allowed items
                foreach (SpawnableItem item in allowedItems)
                {
                    totalWeight += item.spawnProbability;
                }

                float randomVal = Random.Range(0f, totalWeight); // Pick a random number between 0 and the total weight
                SpawnableItem chosenItem = allowedItems[0];

                foreach (SpawnableItem item in allowedItems) // Subtract weights until we hit 0 to find the winner
                {
                    if (randomVal <= item.spawnProbability)
                    {
                        chosenItem = item;
                        break;
                    }
                    randomVal -= item.spawnProbability;
                }

                string itemTag = chosenItem.poolTag; // Extract the data from the winning ScriptableObject item
                float itemY = chosenItem.spawnHeight; 

                // If it's an obstacle, rotate it 90 degrees on the Y axis so it faces the player
                Quaternion spawnRotation = (itemTag == "LowObstacle" || itemTag == "HighObstacle") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

                GameObject spawnedItem = ObjectPooler.Instance.SpawnFromPool(itemTag, new Vector3(randomLane, itemY, currentZ + randomZOffset), spawnRotation);
                
                if (spawnedItem != null)
                {
                    activeItemsOnTracks.Enqueue(spawnedItem);
                    itemsSpawnedOnThisTile++; 
                }
            }
        }
        itemsPerTileQueue.Enqueue(itemsSpawnedOnThisTile); // Push the final count of items created into the queue.
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