using System.Collections.Generic;
using UnityEngine;

// A memory management system. Instead of constantly Instantiating
// and Destroying objects, this script creates a set amount of items 
// when the game starts, hides them, and recycles them over and over.
public class ObjectPooler : MonoBehaviour
{
    // [System.Serializable] makes this custom class visible in the Unity Inspector
    // so you can easily set up tags, prefabs, and sizes without changing code.
    [System.Serializable]
    public class Pool
    {
        public string tag; // "Coin", "Tree", "Bush", "HighObstacle", "LowObstacle"
        public GameObject prefab; // The actual prefab to spawn
        public int size; // How many to create at the start of the game
    }
    // Singleton pattern: Allows any other script in the game to access this 
    // simply by typing ObjectPooler.Instance (TrackManager uses this)
    public static ObjectPooler Instance;
    
    public List<Pool> pools; // The list in the Unity Inspector
    public Dictionary<string, Queue<GameObject>> poolDictionary; // A Dictionary lets us look up a specific Queue using the tag
    private Dictionary<string, GameObject> prefabDictionary; // Dictionary to keep track of prefabs so we can expand the pool dynamically

    void Awake()
    {
        if (Instance == null) Instance = this; // Singleton pattern for easy global access
        else Destroy(gameObject);

        poolDictionary = new Dictionary<string, Queue<GameObject>>(); // Initialize the empty dictionaries
        prefabDictionary = new Dictionary<string, GameObject>(); 

        foreach (Pool pool in pools) //Loop through every pool set up in the Inspector fill them
        {
            prefabDictionary.Add(pool.tag, pool.prefab); // Store the prefab reference so we can clone it later if needed
            
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++) // Generate the initial batch of objects
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // Hide them immediately
                obj.transform.SetParent(this.transform); // Tuck them neatly under the ObjectPooler in the Hierarchy
                objectPool.Enqueue(obj); // Add them to the back of the line
            }
            poolDictionary.Add(pool.tag, objectPool); // Add the fully stocked Queue into our Dictionary, labeled with its tag
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) // Pulls a ready-to-use object from the requested pool, places it at the target location, and activates it.
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue(); // Grab the next object in the queue

        if (objectToSpawn.activeInHierarchy)
        {
            poolDictionary[tag].Enqueue(objectToSpawn); // Put the active object back into the queue safely so we don't lose its reference
            
            // Instantiate a new object to handle the increased difficulty spawn rate
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