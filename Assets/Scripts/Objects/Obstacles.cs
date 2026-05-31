using UnityEngine;

public class Obstacles : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Pass the tag of this obstacle to the GameManager
            GameManager.gameManager.KillPlayer(gameObject.tag);
        }
    }
}