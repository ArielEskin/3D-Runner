using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add to score manager here
            ObjectPooler.Instance.ReturnToPool(this.gameObject); // Immediately pool on pickup
        }
    }
}