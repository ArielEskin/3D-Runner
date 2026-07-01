using System.Collections.Generic;
using UnityEngine;

// creates a small number of track pieces when the game starts and recycles the one behind the camera by moving it to the front of the line
public class TrackPooler : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [Header("Pool Settings")]
    public GameObject trackPrefab;
    public int initialPoolSize = 5;

    private Queue<GameObject> poolQueue;

    // ==========================================================================================================================================================

    void Awake() // Generates the initial batch of floor tiles.
    {
        poolQueue = new Queue<GameObject>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewTile();
        }
    }

    private GameObject CreateNewTile() // Instantiates a new floor prefab and adds it to the storage queue
    {
        GameObject tile = Instantiate(trackPrefab);
        tile.SetActive(false);
        poolQueue.Enqueue(tile);
        return tile;
    }

    public GameObject GetTile() // Called by the TrackManager whenever it needs a new piece of floor to put in front of the player.
    {
        if (poolQueue.Count == 0)
        {
            CreateNewTile();
        }
        GameObject tile = poolQueue.Dequeue();
        tile.SetActive(true);
        return tile;
    }

    public void ReturnTile(GameObject tile) // Called by the TrackManager when a tile has fallen completely behind the camera.
    {
        tile.SetActive(false); 
        poolQueue.Enqueue(tile);
    }
}