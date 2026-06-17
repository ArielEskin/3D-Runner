using UnityEngine;

public class Obstacles : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) // If player hits Obstacle ----> DIE
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.gameManager.isInvincible) return; // Invincibility power ups active

            if (gameObject.name.Contains("Low"))
            {
                GameManager.gameManager.KillPlayer("LowObstacle");
            }
            else
            {
                GameManager.gameManager.KillPlayer("HighObstacle");
            }
        }
    }
}