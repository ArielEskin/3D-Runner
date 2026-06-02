using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private PlayerMovement playerMovement;

    void Update()
    {
        if (GameManager.gameManager.isDead) return; // Stop the camera from moving if the game is over
        transform.Translate(Vector3.forward * (playerMovement.moveSpeed * Time.deltaTime),Space.World); // If the player is alive, move forward
    }
}
