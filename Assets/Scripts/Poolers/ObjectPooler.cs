using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    
    // NEW: Dictionary to keep track of prefabs so we can expand the pool dynamically
    private Dictionary<string, GameObject> prefabDictionary;

    void Awake()
    {
        // Singleton pattern for easy global access
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>(); 

        foreach (Pool pool in pools)
        {
            // Store the prefab reference so we can clone it later if needed
            prefabDictionary.Add(pool.tag, pool.prefab);
            
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(this.transform); // Keep hierarchy clean
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        // Grab the next object in the queue
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // FIX: If the object we just grabbed is ALREADY active in the scene,
        // it means our pool is empty! We must dynamically expand it.
        if (objectToSpawn.activeInHierarchy)
        {
            // Put the active object back into the queue safely so we don't lose its reference
            poolDictionary[tag].Enqueue(objectToSpawn);
            
            // Instantiate a brand new object to handle the increased difficulty spawn rate
            objectToSpawn = Instantiate(prefabDictionary[tag]);
            objectToSpawn.transform.SetParent(this.transform);
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn); // Put it back at the end of the queue

        return objectToSpawn;
    }
    
    public void ReturnToPool(GameObject obj) 
    {
        obj.SetActive(false);
        obj.transform.SetParent(this.transform);
    }
}