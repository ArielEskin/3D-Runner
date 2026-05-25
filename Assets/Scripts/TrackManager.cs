using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public TrackPooler pooler;

    [Header("Track Settings")]
    public float tileLength = 50f;
    public int tilesOnScreen = 5;
    
    [Tooltip("How far past the tile the player must travel before it despawns")]
    public float safeZone = 10f; 
    
    private float spawnZ = 0f;
    private Queue<GameObject> activeTiles;

    void Start()
    {
        activeTiles = new Queue<GameObject>();
        for (int i = 0; i < tilesOnScreen; i++) // Spawn initial tiles to fill the screen
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // Find the exact Z position of the oldest tile (the one at the back)
        float oldestTileZ = spawnZ - (tilesOnScreen * tileLength);

        // Check if the player has passed the end of the tile + the Safe Zone so you cant see it disappear on camera
        if (playerTransform.position.z > oldestTileZ + tileLength + safeZone)
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
        
        spawnZ += tileLength;
    }

    private void RecycleTile()
    {
        GameObject oldTile = activeTiles.Dequeue();
        pooler.ReturnTile(oldTile);
    }
}