using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // ==========references================================================================================================================================================

    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private PlayerMovement playerMovement;
    
    // ==========================================================================================================================================================

    void Update() // Checks if the game is still active, if so moves the camera forward at the exact same movement speed as the player to keep them synced
    {
        if (GameManager.gameManager.isDead) return; 
        transform.Translate(Vector3.forward * (playerMovement.moveSpeed * Time.deltaTime),Space.World);
    }
}
