using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.gameManager.AddCoin(coinValue);
            // Add to score manager here
            ObjectPooler.Instance.ReturnToPool(gameObject); // Immediately pool on pickup
        }
    }
    
    
}