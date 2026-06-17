using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float flySpeed = 25f; // How fast they fly to the player with magnet active
    
    private Transform playerTransform;

    void Start()
    {
        // Find the player once so we know who to fly towards
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (GameManager.gameManager.isMagnetActive && playerTransform != null)
        {
            // Check distance to player
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= GameManager.gameManager.magnetRadius)
            {
                // Fly smoothly towards the player's center
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position + Vector3.up, flySpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.instance.PlaySound3D("PickUpCoin", transform.position); 
            GameManager.gameManager.AddCoin(coinValue); 
            ObjectPooler.Instance.ReturnToPool(gameObject); 
        }
    }
}