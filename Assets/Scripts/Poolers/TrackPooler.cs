using System.Collections.Generic;
using UnityEngine;

// A dedicated object pool just for the track tiles. 
// It creates a small number of track pieces when the game starts and recycles 
// the one behind the camera by moving it to the front of the line.
public class TrackPooler : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject trackPrefab;
    public int initialPoolSize = 5;

    private Queue<GameObject> poolQueue; // First-In-First-Out - The oldest track behind the player is always the first one in line to be reused

    void Awake()
    {
        poolQueue = new Queue<GameObject>(); // Initialize the empty queue
        for (int i = 0; i < initialPoolSize; i++) // generating the initial batch of tiles
        {
            CreateNewTile();
        }
    }

    private GameObject CreateNewTile() // creates a new clone of the track prefab, hides it, and adds it to waiting line.
    {
        GameObject tile = Instantiate(trackPrefab);
        tile.SetActive(false); // Hide it until the TrackManager actually asks for it
        poolQueue.Enqueue(tile); // Put it at the back of the line
        return tile;
    }

    public GameObject GetTile() // Called by the TrackManager whenever it needs a new piece of floor to put in front of the player.
    {
        if (poolQueue.Count == 0) // SAFETY NET: create a brand-new tile on the to prevent the game from crashing
        {
            CreateNewTile();
        }
        GameObject tile = poolQueue.Dequeue(); // Grab the oldest, unused tile from the front of the line
        tile.SetActive(true);
        return tile;
    }

    public void ReturnTile(GameObject tile) // Called by the TrackManager when a tile has fallen completely behind the camera.
    {
        tile.SetActive(false); 
        poolQueue.Enqueue(tile);
    }
}