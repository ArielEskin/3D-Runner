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
    
    private float spawnZ = 0f;
    private Queue<GameObject> activeTiles;

    void Start()
    {
        activeTiles = new Queue<GameObject>();

        // Spawn initial tiles to fill the screen
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // Check if the player has moved far enough to spawn a new tile
        if (playerTransform.position.z - tileLength > spawnZ - (tilesOnScreen * tileLength))
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