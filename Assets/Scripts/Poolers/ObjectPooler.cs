using System.Collections.Generic;
using UnityEngine;

//  creates a set amount of items, when the game starts, hides them, and recycles them over and over.
public class ObjectPooler : MonoBehaviour
{
    // [System.Serializable] makes this custom class visible in the Unity Inspector, so you can easily set up tags, prefabs, and sizes without changing code.
    [System.Serializable]
    public class Pool
    {
        public string tag; // "Coin", "Tree", "Bush", "HighObstacle", "LowObstacle"
        public GameObject prefab; // The actual prefab to spawn
        public int size; // How many to create at the start of the game
    }
    
    // ==========references================================================================================================================================================

    public static ObjectPooler Instance;
    public List<Pool> pools; // The list in the Unity Inspector
    public Dictionary<string, Queue<GameObject>> poolDictionary; // A Dictionary lets us look up a specific Queue using the tag
    private Dictionary<string, GameObject> prefabDictionary; // Dictionary to keep track of prefabs so we can expand the pool dynamically
    
    // ==========================================================================================================================================================

    void Awake() // Generates the initial hidden batches of items based on the Inspector lists and stores them in dictionaries
    {
        if (Instance == null) Instance = this; // Singleton pattern for easy global access
        else Destroy(gameObject);

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabDictionary = new Dictionary<string, GameObject>(); 

        foreach (Pool pool in pools) //Loop through every pool set up in the Inspector fill them
        {
            prefabDictionary.Add(pool.tag, pool.prefab);
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++) // Generate the initial batch of objects
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false); // Hide them immediately
                obj.transform.SetParent(this.transform);
                objectPool.Enqueue(obj); // Add them to the back of the line
            }
            poolDictionary.Add(pool.tag, objectPool); // Add the fully stocked Queue into our Dictionary, labeled with its tag
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) // Pulls a ready-to-use object from the requested pool, places it at the target location, and activates it.
    {
        if (!poolDictionary.ContainsKey(tag)) // Safety Check: misspelling a tag in another script
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        
        // If the object we just pulled is turned on, it means the pool needs to be expanded
        if (objectToSpawn.activeInHierarchy) 
        {
            poolDictionary[tag].Enqueue(objectToSpawn);

            objectToSpawn = Instantiate(prefabDictionary[tag]);
            objectToSpawn.transform.SetParent(this.transform);
        }
        // Set up the object for gameplay
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
    
    public void ReturnToPool(GameObject obj) // Instantly hides an object, Used by items that get "collected" or "destroyed" (a Coin that the player picks up)
    {
        obj.SetActive(false);
        obj.transform.SetParent(this.transform);
    }
}