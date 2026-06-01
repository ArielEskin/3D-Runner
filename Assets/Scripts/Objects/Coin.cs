using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    
    private void OnTriggerEnter(Collider other) // If player touches coin return it to pull and add to score
    {
        if (other.CompareTag("Player"))
        {
            GameManager.gameManager.AddCoin(coinValue); // Add to score
            ObjectPooler.Instance.ReturnToPool(gameObject); // Immediately pool on pickup
        }
    }
    
    
}