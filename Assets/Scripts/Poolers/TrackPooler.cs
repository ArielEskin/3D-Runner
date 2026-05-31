using System.Collections.Generic;
using UnityEngine;

public class TrackPooler : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject trackPrefab;
    public int initialPoolSize = 5;

    private Queue<GameObject> poolQueue;

    void Awake()
    {
        poolQueue = new Queue<GameObject>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewTile();
        }
    }

    private GameObject CreateNewTile()
    {
        GameObject tile = Instantiate(trackPrefab);
        tile.SetActive(false);
        poolQueue.Enqueue(tile);
        return tile;
    }

    public GameObject GetTile()
    {
        if (poolQueue.Count == 0)
        {
            CreateNewTile();
        }
        GameObject tile = poolQueue.Dequeue();
        tile.SetActive(true);
        return tile;
    }

    public void ReturnTile(GameObject tile)
    {
        tile.SetActive(false);
        poolQueue.Enqueue(tile);
    }
}